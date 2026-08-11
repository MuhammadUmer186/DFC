using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Services.Ai;

namespace RestaurantSystem.Controllers
{
    [ApiController]
    [Route("api/ai/inventory-recommendations")]
    [Authorize(Roles = "SuperAdmin,Admin,MainAdmin,StoreKeeper")]
    [EnableRateLimiting("ai")]
    public class AiInventoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly InventoryRecommendationService _service;
        private readonly AiFeatureOptions _features;

        public AiInventoryController(ApplicationDbContext context, InventoryRecommendationService service, IOptions<AiFeatureOptions> features)
        {
            _context = context;
            _service = service;
            _features = features.Value;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status = "Pending")
        {
            var query = _context.AiInventoryRecommendations.Include(r => r.RawItem).AsQueryable();
            if (!string.IsNullOrWhiteSpace(status) && status != "All")
                query = query.Where(r => r.Status == status);

            var results = await query
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => ToDto(r))
                .ToListAsync();

            return Ok(results);
        }

        [HttpPost("recalculate")]
        public async Task<IActionResult> Recalculate()
        {
            if (!_features.InventoryRecommendationsEnabled) return StatusCode(503, new { message = "Inventory recommendations are currently disabled" });

            var recommendations = await _service.GenerateRecommendationsAsync();
            return Ok(recommendations.Select(r => new InventoryRecommendationDto
            {
                Id = r.Id,
                CreatedAt = r.CreatedAt,
                RawItemId = r.RawItemId,
                RawItemName = r.RawItem?.Name ?? string.Empty,
                Unit = r.RawItem?.Unit ?? string.Empty,
                CurrentStock = r.CurrentStock,
                ForecastedDemand = r.ForecastedDemand,
                SuggestedReorderQuantity = r.SuggestedReorderQuantity,
                SuggestedReorderDate = r.SuggestedReorderDate,
                RecommendationType = r.RecommendationType,
                Explanation = r.Explanation,
                DataWarnings = r.DataWarnings,
                ConfidenceLow = r.ConfidenceLow,
                ConfidenceHigh = r.ConfidenceHigh,
                Status = r.Status
            }));
        }

        [HttpPost("{id}/decision")]
        public async Task<IActionResult> Decide(int id, InventoryRecommendationDecisionRequest request)
        {
            if (request.Decision is not ("Approved" or "Rejected" or "Modified"))
                return BadRequest(new { message = "Decision must be Approved, Rejected, or Modified" });
            if (request.Decision == "Modified" && request.ModifiedQuantity is null or < 0)
                return BadRequest(new { message = "ModifiedQuantity is required and must be non-negative when Decision is Modified" });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;

            var result = await _service.RecordDecisionAsync(id, request.Decision, userId, userName, request.ModifiedQuantity, request.Feedback);
            if (result == null) return NotFound();

            var withRawItem = await _context.AiInventoryRecommendations.Include(r => r.RawItem).FirstAsync(r => r.Id == id);
            return Ok(ToDto(withRawItem));
        }

        private static InventoryRecommendationDto ToDto(Models.AiInventoryRecommendation r) => new()
        {
            Id = r.Id,
            CreatedAt = r.CreatedAt,
            RawItemId = r.RawItemId,
            RawItemName = r.RawItem.Name,
            Unit = r.RawItem.Unit,
            CurrentStock = r.CurrentStock,
            ForecastedDemand = r.ForecastedDemand,
            SuggestedReorderQuantity = r.SuggestedReorderQuantity,
            SuggestedReorderDate = r.SuggestedReorderDate,
            RecommendationType = r.RecommendationType,
            Explanation = r.Explanation,
            DataWarnings = r.DataWarnings,
            ConfidenceLow = r.ConfidenceLow,
            ConfidenceHigh = r.ConfidenceHigh,
            Status = r.Status
        };
    }
}
