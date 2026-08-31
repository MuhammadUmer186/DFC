using System;

namespace RestaurantSystem.Sync
{
    /// <summary>
    /// Process-wide ambient identity of the running node, populated once at
    /// startup by <see cref="NodeRegistrationService"/>. Consumed by the
    /// <c>SyncStampingInterceptor</c> and the sync worker.
    /// Offline-first / cloud-sync — Phase 2.
    /// </summary>
    public interface INodeContext
    {
        bool IsReady { get; }
        Guid NodeId { get; }
        Guid BranchId { get; }
        RestaurantSystem.Models.NodeRole Role { get; }
        string AppVersion { get; }
        string? SchemaVersion { get; }
        NodeIdentity? Identity { get; }
        void Set(NodeIdentity identity);
    }

    public sealed class NodeContext : INodeContext
    {
        private NodeIdentity? _identity;

        public bool IsReady => _identity is not null;
        public NodeIdentity? Identity => _identity;

        public Guid NodeId => _identity?.NodeId ?? Guid.Empty;
        public Guid BranchId => _identity?.BranchId ?? Guid.Empty;
        public RestaurantSystem.Models.NodeRole Role => _identity?.Role ?? RestaurantSystem.Models.NodeRole.Edge;
        public string AppVersion => _identity?.AppVersion ?? "0.0.0.0";
        public string? SchemaVersion => _identity?.SchemaVersion;

        public void Set(NodeIdentity identity) => _identity = identity;
    }
}
