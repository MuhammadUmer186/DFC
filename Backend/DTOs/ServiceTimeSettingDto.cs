namespace RestaurantSystem.DTOs
{
    public class ServiceTimeSettingDto
    {
        public int Id { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public int MinMinutes { get; set; }
        public int MaxMinutes { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class UpdateServiceTimeSettingDto
    {
        public int MinMinutes { get; set; }
        public int MaxMinutes { get; set; }
        public bool IsEnabled { get; set; }
    }
}
