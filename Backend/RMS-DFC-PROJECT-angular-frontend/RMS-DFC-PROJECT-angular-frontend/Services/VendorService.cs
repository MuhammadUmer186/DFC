using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Models;
using System;

namespace RestaurantSystem.Services
{
    public class VendorService : IVendorService
    {
        private readonly ApplicationDbContext _context;

        public VendorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Vendor>> GetAllAsync()
            => await _context.Vendors.ToListAsync();

        public async Task<Vendor> CreateAsync(VendorDto dto)
        {
            var vendor = new Vendor
            {
                Name = dto.Name,
                Phone = dto.Phone,
                Address = dto.Address
            };

            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();

            return vendor;
        }
    }

}
