using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Helpers;
using RestaurantSystem.Services;

namespace RestaurantSystem.Services.Ai
{
    // The ONLY way the insights assistant touches data — a fixed allowlist of narrow,
    // parameter-validated operations. The model can never issue SQL, never gets a DB
    // connection, and every tool result is built by application code (sums/averages/etc. are
    // computed here in C#, not by the model) — this is the control referenced throughout the
    // spec's Step 5 ("controlled analytics/tool layer with allowlisted operations").
    public interface IInsightsTools
    {
        List<AiToolDefinition> GetToolDefinitions();
        Task<AiToolExecutionResult> ExecuteAsync(string toolName, string argumentsJson, CancellationToken ct = default);
    }

    public class AiToolExecutionResult
    {
        public bool Success { get; set; }
        public string ResultText { get; set; } = string.Empty;
        public string? Error { get; set; }
    }

    public class InsightsTools : IInsightsTools
    {
        private readonly ApplicationDbContext _context;
        private readonly IReportService _reportService;
        private readonly IMenuProfitService _menuProfitService;
        private readonly ILogger<InsightsTools> _logger;
        private readonly IRestaurantClock _clock;

        private static readonly DateOnly EarliestAllowedDate = new(2000, 1, 1);
        private const int MaxRangeDays = 366;

        public InsightsTools(ApplicationDbContext context, IReportService reportService, IMenuProfitService menuProfitService, ILogger<InsightsTools> logger, IRestaurantClock clock)
        {
            _context = context;
            _reportService = reportService;
            _menuProfitService = menuProfitService;
            _logger = logger;
            _clock = clock;
        }

        public List<AiToolDefinition> GetToolDefinitions() => new()
        {
            new AiToolDefinition
            {
                Name = "get_sales_summary",
                Description = "Total sales, profit, and Online-vs-Site sales breakdown for a date range. Use this for 'why did revenue/profit change' and 'how are online vs in-store sales doing' questions.",
                ParametersJsonSchema = DateRangeSchema()
            },
            new AiToolDefinition
            {
                Name = "get_week_over_week_changes",
                Description = "Compares the last 7 days against the prior 7 days for sales, order count, and waste, flagging changes over 25%. Use this for 'what unusual changes happened this week' questions.",
                ParametersJsonSchema = "{\"type\":\"object\",\"properties\":{}}"
            },
            new AiToolDefinition
            {
                Name = "get_top_waste_items",
                Description = "Ingredients with the most recorded waste (by quantity) in a date range. Use this for 'which items generate the most waste' questions.",
                ParametersJsonSchema = DateRangeSchema(includeTopN: true)
            },
            new AiToolDefinition
            {
                Name = "get_menu_item_margins",
                Description = "Sales amount, cost, and profit per menu item for a date range, ordered by sales — use this to find high-sales/low-margin items.",
                ParametersJsonSchema = DateRangeSchema(includeTopN: true)
            },
            new AiToolDefinition
            {
                Name = "get_prep_plan",
                Description = "The latest AI demand forecast's predicted quantity per menu item for a specific date (defaults to tomorrow). Use this for 'what should be prepared tomorrow' questions.",
                ParametersJsonSchema = "{\"type\":\"object\",\"properties\":{\"date\":{\"type\":\"string\",\"description\":\"yyyy-MM-dd, optional, defaults to tomorrow\"}}}"
            },
            new AiToolDefinition
            {
                Name = "get_low_stock_ingredients",
                Description = "Ingredients currently flagged by the AI inventory recommendation engine as low-stock or needing reorder. Use this for 'which ingredients may run out' questions.",
                ParametersJsonSchema = "{\"type\":\"object\",\"properties\":{}}"
            }
        };

        public async Task<AiToolExecutionResult> ExecuteAsync(string toolName, string argumentsJson, CancellationToken ct = default)
        {
            try
            {
                return toolName switch
                {
                    "get_sales_summary" => await GetSalesSummaryAsync(argumentsJson, ct),
                    "get_week_over_week_changes" => await GetWeekOverWeekChangesAsync(ct),
                    "get_top_waste_items" => await GetTopWasteItemsAsync(argumentsJson, ct),
                    "get_menu_item_margins" => await GetMenuItemMarginsAsync(argumentsJson, ct),
                    "get_prep_plan" => await GetPrepPlanAsync(argumentsJson, ct),
                    "get_low_stock_ingredients" => await GetLowStockIngredientsAsync(ct),
                    _ => new AiToolExecutionResult { Success = false, Error = $"Unknown tool '{toolName}'" }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Insights tool {Tool} failed", toolName);
                return new AiToolExecutionResult { Success = false, Error = "Tool execution failed" };
            }
        }

        // ---- tool implementations ----

        private async Task<AiToolExecutionResult> GetSalesSummaryAsync(string argumentsJson, CancellationToken ct)
        {
            var (from, to, error) = TryParseDateRange(argumentsJson);
            if (error != null) return Fail(error);

            var report = await _reportService.GetReportByRangeAsync(from!.Value, to!.Value);

            var text = $"Sales summary {from:yyyy-MM-dd} to {to:yyyy-MM-dd} (data as of {DateTime.UtcNow:yyyy-MM-dd HH:mm}):\n" +
                       $"- Total sales (united): Rs {report.Sales:0.##} across {report.OnlineOrderCount + report.SiteOrderCount} paid orders\n" +
                       $"- Online: Rs {report.OnlineSales:0.##} ({report.OnlineOrderCount} orders)\n" +
                       $"- Site/POS: Rs {report.SiteSales:0.##} ({report.SiteOrderCount} orders)\n" +
                       $"- Purchase order cost: Rs {report.PurchaseOrdersCost:0.##}\n" +
                       $"- Kitchen cost: Rs {report.KitchenCost:0.##}, waste cost: Rs {report.WasteCost:0.##}\n" +
                       $"- Salary paid: Rs {report.SalaryPaid:0.##}, vendor payments: Rs {report.VendorPayments:0.##}\n" +
                       $"- Net profit (Sales - KitchenCost - WasteCost - SalaryPaid - VendorPayments): Rs {report.Profit:0.##}";

            return Ok(text);
        }

        private async Task<AiToolExecutionResult> GetWeekOverWeekChangesAsync(CancellationToken ct)
        {
            var today = BusinessDayHelper.GetBusinessToday(await _clock.GetTimeZoneAsync());
            var thisWeek = await _reportService.GetReportByRangeAsync(today.AddDays(-6), today);
            var lastWeek = await _reportService.GetReportByRangeAsync(today.AddDays(-13), today.AddDays(-7));

            var thisWasteSum = thisWeek.WasteCost;
            var lastWasteSum = lastWeek.WasteCost;

            string PctChange(decimal current, decimal previous)
            {
                if (previous == 0) return current == 0 ? "0%" : "n/a (no data in prior period)";
                var pct = (current - previous) / previous * 100;
                return $"{pct:+0.#;-0.#}%";
            }

            var salesChange = PctChange(thisWeek.Sales, lastWeek.Sales);
            var orderCountChange = PctChange(thisWeek.OnlineOrderCount + thisWeek.SiteOrderCount, lastWeek.OnlineOrderCount + lastWeek.SiteOrderCount);
            var wasteChange = PctChange(thisWasteSum, lastWasteSum);

            var text = $"Week-over-week ({today.AddDays(-6):yyyy-MM-dd}..{today:yyyy-MM-dd} vs {today.AddDays(-13):yyyy-MM-dd}..{today.AddDays(-7):yyyy-MM-dd}):\n" +
                       $"- Sales: Rs {thisWeek.Sales:0.##} vs Rs {lastWeek.Sales:0.##} ({salesChange})\n" +
                       $"- Paid order count: {thisWeek.OnlineOrderCount + thisWeek.SiteOrderCount} vs {lastWeek.OnlineOrderCount + lastWeek.SiteOrderCount} ({orderCountChange})\n" +
                       $"- Waste cost: Rs {thisWasteSum:0.##} vs Rs {lastWasteSum:0.##} ({wasteChange})\n" +
                       "Note: this system has no Promotion/Campaign records, so promotion-driven changes cannot be attributed automatically — mention that if asked about a specific promotion.";

            return Ok(text);
        }

        private async Task<AiToolExecutionResult> GetTopWasteItemsAsync(string argumentsJson, CancellationToken ct)
        {
            var (from, to, error) = TryParseDateRange(argumentsJson);
            if (error != null) return Fail(error);
            var topN = TryParseTopN(argumentsJson);

            var tz = await _clock.GetTimeZoneAsync();
            var start = BusinessDayHelper.GetStart(from!.Value, tz);
            var end = BusinessDayHelper.GetEnd(to!.Value, tz);

            var items = await _context.WasteItems
                .Where(w => w.WasteRecord.WasteDate >= start && w.WasteRecord.WasteDate < end)
                .GroupBy(w => new { w.RawItemId, w.RawItem.Name, w.RawItem.Unit })
                .Select(g => new { g.Key.Name, g.Key.Unit, Quantity = g.Sum(w => w.Quantity) })
                .OrderByDescending(x => x.Quantity)
                .Take(topN)
                .ToListAsync(ct);

            if (items.Count == 0) return Ok($"No waste was recorded between {from:yyyy-MM-dd} and {to:yyyy-MM-dd}.");

            var lines = items.Select((x, i) => $"{i + 1}. {x.Name}: {x.Quantity:0.##} {x.Unit}");
            return Ok($"Top waste items {from:yyyy-MM-dd} to {to:yyyy-MM-dd}:\n" + string.Join("\n", lines));
        }

        private async Task<AiToolExecutionResult> GetMenuItemMarginsAsync(string argumentsJson, CancellationToken ct)
        {
            var (from, to, error) = TryParseDateRange(argumentsJson);
            if (error != null) return Fail(error);
            var topN = TryParseTopN(argumentsJson);

            var tz = await _clock.GetTimeZoneAsync();
            var profits = await _menuProfitService.GetMenuProfitAsync(
                BusinessDayHelper.GetStart(from!.Value, tz),
                BusinessDayHelper.GetEnd(to!.Value, tz));

            if (profits.Count == 0) return Ok($"No menu-item sales data between {from:yyyy-MM-dd} and {to:yyyy-MM-dd}.");

            var byMargin = profits.Where(p => p.SalesAmount > 0)
                .OrderBy(p => p.SalesAmount == 0 ? 0 : p.Profit / p.SalesAmount) // lowest margin ratio first
                .Take(topN)
                .ToList();

            var lines = byMargin.Select(p =>
            {
                var marginPct = p.SalesAmount > 0 ? p.Profit / p.SalesAmount * 100 : 0;
                return $"- {p.MenuName}: sales Rs {p.SalesAmount:0.##}, cost Rs {p.CostAmount:0.##}, profit Rs {p.Profit:0.##} ({marginPct:0.#}% margin)";
            });

            return Ok($"Menu items by lowest margin, {from:yyyy-MM-dd} to {to:yyyy-MM-dd} (top {topN}):\n" + string.Join("\n", lines));
        }

        private async Task<AiToolExecutionResult> GetPrepPlanAsync(string argumentsJson, CancellationToken ct)
        {
            DateOnly date = BusinessDayHelper.GetBusinessToday(await _clock.GetTimeZoneAsync()).AddDays(1);
            if (!string.IsNullOrWhiteSpace(argumentsJson) && argumentsJson != "{}")
            {
                try
                {
                    using var doc = JsonDocument.Parse(argumentsJson);
                    if (doc.RootElement.TryGetProperty("date", out var dateEl) && dateEl.ValueKind == JsonValueKind.String
                        && DateOnly.TryParse(dateEl.GetString(), out var parsed))
                        date = parsed;
                }
                catch (JsonException) { /* fall back to default date */ }
            }

            var latestRun = await _context.AiForecastRuns
                .Include(r => r.Values).ThenInclude(v => v.MenuItem)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (latestRun == null) return Ok("No demand forecast has been generated yet — recommend generating one first.");
            if (date < latestRun.ForecastFrom || date > latestRun.ForecastTo)
                return Ok($"The latest forecast (run {latestRun.CreatedAt:yyyy-MM-dd}) only covers {latestRun.ForecastFrom:yyyy-MM-dd} to {latestRun.ForecastTo:yyyy-MM-dd}, which doesn't include {date:yyyy-MM-dd}. Recommend recalculating the forecast.");

            var items = latestRun.Values
                .Where(v => v.ForecastDate == date && v.HourOfDay == null && v.MenuItemId != null)
                .OrderByDescending(v => v.PredictedQuantity)
                .Take(15)
                .ToList();

            if (items.Count == 0) return Ok($"No item-level forecast data available for {date:yyyy-MM-dd}.");

            var lines = items.Select(v => $"- {v.MenuItem?.Name ?? "Unknown item"}: {v.PredictedQuantity:0.#} predicted units" + (v.LowConfidence ? " (low confidence — limited history)" : ""));
            return Ok($"Prep plan for {date:yyyy-MM-dd} (forecast run from {latestRun.CreatedAt:yyyy-MM-dd}, model {latestRun.ModelVersion}):\n" + string.Join("\n", lines));
        }

        private async Task<AiToolExecutionResult> GetLowStockIngredientsAsync(CancellationToken ct)
        {
            var items = await _context.AiInventoryRecommendations
                .Include(r => r.RawItem)
                .Where(r => r.Status == "Pending" && (r.RecommendationType == "LowStock" || r.RecommendationType == "Reorder"))
                .OrderByDescending(r => r.CreatedAt)
                .Take(20)
                .ToListAsync(ct);

            if (items.Count == 0) return Ok("No pending low-stock or reorder recommendations right now.");

            var lines = items.Select(r => $"- {r.RawItem.Name}: {r.CurrentStock:0.##} {r.RawItem.Unit} on hand, forecasted demand {r.ForecastedDemand:0.##} {r.RawItem.Unit}, suggested reorder {r.SuggestedReorderQuantity:0.##} {r.RawItem.Unit} ({r.RecommendationType})");
            return Ok("Pending low-stock / reorder recommendations:\n" + string.Join("\n", lines));
        }

        // ---- shared arg parsing/validation ----

        private static (DateOnly? from, DateOnly? to, string? error) TryParseDateRange(string argumentsJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
                var root = doc.RootElement;
                if (!root.TryGetProperty("from", out var fromEl) || !DateOnly.TryParse(fromEl.GetString(), out var from))
                    return (null, null, "Missing or invalid 'from' date (expected yyyy-MM-dd)");
                if (!root.TryGetProperty("to", out var toEl) || !DateOnly.TryParse(toEl.GetString(), out var to))
                    return (null, null, "Missing or invalid 'to' date (expected yyyy-MM-dd)");

                if (from < EarliestAllowedDate || to < EarliestAllowedDate)
                    return (null, null, "Dates out of allowed range");
                if (to < from)
                    return (null, null, "'to' must not be before 'from'");
                if ((to.ToDateTime(TimeOnly.MinValue) - from.ToDateTime(TimeOnly.MinValue)).Days > MaxRangeDays)
                    return (null, null, $"Date range too large — max {MaxRangeDays} days");

                return (from, to, null);
            }
            catch (JsonException)
            {
                return (null, null, "Invalid arguments JSON");
            }
        }

        private static int TryParseTopN(string argumentsJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
                if (doc.RootElement.TryGetProperty("topN", out var el) && el.TryGetInt32(out var n))
                    return Math.Clamp(n, 1, 20);
            }
            catch (JsonException) { }
            return 10;
        }

        private static string DateRangeSchema(bool includeTopN = false)
        {
            var topN = includeTopN ? ",\"topN\":{\"type\":\"integer\",\"description\":\"max results, 1-20, default 10\"}" : "";
            return "{\"type\":\"object\",\"properties\":{\"from\":{\"type\":\"string\",\"description\":\"yyyy-MM-dd\"},\"to\":{\"type\":\"string\",\"description\":\"yyyy-MM-dd\"}" + topN + "},\"required\":[\"from\",\"to\"]}";
        }

        private static AiToolExecutionResult Ok(string text) => new() { Success = true, ResultText = text };
        private static AiToolExecutionResult Fail(string error) => new() { Success = false, Error = error };
    }
}
