using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RestaurantSystem.Models;

public partial class RawItem
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Unit { get; set; } = null!;

    // Optional — feed the AI inventory recommendation engine when set; the engine explicitly
    // warns rather than guessing when either is null (see AiInventoryRecommendation.DataWarnings).
    [Precision(18, 2)]
    public decimal? SafetyStockQuantity { get; set; }
    public int? ShelfLifeDays { get; set; }

    public virtual ICollection<KitchenOutItem> KitchenOutItems { get; set; } = new List<KitchenOutItem>();

    public virtual ICollection<MenuRecipe> MenuRecipes { get; set; } = new List<MenuRecipe>();

    public virtual ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();

    public virtual ICollection<StoreStock> StoreStocks { get; set; } = new List<StoreStock>();

    public virtual ICollection<WasteItem> WasteItems { get; set; } = new List<WasteItem>();
}
