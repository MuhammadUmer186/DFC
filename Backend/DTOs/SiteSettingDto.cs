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
    }
}
