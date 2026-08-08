using System;
using System.Collections.Generic;

namespace RestaurantSystem.Models
{
    public partial class Order
    {
        public int Id { get; set; }

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

        public string? CustomerName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        public string OrderSource { get; set; } = "POS"; // POS / Online
        // ✅ PAYMENT INFO
        public DateTime? PaidAt { get; set; }
        public string? PaymentMethod { get; set; }

        public virtual ICollection<OrderDeal> OrderDeals { get; set; } = new List<OrderDeal>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
