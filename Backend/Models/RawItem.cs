using System;
using System.Collections.Generic;

namespace RestaurantSystem.Models;

public partial class RawItem
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public virtual ICollection<KitchenOutItem> KitchenOutItems { get; set; } = new List<KitchenOutItem>();

    public virtual ICollection<MenuRecipe> MenuRecipes { get; set; } = new List<MenuRecipe>();

    public virtual ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();

    public virtual ICollection<StoreStock> StoreStocks { get; set; } = new List<StoreStock>();

    public virtual ICollection<WasteItem> WasteItems { get; set; } = new List<WasteItem>();
}
