using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.DTOs;

namespace RestaurantSystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DealsController : ControllerBase
    {
        private readonly IDealService _service;

        public DealsController(IDealService service)
        {
            _service = service;
        }
        [Authorize(Roles = "Admin,Cashier,MainAdmin,Waiter")]
        [HttpGet]
        public async Task<IActionResult> GetDeals()
            => Ok(await _service.GetDealsAsync());
        [Authorize(Roles = "Admin,MainAdmin")]
        [HttpPost]
        public async Task<IActionResult> CreateDeal(CreateDealDto dto)
            => Ok(await _service.CreateDealAsync(dto));
        [Authorize(Roles = "Admin,MainAdmin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDeal(int id, CreateDealDto dto)
        {
            var result = await _service.UpdateDealAsync(id, dto);
            if (result == null) return NotFound();
            return Ok(result);
        }
        [Authorize(Roles = "Admin,MainAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeal(int id)
        {
            var result = await _service.DeleteDealAsync(id);
            if (!result) return NotFound();
            return Ok(true);
        }
    }

}
