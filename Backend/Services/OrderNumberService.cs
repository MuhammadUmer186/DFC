using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RestaurantSystem.Data;
using RestaurantSystem.Sync;

namespace RestaurantSystem.Services
{
    public interface IOrderNumberService
    {
        /// <summary>
        /// Atomically allocates the next order number for this node's
        /// (branch, <paramref name="sourceCode"/>, current business day) sequence
        /// and returns the display string, e.g. <c>DFC-POS-000123</c>.
        /// </summary>
        Task<(string display, string sourceCode)> AllocateAsync(string sourceCode, CancellationToken ct = default);
    }

    /// <summary>
    /// Phase 3. Replaces the single-writer counter in <c>SiteSetting (Id=1)</c>
    /// with per-branch/source/business-day sequences so local and cloud never
    /// mint the same number. Preserves the configured prefix, starting number and
    /// business-day reset time.
    /// </summary>
    public sealed class OrderNumberService : IOrderNumberService
    {
        private readonly ApplicationDbContext _db;
        private readonly IRestaurantClock _clock;
        private readonly INodeContext _node;

        public OrderNumberService(ApplicationDbContext db, IRestaurantClock clock, INodeContext node)
        {
            _db = db;
            _clock = clock;
            _node = node;
        }

        public async Task<(string display, string sourceCode)> AllocateAsync(string sourceCode, CancellationToken ct = default)
        {
            sourceCode = Normalize(sourceCode);
            // An online order taken on the Cloud node is a CLD number; on the Edge it is WEB.
            if (sourceCode == "WEB" && _node.Role == Models.NodeRole.Cloud)
                sourceCode = "CLD";

            var settings = await _db.SiteSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Id == 1, ct);
            var prefix = settings?.OrderSerialPrefix ?? string.Empty;
            var startingNumber = settings?.OrderSerialStartingNumber ?? 1;
            var resetTime = settings?.OrderSerialResetTime ?? TimeSpan.Zero;

            // Business day per the restaurant's local clock (unchanged semantics).
            var now = await _clock.GetLocalNowAsync();
            var businessDate = (now.TimeOfDay < resetTime ? now.Date.AddDays(-1) : now.Date);

            var branchId = _node.BranchId;

            // Atomic upsert-and-increment. HOLDLOCK on MERGE serialises concurrent
            // allocations for the same (branch, source, day) key. Raw ADO because
            // EF's SqlQuery wraps SQL in a subquery, which MERGE/OUTPUT disallow.
            const string sql =
                @"MERGE dbo.OrderNumberSequences WITH (HOLDLOCK) AS t
                  USING (SELECT @BranchId AS BranchId, @SourceCode AS SourceCode, CAST(@BusinessDate AS date) AS BusinessDate) AS s
                    ON  t.BranchId = s.BranchId AND t.SourceCode = s.SourceCode AND t.BusinessDate = s.BusinessDate
                  WHEN MATCHED THEN
                    UPDATE SET LastValue = t.LastValue + 1, UpdatedAtUtc = SYSUTCDATETIME()
                  WHEN NOT MATCHED THEN
                    INSERT (BranchId, SourceCode, BusinessDate, LastValue, UpdatedAtUtc)
                    VALUES (s.BranchId, s.SourceCode, s.BusinessDate, @Start, SYSUTCDATETIME())
                  OUTPUT INSERTED.LastValue;";

            var conn = (SqlConnection)_db.Database.GetDbConnection();
            var opened = false;
            if (conn.State != ConnectionState.Open) { await conn.OpenAsync(ct); opened = true; }
            try
            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                // Enlist in the caller's EF transaction (CreateAsync opens one) if present.
                cmd.Transaction = _db.Database.CurrentTransaction?.GetDbTransaction() as SqlTransaction;
                cmd.Parameters.Add(new SqlParameter("@BranchId", SqlDbType.UniqueIdentifier) { Value = branchId });
                cmd.Parameters.Add(new SqlParameter("@SourceCode", SqlDbType.VarChar, 8) { Value = sourceCode });
                cmd.Parameters.Add(new SqlParameter("@BusinessDate", SqlDbType.Date) { Value = businessDate.Date });
                cmd.Parameters.Add(new SqlParameter("@Start", SqlDbType.Int) { Value = startingNumber });
                var scalar = await cmd.ExecuteScalarAsync(ct);
                var value = Convert.ToInt32(scalar);

                var display = string.IsNullOrEmpty(prefix)
                    ? $"{sourceCode}-{value:D6}"
                    : $"{prefix.TrimEnd('-', ' ')}-{sourceCode}-{value:D6}";
                return (display, sourceCode);
            }
            finally
            {
                if (opened) await conn.CloseAsync();
            }
        }

        private static string Normalize(string source)
        {
            var s = (source ?? "").Trim().ToUpperInvariant();
            return s switch
            {
                "POS" => "POS",
                "WEB" or "ONLINE" => "WEB",
                "CLD" or "CLOUD" => "CLD",
                _ => "POS"
            };
        }
    }
}
