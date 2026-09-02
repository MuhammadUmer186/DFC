using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantSystem.Data;

namespace RestaurantSystem.Sync
{
    /// <summary>
    /// One-time, idempotent backfill of <c>OriginNodeId</c> / <c>BranchId</c> on
    /// pre-existing rows of every synchronized aggregate root. The Phase 2
    /// migration adds those columns with an all-zero default; this stamps the
    /// running node/branch onto rows that were created before sync existed.
    /// Runs on every startup; a no-op once complete.
    /// Offline-first / cloud-sync — Phase 2.
    /// </summary>
    public sealed class SyncBackfillService
    {
        private readonly ApplicationDbContext _db;
        private readonly INodeContext _node;
        private readonly ILogger<SyncBackfillService> _log;

        public SyncBackfillService(ApplicationDbContext db, INodeContext node, ILogger<SyncBackfillService> log)
        {
            _db = db;
            _node = node;
            _log = log;
        }

        public async Task RunAsync(CancellationToken ct = default)
        {
            if (!_node.IsReady || _node.NodeId == Guid.Empty || _node.BranchId == Guid.Empty)
            {
                _log.LogWarning("Sync/Phase2: node identity not ready — skipping origin/branch backfill this start.");
                return;
            }

            var rootTables = _db.Model.GetEntityTypes()
                .Where(t => typeof(ISyncableAggregate).IsAssignableFrom(t.ClrType))
                .Select(t => t.GetSchemaQualifiedTableName())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .ToList();

            var totalRows = 0;
            foreach (var table in rootTables)
            {
                // Parameterised; table names come from EF model metadata, not user input.
                var sql =
                    $"UPDATE {table} SET [OriginNodeId] = {{0}}, [BranchId] = {{1}} " +
                    $"WHERE [OriginNodeId] = '00000000-0000-0000-0000-000000000000' " +
                    $"   OR [BranchId] = '00000000-0000-0000-0000-000000000000'";
                try
                {
                    var affected = await _db.Database.ExecuteSqlRawAsync(sql, new object[] { _node.NodeId, _node.BranchId }, ct);
                    if (affected > 0)
                    {
                        totalRows += affected;
                        _log.LogInformation("Sync/Phase2: backfilled {Rows} row(s) in {Table}.", affected, table);
                    }
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Sync/Phase2: backfill failed for {Table} (continuing).", table);
                }
            }

            if (totalRows > 0)
                _log.LogWarning("Sync/Phase2: origin/branch backfill stamped {Total} pre-existing row(s) with node {NodeId} / branch {BranchId}.",
                    totalRows, _node.NodeId, _node.BranchId);
        }
    }
}
