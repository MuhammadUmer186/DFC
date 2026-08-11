using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.Helpers;
using RestaurantSystem.Models;

namespace RestaurantSystem.Services.Ai
{
    // Baseline, fully transparent forecasting model — a recency-weighted moving average over
    // the same weekday in prior weeks, with a seasonal (day-of-week) shape and an intraday
    // (hour-of-day) shape derived the same way. Deliberately NOT a language model and NOT an
    // ML.NET/black-box model: the spec calls for "a transparent baseline model before adding
    // complexity," and every number here can be explained as "average of the last N same
    // weekdays, weighted toward the most recent."
    public class ForecastingService
    {
        private const string ModelVersion = "baseline-seasonal-v1";
        private const int LookbackWeeks = 8;
        private const int MinOccurrencesForConfidence = 3;

        private readonly ApplicationDbContext _context;
        private readonly ILogger<ForecastingService> _logger;

        public ForecastingService(ApplicationDbContext context, ILogger<ForecastingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private record OrderPoint(DateTime CreatedAt, decimal TotalAmount);
        private record ItemPoint(DateTime CreatedAt, int MenuItemId, decimal Quantity);

        public async Task<AiForecastRun> GenerateForecastAsync(int horizonDays = 7, CancellationToken ct = default)
        {
            var today = BusinessDayHelper.GetBusinessToday();
            var forecastFrom = today.AddDays(1);
            var forecastTo = today.AddDays(horizonDays);

            // Training window: strictly BEFORE "today" — no future information ever leaks in,
            // and this run's own outputs are never read back into itself.
            var trainingStart = BusinessDayHelper.GetStart(today.AddDays(-(LookbackWeeks * 7)));
            var trainingEnd = BusinessDayHelper.GetStart(today);

            var historicalOrders = await _context.Orders
                .Where(o => o.CreatedAt >= trainingStart && o.CreatedAt < trainingEnd && o.Status == OrderStatus.Paid)
                .Select(o => new OrderPoint(o.CreatedAt, o.TotalAmount))
                .ToListAsync(ct);

            var historicalItemQuantities = await _context.OrderItems
                .Where(oi => oi.Order!.CreatedAt >= trainingStart && oi.Order.CreatedAt < trainingEnd && oi.Order.Status == OrderStatus.Paid)
                .Select(oi => new ItemPoint(oi.Order!.CreatedAt, oi.MenuItemId, oi.Quantity))
                .ToListAsync(ct);

            var activeMenuItemIds = await _context.MenuItems.Select(m => m.Id).ToListAsync(ct);

            var run = new AiForecastRun
            {
                CreatedAt = DateTime.Now,
                ModelVersion = ModelVersion,
                ForecastFrom = forecastFrom,
                ForecastTo = forecastTo,
                Status = "Completed"
            };

            int itemsWithNoHistory = 0;
            var values = new List<AiForecastValue>();

            for (var date = forecastFrom; date <= forecastTo; date = date.AddDays(1))
            {
                var weekday = date.DayOfWeek;
                var occurrences = WeightedSameWeekdayOccurrences(historicalOrders, weekday);

                var (predictedSales, predictedOrders, salesLow, salesHigh, lowConfidence) = WeightedDayStats(occurrences);

                values.Add(new AiForecastValue
                {
                    ForecastDate = date,
                    HourOfDay = null,
                    MenuItemId = null,
                    PredictedSales = predictedSales,
                    PredictedOrderCount = predictedOrders,
                    PredictedQuantity = 0,
                    ConfidenceLow = salesLow,
                    ConfidenceHigh = salesHigh,
                    LowConfidence = lowConfidence
                });

                // Hourly shape: proportion of each historical same-weekday's sales that fell in
                // each hour, averaged, then scaled onto the day-level prediction above.
                var hourlyProportions = HourlyProportions(historicalOrders, weekday);
                for (int hour = 0; hour < 24; hour++)
                {
                    var proportion = hourlyProportions.TryGetValue(hour, out var p) ? p : (1m / 24m);
                    values.Add(new AiForecastValue
                    {
                        ForecastDate = date,
                        HourOfDay = hour,
                        MenuItemId = null,
                        PredictedSales = Math.Round(predictedSales * proportion, 2),
                        PredictedOrderCount = (int)Math.Round(predictedOrders * proportion),
                        PredictedQuantity = 0,
                        ConfidenceLow = Math.Round(salesLow * proportion, 2),
                        ConfidenceHigh = Math.Round(salesHigh * proportion, 2),
                        LowConfidence = lowConfidence || hourlyProportions.Count == 0
                    });
                }

                // Item-level: weighted average quantity sold on this weekday, per menu item.
                foreach (var menuItemId in activeMenuItemIds)
                {
                    var itemOccurrences = WeightedSameWeekdayItemOccurrences(historicalItemQuantities, menuItemId, weekday);
                    if (itemOccurrences.Count == 0)
                    {
                        itemsWithNoHistory++;
                        continue; // nothing to forecast for an item with zero history on this weekday
                    }

                    var (qty, qtyLow, qtyHigh, itemLowConfidence) = WeightedQuantityStats(itemOccurrences);
                    values.Add(new AiForecastValue
                    {
                        ForecastDate = date,
                        HourOfDay = null,
                        MenuItemId = menuItemId,
                        PredictedSales = 0,
                        PredictedOrderCount = 0,
                        PredictedQuantity = qty,
                        ConfidenceLow = qtyLow,
                        ConfidenceHigh = qtyHigh,
                        LowConfidence = itemLowConfidence
                    });
                }
            }

            run.Values = values;
            if (itemsWithNoHistory > 0)
                run.Notes = $"{itemsWithNoHistory} menu-item/day combinations had no matching weekday history and were skipped (new or rarely-ordered items).";

            _context.AiForecastRuns.Add(run);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Generated forecast run {RunId} for {From}..{To} ({ValueCount} rows)", run.Id, forecastFrom, forecastTo, values.Count);
            return run;
        }

        /// Computes actuals + MAE/WAPE for any past forecast run that hasn't been backfilled yet.
        /// Safe to call repeatedly (idempotent recompute).
        public async Task BackfillAccuracyAsync(CancellationToken ct = default)
        {
            var today = BusinessDayHelper.GetBusinessToday();

            var runsToBackfill = await _context.AiForecastRuns
                .Where(r => r.ForecastTo < today && r.Mae == null)
                .Include(r => r.Values)
                .ToListAsync(ct);

            foreach (var run in runsToBackfill)
            {
                var dayLevelRows = run.Values.Where(v => v.HourOfDay == null && v.MenuItemId == null).ToList();
                if (dayLevelRows.Count == 0) continue;

                decimal totalAbsError = 0;
                decimal totalActual = 0;
                int comparedCount = 0;

                foreach (var row in dayLevelRows)
                {
                    var start = BusinessDayHelper.GetStart(row.ForecastDate);
                    var end = BusinessDayHelper.GetEnd(row.ForecastDate);

                    var actualOrders = await _context.Orders
                        .Where(o => o.CreatedAt >= start && o.CreatedAt < end && o.Status == OrderStatus.Paid)
                        .ToListAsync(ct);

                    row.ActualSales = actualOrders.Sum(o => o.TotalAmount);
                    row.ActualOrderCount = actualOrders.Count;

                    totalAbsError += Math.Abs(row.PredictedSales - row.ActualSales.Value);
                    totalActual += row.ActualSales.Value;
                    comparedCount++;
                }

                if (comparedCount > 0)
                {
                    run.Mae = Math.Round(totalAbsError / comparedCount, 2);
                    run.Wape = totalActual > 0 ? Math.Round(totalAbsError / totalActual * 100, 2) : null;
                }
            }

            if (runsToBackfill.Count > 0)
                await _context.SaveChangesAsync(ct);
        }

        // ---- weighted stats helpers ----

        private static List<(decimal weight, decimal sales, int orders)> WeightedSameWeekdayOccurrences(
            List<OrderPoint> historicalOrders, DayOfWeek weekday)
        {
            var byDate = historicalOrders
                .Where(o => o.CreatedAt.DayOfWeek == weekday)
                .GroupBy(o => DateOnly.FromDateTime(o.CreatedAt))
                .Select(g => new { Date = g.Key, Sales = g.Sum(x => x.TotalAmount), Orders = g.Count() })
                .OrderByDescending(x => x.Date)
                .Take(LookbackWeeks)
                .ToList();

            var result = new List<(decimal, decimal, int)>();
            for (int i = 0; i < byDate.Count; i++)
            {
                decimal weight = byDate.Count - i; // most recent week gets the highest weight
                result.Add((weight, byDate[i].Sales, byDate[i].Orders));
            }
            return result;
        }

        private static (decimal predicted, int orders, decimal low, decimal high, bool lowConfidence) WeightedDayStats(
            List<(decimal weight, decimal sales, int orders)> occurrences)
        {
            if (occurrences.Count == 0)
                return (0, 0, 0, 0, true);

            decimal totalWeight = occurrences.Sum(o => o.weight);
            decimal predictedSales = occurrences.Sum(o => o.weight * o.sales) / totalWeight;
            decimal predictedOrdersDec = occurrences.Sum(o => o.weight * o.orders) / totalWeight;

            decimal variance = occurrences.Sum(o => o.weight * (o.sales - predictedSales) * (o.sales - predictedSales)) / totalWeight;
            decimal stdev = (decimal)Math.Sqrt((double)variance);

            bool lowConfidence = occurrences.Count < MinOccurrencesForConfidence;
            decimal band = lowConfidence ? predictedSales * 0.5m : stdev;

            return (Math.Round(predictedSales, 2), (int)Math.Round(predictedOrdersDec), Math.Max(0, Math.Round(predictedSales - band, 2)), Math.Round(predictedSales + band, 2), lowConfidence);
        }

        private static Dictionary<int, decimal> HourlyProportions(List<OrderPoint> historicalOrders, DayOfWeek weekday)
        {
            var sameWeekday = historicalOrders.Where(o => o.CreatedAt.DayOfWeek == weekday).ToList();
            if (sameWeekday.Count == 0) return new Dictionary<int, decimal>();

            var byDate = sameWeekday.GroupBy(o => DateOnly.FromDateTime(o.CreatedAt))
                .OrderByDescending(g => g.Key)
                .Take(LookbackWeeks)
                .ToList();

            var proportions = new Dictionary<int, decimal>();
            int daysCounted = 0;

            foreach (var day in byDate)
            {
                decimal daySales = day.Sum(x => x.TotalAmount);
                if (daySales <= 0) continue;
                daysCounted++;

                foreach (var hourGroup in day.GroupBy(o => o.CreatedAt.Hour))
                {
                    decimal hourSales = hourGroup.Sum(x => x.TotalAmount);
                    decimal proportion = hourSales / daySales;
                    proportions[hourGroup.Key] = proportions.GetValueOrDefault(hourGroup.Key) + proportion;
                }
            }

            if (daysCounted == 0) return new Dictionary<int, decimal>();
            return proportions.ToDictionary(kv => kv.Key, kv => kv.Value / daysCounted);
        }

        private static List<(decimal weight, decimal quantity)> WeightedSameWeekdayItemOccurrences(
            List<ItemPoint> historicalItemQuantities, int menuItemId, DayOfWeek weekday)
        {
            var byDate = historicalItemQuantities
                .Where(oi => oi.MenuItemId == menuItemId && oi.CreatedAt.DayOfWeek == weekday)
                .GroupBy(oi => DateOnly.FromDateTime(oi.CreatedAt))
                .Select(g => new { Date = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.Date)
                .Take(LookbackWeeks)
                .ToList();

            var result = new List<(decimal, decimal)>();
            for (int i = 0; i < byDate.Count; i++)
            {
                decimal weight = byDate.Count - i;
                result.Add((weight, byDate[i].Quantity));
            }
            return result;
        }

        private static (decimal predicted, decimal low, decimal high, bool lowConfidence) WeightedQuantityStats(List<(decimal weight, decimal quantity)> occurrences)
        {
            decimal totalWeight = occurrences.Sum(o => o.weight);
            decimal predicted = occurrences.Sum(o => o.weight * o.quantity) / totalWeight;

            decimal variance = occurrences.Sum(o => o.weight * (o.quantity - predicted) * (o.quantity - predicted)) / totalWeight;
            decimal stdev = (decimal)Math.Sqrt((double)variance);

            bool lowConfidence = occurrences.Count < MinOccurrencesForConfidence;
            decimal band = lowConfidence ? Math.Max(predicted * 0.5m, 1) : stdev;

            return (Math.Round(predicted, 1), Math.Max(0, Math.Round(predicted - band, 1)), Math.Round(predicted + band, 1), lowConfidence);
        }
    }
}
