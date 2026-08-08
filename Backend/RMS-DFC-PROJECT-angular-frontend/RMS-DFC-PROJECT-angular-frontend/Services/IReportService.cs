using RestaurantSystem.DTOs;

namespace RestaurantSystem.Services
{
    public interface IReportService
    {
        Task<ProfitReportResponseDto> GetProfitReportAsync(DateTime from, DateTime to);
        Task<List<DailySalaryReportDto>> GetDailySalaryReportAsync(DateOnly date);
        Task<(decimal totalPaid, decimal totalUnpaid)> GetSalaryTotalsAsync(DateOnly from, DateOnly? to = null);
        Task<DailyReportDto> GetDailyReportAsync(DateOnly date);
    }

}
