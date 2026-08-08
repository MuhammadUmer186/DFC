using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace RestaurantSystem.Models;

public partial class VendorPayment
{
    public int Id { get; set; }

    public int VendorId { get; set; }
    [Precision(18, 2)]
    public decimal AmountPaid { get; set; }

    public string PaidByUser { get; set; } = null!;

    public DateTime PaidOn { get; set; }

    public virtual Vendor Vendor { get; set; } = null!;
}
