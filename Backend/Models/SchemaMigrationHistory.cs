using System;

namespace RestaurantSystem.Models
{
    /// <summary>
    /// One row per run of the controlled database migrator (Phase 14). Distinct
    /// from EF's own <c>__EFMigrationsHistory</c> — this records <i>who</i> ran
    /// migrations, when, from/to which version, and whether a backup checkpoint
    /// was taken first.
    /// </summary>
    public partial class SchemaMigrationHistory
    {
        public int Id { get; set; }
        public DateTime StartedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
        public string? FromMigration { get; set; }
        public string? ToMigration { get; set; }
        public int AppliedCount { get; set; }
        public string? AppVersion { get; set; }
        public Guid NodeId { get; set; }
        public string NodeRole { get; set; } = "";
        public string? BackupPath { get; set; }
        public bool BackupTaken { get; set; }
        public string Outcome { get; set; } = "started"; // started | success | failed
        public string? Error { get; set; }
    }
}
