using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.Services;

namespace RestaurantSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/menu-profit")]
    public class MenuProfitController : ControllerBase
    {
        private readonly IMenuProfitService _service;

        public MenuProfitController(IMenuProfitService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            return Ok(await _service.GetMenuProfitAsync(from, to));
        }
    }

}
