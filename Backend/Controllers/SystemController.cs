using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Services;
using RestaurantSystem.Sync;

namespace RestaurantSystem.Controllers
{
    [ApiController]
    [Route("api/system")]
    [Authorize] // per-action role gates below (clear-data = SuperAdmin; node-status = SuperAdmin/MainAdmin/Admin)
    public class SystemController : ControllerBase
    {
        private readonly ISystemMaintenanceService _service;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _db;
        private readonly INodeContext _node;

        public SystemController(ISystemMaintenanceService service, IConfiguration configuration,
            ApplicationDbContext db, INodeContext node)
        {
            _service = service;
            _configuration = configuration;
            _db = db;
            _node = node;
        }

        // Phase 17 — safe operational status for the RMS. Broader than clear-data:
        // MainAdmin/Admin can read it too.
        [HttpGet("node-status")]
        [Authorize(Roles = "SuperAdmin,MainAdmin,Admin")]
        public async Task<IActionResult> NodeStatus(System.Threading.CancellationToken ct)
            => Ok(await SyncStatus.BuildAsync(_db, _node, ct));

        // Destructive factory reset. SuperAdmin isn't a Users row (it's config-based — see
        // AuthService.LoginAsync), so clearing every table already leaves nothing of it to
        // preserve. Gated by re-entering the SuperAdmin password, checked the same way login
        // checks it, as a deliberate friction step before an irreversible wipe.
        [HttpPost("clear-data")]
        [Authorize(Roles = "SuperAdmin")]
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
