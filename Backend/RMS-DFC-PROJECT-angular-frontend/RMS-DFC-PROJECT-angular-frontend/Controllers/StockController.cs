using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.Services;
using System.Data;

namespace RestaurantSystem.Controllers
{
    [Authorize(Roles = "Admin,StoreKeeper,MainAdmin")]
    [ApiController]
    [Route("api/stock")]
    public class StockController : ControllerBase
    {
        private readonly IStockReportService _service;

        public StockController(IStockReportService service)
        {
            _service = service;
        }

        // ===============================
        // TOTAL STOCK SUMMARY
        // ===============================
        
        [HttpGet("summary")]
        public async Task<IActionResult> Summary()
            => Ok(await _service.GetStockSummaryAsync());

        // ===============================
        // VENDOR-WISE STOCK REPORT
        // ===============================
        [HttpGet("vendor-wise")]
        public async Task<IActionResult> VendorWise()
            => Ok(await _service.GetVendorWiseStockAsync());
    }

}
