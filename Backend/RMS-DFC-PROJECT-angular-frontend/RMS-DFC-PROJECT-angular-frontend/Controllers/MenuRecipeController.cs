using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.DTOs;
using RestaurantSystem.Services;

namespace RestaurantSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Route("api/menu-recipe")]
    public class MenuRecipeController : ControllerBase
    {
        private readonly IMenuRecipeService _menuRecipeService;

        public MenuRecipeController(IMenuRecipeService menuRecipeService)
        {
            _menuRecipeService = menuRecipeService;
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignRecipe([FromBody] AssignMenuRecipeDto dto)
        {
            await _menuRecipeService.AssignRecipeToMenuItemAsync(dto);
            return Ok("Recipe assigned successfully");
        }

        [HttpGet("{menuItemId}")]
        public async Task<IActionResult> GetRecipe(int menuItemId)
        {
            var recipe = await _menuRecipeService.GetRecipeByMenuItemIdAsync(menuItemId);
            return Ok(recipe);
        }

        [HttpDelete("{menuItemId}")]
        public async Task<IActionResult> DeleteRecipe(int menuItemId)
        {
            await _menuRecipeService.DeleteRecipeByMenuItemIdAsync(menuItemId);
            return Ok("Recipe deleted");
        }
    }
}
