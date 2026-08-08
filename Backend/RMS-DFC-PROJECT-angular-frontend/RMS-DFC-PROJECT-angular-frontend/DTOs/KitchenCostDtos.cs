namespace RestaurantSystem.DTOs
{
    namespace RestaurantSystem.Dtos.Reports
    {
        public class RawItemAvgCostDto
        {
            public int RawItemId { get; set; }
            public string RawItemName { get; set; } = "";
            public decimal AverageCost { get; set; }
        }

        public class DailyKitchenCostDto
        {
            public DateOnly Date { get; set; }
            public decimal TotalKitchenCost { get; set; }
            public decimal TotalWasteCost { get; set; }
        }

        public class MonthlyKitchenCostDto
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public decimal TotalKitchenCost { get; set; }
            public decimal TotalWasteCost { get; set; }
        }
    }

}
