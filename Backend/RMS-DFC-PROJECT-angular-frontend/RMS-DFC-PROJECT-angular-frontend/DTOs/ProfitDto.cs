namespace RestaurantSystem.DTOs
{
    public class ProfitDto
    {
        public DateOnly Date { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal Profit => TotalSales - TotalPurchases;
    }
    public class ProfitSummaryDto
    {
        public decimal TodayProfit { get; set; }
        public decimal WeeklyProfit { get; set; }
        public decimal MonthlyProfit { get; set; }
    }
    public class ProfitReportRequestDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
    public class ProfitReportResponseDto
    {
        public decimal TotalSales { get; set; }
        public decimal TotalSalaries { get; set; }
        public decimal Profit { get; set; }
    }


}
