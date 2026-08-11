using System;
using System.Collections.Generic;

namespace RestaurantSystem.Models
{
    // Scoped to a single manager (UserId) — never visible to any other user, including other
    // managers, per "avoid exposing one customer's/user's information to another."
    public class AiConversation
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastMessageAt { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string? Title { get; set; }

        public virtual ICollection<AiMessageRecord> Messages { get; set; } = new List<AiMessageRecord>();
    }
}
