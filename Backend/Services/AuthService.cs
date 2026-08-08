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
            var superAdminUserName = configuration["SuperAdmin:UserName"];
            var superAdminPassword = configuration["SuperAdmin:Password"];
            if (!string.IsNullOrEmpty(superAdminUserName)
                && request.UserName == superAdminUserName
                && request.Password == superAdminPassword)
            {
                return CreateSuperAdminToken(superAdminUserName);
            }

            var user = await context.Users
                .Include(u => u.Employee)
                .Include(u => u.Rider)
                .FirstOrDefaultAsync(u => u.UserName == request.UserName);
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
            if (request.Roles == "SuperAdmin")
            {
                // SuperAdmin is configured, never created through Register
                throw new Exception("SuperAdmin cannot be created here");
            }
            else if (request.Roles == "MainAdmin")
            {
                // Only allow creating MainAdmin if no users exist yet
                if (await context.Users.AnyAsync())
                    throw new Exception("Cannot create another MainAdmin once users exist");
            }
            else if (request.Roles == "Admin")
            {
                // Only Super Admin can create Admin
                if (currentRole != "SuperAdmin")
                    throw new Exception("Only Super Admin can create Admin accounts");
            }
            else if (request.Roles == "Rider")
            {
                // Only Super Admin can create rider accounts
                if (currentRole != "SuperAdmin")
                    throw new Exception("Only Super Admin can create rider accounts");

                // Rider account must reference an existing Rider record
                if (request.RiderId == null)
                    throw new Exception("RiderId is required for rider accounts");

                if (!await context.Riders.AnyAsync(r => r.Id == request.RiderId))
                    throw new Exception("Rider not found");
            }
            else // Any other role (employee roles)
            {
                // Only Super Admin can create employees
                if (currentRole != "SuperAdmin")
                    throw new Exception("Only Super Admin can create employee accounts");

                // Employee must have EmployeeId
                if (request.EmployeeId == null)
                    throw new Exception("EmployeeId is required for employee accounts");
            }

            // Create user entity
            var user = new User
            {
                UserName = request.UserName,
                Role = request.Roles,
                EmployeeId = request.Roles == "Admin" || request.Roles == "MainAdmin" || request.Roles == "Rider" ? null : request.EmployeeId,
                RiderId = request.Roles == "Rider" ? request.RiderId : null
            };

            // Hash password
            user.PasswordHash = new PasswordHasher<User>()
                .HashPassword(user, request.Password);

            // Save to database
            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }





        private string CreateSuperAdminToken(string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "0"),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, "SuperAdmin")
            };

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

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
    {
        // ✅ STANDARD IDENTITY CLAIMS (VERY IMPORTANT)
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Role, user.Role),

        // ✅ OPTIONAL: custom claims
        new Claim("EmployeeId", user.EmployeeId?.ToString() ?? string.Empty),
        new Claim("RiderId", user.RiderId?.ToString() ?? string.Empty)
    };

            if (user.Employee != null)
            {
                claims.Add(new Claim("EmployeeName", user.Employee.Name));
            }

            if (user.Rider != null)
            {
                claims.Add(new Claim("RiderName", user.Rider.Name));
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
