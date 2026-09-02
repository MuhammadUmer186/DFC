using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestaurantSystem.Data;
using RestaurantSystem.Models;

namespace RestaurantSystem.Sync
{
    /// <summary>
    /// The single per-node sync worker (docs/SYNC_PROTOCOL.md §1). Enabled by
    /// default on Edge, off on Cloud. Exclusivity is enforced with an
    /// <c>sp_getapplock</c> so a second copy simply idles. Each cycle: heartbeat →
    /// push local outbox → pull peer outbox → apply → ack → advance checkpoints,
    /// with exponential backoff + jitter on transport failure.
    /// </summary>
    public sealed class SyncWorker : BackgroundService
    {
        private const string LockResource = "RMS_SyncWorker";

        private readonly IServiceScopeFactory _scopes;
        private readonly SyncOptions _opts;
        private readonly DeploymentOptions _deployment;
        private readonly INodeContext _node;
        private readonly ILogger<SyncWorker> _log;

        public SyncWorker(IServiceScopeFactory scopes, SyncOptions opts, DeploymentOptions deployment,
            INodeContext node, ILogger<SyncWorker> log)
        {
            _scopes = scopes;
            _opts = opts;
            _deployment = deployment;
            _node = node;
            _log = log;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var enabled = _opts.WorkerEnabled ?? (_deployment.ParsedRole == NodeRole.Edge);
            if (!enabled)
            {
                _log.LogInformation("SyncWorker: disabled on this node ({Role}).", _deployment.NodeRole);
                return;
            }

            var peerBaseUrl = _deployment.ParsedRole == NodeRole.Edge
                ? _deployment.CloudBaseUrl
                : _deployment.EdgeBaseUrl;

            if (string.IsNullOrWhiteSpace(peerBaseUrl) || string.IsNullOrWhiteSpace(_opts.HmacSecret))
            {
                _log.LogWarning("SyncWorker: not started — peer base URL or Sync:HmacSecret is not configured.");
                return;
            }

            // Give the app a moment to finish node self-registration.
            try { await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); } catch { return; }

            await using var lockConn = new SqlConnection(GetConnString());
            await lockConn.OpenAsync(stoppingToken);
            if (!await TryAcquireLockAsync(lockConn, stoppingToken))
            {
                _log.LogWarning("SyncWorker: another worker holds {Lock}; idling.", LockResource);
                return;
            }
            _log.LogInformation("SyncWorker: started. Peer={Peer}, interval={Interval}s.", peerBaseUrl, _opts.IntervalSeconds);

            var attempt = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                var ok = await RunCycleAsync(peerBaseUrl, stoppingToken);
                if (ok) attempt = 0;
                else attempt++;

                var delay = ok
                    ? TimeSpan.FromSeconds(_opts.IntervalSeconds)
                    : Backoff(attempt);
                try { await Task.Delay(delay, stoppingToken); } catch { break; }
            }
        }

        private TimeSpan Backoff(int attempt)
        {
            var baseMs = _opts.BackoffBaseSeconds * 1000d * Math.Pow(2, Math.Min(attempt, 10));
            var capped = Math.Min(baseMs, _opts.BackoffMaxSeconds * 1000d);
            var jittered = Random.Shared.NextDouble() * capped;
            return TimeSpan.FromMilliseconds(jittered);
        }

        private async Task<bool> RunCycleAsync(string peerBaseUrl, CancellationToken ct)
        {
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var client = scope.ServiceProvider.GetRequiredService<SyncPeerClient>();
            var apply = scope.ServiceProvider.GetRequiredService<SyncApplyService>();

            try
            {
                // 1) heartbeat
                var pending = await db.SyncOutbox.CountAsync(o => !o.Dispatched, ct);
                await client.HeartbeatAsync(peerBaseUrl, new SyncHeartbeatRequest
                {
                    NodeId = _node.NodeId,
                    NodeRole = _node.Role.ToString(),
                    AppVersion = _node.AppVersion,
                    SchemaVersion = _node.SchemaVersion,
                    SentAtUtc = DateTime.UtcNow,
                    PendingOutbox = pending
                }, ct);

                // 2) push local outbox
                var batch = await db.SyncOutbox.Where(o => !o.Dispatched)
                    .OrderBy(o => o.Id).Take(_opts.BatchSize).ToListAsync(ct);
                if (batch.Count > 0)
                {
                    var req = new SyncPushRequest
                    {
                        BatchId = Guid.NewGuid(),
                        Events = batch.Select(Controllers.SyncController.ToEnvelope).ToArray()
                    };
                    var resp = await client.PushAsync(peerBaseUrl, req, ct);
                    var byId = resp?.Results?.ToDictionary(r => r.EventId) ?? new();
                    var peerNode = await GetPeerNodeIdAsync(db, ct);
                    long maxAcked = 0;
                    foreach (var row in batch)
                    {
                        if (!byId.TryGetValue(row.EventId, out var r)) continue;
                        row.Dispatched = true;
                        row.DispatchedAtUtc = DateTime.UtcNow;
                        row.Attempts += 1;
                        if (r.Status is "conflict" or "deadletter")
                            row.LastError = $"{r.Status}:{r.Detail}";
                        maxAcked = Math.Max(maxAcked, row.Id);
                    }
                    await AdvanceCheckpointAsync(db, peerNode, "push", maxAcked, ct);
                    using (SyncStampingInterceptor.Suppress())
                        await db.SaveChangesAsync(ct);
                    _log.LogInformation("SyncWorker: pushed {N} event(s).", batch.Count);
                }

                // 3) pull peer outbox
                var peerNodeId = await GetPeerNodeIdAsync(db, ct);
                var cp = await db.SyncCheckpoints.FirstOrDefaultAsync(
                    c => c.PeerNodeId == peerNodeId && c.Direction == "pull" && c.AggregateType == "*", ct);
                var since = cp?.LastValue ?? 0;

                var pull = await client.PullAsync(peerBaseUrl, since, _opts.BatchSize, null, ct);
                if (pull is { Events.Length: > 0 })
                {
                    var acks = await apply.ApplyBatchAsync(pull.Events, ct);
                    await AdvanceCheckpointAsync(db, peerNodeId, "pull", pull.NextCursor, ct);
                    using (SyncStampingInterceptor.Suppress())
                        await db.SaveChangesAsync(ct);

                    await client.AckAsync(peerBaseUrl, new SyncPushResponse
                    {
                        BatchId = Guid.NewGuid(),
                        Results = acks.ToArray()
                    }, ct);
                    _log.LogInformation("SyncWorker: pulled {N} event(s) (applied={A}).",
                        pull.Events.Length, acks.Count(a => a.Status == "applied"));
                }

                return true;
            }
            catch (SyncTransportException ex)
            {
                _log.LogWarning("SyncWorker: peer transport error {Status} — {Msg}", ex.StatusCode, ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "SyncWorker: cycle failed.");
                return false;
            }
        }

        private async Task<Guid> GetPeerNodeIdAsync(ApplicationDbContext db, CancellationToken ct)
        {
            // The most-recently-seen active node of this branch that is NOT us.
            var meId = _node.NodeId;
            var meBranch = _node.BranchId;
            var peer = await db.SystemNodes.AsNoTracking()
                .Where(n => n.IsActive && n.NodeId != meId && (meBranch == Guid.Empty || n.BranchId == meBranch))
                .OrderByDescending(n => n.LastSeenAtUtc)
                .Select(n => n.NodeId)
                .FirstOrDefaultAsync(ct);
            return peer;
        }

        private static async Task AdvanceCheckpointAsync(ApplicationDbContext db, Guid peer, string direction, long value, CancellationToken ct)
        {
            if (value <= 0) return;
            var cp = await db.SyncCheckpoints.FirstOrDefaultAsync(
                c => c.PeerNodeId == peer && c.Direction == direction && c.AggregateType == "*", ct);
            if (cp is null)
                db.SyncCheckpoints.Add(new SyncCheckpoint { PeerNodeId = peer, Direction = direction, AggregateType = "*", LastValue = value, UpdatedAtUtc = DateTime.UtcNow });
            else if (value > cp.LastValue) { cp.LastValue = value; cp.UpdatedAtUtc = DateTime.UtcNow; }
        }

        private async Task<bool> TryAcquireLockAsync(SqlConnection conn, CancellationToken ct)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "sp_getapplock";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Resource", LockResource);
            cmd.Parameters.AddWithValue("@LockMode", "Exclusive");
            cmd.Parameters.AddWithValue("@LockOwner", "Session");
            cmd.Parameters.AddWithValue("@LockTimeout", 0);
            var ret = new SqlParameter { Direction = ParameterDirection.ReturnValue };
            cmd.Parameters.Add(ret);
            await cmd.ExecuteNonQueryAsync(ct);
            return (int)(ret.Value ?? -1) >= 0;
        }

        private string GetConnString()
        {
            using var scope = _scopes.CreateScope();
            return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.GetConnectionString()!;
        }
    }
}
