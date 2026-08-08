using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.DTOs;

namespace RestaurantSystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DealsController : ControllerBase
    {
        private readonly IDealService _service;
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        public DealsController(IDealService service)
        {
            _service = service;
        }
        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin,Waiter")]
        [HttpGet]
        public async Task<IActionResult> GetDeals()
            => Ok(await _service.GetActiveDealsAsync());

        [Authorize(Roles = "SuperAdmin,Admin,MainAdmin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllDeals()
            => Ok(await _service.GetDealsAsync());

        [Authorize(Roles = "SuperAdmin,Admin,MainAdmin")]
        [HttpPost]
        public async Task<IActionResult> CreateDeal(CreateDealDto dto)
        {
            try
            {
                return Ok(await _service.CreateDealAsync(dto));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin,MainAdmin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDeal(int id, CreateDealDto dto)
        {
            try
            {
                var result = await _service.UpdateDealAsync(id, dto);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin,MainAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeal(int id)
        {
            var result = await _service.DeleteDealAsync(id);
            if (!result) return NotFound();
            return Ok(true);
        }

        [Authorize(Roles = "SuperAdmin,Admin,MainAdmin")]
        [HttpPost("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                return Ok(await _service.ToggleActiveAsync(id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "deals");
                Directory.CreateDirectory(folder);

                var fileName = $"deal_{id}_{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var url = $"/uploads/deals/{fileName}";
                await _service.SetDealImageUrlAsync(id, url);

                return Ok(new { imageUrl = url });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

}
