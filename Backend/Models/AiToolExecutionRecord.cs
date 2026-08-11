using System;

namespace RestaurantSystem.Models
{
    // One row per tool the assistant actually invoked while answering a question — lets an
    // admin see exactly which allowlisted operations ran, with what arguments, for any answer.
    public class AiToolExecutionRecord
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public DateTime CreatedAt { get; set; }

        public string ToolName { get; set; } = null!;
        public string ArgumentsJson { get; set; } = null!;
        public bool Success { get; set; }
        public string? ResultSummary { get; set; }
        public string? ErrorMessage { get; set; }
        public long DurationMs { get; set; }
    }
}
