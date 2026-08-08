namespace RestaurantSystem.DTOs
{
    public class KitchenOutListDto
    {
        public int Id { get; set; }
        public string? ReferenceNo { get; set; }
        public DateTime IssuedAt { get; set; }
        public List<KitchenOutListItemDto> Items { get; set; } = new();
    }

    public class KitchenOutListItemDto
    {
        public int RawItemId { get; set; }
        public string RawItemName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
    }
}
