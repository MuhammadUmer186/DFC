using System;
using System.Collections.Generic;

namespace RestaurantSystem.Models
{
    public partial class Order
    {
        public int Id { get; set; }

        // ✅ Customer-facing order number (Prefix + daily-resetting serial), e.g. "DFC-0007"
        public string? OrderNumber { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool Paid { get; set; }

        // ✅ ORDER FLOW
        public OrderStatus Status { get; set; }

        // ✅ WHO TOOK THE ORDER (WAITER)
        public int? TakenByEmployeeId { get; set; }
        public Employee? TakenByEmployee { get; set; }

        // ✅ WHO RECEIVED PAYMENT (CASHIER)
        public int? CashierId { get; set; }
        public Employee? Cashier { get; set; }
        public DateTime? CancelledAt { get; set; }
        public int? CancelledByEmployeeId { get; set; }
        public string? CancelledByUserName { get; set; }
        public string? RejectReason { get; set; }

        // ✅ Login username of whoever finalized payment (works for Cashier employees
        // and role-only accounts like Admin/SuperAdmin who have no Employee record)
        public string? CashierUserName { get; set; }

        public string? CustomerName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public string OrderSource { get; set; } = "POS"; // POS / Online
        public string ServiceType { get; set; } = "Delivery"; // DineIn / Takeaway / Delivery
        // ✅ PAYMENT INFO
        public DateTime? PaidAt { get; set; }
        public string? PaymentMethod { get; set; }

        // ✅ RIDER (DELIVERY) INFO
        public int? RiderId { get; set; }
        public Rider? Rider { get; set; }
        public decimal? RiderCost { get; set; }

        // ✅ DELIVERY AREA / FEE (snapshotted at order time)
        public int? AreaId { get; set; }
        public Area? Area { get; set; }
        public decimal? DeliveryFeeCharged { get; set; }

        // ✅ ONLINE ORDER FULFILLMENT TRACKING (separate from Status, which drives kitchen/payment)
        public DeliveryStatus? DeliveryStatus { get; set; }

        public virtual ICollection<OrderDeal> OrderDeals { get; set; } = new List<OrderDeal>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
