namespace RestaurantSystem.DTOs
{
    public class UpdateWhatsAppNumberDto
    {
        public string? WhatsAppNumber { get; set; }
    }
    public class UpdateRestaurantNameDto
    {
        public string? RestaurantName { get; set; }
    }
    public class UpdateCompanyNameDto
    {
        public string? CompanyName { get; set; }
    }
    public class UpdateGoogleMapsUrlDto
    {
        public string? GoogleMapsUrl { get; set; }
    }
    public class UpdateOrderSerialSettingDto
    {
        public string? Prefix { get; set; }
        public int StartingNumber { get; set; }
        public TimeSpan ResetTime { get; set; }
    }
    public class UpdateCountryTimeZoneDto
    {
        public string? Country { get; set; }
        public string? TimeZoneId { get; set; }
        public string? CurrencyCode { get; set; }
        public string? CurrencySymbol { get; set; }
    }
}
