using System;

namespace RestaurantSystem.Models
{
    /// <summary>
    /// A liveness sample for a node. Written on startup ("self" heartbeats) and,
    /// from Phase 5, whenever a peer calls <c>POST /api/sync/heartbeat</c>
    /// ("peer:&lt;nodeId&gt;" heartbeats). The most recent row per node feeds the
    /// operating-mode decision and the RMS status widget.
    /// <para>
    /// Offline-first / cloud-sync — Phase 1. Additive new table.
    /// </para>
    /// </summary>
    public partial class NodeHeartbeat
    {
        public long Id { get; set; }

        public Guid NodeId { get; set; }
        public NodeRole Role { get; set; }
        public Guid BranchId { get; set; }

        /// <summary>When the sender produced the heartbeat (UTC).</summary>
        public DateTime SentAtUtc { get; set; }

        /// <summary>When this node recorded it (UTC).</summary>
        public DateTime ReceivedAtUtc { get; set; }

        public string? AppVersion { get; set; }
        public string? SchemaVersion { get; set; }

        /// <summary>Outbox depth reported by the sender at heartbeat time (Phase 5).</summary>
        public int PendingOutboxCount { get; set; }

        /// <summary><c>self</c> or <c>peer:&lt;nodeId&gt;</c>.</summary>
        public string? Source { get; set; }

        /// <summary>Optional JSON blob of extra, non-sensitive diagnostics.</summary>
        public string? DetailsJson { get; set; }
    }
}
