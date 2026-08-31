using System;

namespace RestaurantSystem.Sync
{
    /// <summary>The wire format for a single synchronized change (see docs/SYNC_PROTOCOL.md §2).</summary>
    public sealed class SyncEnvelope
    {
        public Guid EventId { get; set; }
        public string EventType { get; set; } = "";
        public int SchemaVersion { get; set; } = SyncSchema.Current;
        public string AggregateType { get; set; } = "";
        public Guid AggregateGlobalId { get; set; }
        public long AggregateVersion { get; set; }
        public Guid BranchId { get; set; }
        public Guid OriginNodeId { get; set; }
        public DateTime OccurredAtUtc { get; set; }
        public Guid CorrelationId { get; set; }
        public Guid? CausationId { get; set; }
        public string PayloadJson { get; set; } = "{}";
    }

    public static class SyncSchema
    {
        /// <summary>Bump when the payload shape changes. Adjacent versions may run during a rollout.</summary>
        public const int Current = 1;
        public const int MinSupported = 1;
        public static bool IsSupported(int v) => v >= MinSupported && v <= Current;
    }

    public sealed class SyncPushRequest
    {
        public Guid BatchId { get; set; }
        public SyncEnvelope[] Events { get; set; } = Array.Empty<SyncEnvelope>();
    }

    public sealed class SyncAckItem
    {
        public Guid EventId { get; set; }
        public string Status { get; set; } = ""; // applied | duplicate | stale | conflict | deadletter
        public long? ConflictId { get; set; }
        public string? Detail { get; set; }
    }

    public sealed class SyncPushResponse
    {
        public Guid BatchId { get; set; }
        public SyncAckItem[] Results { get; set; } = Array.Empty<SyncAckItem>();
    }

    public sealed class SyncPullResponse
    {
        public long Cursor { get; set; }
        public long NextCursor { get; set; }
        public bool HasMore { get; set; }
        public SyncEnvelope[] Events { get; set; } = Array.Empty<SyncEnvelope>();
    }

    public sealed class SyncHeartbeatRequest
    {
        public Guid NodeId { get; set; }
        public string NodeRole { get; set; } = "";
        public string? AppVersion { get; set; }
        public string? SchemaVersion { get; set; }
        public DateTime SentAtUtc { get; set; }
        public int PendingOutbox { get; set; }
        public long LastPullCheckpoint { get; set; }
    }
}
