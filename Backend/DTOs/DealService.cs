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
                    ImageUrl = d.ImageUrl,
                    IsActive = d.IsActive,

                    Items = d.DealItems.Select(x => new DealMenuItemDto
                    {
                        MenuItemId = x.MenuItemId,
                        Name = x.MenuItem.Name,
                        Price = x.MenuItem.Price,
                        Quantity = x.Quantity
                    }).ToList()

                }).ToListAsync();

        }

        public async Task<List<DealResponseDto>> GetActiveDealsAsync()
        {
            return await _context.Deals
                .Where(d => d.IsActive)
                .Include(d => d.DealItems)
                .ThenInclude(i => i.MenuItem)
                .Select(d => new DealResponseDto
                {
                    Id = d.Id,
                    DealName = d.DealName,
                    OriginalPrice = d.OriginalPrice,
                    DiscountAmount = d.DiscountAmount,
                    FinalPrice = d.FinalPrice,
                    ImageUrl = d.ImageUrl,
                    IsActive = d.IsActive,

                    Items = d.DealItems.Select(x => new DealMenuItemDto
                    {
                        MenuItemId = x.MenuItemId,
                        Name = x.MenuItem.Name,
                        Price = x.MenuItem.Price,
                        Quantity = x.Quantity
                    }).ToList()

                }).ToListAsync();
        }

        public async Task<string> SetDealImageUrlAsync(int id, string imageUrl)
        {
            var deal = await _context.Deals.FindAsync(id);
            if (deal == null) throw new Exception("Deal not found");

            deal.ImageUrl = imageUrl;
            await _context.SaveChangesAsync();

            return imageUrl;
        }

        public async Task<DealResponseDto> ToggleActiveAsync(int id)
        {
            var deal = await _context.Deals.FindAsync(id);
            if (deal == null) throw new Exception("Deal not found");

            deal.IsActive = !deal.IsActive;
            await _context.SaveChangesAsync();

            return (await GetDealsAsync()).First(x => x.Id == id);
        }

        public async Task<DealResponseDto> CreateDealAsync(CreateDealDto dto)
        {
            var nameExists = await _context.Deals
                .AnyAsync(d => d.DealName.ToLower() == dto.DealName.ToLower());
            if (nameExists)
                throw new Exception("A deal with this name already exists");

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

            var nameExists = await _context.Deals
                .AnyAsync(d => d.Id != id && d.DealName.ToLower() == dto.DealName.ToLower());
            if (nameExists)
                throw new Exception("A deal with this name already exists");

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
