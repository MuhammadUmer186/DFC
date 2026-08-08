using RestaurantSystem.Models;

namespace RestaurantSystem.DTOs
{
    public class EmployeeResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MobileNumber { get; set; }
        public string Designation { get; set; } = string.Empty;
        public string NationalId { get; set; }
        public string Address { get; set; }
        public SalaryType SalaryType { get; set; }
        public decimal SalaryAmount { get; set; }
        public bool IsActive { get; set; }
    }
    // For response of salary status
    public class EmployeeSalaryStatusDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string MobileNumber { get; set; }
        public int SalaryType { get; set; } // 1 = Daily, 2 = Monthly
        public decimal SalaryAmount { get; set; }
        public bool IsPaid { get; set; }    // true if paid
        public DateOnly? ForDate { get; set; }  // only for daily
        public string? ForMonth { get; set; }   // only for monthly
    }



}
