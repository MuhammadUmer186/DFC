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
            await _context.SaveChangesAsync();
        }
    }
}
