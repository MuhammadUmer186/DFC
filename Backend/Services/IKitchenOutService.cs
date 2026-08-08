using RestaurantSystem.DTOs;
using RestaurantSystem.DTOs.RestaurantSystem.Dtos.Reports;

namespace RestaurantSystem.Services
{
    public interface IKitchenOutService
    {
        Task CreateAsync(KitchenOutCreateDto dto);
        Task<List<KitchenOutListDto>> GetAllAsync();
        Task<PagedResult<KitchenOutListDto>> GetPagedByDateAsync(DateOnly date, int page = 0, int pageSize = 5);
        Task<List<RawItemAvgCostDto>> GetAverageCostsAsync();
        Task<DailyKitchenCostDto> GetDailyKitchenCostAsync(DateOnly date);
        Task<MonthlyKitchenCostDto> GetMonthlyKitchenCostAsync(int year, int month);
        Task<List<KitchenInventoryDto>> GetCurrentKitchenInventoryAsync();
        Task ConsumeAsync(Dictionary<int, decimal> rawItemConsumption, DateTime issuedAt, int? employeeId);
    }
}
