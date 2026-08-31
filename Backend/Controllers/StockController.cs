using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.Services;
using System.Data;

namespace RestaurantSystem.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,StoreKeeper,MainAdmin")]
    [ApiController]
    [Route("api/stock")]
    public class StockController : ControllerBase
    {
        private readonly IStockReportService _service;
        private readonly IStockLedger _stockLedger;

        public StockController(IStockReportService service, IStockLedger stockLedger)
        {
            _service = service;
            _stockLedger = stockLedger;
        }

        // ===============================
        // PHASE 4 — LEDGER RECONCILIATION (StoreStock projection vs immutable ledger)
        // ===============================
        [Authorize(Roles = "SuperAdmin,MainAdmin")]
        [HttpGet("reconciliation")]
        public async Task<IActionResult> Reconciliation([FromQuery] bool onlyDiscrepancies = true)
        {
            var lines = await _stockLedger.ReconcileAsync(onlyDiscrepancies);
            var discrepancies = lines.Count(l => System.Math.Abs(l.Difference) >= 0.0001m);
            return Ok(new
            {
                generatedAtUtc = System.DateTime.UtcNow,
                discrepancyCount = discrepancies,
                balanced = discrepancies == 0,
                lines
            });
        }

        [Authorize(Roles = "SuperAdmin,MainAdmin")]
        [HttpPost("reconciliation/rebuild-projection")]
        public async Task<IActionResult> RebuildProjection()
        {
            await _stockLedger.RebuildProjectionAsync();
            return Ok(new { rebuilt = true, atUtc = System.DateTime.UtcNow });
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
