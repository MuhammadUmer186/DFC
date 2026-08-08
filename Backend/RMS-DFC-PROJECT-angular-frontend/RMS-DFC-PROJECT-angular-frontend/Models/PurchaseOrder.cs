using System;
using System.Collections.Generic;

namespace RestaurantSystem.Models;

public partial class PurchaseOrder
{
    public int Id { get; set; }

    public string BillNo { get; set; } = null!;

    public DateTime PurchaseDate { get; set; }

    public int VendorId { get; set; }

    public decimal TotalAmount { get; set; }

    public virtual ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();

    public virtual Vendor Vendor { get; set; } = null!;
}
