using RestaurantSystem.Models;

namespace RestaurantSystem.DTOs
{
    public class PaySalaryDto
    {
        public int EmployeeId { get; set; }
        public SalaryType SalaryType { get; set; }
        public decimal AmountPaid { get; set; }
        public DateOnly? ForDate { get; set; }   // Daily
        public string? ForMonth { get; set; }    // Monthly (YYYY-MM)
        public string? Remarks { get; set; }
    }

    public class SalaryPaymentResponseDto
    {
        public int Id { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaidAt { get; set; }
        public DateOnly? ForDate { get; set; }
        public string? ForMonth { get; set; }
    }


}
