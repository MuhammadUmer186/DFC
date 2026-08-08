using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.DTOs;
using RestaurantSystem.DTOs.RestaurantSystem.DTOs;
using RestaurantSystem.Interfaces;

namespace RestaurantSystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UtilityBillsController : ControllerBase
    {
        private readonly IUtilityBillService _service;

        public UtilityBillsController(IUtilityBillService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{billdate}")]
        public async Task<IActionResult> Get(DateOnly billdate)
        {
            var bill = await _service.GetByIdAsync(billdate);
            return bill == null ? NotFound() : Ok(bill);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UtilityBillDto dto)
        {
            var bill = await _service.CreateAsync(dto);
            return Ok(bill);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UtilityBillDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return await _service.DeleteAsync(id)
                ? Ok(true)
                : NotFound();
        }

        // 🔥 Dashboard Summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            return Ok(await _service.GetBillsSummaryAsync());
        }
    }
}
