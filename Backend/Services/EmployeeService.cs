using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Models;

namespace RestaurantSystem.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly  ApplicationDbContext _context;

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<EmployeeResponseDto>> GetAllAsync()
        {
            return await _context.Employees
                .Where(e => e.IsActive)
                .Select(e => new EmployeeResponseDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    MobileNumber = e.MobileNumber,
                    Designation = e.Designation,
                    NationalId = e.NationalId,
                    Address = e.Address,
                    SalaryType = e.SalaryType,
                    SalaryAmount = e.SalaryAmount,
                    IsActive = e.IsActive
                })
                .ToListAsync();
        }

        public async Task<EmployeeResponseDto?> GetByIdAsync(int id)
        {
            var e = await _context.Employees.FindAsync(id);
            if (e == null) return null;

            return new EmployeeResponseDto
            {
                Id = e.Id,
                Name = e.Name,
                MobileNumber = e.MobileNumber,
                Designation=e.Designation,
                NationalId = e.NationalId,
                Address = e.Address,
                SalaryType = e.SalaryType,
                SalaryAmount = e.SalaryAmount,
                IsActive = e.IsActive
            };
        }

        public async Task<EmployeeResponseDto> CreateAsync(CreateEmployeeDto dto)
        {
            var employee = new Employee
            {
                Name = dto.Name,
                MobileNumber = dto.MobileNumber,
                Designation = dto.Designation,
                NationalId = dto.NationalId,
                Address = dto.Address,
                SalaryType = dto.SalaryType,
                SalaryAmount = dto.SalaryAmount,
                IsActive = true
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(employee.Id);
        }

        public async Task<EmployeeResponseDto?> UpdateAsync(int id, UpdateEmployeeDto dto)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return null;

            employee.Name = dto.Name;
            employee.MobileNumber = dto.MobileNumber;
            employee.NationalId = dto.NationalId;
            employee.Designation = dto.Designation;
            employee.Address = dto.Address;
            employee.SalaryType = dto.SalaryType;
            employee.SalaryAmount = dto.SalaryAmount;
            employee.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return false;

            employee.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
