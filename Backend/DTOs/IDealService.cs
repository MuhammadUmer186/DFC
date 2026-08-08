namespace RestaurantSystem.DTOs
{
    public interface IDealService
    {
        Task<List<DealResponseDto>> GetDealsAsync();
        Task<List<DealResponseDto>> GetActiveDealsAsync();
        Task<DealResponseDto> CreateDealAsync(CreateDealDto dto);
        Task<DealResponseDto> UpdateDealAsync(int id, CreateDealDto dto);
        Task<bool> DeleteDealAsync(int id);
        Task<string> SetDealImageUrlAsync(int id, string imageUrl);
        Task<DealResponseDto> ToggleActiveAsync(int id);
    }

}
