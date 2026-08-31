using System;

namespace RestaurantSystem.Models
{
    /// <summary>
    /// A per-writer order-number counter (Phase 3). One row per
    /// (branch, source, business day). Allocation is a single atomic
    /// <c>MERGE … WITH (HOLDLOCK) … OUTPUT</c>, so concurrent order creation on
    /// the same node cannot hand out duplicates. Node-local — each node owns its
    /// own sequences (an Edge issues <c>POS</c>/<c>WEB</c>, the Cloud issues
    /// <c>CLD</c>), which is exactly why local and cloud never collide.
    /// </summary>
    public partial class OrderNumberSequence
    {
        public int Id { get; set; }

        public Guid BranchId { get; set; }

        /// <summary><c>POS</c> | <c>WEB</c> | <c>CLD</c>.</summary>
        public string SourceCode { get; set; } = null!;

        /// <summary>Restaurant business day (date component), from <see cref="Services.IRestaurantClock"/>.</summary>
        public DateTime BusinessDate { get; set; }

        public int LastValue { get; set; }

        public DateTime UpdatedAtUtc { get; set; }
    }
}
