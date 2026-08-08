using RestaurantSystem.DTOs;

namespace RestaurantSystem.Services
{
    public interface IVendorAccountService
    {
        Task<VendorAccountSummaryDto> GetVendorAccountAsync(int vendorId);

        Task<bool> AddVendorPaymentAsync(AddVendorPaymentRequest request);
    }
    public class AddVendorPaymentRequest
    {
        public int VendorId { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaidByUser { get; set; }
    }

}
