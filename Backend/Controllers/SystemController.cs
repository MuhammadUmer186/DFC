using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.DTOs;
using RestaurantSystem.Services;

namespace RestaurantSystem.Controllers
{
    [ApiController]
    [Route("api/system")]
    [Authorize(Roles = "SuperAdmin")]
    public class SystemController : ControllerBase
    {
        private readonly ISystemMaintenanceService _service;
        private readonly IConfiguration _configuration;

        public SystemController(ISystemMaintenanceService service, IConfiguration configuration)
        {
            _service = service;
            _configuration = configuration;
        }

        // Destructive factory reset. SuperAdmin isn't a Users row (it's config-based — see
        // AuthService.LoginAsync), so clearing every table already leaves nothing of it to
        // preserve. Gated by re-entering the SuperAdmin password, checked the same way login
        // checks it, as a deliberate friction step before an irreversible wipe.
        [HttpPost("clear-data")]
        public async Task<IActionResult> ClearData(ClearDataRequest request)
        {
            var superAdminPassword = _configuration["SuperAdmin:Password"];
            if (string.IsNullOrEmpty(request.Password) || request.Password != superAdminPassword)
                return BadRequest("Incorrect password");

            await _service.ClearAllDataAsync();
            return Ok(new { message = "All data cleared" });
        }
    }
}
