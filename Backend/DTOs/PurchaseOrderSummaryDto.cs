namespace RestaurantSystem.DTOs
{
    public class PurchaseOrderSummaryDto
    {
        public string BillNo { get; set; }
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
