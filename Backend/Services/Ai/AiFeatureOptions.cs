namespace RestaurantSystem.Services.Ai
{
    // Bound from the "AiFeatures" config section — lets each AI capability be switched off
    // independently (env-var override e.g. AiFeatures__ForecastingEnabled=false) without a
    // redeploy of code, per the "every AI feature can be enabled or disabled independently" rule.
    public class AiFeatureOptions
    {
        public bool ForecastingEnabled { get; set; } = true;
        public bool InventoryRecommendationsEnabled { get; set; } = true;
        public bool InsightsAssistantEnabled { get; set; } = true;

        /// Forces the Mock provider even when a real API key is configured — for local dev/CI.
        public bool UseMockProvider { get; set; } = false;
    }
}
