using RestaurantSystem.DTOs;

namespace RestaurantSystem.Services
{
    public interface IMenuRecipeService
    {
        Task AssignRecipeToMenuItemAsync(AssignMenuRecipeDto dto);

        Task<List<MenuRecipeResponseDto>> GetRecipeByMenuItemIdAsync(int menuItemId);

        Task DeleteRecipeByMenuItemIdAsync(int menuItemId);

        // Recipe Module
        Task<List<RecipeOverviewCategoryDto>> GetOverviewAsync();

        Task<KitchenAuditReportDto> GetKitchenAuditAsync(DateTime fromUtc, DateTime toUtc, bool includeByDish = true);
    }
}
