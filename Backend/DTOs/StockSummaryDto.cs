namespace RestaurantSystem.DTOs
{
    public class StockSummaryDto
    {
        public int RawItemId { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public decimal TotalQuantity { get; set; }
    }

    public class StockUsagePercentageDto
    {
        public decimal KitchenOutPercentage { get; set; }
        public decimal WastePercentage { get; set; }
    }


}
