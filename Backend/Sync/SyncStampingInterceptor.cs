using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RestaurantSystem.Models;

namespace RestaurantSystem.Sync
{
    /// <summary>
    /// Stamps synchronization identity on every write:
    /// <list type="bullet">
    /// <item>new roots/children get a <see cref="ISyncableAggregate.GlobalId"/> and UTC timestamps;</item>
    /// <item>modified roots bump <see cref="ISyncableAggregate.AggregateVersion"/> and <c>UpdatedAtUtc</c>;</item>
    /// <item>deleted roots leave a <see cref="SyncTombstone"/> in the same transaction.</item>
    /// </list>
    /// Phase 5 extends this same interceptor to also write the transactional outbox.
    /// <para>
    /// Set <see cref="AsyncLocalSuppression"/> while applying inbound sync events so
    /// imported writes are not re-stamped or re-versioned.
    /// </para>
    /// Offline-first / cloud-sync — Phase 2.
    /// </summary>
    public sealed class SyncStampingInterceptor : SaveChangesInterceptor
    {
        private static readonly AsyncLocal<bool> _suppressed = new();

        /// <summary>When true on the current async flow, the interceptor is a no-op.</summary>
        public static bool AsyncLocalSuppression
        {
            get => _suppressed.Value;
            set => _suppressed.Value = value;
        }

        /// <summary>RAII helper: <c>using var _ = SyncStampingInterceptor.Suppress();</c></summary>
        public static IDisposable Suppress()
        {
            var prev = _suppressed.Value;
            _suppressed.Value = true;
            return new Restore(prev);
        }

        private readonly INodeContext _node;

        public SyncStampingInterceptor(INodeContext node) => _node = node;

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData, InterceptionResult<int> result)
        {
            Stamp(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Stamp(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void Stamp(DbContext? context)
        {
            if (context is null || _suppressed.Value) return;

            var now = DateTime.UtcNow;
            var nodeId = _node.NodeId;
            var branchId = _node.BranchId;

            // Snapshot the entries first — we add tombstone rows while iterating.
            var entries = context.ChangeTracker.Entries().ToList();

            foreach (var entry in entries)
            {
                switch (entry.Entity)
                {
                    case ISyncableAggregate root:
                        StampRoot(context, entry, root, now, nodeId, branchId);
                        break;
                    case ISyncableChild child:
                        StampChild(entry, child, now);
                        break;
                }
            }
        }

        private static void StampRoot(
            DbContext context, EntityEntry entry, ISyncableAggregate root,
            DateTime now, Guid nodeId, Guid branchId)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (root.GlobalId == Guid.Empty) root.GlobalId = Guid.NewGuid();
                    if (root.OriginNodeId == Guid.Empty) root.OriginNodeId = nodeId;
                    if (root.BranchId == Guid.Empty) root.BranchId = branchId;
                    if (root.CreatedAtUtc == default) root.CreatedAtUtc = now;
                    root.UpdatedAtUtc = now;
                    if (root.AggregateVersion <= 0) root.AggregateVersion = 1;
                    break;

                case EntityState.Modified:
                    if (root.GlobalId == Guid.Empty) root.GlobalId = Guid.NewGuid();
                    if (root.OriginNodeId == Guid.Empty) root.OriginNodeId = nodeId;
                    if (root.BranchId == Guid.Empty) root.BranchId = branchId;
                    if (root.CreatedAtUtc == default) root.CreatedAtUtc = now;
                    root.UpdatedAtUtc = now;
                    root.AggregateVersion = root.AggregateVersion <= 0 ? 1 : root.AggregateVersion + 1;
                    break;

                case EntityState.Deleted:
                    // Tombstone in the same unit of work, then let the delete proceed.
                    var version = (root.AggregateVersion <= 0 ? 1 : root.AggregateVersion) + 1;
                    context.Add(new SyncTombstone
                    {
                        GlobalId = root.GlobalId == Guid.Empty ? Guid.NewGuid() : root.GlobalId,
                        AggregateType = entry.Metadata.ClrType.Name,
                        BranchId = root.BranchId == Guid.Empty ? branchId : root.BranchId,
                        OriginNodeId = nodeId,
                        AggregateVersion = version,
                        DeletedAtUtc = now,
                        Dispatched = false
                    });
                    break;
            }
        }

        private static void StampChild(EntityEntry entry, ISyncableChild child, DateTime now)
        {
            if (entry.State is EntityState.Added)
            {
                if (child.GlobalId == Guid.Empty) child.GlobalId = Guid.NewGuid();
                if (child.CreatedAtUtc == default) child.CreatedAtUtc = now;
                child.UpdatedAtUtc = now;
            }
            else if (entry.State is EntityState.Modified)
            {
                if (child.GlobalId == Guid.Empty) child.GlobalId = Guid.NewGuid();
                if (child.CreatedAtUtc == default) child.CreatedAtUtc = now;
                child.UpdatedAtUtc = now;
            }
        }

        private sealed class Restore : IDisposable
        {
            private readonly bool _prev;
            public Restore(bool prev) => _prev = prev;
            public void Dispose() => _suppressed.Value = _prev;
        }
    }
}
