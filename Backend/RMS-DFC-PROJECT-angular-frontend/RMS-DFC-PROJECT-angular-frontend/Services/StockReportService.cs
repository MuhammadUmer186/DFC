using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using System;

namespace RestaurantSystem.Services
{
    public class StockReportService : IStockReportService
    {
        private readonly ApplicationDbContext _context;

        public StockReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===============================
        // 1️⃣ STOCK SUMMARY (ALL VENDORS)
        // ===============================
        public async Task<List<StockSummaryDto>> GetStockSummaryAsync()
        {
            return await _context.StoreStocks
                .Include(s => s.RawItem)
                .GroupBy(s => new
                {
                    s.RawItemId,
                    s.RawItem.Name,
                    s.RawItem.Unit
                })
                .Select(g => new StockSummaryDto
                {
                    RawItemId = g.Key.RawItemId,
                    ItemName = g.Key.Name,
                    Unit = g.Key.Unit,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .OrderBy(x => x.ItemName)
                .ToListAsync();
        }

        // ===============================
        // 2️⃣ VENDOR-WISE STOCK
        // ===============================
        public async Task<List<VendorStockDto>> GetVendorWiseStockAsync()
        {
            return await _context.StoreStocks
                .Include(s => s.Vendor)
                .Include(s => s.RawItem)
                .Select(s => new VendorStockDto
                {
                    VendorId = s.VendorId,
                    VendorName = s.Vendor.Name,
                    RawItemId = s.RawItemId,
                    ItemName = s.RawItem.Name,
                    Unit = s.RawItem.Unit,
                    Quantity = s.Quantity
                })
                .OrderBy(x => x.VendorName)
                .ThenBy(x => x.ItemName)
                .ToListAsync();
        }
    }

}
