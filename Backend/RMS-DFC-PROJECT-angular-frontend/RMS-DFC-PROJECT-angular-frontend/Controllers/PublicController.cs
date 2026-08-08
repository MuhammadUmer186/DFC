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

        public PublicController(ApplicationDbContext context, IOrderService orderService)
        {
            _context = context;
            _orderService = orderService;
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
                            ImageUrl = m.ImageUrl
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

            // Call your existing service
            var order = await _orderService.CreateOnlineOrderAsync(request);

            return Ok(order);
        }
    }
}
