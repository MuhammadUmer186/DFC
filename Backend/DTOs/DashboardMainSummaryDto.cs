namespace RestaurantSystem.DTOs
{
    public class DashboardMainSummaryDto
    {
        public int TotalCategories { get; set; }
        public int TotalMenuItems { get; set; }
        public int TotalVendors { get; set; }
        public int TotalRawItems { get; set; }

        public decimal TodaySales { get; set; }
        public decimal TodayOnlineSales { get; set; }
        public decimal TodaySiteSales { get; set; }
        public decimal TodayPurchases { get; set; }

        public decimal CurrentStockValue { get; set; }
    }

}
