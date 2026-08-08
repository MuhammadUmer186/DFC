using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace RestaurantSystem.Models;

public partial class UtilityBill
{
    public int Id { get; set; }

    public string BillType { get; set; } = null!;
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    public DateOnly BillDate { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}
