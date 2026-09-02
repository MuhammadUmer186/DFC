using System;
using System.ComponentModel.DataAnnotations;
using RestaurantSystem.Sync;

namespace RestaurantSystem.Models
{
    /// <summary>
    /// An immutable inventory-ledger entry (Phase 4). Stock on hand is the sum of
    /// <see cref="QuantityDelta"/> per raw item / vendor; <see cref="StoreStock"/>
    /// is only a rebuildable projection of this. Never updated or deleted — a
    /// reversal is a new compensating row referencing the original.
    /// <para>Synchronized: merges append-only across nodes; balances are never
    /// copied between nodes.</para>
    /// </summary>
    public partial class StockMovement : ISyncableAggregate
    {
        public long Id { get; set; }

        public StockMovementType MovementType { get; set; }

        public int RawItemId { get; set; }
        public Guid RawItemGlobalId { get; set; }

        /// <summary>Nullable — kitchen-out / consumption is not vendor-specific.</summary>
        public int? VendorId { get; set; }
        public Guid? VendorGlobalId { get; set; }

        /// <summary>Signed: positive = stock in, negative = stock out.</summary>
        public decimal QuantityDelta { get; set; }

        /// <summary>What caused this movement, e.g. <c>Order</c>, <c>PurchaseOrder</c>, <c>WasteRecord</c>, <c>KitchenOut</c>, <c>OpeningBalance</c>, <c>OrderCancellation</c>.</summary>
        public string ReferenceType { get; set; } = null!;
        public Guid ReferenceGlobalId { get; set; }

        /// <summary>GlobalId of the compensated movement, when this is a reversal.</summary>
        public Guid? ReversesMovementGlobalId { get; set; }

        public DateTime OccurredAtUtc { get; set; }
        public Guid? CreatedByUserGlobalId { get; set; }

        // ---- ISyncableAggregate (append-only: AggregateVersion stays 1) ----
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
