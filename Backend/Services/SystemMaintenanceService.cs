using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.Models;

namespace RestaurantSystem.Services
{
    public class SystemMaintenanceService : ISystemMaintenanceService
    {
        private readonly ApplicationDbContext _context;

        public SystemMaintenanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Configured from the Settings page — left untouched by Clear Data so restaurant
        // branding, currency, order numbering and service-type timing survive a reset.
        private static readonly HashSet<string> ExcludedTables = new(StringComparer.OrdinalIgnoreCase)
        {
            "SiteSettings",
            "ServiceTimeSettings"
        };

        public async Task ClearAllDataAsync()
        {
            // Every table EF Core maps, discovered from the model rather than hardcoded, so
            // this doesn't silently miss a table added later. __EFMigrationsHistory isn't part
            // of the model, so it's untouched and migration state stays intact.
            var tableNames = _context.Model.GetEntityTypes()
                .Select(t => t.GetTableName())
                .Where(t => t != null && !ExcludedTables.Contains(t))
                .Distinct()
                .ToList();

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Disable FK constraint checking on every table in the DB so the DELETEs below
                // don't have to be ordered around Restrict/NoAction relationships.
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_msforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'");

                foreach (var table in tableNames)
                {
                    await _context.Database.ExecuteSqlRawAsync($"DELETE FROM [{table}]");
                }

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_msforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'");

                await ReseedBaselineRowsAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Mirrors the HasData seed row in ApplicationDbContext.OnModelCreating — the state a
        // freshly migrated, never-used database starts in. SiteSettings/ServiceTimeSettings
        // aren't touched by the wipe (see ExcludedTables), so there's nothing to reseed for them.
        private async Task ReseedBaselineRowsAsync()
        {
            var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            await InsertWithIdentityAsync("Areas", () =>
                _context.Areas.Add(new Area { Id = 1, Name = "Unknown", DeliveryFee = 100, IsActive = true, CreatedAt = seedDate }));
        }

        private async Task InsertWithIdentityAsync(string table, Action addEntities)
        {
            await _context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{table}] ON");
            addEntities();
            await _context.SaveChangesAsync();
            await _context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{table}] OFF");
        }
    }
}
