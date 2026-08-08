namespace RestaurantSystem.DTOs
{
    public interface IDealService
    {
        Task<List<DealResponseDto>> GetDealsAsync();
        Task<DealResponseDto> CreateDealAsync(CreateDealDto dto);
        Task<DealResponseDto> UpdateDealAsync(int id, CreateDealDto dto);
        Task<bool> DeleteDealAsync(int id);
    }

}
