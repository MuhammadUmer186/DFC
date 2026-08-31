namespace RestaurantSystem.Sync
{
    /// <summary>Bound from the <c>Idempotency</c> config section. Phase 6.</summary>
    public sealed class IdempotencyOptions
    {
        public const string SectionName = "Idempotency";

        /// <summary>Master switch. When false the middleware is a pass-through.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Max response bytes stored for replay. Larger responses are still replayed by status only.</summary>
        public int MaxStoredBodyBytes { get; set; } = 65536;

        /// <summary>
        /// If a matching key is still <c>in-progress</c>, reject with 409 +
        /// this <c>Retry-After</c> (seconds) instead of waiting.
        /// </summary>
        public int InProgressRetryAfterSeconds { get; set; } = 2;
    }
}
