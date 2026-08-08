using RestaurantSystem.DTOs;

namespace RestaurantSystem.Services
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSalesSummaryAsync();
        Task<List<DateAmountDto>> GetSalesByDateRangeAsync(DateOnly from, DateOnly to);

        Task<DashboardSummaryDto> GetPurchaseSummaryAsync();
        Task<List<DateAmountDto>> GetPurchasesByDateRangeAsync(DateOnly from, DateOnly to);
        Task<DashboardMainSummaryDto> GetMainSummaryAsync();
        Task<List<StockSummaryDto>> GetStockSummaryAsync();
        Task<List<VendorStockDto>> GetVendorStockAsync();
        Task<ProfitDto> GetProfitAsync(DateOnly from, DateOnly to);
        Task<List<ProfitDto>> GetDailyProfitAsync(DateOnly from, DateOnly to);
        Task<ProfitSummaryDto> GetProfitSummaryAsync();
        Task<List<VendorAccountSummaryDto>> GetVendorPaymentSummaryAsync();
        Task<List<CategorySalesDto>> GetTopSellingCategoriesAsync();
        Task<OrderCountSummaryDto> GetOrderCountSummaryAsync();
        Task<StockUsagePercentageDto> GetStockUsagePercentageAsync();
        Task<DashboardDto> GetUtilities();
    }

}
