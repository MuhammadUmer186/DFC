using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.DTOs;
using RestaurantSystem.Models;
using System.Security.Claims;

namespace RestaurantSystem.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto request, ClaimsPrincipal? currentUser = null);
        Task<string?> LoginAsync(UserDto request);
        
    }
}
