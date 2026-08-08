using RestaurantSystem.DTOs;

namespace RestaurantSystem.Services
{
    public interface IRiderService
    {
        Task<List<RiderDto>> GetAllAsync();
        Task<RiderDto?> GetByIdAsync(int id);
        Task<RiderDto> CreateAsync(CreateRiderDto dto);
        Task<RiderDto?> UpdateAsync(int id, UpdateRiderDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
