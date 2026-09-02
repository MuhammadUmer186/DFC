using System;

namespace RestaurantSystem.Models
{
    // ================= Offline-first / cloud-sync — Phase 5 =================
    // Transactional outbox / idempotent inbox / checkpoints / conflicts /
    // dead-letters / nonce store. All node-local bookkeeping — NOT themselves
    // synchronized.

    /// <summary>An outbound change event, written in the same transaction as the business change.</summary>
    public partial class SyncOutbox
    {
        public long Id { get; set; }
        public Guid EventId { get; set; }
        public string EventType { get; set; } = null!;
        public int SchemaVersion { get; set; }
        public string AggregateType { get; set; } = null!;
        public Guid AggregateGlobalId { get; set; }
        public long AggregateVersion { get; set; }
        public Guid BranchId { get; set; }
        public Guid OriginNodeId { get; set; }
        public DateTime OccurredAtUtc { get; set; }
        public Guid CorrelationId { get; set; }
        public Guid? CausationId { get; set; }
        public string PayloadJson { get; set; } = null!;

        public DateTime CreatedAtUtc { get; set; }
        public bool Dispatched { get; set; }
        public DateTime? DispatchedAtUtc { get; set; }
        public int Attempts { get; set; }
        public string? LastError { get; set; }
    }

    /// <summary>Record of an applied inbound event — the idempotency guard for receivers.</summary>
    public partial class SyncInbox
    {
        public long Id { get; set; }
        public Guid EventId { get; set; }
        public string EventType { get; set; } = null!;
        public string AggregateType { get; set; } = null!;
        public Guid AggregateGlobalId { get; set; }
        public long AggregateVersion { get; set; }
        public Guid OriginNodeId { get; set; }
        public DateTime ReceivedAtUtc { get; set; }
        public DateTime AppliedAtUtc { get; set; }
        public string Status { get; set; } = "applied"; // applied | duplicate | stale | conflict | deadletter
        public long? ConflictId { get; set; }
    }

    /// <summary>Per-peer, per-direction, per-aggregate-type high-water mark.</summary>
    public partial class SyncCheckpoint
    {
        public int Id { get; set; }
        public Guid PeerNodeId { get; set; }
        public string Direction { get; set; } = null!; // push | pull
        public string AggregateType { get; set; } = "*";
        public long LastValue { get; set; }            // last Outbox.Id acked (push) / last SyncInbox cursor (pull)
        public DateTime UpdatedAtUtc { get; set; }
    }

    /// <summary>An unresolved cross-node conflict for MainAdmin/SuperAdmin review.</summary>
    public partial class SyncConflict
    {
        public long Id { get; set; }
        public Guid EventId { get; set; }
        public string Kind { get; set; } = null!;      // stale | same-version-divergent | version-gap | domain-rule | negative-stock | schema
        public string AggregateType { get; set; } = null!;
        public Guid AggregateGlobalId { get; set; }
        public long IncomingVersion { get; set; }
        public long LocalVersion { get; set; }
        public Guid OriginNodeId { get; set; }
        public string? IncomingPayloadJson { get; set; }
        public string? LocalSnapshotJson { get; set; }
        public string? Detail { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public bool Resolved { get; set; }
        public string? Resolution { get; set; }        // keepLocal | keepRemote | manual
        public string? ResolvedByUserName { get; set; }
        public DateTime? ResolvedAtUtc { get; set; }
    }

    /// <summary>An event that could not be applied and is not a plain conflict (bad schema, repeated failure).</summary>
    public partial class SyncDeadLetter
    {
        public long Id { get; set; }
        public Guid EventId { get; set; }
        public string Kind { get; set; } = null!;      // schema | apply-error
        public string AggregateType { get; set; } = null!;
        public Guid AggregateGlobalId { get; set; }
        public int SchemaVersion { get; set; }
        public string EnvelopeJson { get; set; } = null!;
        public string? Error { get; set; }
        public int Attempts { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? LastAttemptAtUtc { get; set; }
        public bool Replayed { get; set; }
    }

    /// <summary>Seen HMAC nonces, for replay rejection. TTL-pruned.</summary>
    public partial class SyncNonce
    {
        public long Id { get; set; }
        public Guid NodeId { get; set; }
        public string Nonce { get; set; } = null!;
        public DateTime SeenAtUtc { get; set; }
    }
}
