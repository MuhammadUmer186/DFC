using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.DTOs;
using RestaurantSystem.Services;

namespace RestaurantSystem.Controllers
{
    // Staff-facing CRUD only for now. A customer-facing self-service consent toggle (spec:
    // "provide a way for customers to reset or disable personalization") needs a way to verify
    // the caller actually owns the phone number first (OTP or similar) — that verification
    // mechanism doesn't exist anywhere in this codebase yet, so an unauthenticated
    // toggle-by-phone-number endpoint would let anyone flip a stranger's consent. Deferred to
    // Milestone 2, once/if phone verification exists.
    [ApiController]
    [Route("api/customers")]
    [Authorize(Roles = "SuperAdmin,Admin,MainAdmin,Cashier")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _service;

        public CustomersController(ICustomerService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("by-phone/{phoneNumber}")]
        public async Task<IActionResult> GetByPhone(string phoneNumber)
        {
            var result = await _service.GetByPhoneAsync(phoneNumber);
            return result == null ? NotFound() : Ok(result);
        }

        [Authorize(Roles = "SuperAdmin,Admin,MainAdmin")]
        [HttpPost]
        public async Task<IActionResult> Upsert(UpsertCustomerDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
                return BadRequest("Phone number is required");

            return Ok(await _service.UpsertByPhoneAsync(dto));
        }
    }
}
