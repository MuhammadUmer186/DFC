namespace RestaurantSystem.DTOs
{
    public class VendorStockDto
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public int RawItemId { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public decimal Quantity { get; set; }
    }


}
