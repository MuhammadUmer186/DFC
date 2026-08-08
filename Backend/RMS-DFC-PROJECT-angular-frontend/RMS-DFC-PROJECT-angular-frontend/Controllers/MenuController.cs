using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantSystem.DTOs;
using RestaurantSystem.Services;
using System.Data;

namespace RestaurantSystem.Controllers
{
   [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }
        [Authorize(Roles = "Admin,Cashier,MainAdmin,Waiter")]
        [HttpGet]
        public async Task<IActionResult> GetMenu()
        {
            return Ok(await _menuService.GetMenuAsync());
        }
        [Authorize(Roles = "Admin,MainAdmin")]
        [HttpPost]
        public async Task<IActionResult> CreateMenuItem(CreateMenuItemDto dto)
        {
            await _menuService.CreateMenuItemAsync(dto);
            return Ok();
        }
        [Authorize(Roles = "Admin,MainAdmin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMenuItem(int id, CreateMenuItemDto dto)
        {
            await _menuService.UpdateMenuItemAsync(id, dto);
            return Ok();
        }
        [Authorize(Roles = "Admin,MainAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            await _menuService.DeleteMenuItemAsync(id);
            return Ok();
        }
        [Authorize(Roles = "Admin,Cashier,MainAdmin,Waiter")]
        [HttpGet("ByCategory/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var result = await _menuService.GetByCategoryAsync(categoryId);

            if (result == null || !result.Any())
                return NotFound("No menu items found for this category");

            return Ok(result);
        }

        [HttpGet("menu-stats")]
        public async Task<ActionResult<List<MenuItemStatsDto>>> GetMenuStats()
        {
            var stats = await _menuService.GetMenuItemStatsOptimizedAsync();
            return Ok(stats);
        }
        [HttpGet("menu-stats-date")]
        public async Task<ActionResult<List<MenuItemStatsDto>>> GetMenuStatsdatewise([FromQuery] DateOnly date)
        {
            var stats = await _menuService.GetMenuItemStatsByDateAsync(date);
            return Ok(stats);
        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchMenuItems(string term)
        {
            var result = await _menuService.SearchMenuItemsAsync(term);
            return Ok(result);
        }
    }

}
