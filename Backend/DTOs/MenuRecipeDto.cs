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
}
