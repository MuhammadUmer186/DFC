using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Models;

namespace RestaurantSystem.Services
{
    public class RiderService : IRiderService
    {
        private readonly ApplicationDbContext _context;

        public RiderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RiderDto>> GetAllAsync()
        {
            return await _context.Riders
                .OrderBy(r => r.Name)
                .Select(r => new RiderDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    PhoneNumber = r.PhoneNumber,
                    VehicleNumber = r.VehicleNumber,
                    IsActive = r.IsActive,
                    CreatedAt = r.CreatedAt,
                    ActiveDeliveryCount = r.Orders.Count(o => o.Status == OrderStatus.Queued)
                })
                .ToListAsync();
        }

        public async Task<RiderDto?> GetByIdAsync(int id)
        {
            var r = await _context.Riders.FindAsync(id);
            if (r == null) return null;

            return new RiderDto
            {
                Id = r.Id,
                Name = r.Name,
                PhoneNumber = r.PhoneNumber,
                VehicleNumber = r.VehicleNumber,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt,
                ActiveDeliveryCount = await _context.Orders.CountAsync(o => o.RiderId == id && o.Status == OrderStatus.Queued)
            };
        }

        public async Task<RiderDto> CreateAsync(CreateRiderDto dto)
        {
            var rider = new Rider
            {
                Name = dto.Name,
                PhoneNumber = dto.PhoneNumber,
                VehicleNumber = dto.VehicleNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Riders.Add(rider);
            await _context.SaveChangesAsync();

            return (await GetByIdAsync(rider.Id))!;
        }

        public async Task<RiderDto?> UpdateAsync(int id, UpdateRiderDto dto)
        {
            var rider = await _context.Riders.FindAsync(id);
            if (rider == null) return null;

            rider.Name = dto.Name;
            rider.PhoneNumber = dto.PhoneNumber;
            rider.VehicleNumber = dto.VehicleNumber;
            rider.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var rider = await _context.Riders.FindAsync(id);
            if (rider == null) return false;

            var hasOrders = await _context.Orders.AnyAsync(o => o.RiderId == id);
            if (hasOrders)
                throw new InvalidOperationException("This rider has past orders and cannot be permanently deleted. Deactivate them instead.");

            var hasLogin = await _context.Users.AnyAsync(u => u.RiderId == id);
            if (hasLogin)
                throw new InvalidOperationException("This rider still has a login account. Delete their login first, then delete the rider.");

            _context.Riders.Remove(rider);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
