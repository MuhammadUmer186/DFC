using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.DTOs;
using RestaurantSystem.Services;

namespace RestaurantSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/kitchen-out")]
    public class KitchenOutController : ControllerBase
    {
        private readonly IKitchenOutService _service;

        public KitchenOutController(IKitchenOutService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(KitchenOutCreateDto dto)
        {
            try
            {
                await _service.CreateAsync(dto);
                return Ok("Items issued to kitchen");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<ActionResult<List<KitchenOutListDto>>> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(data);
        }
        [HttpGet("paged-by-date")]
        public async Task<IActionResult> GetPagedByDate(
            [FromQuery] DateOnly date,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 5)
        {
            var result = await _service.GetPagedByDateAsync(date, page, pageSize);

            if (result == null || result.Items.Count == 0)
                return NoContent();

            return Ok(result);
        }


        [HttpGet("average-costs")]
        public async Task<IActionResult> GetAvgCosts()
        {
            return Ok(await _service.GetAverageCostsAsync());
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetDaily([FromQuery] DateOnly date)
        {
            return Ok(await _service.GetDailyKitchenCostAsync(date));
        }

        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthly([FromQuery] int year, [FromQuery] int month)
        {
            return Ok(await _service.GetMonthlyKitchenCostAsync(year, month));
        }
        [HttpGet("kitchen/inventory")]
        public async Task<IActionResult> GetKitchenInventory()
        {
            var inventory = await _service.GetCurrentKitchenInventoryAsync();
            return Ok(inventory);
        }

    }

}
