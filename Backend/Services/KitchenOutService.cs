using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.DTOs.RestaurantSystem.Dtos.Reports;
using RestaurantSystem.Helpers;
using RestaurantSystem.Models;
using System;

namespace RestaurantSystem.Services
{
    public class KitchenOutService : IKitchenOutService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRestaurantClock _clock;

        public KitchenOutService(ApplicationDbContext context, IRestaurantClock clock)
        {
            _context = context;
            _clock = clock;
        }

        public async Task CreateAsync(KitchenOutCreateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var kitchenOut = new KitchenOut
                {
                    IssuedAt = DateTime.UtcNow
                };

                foreach (var item in dto.Items)
                {
                    // Deduct from StoreStock
                    var stock = await _context.StoreStocks
                        .FirstOrDefaultAsync(s => s.RawItemId == item.RawItemId);

                    if (stock == null || stock.Quantity < item.Quantity)
                        throw new Exception($"Insufficient stock for RawItemId {item.RawItemId}");

                    stock.Quantity -= item.Quantity;
                    stock.LastUpdated = DateTime.UtcNow;

                    // Add to KitchenOutItems
                    kitchenOut.KitchenOutItems.Add(new KitchenOutItem
                    {
                        RawItemId = item.RawItemId,
                        Quantity = item.Quantity
                    });
                }

                _context.KitchenOuts.Add(kitchenOut);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<KitchenOutListDto>> GetAllAsync()
        {
            return await _context.KitchenOuts
                .Include(k => k.KitchenOutItems)
                    .ThenInclude(i => i.RawItem)
                .OrderByDescending(k => k.Id)
                .Select(k => new KitchenOutListDto
                {
                    Id = k.Id,
                    //ReferenceNo = k.ReferenceNo,
                    IssuedAt = k.IssuedAt,
                    Items = k.KitchenOutItems.Select(i => new KitchenOutListItemDto
                    {
                        RawItemId = i.RawItemId,
                        RawItemName = i.RawItem != null ? i.RawItem.Name : "",
                        Quantity = i.Quantity
                    }).ToList()
                })
                .ToListAsync();
        }
        public async Task<PagedResult<KitchenOutListDto>> GetPagedByDateAsync(
    DateOnly date,
    int page = 0,
    int pageSize = 5)
        {
            var tz = await _clock.GetTimeZoneAsync();
            var start = BusinessDayHelper.GetStart(date, tz);
            var end = BusinessDayHelper.GetEnd(date, tz);

            var query = _context.KitchenOuts
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
                        RawItemName = i.RawItem != null ? i.RawItem.Name : "",
                        Quantity = i.Quantity
                    }).ToList()
                });

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(k => k.Id)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<KitchenOutListDto>
            {
                Items = items,
                TotalCount = totalCount
            };
        }


        public async Task<List<KitchenInventoryDto>> GetCurrentKitchenInventoryAsync()
        {
            // 1️⃣ Net kitchen movement (Issued + Consumed via negative entries)
            var kitchenMovements = await _context.KitchenOutItems
                .GroupBy(x => x.RawItemId)
                .Select(g => new
                {
                    RawItemId = g.Key,
                    NetQuantity = g.Sum(x => x.Quantity) // +issue, -consume
                })
                .ToListAsync();

            // 2️⃣ Waste
            var wasted = await _context.WasteItems
                .GroupBy(x => x.RawItemId)
                .Select(g => new
                {
                    RawItemId = g.Key,
                    Waste = g.Sum(x => x.Quantity)
                })
                .ToListAsync();

            // 3️⃣ Final kitchen inventory
            var inventory = kitchenMovements.Select(k =>
            {
                var waste = wasted.FirstOrDefault(w => w.RawItemId == k.RawItemId)?.Waste ?? 0;

                return new KitchenInventoryDto
                {
                    RawItemId = k.RawItemId,
                    RawItemName = _context.RawItems
                        .First(r => r.Id == k.RawItemId).Name,
                    Quantity = k.NetQuantity - waste
                };
            }).ToList();

            return inventory;
        }
        // ----------------------------------------------------
        // 1️⃣ Weighted Average Cost from Purchase Orders
        // ----------------------------------------------------
        public async Task<List<RawItemAvgCostDto>> GetAverageCostsAsync()
        {
            var result = await _context.PurchaseOrderItems
                .GroupBy(x => new { x.RawItemId, x.RawItem.Name })
                .Select(g => new RawItemAvgCostDto
                {
                    RawItemId = g.Key.RawItemId,
                    RawItemName = g.Key.Name,
                    AverageCost = g.Sum(x => x.Quantity) == 0 ? 0
                                : g.Sum(x => x.Quantity * x.UnitPrice) / (decimal)g.Sum(x => x.Quantity)
                })
                .ToListAsync();

            return result;
        }

        // ----------------------------------------------------
        // 2️⃣ Daily Kitchen Cost + Waste Cost
        // ----------------------------------------------------
        public async Task<DailyKitchenCostDto> GetDailyKitchenCostAsync(DateOnly date)
        {
            var avgPrices = await GetAverageCostsAsync();

            var tz = await _clock.GetTimeZoneAsync();
            var start = BusinessDayHelper.GetStart(date, tz);
            var end = BusinessDayHelper.GetEnd(date, tz);

            var kitchenOut = await _context.KitchenOuts
                .Include(k => k.KitchenOutItems)
                .Where(k => k.IssuedAt >= start && k.IssuedAt < end)
                .ToListAsync();

            decimal kitchenCost = 0;

            foreach (var k in kitchenOut)
            {
                foreach (var item in k.KitchenOutItems)
                {
                    var avg = avgPrices.FirstOrDefault(a => a.RawItemId == item.RawItemId);

                    if (avg != null)
                        kitchenCost += item.Quantity * avg.AverageCost;
                }
            }

            var wastes = await _context.WasteRecords
                .Include(w => w.WasteItems)
                .Where(w => w.WasteDate >= start && w.WasteDate < end)
                .ToListAsync();

            decimal wasteCost = 0;

            foreach (var w in wastes)
            {
                foreach (var item in w.WasteItems)
                {
                    var avg = avgPrices.FirstOrDefault(a => a.RawItemId == item.RawItemId);

                    if (avg != null)
                        wasteCost += item.Quantity * avg.AverageCost;
                }
            }

            return new DailyKitchenCostDto
            {
                Date = date,
                TotalKitchenCost = Math.Round(kitchenCost, 2),
                TotalWasteCost = Math.Round(wasteCost, 2)
            };
        }

        // ----------------------------------------------------
        // 3️⃣ Monthly Kitchen Cost + Waste Cost
        // ----------------------------------------------------
        public async Task<MonthlyKitchenCostDto> GetMonthlyKitchenCostAsync(int year, int month)
        {
            var avgPrices = await GetAverageCostsAsync();

            var kitchenOut = await _context.KitchenOuts
                .Include(k => k.KitchenOutItems)
                .Where(k => k.IssuedAt.Year == year && k.IssuedAt.Month == month)
                .ToListAsync();

            decimal kitchenCost = 0;

            foreach (var k in kitchenOut)
            {
                foreach (var item in k.KitchenOutItems)
                {
                    var avg = avgPrices.FirstOrDefault(a => a.RawItemId == item.RawItemId);
                    if (avg != null)
                        kitchenCost += item.Quantity * avg.AverageCost;
                }
            }

            var wastes = await _context.WasteRecords
                .Include(w => w.WasteItems)
                .Where(w => w.WasteDate.Year == year && w.WasteDate.Month == month)
                .ToListAsync();

            decimal wasteCost = 0;

            foreach (var w in wastes)
            {
                foreach (var item in w.WasteItems)
                {
                    var avg = avgPrices.FirstOrDefault(a => a.RawItemId == item.RawItemId);
                    if (avg != null)
                        wasteCost += item.Quantity * avg.AverageCost;
                }
            }

            return new MonthlyKitchenCostDto
            {
                Year = year,
                Month = month,
                TotalKitchenCost = Math.Round(kitchenCost, 2),
                TotalWasteCost = Math.Round(wasteCost, 2)
            };
        }
        public async Task ConsumeAsync(
    Dictionary<int, decimal> rawItemConsumption,
    DateTime issuedAt,
    int? employeeId)
        {
            // 1️⃣ Validate against current kitchen inventory
            var kitchenInventory = await GetCurrentKitchenInventoryAsync();

            foreach (var entry in rawItemConsumption)
            {
                var available = kitchenInventory
                    .FirstOrDefault(x => x.RawItemId == entry.Key)?.Quantity ?? 0;

                if (available < entry.Value)
                    throw new Exception($"Insufficient kitchen stock for RawItemId {entry.Key}");
            }

            // 2️⃣ Create KitchenOut with NEGATIVE quantities
            var kitchenOut = new KitchenOut
            {
                IssuedAt = issuedAt
            };

            foreach (var entry in rawItemConsumption)
            {
                kitchenOut.KitchenOutItems.Add(new KitchenOutItem
                {
                    RawItemId = entry.Key,
                    Quantity = -entry.Value // 🔥 NEGATIVE = consumption
                });
            }

            _context.KitchenOuts.Add(kitchenOut);
            await _context.SaveChangesAsync();
        }

    }

}
