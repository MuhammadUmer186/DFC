using System;

namespace RestaurantSystem.Models
{
    // Deliberately minimal — groundwork for Milestone 2 (personalized recommendations), not
    // wired into order creation yet. PhoneNumber is the natural key since it's already how
    // Order.PhoneNumber identifies a guest today. Allergens/DietaryPreferences are plain
    // comma-separated text for now rather than normalized child tables — a reasonable
    // Milestone-2 follow-up once personalization logic actually consumes them.
    public partial class Customer
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string? Name { get; set; }
        public string? Address { get; set; }

        /// Must be explicitly true before any personalization/recommendation logic may use this
        /// customer's history — "customer consent and privacy settings" / "provide a way for
        /// customers to reset or disable personalization."
        public bool PersonalizationConsent { get; set; } = false;

        public string? Allergens { get; set; }
        public string? DietaryPreferences { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
