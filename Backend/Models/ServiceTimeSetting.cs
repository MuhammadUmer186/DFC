using System;

namespace RestaurantSystem.Models
{
    public partial class ServiceTimeSetting
    {
        public int Id { get; set; }
        public string ServiceType { get; set; } = string.Empty; // DineIn / Takeaway / Delivery
        public int MinMinutes { get; set; }
        public int MaxMinutes { get; set; }
        public bool IsEnabled { get; set; } = true;
        public DateTime UpdatedAt { get; set; }
    }
}
