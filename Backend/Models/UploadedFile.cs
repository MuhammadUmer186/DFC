using System;
using System.ComponentModel.DataAnnotations;
using RestaurantSystem.Sync;

namespace RestaurantSystem.Models
{
    /// <summary>
    /// Metadata for a file under <c>wwwroot/uploads</c> (Phase 12). The metadata
    /// synchronizes; the bytes are fetched on demand by SHA-256 over the sync
    /// blob endpoints. Existing public URLs (<c>/uploads/&lt;key&gt;</c>) are
    /// unchanged.
    /// </summary>
    public partial class UploadedFile : ISyncableAggregate
    {
        public long Id { get; set; }

        /// <summary>Path under <c>/uploads/</c>, e.g. <c>menu-items/item_3_ab12.png</c>. Stable, public.</summary>
        public string StorageKey { get; set; } = null!;

        public string? OriginalFileName { get; set; }
        public string ContentType { get; set; } = "application/octet-stream";
        public long Size { get; set; }

        /// <summary>Lowercase hex SHA-256 of the bytes — the dedupe / fetch key.</summary>
        public string Sha256Hash { get; set; } = null!;

        /// <summary>Logical bucket: <c>menu-items</c>, <c>categories</c>, <c>deals</c>, <c>site</c>.</summary>
        public string Category { get; set; } = "misc";

        /// <summary>pending | available | missing</summary>
        public string SyncState { get; set; } = "available";

        // ---- ISyncableAggregate ----
        public Guid GlobalId { get; set; }
        public Guid BranchId { get; set; }
        public Guid OriginNodeId { get; set; }
        public long AggregateVersion { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
