namespace RestaurantSystem.DTOs
{
    public class AskAssistantRequest
    {
        public int? ConversationId { get; set; }
        public string Question { get; set; } = null!;
    }

    public class AskAssistantResponse
    {
        public int ConversationId { get; set; }
        public string Answer { get; set; } = null!;
        public List<string> ToolsUsed { get; set; } = new();
    }

    public class ConversationSummaryDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
    }

    public class ConversationMessageDto
    {
        public string Role { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class ConversationDetailDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public List<ConversationMessageDto> Messages { get; set; } = new();
    }
}
