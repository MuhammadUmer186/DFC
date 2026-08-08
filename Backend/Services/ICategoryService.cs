using RestaurantSystem.DTOs;

public interface ICategoryService
{
    Task<List<CategoryResponseDto>> GetAllAsync();
    Task<CategoryResponseDto> GetByIdAsync(int id);
    Task CreateAsync(CatDto dto);
    Task UpdateAsync(int id, CatDto dto);
    Task DeleteAsync(int id);
    Task<string> SetImageUrlAsync(int id, string imageUrl);
}
