using RestaurantSystem.DTOs;
using RestaurantSystem.DTOs.RestaurantSystem.DTOs;

namespace RestaurantSystem.Interfaces
{
    public interface IUtilityBillService
    {
        Task<List<UtilityBillDto>> GetAllAsync();
        Task<UtilityBillDto?> GetByIdAsync(DateOnly Billdate);
        Task<UtilityBillDto> CreateAsync(UtilityBillDto dto);
        Task<UtilityBillDto?> UpdateAsync(int id, UtilityBillDto dto);
        Task<bool> DeleteAsync(int id);

        Task<BillsSummaryDto> GetBillsSummaryAsync();
    }
}
