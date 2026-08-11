namespace RestaurantSystem.DTOs
{
    public class ForecastValueDto
    {
        public DateOnly ForecastDate { get; set; }
        public int? HourOfDay { get; set; }
        public int? MenuItemId { get; set; }
        public string? MenuItemName { get; set; }
        public decimal PredictedSales { get; set; }
        public int PredictedOrderCount { get; set; }
        public decimal PredictedQuantity { get; set; }
        public decimal ConfidenceLow { get; set; }
        public decimal ConfidenceHigh { get; set; }
        public bool LowConfidence { get; set; }
        public decimal? ActualSales { get; set; }
        public int? ActualOrderCount { get; set; }
    }

    public class ForecastRunDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ModelVersion { get; set; } = null!;
        public DateOnly ForecastFrom { get; set; }
        public DateOnly ForecastTo { get; set; }
        public string Status { get; set; } = null!;
        public string? Notes { get; set; }
        public decimal? Mae { get; set; }
        public decimal? Wape { get; set; }
        public List<ForecastValueDto> Values { get; set; } = new();
    }

    public class ForecastRunSummaryDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateOnly ForecastFrom { get; set; }
        public DateOnly ForecastTo { get; set; }
        public decimal? Mae { get; set; }
        public decimal? Wape { get; set; }
    }
}
