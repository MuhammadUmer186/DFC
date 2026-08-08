namespace RestaurantSystem.DTOs
{
    public class OrderDealDto
    {
        public int DealId { get; set; }
        public string DealName { get; set; }
        public decimal DealPrice { get; set; }
        public int Quantity { get; set; }

        public List<DealMenuItemDto> Items { get; set; }
    }


}
