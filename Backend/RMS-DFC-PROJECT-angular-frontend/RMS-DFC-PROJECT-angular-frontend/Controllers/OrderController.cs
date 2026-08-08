using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.DTOs;
using RestaurantSystem.Services;

namespace RestaurantSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrdersController(IOrderService service)
        {
            _service = service;
        }
        [Authorize(Roles = "Admin,Cashier,MainAdmin,Waiter")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderRequest request, [FromQuery] decimal discount)
        {
            var result = await _service.CreateAsync(request, discount, User,false);
            return Ok(result);
        }
        [Authorize(Roles = "Admin,Cashier,MainAdmin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }
        [Authorize(Roles = "Admin,Cashier,MainAdmin")]
        //[HttpGet]
        //public async Task<IActionResult> GetAll()
        //{
        //    return Ok(await _service.GetAllAsync());
        //}
        [Authorize(Roles = "Admin,Cashier,MainAdmin")]
        [HttpGet]
        public async Task<ActionResult<PagedResult<OrderDto>>> GetOrders(
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 5)  // pageSize as argument
        {
            var result = await _service.GetPagedAsync(page, pageSize);
            return Ok(result);
        }
        [Authorize(Roles = "Admin,Cashier,MainAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new { message = "Order deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("queue")]
        public async Task<IActionResult> GetQueuedOrders()
        {
            return Ok(await _service.GetQueuedOrdersAsync());
        }

        [HttpPost("pay")]
        public async Task<IActionResult> PayOrder(PayOrderRequest request)
        {
            return Ok(await _service.PayOrderAsync(request,User));
        }
        [Authorize(Roles = "Admin,Cashier,MainAdmin,Waiter")]
        [HttpPost("cancel/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            return Ok(await _service.CancelOrderAsync(orderId, User));
        }

        [HttpGet("paged")]
        public async Task<PagedResult<OrderDto>> GetPaged(
            DateOnly? date,
            int page = 0,
            int pageSize = 5)
        {
            return await _service.GetPagedAsync(date, page, pageSize);
        }
        [HttpPost("PublicOrder")]
        public async Task<IActionResult> CreateOnlineOrder([FromBody] PublicOrderRequest request)
        {
            var result = await _service.CreateOnlineOrderAsync(request);
            return Ok(result);
        }


    }



}
