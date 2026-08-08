using RestaurantSystem.DTOs;
using RestaurantSystem.Models;

namespace RestaurantSystem.Services
{
    public interface ISalaryService
    {
        Task<bool> PaySalaryAsync(PaySalaryDto dto);
        Task<List<SalaryPaymentResponseDto>> GetEmployeePaymentsAsync(int employeeId);
        Task<bool> IsSalaryPaidAsync(int employeeId, SalaryType type, DateOnly? date, string? month);
        Task<List<EmployeeSalaryStatusDto>> GetSalaryStatusAsync(DateOnly date);
        Task<bool> PaySalaryAsync(int employeeId, decimal amount, SalaryType type, DateOnly? date = null, string? month = null);
    }

}
