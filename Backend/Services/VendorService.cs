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
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new Exception("Vendor name is required");

            var nameExists = await _context.Vendors
                .AnyAsync(v => v.Name.ToLower() == dto.Name.ToLower());
            if (nameExists)
                throw new Exception("A vendor with this name already exists");

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

        public async Task<Vendor> UpdateAsync(int id, VendorDto dto)
        {
            var vendor = await _context.Vendors.FindAsync(id);
            if (vendor == null)
                throw new Exception("Vendor not found");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new Exception("Vendor name is required");

            var nameExists = await _context.Vendors
                .AnyAsync(v => v.Id != id && v.Name.ToLower() == dto.Name.ToLower());
            if (nameExists)
                throw new Exception("A vendor with this name already exists");

            vendor.Name = dto.Name;
            vendor.Phone = dto.Phone;
            vendor.Address = dto.Address;

            await _context.SaveChangesAsync();
            return vendor;
        }

        public async Task DeleteAsync(int id)
        {
            var vendor = await _context.Vendors.FindAsync(id);
            if (vendor == null)
                throw new Exception("Vendor not found");

            var hasPurchaseOrders = await _context.PurchaseOrders.AnyAsync(p => p.VendorId == id);
            var hasStoreStock = await _context.StoreStocks.AnyAsync(s => s.VendorId == id);
            var hasPayments = await _context.VendorPayments.AnyAsync(p => p.VendorId == id);

            if (hasPurchaseOrders || hasStoreStock || hasPayments)
                throw new Exception("Cannot delete vendor with existing purchase orders, stock, or payment history");

            _context.Vendors.Remove(vendor);
            await _context.SaveChangesAsync();
        }
    }

}
