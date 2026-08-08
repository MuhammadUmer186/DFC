using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Models;
using RestaurantSystem.Services;

namespace RestaurantSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _context;

        public AuthController(IAuthService authService, ApplicationDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            var user = await _authService.RegisterAsync(request, User); // pass current user
            if (user is null) return BadRequest("User Already Exist or unauthorized");
            return Ok(user);
        }

        [HttpPost("Login")]
        public async Task<ActionResult> Login(UserDto request)
        {
            var token = await _authService.LoginAsync(request);
            if (token is null) return BadRequest("Invalid username or password");
            return Ok(new { token });
        }

        [HttpPost("bootstrap-mainadmin")]
        public async Task<IActionResult> BootstrapMainAdmin()
        {
            if (await _context.Users.AnyAsync())
                return BadRequest("MainAdmin already exists");

            var mainAdmin = new User
            {
                UserName = "HassanAli",
                Role = "MainAdmin",
                EmployeeId = null // No employee linked
            };

            mainAdmin.PasswordHash = new PasswordHasher<User>()
                .HashPassword(mainAdmin, "lup@fg3WT3"); // default password

            _context.Users.Add(mainAdmin);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "MainAdmin created successfully",
                UserName = mainAdmin.UserName,
                Password = "lup@fg3WT3"
            });
        }
    }
}
