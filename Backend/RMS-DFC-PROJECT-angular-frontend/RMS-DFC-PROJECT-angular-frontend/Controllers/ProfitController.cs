using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.Services;

namespace RestaurantSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/profit")]
    public class ProfitController : ControllerBase
    {
        private readonly IProfitService _service;

        public ProfitController(IProfitService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfit(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            return Ok(await _service.GetProfitAsync(from, to));
        }
    }

}
