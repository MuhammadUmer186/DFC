namespace RestaurantSystem.DTOs
{
    public class MenuRecipeDto
    {
        public int RawItemId { get; set; }
        public decimal QuantityRequired { get; set; }
    }
    public class AssignMenuRecipeDto
    {
        public int MenuItemId { get; set; }
        public List<MenuRecipeDto> RecipeItems { get; set; } = new();
    }
    public class MenuRecipeResponseDto
    {
        public int RawItemId { get; set; }
        public string RawItemName { get; set; } = null!;
        public decimal QuantityRequired { get; set; }
        public string Unit { get; set; } = null!;
    }

    // ===== Recipe Module — category-wise overview =====
    public class RecipeOverviewItemDto
    {
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; } = null!;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public int IngredientCount { get; set; }   // 0 = no recipe yet
    }

    public class RecipeOverviewCategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public int ItemCount { get; set; }
        public int ItemsWithRecipe { get; set; }
        public List<RecipeOverviewItemDto> Items { get; set; } = new();
    }

    // ===== Recipe Module — kitchen ingredient audit (from sales) =====
    public class KitchenAuditRowDto
    {
        public int RawItemId { get; set; }
        public string RawItemName { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal ExpectedFromSales { get; set; }  // Σ recipe qty × units sold
        public decimal ActualConsumed { get; set; }     // Σ order-consumption kitchen-out in range
        public decimal Variance { get; set; }           // Actual − Expected
    }

    public class KitchenAuditDishDto
    {
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; } = null!;
        public decimal UnitsSold { get; set; }
        public bool HasRecipe { get; set; }
        public List<KitchenAuditRowDto> Ingredients { get; set; } = new();
    }

    public class KitchenAuditReportDto
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public int OrdersCounted { get; set; }
        public decimal LineUnitsCounted { get; set; }
        public int DishesWithoutRecipe { get; set; }
        public List<KitchenAuditRowDto> Totals { get; set; } = new();
        public List<KitchenAuditDishDto> ByDish { get; set; } = new();
    }
}
