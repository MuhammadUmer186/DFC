using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantSystem.Data;
using RestaurantSystem.Models;
using RestaurantSystem.Services;

namespace RestaurantSystem.Sync
{
    /// <summary>
    /// Phase 4 backfill. Seeds one <see cref="StockMovementType.OpeningBalance"/>
    /// ledger row per existing <see cref="StoreStock"/> row, capturing the
    /// current on-hand quantity as the ledger's starting point. Idempotent — the
    /// unique <c>(ReferenceType, ReferenceGlobalId, MovementType, RawItemId)</c>
    /// index makes re-runs a no-op.
    /// </summary>
    public sealed class StockLedgerBackfillService
    {
        private static readonly Guid OpeningNamespace = new("6b1f0c9e-6a1e-4c2a-9d3b-8e5a2f7c14d0");

        private readonly ApplicationDbContext _db;
        private readonly IStockLedger _ledger;
        private readonly ILogger<StockLedgerBackfillService> _log;

        public StockLedgerBackfillService(ApplicationDbContext db, IStockLedger ledger, ILogger<StockLedgerBackfillService> log)
        {
            _db = db;
            _ledger = ledger;
            _log = log;
        }

        public async Task RunAsync(CancellationToken ct = default)
        {
            var stocks = await _db.StoreStocks.AsNoTracking()
                .Select(s => new { s.Id, s.RawItemId, s.VendorId, s.Quantity, s.LastUpdated })
                .ToListAsync(ct);
            if (stocks.Count == 0) return;

            var reqs = stocks.Select(s => new StockMovementRequest(
                MovementType: StockMovementType.OpeningBalance,
                RawItemId: s.RawItemId,
                QuantityDelta: s.Quantity,
                ReferenceType: "OpeningBalance",
                ReferenceGlobalId: DeterministicRef(s.RawItemId, s.VendorId),
                VendorId: s.VendorId,
                OccurredAtUtc: s.LastUpdated == default ? DateTime.UtcNow : s.LastUpdated));

            var added = await _ledger.RecordManyAsync(reqs, ct);
            if (added > 0)
                _log.LogWarning("Sync/Phase4: seeded {Count} OpeningBalance ledger movement(s) from StoreStock.", added);
        }

        private static Guid DeterministicRef(int rawItemId, int vendorId)
        {
            Span<byte> buf = stackalloc byte[16 + 8];
            OpeningNamespace.TryWriteBytes(buf);
            BitConverter.TryWriteBytes(buf.Slice(16, 4), rawItemId);
            BitConverter.TryWriteBytes(buf.Slice(20, 4), vendorId);
            Span<byte> hash = stackalloc byte[32];
            System.Security.Cryptography.SHA256.HashData(buf, hash);
            return new Guid(hash.Slice(0, 16));
        }
    }
}
