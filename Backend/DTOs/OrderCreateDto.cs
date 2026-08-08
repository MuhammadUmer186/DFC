namespace RestaurantSystem.DTOs
{
    public class OrderCreateDto
    {
        public List<OrderItemCreateDto> Items { get; set; } = new();
    }
    public class OrderListDto
    {
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
