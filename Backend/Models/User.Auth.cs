using System;

namespace RestaurantSystem.Models
{
    // Phase 8 — offline authentication. Additive partial fragment.
    public partial class User
    {
        /// <summary>Disabled accounts cannot log in. Syncs; revocation while the edge is offline is delayed to the next sync.</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Rotated on disable / password change. A token whose <c>stamp</c> claim
        /// no longer matches is rejected even before it expires.
        /// </summary>
        public Guid SecurityStamp { get; set; } = Guid.NewGuid();
    }

    /// <summary>Login / token-issue audit (Phase 8). Node-local — not synced.</summary>
    public partial class AuthAuditLog
    {
        public long Id { get; set; }
        public DateTime AtUtc { get; set; }
        public string UserName { get; set; } = "";
        public string? Role { get; set; }
        public string Result { get; set; } = "";      // success | bad-credentials | disabled | superadmin-config
        public string? Issuer { get; set; }
        public Guid NodeId { get; set; }
        public string? IpAddress { get; set; }
        public string? Detail { get; set; }
    }
}
