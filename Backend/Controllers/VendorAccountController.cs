using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.Services;

namespace RestaurantSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/vendor-account")]
    public class VendorAccountController : ControllerBase
    {
        private readonly IVendorAccountService _service;

        public VendorAccountController(IVendorAccountService service)
        {
            _service = service;
        }

        [HttpGet("{vendorId}")]
        public async Task<IActionResult> GetVendorAccount(int vendorId)
        {
            try
            {
                var result = await _service.GetVendorAccountAsync(vendorId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("pay")]
        public async Task<IActionResult> PayVendor(AddVendorPaymentRequest request)
        {
            try
            {
                await _service.AddVendorPaymentAsync(request);
                return Ok("Payment Recorded");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

}
