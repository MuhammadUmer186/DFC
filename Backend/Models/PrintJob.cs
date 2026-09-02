using System;

namespace RestaurantSystem.Models
{
    /// <summary>
    /// A print request and its outcome (Phase 13). Node-local — printing is a
    /// local device concern and is never synchronized. The row is also the audit
    /// trail for manual reprints.
    /// </summary>
    public partial class PrintJob
    {
        public long Id { get; set; }

        public Guid PrintJobId { get; set; }

        /// <summary>KitchenTicket | Receipt | DeliverySlip</summary>
        public string JobType { get; set; } = "DeliverySlip";

        /// <summary>e.g. <c>customer</c> / <c>kitchen</c> — distinguishes copies of the same slip.</summary>
        public string Copy { get; set; } = "customer";

        public int? OrderId { get; set; }
        public Guid? OrderGlobalId { get; set; }

        public string PayloadJson { get; set; } = "{}";

        /// <summary>queued | printed | failed | skipped</summary>
        public string Status { get; set; } = "queued";
        public int Attempts { get; set; }
        public string? Error { get; set; }

        public bool IsReprint { get; set; }
        public string? ReprintReason { get; set; }
        public string? RequestedByUserName { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
    }
}
