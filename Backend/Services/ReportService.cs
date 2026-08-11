using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Helpers;
using RestaurantSystem.Models;

namespace RestaurantSystem.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IKitchenOutService _kitchenOutService;
        private readonly IOrderService _orderService;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IWasteService _wasteService;

        public ReportService(
            ApplicationDbContext context,
            IKitchenOutService kitchenOutService,
            IOrderService orderService,
            IPurchaseOrderService purchaseOrderService,
            IWasteService wasteService)
        {
            _context = context;
            _kitchenOutService = kitchenOutService;
            _orderService = orderService;
            _purchaseOrderService = purchaseOrderService;
            _wasteService = wasteService;
        }

        // 🔹 PROFIT REPORT
        public async Task<ProfitReportResponseDto> GetProfitReportAsync(DateTime from, DateTime to)
        {
            var totalSales = await _context.Orders
                .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
                .SumAsync(o => o.TotalAmount);

            var totalSalaries = await _context.SalaryPayments
                .Where(s => s.PaidAt >= from && s.PaidAt <= to)
                .SumAsync(s => s.AmountPaid);

            return new ProfitReportResponseDto
            {
                TotalSales = totalSales,
                TotalSalaries = totalSalaries,
                Profit = totalSales - totalSalaries
            };
        }

        // 🔹 DAILY SALARY PAID DETAIL
        public async Task<List<DailySalaryReportDto>> GetDailySalaryReportAsync(DateOnly date)
        {
            return await _context.SalaryPayments
                .Where(s =>
                    (s.SalaryType == SalaryType.Daily && s.ForDate == date) ||
                    (s.SalaryType == SalaryType.Monthly &&
                     s.PaidAt.Date == date.ToDateTime(TimeOnly.MinValue).Date)
                )
                .Include(s => s.Employee)
                .Select(s => new DailySalaryReportDto
                {
                    EmployeeId = s.EmployeeId,
                    EmployeeName = s.Employee.Name,
                    SalaryType = s.SalaryType,
                    AmountPaid = s.AmountPaid,
                    PaidAt = s.PaidAt
                })
                .ToListAsync();
        }

        public async Task<(decimal totalPaid, decimal totalUnpaid)> GetSalaryTotalsAsync(DateOnly from, DateOnly? to = null)
        {
            var endDate = to ?? from; // If "to" not provided, use single date

            // Total paid salaries
            var totalPaid = await _context.SalaryPayments
                .Where(s =>
                    s.ForDate >= from &&
                    s.ForDate <= endDate &&
                    s.SalaryType == SalaryType.Daily &&
                    s.AmountPaid > 0)
                .SumAsync(s => s.AmountPaid);

            // Total unpaid salaries: sum expected salary for employees who have not yet received salary
            var unpaidPayments = await _context.Employees
                .Where(e => !_context.SalaryPayments
                    .Any(sp => sp.EmployeeId == e.Id &&
                               sp.ForDate >= from &&
                               sp.ForDate <= endDate &&
                               sp.SalaryType == SalaryType.Daily &&
                               sp.AmountPaid > 0))
                .SumAsync(e => e.SalaryAmount);

            return (totalPaid, unpaidPayments);
        }



        public async Task<DailyReportDto> GetDailyReportAsync(DateOnly date)
        {
            var start = BusinessDayHelper.GetStart(date);
            var end = BusinessDayHelper.GetEnd(date);

            var report = new DailyReportDto();
            report.Date = date;

            // ================= SALES =================
            report.Sales = await _context.Orders
                .Where(o => o.CreatedAt >= start && o.CreatedAt < end && o.Status == OrderStatus.Paid)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            report.OnlineSales = await _context.Orders
                .Where(o => o.CreatedAt >= start && o.CreatedAt < end && o.Status == OrderStatus.Paid && o.OrderSource == "Online")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
            report.SiteSales = report.Sales - report.OnlineSales;

            report.OnlineOrderCount = await _context.Orders
                .CountAsync(o => o.CreatedAt >= start && o.CreatedAt < end && o.Status == OrderStatus.Paid && o.OrderSource == "Online");
            report.SiteOrderCount = await _context.Orders
                .CountAsync(o => o.CreatedAt >= start && o.CreatedAt < end && o.Status == OrderStatus.Paid && o.OrderSource != "Online");

            // ================= PURCHASE ORDERS =================
            report.PurchaseOrdersCost = await _context.PurchaseOrders
                .Where(p => p.PurchaseDate >= start && p.PurchaseDate <= end)
                .SumAsync(p => (decimal?)p.TotalAmount) ?? 0;

            // ================= KITCHEN COST =================
            var kitchenCost = await _kitchenOutService.GetDailyKitchenCostAsync(date);
            report.KitchenCost = kitchenCost.TotalKitchenCost;
            report.WasteCost = kitchenCost.TotalWasteCost;

            // ================= SALARY =================
            // Daily salaries for the date
            var dailySalaries = await _context.SalaryPayments
                .Where(s => s.SalaryType == SalaryType.Daily && s.ForDate == date)
                .SumAsync(s => (decimal?)s.AmountPaid) ?? 0;

            // Monthly salaries for the month of the date
            string month = date.ToString("yyyy-MM");
            var monthlySalaries = await _context.SalaryPayments
                .Where(s => s.SalaryType == SalaryType.Monthly && s.ForMonth == month)
                .SumAsync(s => (decimal?)s.AmountPaid) ?? 0;

            report.SalaryPaid = dailySalaries + monthlySalaries;

            // ================= VENDOR PAYMENTS =================
            report.VendorPayments = await _context.VendorPayments
                .Where(v => v.PaidOn >= start && v.PaidOn <= end)
                .SumAsync(v => (decimal?)v.AmountPaid) ?? 0;

            // ================= PROFIT =================
            report.Profit =
                report.Sales
                - report.KitchenCost
                - report.WasteCost
                - report.SalaryPaid
                - report.VendorPayments;

            // ================= ORDER HISTORY =================
            report.Orders = await _orderService.GetByDateAsync(date);

            // ================= PURCHASE ORDERS HISTORY =================
            report.PurchaseOrders = await _purchaseOrderService.GetByDateAsync(date);

            // ================= KITCHEN OUT HISTORY =================
            report.KitchenOuts = await _context.KitchenOuts
                .Include(k => k.KitchenOutItems)
                .ThenInclude(i => i.RawItem)
                .Where(k => k.IssuedAt >= start && k.IssuedAt <= end)
                .Select(k => new KitchenOutListDto
                {
                    Id = k.Id,
                    IssuedAt = k.IssuedAt,
                    Items = k.KitchenOutItems.Select(i => new KitchenOutListItemDto
                    {
                        RawItemId = i.RawItemId,
                        RawItemName = i.RawItem != null ? i.RawItem.Name : "N/A",
                        Quantity = i.Quantity
                    }).ToList()
                })
                .ToListAsync();

            // ================= WASTES =================
            report.Wastes = await _wasteService.GetByDateAsync(date);

            return report;
        }

        // 🔹 RANGE REPORT (Today / Yesterday / This Week / This Month / This Year / Overall / Custom)
        public async Task<RangeReportDto> GetReportByRangeAsync(DateOnly from, DateOnly to)
        {
            var start = BusinessDayHelper.GetStart(from);
            var end = BusinessDayHelper.GetEnd(to);

            var report = new RangeReportDto { From = from, To = to };

            // ================= SALES =================
            report.Sales = await _context.Orders
                .Where(o => o.CreatedAt >= start && o.CreatedAt < end && o.Status == OrderStatus.Paid)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            report.OnlineSales = await _context.Orders
                .Where(o => o.CreatedAt >= start && o.CreatedAt < end && o.Status == OrderStatus.Paid && o.OrderSource == "Online")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
            report.SiteSales = report.Sales - report.OnlineSales;

            report.OnlineOrderCount = await _context.Orders
                .CountAsync(o => o.CreatedAt >= start && o.CreatedAt < end && o.Status == OrderStatus.Paid && o.OrderSource == "Online");
            report.SiteOrderCount = await _context.Orders
                .CountAsync(o => o.CreatedAt >= start && o.CreatedAt < end && o.Status == OrderStatus.Paid && o.OrderSource != "Online");

            // ================= PURCHASE ORDERS =================
            report.PurchaseOrdersCost = await _context.PurchaseOrders
                .Where(p => p.PurchaseDate >= start && p.PurchaseDate < end)
                .SumAsync(p => (decimal?)p.TotalAmount) ?? 0;

            // ================= KITCHEN COST & WASTE COST =================
            var avgPrices = await _kitchenOutService.GetAverageCostsAsync();

            var kitchenOutsForCost = await _context.KitchenOuts
                .Include(k => k.KitchenOutItems)
                .Where(k => k.IssuedAt >= start && k.IssuedAt < end)
                .ToListAsync();

            decimal kitchenCost = 0;
            foreach (var k in kitchenOutsForCost)
                foreach (var item in k.KitchenOutItems)
                {
                    var avg = avgPrices.FirstOrDefault(a => a.RawItemId == item.RawItemId);
                    if (avg != null) kitchenCost += item.Quantity * avg.AverageCost;
                }
            report.KitchenCost = kitchenCost;

            var wastesForCost = await _context.WasteRecords
                .Include(w => w.WasteItems)
                .Where(w => w.WasteDate >= start && w.WasteDate < end)
                .ToListAsync();

            decimal wasteCost = 0;
            foreach (var w in wastesForCost)
                foreach (var item in w.WasteItems)
                {
                    var avg = avgPrices.FirstOrDefault(a => a.RawItemId == item.RawItemId);
                    if (avg != null) wasteCost += item.Quantity * avg.AverageCost;
                }
            report.WasteCost = wasteCost;

            // ================= SALARY =================
            var dailySalaries = await _context.SalaryPayments
                .Where(s => s.SalaryType == SalaryType.Daily && s.ForDate >= from && s.ForDate <= to)
                .SumAsync(s => (decimal?)s.AmountPaid) ?? 0;

            var months = new HashSet<string>();
            for (var d = from; d <= to; d = d.AddDays(1))
                months.Add(d.ToString("yyyy-MM"));

            var monthlySalaries = await _context.SalaryPayments
                .Where(s => s.SalaryType == SalaryType.Monthly && months.Contains(s.ForMonth!))
                .SumAsync(s => (decimal?)s.AmountPaid) ?? 0;

            report.SalaryPaid = dailySalaries + monthlySalaries;

            // ================= VENDOR PAYMENTS =================
            report.VendorPayments = await _context.VendorPayments
                .Where(v => v.PaidOn >= start && v.PaidOn < end)
                .SumAsync(v => (decimal?)v.AmountPaid) ?? 0;

            // ================= PROFIT =================
            report.Profit =
                report.Sales
                - report.KitchenCost
                - report.WasteCost
                - report.SalaryPaid
                - report.VendorPayments;

            // ================= ORDER HISTORY =================
            report.Orders = await _context.Orders
                .Where(o => o.CreatedAt >= start && o.CreatedAt < end)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    CreatedAt = o.CreatedAt,
                    TotalAmount = o.TotalAmount,
                    Paid = o.Paid,
                    Status = o.Status,
                    TakenByEmployeeId = o.TakenByEmployeeId,
                    TakenByEmployeeName = o.TakenByEmployee != null ? o.TakenByEmployee.Name : null,
                    CashierId = o.CashierId,
                    CashierName = o.Cashier != null ? o.Cashier.Name : null,
                    CustomerName = o.CustomerName,
                    PhoneNumber = o.PhoneNumber,
                    Address = o.Address,
                    OrderSource = o.OrderSource,
                    Items = o.OrderItems.Select(i => new OrderItemDto
                    {
                        MenuItemId = i.MenuItemId,
                        MenuItemName = i.MenuItem.Name,
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity
                    }).ToList(),
                    Deals = o.OrderDeals.Select(d => new OrderDealDto
                    {
                        DealId = d.DealId,
                        DealName = d.Deal.DealName,
                        DealPrice = d.DealPrice,
                        Quantity = d.Quantity
                    }).ToList()
                })
                .ToListAsync();

            // ================= PURCHASE ORDERS HISTORY =================
            report.PurchaseOrders = await _context.PurchaseOrders
                .Where(p => p.PurchaseDate >= start && p.PurchaseDate < end)
                .Select(p => new PurchaseOrderListDto
                {
                    Id = p.Id,
                    BillNo = p.BillNo,
                    PurchaseDate = p.PurchaseDate,
                    VendorId = p.VendorId,
                    VendorName = p.Vendor != null ? p.Vendor.Name : "N/A",
                    TotalAmount = p.TotalAmount,
                    Items = p.PurchaseOrderItems.Select(i => new PurchaseOrderItemDto
                    {
                        RawItemId = i.RawItemId,
                        RawItemName = i.RawItem != null ? i.RawItem.Name : "N/A",
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity,
                        TotalPrice = i.TotalPrice
                    }).ToList()
                })
                .ToListAsync();

            // ================= KITCHEN OUT HISTORY =================
            report.KitchenOuts = await _context.KitchenOuts
                .Include(k => k.KitchenOutItems)
                .ThenInclude(i => i.RawItem)
                .Where(k => k.IssuedAt >= start && k.IssuedAt < end)
                .Select(k => new KitchenOutListDto
                {
                    Id = k.Id,
                    IssuedAt = k.IssuedAt,
                    Items = k.KitchenOutItems.Select(i => new KitchenOutListItemDto
                    {
                        RawItemId = i.RawItemId,
                        RawItemName = i.RawItem != null ? i.RawItem.Name : "N/A",
                        Quantity = i.Quantity
                    }).ToList()
                })
                .ToListAsync();

            // ================= WASTES =================
            report.Wastes = await _context.WasteRecords
                .Where(w => w.WasteDate >= start && w.WasteDate < end)
                .Select(w => new WasteRecordDto
                {
                    Id = w.Id,
                    CreatedAt = w.WasteDate
                })
                .ToListAsync();

            return report;
        }

    }

}

