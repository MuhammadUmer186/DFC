using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.Models;

namespace RestaurantSystem.Sync
{
    /// <summary>Bound from the <c>PublicOrdering</c> config section. Phase 11.</summary>
    public sealed class PublicOrderingOptions
    {
        public const string SectionName = "PublicOrdering";

        /// <summary>Block checkout on the public site while the restaurant edge is offline.</summary>
        public bool DisableCheckoutWhenEdgeOffline { get; set; } = false;

        /// <summary>
        /// Accept online orders while the edge is offline, holding them as
        /// awaiting-confirmation (no inventory, no promises). They sync to the
        /// edge when it reconnects. Takes precedence over
        /// <see cref="DisableCheckoutWhenEdgeOffline"/>.
        /// </summary>
        public bool AllowDelayedOnlineOrders { get; set; } = true;

        /// <summary>Seconds since the last edge heartbeat before the edge counts as offline.</summary>
        public int EdgeOfflineThresholdSeconds { get; set; } = 90;
    }

    /// <summary>Reports whether the restaurant edge is currently reachable (meaningful on the Cloud node).</summary>
    public sealed class EdgeConnectivity
    {
        private readonly ApplicationDbContext _db;
        private readonly PublicOrderingOptions _opts;
        private readonly INodeContext _node;

        public EdgeConnectivity(ApplicationDbContext db,
            Microsoft.Extensions.Options.IOptions<PublicOrderingOptions> opts, INodeContext node)
        {
            _db = db;
            _opts = opts.Value;
            _node = node;
        }

        public async Task<(bool edgeOnline, DateTime? lastSeenUtc)> GetAsync(CancellationToken ct = default)
        {
            // On an Edge node the shop is "online" by definition.
            if (_node.Role == NodeRole.Edge) return (true, DateTime.UtcNow);

            var since = DateTime.UtcNow.AddSeconds(-_opts.EdgeOfflineThresholdSeconds);
            var last = await _db.NodeHeartbeats.AsNoTracking()
                .Where(h => h.Role == NodeRole.Edge)
                .OrderByDescending(h => h.ReceivedAtUtc)
                .Select(h => (DateTime?)h.ReceivedAtUtc)
                .FirstOrDefaultAsync(ct);

            // fall back to the Edge SystemNode's LastSeenAtUtc
            last ??= await _db.SystemNodes.AsNoTracking()
                .Where(n => n.Role == NodeRole.Edge && n.IsActive)
                .OrderByDescending(n => n.LastSeenAtUtc)
                .Select(n => (DateTime?)n.LastSeenAtUtc)
                .FirstOrDefaultAsync(ct);

            return (last is not null && last >= since, last);
        }

        public async Task<(bool acceptingOrders, bool delayed, string message)> EvaluateCheckoutAsync(CancellationToken ct = default)
        {
            var (edgeOnline, _) = await GetAsync(ct);
            if (edgeOnline)
                return (true, false, "Online ordering is open.");

            if (_opts.AllowDelayedOnlineOrders)
                return (true, true,
                    "The restaurant is briefly offline. Your order will be received and confirmed by staff shortly — timing isn't guaranteed until then.");

            if (_opts.DisableCheckoutWhenEdgeOffline)
                return (false, false,
                    "Online ordering is temporarily unavailable while the restaurant is offline. Please try again shortly or call us.");

            return (true, false, "Online ordering is open.");
        }
    }
}
