using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.DTOs;
using RestaurantSystem.Services;

namespace RestaurantSystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WasteController : ControllerBase
    {
        private readonly IWasteService _service;

        public WasteController(IWasteService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateWaste(WasteCreateRequestDto request)
        {
            var result = await _service.CreateWasteAsync(request);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(data);
        }
    }


}
