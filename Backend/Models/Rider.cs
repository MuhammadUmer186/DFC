using System;
using System.Collections.Generic;

namespace RestaurantSystem.Models
{
    public partial class Rider
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public string? VehicleNumber { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
