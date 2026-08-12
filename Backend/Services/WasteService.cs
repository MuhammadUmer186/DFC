namespace RestaurantSystem.Services
{
    using Microsoft.EntityFrameworkCore;
    using RestaurantSystem.Data;
    using RestaurantSystem.DTOs;
    using RestaurantSystem.Models;

    public class WasteService : IWasteService
    {
        private readonly ApplicationDbContext _context;

        public WasteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<WasteResponseDto> CreateWasteAsync(WasteCreateRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var waste = new WasteRecord
                {
                    WasteDate = DateTime.UtcNow,
                    Reason = request.Reason
                };

                _context.WasteRecords.Add(waste);
                await _context.SaveChangesAsync();

                foreach (var item in request.Items)
                {
                    var stock = await _context.StoreStocks
                        .FirstOrDefaultAsync(s => s.RawItemId == item.RawItemId);

                    if (stock == null)
                        throw new Exception($"No stock found for RawItemId: {item.RawItemId}");

                    if (stock.Quantity < item.Quantity)
                        throw new Exception($"Not enough stock for RawItemId: {item.RawItemId}. Available: {stock.Quantity}");

                    // Deduct Stock
                    stock.Quantity -= item.Quantity;
                    stock.LastUpdated = DateTime.UtcNow;

                    _context.WasteItems.Add(new WasteItem
                    {
                        WasteRecordId = waste.Id,
                        RawItemId = item.RawItemId,
                        Quantity = item.Quantity
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = await _context.WasteRecords
                    .Where(w => w.Id == waste.Id)
                    .Select(w => new WasteResponseDto
                    {
                        Id = w.Id,
                        WasteDate = w.WasteDate,
                        Reason = w.Reason,
                        Items = w.WasteItems.Select(i => new WasteItemDetailDto
                        {
                            RawItemId = i.RawItemId,
                            RawItemName = i.RawItem.Name,
                            Quantity = i.Quantity
                        }).ToList()
                    })
                    .FirstAsync();

                return response;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<List<WasteRecordDto>> GetByDateAsync(DateOnly date)
        {
            var start = date.ToDateTime(TimeOnly.MinValue);
            var end = date.ToDateTime(TimeOnly.MaxValue);

            return await _context.WasteRecords
                .Where(w => w.WasteDate >= start && w.WasteDate <= end)
                .Select(w => new WasteRecordDto
                {
                    Id = w.Id,
                     CreatedAt= w.WasteDate
                })
                .ToListAsync();
        }

        public async Task<List<WasteRecordDto>> GetAllAsync()
        {
            return await _context.WasteRecords
                .Include(w => w.WasteItems)
                    .ThenInclude(i => i.RawItem)
                .OrderByDescending(w => w.WasteDate)
                .Select(w => new WasteRecordDto
                {
                    Id = w.Id,
                    ReferenceNo = "WST-" + w.Id,
                    CreatedAt = w.WasteDate,
                    Reason = w.Reason,
                    Items = w.WasteItems.Select(i => new WasteRecordItemDto
                    {
                        RawItemId = i.RawItemId,
                        RawItemName = i.RawItem.Name,
                        Quantity = i.Quantity
                    }).ToList()
                })
                .ToListAsync();
        }

    }


}
