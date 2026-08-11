using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;

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
            return Ok(new
            {
                heroImageUrl = setting?.HeroImageUrl,
                whatsAppNumber = setting?.WhatsAppNumber,
                restaurantName = setting?.RestaurantName,
                logoUrl = setting?.LogoUrl
            });
        }

        [HttpPut("restaurant-name")]
        public async Task<IActionResult> UpdateRestaurantName([FromBody] UpdateRestaurantNameDto dto)
        {
            var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Id == 1);
            if (setting == null)
            {
                setting = new Models.SiteSetting { Id = 1, RestaurantName = dto.RestaurantName };
                _context.SiteSettings.Add(setting);
            }
            else
            {
                setting.RestaurantName = dto.RestaurantName;
            }

            await _context.SaveChangesAsync();

            return Ok(new { restaurantName = setting.RestaurantName });
        }

        [HttpPost("logo")]
        [RequestSizeLimit(5_000_000)] // 5 MB — a logo, not a banner
        public async Task<IActionResult> UploadLogo(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(ext))
                return BadRequest("Unsupported image type. Use jpg, png or webp.");

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "site");
            Directory.CreateDirectory(folder);

            var fileName = $"logo_{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/uploads/site/{fileName}";

            var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Id == 1);
            if (setting == null)
            {
                setting = new Models.SiteSetting { Id = 1, LogoUrl = url };
                _context.SiteSettings.Add(setting);
            }
            else
            {
                setting.LogoUrl = url;
            }

            await _context.SaveChangesAsync();

            return Ok(new { logoUrl = url });
        }

        [HttpPut("whatsapp-number")]
        public async Task<IActionResult> UpdateWhatsAppNumber([FromBody] UpdateWhatsAppNumberDto dto)
        {
            var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Id == 1);
            if (setting == null)
            {
                setting = new Models.SiteSetting { Id = 1, WhatsAppNumber = dto.WhatsAppNumber };
                _context.SiteSettings.Add(setting);
            }
            else
            {
                setting.WhatsAppNumber = dto.WhatsAppNumber;
            }

            await _context.SaveChangesAsync();

            return Ok(new { whatsAppNumber = setting.WhatsAppNumber });
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
