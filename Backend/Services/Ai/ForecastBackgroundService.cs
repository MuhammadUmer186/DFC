namespace RestaurantSystem.Services.Ai
{
    // No job scheduler (Hangfire/Quartz) exists in this codebase yet, and there's only ever one
    // backend instance running (see docker-compose.yml), so a plain BackgroundService is
    // proportional — it recomputes the forecast once a day and backfills accuracy for past runs.
    // If this is ever scaled to multiple backend instances, this needs to move to a real job
    // scheduler with distributed locking (see final report's "known limitations").
    public class ForecastBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ForecastBackgroundService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

        public ForecastBackgroundService(IServiceScopeFactory scopeFactory, ILogger<ForecastBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Small delay so this doesn't compete with app startup (DB migration, etc.).
            try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); } catch (OperationCanceledException) { return; }

            while (!stoppingToken.IsCancellationRequested)
            {
                await RunOnceAsync(stoppingToken);

                try { await Task.Delay(Interval, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }

        private async Task RunOnceAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var featureOptions = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AiFeatureOptions>>().Value;
            if (!featureOptions.ForecastingEnabled)
            {
                _logger.LogInformation("Forecasting feature flag is off — skipping scheduled forecast run");
                return;
            }

            var forecastingService = scope.ServiceProvider.GetRequiredService<ForecastingService>();
            try
            {
                await forecastingService.BackfillAccuracyAsync(ct);
                await forecastingService.GenerateForecastAsync(horizonDays: 7, ct: ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scheduled forecast run failed");
            }
        }
    }
}
