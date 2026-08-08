using System;
using System.Collections.Generic;

namespace RestaurantSystem.Models;

public partial class PurchaseOrderItem
{
    public int Id { get; set; }

    public int PurchaseOrderId { get; set; }

    public int RawItemId { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Quantity { get; set; }

    public decimal TotalPrice { get; set; }

    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;

    public virtual RawItem RawItem { get; set; } = null!;
}
