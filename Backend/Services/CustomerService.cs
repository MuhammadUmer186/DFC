using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Models;

namespace RestaurantSystem.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CustomerDto>> GetAllAsync()
        {
            return await _context.Customers
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => ToDto(c))
                .ToListAsync();
        }

        public async Task<CustomerDto?> GetByIdAsync(int id)
        {
            var c = await _context.Customers.FindAsync(id);
            return c == null ? null : ToDto(c);
        }

        public async Task<CustomerDto?> GetByPhoneAsync(string phoneNumber)
        {
            var c = await _context.Customers.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);
            return c == null ? null : ToDto(c);
        }

        public async Task<CustomerDto> UpsertByPhoneAsync(UpsertCustomerDto dto)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == dto.PhoneNumber);
            if (customer == null)
            {
                customer = new Customer
                {
                    PhoneNumber = dto.PhoneNumber,
                    CreatedAt = DateTime.Now
                };
                _context.Customers.Add(customer);
            }

            customer.Name = dto.Name;
            customer.PersonalizationConsent = dto.PersonalizationConsent;
            customer.Allergens = dto.Allergens;
            customer.DietaryPreferences = dto.DietaryPreferences;
            customer.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return ToDto(customer);
        }

        public async Task<bool> SetConsentAsync(string phoneNumber, bool consent)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
            if (customer == null) return false;

            customer.PersonalizationConsent = consent;
            customer.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        private static CustomerDto ToDto(Customer c) => new()
        {
            Id = c.Id,
            PhoneNumber = c.PhoneNumber,
            Name = c.Name,
            PersonalizationConsent = c.PersonalizationConsent,
            Allergens = c.Allergens,
            DietaryPreferences = c.DietaryPreferences,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
    }
}
