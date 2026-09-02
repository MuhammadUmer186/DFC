using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.DTOs;
using RestaurantSystem.Services;

namespace RestaurantSystem.Controllers
{
    [ApiController]
    [Route("api/menu-recipe")]
    [Authorize(Roles = "SuperAdmin,Admin,MainAdmin")]
    public class MenuRecipeController : ControllerBase
    {
        private readonly IMenuRecipeService _menuRecipeService;

        public MenuRecipeController(IMenuRecipeService menuRecipeService)
        {
            _menuRecipeService = menuRecipeService;
        }

        /// <summary>Recipe Module — categories with their menu items and recipe coverage.</summary>
        [HttpGet("overview")]
        public async Task<IActionResult> Overview()
            => Ok(await _menuRecipeService.GetOverviewAsync());

        /// <summary>Recipe Module — total ingredients utilised by sales in a window (kitchen audit).</summary>
        [HttpGet("kitchen-audit")]
        public async Task<IActionResult> KitchenAudit(
            [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] bool byDish = true)
        {
            if (from == default) from = DateTime.UtcNow.Date.AddDays(-7);
            if (to == default) to = DateTime.UtcNow;
            return Ok(await _menuRecipeService.GetKitchenAuditAsync(
                from.ToUniversalTime(), to.ToUniversalTime(), byDish));
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignRecipe([FromBody] AssignMenuRecipeDto dto)
        {
            await _menuRecipeService.AssignRecipeToMenuItemAsync(dto);
            return Ok(new { message = "Recipe assigned successfully" });
        }

        [HttpGet("{menuItemId:int}")]
        public async Task<IActionResult> GetRecipe(int menuItemId)
            => Ok(await _menuRecipeService.GetRecipeByMenuItemIdAsync(menuItemId));

        [HttpDelete("{menuItemId:int}")]
        public async Task<IActionResult> DeleteRecipe(int menuItemId)
        {
            await _menuRecipeService.DeleteRecipeByMenuItemIdAsync(menuItemId);
            return Ok(new { message = "Recipe deleted" });
        }
    }
}
