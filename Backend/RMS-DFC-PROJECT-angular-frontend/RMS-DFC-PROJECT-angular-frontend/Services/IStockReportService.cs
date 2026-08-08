using RestaurantSystem.DTOs;

namespace RestaurantSystem.Services
{
    public interface IStockReportService
    {
        Task<List<StockSummaryDto>> GetStockSummaryAsync();
        Task<List<VendorStockDto>> GetVendorWiseStockAsync();
    }


}
