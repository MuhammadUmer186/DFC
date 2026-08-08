using System;

namespace RestaurantSystem.DTOs
{
    public class AreaDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal DeliveryFee { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAreaDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal DeliveryFee { get; set; }
    }

    public class UpdateAreaDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal DeliveryFee { get; set; }
        public bool IsActive { get; set; }
    }
}
