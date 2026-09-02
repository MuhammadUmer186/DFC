using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Models;

namespace RestaurantSystem.Services
{
    public class MenuRecipeService : IMenuRecipeService
    {
        private readonly ApplicationDbContext _context;

        public MenuRecipeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AssignRecipeToMenuItemAsync(AssignMenuRecipeDto dto)
        {
            // 1. Validate MenuItem
            var menuItemExists = await _context.MenuItems
                .AnyAsync(x => x.Id == dto.MenuItemId);

            if (!menuItemExists)
                throw new Exception("Menu item not found");

            // 2. Remove existing recipe
            var existingRecipes = _context.MenuRecipes
                .Where(x => x.MenuItemId == dto.MenuItemId);

            _context.MenuRecipes.RemoveRange(existingRecipes);

            // Touch the parent MenuItem so the sync outbox emits a MenuItemUpserted
            // event carrying the new recipe list (MenuRecipe rides in the snapshot).
            var parent = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == dto.MenuItemId);
            if (parent is not null) parent.UpdatedAtUtc = DateTime.UtcNow;

            // 3. Validate & add new recipe
            foreach (var item in dto.RecipeItems)
            {
                if (item.QuantityRequired <= 0)
                    throw new Exception("Quantity must be greater than zero");

                var rawItemExists = await _context.RawItems
                    .AnyAsync(x => x.Id == item.RawItemId);

                if (!rawItemExists)
                    throw new Exception("Raw item not found");

                _context.MenuRecipes.Add(new MenuRecipe
                {
                    MenuItemId = dto.MenuItemId,
                    RawItemId = item.RawItemId,
                    QuantityRequired = item.QuantityRequired
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<MenuRecipeResponseDto>> GetRecipeByMenuItemIdAsync(int menuItemId)
        {
            return await _context.MenuRecipes
                .Where(x => x.MenuItemId == menuItemId)
                .Select(x => new MenuRecipeResponseDto
                {
                    RawItemId = x.RawItemId,
                    RawItemName = x.RawItem.Name,
                    QuantityRequired = x.QuantityRequired,
                    Unit = x.RawItem.Unit
                })
                .ToListAsync();
        }

        public async Task DeleteRecipeByMenuItemIdAsync(int menuItemId)
        {
            var recipes = _context.MenuRecipes
                .Where(x => x.MenuItemId == menuItemId);

            _context.MenuRecipes.RemoveRange(recipes);
            var parent = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == menuItemId);
            if (parent is not null) parent.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // ============================ Recipe Module ============================

        public async Task<List<RecipeOverviewCategoryDto>> GetOverviewAsync()
        {
            // Ingredient count per menu item.
            var counts = await _context.MenuRecipes
                .GroupBy(r => r.MenuItemId)
                .Select(g => new { MenuItemId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.MenuItemId, x => x.Count);

            var cats = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    Items = c.MenuItems
                        .OrderBy(m => m.Name)
                        .Select(m => new { m.Id, m.Name, m.Price, m.IsAvailable })
                        .ToList()
                })
                .ToListAsync();

            return cats.Select(c =>
            {
                var items = c.Items.Select(m => new RecipeOverviewItemDto
                {
                    MenuItemId = m.Id,
                    MenuItemName = m.Name,
                    Price = m.Price,
                    IsAvailable = m.IsAvailable,
                    IngredientCount = counts.TryGetValue(m.Id, out var n) ? n : 0
                }).ToList();

                return new RecipeOverviewCategoryDto
                {
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    ItemCount = items.Count,
                    ItemsWithRecipe = items.Count(i => i.IngredientCount > 0),
                    Items = items
                };
            }).ToList();
        }

        public async Task<KitchenAuditReportDto> GetKitchenAuditAsync(DateTime fromUtc, DateTime toUtc, bool includeByDish = true)
        {
            if (toUtc < fromUtc) (fromUtc, toUtc) = (toUtc, fromUtc);

            // ---- 1. Units sold per menu item over the window (paid, non-cancelled) ----
            var soldOrders = _context.Orders
                .Where(o => o.Paid && o.Status != OrderStatus.Cancelled
                            && o.CreatedAt >= fromUtc && o.CreatedAt <= toUtc);

            var ordersCounted = await soldOrders.CountAsync();

            var directLines = await soldOrders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.MenuItemId)
                .Select(g => new { MenuItemId = g.Key, Units = g.Sum(x => (decimal)x.Quantity) })
                .ToListAsync();

            var unitsByMenuItem = new Dictionary<int, decimal>();
            foreach (var l in directLines)
                unitsByMenuItem[l.MenuItemId] = unitsByMenuItem.GetValueOrDefault(l.MenuItemId) + l.Units;

            // deal lines expand: dealQty × dealItemQty
            var dealQtyByDeal = await soldOrders
                .SelectMany(o => o.OrderDeals)
                .GroupBy(od => od.DealId)
                .Select(g => new { DealId = g.Key, Qty = g.Sum(x => (decimal)x.Quantity) })
                .ToListAsync();

            if (dealQtyByDeal.Count > 0)
            {
                var dealIds = dealQtyByDeal.Select(d => d.DealId).ToList();
                var dealItems = await _context.DealItems
                    .Where(di => dealIds.Contains(di.DealId))
                    .Select(di => new { di.DealId, di.MenuItemId, di.Quantity })
                    .ToListAsync();
                foreach (var d in dealQtyByDeal)
                    foreach (var di in dealItems.Where(x => x.DealId == d.DealId))
                        unitsByMenuItem[di.MenuItemId] =
                            unitsByMenuItem.GetValueOrDefault(di.MenuItemId) + di.Quantity * d.Qty;
            }

            var menuItemIds = unitsByMenuItem.Keys.ToList();
            var menuItems = await _context.MenuItems
                .Where(m => menuItemIds.Contains(m.Id))
                .Select(m => new { m.Id, m.Name })
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            var recipes = await _context.MenuRecipes
                .Where(r => menuItemIds.Contains(r.MenuItemId))
                .Select(r => new { r.MenuItemId, r.RawItemId, r.QuantityRequired, r.RawItem.Name, r.RawItem.Unit })
                .ToListAsync();

            // ---- 2. Expected consumption per raw item (recipe × units sold) ----
            var expected = new Dictionary<int, (string name, string unit, decimal qty)>();
            var byDish = new List<KitchenAuditDishDto>();

            foreach (var (menuItemId, units) in unitsByMenuItem.OrderByDescending(x => x.Value))
            {
                var mr = recipes.Where(r => r.MenuItemId == menuItemId).ToList();
                var dish = new KitchenAuditDishDto
                {
                    MenuItemId = menuItemId,
                    MenuItemName = menuItems.GetValueOrDefault(menuItemId, $"#{menuItemId}"),
                    UnitsSold = units,
                    HasRecipe = mr.Count > 0
                };

                foreach (var r in mr)
                {
                    var q = r.QuantityRequired * units;
                    var cur = expected.GetValueOrDefault(r.RawItemId, (r.Name, r.Unit, 0m));
                    expected[r.RawItemId] = (r.Name, r.Unit, cur.Item3 + q);
                    dish.Ingredients.Add(new KitchenAuditRowDto
                    {
                        RawItemId = r.RawItemId, RawItemName = r.Name, Unit = r.Unit,
                        ExpectedFromSales = q
                    });
                }
                if (includeByDish) byDish.Add(dish);
            }

            // ---- 3. Actual consumption from order-driven kitchen-out (negative rows) ----
            var actual = await _context.KitchenOutItems
                .Where(ki => ki.Quantity < 0
                             && ki.KitchenOut.IssuedAt >= fromUtc && ki.KitchenOut.IssuedAt <= toUtc)
                .GroupBy(ki => ki.RawItemId)
                .Select(g => new { RawItemId = g.Key, Qty = g.Sum(x => -x.Quantity) })
                .ToDictionaryAsync(x => x.RawItemId, x => x.Qty);

            // ---- 4. Merge into the totals table ----
            var rawIds = expected.Keys.Union(actual.Keys).ToList();
            var missingNames = await _context.RawItems
                .Where(ri => rawIds.Contains(ri.Id))
                .Select(ri => new { ri.Id, ri.Name, ri.Unit })
                .ToDictionaryAsync(x => x.Id, x => (x.Name, x.Unit));

            var totals = rawIds.Select(id =>
            {
                var exp = expected.GetValueOrDefault(id, (missingNames.GetValueOrDefault(id).Name ?? $"#{id}",
                                                          missingNames.GetValueOrDefault(id).Unit ?? "", 0m));
                var act = actual.GetValueOrDefault(id, 0m);
                return new KitchenAuditRowDto
                {
                    RawItemId = id,
                    RawItemName = exp.Item1,
                    Unit = exp.Item2,
                    ExpectedFromSales = exp.Item3,
                    ActualConsumed = act,
                    Variance = act - exp.Item3
                };
            })
            .OrderByDescending(r => Math.Abs(r.ExpectedFromSales))
            .ToList();

            return new KitchenAuditReportDto
            {
                FromUtc = fromUtc,
                ToUtc = toUtc,
                OrdersCounted = ordersCounted,
                LineUnitsCounted = unitsByMenuItem.Values.Sum(),
                DishesWithoutRecipe = byDish.Count(d => !d.HasRecipe),
                Totals = totals,
                ByDish = byDish
            };
        }
    }
}
