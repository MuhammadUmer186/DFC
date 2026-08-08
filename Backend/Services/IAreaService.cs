using RestaurantSystem.DTOs;

namespace RestaurantSystem.Services
{
    public interface IAreaService
    {
        Task<List<AreaDto>> GetAllAsync();
        Task<List<AreaDto>> GetActiveAsync();
        Task<AreaDto?> GetByIdAsync(int id);
        Task<AreaDto> CreateAsync(CreateAreaDto dto);
        Task<AreaDto?> UpdateAsync(int id, UpdateAreaDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
