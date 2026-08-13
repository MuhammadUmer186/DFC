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

        /// Called from online order creation. Only touches identity fields (name/address) —
        /// never overwrites PersonalizationConsent/Allergens/DietaryPreferences, since an order
        /// doesn't carry those and a full UpsertByPhoneAsync would silently wipe them out.
        Task UpsertFromOrderAsync(string phoneNumber, string? name, string? address);
    }
}
