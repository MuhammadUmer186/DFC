namespace RestaurantSystem.DTOs
{
    namespace RestaurantSystem.DTOs
    {
        public class UtilityBillDto
        {
            public int Id { get; set; }
            public string BillType { get; set; }
            public decimal Amount { get; set; }
            public DateOnly BillDate { get; set; }
            public string? Notes { get; set; }
        }

        public class BillsSummaryDto
        {
            public decimal TodayTotal { get; set; }
            public decimal WeeklyTotal { get; set; }
            public decimal MonthlyTotal { get; set; }
            public decimal OverallTotal { get; set; }
        }
    }

}
