using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.DTOs;

namespace RestaurantSystem.Services
{
    public interface IMenuService
    {
        Task<List<CategoryDto>> GetMenuAsync();
        Task<int> CreateMenuItemAsync(CreateMenuItemDto dto);
        Task UpdateMenuItemAsync(int id, CreateMenuItemDto dto);
        Task DeleteMenuItemAsync(int id);
        Task<string> SetMenuItemImageUrlAsync(int id, string imageUrl);
        Task<List<MenuItemDto>> GetByCategoryAsync(int categoryId);
        Task<List<MenuItemStatsDto>> GetMenuItemStatsOptimizedAsync();
        Task<List<MenuItemStatsDto>> GetMenuItemStatsByDateAsync(DateOnly date);
        Task<List<MenuItemDto>> SearchMenuItemsAsync(string term);
    }
}
