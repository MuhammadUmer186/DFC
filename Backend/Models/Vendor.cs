using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RestaurantSystem.Models;

public partial class Vendor
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    // Optional — same rationale as RawItem's safety-stock/shelf-life fields: used by the AI
    // inventory recommendation engine when available, warned-about when missing.
    public int? LeadTimeDays { get; set; }
    [Precision(18, 2)]
    public decimal? MinimumOrderQuantity { get; set; }

    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    public virtual ICollection<StoreStock> StoreStocks { get; set; } = new List<StoreStock>();

    public virtual ICollection<VendorPayment> VendorPayments { get; set; } = new List<VendorPayment>();
}
