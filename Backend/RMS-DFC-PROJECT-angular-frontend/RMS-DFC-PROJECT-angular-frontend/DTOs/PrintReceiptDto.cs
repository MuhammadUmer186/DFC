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
    }

    public class PrintItemDto
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

}
