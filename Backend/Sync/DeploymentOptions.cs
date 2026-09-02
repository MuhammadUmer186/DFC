using RestaurantSystem.Models;

namespace RestaurantSystem.Sync
{
    /// <summary>
    /// Bound from the <c>Deployment</c> configuration section. Real values come
    /// from environment variables / mounted secrets (see <c>.env.example</c>,
    /// <c>.env.edge.example</c>); the checked-in <c>appsettings*.json</c> ship
    /// empty placeholders only.
    /// <para>
    /// Env-var overrides use the standard double-underscore form, e.g.
    /// <c>Deployment__NodeRole=Edge</c>, <c>Deployment__CloudBaseUrl=https://...</c>.
    /// </para>
    /// Offline-first / cloud-sync — Phase 1.
    /// </summary>
    public sealed class DeploymentOptions
    {
        public const string SectionName = "Deployment";

        /// <summary>
        /// Stable GUID for this deployment. When empty, the node id is read from
        /// (or written to) <see cref="NodeIdFilePath"/> so it survives restarts.
        /// </summary>
        public string NodeId { get; set; } = "";

        /// <summary><c>Cloud</c> or <c>Edge</c>. Defaults to <c>Edge</c>.</summary>
        public string NodeRole { get; set; } = "Edge";

        /// <summary>Stable GUID of the <see cref="Models.Branch"/> this node serves.</summary>
        public string BranchId { get; set; } = "";

        /// <summary>Base URL of the Cloud node's API (used by the Edge sync worker, Phase 5).</summary>
        public string CloudBaseUrl { get; set; } = "";

        /// <summary>Base URL of the Edge node's API (advertised to peers / used by the Cloud, Phase 5).</summary>
        public string EdgeBaseUrl { get; set; } = "";

        // ---- first-registration-only metadata (ignored once the rows exist) ----

        /// <summary>Branch display name used only when creating the Branch row for the first time.</summary>
        public string BranchName { get; set; } = "Main Branch";

        /// <summary>
        /// Branch short code used only when creating the Branch row. Falls back to
        /// <c>SiteSetting.OrderSerialPrefix</c> when blank.
        /// </summary>
        public string BranchCode { get; set; } = "";

        /// <summary>Human label for this node's <see cref="Models.SystemNode.DisplayName"/>.</summary>
        public string NodeDisplayName { get; set; } = "";

        /// <summary>
        /// When true (default), the API ensures a <see cref="Models.Branch"/> and
        /// <see cref="Models.SystemNode"/> row exist for this deployment on
        /// startup. Set false to require rows to be provisioned explicitly.
        /// </summary>
        public bool AutoRegisterNode { get; set; } = true;

        /// <summary>
        /// Where the generated node id is persisted when <see cref="NodeId"/> is
        /// not configured. Relative paths resolve against the content root. The
        /// edge compose mounts a persistent volume here.
        /// </summary>
        public string NodeIdFilePath { get; set; } = "keys/node-id.txt";

        public NodeRole ParsedRole =>
            string.Equals(NodeRole, "Cloud", System.StringComparison.OrdinalIgnoreCase)
                ? Models.NodeRole.Cloud
                : Models.NodeRole.Edge;
    }
}
