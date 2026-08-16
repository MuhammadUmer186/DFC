using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Services;

namespace RestaurantSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrderService _orderService;
        private readonly IAreaService _areaService;

        public PublicController(ApplicationDbContext context, IOrderService orderService, IAreaService areaService)
        {
            _context = context;
            _orderService = orderService;
            _areaService = areaService;
        }

        // ===============================
        // 1. GET MENU (CUSTOMER SIDE)
        // ===============================
        [HttpGet("menu")]
        public async Task<ActionResult<PublicMenuDto>> GetMenu()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .Select(c => new PublicCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Items = c.MenuItems
                        .Where(m => m.IsAvailable)
                        .Select(m => new PublicMenuItemDto
                        {
                            Id = m.Id,
                            Name = m.Name,
                            Price = m.Price,
                            ImageUrl = m.ImageUrl,
                            Description = m.Description
                        }).ToList()
                })
                .ToListAsync();

            var deals = await _context.Deals
               .Where(d => d.IsActive)
                .Select(d => new PublicDealDto
                {
                    Id = d.Id,
                    DealName = d.DealName,
                    Price = d.FinalPrice,
                    ImageUrl = d.ImageUrl,
                    Items = d.DealItems.Select(di => new PublicDealItemDto
                    {
                        MenuItemId = di.MenuItemId,
                        Name = di.MenuItem.Name,
                        Quantity = di.Quantity
                    }).ToList()
                })
                .ToListAsync();

            var result = new PublicMenuDto
            {
                Categories = categories,
                Deals = deals
            };

            return Ok(result);
        }

        // ===============================
        // 1b. GET ESTIMATED SERVICE TIMES (CUSTOMER SIDE)
        // ===============================
        [HttpGet("service-times")]
        public async Task<ActionResult<List<ServiceTimeSettingDto>>> GetServiceTimes()
        {
            var settings = await _context.ServiceTimeSettings
                .Where(s => s.IsEnabled)
                .Select(s => new ServiceTimeSettingDto
                {
                    Id = s.Id,
                    ServiceType = s.ServiceType,
                    MinMinutes = s.MinMinutes,
                    MaxMinutes = s.MaxMinutes,
                    IsEnabled = s.IsEnabled
                })
                .ToListAsync();

            return Ok(settings);
        }

        // ===============================
        // 1c. GET SITE SETTINGS (HERO IMAGE, ETC.)
        // ===============================
        [HttpGet("site-settings")]
        public async Task<IActionResult> GetSiteSettings()
        {
            var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Id == 1);
            return Ok(new
            {
                heroImageUrl = setting?.HeroImageUrl,
                whatsAppNumber = setting?.WhatsAppNumber,
                restaurantName = setting?.RestaurantName,
                logoUrl = setting?.LogoUrl,
                companyName = setting?.CompanyName,
                companyLogoUrl = setting?.CompanyLogoUrl,
                country = setting?.Country ?? "Pakistan",
                timeZoneId = setting?.TimeZoneId ?? "Asia/Karachi",
                currencyCode = setting?.CurrencyCode ?? "PKR",
                currencySymbol = setting?.CurrencySymbol ?? "Rs"
            });
        }

        // ===============================
        // 1c-2. MENU PDF — stable QR target. This URL never changes even when the PDF is
        // re-uploaded (a new file with a new GUID name), so a printed QR code stays valid
        // indefinitely — it always redirects to whatever the current file is.
        // ===============================
        [HttpGet("menu-pdf")]
        public async Task<IActionResult> GetMenuPdf()
        {
            var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Id == 1);
            if (string.IsNullOrEmpty(setting?.MenuPdfUrl))
                return NotFound("No menu PDF has been uploaded yet");

            return Redirect(setting.MenuPdfUrl);
        }

        // ===============================
        // 1c-3. LOCATION — stable QR target for the restaurant's Google Maps pin. Same
        // pattern as menu-pdf: the printed QR encodes this endpoint, not the Maps link
        // directly, so updating the address in Settings never invalidates a printed QR code.
        // ===============================
        [HttpGet("location")]
        public async Task<IActionResult> GetLocation()
        {
            var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Id == 1);
            if (string.IsNullOrEmpty(setting?.GoogleMapsUrl))
                return NotFound("No location has been set yet");

            return Redirect(setting.GoogleMapsUrl);
        }

        // ===============================
        // 1d. GET DELIVERY AREAS (CUSTOMER SIDE)
        // ===============================
        [HttpGet("areas")]
        public async Task<IActionResult> GetAreas()
            => Ok(await _areaService.GetActiveAsync());

        // ===============================
        // 2. PLACE ORDER (ONLINE)
        // ===============================
        [HttpPost("order")]
        public async Task<ActionResult> PlaceOrder([FromBody] PublicOrderRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request");

            if ((request.Items == null || !request.Items.Any()) &&
                (request.Deals == null || !request.Deals.Any()))
            {
                return BadRequest("Order must contain at least one item or deal");
            }

            try
            {
                var order = await _orderService.CreateOnlineOrderAsync(request);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ===============================
        // 3. TRACK ORDER (order id + phone, so strangers can't scan through order ids)
        // ===============================
        [HttpGet("order/{id}/status")]
        public async Task<ActionResult<PublicOrderStatusDto>> GetOrderStatus(int id, [FromQuery] string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return BadRequest("Phone number is required");

            var result = await _orderService.GetPublicOrderStatusAsync(id, phone);
            if (result == null)
                return NotFound("Order not found. Double-check the order number and phone number.");

            return Ok(result);
        }
    }
}
