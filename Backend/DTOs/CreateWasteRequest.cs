namespace RestaurantSystem.DTOs
{
    public class CreateWasteRequest
    {
        public string ReferenceNo { get; set; }
        public string? Reason { get; set; }

        public List<WasteItemRequest> Items { get; set; }
    }
    public class WasteItemRequest
    {
        public int RawItemId { get; set; }
        public decimal Quantity { get; set; }
    }
}
