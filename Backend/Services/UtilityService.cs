using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.DTOs.RestaurantSystem.DTOs;
using RestaurantSystem.Interfaces;
using RestaurantSystem.Models;

namespace RestaurantSystem.Services
{
    public class UtilityBillService : IUtilityBillService
    {
        private readonly ApplicationDbContext _context;

        public UtilityBillService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<UtilityBillDto>> GetAllAsync()
        {
            return await _context.UtilityBills
                .OrderByDescending(b => b.BillDate)
                .Select(b => new UtilityBillDto
                {
                    Id = b.Id,
                    BillType = b.BillType,
                    Amount = b.Amount,
                    BillDate = b.BillDate,
                    Notes = b.Notes
                })
                .ToListAsync();
        }

        public async Task<UtilityBillDto?> GetByIdAsync(DateOnly Billdate)
        {
            return await _context.UtilityBills
                .Where(b => b.BillDate == Billdate)
                .Select(b => new UtilityBillDto
                {
                    Id = b.Id,
                    BillType = b.BillType,
                    Amount = b.Amount,
                    BillDate = b.BillDate,
                    Notes = b.Notes
                })
                .FirstOrDefaultAsync();
        }

        public async Task<UtilityBillDto> CreateAsync(UtilityBillDto dto)
        {
            var bill = new UtilityBill
            {
                BillType = dto.BillType,
                Amount = dto.Amount,
                BillDate = dto.BillDate,
                Notes = dto.Notes
            };

            _context.UtilityBills.Add(bill);
            await _context.SaveChangesAsync();

            dto.Id = bill.Id;
            return dto;
        }

        public async Task<UtilityBillDto?> UpdateAsync(int id, UtilityBillDto dto)
        {
            var bill = await _context.UtilityBills.FindAsync(id);
            if (bill == null) return null;

            bill.BillType = dto.BillType;
            bill.Amount = dto.Amount;
            bill.BillDate = dto.BillDate;
            bill.Notes = dto.Notes;

            await _context.SaveChangesAsync();
            dto.Id = bill.Id;
            return dto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var bill = await _context.UtilityBills.FindAsync(id);
            if (bill == null) return false;

            _context.UtilityBills.Remove(bill);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<BillsSummaryDto> GetBillsSummaryAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var weekStart = today.AddDays(-7);
            var monthStart = new DateOnly(today.Year, today.Month, 1);

            return new BillsSummaryDto
            {
                TodayTotal = await _context.UtilityBills
                    .Where(b => b.BillDate == today)
                    .SumAsync(b => (decimal?)b.Amount) ?? 0,

                WeeklyTotal = await _context.UtilityBills
                    .Where(b => b.BillDate >= weekStart)
                    .SumAsync(b => (decimal?)b.Amount) ?? 0,

                MonthlyTotal = await _context.UtilityBills
                    .Where(b => b.BillDate >= monthStart)
                    .SumAsync(b => (decimal?)b.Amount) ?? 0,

                OverallTotal = await _context.UtilityBills
                    .SumAsync(b => (decimal?)b.Amount) ?? 0
            };
        }
    }
}
