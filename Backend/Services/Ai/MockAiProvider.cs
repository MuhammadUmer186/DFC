namespace RestaurantSystem.Services.Ai
{
    // Used automatically when no OpenAI:ApiKey is configured, or when AiFeatures:UseMockProvider
    // is forced on (dev/CI) — deterministic, no network call, no cost. Never selected in a
    // deployment that has a real API key unless explicitly forced.
    public class MockAiProvider : IAiProvider
    {
        public string ProviderName => "Mock";

        public Task<AiCompletionResult> CompleteAsync(AiCompletionRequest request, CancellationToken ct = default)
        {
            // If the caller offered tools AND none has been called yet in this exchange, "call"
            // the first one with empty arguments so the orchestration path (tool -> result ->
            // follow-up) is exercised end to end. Once a tool result is already in the message
            // history, fall through to a text reply — otherwise this would call the same tool
            // forever and never produce a final answer (a real model always eventually replies).
            bool alreadyRanATool = request.Messages.Any(m => m.Role == AiMessageRole.Tool);
            if (request.Tools is { Count: > 0 } && !alreadyRanATool)
            {
                var tool = request.Tools[0];
                return Task.FromResult(new AiCompletionResult
                {
                    Success = true,
                    Provider = ProviderName,
                    Model = "mock",
                    ToolCalls = new List<AiToolCall>
                    {
                        new() { Id = "mock-call-1", Name = tool.Name, ArgumentsJson = "{}" }
                    }
                });
            }

            var lastUserMessage = request.Messages.LastOrDefault(m => m.Role == AiMessageRole.User)?.Content ?? string.Empty;
            var toolResultNote = alreadyRanATool ? " (a mock tool result was fetched above but isn't summarized here since no real model is configured)" : "";

            return Task.FromResult(new AiCompletionResult
            {
                Success = true,
                Provider = ProviderName,
                Model = "mock",
                Content = $"[mock response] No AI provider is configured yet, so this is a placeholder answer for: \"{Truncate(lastUserMessage, 120)}\"{toolResultNote}. Set OpenAI:ApiKey to get real responses."
            });
        }

        private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max] + "…";
    }
}
