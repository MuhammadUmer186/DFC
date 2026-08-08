namespace RestaurantSystem.DTOs
{
    public class PublicMenuDto
    {
        public List<PublicCategoryDto> Categories { get; set; }
        public List<PublicDealDto> Deals { get; set; }
    }
    public class PublicCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<PublicMenuItemDto> Items { get; set; }
    }
    public class PublicMenuItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
    }
    public class PublicDealDto
    {
        public int Id { get; set; }
        public string DealName { get; set; }
        public decimal Price { get; set; }

        public List<PublicDealItemDto> Items { get; set; }
    }
    public class PublicDealItemDto
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
    }
}
