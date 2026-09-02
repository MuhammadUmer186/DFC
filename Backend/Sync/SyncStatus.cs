using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;

namespace RestaurantSystem.Sync
{
    /// <summary>Safe operational snapshot for /api/sync/status and the RMS widget (Phase 5/17).</summary>
    public static class SyncStatus
    {
        public static async Task<object> BuildAsync(ApplicationDbContext db, INodeContext node, CancellationToken ct = default)
        {
            var pendingOutbox = await db.SyncOutbox.CountAsync(o => !o.Dispatched, ct);
            var deadLetters = await db.SyncDeadLetters.CountAsync(d => !d.Replayed, ct);
            var conflicts = await db.SyncConflicts.CountAsync(c => !c.Resolved, ct);

            var lastPush = await db.SyncCheckpoints.Where(c => c.Direction == "push")
                .OrderByDescending(c => c.UpdatedAtUtc).Select(c => (DateTime?)c.UpdatedAtUtc).FirstOrDefaultAsync(ct);
            var lastPull = await db.SyncCheckpoints.Where(c => c.Direction == "pull")
                .OrderByDescending(c => c.UpdatedAtUtc).Select(c => (DateTime?)c.UpdatedAtUtc).FirstOrDefaultAsync(ct);

            var lastPeerHeartbeat = await db.NodeHeartbeats
                .Where(h => h.Source != null && h.Source.StartsWith("peer:"))
                .OrderByDescending(h => h.ReceivedAtUtc)
                .Select(h => (DateTime?)h.ReceivedAtUtc).FirstOrDefaultAsync(ct);

            bool dbConnected;
            try { dbConnected = await db.Database.CanConnectAsync(ct); }
            catch { dbConnected = false; }

            return new
            {
                nodeId = node.NodeId,
                nodeRole = node.Role.ToString(),
                branchId = node.BranchId,
                appVersion = node.AppVersion,
                schemaVersion = node.SchemaVersion,
                databaseConnected = dbConnected,
                lastSuccessfulPushUtc = lastPush,
                lastSuccessfulPullUtc = lastPull,
                pendingOutboxCount = pendingOutbox,
                deadLetterCount = deadLetters,
                conflictCount = conflicts,
                lastPeerHeartbeatUtc = lastPeerHeartbeat,
                serverTimeUtc = DateTime.UtcNow
            };
        }
    }
}
