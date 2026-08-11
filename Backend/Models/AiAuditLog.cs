using System;

namespace RestaurantSystem.Models
{
    // One row per AI request across every AI feature (forecast recompute, inventory
    // recommendation generation, insights-assistant question, etc.) — "record AI requests,
    // recommendations, actions, approvals, errors, model versions, and relevant metadata".
    // Deliberately stores SUMMARIES, not raw prompts/DB rows — never persist payment data,
    // raw customer PII, or payroll figures here.
    public class AiAuditLog
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }

        public string Feature { get; set; } = null!; // "Forecast" | "InventoryRecommendation" | "InsightsAssistant"
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Role { get; set; }

        public string? RequestSummary { get; set; }
        public string? ResponseSummary { get; set; }

        public string? Provider { get; set; }
        public string? Model { get; set; }
        public int? InputTokens { get; set; }
        public int? OutputTokens { get; set; }

        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public long DurationMs { get; set; }
    }
}
