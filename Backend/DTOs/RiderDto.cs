using System;

namespace RestaurantSystem.DTOs
{
    public class RiderDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? VehicleNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ActiveDeliveryCount { get; set; }
    }

    public class CreateRiderDto
    {
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? VehicleNumber { get; set; }
    }

    public class UpdateRiderDto
    {
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? VehicleNumber { get; set; }
        public bool IsActive { get; set; }
    }
}
