using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Models;

namespace RestaurantSystem.Services
{
    public class VendorAccountService : IVendorAccountService

    {
        private readonly ApplicationDbContext _context;

        public VendorAccountService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<VendorAccountSummaryDto> GetVendorAccountAsync(int vendorId)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == vendorId);

            var totalPurchased = await _context.PurchaseOrders
                .Where(p => p.VendorId == vendorId)
                .SumAsync(p => p.TotalAmount);

            var totalPaid = await _context.VendorPayments
                .Where(p => p.VendorId == vendorId)
                .SumAsync(p => p.AmountPaid);

            var orders = await _context.PurchaseOrders
                .Where(p => p.VendorId == vendorId)
                .Select(p => new PurchaseOrderSummaryDto
                {
                    BillNo=p.BillNo,
                    Id = p.Id,
                    TotalAmount = p.TotalAmount,
                    CreatedAt = p.PurchaseDate
                }).ToListAsync();

            var payments = await _context.VendorPayments
                .Where(p => p.VendorId == vendorId)
                .Select(p => new VendorPaymentDto
                {
                    Id = p.Id,
                    AmountPaid = p.AmountPaid,
                    PaidByUser = p.PaidByUser,
                    PaidOn = p.PaidOn
                }).ToListAsync();

            return new VendorAccountSummaryDto
            {
                VendorId = vendor.Id,
                VendorName = vendor.Name,
                TotalPurchased = totalPurchased,
                TotalPaid = totalPaid,
                PurchaseOrders = orders,
                Payments = payments
            };
        }


        public async Task<bool> AddVendorPaymentAsync(AddVendorPaymentRequest request)
        {
            var payment = new VendorPayment
            {
                VendorId = request.VendorId,
                AmountPaid = request.AmountPaid,
                PaidByUser = request.PaidByUser,
                PaidOn = DateTime.Now
            };

            _context.VendorPayments.Add(payment);
            await _context.SaveChangesAsync();

            return true;
        }

    }
}
