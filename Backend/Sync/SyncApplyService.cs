using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantSystem.Data;
using RestaurantSystem.Models;

namespace RestaurantSystem.Sync
{
    /// <summary>
    /// Receiver-side apply of inbound sync events (docs/SYNC_PROTOCOL.md §5-6):
    /// idempotent inbox, aggregate-version ordering, conflict + dead-letter
    /// recording, per-event acknowledgement. Each event is applied in its own
    /// transaction together with its inbox row.
    /// </summary>
    public sealed class SyncApplyService
    {
        private readonly ApplicationDbContext _db;
        private readonly AggregateSnapshotService _snap;
        private readonly ILogger<SyncApplyService> _log;

        public SyncApplyService(ApplicationDbContext db, AggregateSnapshotService snap, ILogger<SyncApplyService> log)
        {
            _db = db;
            _snap = snap;
            _log = log;
        }

        public async Task<List<SyncAckItem>> ApplyBatchAsync(IEnumerable<SyncEnvelope> events, CancellationToken ct = default)
        {
            var acks = new List<SyncAckItem>();
            foreach (var e in events)
                acks.Add(await ApplyOneAsync(e, ct));
            return acks;
        }

        private async Task<SyncAckItem> ApplyOneAsync(SyncEnvelope e, CancellationToken ct)
        {
            var ack = new SyncAckItem { EventId = e.EventId };

            // schema gate
            if (!SyncSchema.IsSupported(e.SchemaVersion))
            {
                await DeadLetterAsync(e, "schema", $"Unsupported schema version {e.SchemaVersion}", ct);
                ack.Status = "deadletter";
                return ack;
            }

            // idempotent inbox
            if (await _db.SyncInbox.AsNoTracking().AnyAsync(x => x.EventId == e.EventId, ct))
            {
                ack.Status = "duplicate";
                return ack;
            }

            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var local = await _snap.GetLocalStateAsync(e.AggregateType, e.AggregateGlobalId, ct);
                var isDelete = e.EventType.EndsWith("Deleted", StringComparison.Ordinal);
                string status;

                if (local is null)
                {
                    await _snap.ApplyAsync(e.AggregateType, e.AggregateGlobalId, e.AggregateVersion,
                        e.BranchId, e.OriginNodeId, e.PayloadJson, isDelete, ct);
                    status = "applied";
                }
                else
                {
                    var (localVersion, _) = local.Value;
                    if (localVersion > e.AggregateVersion)
                    {
                        status = "stale";
                    }
                    else if (localVersion == e.AggregateVersion)
                    {
                        status = "duplicate";
                    }
                    else
                    {
                        var domainIssue = await DomainRuleViolationAsync(e, ct);
                        if (domainIssue is not null)
                        {
                            var cid = await RecordConflictAsync(e, "domain-rule", localVersion, domainIssue, ct);
                            ack.Status = "conflict";
                            ack.ConflictId = cid;
                            ack.Detail = domainIssue;
                            await WriteInboxAsync(e, "conflict", cid, ct);
                            await tx.CommitAsync(ct);
                            return ack;
                        }

                        if (e.AggregateVersion > localVersion + 1)
                        {
                            // apply the latest snapshot but flag the gap for audit
                            await RecordConflictAsync(e, "version-gap", localVersion,
                                $"gap {localVersion} -> {e.AggregateVersion}", ct);
                        }

                        await _snap.ApplyAsync(e.AggregateType, e.AggregateGlobalId, e.AggregateVersion,
                            e.BranchId, e.OriginNodeId, e.PayloadJson, isDelete, ct);
                        status = "applied";
                    }
                }

                await WriteInboxAsync(e, status, null, ct);
                await tx.CommitAsync(ct);
                ack.Status = status;
                return ack;
            }
            catch (SnapshotResolutionException sre)
            {
                await tx.RollbackAsync(ct);
                var cid = await RecordConflictAsync(e, "version-gap", 0,
                    $"missing dependency: {sre.PrincipalType} {sre.MissingGlobalId}", ct);
                ack.Status = "conflict";
                ack.ConflictId = cid;
                ack.Detail = sre.Message;
                return ack;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                _log.LogError(ex, "Sync apply failed for {EventId} ({Type})", e.EventId, e.EventType);
                await DeadLetterAsync(e, "apply-error", ex.Message, ct);
                ack.Status = "deadletter";
                return ack;
            }
        }

        // Phase 7 (subset): forbidden backward order transitions.
        private async Task<string?> DomainRuleViolationAsync(SyncEnvelope e, CancellationToken ct)
        {
            if (e.AggregateType != nameof(Order)) return null;
            try
            {
                var root = JsonDocument.Parse(e.PayloadJson).RootElement;
                if (!root.TryGetProperty("scalars", out var s)) return null;

                var incomingDelivery = s.TryGetProperty(nameof(Order.DeliveryStatus), out var d) && d.ValueKind == JsonValueKind.String
                    ? d.GetString() : null;
                if (incomingDelivery is null) return null;

                var localOrder = await _db.Orders.AsNoTracking()
                    .Where(o => o.GlobalId == e.AggregateGlobalId)
                    .Select(o => new { o.DeliveryStatus, o.Status })
                    .FirstOrDefaultAsync(ct);
                if (localOrder?.DeliveryStatus is null) return null;

                int Rank(string v) => v switch
                {
                    "Approved" => 0, "Preparing" => 1, "Enroute" => 2, "Delivered" => 3, "Rejected" => 3, _ => -1
                };
                if (Rank(incomingDelivery) >= 0 && Rank(incomingDelivery) < Rank(localOrder.DeliveryStatus.ToString()!))
                    return $"backward DeliveryStatus {localOrder.DeliveryStatus} -> {incomingDelivery} rejected";
                return null;
            }
            catch { return null; }
        }

        private async Task WriteInboxAsync(SyncEnvelope e, string status, long? conflictId, CancellationToken ct)
        {
            _db.SyncInbox.Add(new SyncInbox
            {
                EventId = e.EventId,
                EventType = e.EventType,
                AggregateType = e.AggregateType,
                AggregateGlobalId = e.AggregateGlobalId,
                AggregateVersion = e.AggregateVersion,
                OriginNodeId = e.OriginNodeId,
                ReceivedAtUtc = DateTime.UtcNow,
                AppliedAtUtc = DateTime.UtcNow,
                Status = status,
                ConflictId = conflictId
            });
            using (SyncStampingInterceptor.Suppress())
                await _db.SaveChangesAsync(ct);
        }

        private async Task<long> RecordConflictAsync(SyncEnvelope e, string kind, long localVersion, string detail, CancellationToken ct)
        {
            var c = new SyncConflict
            {
                EventId = e.EventId,
                Kind = kind,
                AggregateType = e.AggregateType,
                AggregateGlobalId = e.AggregateGlobalId,
                IncomingVersion = e.AggregateVersion,
                LocalVersion = localVersion,
                OriginNodeId = e.OriginNodeId,
                IncomingPayloadJson = e.PayloadJson,
                Detail = detail,
                CreatedAtUtc = DateTime.UtcNow,
                Resolved = false
            };
            _db.SyncConflicts.Add(c);
            using (SyncStampingInterceptor.Suppress())
                await _db.SaveChangesAsync(ct);
            _log.LogWarning("Sync conflict [{Kind}] {Type} {Gid}: {Detail}", kind, e.AggregateType, e.AggregateGlobalId, detail);
            return c.Id;
        }

        private async Task DeadLetterAsync(SyncEnvelope e, string kind, string error, CancellationToken ct)
        {
            if (await _db.SyncDeadLetters.AnyAsync(x => x.EventId == e.EventId, ct)) return;
            _db.SyncDeadLetters.Add(new SyncDeadLetter
            {
                EventId = e.EventId,
                Kind = kind,
                AggregateType = e.AggregateType,
                AggregateGlobalId = e.AggregateGlobalId,
                SchemaVersion = e.SchemaVersion,
                EnvelopeJson = JsonSerializer.Serialize(e),
                Error = error,
                Attempts = 1,
                CreatedAtUtc = DateTime.UtcNow,
                LastAttemptAtUtc = DateTime.UtcNow
            });
            using (SyncStampingInterceptor.Suppress())
                await _db.SaveChangesAsync(ct);
            _log.LogError("Sync dead-letter [{Kind}] {Type} {Gid}: {Error}", kind, e.AggregateType, e.AggregateGlobalId, error);
        }
    }
}
