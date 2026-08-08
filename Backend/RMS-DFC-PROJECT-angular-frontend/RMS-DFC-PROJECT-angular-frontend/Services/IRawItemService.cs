using RestaurantSystem.DTOs;
using RestaurantSystem.Models;

namespace RestaurantSystem.Services
{
    public interface IRawItemService
    {
        Task<List<RawItem>> GetAllAsync();
        Task<RawItem> CreateAsync(RawItemDto dto);
    }

}
