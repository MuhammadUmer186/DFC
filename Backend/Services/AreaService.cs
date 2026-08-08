using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Models;

namespace RestaurantSystem.Services
{
    public class AreaService : IAreaService
    {
        private readonly ApplicationDbContext _context;

        public AreaService(ApplicationDbContext context)
        {
            _context = context;
        }

        private static AreaDto ToDto(Area a) => new AreaDto
        {
            Id = a.Id,
            Name = a.Name,
            DeliveryFee = a.DeliveryFee,
            IsActive = a.IsActive,
            CreatedAt = a.CreatedAt
        };

        public async Task<List<AreaDto>> GetAllAsync()
        {
            return await _context.Areas
                .OrderBy(a => a.Name)
                .Select(a => new AreaDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    DeliveryFee = a.DeliveryFee,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<List<AreaDto>> GetActiveAsync()
        {
            return await _context.Areas
                .Where(a => a.IsActive)
                .OrderBy(a => a.Name)
                .Select(a => new AreaDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    DeliveryFee = a.DeliveryFee,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<AreaDto?> GetByIdAsync(int id)
        {
            var a = await _context.Areas.FindAsync(id);
            return a == null ? null : ToDto(a);
        }

        public async Task<AreaDto> CreateAsync(CreateAreaDto dto)
        {
            var area = new Area
            {
                Name = dto.Name,
                DeliveryFee = dto.DeliveryFee,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Areas.Add(area);
            await _context.SaveChangesAsync();

            return ToDto(area);
        }

        public async Task<AreaDto?> UpdateAsync(int id, UpdateAreaDto dto)
        {
            var area = await _context.Areas.FindAsync(id);
            if (area == null) return null;

            area.Name = dto.Name;
            area.DeliveryFee = dto.DeliveryFee;
            area.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return ToDto(area);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var area = await _context.Areas.FindAsync(id);
            if (area == null) return false;

            var hasOrders = await _context.Orders.AnyAsync(o => o.AreaId == id);
            if (hasOrders)
                throw new InvalidOperationException("This area has past orders and cannot be permanently deleted. Deactivate it instead.");

            _context.Areas.Remove(area);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
