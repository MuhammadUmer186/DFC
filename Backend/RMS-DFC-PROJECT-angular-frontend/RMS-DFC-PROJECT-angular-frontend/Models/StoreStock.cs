using System;
using System.Collections.Generic;

namespace RestaurantSystem.Models;

public partial class StoreStock
{
    public int Id { get; set; }

    public int RawItemId { get; set; }

    public int VendorId { get; set; }

    public decimal Quantity { get; set; }

    public DateTime LastUpdated { get; set; }

    public virtual RawItem RawItem { get; set; } = null!;

    public virtual Vendor Vendor { get; set; } = null!;
}
