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
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }
        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin,Waiter")]
        [HttpGet]
        public async Task<IActionResult> GetMenu()
        {
            return Ok(await _menuService.GetMenuAsync());
        }
        [Authorize(Roles = "SuperAdmin,Admin,MainAdmin")]
        [HttpPost]
        public async Task<IActionResult> CreateMenuItem(CreateMenuItemDto dto)
        {
            try
            {
                var id = await _menuService.CreateMenuItemAsync(dto);
                return Ok(new { id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "SuperAdmin,Admin,MainAdmin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMenuItem(int id, CreateMenuItemDto dto)
        {
            try
            {
                await _menuService.UpdateMenuItemAsync(id, dto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "SuperAdmin,Admin,MainAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            try
            {
                await _menuService.DeleteMenuItemAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin,Waiter")]
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

        [Authorize(Roles = "SuperAdmin,Admin,MainAdmin")]
        [HttpPost("{id}/image")]
        [RequestSizeLimit(5_000_000)] // 5 MB
        public async Task<IActionResult> UploadImage(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(ext))
                return BadRequest("Unsupported image type. Use jpg, png, webp or gif.");

            try
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "menu-items");
                Directory.CreateDirectory(folder);

                var fileName = $"item_{id}_{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var url = $"/uploads/menu-items/{fileName}";
                await _menuService.SetMenuItemImageUrlAsync(id, url);

                return Ok(new { imageUrl = url });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

}
