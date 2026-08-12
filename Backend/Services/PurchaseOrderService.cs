using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Models;
using System;

namespace RestaurantSystem.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly ApplicationDbContext _context;

        public PurchaseOrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(PurchaseOrderCreateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = new PurchaseOrder
                {
                    BillNo = dto.BillNo,
                    VendorId = dto.VendorId,
                    PurchaseDate = DateTime.UtcNow,
                    PurchaseOrderItems = new List<PurchaseOrderItem>()
                };

                foreach (var item in dto.Items)
                {
                    // --- Add Purchase Order Item ---
                    var orderItem = new PurchaseOrderItem
                    {
                        RawItemId = item.RawItemId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.Quantity * item.UnitPrice
                    };

                    order.PurchaseOrderItems.Add(orderItem);

                    // --- Stock Update Logic ---
                    var stock = await _context.StoreStocks
                        .FirstOrDefaultAsync(s =>
                            s.RawItemId == item.RawItemId &&
                            s.VendorId == dto.VendorId);

                    // If stock does not exist → create new
                    if (stock == null)
                    {
                        stock = new StoreStock
                        {
                            RawItemId = item.RawItemId,
                            VendorId = dto.VendorId,
                            Quantity = item.Quantity,
                            LastUpdated = DateTime.UtcNow
                        };

                        _context.StoreStocks.Add(stock);
                    }
                    else
                    {
                        // Update Existing stock
                        stock.Quantity += item.Quantity;
                        stock.LastUpdated = DateTime.UtcNow;
                    }
                }

                // --- Total PO Amount ---
                order.TotalAmount = order.PurchaseOrderItems.Sum(i => i.TotalPrice);

                // --- Save Order ---
                _context.PurchaseOrders.Add(order);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<List<PurchaseOrderListDto>> GetByDateAsync(DateOnly date)
        {
            var start = date.ToDateTime(TimeOnly.MinValue);
            var end = date.ToDateTime(TimeOnly.MaxValue);

            return await _context.PurchaseOrders
                .Where(p => p.PurchaseDate >= start && p.PurchaseDate <= end)
                .Select(p => new PurchaseOrderListDto
                {
                    Id = p.Id,
                    BillNo = p.BillNo,
                    PurchaseDate = p.PurchaseDate,
                    VendorId = p.VendorId,
                    VendorName = p.Vendor != null ? p.Vendor.Name : "N/A",
                    TotalAmount = p.TotalAmount,

                    Items = p.PurchaseOrderItems.Select(i => new PurchaseOrderItemDto
                    {
                        RawItemId = i.RawItemId,
                        RawItemName = i.RawItem != null ? i.RawItem.Name : "N/A",
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity,
                        TotalPrice = i.TotalPrice
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<List<PurchaseOrderListDto>> GetAllAsync()
        {
            return await _context.PurchaseOrders
                .Include(p => p.Vendor)
                .Include(p => p.PurchaseOrderItems)
                    .ThenInclude(i => i.RawItem)
                .OrderByDescending(p => p.Id)
                .Select(p => new PurchaseOrderListDto
                {
                    Id = p.Id,
                    BillNo = p.BillNo,
                    PurchaseDate = p.PurchaseDate,
                    VendorId = p.VendorId,
                    VendorName = p.Vendor != null ? p.Vendor.Name : "N/A",
                    TotalAmount = p.TotalAmount,

                    Items = p.PurchaseOrderItems.Select(i => new PurchaseOrderItemDto
                    {
                        RawItemId = i.RawItemId,
                        RawItemName = i.RawItem != null ? i.RawItem.Name : "N/A",
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity,
                        TotalPrice = i.TotalPrice
                    }).ToList()
                })
                .ToListAsync();
        }

    }

}
