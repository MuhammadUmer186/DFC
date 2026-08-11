using System;
using Microsoft.EntityFrameworkCore;

namespace RestaurantSystem.Models
{
    // Three "kinds" of row share this table, distinguished by which nullable keys are set:
    //   - Day total:   HourOfDay = null, MenuItemId = null   (one row per forecasted date)
    //   - Hourly total: HourOfDay = 0..23, MenuItemId = null (24 rows per date — peak periods)
    //   - Per-item day total: HourOfDay = null, MenuItemId = X (one row per active item per date)
    public class AiForecastValue
    {
        public int Id { get; set; }
        public int ForecastRunId { get; set; }
        public virtual AiForecastRun ForecastRun { get; set; } = null!;

        public DateOnly ForecastDate { get; set; }
        public int? HourOfDay { get; set; }
        public int? MenuItemId { get; set; }
        public virtual MenuItem? MenuItem { get; set; }

        [Precision(18, 2)]
        public decimal PredictedSales { get; set; }
        public int PredictedOrderCount { get; set; }
        [Precision(18, 2)]
        public decimal PredictedQuantity { get; set; }
        [Precision(18, 2)]
        public decimal ConfidenceLow { get; set; }
        [Precision(18, 2)]
        public decimal ConfidenceHigh { get; set; }

        /// True when fewer than the minimum lookback occurrences were available (new item, or
        /// not enough weekday history yet) — the confidence band is intentionally wide and
        /// callers should surface that to the user rather than presenting it as reliable.
        public bool LowConfidence { get; set; }

        // Filled in by BackfillAccuracyAsync once the forecasted date has passed.
        [Precision(18, 2)]
        public decimal? ActualSales { get; set; }
        public int? ActualOrderCount { get; set; }
        [Precision(18, 2)]
        public decimal? ActualQuantity { get; set; }
    }
}
