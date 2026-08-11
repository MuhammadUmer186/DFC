using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.Helpers;
using RestaurantSystem.Models;

namespace RestaurantSystem.Services.Ai
{
    // Converts the latest forecast's per-menu-item demand into per-ingredient demand via
    // MenuRecipe, compares against current StoreStock, and produces recommendations a manager
    // must explicitly approve — this service never creates or submits a PurchaseOrder itself.
    public class InventoryRecommendationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InventoryRecommendationService> _logger;

        // Thresholds are deliberately simple constants (not learned/tuned) so every
        // recommendation stays explainable in plain language.
        private const decimal ExcessStockMultiplier = 3.0m;
        private const decimal WasteSignificanceRatio = 0.10m; // waste > 10% of forecasted demand

        public InventoryRecommendationService(ApplicationDbContext context, ILogger<InventoryRecommendationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<AiInventoryRecommendation>> GenerateRecommendationsAsync(CancellationToken ct = default)
        {
            var latestRun = await _context.AiForecastRuns
                .Include(r => r.Values)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (latestRun == null)
            {
                _logger.LogWarning("No forecast run available — cannot generate inventory recommendations yet");
                return new List<AiInventoryRecommendation>();
            }

            var horizonDays = Math.Max(1, (latestRun.ForecastTo.ToDateTime(TimeOnly.MinValue) - latestRun.ForecastFrom.ToDateTime(TimeOnly.MinValue)).Days + 1);

            // Aggregate item-level forecast rows -> total predicted quantity per menu item over the horizon.
            var demandByMenuItem = latestRun.Values
                .Where(v => v.HourOfDay == null && v.MenuItemId != null)
                .GroupBy(v => v.MenuItemId!.Value)
                .Select(g => new { MenuItemId = g.Key, Quantity = g.Sum(v => v.PredictedQuantity), AnyLowConfidence = g.Any(v => v.LowConfidence) })
                .ToList();

            var recipes = await _context.MenuRecipes
                .Include(r => r.RawItem)
                .ToListAsync(ct);

            // raw item -> (forecasted demand, any contributing forecast was low-confidence)
            var demandByRawItem = new Dictionary<int, (decimal demand, bool lowConfidence)>();
            foreach (var item in demandByMenuItem)
            {
                foreach (var recipe in recipes.Where(r => r.MenuItemId == item.MenuItemId))
                {
                    var required = item.Quantity * recipe.QuantityRequired;
                    if (demandByRawItem.TryGetValue(recipe.RawItemId, out var existing))
                        demandByRawItem[recipe.RawItemId] = (existing.demand + required, existing.lowConfidence || item.AnyLowConfidence);
                    else
                        demandByRawItem[recipe.RawItemId] = (required, item.AnyLowConfidence);
                }
            }

            var rawItemIds = demandByRawItem.Keys.ToList();
            var rawItems = await _context.RawItems.Where(r => rawItemIds.Contains(r.Id)).ToListAsync(ct);

            var stockByRawItem = await _context.StoreStocks
                .Where(s => rawItemIds.Contains(s.RawItemId))
                .GroupBy(s => s.RawItemId)
                .Select(g => new { RawItemId = g.Key, Quantity = g.Sum(s => s.Quantity), LatestVendorId = g.OrderByDescending(s => s.LastUpdated).First().VendorId })
                .ToListAsync(ct);

            var wasteSince = DateTime.Now.AddDays(-30);
            var recentWasteByRawItem = await _context.WasteItems
                .Where(w => rawItemIds.Contains(w.RawItemId) && w.WasteRecord.WasteDate >= wasteSince)
                .GroupBy(w => w.RawItemId)
                .Select(g => new { RawItemId = g.Key, Quantity = g.Sum(w => w.Quantity) })
                .ToListAsync(ct);

            var vendorIds = stockByRawItem.Select(s => s.LatestVendorId).Distinct().ToList();
            var vendors = await _context.Vendors.Where(v => vendorIds.Contains(v.Id)).ToListAsync(ct);

            var today = BusinessDayHelper.GetBusinessToday();
            var recommendations = new List<AiInventoryRecommendation>();

            foreach (var rawItem in rawItems)
            {
                var (forecastedDemand, demandLowConfidence) = demandByRawItem[rawItem.Id];
                var stockInfo = stockByRawItem.FirstOrDefault(s => s.RawItemId == rawItem.Id);
                var currentStock = stockInfo?.Quantity ?? 0;
                var vendor = stockInfo != null ? vendors.FirstOrDefault(v => v.Id == stockInfo.LatestVendorId) : null;
                var recentWaste = recentWasteByRawItem.FirstOrDefault(w => w.RawItemId == rawItem.Id)?.Quantity ?? 0;

                var warnings = new List<string>();
                if (rawItem.SafetyStockQuantity == null) warnings.Add("No safety-stock threshold on file for this ingredient");
                if (rawItem.ShelfLifeDays == null) warnings.Add("No shelf-life on file for this ingredient");
                if (vendor == null) warnings.Add("No recent purchase/stock vendor on file — lead time and minimum order quantity unknown");
                else
                {
                    if (vendor.LeadTimeDays == null) warnings.Add($"No lead time on file for vendor '{vendor.Name}'");
                    if (vendor.MinimumOrderQuantity == null) warnings.Add($"No minimum order quantity on file for vendor '{vendor.Name}'");
                }

                var safetyStock = rawItem.SafetyStockQuantity ?? 0;
                var shortfall = Math.Max(0, forecastedDemand + safetyStock - currentStock);

                string? recommendationType = null;
                string explanation;
                decimal suggestedReorderQuantity = 0;
                DateOnly? suggestedReorderDate = null;

                if (currentStock < forecastedDemand)
                {
                    // Already short for the horizon — the only honest date to suggest is "now."
                    // Vendor.LeadTimeDays isn't used to push this later; it only feeds the
                    // DataWarnings above when absent.
                    recommendationType = "LowStock";
                    suggestedReorderQuantity = shortfall;
                    suggestedReorderDate = today;
                    explanation = $"Forecasted demand over the next {horizonDays} day(s) is {forecastedDemand:0.##} {rawItem.Unit}, but current stock is only {currentStock:0.##} {rawItem.Unit} — stock will run out before demand is met.";
                }
                else if (shortfall > 0)
                {
                    recommendationType = "Reorder";
                    suggestedReorderQuantity = shortfall;
                    suggestedReorderDate = today;
                    explanation = $"Current stock ({currentStock:0.##} {rawItem.Unit}) covers forecasted demand ({forecastedDemand:0.##} {rawItem.Unit}) but not the configured safety-stock buffer ({safetyStock:0.##} {rawItem.Unit}).";
                }
                else if (forecastedDemand > 0 && currentStock > forecastedDemand * ExcessStockMultiplier)
                {
                    recommendationType = "ExcessStock";
                    explanation = $"Current stock ({currentStock:0.##} {rawItem.Unit}) is more than {ExcessStockMultiplier:0}x the {horizonDays}-day forecasted demand ({forecastedDemand:0.##} {rawItem.Unit}) — consider pausing reorders.";
                }
                else if (rawItem.ShelfLifeDays != null && forecastedDemand > 0)
                {
                    var dailyConsumption = forecastedDemand / horizonDays;
                    var daysOfStockOnHand = dailyConsumption > 0 ? currentStock / dailyConsumption : decimal.MaxValue;
                    if (daysOfStockOnHand > rawItem.ShelfLifeDays.Value)
                    {
                        recommendationType = "ExpiryRisk";
                        explanation = $"At the forecasted consumption rate, current stock ({currentStock:0.##} {rawItem.Unit}) would take about {daysOfStockOnHand:0.#} days to use, longer than this ingredient's {rawItem.ShelfLifeDays} day shelf life.";
                    }
                    else explanation = string.Empty;
                }
                else explanation = string.Empty;

                if (recommendationType == null && recentWaste > 0 && forecastedDemand > 0 && recentWaste > forecastedDemand * WasteSignificanceRatio)
                {
                    recommendationType = "WasteReduction";
                    explanation = $"{recentWaste:0.##} {rawItem.Unit} was recorded as waste in the last 30 days — over {WasteSignificanceRatio:P0} of the {horizonDays}-day forecasted demand. Consider reviewing portioning or storage for this ingredient.";
                }

                if (recommendationType == null) continue; // nothing worth flagging

                if (suggestedReorderQuantity > 0 && vendor?.MinimumOrderQuantity is decimal moq && suggestedReorderQuantity < moq)
                {
                    explanation += $" Bumped up to the vendor's minimum order quantity of {moq:0.##} {rawItem.Unit} (raw shortfall was {suggestedReorderQuantity:0.##} {rawItem.Unit}).";
                    suggestedReorderQuantity = moq;
                }

                var confidenceBandMultiplier = demandLowConfidence ? 1.3m : 1.0m;

                recommendations.Add(new AiInventoryRecommendation
                {
                    CreatedAt = DateTime.Now,
                    RawItemId = rawItem.Id,
                    CurrentStock = Math.Max(0, currentStock),
                    ForecastedDemand = Math.Max(0, forecastedDemand),
                    SuggestedReorderQuantity = Math.Max(0, suggestedReorderQuantity),
                    SuggestedReorderDate = suggestedReorderDate,
                    RecommendationType = recommendationType,
                    Explanation = demandLowConfidence
                        ? explanation + " (Based on limited historical data for the menu items using this ingredient — treat this as a rough estimate.)"
                        : explanation,
                    DataWarnings = warnings.Count > 0 ? string.Join("; ", warnings) : null,
                    ConfidenceLow = Math.Max(0, Math.Round(forecastedDemand / confidenceBandMultiplier, 1)),
                    ConfidenceHigh = Math.Round(forecastedDemand * confidenceBandMultiplier, 1),
                    Status = "Pending"
                });
            }

            if (recommendations.Count > 0)
            {
                _context.AiInventoryRecommendations.AddRange(recommendations);
                await _context.SaveChangesAsync(ct);
            }

            _logger.LogInformation("Generated {Count} inventory recommendations from forecast run {RunId}", recommendations.Count, latestRun.Id);
            return recommendations;
        }

        public async Task<AiInventoryRecommendation?> RecordDecisionAsync(int recommendationId, string decision, int? userId, string? userName, decimal? modifiedQuantity, string? feedback, CancellationToken ct = default)
        {
            var recommendation = await _context.AiInventoryRecommendations.FirstOrDefaultAsync(r => r.Id == recommendationId, ct);
            if (recommendation == null) return null;

            recommendation.Status = decision; // Approved | Rejected | Modified

            _context.AiInventoryRecommendationDecisions.Add(new AiInventoryRecommendationDecision
            {
                RecommendationId = recommendationId,
                DecidedAt = DateTime.Now,
                DecidedByUserId = userId,
                DecidedByUserName = userName,
                Decision = decision,
                ModifiedQuantity = modifiedQuantity,
                Feedback = feedback
            });

            await _context.SaveChangesAsync(ct);
            return recommendation;
        }
    }
}
