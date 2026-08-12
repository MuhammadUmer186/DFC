using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.DTOs;
using RestaurantSystem.Services;

namespace RestaurantSystem.Controllers
{
    [ApiController]
    [Route("api/salaries")]
    public class SalaryController : ControllerBase
    {
        private readonly ISalaryService _service;
        private readonly IRestaurantClock _clock;

        public SalaryController(ISalaryService service, IRestaurantClock clock)
        {
            _service = service;
            _clock = clock;
        }

        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin")]
        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetEmployeePayments(int employeeId)
            => Ok(await _service.GetEmployeePaymentsAsync(employeeId));
        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin")]
        [HttpGet("status")]
        public async Task<IActionResult> GetSalaryStatus([FromQuery] DateOnly? date)
        {
            // If no date provided, default to today (in the restaurant's configured local time)
            var targetDate = date ?? DateOnly.FromDateTime(await _clock.GetLocalNowAsync());

            var result = await _service.GetSalaryStatusAsync(targetDate);
            return Ok(result);
        }
        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin")]
        // POST: api/salary/pay
        [HttpPost("pay")]
        public async Task<IActionResult> PaySalary([FromBody] PaySalaryDto dto)
        {
            await _service.PaySalaryAsync(dto);
            return Ok(new { message = "Salary paid successfully." });
        }
    }

}
