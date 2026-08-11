namespace RestaurantSystem.Services.Ai
{
    // Provider-independent chat/tool-calling contract. Nothing outside this file (or a
    // provider implementation) should reference an SDK type (OpenAI.Chat.*, etc.) directly —
    // that's what keeps business logic swappable between providers.

    public class AiToolDefinition
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        /// Raw JSON Schema (as a string) describing the tool's parameters object.
        public string ParametersJsonSchema { get; set; } = "{\"type\":\"object\",\"properties\":{}}";
    }

    public class AiToolCall
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string ArgumentsJson { get; set; } = "{}";
    }

    public enum AiMessageRole { System, User, Assistant, Tool }

    public class AiMessage
    {
        public AiMessageRole Role { get; set; }
        public string? Content { get; set; }
        /// Set when Role == Tool — links this message back to the AiToolCall.Id it answers.
        public string? ToolCallId { get; set; }
        /// Set when Role == Assistant and the model requested tool calls instead of replying directly.
        public List<AiToolCall>? ToolCalls { get; set; }

        public static AiMessage System(string content) => new() { Role = AiMessageRole.System, Content = content };
        public static AiMessage User(string content) => new() { Role = AiMessageRole.User, Content = content };
    }

    public class AiCompletionRequest
    {
        public List<AiMessage> Messages { get; set; } = new();
        public List<AiToolDefinition>? Tools { get; set; }
        /// When set, asks the provider to constrain its final text reply to this JSON Schema.
        /// Ignored while the model is still making tool calls.
        public string? ResponseJsonSchema { get; set; }
        public string? ResponseJsonSchemaName { get; set; }
        public double Temperature { get; set; } = 0.2;
        public int MaxOutputTokens { get; set; } = 900;
    }

    public class AiCompletionResult
    {
        public bool Success { get; set; }
        public string? Content { get; set; }
        public List<AiToolCall> ToolCalls { get; set; } = new();
        public string? Model { get; set; }
        public string? Provider { get; set; }
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
        public string? Error { get; set; }

        public static AiCompletionResult Failed(string error, string? provider = null) =>
            new() { Success = false, Error = error, Provider = provider };
    }
}
