namespace RestaurantSystem.DTOs
{
    public class CreateMenuItemDto
    {
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
    }
}
