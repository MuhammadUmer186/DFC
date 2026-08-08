using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.Services;

namespace RestaurantSystem.Controllers
{
   [Authorize(Roles = "SuperAdmin,Admin,MainAdmin")]
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;

        public DashboardController(IDashboardService service)
        {
            _service = service;
        }

        // =======================
        // SALES
        // =======================
        [HttpGet("sales/summary")]
        public async Task<IActionResult> SalesSummary()
            => Ok(await _service.GetSalesSummaryAsync());
        
        [HttpGet("sales-range")]
        public async Task<IActionResult> GetSalesRange(DateOnly from, DateOnly to)
        {
            var result = await _service.GetSalesByDateRangeAsync(from, to);
            return Ok(result);
        }


        // =======================
        // PURCHASES
        // =======================
        [HttpGet("purchases/summary")]
        public async Task<IActionResult> PurchaseSummary()
            => Ok(await _service.GetPurchaseSummaryAsync());

        [HttpGet("purchases/by-date")]
        public async Task<IActionResult> PurchasesByDate([FromQuery] DateOnly from, [FromQuery] DateOnly to)
            => Ok(await _service.GetPurchasesByDateRangeAsync(from, to));
        [HttpGet("mainsummary")]
        public async Task<IActionResult> MainSummary()
            => Ok(await _service.GetMainSummaryAsync());
        [HttpGet("StockSummary")]
        public async Task<IActionResult> GetStockSummaryAsync()
            => Ok(await _service.GetStockSummaryAsync());
        [HttpGet("VendorSummary")]
        public async Task<IActionResult> VendorSummaryAsync()
            => Ok(await _service.GetVendorStockAsync());
        [HttpGet("profit/by-date")]
        public async Task<IActionResult> ProfitBydate([FromQuery] DateOnly from, [FromQuery] DateOnly to)
            => Ok(await _service.GetProfitAsync(from, to));

        [HttpGet("daily-profit")]
        public async Task<IActionResult> GetDailyProfit(DateOnly from, DateOnly to)
        {
            var data = await _service.GetDailyProfitAsync(from, to);
            return Ok(data);
        }

        [HttpGet("profit/summary")]
        public async Task<IActionResult> GetProfitSummary()
        {
            return Ok(await _service.GetProfitSummaryAsync());
        }

        [HttpGet("vendors/payment-summary")]
        public async Task<IActionResult> GetVendorPaymentSummary()
    => Ok(await _service.GetVendorPaymentSummaryAsync());
        [HttpGet("sales/top-categories")]
        public async Task<IActionResult> GetTopCategories()
    => Ok(await _service.GetTopSellingCategoriesAsync());

        [HttpGet("orders/count-summary")]
        public async Task<IActionResult> GetOrderCountSummary()
    => Ok(await _service.GetOrderCountSummaryAsync());

        [HttpGet("stock/usage-percentage")]
        public async Task<IActionResult> GetStockUsagePercentage()
    => Ok(await _service.GetStockUsagePercentageAsync());

        [HttpGet("utility")]
        public async Task<IActionResult> GetUtilities()
        {
            var data = await _service.GetUtilities();
            return Ok(data);
        }


    }


}
