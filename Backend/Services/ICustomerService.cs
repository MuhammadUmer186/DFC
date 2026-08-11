using RestaurantSystem.DTOs;

namespace RestaurantSystem.Services
{
    public interface ICustomerService
    {
        Task<List<CustomerDto>> GetAllAsync();
        Task<CustomerDto?> GetByIdAsync(int id);
        Task<CustomerDto?> GetByPhoneAsync(string phoneNumber);
        Task<CustomerDto> UpsertByPhoneAsync(UpsertCustomerDto dto);
        Task<bool> SetConsentAsync(string phoneNumber, bool consent);
    }
}
