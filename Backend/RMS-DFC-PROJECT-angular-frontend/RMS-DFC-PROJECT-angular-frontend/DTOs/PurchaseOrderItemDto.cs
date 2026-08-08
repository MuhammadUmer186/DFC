namespace RestaurantSystem.DTOs
{
    public class PurchaseOrderItemDto
    {
        public int RawItemId { get; set; }
        public string? RawItemName { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
    public class PurchaseOrderListDto
    {
        public int Id { get; set; }
        public string BillNo { get; set; }
        public DateTime PurchaseDate { get; set; }

        public int VendorId { get; set; }
        public string VendorName { get; set; }

        public decimal TotalAmount { get; set; }

        public List<PurchaseOrderItemDto> Items { get; set; }
    }
}
