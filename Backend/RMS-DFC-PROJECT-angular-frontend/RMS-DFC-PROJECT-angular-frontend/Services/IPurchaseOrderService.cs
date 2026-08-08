using RestaurantSystem.DTOs;

namespace RestaurantSystem.Services
{
    public interface IPurchaseOrderService
    {
        Task CreateAsync(PurchaseOrderCreateDto dto);
        Task<List<PurchaseOrderListDto>> GetAllAsync();
        Task<List<PurchaseOrderListDto>> GetByDateAsync(DateOnly date);
    }
}
