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

    }

}

