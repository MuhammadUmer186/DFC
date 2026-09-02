using System;

namespace RestaurantSystem.Models
{
    /// <summary>
    /// Records that a synchronized aggregate root was deleted, so the deletion
    /// propagates to peers even when the row itself is hard-deleted locally.
    /// Written by <c>SyncStampingInterceptor</c> in the same transaction as the
    /// delete. Offline-first / cloud-sync — Phase 2.
    /// </summary>
    public partial class SyncTombstone
    {
        public long Id { get; set; }

        public Guid GlobalId { get; set; }
        public string AggregateType { get; set; } = null!;

        public Guid BranchId { get; set; }
        public Guid OriginNodeId { get; set; }

        /// <summary>Last known aggregate version before deletion (+1).</summary>
        public long AggregateVersion { get; set; }

        public DateTime DeletedAtUtc { get; set; }

        /// <summary>Set once the sync worker has emitted the delete event for this tombstone.</summary>
        public bool Dispatched { get; set; }
    }
}
