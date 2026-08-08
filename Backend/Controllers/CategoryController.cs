using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.DTOs;
namespace RestaurantSystem.Controllers
{

    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        public CategoriesController(ICategoryService service)
        {
            _service = service;
        }
        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin,Waiter")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }
        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin,Waiter")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }
        [Authorize(Roles = "SuperAdmin,Admin,MainAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create(CatDto dto)
        {
            try
            {
                await _service.CreateAsync(dto);
                return Ok(new { message = "Category created" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "SuperAdmin,Admin,MainAdmin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CatDto dto)
        {
            try
            {
                await _service.UpdateAsync(id, dto);
                return Ok(new { message = "Category updated" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "SuperAdmin,Admin,MainAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new { message = "Category deleted" });
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

            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
                return NotFound("Category not found");

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "categories");
            Directory.CreateDirectory(folder);

            var fileName = $"cat_{id}_{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/uploads/categories/{fileName}";
            await _service.SetImageUrlAsync(id, url);

            return Ok(new { imageUrl = url });
        }
    }
}

