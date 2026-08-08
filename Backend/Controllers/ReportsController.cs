using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.Services;
using RestaurantSystem.DTOs;

namespace RestaurantSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _service;

        public ReportsController(IReportService service)
        {
            _service = service;
        }
        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin")]
        // 🔹 PROFIT REPORT
        [HttpGet("profit")]
        public async Task<IActionResult> ProfitReport(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            return Ok(await _service.GetProfitReportAsync(from, to));
        }
        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin")]
        // 🔹 DAILY SALARY DETAIL
        [HttpGet("daily-salary")]
        public async Task<IActionResult> DailySalaryReport(
            [FromQuery] DateOnly date)
        {
            return Ok(await _service.GetDailySalaryReportAsync(date));
        }
        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin")]
        [HttpGet]
        public async Task<IActionResult> GetSalaryTotals([FromQuery] string from, [FromQuery] string? to = null)
        {
            try
            {
                // Parse dates from query string
                var fromDate = DateOnly.ParseExact(from, "yyyy-MM-dd");
                DateOnly? toDate = null;
                if (!string.IsNullOrWhiteSpace(to))
                {
                    toDate = DateOnly.ParseExact(to, "yyyy-MM-dd");
                }

                // Call service
                var (totalPaid, totalUnpaid) = await _service.GetSalaryTotalsAsync(fromDate, toDate);

                // Return JSON
                return Ok(new
                {
                    from = fromDate,
                    to = toDate ?? fromDate,
                    totalPaid,
                    totalUnpaid
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = "Invalid date format. Use yyyy-MM-dd"
                });
            }
        }
        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyReport([FromQuery] DateOnly date)
        {
            var result = await _service.GetDailyReportAsync(date);
            return Ok(result);
        }

        [Authorize(Roles = "SuperAdmin,Admin,Cashier,MainAdmin")]
        // 🔹 RANGE REPORT (Today / Yesterday / This Week / This Month / This Year / Overall / Custom)
        [HttpGet("range")]
        public async Task<IActionResult> GetReportByRange([FromQuery] DateOnly from, [FromQuery] DateOnly to)
        {
            if (to < from)
                return BadRequest("'to' date cannot be before 'from' date");

            var result = await _service.GetReportByRangeAsync(from, to);
            return Ok(result);
        }

    }
}
