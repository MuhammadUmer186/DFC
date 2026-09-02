using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.Sync;

namespace RestaurantSystem.Controllers
{
    /// <summary>
    /// Operator-facing sync administration (Phase 5/17). JWT + role gated — NOT
    /// part of the HMAC node channel. Backs the RMS sync page.
    /// </summary>
    [ApiController]
    [Authorize(Roles = "SuperAdmin,MainAdmin")]
    [Route("api/sync-admin")]
    public sealed class SyncAdminController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly SyncApplyService _apply;
        private readonly INodeContext _node;

        public SyncAdminController(ApplicationDbContext db, SyncApplyService apply, INodeContext node)
        {
            _db = db;
            _apply = apply;
            _node = node;
        }

        [HttpGet("status")]
        public async Task<IActionResult> Status(CancellationToken ct)
            => Ok(await SyncStatus.BuildAsync(_db, _node, ct));

        [HttpGet("conflicts")]
        public async Task<IActionResult> Conflicts([FromQuery] bool includeResolved = false,
            [FromQuery] int page = 0, [FromQuery] int pageSize = 50, CancellationToken ct = default)
        {
            var q = _db.SyncConflicts.AsNoTracking();
            if (!includeResolved) q = q.Where(c => !c.Resolved);
            var total = await q.CountAsync(ct);
            var items = await q.OrderByDescending(c => c.CreatedAtUtc)
                .Skip(page * pageSize).Take(Math.Clamp(pageSize, 1, 200)).ToListAsync(ct);
            return Ok(new { total, page, pageSize, items });
        }

        [HttpGet("dead-letters")]
        public async Task<IActionResult> DeadLetters([FromQuery] bool includeReplayed = false, CancellationToken ct = default)
        {
            var q = _db.SyncDeadLetters.AsNoTracking();
            if (!includeReplayed) q = q.Where(d => !d.Replayed);
            return Ok(await q.OrderByDescending(d => d.CreatedAtUtc).Take(200).ToListAsync(ct));
        }

        public sealed class ResolveRequest
        {
            public string Resolution { get; set; } = "keepLocal"; // keepLocal | keepRemote | manual
            public string? PatchJson { get; set; }
        }

        [HttpPost("conflicts/{id:long}/resolve")]
        public async Task<IActionResult> Resolve(long id, [FromBody] ResolveRequest req, CancellationToken ct)
        {
            var c = await _db.SyncConflicts.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (c is null) return NotFound();
            if (c.Resolved) return Ok(new { alreadyResolved = true });

            if (req.Resolution == "keepRemote" && !string.IsNullOrWhiteSpace(c.IncomingPayloadJson))
            {
                await new AggregateSnapshotService(_db).ApplyAsync(
                    c.AggregateType, c.AggregateGlobalId, c.IncomingVersion,
                    Guid.Empty, c.OriginNodeId, c.IncomingPayloadJson, isDelete: false, ct);
            }
            else if (req.Resolution == "manual" && !string.IsNullOrWhiteSpace(req.PatchJson))
            {
                await new AggregateSnapshotService(_db).ApplyAsync(
                    c.AggregateType, c.AggregateGlobalId, c.IncomingVersion + 1,
                    Guid.Empty, _node.NodeId, req.PatchJson!, isDelete: false, ct);
            }
            // keepLocal: nothing to apply; the local state stands and will propagate on its next change.

            c.Resolved = true;
            c.Resolution = req.Resolution;
            c.ResolvedByUserName = User?.Identity?.Name;
            c.ResolvedAtUtc = DateTime.UtcNow;
            using (SyncStampingInterceptor.Suppress())
                await _db.SaveChangesAsync(ct);

            return Ok(new { resolved = true, resolution = req.Resolution });
        }

        [HttpPost("dead-letters/{id:long}/replay")]
        public async Task<IActionResult> Replay(long id, CancellationToken ct)
        {
            var d = await _db.SyncDeadLetters.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (d is null) return NotFound();
            var env = System.Text.Json.JsonSerializer.Deserialize<SyncEnvelope>(d.EnvelopeJson);
            if (env is null) return BadRequest(new { error = "bad-envelope" });

            var acks = await _apply.ApplyBatchAsync(new[] { env }, ct);
            d.Replayed = acks.All(a => a.Status is "applied" or "duplicate");
            d.Attempts += 1;
            d.LastAttemptAtUtc = DateTime.UtcNow;
            using (SyncStampingInterceptor.Suppress())
                await _db.SaveChangesAsync(ct);
            return Ok(new { replayed = d.Replayed, results = acks });
        }
    }
}
