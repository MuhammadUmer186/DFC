using System.ComponentModel.DataAnnotations;

namespace RestaurantSystem.DTOs
{
    public class UserDto
    {
        public string UserName { get; set; } = string.Empty;
        
        public string Password { get; set; } = string.Empty;
        public string Roles { get; set; } = string.Empty;
        public int? EmployeeId { get; set; }
        public int? RiderId { get; set; }
    }
}
