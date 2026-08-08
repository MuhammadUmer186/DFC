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

        public SalaryController(ISalaryService service)
        {
            _service = service;
        }

        [Authorize(Roles = "Admin,Cashier,MainAdmin")]
        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetEmployeePayments(int employeeId)
            => Ok(await _service.GetEmployeePaymentsAsync(employeeId));
        [Authorize(Roles = "Admin,Cashier,MainAdmin")]
        [HttpGet("status")]
        public async Task<IActionResult> GetSalaryStatus([FromQuery] DateOnly? date)
        {
            // If no date provided, default to today
            var targetDate = date ?? DateOnly.FromDateTime(DateTime.Now);

            var result = await _service.GetSalaryStatusAsync(targetDate);
            return Ok(result);
        }
        [Authorize(Roles = "Admin,Cashier,MainAdmin")]
        // POST: api/salary/pay
        [HttpPost("pay")]
        public async Task<IActionResult> PaySalary([FromBody] PaySalaryDto dto)
        {
            await _service.PaySalaryAsync(dto);
            return Ok(new { message = "Salary paid successfully." });
        }
    }

}
