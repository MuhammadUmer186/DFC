using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Models;

namespace RestaurantSystem.Services
{
    public class SalaryService : ISalaryService
    {
        private readonly ApplicationDbContext _context;

        public SalaryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> PaySalaryAsync(PaySalaryDto dto)
        {
            bool alreadyPaid = await IsSalaryPaidAsync(
                dto.EmployeeId,
                dto.SalaryType,
                dto.ForDate,
                dto.ForMonth
            );

            if (alreadyPaid)
                throw new Exception("Salary already paid for this period.");

            var payment = new SalaryPayment
            {
                EmployeeId = dto.EmployeeId,
                SalaryType = dto.SalaryType,
                AmountPaid = dto.AmountPaid,
                ForDate = dto.ForDate,
                ForMonth = dto.ForMonth,
                Remarks = dto.Remarks
            };

            _context.SalaryPayments.Add(payment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<SalaryPaymentResponseDto>> GetEmployeePaymentsAsync(int employeeId)
        {
            return await _context.SalaryPayments
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.PaidAt)
                .Select(x => new SalaryPaymentResponseDto
                {
                    Id = x.Id,
                    AmountPaid = x.AmountPaid,
                    PaidAt = x.PaidAt,
                    ForDate = x.ForDate,
                    ForMonth = x.ForMonth
                })
                .ToListAsync();
        }

        public async Task<bool> IsSalaryPaidAsync(int employeeId, SalaryType type, DateOnly? date, string? month)
        {
            return await _context.SalaryPayments.AnyAsync(x =>
                x.EmployeeId == employeeId &&
                x.SalaryType == type &&
                (type == SalaryType.Daily && x.ForDate == date ||
                 type == SalaryType.Monthly && x.ForMonth == month)
            );
        }

        public async Task<List<EmployeeSalaryStatusDto>> GetSalaryStatusAsync(DateOnly date)
        {
            var employees = await _context.Employees
                .Where(e => e.IsActive)
                .ToListAsync();

            var result = new List<EmployeeSalaryStatusDto>();

            foreach (var emp in employees)
            {
                if (emp.SalaryType == SalaryType.Daily)
                {
                    bool paid = await _context.SalaryPayments
                        .AnyAsync(x => x.EmployeeId == emp.Id &&
                                       x.SalaryType == SalaryType.Daily &&
                                       x.ForDate == date);

                    result.Add(new EmployeeSalaryStatusDto
                    {
                        EmployeeId = emp.Id,
                        EmployeeName = emp.Name,
                        MobileNumber = emp.MobileNumber,
                        SalaryType = (int)SalaryType.Daily,
                        SalaryAmount = emp.SalaryAmount,
                        IsPaid = paid,
                        ForDate = date
                    });
                }
                else
                {
                    string month = date.ToString("yyyy-MM");

                    bool paid = await _context.SalaryPayments
                        .AnyAsync(x => x.EmployeeId == emp.Id &&
                                       x.SalaryType == SalaryType.Monthly &&
                                       x.ForMonth == month);

                    result.Add(new EmployeeSalaryStatusDto
                    {
                        EmployeeId = emp.Id,
                        EmployeeName = emp.Name,
                        MobileNumber = emp.MobileNumber,
                        SalaryType = (int)SalaryType.Monthly,
                        SalaryAmount = emp.SalaryAmount,
                        IsPaid = paid,
                        ForMonth = month
                    });
                }
            }

            return result;
        }

        // Pay salary (daily or monthly)
        public async Task<bool> PaySalaryAsync(int employeeId, decimal amount, SalaryType type, DateOnly? date = null, string? month = null)
        {
            bool alreadyPaid = await IsSalaryPaidAsync(employeeId, type, date, month);
            if (alreadyPaid)
                throw new Exception("Salary already paid for this period.");

            var payment = new SalaryPayment
            {
                EmployeeId = employeeId,
                SalaryType = type,
                AmountPaid = amount,
                ForDate = date,
                ForMonth = month
            };

            _context.SalaryPayments.Add(payment);
            await _context.SaveChangesAsync();
            return true;
        }


    } 
}
