namespace RestaurantSystem.DTOs
{
    public class InventoryRecommendationDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int RawItemId { get; set; }
        public string RawItemName { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal CurrentStock { get; set; }
        public decimal ForecastedDemand { get; set; }
        public decimal SuggestedReorderQuantity { get; set; }
        public DateOnly? SuggestedReorderDate { get; set; }
        public string RecommendationType { get; set; } = null!;
        public string Explanation { get; set; } = null!;
        public string? DataWarnings { get; set; }
        public decimal ConfidenceLow { get; set; }
        public decimal ConfidenceHigh { get; set; }
        public string Status { get; set; } = null!;
    }

    public class InventoryRecommendationDecisionRequest
    {
        public string Decision { get; set; } = null!; // Approved | Rejected | Modified
        public decimal? ModifiedQuantity { get; set; }
        public string? Feedback { get; set; }
    }
}
