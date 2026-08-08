using RestaurantSystem.Models;

namespace RestaurantSystem.DTOs
{
    public class DailySalaryReportDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public SalaryType SalaryType { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaidAt { get; set; }
    }

}
