namespace RestaurantSystem.DTOs
{
    public class PurchaseOrderCreateDto
    {
        public string BillNo { get; set; }
        public int VendorId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public List<PurchaseOrderItemDto> Items { get; set; }
    }
}
