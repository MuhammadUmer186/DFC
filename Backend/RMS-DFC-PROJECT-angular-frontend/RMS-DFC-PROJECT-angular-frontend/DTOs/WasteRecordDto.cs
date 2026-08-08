namespace RestaurantSystem.DTOs
{
    public class WasteRecordDto
    {
        public int Id { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Reason { get; set; }

        public List<WasteRecordItemDto> Items { get; set; }
    }
    public class WasteResponseDto
    {
        public int Id { get; set; }
        public DateTime WasteDate { get; set; }
        public string Reason { get; set; }

        public List<WasteItemDetailDto> Items { get; set; }
    }

    public class WasteItemDetailDto
    {
        public int RawItemId { get; set; }
        public string RawItemName { get; set; }
        public decimal Quantity { get; set; }
    }

    public class WasteItemDto
    {
        public int RawItemId { get; set; }
        public decimal Quantity { get; set; }
    }


    public class WasteRecordItemDto
    {
        public int RawItemId { get; set; }
        public string RawItemName { get; set; }
        public decimal Quantity { get; set; }
    }
    public class WasteCreateRequestDto
    {
        public string Reason { get; set; }

        public List<WasteItemDto> Items { get; set; }
    }


}
