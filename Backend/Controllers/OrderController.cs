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
        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin,Waiter")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderRequest request, [FromQuery] decimal discount)
        {
            var result = await _service.CreateAsync(request, discount, User,false);
            return Ok(result);
        }
        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }
        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin")]
        //[HttpGet]
        //public async Task<IActionResult> GetAll()
        //{
        //    return Ok(await _service.GetAllAsync());
        //}
        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin")]
        [HttpGet]
        public async Task<ActionResult<PagedResult<OrderDto>>> GetOrders(
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 5)  // pageSize as argument
        {
            var result = await _service.GetPagedAsync(page, pageSize);
            return Ok(result);
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
        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin,Waiter")]
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

        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin")]
        [HttpGet("pending-online")]
        public async Task<IActionResult> GetPendingOnlineOrders()
        {
            return Ok(await _service.GetPendingOnlineOrdersAsync());
        }

        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin")]
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveOnlineOrder(int id)
        {
            try
            {
                var result = await _service.ApproveOnlineOrderAsync(id, User);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin")]
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectOnlineOrder(int id)
        {
            try
            {
                var result = await _service.RejectOnlineOrderAsync(id, User);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin")]
        [HttpGet("active-online")]
        public async Task<IActionResult> GetActiveOnlineOrders()
        {
            return Ok(await _service.GetActiveOnlineOrdersAsync());
        }

        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin,Rider")]
        [HttpPatch("{id}/delivery-status")]
        public async Task<IActionResult> UpdateDeliveryStatus(int id, UpdateDeliveryStatusRequest request)
        {
            try
            {
                var result = await _service.UpdateDeliveryStatusAsync(id, request.Status, User);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Rider")]
        [HttpGet("my-deliveries")]
        public async Task<IActionResult> GetMyDeliveries()
        {
            try
            {
                var result = await _service.GetMyDeliveriesAsync(User);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin")]
        [HttpPost("{id}/assign-rider")]
        public async Task<IActionResult> AssignRider(int id, AssignRiderDto dto)
        {
            try
            {
                var result = await _service.AssignRiderAsync(id, dto, User);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin")]
        [HttpPost("{id}/reprint-delivery-slip")]
        public async Task<IActionResult> ReprintDeliverySlip(int id, [FromQuery] string copy = "customer")
        {
            try
            {
                var success = await _service.ReprintDeliverySlipAsync(id, copy);
                return Ok(new { printed = success });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }



}
