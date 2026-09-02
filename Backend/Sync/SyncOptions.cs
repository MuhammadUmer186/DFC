namespace RestaurantSystem.Sync
{
    /// <summary>Bound from the <c>Sync</c> configuration section. Phase 5.</summary>
    public sealed class SyncOptions
    {
        public const string SectionName = "Sync";

        /// <summary>Run the background sync worker on this node. Default: true on Edge, false on Cloud.</summary>
        public bool? WorkerEnabled { get; set; }

        /// <summary>Seconds between worker cycles when healthy.</summary>
        public int IntervalSeconds { get; set; } = 10;

        /// <summary>Max events per push/pull batch.</summary>
        public int BatchSize { get; set; } = 200;

        /// <summary>Shared HMAC-SHA256 secret for the edge&lt;-&gt;cloud pair. From env <c>SYNC_HMAC_SECRET</c>.</summary>
        public string HmacSecret { get; set; } = "";

        /// <summary>Allowed clock skew (minutes) for signed requests.</summary>
        public int ClockSkewMinutes { get; set; } = 5;

        /// <summary>Nonce retention window (minutes) for replay rejection.</summary>
        public int NonceWindowMinutes { get; set; } = 10;

        /// <summary>Retry backoff base / ceiling (seconds).</summary>
        public int BackoffBaseSeconds { get; set; } = 2;
        public int BackoffMaxSeconds { get; set; } = 300;

        /// <summary>How many history days the first bootstrap pull requests for transactional aggregates.</summary>
        public int BootstrapHistoryDays { get; set; } = 90;
    }
}
