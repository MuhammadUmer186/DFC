namespace RestaurantSystem.DTOs
{
    public class OrderItemDto
    {
        public int MenuItemId { get; set; }
        public string? MenuItemName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        public decimal LineTotal => UnitPrice * Quantity;  
}
    

}
