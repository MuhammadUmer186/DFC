using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Helpers;
using RestaurantSystem.Models;

namespace RestaurantSystem.Services
{
    public class MenuService: IMenuService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRestaurantClock _clock;
        public MenuService(ApplicationDbContext db, IRestaurantClock clock) { _context = db; _clock = clock; }

        public async Task<int> CreateMenuItemAsync(CreateMenuItemDto dto)
        {
            var item = new MenuItem
            {
                Name = dto.Name,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                Description = dto.Description
            };

            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();

            return item.Id;
        }

        public async Task<string> SetMenuItemImageUrlAsync(int id, string imageUrl)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null) throw new Exception("Item not found");

            item.ImageUrl = imageUrl;
            await _context.SaveChangesAsync();

            return imageUrl;
        }

        public async Task DeleteMenuItemAsync(int id)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null) throw new Exception("Item not found");

            _context.MenuItems.Remove(item);
            await _context.SaveChangesAsync();
        }

        public async Task<List<MenuItemDto>> GetByCategoryAsync(int categoryId)
        {
            return await _context.MenuItems
                .Where(x => x.CategoryId == categoryId)
                .Select(x => new MenuItemDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Price = x.Price,
                    CategoryId = x.CategoryId,
                    ImageUrl = x.ImageUrl,
                    Description = x.Description
                })
                .ToListAsync();
        }

        public async Task<List<CategoryDto>> GetMenuAsync()
        {
            return await _context.Categories
                .Include(c => c.MenuItems)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Items = c.MenuItems.Select(i => new MenuItemDto
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Price = i.Price,
                        CategoryId = i.CategoryId,
                        ImageUrl = i.ImageUrl,
                        Description = i.Description
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task UpdateMenuItemAsync(int id, CreateMenuItemDto dto)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null) throw new Exception("Item not found");

            item.Name = dto.Name;
            item.Price = dto.Price;
            item.CategoryId = dto.CategoryId;
            item.Description = dto.Description;

            await _context.SaveChangesAsync();
        }
        public async Task<List<MenuItemStatsDto>> GetMenuItemStatsOptimizedAsync()
        {
            var tz = await _clock.GetTimeZoneAsync();
            var now = await _clock.GetLocalNowAsync();
            var today = BusinessDayHelper.GetBusinessToday(tz);

            // Business day ranges
            var todayStart = BusinessDayHelper.GetStart(today, tz);
            var todayEnd = BusinessDayHelper.GetEnd(today, tz);

            var weekStart = BusinessDayHelper.GetStart(DateOnly.FromDateTime(now.AddDays(-(int)now.DayOfWeek)), tz);
            var monthStart = BusinessDayHelper.GetStart(DateOnly.FromDateTime(new DateTime(now.Year, now.Month, 1)), tz);

            // 1️⃣ Get all menu items
            var menuItems = await _context.MenuItems.ToListAsync();

            // 2️⃣ Query order items
            var orderItemsQuery = _context.OrderItems
                .Where(oi => oi.Order.Paid)
                .Select(oi => new
                {
                    oi.MenuItemId,
                    oi.Quantity,
                    oi.Order.CreatedAt
                });

            // 3️⃣ Query order deals and their menu items
            var dealItemsQuery = _context.OrderDeals
                .Where(od => od.Order.Paid)
                .SelectMany(od => od.Deal.DealItems, (od, di) => new
                {
                    di.MenuItemId,
                    Quantity = di.Quantity * od.Quantity, // deal qty * menu item qty in deal
                    od.Order.CreatedAt
                });

            // 4️⃣ Combine both
            var allItems = orderItemsQuery.Concat(dealItemsQuery);

            // 5️⃣ Group by MenuItemId and calculate counts per period
            var grouped = await allItems
                .GroupBy(i => i.MenuItemId)
                .Select(g => new
                {
                    MenuItemId = g.Key,
                    TodayCount = g.Where(x => x.CreatedAt >= todayStart && x.CreatedAt < todayEnd)
                                  .Sum(x => x.Quantity),
                    WeekCount = g.Where(x => x.CreatedAt >= weekStart).Sum(x => x.Quantity),
                    MonthCount = g.Where(x => x.CreatedAt >= monthStart).Sum(x => x.Quantity)
                })
                .ToListAsync();

            // 6️⃣ Map to DTO and include items with zero sales
            var stats = menuItems.Select(mi =>
            {
                var data = grouped.FirstOrDefault(g => g.MenuItemId == mi.Id);
                return new MenuItemStatsDto
                {
                    MenuItemId = mi.Id,
                    Name = mi.Name,
                    TodayCount = data?.TodayCount ?? 0,
                    WeekCount = data?.WeekCount ?? 0,
                    MonthCount = data?.MonthCount ?? 0
                };
            }).ToList();

            return stats;
        }
        public async Task<List<MenuItemStatsDto>> GetMenuItemStatsByDateAsync(DateOnly date)
        {
            var tz = await _clock.GetTimeZoneAsync();
            var today = BusinessDayHelper.GetBusinessToday(tz);
            // Convert DateOnly to DateTime range
            var start = BusinessDayHelper.GetStart(today, tz);
            var end = BusinessDayHelper.GetEnd(today, tz);

            // 1️⃣ Get all menu items
            var menuItems = await _context.MenuItems.ToListAsync();

            // 2️⃣ Query order items for the selected date
            var orderItemsQuery = _context.OrderItems
                .Where(oi => oi.Order.Paid && oi.Order.CreatedAt >= start && oi.Order.CreatedAt <= end)
                .Select(oi => new
                {
                    oi.MenuItemId,
                    oi.Quantity
                });

            // 3️⃣ Query order deals and their menu items for the selected date
            var dealItemsQuery = _context.OrderDeals
                .Where(od => od.Order.Paid && od.Order.CreatedAt >= start && od.Order.CreatedAt <= end)
                .SelectMany(od => od.Deal.DealItems, (od, di) => new
                {
                    di.MenuItemId,
                    Quantity = di.Quantity * od.Quantity // deal qty * menu item qty in deal
                });

            // 4️⃣ Combine both queries
            var allItems = orderItemsQuery.Concat(dealItemsQuery);

            // 5️⃣ Group by MenuItemId and sum quantities
            var grouped = await allItems
                .GroupBy(i => i.MenuItemId)
                .Select(g => new
                {
                    MenuItemId = g.Key,
                    Count = g.Sum(x => x.Quantity)
                })
                .ToListAsync();

            // 6️⃣ Map to DTO and include items with zero sales
            var stats = menuItems.Select(mi =>
            {
                var data = grouped.FirstOrDefault(g => g.MenuItemId == mi.Id);
                return new MenuItemStatsDto
                {
                    MenuItemId = mi.Id,
                    Name = mi.Name,
                    TodayCount = data?.Count ?? 0, // here TodayCount represents the selected date
                    //WeekCount = 0, // optional, can leave 0
                    //MonthCount = 0 // optional, can leave 0
                };
            }).ToList();

            return stats;
        }

        public async Task<List<MenuItemDto>> SearchMenuItemsAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new List<MenuItemDto>();

            term = term.ToLower();

            return await _context.MenuItems
                .Where(x => x.Name.ToLower().Contains(term))
                .Select(x => new MenuItemDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Price = x.Price,
                    CategoryId = x.CategoryId,
                    ImageUrl = x.ImageUrl,
                    Description = x.Description
                })
                .Take(20)
                .ToListAsync();
        }


    }
}
