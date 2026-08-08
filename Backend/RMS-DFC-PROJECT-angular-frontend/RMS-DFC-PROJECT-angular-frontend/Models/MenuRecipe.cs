using System;
using System.Collections.Generic;

namespace RestaurantSystem.Models;

public partial class MenuRecipe
{
    public int Id { get; set; }

    public int MenuItemId { get; set; }

    public int RawItemId { get; set; }
    
    public decimal QuantityRequired { get; set; }

    public virtual MenuItem MenuItem { get; set; } = null!;

    public virtual RawItem RawItem { get; set; } = null!;
}
