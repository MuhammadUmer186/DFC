using RestaurantSystem.Models;

namespace RestaurantSystem.DTOs
{
    public class CreateEmployeeDto
    {
        public string Name { get; set; }
        public string MobileNumber { get; set; }
        public string Designation { get; set; }
        public string NationalId { get; set; }
        public string Address { get; set; }
        public SalaryType SalaryType { get; set; }
        public decimal SalaryAmount { get; set; }
    }

    public class UpdateEmployeeDto : CreateEmployeeDto
    {
        public bool IsActive { get; set; }
    }


}
