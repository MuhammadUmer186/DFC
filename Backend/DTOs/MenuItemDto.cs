namespace RestaurantSystem.DTOs
{
    public class MenuItemDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
    }
    public class MenuItemStatsDto
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; }
        public int TodayCount { get; set; }
        public int WeekCount { get; set; }
        public int MonthCount { get; set; }
    }
}
