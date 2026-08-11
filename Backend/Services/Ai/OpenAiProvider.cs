using System.ClientModel;
using OpenAI.Chat;

namespace RestaurantSystem.Services.Ai
{
    public class AiProviderOptions
    {
        public string? ApiKey { get; set; }
        public string Model { get; set; } = "gpt-4o-mini";
        public int TimeoutSeconds { get; set; } = 60;
    }

    public class OpenAiProvider : IAiProvider
    {
        private readonly ChatClient _client;
        private readonly ILogger<OpenAiProvider> _logger;

        public string ProviderName => "OpenAI";

        public OpenAiProvider(Microsoft.Extensions.Options.IOptions<AiProviderOptions> options, ILogger<OpenAiProvider> logger)
        {
            _logger = logger;
            var opts = options.Value;
            _client = new ChatClient(opts.Model, opts.ApiKey);
        }

        public async Task<AiCompletionResult> CompleteAsync(AiCompletionRequest request, CancellationToken ct = default)
        {
            try
            {
                var messages = request.Messages.Select(ToSdkMessage).ToList();

                var completionOptions = new ChatCompletionOptions
                {
                    Temperature = (float)request.Temperature,
                    MaxOutputTokenCount = request.MaxOutputTokens
                };

                if (request.Tools != null)
                {
                    foreach (var tool in request.Tools)
                    {
                        completionOptions.Tools.Add(ChatTool.CreateFunctionTool(
                            functionName: tool.Name,
                            functionDescription: tool.Description,
                            functionParameters: BinaryData.FromString(tool.ParametersJsonSchema)));
                    }
                }
                else if (!string.IsNullOrEmpty(request.ResponseJsonSchema))
                {
                    completionOptions.ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                        jsonSchemaFormatName: request.ResponseJsonSchemaName ?? "response",
                        jsonSchema: BinaryData.FromString(request.ResponseJsonSchema),
                        jsonSchemaIsStrict: true);
                }

                ChatCompletion completion = await _client.CompleteChatAsync(messages, completionOptions, ct);

                var result = new AiCompletionResult
                {
                    Success = true,
                    Provider = ProviderName,
                    Model = completion.Model,
                    InputTokens = completion.Usage?.InputTokenCount ?? 0,
                    OutputTokens = completion.Usage?.OutputTokenCount ?? 0
                };

                if (completion.FinishReason == ChatFinishReason.ToolCalls)
                {
                    result.ToolCalls = completion.ToolCalls.Select(tc => new AiToolCall
                    {
                        Id = tc.Id,
                        Name = tc.FunctionName,
                        ArgumentsJson = tc.FunctionArguments.ToString()
                    }).ToList();
                }
                else
                {
                    result.Content = completion.Content.Count > 0 ? completion.Content[0].Text : null;
                }

                return result;
            }
            catch (ClientResultException ex)
            {
                _logger.LogError(ex, "OpenAI request failed (status {Status})", ex.Status);
                return AiCompletionResult.Failed($"AI provider error (status {ex.Status})", ProviderName);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling OpenAI provider");
                return AiCompletionResult.Failed("Unexpected AI provider error", ProviderName);
            }
        }

        private static ChatMessage ToSdkMessage(AiMessage m) => m.Role switch
        {
            AiMessageRole.System => new SystemChatMessage(m.Content ?? string.Empty),
            AiMessageRole.User => new UserChatMessage(m.Content ?? string.Empty),
            AiMessageRole.Tool => new ToolChatMessage(m.ToolCallId ?? string.Empty, m.Content ?? string.Empty),
            AiMessageRole.Assistant => BuildAssistantMessage(m),
            _ => throw new NotSupportedException($"Unsupported role {m.Role}")
        };

        private static AssistantChatMessage BuildAssistantMessage(AiMessage m)
        {
            if (m.ToolCalls is { Count: > 0 })
            {
                var toolCalls = m.ToolCalls.Select(tc =>
                    ChatToolCall.CreateFunctionToolCall(tc.Id, tc.Name, BinaryData.FromString(tc.ArgumentsJson)));
                return new AssistantChatMessage(toolCalls);
            }
            return new AssistantChatMessage(m.Content ?? string.Empty);
        }
    }
}
