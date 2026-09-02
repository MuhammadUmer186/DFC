using System;

namespace RestaurantSystem.Models
{
    public partial class Area
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal DeliveryFee { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}
