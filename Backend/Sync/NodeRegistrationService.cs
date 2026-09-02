using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantSystem.Data;
using RestaurantSystem.Models;

namespace RestaurantSystem.Sync
{
    /// <summary>
    /// Ensures this deployment has a stable <see cref="Branch"/> + <see cref="SystemNode"/>
    /// identity. Runs once per process start, is fully idempotent, and never
    /// touches any pre-existing table. Offline-first / cloud-sync — Phase 1.
    /// </summary>
    public sealed class NodeRegistrationService
    {
        private readonly ApplicationDbContext _db;
        private readonly DeploymentOptions _opts;
        private readonly ILogger<NodeRegistrationService> _log;
        private readonly string _contentRoot;

        public NodeRegistrationService(
            ApplicationDbContext db,
            DeploymentOptions opts,
            ILogger<NodeRegistrationService> log,
            string contentRoot)
        {
            _db = db;
            _opts = opts;
            _log = log;
            _contentRoot = contentRoot;
        }

        /// <summary>The resolved node identity for this process (set after <see cref="EnsureRegisteredAsync"/>).</summary>
        public NodeIdentity? Identity { get; private set; }

        public async Task<NodeIdentity> EnsureRegisteredAsync(CancellationToken ct = default)
        {
            var role = _opts.ParsedRole;
            var appVersion = ResolveAppVersion();
            var schemaVersion = await ResolveSchemaVersionAsync(ct);
            var now = DateTime.UtcNow;

            // ---- 1. Resolve a stable NodeId -------------------------------------------------
            var (nodeId, nodeIdSource) = ResolveNodeId();

            // ---- 2. Ensure the Branch row -------------------------------------------------
            var branch = await EnsureBranchAsync(now, ct);

            // ---- 3. Upsert the SystemNode row ------------------------------------------------
            if (_opts.AutoRegisterNode)
            {
                var node = await _db.SystemNodes.FirstOrDefaultAsync(n => n.NodeId == nodeId, ct);
                if (node is null)
                {
                    node = new SystemNode
                    {
                        NodeId = nodeId,
                        Role = role,
                        BranchId = branch.BranchId,
                        DisplayName = string.IsNullOrWhiteSpace(_opts.NodeDisplayName)
                            ? $"{role} node ({branch.Name})"
                            : _opts.NodeDisplayName,
                        BaseUrl = role == NodeRole.Cloud ? NullIfBlank(_opts.CloudBaseUrl) : NullIfBlank(_opts.EdgeBaseUrl),
                        AppVersion = appVersion,
                        SchemaVersion = schemaVersion,
                        IsActive = true,
                        RegisteredAtUtc = now,
                        LastSeenAtUtc = now
                    };
                    _db.SystemNodes.Add(node);
                    _log.LogWarning(
                        "Sync/Phase1: registered NEW {Role} node {NodeId} (source={Source}) for branch {BranchId} ({BranchName}). " +
                        "Pin Deployment:NodeId in configuration so it never changes.",
                        role, nodeId, nodeIdSource, branch.BranchId, branch.Name);
                }
                else
                {
                    // Refresh the mutable, non-identity fields only.
                    node.Role = role;
                    node.BranchId = branch.BranchId;
                    node.BaseUrl = role == NodeRole.Cloud ? NullIfBlank(_opts.CloudBaseUrl) : NullIfBlank(_opts.EdgeBaseUrl);
                    node.AppVersion = appVersion;
                    node.SchemaVersion = schemaVersion;
                    node.LastSeenAtUtc = now;
                    if (!string.IsNullOrWhiteSpace(_opts.NodeDisplayName))
                        node.DisplayName = _opts.NodeDisplayName;
                    _log.LogInformation(
                        "Sync/Phase1: {Role} node {NodeId} present; refreshed last-seen for branch {BranchId}.",
                        role, nodeId, branch.BranchId);
                }

                // ---- 4. Record a "self" heartbeat ------------------------------------------
                _db.NodeHeartbeats.Add(new NodeHeartbeat
                {
                    NodeId = nodeId,
                    Role = role,
                    BranchId = branch.BranchId,
                    SentAtUtc = now,
                    ReceivedAtUtc = now,
                    AppVersion = appVersion,
                    SchemaVersion = schemaVersion,
                    PendingOutboxCount = 0,
                    Source = "self",
                    DetailsJson = null
                });

                await _db.SaveChangesAsync(ct);
            }
            else
            {
                _log.LogInformation("Sync/Phase1: AutoRegisterNode=false — skipped node/branch row upsert.");
            }

            Identity = new NodeIdentity(nodeId, role, branch.BranchId, branch.Code, appVersion, schemaVersion);
            return Identity;
        }

        // -----------------------------------------------------------------------------------

        private (Guid nodeId, string source) ResolveNodeId()
        {
            if (Guid.TryParse(_opts.NodeId, out var configured) && configured != Guid.Empty)
                return (configured, "config:Deployment:NodeId");

            var path = Path.IsPathRooted(_opts.NodeIdFilePath)
                ? _opts.NodeIdFilePath
                : Path.Combine(_contentRoot, _opts.NodeIdFilePath);

            try
            {
                if (File.Exists(path))
                {
                    var text = File.ReadAllText(path).Trim();
                    if (Guid.TryParse(text, out var persisted) && persisted != Guid.Empty)
                        return (persisted, $"file:{path}");
                }

                var generated = Guid.NewGuid();
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, generated.ToString());
                _log.LogWarning(
                    "Sync/Phase1: Deployment:NodeId not configured — generated {NodeId} and persisted it to {Path}. " +
                    "Mount this path on a durable volume, or set Deployment:NodeId explicitly.",
                    generated, path);
                return (generated, $"generated+file:{path}");
            }
            catch (Exception ex)
            {
                // Last resort: reuse an existing row for this role, else ephemeral (loud warning).
                var existing = _db.SystemNodes
                    .Where(n => n.Role == _opts.ParsedRole && n.IsActive)
                    .OrderBy(n => n.RegisteredAtUtc)
                    .FirstOrDefault();
                if (existing is not null)
                {
                    _log.LogWarning(ex,
                        "Sync/Phase1: could not read/write node-id file {Path}; reusing existing {Role} node {NodeId} from the database.",
                        path, _opts.ParsedRole, existing.NodeId);
                    return (existing.NodeId, "db:existing");
                }

                var ephemeral = Guid.NewGuid();
                _log.LogError(ex,
                    "Sync/Phase1: could not persist a node id ({Path}) and no existing node row found — using EPHEMERAL {NodeId}. " +
                    "This will change on restart; set Deployment:NodeId before enabling sync.",
                    path, ephemeral);
                return (ephemeral, "ephemeral");
            }
        }

        private async Task<Branch> EnsureBranchAsync(DateTime now, CancellationToken ct)
        {
            Branch? branch = null;

            if (Guid.TryParse(_opts.BranchId, out var configuredBranchId) && configuredBranchId != Guid.Empty)
            {
                branch = await _db.Branches.FirstOrDefaultAsync(b => b.BranchId == configuredBranchId, ct);
                if (branch is null)
                {
                    branch = NewBranch(configuredBranchId, now);
                    _db.Branches.Add(branch);
                    _log.LogWarning("Sync/Phase1: created Branch {BranchId} ({Name}) from Deployment:BranchId.",
                        branch.BranchId, branch.Name);
                }
                return branch;
            }

            // No BranchId configured — reuse the first existing branch, else create one.
            branch = await _db.Branches.OrderBy(b => b.Id).FirstOrDefaultAsync(ct);
            if (branch is null)
            {
                branch = NewBranch(Guid.NewGuid(), now);
                _db.Branches.Add(branch);
                _log.LogWarning(
                    "Sync/Phase1: no Branch configured or present — created default Branch {BranchId} ({Name}). " +
                    "Set Deployment:BranchId to this value on every node of this branch.",
                    branch.BranchId, branch.Name);
            }
            return branch;
        }

        private Branch NewBranch(Guid branchId, DateTime now)
        {
            var code = NullIfBlank(_opts.BranchCode)
                       ?? _db.SiteSettings.AsNoTracking()
                              .Where(s => s.Id == 1)
                              .Select(s => s.OrderSerialPrefix)
                              .FirstOrDefault();
            return new Branch
            {
                BranchId = branchId,
                Name = string.IsNullOrWhiteSpace(_opts.BranchName) ? "Main Branch" : _opts.BranchName,
                Code = NullIfBlank(code?.TrimEnd('-', ' ')),
                IsActive = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
        }

        private static string ResolveAppVersion() =>
            Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";

        private async Task<string?> ResolveSchemaVersionAsync(CancellationToken ct)
        {
            try
            {
                var applied = await _db.Database.GetAppliedMigrationsAsync(ct);
                return applied.LastOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private static string? NullIfBlank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }

    /// <summary>Immutable identity snapshot for the running process.</summary>
    public sealed record NodeIdentity(
        Guid NodeId,
        NodeRole Role,
        Guid BranchId,
        string? BranchCode,
        string AppVersion,
        string? SchemaVersion);
}
