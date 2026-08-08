namespace RestaurantSystem.DTOs
{
    public class MenuProfitDto
    {
        public string MenuName { get; set; }
        public decimal SalesAmount { get; set; }
        public decimal CostAmount { get; set; }
        public decimal Profit => SalesAmount - CostAmount;
    }

}
