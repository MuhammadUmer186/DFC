using System;
using Microsoft.EntityFrameworkCore;

namespace RestaurantSystem.Models
{
    // Every approve/reject/modify action a manager takes on a recommendation, kept even after
    // the recommendation itself is superseded by a newer run — this is the audit trail plus the
    // feedback signal a future model iteration would train against.
    public class AiInventoryRecommendationDecision
    {
        public int Id { get; set; }
        public int RecommendationId { get; set; }
        public virtual AiInventoryRecommendation Recommendation { get; set; } = null!;

        public DateTime DecidedAt { get; set; }
        public int? DecidedByUserId { get; set; }
        public string? DecidedByUserName { get; set; }
        public string Decision { get; set; } = null!; // Approved | Rejected | Modified
        [Precision(18, 2)]
        public decimal? ModifiedQuantity { get; set; }
        public string? Feedback { get; set; }
    }
}
