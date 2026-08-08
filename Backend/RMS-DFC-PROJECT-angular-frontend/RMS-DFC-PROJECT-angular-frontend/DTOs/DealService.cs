using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.Models;
using System;

namespace RestaurantSystem.DTOs
{
    public class DealService : IDealService
    {
        private readonly ApplicationDbContext _context;

        public DealService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DealResponseDto>> GetDealsAsync()
        {
            return await _context.Deals
                .Include(d => d.DealItems)
                .ThenInclude(i => i.MenuItem)
                .Select(d => new DealResponseDto
                {
                    Id = d.Id,
                    DealName = d.DealName,
                    OriginalPrice = d.OriginalPrice,
                    DiscountAmount = d.DiscountAmount,
                    FinalPrice = d.FinalPrice,

                    Items = d.DealItems.Select(x => new DealMenuItemDto
                    {
                        MenuItemId = x.MenuItemId,
                        Name = x.MenuItem.Name,
                        Price = x.MenuItem.Price,
                        Quantity = x.Quantity
                    }).ToList()

                }).ToListAsync();

        }

        public async Task<DealResponseDto> CreateDealAsync(CreateDealDto dto)
        {
            decimal originalPrice = 0;

            foreach (var i in dto.Items)
            {
                var menu = await _context.MenuItems.FindAsync(i.MenuItemId);
                originalPrice += (menu.Price * i.Quantity);
            }

            var deal = new Deal
            {
                DealName = dto.DealName,
                OriginalPrice = originalPrice,
                DiscountAmount = dto.DiscountAmount,
                FinalPrice = originalPrice - dto.DiscountAmount
            };

            _context.Deals.Add(deal);
            await _context.SaveChangesAsync();

            foreach (var i in dto.Items)
            {
                _context.DealItems.Add(new DealItem
                {
                    DealId = deal.Id,
                    MenuItemId = i.MenuItemId,
                    Quantity = i.Quantity
                });
            }

            await _context.SaveChangesAsync();

            return await GetDealsAsync().ContinueWith(t => t.Result.Last());
        }

        public async Task<DealResponseDto> UpdateDealAsync(int id, CreateDealDto dto)
        {
            var deal = await _context.Deals
                .Include(d => d.DealItems)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (deal == null) return null;

            decimal originalPrice = 0;

            foreach (var i in dto.Items)
            {
                var menu = await _context.MenuItems.FindAsync(i.MenuItemId);
                originalPrice += (menu.Price * i.Quantity);
            }

            deal.DealName = dto.DealName;
            deal.OriginalPrice = originalPrice;
            deal.DiscountAmount = dto.DiscountAmount;
            deal.FinalPrice = originalPrice - dto.DiscountAmount;

            _context.DealItems.RemoveRange(deal.DealItems);

            foreach (var i in dto.Items)
            {
                _context.DealItems.Add(new DealItem
                {
                    DealId = id,
                    MenuItemId = i.MenuItemId,
                    Quantity = i.Quantity
                });
            }

            await _context.SaveChangesAsync();

            return (await GetDealsAsync()).First(x => x.Id == id);
        }

        public async Task<bool> DeleteDealAsync(int id)
        {
            var deal = await _context.Deals.FindAsync(id);
            if (deal == null) return false;

            _context.Deals.Remove(deal);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
