using System;

namespace RestaurantSystem.Models
{
    /// <summary>
    /// Idempotency ledger (Phase 6). One row per <c>Idempotency-Key</c> seen on a
    /// mutating request. A retry with the same key + same request body replays the
    /// stored result; the same key with a different body is a <c>409</c>.
    /// Node-local — NOT synchronized (cross-node dedupe is handled by the
    /// aggregate <c>GlobalId</c> + the sync inbox).
    /// </summary>
    public partial class ProcessedCommand
    {
        public long Id { get; set; }

        /// <summary>The client-supplied <c>Idempotency-Key</c> (a UUID).</summary>
        public Guid CommandId { get; set; }

        public Guid NodeId { get; set; }

        /// <summary><c>METHOD /path</c>.</summary>
        public string Route { get; set; } = null!;

        /// <summary>base64(SHA-256(method \n path \n rawBody)).</summary>
        public string RequestHash { get; set; } = null!;

        /// <summary>in-progress | completed | failed</summary>
        public string State { get; set; } = "in-progress";

        public int? StatusCode { get; set; }

        /// <summary>GlobalId of the aggregate the command created/affected, when known.</summary>
        public Guid? ResultGlobalId { get; set; }

        public string? ResponseContentType { get; set; }

        /// <summary>Captured response body (truncated to <c>Idempotency:MaxStoredBodyBytes</c>).</summary>
        public string? ResponseBody { get; set; }

        public bool ResponseTruncated { get; set; }

        public DateTime StartedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
    }
}
