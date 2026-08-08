using System;
using System.Collections.Generic;

namespace RestaurantSystem.Models;

public partial class KitchenOutItem
{
    public int Id { get; set; }

    public int KitchenOutId { get; set; }

    public int RawItemId { get; set; }

    public decimal Quantity { get; set; }

    public virtual KitchenOut KitchenOut { get; set; } = null!;

    public virtual RawItem RawItem { get; set; } = null!;
}
