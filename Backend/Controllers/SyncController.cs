using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.Models;
using RestaurantSystem.Sync;

namespace RestaurantSystem.Controllers
{
    /// <summary>
    /// Node-to-node sync transport (docs/SYNC_PROTOCOL.md §3). Guarded by
    /// <see cref="SyncHmacMiddleware"/> — every call must carry a valid per-node
    /// HMAC signature. Not user-authenticated, never anonymous.
    /// </summary>
    [ApiController]
    [AllowAnonymous]
    [Route("api/sync")]
    public sealed class SyncController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly SyncApplyService _apply;
        private readonly INodeContext _node;

        public SyncController(ApplicationDbContext db, SyncApplyService apply, INodeContext node)
        {
            _db = db;
            _apply = apply;
            _node = node;
        }

        /// <summary>Receive a batch of change events from a peer and apply them idempotently.</summary>
        [HttpPost("push")]
        public async Task<ActionResult<SyncPushResponse>> Push([FromBody] SyncPushRequest req, CancellationToken ct)
        {
            var results = await _apply.ApplyBatchAsync(req.Events ?? Array.Empty<SyncEnvelope>(), ct);
            return Ok(new SyncPushResponse { BatchId = req.BatchId, Results = results.ToArray() });
        }

        /// <summary>Return this node's outbound events after <paramref name="since"/> (this node's SyncOutbox.Id).</summary>
        [HttpGet("pull")]
        public async Task<ActionResult<SyncPullResponse>> Pull(
            [FromQuery] long since = 0, [FromQuery] int max = 200, [FromQuery] string? aggregateTypes = null, CancellationToken ct = default)
        {
            max = Math.Clamp(max, 1, 1000);
            var q = _db.SyncOutbox.AsNoTracking().Where(o => o.Id > since);
            if (!string.IsNullOrWhiteSpace(aggregateTypes))
            {
                var set = aggregateTypes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                q = q.Where(o => set.Contains(o.AggregateType));
            }

            var rows = await q.OrderBy(o => o.Id).Take(max + 1).ToListAsync(ct);
            var hasMore = rows.Count > max;
            if (hasMore) rows.RemoveAt(rows.Count - 1);

            return Ok(new SyncPullResponse
            {
                Cursor = since,
                NextCursor = rows.Count > 0 ? rows[^1].Id : since,
                HasMore = hasMore,
                Events = rows.Select(ToEnvelope).ToArray()
            });
        }

        /// <summary>Peer acknowledges events it consumed from us; mark them dispatched.</summary>
        [HttpPost("ack")]
        public async Task<IActionResult> Ack([FromBody] SyncPushResponse ack, CancellationToken ct)
        {
            var doneIds = ack.Results?
                .Where(r => r.Status is "applied" or "duplicate" or "stale")
                .Select(r => r.EventId).ToHashSet() ?? new();
            if (doneIds.Count == 0) return Ok(new { acked = 0 });

            var rows = await _db.SyncOutbox.Where(o => doneIds.Contains(o.EventId) && !o.Dispatched).ToListAsync(ct);
            foreach (var r in rows) { r.Dispatched = true; r.DispatchedAtUtc = DateTime.UtcNow; }

            var caller = HttpContext.Items.TryGetValue("SyncCallerNodeId", out var c) && c is Guid g ? g : Guid.Empty;
            if (caller != Guid.Empty && rows.Count > 0)
            {
                var maxId = rows.Max(r => r.Id);
                var cp = await _db.SyncCheckpoints.FirstOrDefaultAsync(
                    x => x.PeerNodeId == caller && x.Direction == "push" && x.AggregateType == "*", ct);
                if (cp is null)
                    _db.SyncCheckpoints.Add(new SyncCheckpoint { PeerNodeId = caller, Direction = "push", AggregateType = "*", LastValue = maxId, UpdatedAtUtc = DateTime.UtcNow });
                else if (maxId > cp.LastValue) { cp.LastValue = maxId; cp.UpdatedAtUtc = DateTime.UtcNow; }
            }

            using (SyncStampingInterceptor.Suppress())
                await _db.SaveChangesAsync(ct);
            return Ok(new { acked = rows.Count });
        }

        [HttpPost("heartbeat")]
        public async Task<IActionResult> Heartbeat([FromBody] SyncHeartbeatRequest hb, CancellationToken ct)
        {
            var node = await _db.SystemNodes.FirstOrDefaultAsync(n => n.NodeId == hb.NodeId, ct);
            if (node is not null)
            {
                node.LastSeenAtUtc = DateTime.UtcNow;
                node.AppVersion = hb.AppVersion ?? node.AppVersion;
                node.SchemaVersion = hb.SchemaVersion ?? node.SchemaVersion;
            }
            _db.NodeHeartbeats.Add(new NodeHeartbeat
            {
                NodeId = hb.NodeId,
                Role = Enum.TryParse<NodeRole>(hb.NodeRole, true, out var r) ? r : NodeRole.Edge,
                BranchId = node?.BranchId ?? Guid.Empty,
                SentAtUtc = hb.SentAtUtc == default ? DateTime.UtcNow : hb.SentAtUtc,
                ReceivedAtUtc = DateTime.UtcNow,
                AppVersion = hb.AppVersion,
                SchemaVersion = hb.SchemaVersion,
                PendingOutboxCount = hb.PendingOutbox,
                Source = $"peer:{hb.NodeId}"
            });
            using (SyncStampingInterceptor.Suppress())
                await _db.SaveChangesAsync(ct);

            return Ok(new { receivedAtUtc = DateTime.UtcNow, nodeRole = _node.Role.ToString(), schemaVersion = _node.SchemaVersion });
        }

        [HttpGet("status")]
        public async Task<IActionResult> Status(CancellationToken ct) => Ok(await SyncStatus.BuildAsync(_db, _node, ct));

        /// <summary>Phase 12: fetch an uploaded file's bytes by SHA-256 (peer-to-peer, HMAC-gated).</summary>
        [HttpGet("blob/{hash}")]
        public async Task<IActionResult> GetBlob(string hash, [FromServices] UploadStore store, CancellationToken ct)
        {
            var f = await store.ReadByHashAsync(hash, ct);
            if (f is null) return NotFound();
            return File(f.Value.bytes, f.Value.contentType, f.Value.fileName);
        }

        /// <summary>Phase 12: receive an uploaded file's bytes for a metadata row already synced.</summary>
        [HttpPost("blob/{hash}")]
        public async Task<IActionResult> PutBlob(string hash, [FromServices] UploadStore store, CancellationToken ct)
        {
            using var ms = new System.IO.MemoryStream();
            await Request.Body.CopyToAsync(ms, ct);
            var ok = await store.WriteFetchedAsync(hash, ms.ToArray(), ct);
            return ok ? Ok(new { stored = true }) : BadRequest(new { error = "hash-mismatch-or-unknown" });
        }

        internal static SyncEnvelope ToEnvelope(SyncOutbox o) => new()
        {
            EventId = o.EventId,
            EventType = o.EventType,
            SchemaVersion = o.SchemaVersion,
            AggregateType = o.AggregateType,
            AggregateGlobalId = o.AggregateGlobalId,
            AggregateVersion = o.AggregateVersion,
            BranchId = o.BranchId,
            OriginNodeId = o.OriginNodeId,
            OccurredAtUtc = o.OccurredAtUtc,
            CorrelationId = o.CorrelationId,
            CausationId = o.CausationId,
            PayloadJson = o.PayloadJson
        };
    }
}
