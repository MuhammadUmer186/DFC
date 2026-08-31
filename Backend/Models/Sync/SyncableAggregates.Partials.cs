using System;
using System.ComponentModel.DataAnnotations;
using RestaurantSystem.Sync;

// Offline-first / cloud-sync — Phase 2.
// Additive partial-class fragments that attach the synchronization identity
// columns to the prioritized aggregates WITHOUT editing the original entity
// files. Column mapping (unique GlobalId index, rowversion) is applied centrally
// in ApplicationDbContext.ApplySyncConventions().

namespace RestaurantSystem.Models
{
    // ---- Aggregate roots -----------------------------------------------------

    public partial class Order : ISyncableAggregate
    {
        public Guid GlobalId { get; set; }
        public Guid BranchId { get; set; }
        public Guid OriginNodeId { get; set; }
        public long AggregateVersion { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public partial class Customer : ISyncableAggregate
    {
        public Guid GlobalId { get; set; }
        public Guid BranchId { get; set; }
        public Guid OriginNodeId { get; set; }
        public long AggregateVersion { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public partial class Area : ISyncableAggregate
    {
        public Guid GlobalId { get; set; }
        public Guid BranchId { get; set; }
        public Guid OriginNodeId { get; set; }
        public long AggregateVersion { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public partial class MenuItem : ISyncableAggregate
    {
        public Guid GlobalId { get; set; }
        public Guid BranchId { get; set; }
        public Guid OriginNodeId { get; set; }
        public long AggregateVersion { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public partial class Category : ISyncableAggregate
    {
        public Guid GlobalId { get; set; }
        public Guid BranchId { get; set; }
        public Guid OriginNodeId { get; set; }
        public long AggregateVersion { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public partial class Deal : ISyncableAggregate
    {
        public Guid GlobalId { get; set; }
        public Guid BranchId { get; set; }
        public Guid OriginNodeId { get; set; }
        public long AggregateVersion { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public partial class RawItem : ISyncableAggregate
    {
        public Guid GlobalId { get; set; }
        public Guid BranchId { get; set; }
        public Guid OriginNodeId { get; set; }
        public long AggregateVersion { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public partial class Vendor : ISyncableAggregate
    {
        public Guid GlobalId { get; set; }
        public Guid BranchId { get; set; }
        public Guid OriginNodeId { get; set; }
        public long AggregateVersion { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public partial class PurchaseOrder : ISyncableAggregate
    {
        public Guid GlobalId { get; set; }
        public Guid BranchId { get; set; }
        public Guid OriginNodeId { get; set; }
        public long AggregateVersion { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public partial class KitchenOut : ISyncableAggregate
    {
        public Guid GlobalId { get; set; }
        public Guid BranchId { get; set; }
        public Guid OriginNodeId { get; set; }
        public long AggregateVersion { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public partial class WasteRecord : ISyncableAggregate
    {
        public Guid GlobalId { get; set; }
        public Guid BranchId { get; set; }
        public Guid OriginNodeId { get; set; }
        public long AggregateVersion { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public partial class User : ISyncableAggregate
    {
        public Guid GlobalId { get; set; }
        public Guid BranchId { get; set; }
        public Guid OriginNodeId { get; set; }
        public long AggregateVersion { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public partial class SiteSetting : ISyncableAggregate
    {
        public Guid GlobalId { get; set; }
        public Guid BranchId { get; set; }
        public Guid OriginNodeId { get; set; }
        public long AggregateVersion { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public partial class ServiceTimeSetting : ISyncableAggregate
    {
        public Guid GlobalId { get; set; }
        public Guid BranchId { get; set; }
        public Guid OriginNodeId { get; set; }
        public long AggregateVersion { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public partial class Rider : ISyncableAggregate
    {
        public Guid GlobalId { get; set; }
        public Guid BranchId { get; set; }
        public Guid OriginNodeId { get; set; }
        public long AggregateVersion { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    // ---- Owned children (sync inside their root's snapshot) -----------------

    public partial class OrderItem : ISyncableChild
    {
        public Guid GlobalId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    public partial class OrderDeal : ISyncableChild
    {
        public Guid GlobalId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    public partial class DealItem : ISyncableChild
    {
        public Guid GlobalId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    public partial class MenuRecipe : ISyncableChild
    {
        public Guid GlobalId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    public partial class PurchaseOrderItem : ISyncableChild
    {
        public Guid GlobalId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    public partial class KitchenOutItem : ISyncableChild
    {
        public Guid GlobalId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    public partial class WasteItem : ISyncableChild
    {
        public Guid GlobalId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
