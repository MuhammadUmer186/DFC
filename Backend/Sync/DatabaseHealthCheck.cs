using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RestaurantSystem.Data;

namespace RestaurantSystem.Sync
{
    /// <summary>Readiness check: the database is reachable and the schema is current. Phase 17.</summary>
    public sealed class DatabaseHealthCheck : IHealthCheck
    {
        private readonly ApplicationDbContext _db;
        public DatabaseHealthCheck(ApplicationDbContext db) => _db = db;

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
        {
            try
            {
                if (!await _db.Database.CanConnectAsync(ct))
                    return HealthCheckResult.Unhealthy("database unreachable");

                var pending = await _db.Database.GetPendingMigrationsAsync(ct);
                foreach (var _ in pending)
                    return HealthCheckResult.Degraded("pending migrations — run the migrator");

                return HealthCheckResult.Healthy();
            }
            catch (System.Exception ex)
            {
                return HealthCheckResult.Unhealthy("database check failed", ex);
            }
        }
    }
}
