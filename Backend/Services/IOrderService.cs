using RestaurantSystem.DTOs;
using RestaurantSystem.Models;
using System.Security.Claims;

namespace RestaurantSystem.Services
{
    public interface IOrderService
    {
        Task<OrderDto> CreateAsync(CreateOrderRequest request, decimal discount, ClaimsPrincipal userClaims, bool skipStockCheck = false);
        Task<OrderDto> GetByIdAsync(int id);
        Task<OrderDto> CreateOnlineOrderAsync(PublicOrderRequest request);
        Task<List<OrderDto>> GetPendingOnlineOrdersAsync();
        Task<ApproveOrderResult> ApproveOnlineOrderAsync(int orderId, ClaimsPrincipal userClaims);
        Task<OrderDto> RejectOnlineOrderAsync(int orderId, string? reason, ClaimsPrincipal userClaims);
        Task<List<OrderDto>> GetActiveOnlineOrdersAsync();
        Task<OrderDto> UpdateDeliveryStatusAsync(int orderId, DeliveryStatus newStatus, ClaimsPrincipal userClaims);
        Task<OrderDto> AssignRiderAsync(int orderId, AssignRiderDto dto, ClaimsPrincipal userClaims);
        Task<List<OrderDto>> GetMyDeliveriesAsync(ClaimsPrincipal userClaims);
        Task<bool> ReprintDeliverySlipAsync(int orderId, string copy);
        Task<PublicOrderStatusDto> GetPublicOrderStatusAsync(int orderId, string phone);
        Task<List<OrderDto>> GetAllAsync();
        Task<PagedResult<OrderDto>> GetPagedAsync(int page = 0, int pageSize = 5);
        Task<List<OrderDto>> GetQueuedOrdersAsync();
        Task<OrderDto> PayOrderAsync(PayOrderRequest request,ClaimsPrincipal userClaims);
        Task<OrderDto> CancelOrderAsync(int orderId, ClaimsPrincipal userClaims);
        Task<bool> CancelOrderAsync(CancelOrderRequest request);
        Task<List<OrderDto>> GetByDateAsync(DateOnly date);
        Task<PagedResult<OrderDto>> GetPagedAsync(DateOnly? date, int page = 0, int pageSize = 5);
    }

}
