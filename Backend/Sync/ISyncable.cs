using System;

namespace RestaurantSystem.Sync
{
    /// <summary>
    /// A synchronized aggregate <b>root</b>. Carries a globally unique identity,
    /// cross-node version, UTC audit timestamps, a tombstone marker and a
    /// per-database concurrency token.
    /// <para>
    /// Offline-first / cloud-sync — Phase 2. The integer <c>Id</c> stays the
    /// internal key; <see cref="GlobalId"/> is the identity used by the sync
    /// protocol and by every cross-node reference.
    /// </para>
    /// </summary>
    public interface ISyncableAggregate
    {
        /// <summary>App-generated, globally unique, unique-indexed. Never changes.</summary>
        Guid GlobalId { get; set; }

        /// <summary><see cref="Models.Branch.BranchId"/> this row belongs to.</summary>
        Guid BranchId { get; set; }

        /// <summary><see cref="Models.SystemNode.NodeId"/> the row was first created on.</summary>
        Guid OriginNodeId { get; set; }

        /// <summary>
        /// Monotonically increasing per aggregate. Bumped on every change at the
        /// origin. Used for cross-node event ordering and stale-write rejection.
        /// Never derived from <see cref="RowVersion"/>.
        /// </summary>
        long AggregateVersion { get; set; }

        DateTime CreatedAtUtc { get; set; }
        DateTime UpdatedAtUtc { get; set; }

        /// <summary>Non-null once tombstoned. A tombstone row also lands in <c>SyncTombstones</c>.</summary>
        DateTime? DeletedAtUtc { get; set; }

        /// <summary>SQL <c>rowversion</c>. Concurrency inside ONE database only — never compared across nodes.</summary>
        byte[] RowVersion { get; set; }
    }

    /// <summary>
    /// A value/child entity owned by exactly one <see cref="ISyncableAggregate"/>
    /// root (order lines, deal items, recipe rows, ...). It has a stable
    /// <see cref="GlobalId"/> so the root's snapshot can be applied idempotently,
    /// but no independent version/tombstone/outbox handling — it syncs as part of
    /// its root.
    /// </summary>
    public interface ISyncableChild
    {
        Guid GlobalId { get; set; }
        DateTime CreatedAtUtc { get; set; }
        DateTime UpdatedAtUtc { get; set; }
    }
}
