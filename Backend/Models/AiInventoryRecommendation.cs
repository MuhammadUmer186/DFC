using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RestaurantSystem.Models
{
    public class AiInventoryRecommendation
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }

        public int RawItemId { get; set; }
        public virtual RawItem RawItem { get; set; } = null!;

        [Precision(18, 2)]
        public decimal CurrentStock { get; set; }
        [Precision(18, 2)]
        public decimal ForecastedDemand { get; set; }
        [Precision(18, 2)]
        public decimal SuggestedReorderQuantity { get; set; }
        public DateOnly? SuggestedReorderDate { get; set; }

        public string RecommendationType { get; set; } = null!; // LowStock | Reorder | ExcessStock | ExpiryRisk | WasteReduction
        public string Explanation { get; set; } = null!;

        /// Populated when supplier lead time, MOQ, safety stock, or shelf life data is missing —
        /// per "warn when recipe, inventory, or supplier data is missing or inconsistent" instead
        /// of silently assuming a default.
        public string? DataWarnings { get; set; }

        [Precision(18, 2)]
        public decimal ConfidenceLow { get; set; }
        [Precision(18, 2)]
        public decimal ConfidenceHigh { get; set; }

        public string Status { get; set; } = "Pending"; // Pending | Approved | Rejected | Modified

        public virtual ICollection<AiInventoryRecommendationDecision> Decisions { get; set; } = new List<AiInventoryRecommendationDecision>();
    }
}
