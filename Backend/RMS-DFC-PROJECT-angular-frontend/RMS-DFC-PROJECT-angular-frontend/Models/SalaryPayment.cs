using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace RestaurantSystem.Models;

public partial class SalaryPayment
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public SalaryType SalaryType { get; set; }
    [Precision(18, 2)]
    public decimal AmountPaid { get; set; }

    public DateOnly? ForDate { get; set; }

    public string? ForMonth { get; set; }

    public DateTime PaidAt { get; set; }

    public string Remarks { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;
}
