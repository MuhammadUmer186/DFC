using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace RestaurantSystem.Models;

public partial class Deal
{
    public int Id { get; set; }

    public string DealName { get; set; } = null!;
    [Precision(18,2)]
    public decimal OriginalPrice { get; set; }
    [Precision(18, 2)]
    public decimal DiscountAmount { get; set; }
    [Precision(18, 2)]
    public decimal FinalPrice { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ICollection<DealItem> DealItems { get; set; } = new List<DealItem>();

    public virtual ICollection<OrderDeal> OrderDeals { get; set; } = new List<OrderDeal>();
}
