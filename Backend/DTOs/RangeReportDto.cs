using Microsoft.EntityFrameworkCore;

namespace RestaurantSystem.DTOs
{
    public class RangeReportDto
    {
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }

        // SUMMARY
        [Precision(18, 2)]
        public decimal Sales { get; set; }
        [Precision(18, 2)]
        public decimal PurchaseOrdersCost { get; set; }
        [Precision(18, 2)]
        public decimal KitchenCost { get; set; }
        [Precision(18, 2)]
        public decimal WasteCost { get; set; }
        [Precision(18, 2)]
        public decimal SalaryPaid { get; set; }
        [Precision(18, 2)]
        public decimal VendorPayments { get; set; }
        [Precision(18, 2)]
        public decimal Profit { get; set; }

        // HISTORY
        public List<OrderDto> Orders { get; set; } = new();
        public List<PurchaseOrderListDto> PurchaseOrders { get; set; } = new();
        public List<KitchenOutListDto> KitchenOuts { get; set; } = new();
        public List<WasteRecordDto> Wastes { get; set; } = new();
    }
}
