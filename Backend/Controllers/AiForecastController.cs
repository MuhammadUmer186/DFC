using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Models;
using RestaurantSystem.Services.Ai;

namespace RestaurantSystem.Controllers
{
    [ApiController]
    [Route("api/ai/forecast")]
    [Authorize(Roles = "SuperAdmin,Admin,MainAdmin")]
    [EnableRateLimiting("ai")]
    public class AiForecastController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ForecastingService _forecastingService;
        private readonly AiFeatureOptions _features;

        public AiForecastController(ApplicationDbContext context, ForecastingService forecastingService, IOptions<AiFeatureOptions> features)
        {
            _context = context;
            _forecastingService = forecastingService;
            _features = features.Value;
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest()
        {
            if (!_features.ForecastingEnabled) return StatusCode(503, new { message = "Forecasting is currently disabled" });

            var run = await _context.AiForecastRuns
                .Include(r => r.Values).ThenInclude(v => v.MenuItem)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            if (run == null) return NotFound(new { message = "No forecast has been generated yet" });

            return Ok(ToDto(run));
        }

        [HttpGet("runs")]
        public async Task<IActionResult> GetRuns()
        {
            var runs = await _context.AiForecastRuns
                .OrderByDescending(r => r.CreatedAt)
                .Take(30)
                .Select(r => new ForecastRunSummaryDto
                {
                    Id = r.Id,
                    CreatedAt = r.CreatedAt,
                    ForecastFrom = r.ForecastFrom,
                    ForecastTo = r.ForecastTo,
                    Mae = r.Mae,
                    Wape = r.Wape
                })
                .ToListAsync();

            return Ok(runs);
        }

        [HttpGet("runs/{id}")]
        public async Task<IActionResult> GetRun(int id)
        {
            var run = await _context.AiForecastRuns
                .Include(r => r.Values).ThenInclude(v => v.MenuItem)
                .FirstOrDefaultAsync(r => r.Id == id);

            return run == null ? NotFound() : Ok(ToDto(run));
        }

        [HttpPost("recalculate")]
        public async Task<IActionResult> Recalculate([FromQuery] int horizonDays = 7)
        {
            if (!_features.ForecastingEnabled) return StatusCode(503, new { message = "Forecasting is currently disabled" });
            if (horizonDays is < 1 or > 30) return BadRequest(new { message = "horizonDays must be between 1 and 30" });

            await _forecastingService.BackfillAccuracyAsync();
            var run = await _forecastingService.GenerateForecastAsync(horizonDays);

            var full = await _context.AiForecastRuns
                .Include(r => r.Values).ThenInclude(v => v.MenuItem)
                .FirstAsync(r => r.Id == run.Id);

            return Ok(ToDto(full));
        }

        private static ForecastRunDto ToDto(AiForecastRun run) => new()
        {
            Id = run.Id,
            CreatedAt = run.CreatedAt,
            ModelVersion = run.ModelVersion,
            ForecastFrom = run.ForecastFrom,
            ForecastTo = run.ForecastTo,
            Status = run.Status,
            Notes = run.Notes,
            Mae = run.Mae,
            Wape = run.Wape,
            Values = run.Values.Select(v => new ForecastValueDto
            {
                ForecastDate = v.ForecastDate,
                HourOfDay = v.HourOfDay,
                MenuItemId = v.MenuItemId,
                MenuItemName = v.MenuItem?.Name,
                PredictedSales = v.PredictedSales,
                PredictedOrderCount = v.PredictedOrderCount,
                PredictedQuantity = v.PredictedQuantity,
                ConfidenceLow = v.ConfidenceLow,
                ConfidenceHigh = v.ConfidenceHigh,
                LowConfidence = v.LowConfidence,
                ActualSales = v.ActualSales,
                ActualOrderCount = v.ActualOrderCount
            }).ToList()
        };
    }
}
