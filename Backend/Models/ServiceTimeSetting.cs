using System;

namespace RestaurantSystem.Models
{
    public class ServiceTimeSetting
    {
        public int Id { get; set; }
        public string ServiceType { get; set; } = string.Empty; // DineIn / Takeaway / Delivery
        public int MinMinutes { get; set; }
        public int MaxMinutes { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
