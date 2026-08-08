using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.Services;

namespace RestaurantSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/consumption")]
    public class ConsumptionController : ControllerBase
    {
        private readonly IConsumptionService _service;

        public ConsumptionController(IConsumptionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            return Ok(await _service.GetConsumptionAsync(from, to));
        }
    }

}
