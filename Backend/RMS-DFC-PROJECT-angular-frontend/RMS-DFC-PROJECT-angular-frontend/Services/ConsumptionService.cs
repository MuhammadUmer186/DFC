using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using System;

namespace RestaurantSystem.Services
{
    public class ConsumptionService : IConsumptionService
    {
        private readonly ApplicationDbContext _context;

        public ConsumptionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ItemConsumptionDto>> GetConsumptionAsync(DateTime from, DateTime to)
        {
            return await _context.KitchenOutItems
                .Include(i => i.RawItem)
                .Include(i => i.KitchenOut)
                .Where(i => i.KitchenOut.IssuedAt >= from &&
                            i.KitchenOut.IssuedAt <= to)
                .GroupBy(i => new { i.RawItem.Name, i.RawItem.Unit })
                .Select(g => new ItemConsumptionDto
                {
                    ItemName = g.Key.Name,
                    Unit = g.Key.Unit,
                    TotalConsumed = g.Sum(x => x.Quantity)
                })
                .ToListAsync();
        }
    }

}
