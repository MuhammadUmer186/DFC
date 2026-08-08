namespace RestaurantSystem.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<MenuItemDto>? Items { get; set; }

    }
    public class CategorySalesDto
    {
        public string CategoryName { get; set; }
        public int TotalOrders { get; set; }
    }

}
