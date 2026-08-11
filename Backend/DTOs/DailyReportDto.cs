using Microsoft.EntityFrameworkCore;

namespace RestaurantSystem.DTOs
{
    public class DailyReportDto
    {
        public DateOnly Date { get; set; }

        // SUMMARY
        [Precision(18, 2)]
        public decimal Sales { get; set; }
        [Precision(18, 2)]
        public decimal OnlineSales { get; set; }
        [Precision(18, 2)]
        public decimal SiteSales { get; set; }
        public int OnlineOrderCount { get; set; }
        public int SiteOrderCount { get; set; }
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
