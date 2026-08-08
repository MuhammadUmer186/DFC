using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using System;

namespace RestaurantSystem.Services
{
    public class ProfitService : IProfitService
    {
        private readonly ApplicationDbContext _context;

        public ProfitService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProfitDto> GetProfitAsync(DateTime from, DateTime to)
        {
            var sales = await _context.Orders
                .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            var purchases = await _context.PurchaseOrders
                .Where(p => p.PurchaseDate >= from && p.PurchaseDate <= to)
                .SumAsync(p => (decimal?)p.TotalAmount) ?? 0;

            return new ProfitDto
            {
                TotalSales = sales,
                TotalPurchases = purchases
            };
        }
    }

}
