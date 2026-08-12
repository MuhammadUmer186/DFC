namespace RestaurantSystem.DTOs
{
    public class PrintReceiptDto
    {
        public string CopyType { get; set; } = "customer";// customer | kitchen
        public int OrderNo { get; set; }
        public decimal Total { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalTotal { get; set; }
        public List<PrintItemDto> Items { get; set; } = [];

        // Restaurant-local time to print on the slip — the printing server's own OS clock may run
        // in a different zone (e.g. UTC), so callers must resolve this via IRestaurantClock rather
        // than leaving the builder to call DateTime.Now itself.
        public DateTime PrintedAt { get; set; }

        // Delivery/online order extras — null for regular POS receipts (no change in output)
        public string? OrderTypeLabel { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
    }

    public class PrintItemDto
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

}
