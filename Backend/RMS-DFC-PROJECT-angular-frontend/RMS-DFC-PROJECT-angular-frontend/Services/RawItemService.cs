using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Models;
using System;

namespace RestaurantSystem.Services
{
    public class RawItemService : IRawItemService
    {
        private readonly ApplicationDbContext _context;

        public RawItemService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RawItem>> GetAllAsync()
            => await _context.RawItems.ToListAsync();

        public async Task<RawItem> CreateAsync(RawItemDto dto)
        {
            var item = new RawItem
            {
                Name = dto.Name,
                Unit = dto.Unit
            };

            _context.RawItems.Add(item);
            await _context.SaveChangesAsync();

            return item;
        }
    }

}
