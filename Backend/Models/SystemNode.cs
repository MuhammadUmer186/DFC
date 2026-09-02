using System;

namespace RestaurantSystem.Models
{
    /// <summary>
    /// A physical deployment of the backend + database (a Cloud install on the
    /// VPS, or an Edge install on the restaurant LAN). Every synced record stamps
    /// the <see cref="NodeId"/> it was created on via its <c>OriginNodeId</c>
    /// column (Phase 2).
    /// <para>
    /// Offline-first / cloud-sync — Phase 1. Additive new table.
    /// </para>
    /// </summary>
    public partial class SystemNode
    {
        /// <summary>Internal per-database key. Not meaningful across nodes.</summary>
        public int Id { get; set; }

        /// <summary>
        /// Stable identity for this deployment. Sourced, in order of precedence:
        /// <c>Deployment:NodeId</c> config → persisted node-id file → generated
        /// once and persisted. Never changes across container restarts.
        /// </summary>
        public Guid NodeId { get; set; }

        public NodeRole Role { get; set; }

        /// <summary>The <see cref="Branch.BranchId"/> this node serves.</summary>
        public Guid BranchId { get; set; }

        public string? DisplayName { get; set; }

        /// <summary>
        /// Base URL other nodes use to reach this one's <c>/api/sync/*</c>
        /// endpoints (from <c>Deployment:EdgeBaseUrl</c> / <c>CloudBaseUrl</c>).
        /// </summary>
        public string? BaseUrl { get; set; }

        /// <summary>Informational — assembly version last seen running on this node.</summary>
        public string? AppVersion { get; set; }

        /// <summary>Informational — EF migration/schema version last seen on this node.</summary>
        public string? SchemaVersion { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime RegisteredAtUtc { get; set; }

        /// <summary>Updated on every startup self-registration and every heartbeat.</summary>
        public DateTime LastSeenAtUtc { get; set; }
    }
}
