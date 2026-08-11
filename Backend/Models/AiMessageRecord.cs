using System;

namespace RestaurantSystem.Models
{
    // Named *Record* to avoid clashing with the in-memory AiMessage DTO used for the live
    // provider call (Services/Ai/AiModels.cs) — this is the persisted row.
    public class AiMessageRecord
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public virtual AiConversation Conversation { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public string Role { get; set; } = null!; // "user" | "assistant"
        public string Content { get; set; } = null!;
    }
}
