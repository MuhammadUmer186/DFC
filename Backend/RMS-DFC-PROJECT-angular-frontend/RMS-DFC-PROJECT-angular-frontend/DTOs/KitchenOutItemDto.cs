namespace RestaurantSystem.DTOs
{
    public class KitchenOutItemDto
    {
        public int RawItemId { get; set; }
        public decimal Quantity { get; set; }
    }
    public class KitchenInventoryDto
    {
        public int RawItemId { get; set; }
        public string RawItemName { get; set; } = "";
        public decimal Quantity { get; set; }
    }
}
