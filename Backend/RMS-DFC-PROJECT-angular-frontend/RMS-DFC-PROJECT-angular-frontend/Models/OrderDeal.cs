using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace RestaurantSystem.Models;

public partial class OrderDeal
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int DealId { get; set; }

    public int Quantity { get; set; }
    [Precision(18, 2)]
    public decimal DealPrice { get; set; }

    public virtual Deal Deal { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
