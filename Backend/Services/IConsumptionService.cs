using RestaurantSystem.DTOs;

namespace RestaurantSystem.Services
{
    public interface IConsumptionService
    {
        Task<List<ItemConsumptionDto>> GetConsumptionAsync(DateTime from, DateTime to);
    }

}
