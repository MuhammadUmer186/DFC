using System;
using System.Collections.Generic;

namespace RestaurantSystem.Models;

public partial class MenuItem
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public int CategoryId { get; set; }
    public string? ImageUrl { get; set; } // ⭐ for website / POS images
    public string? Description { get; set; } // ⭐ shown on the online menu
    public bool IsAvailable { get; set; } = true; // ⭐ show/hide item
    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<DealItem> DealItems { get; set; } = new List<DealItem>();

    public virtual ICollection<MenuRecipe> MenuRecipes { get; set; } = new List<MenuRecipe>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
