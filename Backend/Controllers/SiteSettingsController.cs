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
                logoUrl = setting?.LogoUrl,
                menuPdfUrl = setting?.MenuPdfUrl,
                orderSerialPrefix = setting?.OrderSerialPrefix ?? string.Empty,
                orderSerialStartingNumber = setting?.OrderSerialStartingNumber ?? 1,
                orderSerialResetTime = setting?.OrderSerialResetTime ?? TimeSpan.Zero,
                country = setting?.Country ?? "Pakistan",
                timeZoneId = setting?.TimeZoneId ?? "Asia/Karachi",
                currencyCode = setting?.CurrencyCode ?? "PKR",
                currencySymbol = setting?.CurrencySymbol ?? "Rs"
            });
        }

        [HttpPut("country-timezone")]
        public async Task<IActionResult> UpdateCountryTimeZone([FromBody] UpdateCountryTimeZoneDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Country))
                return BadRequest("Country is required");

            if (string.IsNullOrWhiteSpace(dto.TimeZoneId))
                return BadRequest("Time zone is required");

            if (string.IsNullOrWhiteSpace(dto.CurrencyCode))
                return BadRequest("Currency code is required");

            if (string.IsNullOrWhiteSpace(dto.CurrencySymbol))
                return BadRequest("Currency symbol is required");

            try
            {
                // Validates the IANA id is actually recognized on this server before saving it —
                // an unrecognized id would otherwise silently break the dashboard clock later.
                TimeZoneInfo.FindSystemTimeZoneById(dto.TimeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                return BadRequest("Unrecognized time zone");
            }

            var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Id == 1);
            if (setting == null)
            {
                setting = new Models.SiteSetting { Id = 1 };
                _context.SiteSettings.Add(setting);
            }

            setting.Country = dto.Country.Trim();
            setting.TimeZoneId = dto.TimeZoneId.Trim();
            setting.CurrencyCode = dto.CurrencyCode.Trim().ToUpperInvariant();
            setting.CurrencySymbol = dto.CurrencySymbol.Trim();

            await _context.SaveChangesAsync();

            return Ok(new { country = setting.Country, timeZoneId = setting.TimeZoneId, currencyCode = setting.CurrencyCode, currencySymbol = setting.CurrencySymbol });
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

        [HttpPut("order-serial")]
        public async Task<IActionResult> UpdateOrderSerialSetting([FromBody] UpdateOrderSerialSettingDto dto)
        {
            if (dto.StartingNumber < 0)
                return BadRequest("Starting number cannot be negative");

            if (dto.ResetTime < TimeSpan.Zero || dto.ResetTime >= TimeSpan.FromDays(1))
                return BadRequest("Reset time must be a valid time of day");

            var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Id == 1);
            if (setting == null)
            {
                setting = new Models.SiteSetting { Id = 1 };
                _context.SiteSettings.Add(setting);
            }

            setting.OrderSerialPrefix = dto.Prefix ?? string.Empty;
            setting.OrderSerialStartingNumber = dto.StartingNumber;
            setting.OrderSerialResetTime = dto.ResetTime;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                orderSerialPrefix = setting.OrderSerialPrefix,
                orderSerialStartingNumber = setting.OrderSerialStartingNumber,
                orderSerialResetTime = setting.OrderSerialResetTime
            });
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

        [HttpPost("menu-pdf")]
        [RequestSizeLimit(20_000_000)] // 20 MB
        public async Task<IActionResult> UploadMenuPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".pdf")
                return BadRequest("Unsupported file type. Please upload a PDF.");

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "site");
            Directory.CreateDirectory(folder);

            // A fresh GUID filename on every upload — the QR code customers scan points at the
            // stable Public/menu-pdf redirect (never this URL directly), so re-uploading here
            // never breaks an already-printed QR code.
            var fileName = $"menu_{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/uploads/site/{fileName}";

            var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Id == 1);
            if (setting == null)
            {
                setting = new Models.SiteSetting { Id = 1, MenuPdfUrl = url };
                _context.SiteSettings.Add(setting);
            }
            else
            {
                setting.MenuPdfUrl = url;
            }

            await _context.SaveChangesAsync();

            return Ok(new { menuPdfUrl = url });
        }
    }
}
