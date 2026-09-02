using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantSystem.Data;
using RestaurantSystem.Models;

namespace RestaurantSystem.Sync
{
    /// <summary>
    /// The controlled schema-migration path (Phase 14). Invoked as a one-shot:
    /// <c>dotnet RestaurantSystem.dll --migrate</c> (or env <c>RUN_MIGRATOR=true</c>).
    /// <list type="number">
    /// <item>wait for SQL Server;</item>
    /// <item>acquire an exclusive app-lock so only one migrator runs;</item>
    /// <item>take / record a backup checkpoint;</item>
    /// <item>apply pending migrations once;</item>
    /// <item>record the schema version;</item>
    /// <item>exit 0 (non-zero on any failure — the API container is gated on this).</item>
    /// </list>
    /// </summary>
    public sealed class DatabaseMigrator
    {
        private const string LockResource = "RMS_SchemaMigration";

        private readonly ApplicationDbContext _db;
        private readonly MigratorOptions _opts;
        private readonly DeploymentOptions _deployment;
        private readonly ILogger<DatabaseMigrator> _log;

        public DatabaseMigrator(
            ApplicationDbContext db,
            MigratorOptions opts,
            DeploymentOptions deployment,
            ILogger<DatabaseMigrator> log)
        {
            _db = db;
            _opts = opts;
            _deployment = deployment;
            _log = log;
        }

        public async Task<int> RunAsync(CancellationToken ct = default)
        {
            _log.LogInformation("Migrator: starting controlled migration run.");

            if (!await WaitForSqlAsync(ct))
            {
                _log.LogCritical("Migrator: SQL Server not reachable within {Sec}s — aborting.", _opts.SqlWaitSeconds);
                return 1;
            }

            await using var conn = new SqlConnection(_db.Database.GetConnectionString());
            await conn.OpenAsync(ct);

            if (!await AcquireLockAsync(conn, ct))
            {
                _log.LogCritical("Migrator: could not acquire the '{Res}' app-lock within {Sec}s — another migrator may be running.",
                    LockResource, _opts.LockTimeoutSeconds);
                return 1;
            }

            var history = new SchemaMigrationHistory
            {
                StartedAtUtc = DateTime.UtcNow,
                NodeId = Guid.TryParse(_deployment.NodeId, out var nid) ? nid : Guid.Empty,
                NodeRole = _deployment.NodeRole,
                AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
                Outcome = "started"
            };

            try
            {
                var applied = (await _db.Database.GetAppliedMigrationsAsync(ct)).ToList();
                var pending = (await _db.Database.GetPendingMigrationsAsync(ct)).ToList();
                history.FromMigration = applied.LastOrDefault();

                if (pending.Count == 0)
                {
                    _log.LogInformation("Migrator: database already up to date at '{Ver}'. Nothing to do.", history.FromMigration);
                    await ReleaseLockAsync(conn);
                    return 0;
                }

                _log.LogWarning("Migrator: {Count} pending migration(s): {List}", pending.Count, string.Join(", ", pending));

                (history.BackupTaken, history.BackupPath) = await TryBackupAsync(conn, ct);

                await _db.Database.MigrateAsync(ct);

                var nowApplied = (await _db.Database.GetAppliedMigrationsAsync(ct)).ToList();
                history.ToMigration = nowApplied.LastOrDefault();
                history.AppliedCount = pending.Count;
                history.CompletedAtUtc = DateTime.UtcNow;
                history.Outcome = "success";

                // The SchemaMigrationHistory table is created by the migrations we
                // just applied, so this insert is safe now.
                _db.Set<SchemaMigrationHistory>().Add(history);
                using (SyncStampingInterceptor.Suppress())
                    await _db.SaveChangesAsync(ct);

                _log.LogInformation("Migrator: applied {Count} migration(s); schema now at '{Ver}'. Backup: {Backup}.",
                    history.AppliedCount, history.ToMigration, history.BackupTaken ? history.BackupPath : "NOT TAKEN");

                await ReleaseLockAsync(conn);
                return 0;
            }
            catch (Exception ex)
            {
                history.Outcome = "failed";
                history.Error = ex.Message;
                history.CompletedAtUtc = DateTime.UtcNow;
                _log.LogCritical(ex, "Migrator: migration run FAILED. Restore the backup checkpoint before retrying if the schema is partially applied.");
                try
                {
                    if (await _db.Database.CanConnectAsync(ct) &&
                        (await _db.Database.GetAppliedMigrationsAsync(ct)).Contains(nameof(SchemaMigrationHistory)) == false)
                    {
                        // best-effort: only if the table exists
                        _db.Set<SchemaMigrationHistory>().Add(history);
                        using (SyncStampingInterceptor.Suppress())
                            await _db.SaveChangesAsync(ct);
                    }
                }
                catch { /* history is best-effort on failure */ }
                await ReleaseLockAsync(conn);
                return 1;
            }
        }

        private async Task<bool> WaitForSqlAsync(CancellationToken ct)
        {
            var deadline = DateTime.UtcNow.AddSeconds(_opts.SqlWaitSeconds);
            var attempt = 0;
            while (DateTime.UtcNow < deadline && !ct.IsCancellationRequested)
            {
                attempt++;
                try
                {
                    if (await _db.Database.CanConnectAsync(ct))
                    {
                        _log.LogInformation("Migrator: SQL Server reachable (attempt {N}).", attempt);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _log.LogDebug("Migrator: SQL not ready (attempt {N}): {Msg}", attempt, ex.Message);
                }
                await Task.Delay(TimeSpan.FromSeconds(Math.Min(10, attempt)), ct);
            }
            return false;
        }

        private async Task<bool> AcquireLockAsync(SqlConnection conn, CancellationToken ct)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "sp_getapplock";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = _opts.LockTimeoutSeconds + 30;
            cmd.Parameters.AddWithValue("@Resource", LockResource);
            cmd.Parameters.AddWithValue("@LockMode", "Exclusive");
            cmd.Parameters.AddWithValue("@LockOwner", "Session");
            cmd.Parameters.AddWithValue("@LockTimeout", _opts.LockTimeoutSeconds * 1000);
            var ret = new SqlParameter { Direction = ParameterDirection.ReturnValue };
            cmd.Parameters.Add(ret);
            await cmd.ExecuteNonQueryAsync(ct);
            var code = (int)(ret.Value ?? -99);
            // >= 0 : granted (0 = granted, 1 = granted after wait)
            return code >= 0;
        }

        private async Task ReleaseLockAsync(SqlConnection conn)
        {
            try
            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "sp_releaseapplock";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Resource", LockResource);
                cmd.Parameters.AddWithValue("@LockOwner", "Session");
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _log.LogDebug("Migrator: release lock (ignored): {Msg}", ex.Message);
            }
        }

        private async Task<(bool taken, string? path)> TryBackupAsync(SqlConnection conn, CancellationToken ct)
        {
            if (!_opts.BackupBeforeMigrate || string.IsNullOrWhiteSpace(_opts.BackupPath))
            {
                _log.LogWarning("Migrator: no pre-migration backup taken (Migrator:BackupPath not set). " +
                                "Ensure Phase 16 scheduled backups are in place.");
                return (false, null);
            }

            try
            {
                var dbName = conn.Database;
                var file = Path.Combine(_opts.BackupPath,
                    $"{dbName}_pre_{DateTime.UtcNow:yyyyMMdd_HHmmss}Z.bak");
                await using var cmd = conn.CreateCommand();
                cmd.CommandTimeout = 3600;
                // No COMPRESSION — unsupported on SQL Server Express (the production edition).
                cmd.CommandText =
                    $"BACKUP DATABASE [{dbName}] TO DISK = @file WITH INIT, CHECKSUM, NAME = N'pre-migration checkpoint';";
                cmd.Parameters.AddWithValue("@file", file);
                await cmd.ExecuteNonQueryAsync(ct);
                _log.LogInformation("Migrator: pre-migration backup written to {File}.", file);
                return (true, file);
            }
            catch (Exception ex)
            {
                if (_opts.BackupRequired)
                {
                    _log.LogCritical(ex, "Migrator: pre-migration BACKUP DATABASE failed and Migrator:BackupRequired=true — aborting.");
                    throw;
                }
                _log.LogWarning(ex,
                    "Migrator: pre-migration BACKUP DATABASE failed (path '{Path}' not writable / volume not mounted?). " +
                    "Continuing WITHOUT a checkpoint because Migrator:BackupRequired is not set. " +
                    "Take a manual backup and/or fix the backup volume.", _opts.BackupPath);
                return (false, null);
            }
        }
    }
}
