using RestaurantSystem.DTOs;

namespace RestaurantSystem.Services
{
    public interface IMenuRecipeService
    {
        Task AssignRecipeToMenuItemAsync(AssignMenuRecipeDto dto);

        Task<List<MenuRecipeResponseDto>> GetRecipeByMenuItemIdAsync(int menuItemId);

        Task DeleteRecipeByMenuItemIdAsync(int menuItemId);
    }
}
