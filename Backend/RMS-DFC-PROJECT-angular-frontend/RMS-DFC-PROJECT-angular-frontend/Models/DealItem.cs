using System;
using System.Collections.Generic;

namespace RestaurantSystem.Models;

public partial class DealItem
{
    public int Id { get; set; }

    public int DealId { get; set; }

    public int MenuItemId { get; set; }

    public int Quantity { get; set; }

    public virtual Deal Deal { get; set; } = null!;

    public virtual MenuItem MenuItem { get; set; } = null!;
}
