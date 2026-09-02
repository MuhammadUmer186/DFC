namespace RestaurantSystem.Sync
{
    /// <summary>
    /// Bound from the <c>Migrator</c> configuration section. Controls the
    /// dedicated schema-migration path (Phase 14).
    /// </summary>
    public sealed class MigratorOptions
    {
        public const string SectionName = "Migrator";

        /// <summary>
        /// When true, a normal API start also applies pending migrations.
        /// Default: on in Development, off in Production. The production path is
        /// the one-shot migrator container (<c>dotnet RestaurantSystem.dll --migrate</c>).
        /// </summary>
        public bool? AutoMigrate { get; set; }

        /// <summary>
        /// When true (default in Production), a normal API start throws if there
        /// are pending migrations, so a misconfigured deploy fails fast instead
        /// of running against an old schema.
        /// </summary>
        public bool? RequireUpToDate { get; set; }

        /// <summary>Seconds the migrator waits for SQL Server to accept connections.</summary>
        public int SqlWaitSeconds { get; set; } = 120;

        /// <summary>Seconds to wait for the exclusive migration app-lock.</summary>
        public int LockTimeoutSeconds { get; set; } = 300;

        /// <summary>
        /// Directory (server-visible / mounted) for the pre-migration
        /// <c>BACKUP DATABASE</c> checkpoint. Empty ⇒ no physical backup; a
        /// warning is logged and the history row is marked <c>BackupTaken=false</c>.
        /// </summary>
        public string BackupPath { get; set; } = "";

        /// <summary>Take a <c>BACKUP DATABASE</c> checkpoint before applying (needs <see cref="BackupPath"/>).</summary>
        public bool BackupBeforeMigrate { get; set; } = true;

        /// <summary>
        /// When true, a failed pre-migration backup ABORTS the migrator. Default
        /// false: a failed/unavailable backup logs a warning and migrations still
        /// apply, so a missing backup volume never blocks a deployment.
        /// </summary>
        public bool BackupRequired { get; set; } = false;
    }
}
