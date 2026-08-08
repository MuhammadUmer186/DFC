using System;
using System.Collections.Generic;

namespace RestaurantSystem.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public string? ImageUrl { get; set; }

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}
