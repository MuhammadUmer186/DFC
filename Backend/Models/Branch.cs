using System;

namespace RestaurantSystem.Models
{
    /// <summary>
    /// A physical restaurant branch. One branch is served by one or more
    /// <see cref="SystemNode"/>s (typically a Cloud node plus an Edge node).
    /// <para>
    /// Offline-first / cloud-sync — Phase 1. Additive: this is a brand new table,
    /// no existing entity or column is touched.
    /// </para>
    /// </summary>
    public partial class Branch
    {
        /// <summary>Internal per-database key. Not meaningful across nodes.</summary>
        public int Id { get; set; }

        /// <summary>
        /// Stable global identity used by every synced aggregate's <c>BranchId</c>
        /// column and by the sync protocol. Comes from <c>Deployment:BranchId</c>
        /// configuration; generated once and logged if configuration is empty.
        /// </summary>
        public Guid BranchId { get; set; }

        public string Name { get; set; } = null!;

        /// <summary>
        /// Short human code (e.g. <c>DFC</c>). Defaults from
        /// <c>SiteSetting.OrderSerialPrefix</c> when not configured. Used by the
        /// per-branch order-number sequences introduced in Phase 3.
        /// </summary>
        public string? Code { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
