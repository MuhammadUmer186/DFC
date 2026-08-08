using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.DTOs;
using RestaurantSystem.Services;

namespace RestaurantSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/purchase-orders")]
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly IPurchaseOrderService _service;

        public PurchaseOrdersController(IPurchaseOrderService service)
        {
            _service = service;
        }

        // CREATE PURCHASE ORDER
        [HttpPost]
        public async Task<IActionResult> Create(PurchaseOrderCreateDto dto)
        {
            await _service.CreateAsync(dto);
            return Ok("Purchase order created");
        }

        // GET ALL PURCHASE ORDERS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _service.GetAllAsync();
            return Ok(orders);
        }

        // ⭐ GET PURCHASE ORDERS BY DATE
        [HttpGet("by-date")]
        public async Task<IActionResult> GetByDate([FromQuery] DateOnly date)
        {
            var orders = await _service.GetByDateAsync(date);

            if (orders == null || orders.Count == 0)
                return NoContent();

            return Ok(orders);
        }
    }
}
