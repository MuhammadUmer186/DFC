using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RestaurantSystem.Models
{
    // One row per forecast computation. ModelVersion is a plain string tag (e.g.
    // "baseline-seasonal-v1") so a future model change is traceable against past runs and
    // their accuracy — "store forecasts, confidence ranges, model version, creation time, and
    // accuracy metrics."
    public class AiForecastRun
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ModelVersion { get; set; } = null!;
        public DateOnly ForecastFrom { get; set; }
        public DateOnly ForecastTo { get; set; }
        public string Status { get; set; } = "Completed"; // Completed | Failed
        public string? Notes { get; set; }

        // Populated later, once actuals for the forecasted dates are known (see
        // ForecastingService.BackfillAccuracyAsync).
        [Precision(18, 2)]
        public decimal? Mae { get; set; }
        [Precision(18, 2)]
        public decimal? Wape { get; set; }

        public virtual ICollection<AiForecastValue> Values { get; set; } = new List<AiForecastValue>();
    }
}
