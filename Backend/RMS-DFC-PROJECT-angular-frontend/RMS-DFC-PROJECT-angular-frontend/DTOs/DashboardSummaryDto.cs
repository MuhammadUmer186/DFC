namespace RestaurantSystem.DTOs
{
    public class DashboardSummaryDto
    {
        public decimal TodayTotal { get; set; }
        public decimal WeeklyTotal { get; set; }
        public decimal MonthlyTotal { get; set; }
    }
    public class DashboardDto
    {
        public decimal MonthlyUtilityBills { get; set; }
        // other fields already existing…
    }

}
