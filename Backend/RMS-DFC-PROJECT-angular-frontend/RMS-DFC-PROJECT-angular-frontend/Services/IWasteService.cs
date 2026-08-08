using RestaurantSystem.DTOs;

namespace RestaurantSystem.Services
{
    public interface IWasteService
    {
        Task<WasteResponseDto> CreateWasteAsync(WasteCreateRequestDto request);
        Task<List<WasteRecordDto>> GetAllAsync();
        Task<List<WasteRecordDto>> GetByDateAsync(DateOnly date);
    }

}
