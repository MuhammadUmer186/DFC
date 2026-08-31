using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.Models;
using RestaurantSystem.Sync;

namespace RestaurantSystem.Services
{
    public sealed record StockMovementRequest(
        StockMovementType MovementType,
        int RawItemId,
        decimal QuantityDelta,
        string ReferenceType,
        Guid ReferenceGlobalId,
        int? VendorId = null,
        DateTime? OccurredAtUtc = null,
        Guid? CreatedByUserGlobalId = null,
        Guid? ReversesMovementGlobalId = null);

    public sealed record StockReconciliationLine(
        int RawItemId, string RawItemName, int? VendorId,
        decimal ProjectionQuantity, decimal LedgerQuantity, decimal Difference);

    public interface IStockLedger
    {
        /// <summary>Append one movement. Idempotent per (ReferenceType, ReferenceGlobalId, MovementType, RawItemId).</summary>
        Task<bool> RecordAsync(StockMovementRequest request, CancellationToken ct = default);

        /// <summary>Append many movements atomically (skips ones already present).</summary>
        Task<int> RecordManyAsync(IEnumerable<StockMovementRequest> requests, CancellationToken ct = default);

        /// <summary>Recompute <see cref="StoreStock"/> quantities from the ledger.</summary>
        Task RebuildProjectionAsync(int? rawItemId = null, CancellationToken ct = default);

        /// <summary>Compare the <see cref="StoreStock"/> projection against the ledger sum.</summary>
        Task<IReadOnlyList<StockReconciliationLine>> ReconcileAsync(bool onlyDiscrepancies = true, CancellationToken ct = default);
    }

    public sealed class StockLedger : IStockLedger
    {
        private readonly ApplicationDbContext _db;
        private readonly INodeContext _node;

        public StockLedger(ApplicationDbContext db, INodeContext node)
        {
            _db = db;
            _node = node;
        }

        public async Task<bool> RecordAsync(StockMovementRequest r, CancellationToken ct = default)
            => await RecordManyAsync(new[] { r }, ct) > 0;

        public async Task<int> RecordManyAsync(IEnumerable<StockMovementRequest> requests, CancellationToken ct = default)
        {
            var list = requests.ToList();
            if (list.Count == 0) return 0;

            // Which of these are already recorded? (dedupe for retries / duplicate sync events)
            var keys = list.Select(x => new { x.ReferenceType, x.ReferenceGlobalId, x.MovementType, x.RawItemId }).ToHashSet();
            var refIds = list.Select(x => x.ReferenceGlobalId).Distinct().ToList();
            var existing = await _db.Set<StockMovement>()
                .Where(m => refIds.Contains(m.ReferenceGlobalId))
                .Select(m => new { m.ReferenceType, m.ReferenceGlobalId, m.MovementType, m.RawItemId })
                .ToListAsync(ct);
            var existingSet = existing.Select(e => new { e.ReferenceType, e.ReferenceGlobalId, e.MovementType, e.RawItemId }).ToHashSet();

            var rawItemIds = list.Select(x => x.RawItemId).Distinct().ToList();
            var rawGlobalIds = await _db.Set<RawItem>()
                .Where(ri => rawItemIds.Contains(ri.Id))
                .Select(ri => new { ri.Id, ri.GlobalId })
                .ToDictionaryAsync(x => x.Id, x => x.GlobalId, ct);
            var vendorIds = list.Where(x => x.VendorId.HasValue).Select(x => x.VendorId!.Value).Distinct().ToList();
            var vendorGlobalIds = vendorIds.Count == 0
                ? new Dictionary<int, Guid>()
                : await _db.Set<Vendor>().Where(v => vendorIds.Contains(v.Id))
                    .Select(v => new { v.Id, v.GlobalId }).ToDictionaryAsync(x => x.Id, x => x.GlobalId, ct);

            var added = 0;
            foreach (var r in list)
            {
                var k = new { r.ReferenceType, r.ReferenceGlobalId, r.MovementType, r.RawItemId };
                if (existingSet.Contains(k)) continue;

                _db.Set<StockMovement>().Add(new StockMovement
                {
                    MovementType = r.MovementType,
                    RawItemId = r.RawItemId,
                    RawItemGlobalId = rawGlobalIds.TryGetValue(r.RawItemId, out var g) ? g : Guid.Empty,
                    VendorId = r.VendorId,
                    VendorGlobalId = r.VendorId.HasValue && vendorGlobalIds.TryGetValue(r.VendorId.Value, out var vg) ? vg : null,
                    QuantityDelta = r.QuantityDelta,
                    ReferenceType = r.ReferenceType,
                    ReferenceGlobalId = r.ReferenceGlobalId,
                    ReversesMovementGlobalId = r.ReversesMovementGlobalId,
                    OccurredAtUtc = r.OccurredAtUtc ?? DateTime.UtcNow,
                    CreatedByUserGlobalId = r.CreatedByUserGlobalId,
                    AggregateVersion = 1
                });
                added++;
            }

            if (added > 0)
                await _db.SaveChangesAsync(ct);
            return added;
        }

        public async Task RebuildProjectionAsync(int? rawItemId = null, CancellationToken ct = default)
        {
            var q = _db.Set<StockMovement>().AsQueryable();
            if (rawItemId.HasValue) q = q.Where(m => m.RawItemId == rawItemId.Value);

            var sums = await q
                .GroupBy(m => new { m.RawItemId, m.VendorId })
                .Select(g => new { g.Key.RawItemId, g.Key.VendorId, Qty = g.Sum(x => x.QuantityDelta) })
                .ToListAsync(ct);

            foreach (var s in sums)
            {
                // Ledger rows without a vendor can't be projected onto a vendor-keyed
                // StoreStock row; those are reconciled via the report, not the projection.
                if (s.VendorId is null) continue;

                var row = await _db.Set<StoreStock>()
                    .FirstOrDefaultAsync(x => x.RawItemId == s.RawItemId && x.VendorId == s.VendorId.Value, ct);
                if (row is null)
                {
                    row = new StoreStock { RawItemId = s.RawItemId, VendorId = s.VendorId.Value };
                    _db.Set<StoreStock>().Add(row);
                }
                row.Quantity = s.Qty;
                row.LastUpdated = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<StockReconciliationLine>> ReconcileAsync(bool onlyDiscrepancies = true, CancellationToken ct = default)
        {
            var ledger = await _db.Set<StockMovement>()
                .Where(m => m.VendorId != null)
                .GroupBy(m => new { m.RawItemId, m.VendorId })
                .Select(g => new { g.Key.RawItemId, g.Key.VendorId, Qty = g.Sum(x => x.QuantityDelta) })
                .ToListAsync(ct);

            var projection = await _db.Set<StoreStock>()
                .Select(s => new { s.RawItemId, s.VendorId, s.Quantity, Name = s.RawItem.Name })
                .ToListAsync(ct);

            var names = projection.ToDictionary(p => p.RawItemId, p => p.Name);
            var projByKey = projection.ToDictionary(p => (p.RawItemId, (int?)p.VendorId), p => p.Quantity);
            var lines = new List<StockReconciliationLine>();

            var keys = ledger.Select(l => (l.RawItemId, l.VendorId))
                .Concat(projection.Select(p => (p.RawItemId, (int?)p.VendorId)))
                .Distinct();

            foreach (var (rawItemId, vendorId) in keys)
            {
                var led = ledger.FirstOrDefault(l => l.RawItemId == rawItemId && l.VendorId == vendorId)?.Qty ?? 0m;
                var proj = projByKey.TryGetValue((rawItemId, vendorId), out var p) ? p : 0m;
                var diff = proj - led;
                if (onlyDiscrepancies && Math.Abs(diff) < 0.0001m) continue;
                lines.Add(new StockReconciliationLine(
                    rawItemId, names.TryGetValue(rawItemId, out var n) ? n : $"#{rawItemId}",
                    vendorId, proj, led, diff));
            }
            return lines.OrderByDescending(l => Math.Abs(l.Difference)).ToList();
        }
    }
}
