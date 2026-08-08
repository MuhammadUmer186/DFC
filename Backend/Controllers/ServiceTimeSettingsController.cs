using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;

namespace RestaurantSystem.Controllers
{
    [ApiController]
    [Route("api/servicetimesettings")]
    [Authorize(Roles = "SuperAdmin")]
    public class ServiceTimeSettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceTimeSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var settings = await _context.ServiceTimeSettings
                .Select(s => new ServiceTimeSettingDto
                {
                    Id = s.Id,
                    ServiceType = s.ServiceType,
                    MinMinutes = s.MinMinutes,
                    MaxMinutes = s.MaxMinutes,
                    IsEnabled = s.IsEnabled
                })
                .ToListAsync();

            return Ok(settings);
        }

        [HttpPut("{serviceType}")]
        public async Task<IActionResult> Update(string serviceType, UpdateServiceTimeSettingDto dto)
        {
            if (dto.MinMinutes < 0 || dto.MaxMinutes < dto.MinMinutes)
                return BadRequest("Invalid time range");

            var setting = await _context.ServiceTimeSettings
                .FirstOrDefaultAsync(s => s.ServiceType == serviceType);

            if (setting == null)
                return NotFound($"No settings found for service type '{serviceType}'");

            setting.MinMinutes = dto.MinMinutes;
            setting.MaxMinutes = dto.MaxMinutes;
            setting.IsEnabled = dto.IsEnabled;
            setting.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ServiceTimeSettingDto
            {
                Id = setting.Id,
                ServiceType = setting.ServiceType,
                MinMinutes = setting.MinMinutes,
                MaxMinutes = setting.MaxMinutes,
                IsEnabled = setting.IsEnabled
            });
        }
    }
}
