using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace RestaurantSystem.Models;

public partial class Employee
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string MobileNumber { get; set; } = null!;

    public string NationalId { get; set; } = null!;
    public string Designation { get; set; }

    public string Address { get; set; } = null!;

    public SalaryType SalaryType { get; set; }
    [Precision(18, 2)]
    public decimal SalaryAmount { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<SalaryPayment> SalaryPayments { get; set; } = new List<SalaryPayment>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
