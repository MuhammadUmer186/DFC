using RestaurantSystem.DTOs;
using RestaurantSystem.Models;

namespace RestaurantSystem.Services
{
    public interface IVendorService
    {
        Task<List<Vendor>> GetAllAsync();
        Task<Vendor> CreateAsync(VendorDto dto);
        Task<Vendor> UpdateAsync(int id, VendorDto dto);
        Task DeleteAsync(int id);
    }
}
