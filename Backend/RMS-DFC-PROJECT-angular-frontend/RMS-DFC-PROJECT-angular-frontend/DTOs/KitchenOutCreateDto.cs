namespace RestaurantSystem.DTOs
{
    public class KitchenOutCreateDto
    {
        public string? ReferenceNo { get; set; }
        public List<KitchenOutItemDto> Items { get; set; }
    }
}
