using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using System;

namespace RestaurantSystem.Services
{
    public class MenuProfitService : IMenuProfitService
    {
        private readonly ApplicationDbContext _context;

        public MenuProfitService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MenuProfitDto>> GetMenuProfitAsync(DateTime from, DateTime to)
        {
            var orderItems = await _context.OrderItems
                .Include(o => o.MenuItem)
                .Where(o => o.Order.CreatedAt >= from &&
                            o.Order.CreatedAt <= to)
                .ToListAsync();

            var recipes = await _context.MenuRecipes
                .Include(r => r.RawItem)
                .ToListAsync();

            var stockRates = await _context.PurchaseOrderItems
                .GroupBy(p => p.RawItemId)
                .Select(g => new
                {
                    RawItemId = g.Key,
                    AvgRate = g.Average(x => x.UnitPrice)
                })
                .ToListAsync();

            var result = orderItems
                .GroupBy(o => o.MenuItem.Name)
                .Select(g =>
                {
                    decimal cost = 0;

                    foreach (var recipe in recipes.Where(r => r.MenuItem.Name == g.Key))
                    {
                        var rate = stockRates
                            .FirstOrDefault(r => r.RawItemId == recipe.RawItemId)?.AvgRate ?? 0;

                        cost += recipe.QuantityRequired * rate * g.Sum(x => x.Quantity);
                    }

                    return new MenuProfitDto
                    {
                        MenuName = g.Key,
                        SalesAmount = g.Sum(x => x.Quantity * x.UnitPrice),
                        CostAmount = cost
                    };
                })
                .ToList();

            return result;
        }
    }

}
