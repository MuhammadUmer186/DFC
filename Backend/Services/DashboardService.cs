using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Helpers;
using System;

namespace RestaurantSystem.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===========================
        // SALES SUMMARY
        // ===========================
        public async Task<DashboardSummaryDto> GetSalesSummaryAsync()
        {
            // Business "today" (03:00 AM cutoff)
            var today = BusinessDayHelper.GetBusinessToday();

            var todayStart = BusinessDayHelper.GetStart(today);
            var todayEnd = BusinessDayHelper.GetEnd(today);

            var weekStart = today.AddDays(-6); // last 7 business days
            var monthStart = new DateOnly(today.Year, today.Month, 1);

            return new DashboardSummaryDto
            {
                TodayTotal = await _context.Orders
                    .Where(o => o.CreatedAt >= todayStart && o.CreatedAt < todayEnd)
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0,

                WeeklyTotal = await _context.Orders
                    .Where(o =>
                        o.CreatedAt >= BusinessDayHelper.GetStart(weekStart) &&
                        o.CreatedAt < todayEnd)
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0,

                MonthlyTotal = await _context.Orders
                    .Where(o =>
                        o.CreatedAt >= BusinessDayHelper.GetStart(monthStart) &&
                        o.CreatedAt < todayEnd)
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0
            };
        }

        // ===========================
        // SALES DATE RANGE
        // ===========================

        public async Task<List<DateAmountDto>> GetSalesByDateRangeAsync(DateOnly from, DateOnly to)
        {
            var start = BusinessDayHelper.GetStart(from);
            var end = BusinessDayHelper.GetEnd(to);

            // Fetch all orders in the range
            var ordersInRange = await _context.Orders
                .Where(o => o.CreatedAt >= start && o.CreatedAt < end)
                .ToListAsync();

            // Group by business day
            var grouped = ordersInRange
                .GroupBy(o => BusinessDayHelper.GetBusinessToday(o.CreatedAt))
                .Select(g => new DateAmountDto
                {
                    Date = g.Key.ToDateTime(TimeOnly.MinValue),
                    TotalAmount = g.Sum(x => x.TotalAmount)
                })
                .OrderBy(x => x.Date)
                .ToList();

            return grouped;
        }


        // ===========================
        // PURCHASE SUMMARY
        // ===========================

        public async Task<DashboardSummaryDto> GetPurchaseSummaryAsync()
        {
            // Business "today" (03:00 AM cutoff)
            var today = BusinessDayHelper.GetBusinessToday();
            var todayStart = BusinessDayHelper.GetStart(today);
            var todayEnd = BusinessDayHelper.GetEnd(today);

            // Last 7 business days
            var weekStart = today.AddDays(-6);
            var weekStartStart = BusinessDayHelper.GetStart(weekStart);

            // Start of the month
            var monthStart = new DateOnly(today.Year, today.Month, 1);
            var monthStartStart = BusinessDayHelper.GetStart(monthStart);

            return new DashboardSummaryDto
            {
                TodayTotal = await _context.PurchaseOrders
                    .Where(p => p.PurchaseDate >= todayStart && p.PurchaseDate < todayEnd)
                    .SumAsync(p => (decimal?)p.TotalAmount) ?? 0,

                WeeklyTotal = await _context.PurchaseOrders
                    .Where(p => p.PurchaseDate >= weekStartStart && p.PurchaseDate < todayEnd)
                    .SumAsync(p => (decimal?)p.TotalAmount) ?? 0,

                MonthlyTotal = await _context.PurchaseOrders
                    .Where(p => p.PurchaseDate >= monthStartStart && p.PurchaseDate < todayEnd)
                    .SumAsync(p => (decimal?)p.TotalAmount) ?? 0
            };
        }

        // ===========================
        // PURCHASE DATE RANGE
        // ===========================

        public async Task<List<DateAmountDto>> GetPurchasesByDateRangeAsync(DateOnly from, DateOnly to)
        {
            var start = BusinessDayHelper.GetStart(from);
            var end = BusinessDayHelper.GetEnd(to);

            // Fetch data first
            var purchases = await _context.PurchaseOrders
                .Where(p => p.PurchaseDate >= start && p.PurchaseDate < end)
                .ToListAsync(); // now we are in memory

            // Group by business day
            var grouped = purchases
                .GroupBy(p => BusinessDayHelper.GetBusinessToday(p.PurchaseDate))
                .Select(g => new DateAmountDto
                {
                    Date = g.Key.ToDateTime(TimeOnly.MinValue),
                    TotalAmount = g.Sum(x => x.TotalAmount)
                })
                .OrderBy(x => x.Date)
                .ToList();

            return grouped;
        }


        public async Task<DashboardMainSummaryDto> GetMainSummaryAsync()
    {
        var today = BusinessDayHelper.GetBusinessToday();
        var todayStart = BusinessDayHelper.GetStart(today);
        var todayEnd = BusinessDayHelper.GetEnd(today);

        return new DashboardMainSummaryDto
        {
            TotalCategories = await _context.Categories.CountAsync(),
            TotalMenuItems = await _context.MenuItems.CountAsync(),
            TotalVendors = await _context.Vendors.CountAsync(),
            TotalRawItems = await _context.RawItems.CountAsync(),

            TodaySales = await _context.Orders
                .Where(o => o.CreatedAt >= todayStart && o.CreatedAt < todayEnd)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0,

            TodayOnlineSales = await _context.Orders
                .Where(o => o.CreatedAt >= todayStart && o.CreatedAt < todayEnd && o.OrderSource == "Online")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0,

            TodaySiteSales = await _context.Orders
                .Where(o => o.CreatedAt >= todayStart && o.CreatedAt < todayEnd && o.OrderSource != "Online")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0,

            TodayPurchases = await _context.PurchaseOrders
                .Where(p => p.PurchaseDate >= todayStart && p.PurchaseDate < todayEnd)
                .SumAsync(p => (decimal?)p.TotalAmount) ?? 0

            //CurrentStockValue = await _context.StoreStocks
            //    .SumAsync(s => s.Quantity * s.AveragePrice)
        };
    }

    public async Task<List<StockSummaryDto>> GetStockSummaryAsync()
        {
            return await _context.StoreStocks
                .Include(s => s.RawItem)
                .GroupBy(s => new { s.RawItemId, s.RawItem.Name, s.RawItem.Unit })
                .Select(g => new StockSummaryDto
                {
                    RawItemId = g.Key.RawItemId,
                    ItemName = g.Key.Name,
                    Unit = g.Key.Unit,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .ToListAsync();
        }
        public async Task<List<VendorStockDto>> GetVendorStockAsync()
        {
            return await _context.StoreStocks
                .Include(s => s.RawItem)
                .Include(s => s.Vendor)
                .Select(s => new VendorStockDto
                {
                    VendorId = s.VendorId,
                    VendorName = s.Vendor.Name,
                    RawItemId = s.RawItemId,
                    ItemName = s.RawItem.Name,
                    Unit = s.RawItem.Unit,
                    Quantity = s.Quantity
                })
                .ToListAsync();
        }

public async Task<ProfitDto> GetProfitAsync(DateOnly from, DateOnly to)
    {
        var start = BusinessDayHelper.GetStart(from);
        var end = BusinessDayHelper.GetEnd(to);

        var sales = await _context.Orders
            .Where(o => o.CreatedAt >= start && o.CreatedAt < end)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        var purchases = await _context.PurchaseOrders
            .Where(p => p.PurchaseDate >= start && p.PurchaseDate < end)
            .SumAsync(p => (decimal?)p.TotalAmount) ?? 0;

        return new ProfitDto
        {
            TotalSales = sales,
            TotalPurchases = purchases
        };
    }

        public async Task<List<ProfitDto>> GetDailyProfitAsync(DateOnly from, DateOnly to)
        {
            var start = BusinessDayHelper.GetStart(from);
            var end = BusinessDayHelper.GetEnd(to);

            // Fetch sales/purchases in range first
            var sales = await _context.Orders
                .Where(o => o.CreatedAt >= start && o.CreatedAt < end)
                .ToListAsync();

            var purchases = await _context.PurchaseOrders
                .Where(p => p.PurchaseDate >= start && p.PurchaseDate < end)
                .ToListAsync();

            var result = new List<ProfitDto>();

            for (var date = from; date <= to; date = date.AddDays(1))
            {
                var sale = sales
                    .Where(x => BusinessDayHelper.GetBusinessToday(x.CreatedAt) == date)
                    .Sum(x => x.TotalAmount);

                var purchase = purchases
                    .Where(x => BusinessDayHelper.GetBusinessToday(x.PurchaseDate) == date)
                    .Sum(x => x.TotalAmount);

                result.Add(new ProfitDto
                {
                    Date = date,
                    TotalSales = sale,
                    TotalPurchases = purchase
                });
            }

            return result.OrderBy(x => x.Date).ToList();
        }

public async Task<ProfitSummaryDto> GetProfitSummaryAsync()
    {
        // Use business-day helper
        var today = BusinessDayHelper.GetBusinessToday();
        var weekStart = today.AddDays(-6); // last 7 business days
        var monthStart = new DateOnly(today.Year, today.Month, 1);

        // Today
        var todayProfit = await GetProfitAsync(today, today);

        // Weekly
        var weeklyProfit = await GetProfitAsync(weekStart, today);

        // Monthly
        var monthlyProfit = await GetProfitAsync(monthStart, today);

        return new ProfitSummaryDto
        {
            TodayProfit = todayProfit.TotalSales - todayProfit.TotalPurchases,
            WeeklyProfit = weeklyProfit.TotalSales - weeklyProfit.TotalPurchases,
            MonthlyProfit = monthlyProfit.TotalSales - monthlyProfit.TotalPurchases
        };
    }

    public async Task<List<VendorAccountSummaryDto>> GetVendorPaymentSummaryAsync()
        {
            var data = await _context.Vendors
                .Select(v => new VendorAccountSummaryDto
                {
                    VendorId = v.Id,
                    VendorName = v.Name,
                    TotalPurchased = _context.PurchaseOrders
                            .Where(p => p.VendorId == v.Id)
                            .Sum(p => (decimal?)p.TotalAmount) ?? 0,

                    TotalPaid = _context.VendorPayments
                            .Where(p => p.VendorId == v.Id)
                            .Sum(p => (decimal?)p.AmountPaid) ?? 0
                })
                .ToListAsync();

            return data;
        }

        public async Task<List<CategorySalesDto>> GetTopSellingCategoriesAsync()
        {
            return await _context.OrderItems
                .Include(o => o.MenuItem)
                .ThenInclude(m => m.Category)
                .GroupBy(o => o.MenuItem.Category.Name)
                .Select(g => new CategorySalesDto
                {
                    CategoryName = g.Key,
                    TotalOrders = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalOrders)
                .ToListAsync();
        }

public async Task<OrderCountSummaryDto> GetOrderCountSummaryAsync()
    {
        var today = BusinessDayHelper.GetBusinessToday();
        var todayStart = BusinessDayHelper.GetStart(today);
        var todayEnd = BusinessDayHelper.GetEnd(today);

        var weekStart = today.AddDays(-6); // last 7 business days
        var weekStartStart = BusinessDayHelper.GetStart(weekStart);

        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var monthStartStart = BusinessDayHelper.GetStart(monthStart);

        return new OrderCountSummaryDto
        {
            TodayOrders = await _context.Orders
                .CountAsync(o => o.CreatedAt >= todayStart && o.CreatedAt < todayEnd),

            WeeklyOrders = await _context.Orders
                .CountAsync(o => o.CreatedAt >= weekStartStart && o.CreatedAt < todayEnd),

            MonthlyOrders = await _context.Orders
                .CountAsync(o => o.CreatedAt >= monthStartStart && o.CreatedAt < todayEnd),

            TodayOnlineOrders = await _context.Orders
                .CountAsync(o => o.CreatedAt >= todayStart && o.CreatedAt < todayEnd && o.OrderSource == "Online"),
            TodaySiteOrders = await _context.Orders
                .CountAsync(o => o.CreatedAt >= todayStart && o.CreatedAt < todayEnd && o.OrderSource != "Online"),

            WeeklyOnlineOrders = await _context.Orders
                .CountAsync(o => o.CreatedAt >= weekStartStart && o.CreatedAt < todayEnd && o.OrderSource == "Online"),
            WeeklySiteOrders = await _context.Orders
                .CountAsync(o => o.CreatedAt >= weekStartStart && o.CreatedAt < todayEnd && o.OrderSource != "Online"),

            MonthlyOnlineOrders = await _context.Orders
                .CountAsync(o => o.CreatedAt >= monthStartStart && o.CreatedAt < todayEnd && o.OrderSource == "Online"),
            MonthlySiteOrders = await _context.Orders
                .CountAsync(o => o.CreatedAt >= monthStartStart && o.CreatedAt < todayEnd && o.OrderSource != "Online")
        };
    }

        public async Task<StockUsagePercentageDto> GetStockUsagePercentageAsync()
        {
            var kitchenOut = await _context.KitchenOutItems
                .SumAsync(k => (decimal?)k.Quantity) ?? 0;

            var wasteOut = await _context.WasteItems
                .SumAsync(w => (decimal?)w.Quantity) ?? 0;

            var total = kitchenOut + wasteOut;

            if (total == 0)
            {
                return new StockUsagePercentageDto
                {
                    KitchenOutPercentage = 0,
                    WastePercentage = 0
                };
            }

            return new StockUsagePercentageDto
            {
                KitchenOutPercentage = Math.Round((kitchenOut / total) * 100, 2),
                WastePercentage = Math.Round((wasteOut / total) * 100, 2)
            };
        }

        public async Task<DashboardDto> GetUtilities()
        {
            var dto = new DashboardDto();

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            dto.MonthlyUtilityBills = await _context.UtilityBills
                .Where(x => x.BillDate.Month == currentMonth && x.BillDate.Year == currentYear)
                .SumAsync(x => x.Amount);

            return dto;
        }

    }


}
