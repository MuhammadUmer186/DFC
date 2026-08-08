using RestaurantSystem.DTOs;

namespace RestaurantSystem.Services
{
    public interface IMenuProfitService
    {
        Task<List<MenuProfitDto>> GetMenuProfitAsync(DateTime from, DateTime to);
    }

}
