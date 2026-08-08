namespace RestaurantSystem.DTOs
{
    public class VendorAccountSummaryDto
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; }

        public decimal TotalPurchased { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal UnPaid => TotalPurchased - TotalPaid;

        public List<PurchaseOrderSummaryDto> PurchaseOrders { get; set; }
        public List<VendorPaymentDto> Payments { get; set; }
    }

}
