using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RestaurantSystem.Services
{
    public class AuthService(ApplicationDbContext context, IConfiguration configuration) : IAuthService
    {
        public async Task<string?> LoginAsync(UserDto request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);
            if (user is null)
                return null;
            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
                == PasswordVerificationResult.Failed)
                return null;
            return CreateToken(user);
        }

        public async Task<User?> RegisterAsync(UserDto request, ClaimsPrincipal? currentUser = null)
        {
            // Check if username already exists
            if (await context.Users.AnyAsync(u => u.UserName == request.UserName))
                return null;

            var currentRole = currentUser?.FindFirst(ClaimTypes.Role)?.Value;

            // Role rules
            if (request.Roles == "MainAdmin")
            {
                // Only allow creating MainAdmin if no users exist yet
                if (await context.Users.AnyAsync())
                    throw new Exception("Cannot create another MainAdmin once users exist");
            }
            else if (request.Roles == "Admin")
            {
                // Only MainAdmin can create Admin
                if (currentRole != "MainAdmin")
                    throw new Exception("Only MainAdmin can create Admin accounts");
            }
            else // Any other role (employee roles)
            {
                // Only MainAdmin or Admin can create employees
                if (currentRole != "MainAdmin" && currentRole != "Admin")
                    throw new Exception("Only MainAdmin or Admin can create employee accounts");

                // Employee must have EmployeeId
                if (request.EmployeeId == null)
                    throw new Exception("EmployeeId is required for employee accounts");
            }

            // Create user entity
            var user = new User
            {
                UserName = request.UserName,
                Role = request.Roles,
                EmployeeId = request.Roles == "Admin" || request.Roles == "MainAdmin" ? null : request.EmployeeId
            };

            // Hash password
            user.PasswordHash = new PasswordHasher<User>()
                .HashPassword(user, request.Password);

            // Save to database
            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }





        private string CreateToken(User user)
        {
            var claims = new List<Claim>
    {
        // ✅ STANDARD IDENTITY CLAIMS (VERY IMPORTANT)
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Role, user.Role),

        // ✅ OPTIONAL: custom claims
        new Claim("EmployeeId", user.EmployeeId?.ToString() ?? string.Empty)
    };

            if (user.Employee != null)
            {
                claims.Add(new Claim("EmployeeName", user.Employee.Name));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Appsettings:Token"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration["Appsettings:Issuer"],
                audience: configuration["Appsettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}
