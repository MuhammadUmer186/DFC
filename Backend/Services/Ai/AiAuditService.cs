using RestaurantSystem.Data;
using RestaurantSystem.Models;

namespace RestaurantSystem.Services.Ai
{
    public interface IAiAuditService
    {
        Task LogAsync(AiAuditLog entry, CancellationToken ct = default);
    }

    public class AiAuditService : IAiAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AiAuditService> _logger;

        public AiAuditService(ApplicationDbContext context, ILogger<AiAuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogAsync(AiAuditLog entry, CancellationToken ct = default)
        {
            entry.CreatedAt = DateTime.Now;
            try
            {
                _context.AiAuditLogs.Add(entry);
                await _context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                // Audit logging must never take down the actual AI feature — log and move on.
                _logger.LogError(ex, "Failed to write AI audit log entry for feature {Feature}", entry.Feature);
            }
        }
    }
}
