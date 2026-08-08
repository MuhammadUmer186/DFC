using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;

namespace RestaurantSystem.Controllers
{
    [ApiController]
    [Route("api/sitesettings")]
    [Authorize(Roles = "SuperAdmin")]
    public class SiteSettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public SiteSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Id == 1);
            return Ok(new { heroImageUrl = setting?.HeroImageUrl });
        }

        [HttpPost("hero-image")]
        [RequestSizeLimit(15_000_000)] // 15 MB
        public async Task<IActionResult> UploadHeroImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(ext))
                return BadRequest("Unsupported image type. Use jpg, png or webp.");

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "site");
            Directory.CreateDirectory(folder);

            var fileName = $"hero_{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/uploads/site/{fileName}";

            var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Id == 1);
            if (setting == null)
            {
                setting = new Models.SiteSetting { Id = 1, HeroImageUrl = url };
                _context.SiteSettings.Add(setting);
            }
            else
            {
                setting.HeroImageUrl = url;
            }

            await _context.SaveChangesAsync();

            return Ok(new { heroImageUrl = url });
        }
    }
}
