using RestaurantSystem.DTOs;

namespace RestaurantSystem.Services
{
    public interface IProfitService
    {
        Task<ProfitDto> GetProfitAsync(DateTime from, DateTime to);
    }

}
