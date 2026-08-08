namespace RestaurantSystem.DTOs
{
    public class DealItemDto
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
    }
    public class CreateDealDto
    {
        public string DealName { get; set; }
        public decimal DiscountAmount { get; set; }

        public List<DealItemDto> Items { get; set; }
    }
    public class DealResponseDto
    {
        public int Id { get; set; }
        public string DealName { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalPrice { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }

        public List<DealMenuItemDto> Items { get; set; }
    }
    public class DealMenuItemDto
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }   // unit price of item (for display only)
        public int Quantity { get; set; }
    }




}
