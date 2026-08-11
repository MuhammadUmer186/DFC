namespace RestaurantSystem.Services.Ai
{
    // Implement this once per model provider (OpenAiProvider, MockAiProvider, and — if a
    // second provider is ever added — e.g. a Gemini/Claude provider) so callers never depend
    // on a specific vendor SDK.
    public interface IAiProvider
    {
        string ProviderName { get; }

        Task<AiCompletionResult> CompleteAsync(AiCompletionRequest request, CancellationToken ct = default);
    }
}
