namespace RestaurantSystem.DTOs
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string? Name { get; set; }
        public bool PersonalizationConsent { get; set; }
        public string? Allergens { get; set; }
        public string? DietaryPreferences { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UpsertCustomerDto
    {
        public string PhoneNumber { get; set; } = null!;
        public string? Name { get; set; }
        public bool PersonalizationConsent { get; set; }
        public string? Allergens { get; set; }
        public string? DietaryPreferences { get; set; }
    }
}
