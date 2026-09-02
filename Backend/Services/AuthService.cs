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
    public class AuthService(ApplicationDbContext context, IConfiguration configuration,
        RestaurantSystem.Sync.AuthKeyProvider keys, RestaurantSystem.Sync.INodeContext node) : IAuthService
    {
        // Phase 8: config-based SuperAdmin is a ONE-TIME bootstrap. Once a DB
        // SuperAdmin exists it is used instead and the config path is disabled.
        private async Task EnsureDbSuperAdminAsync()
        {
            var name = configuration["SuperAdmin:UserName"];
            var pass = configuration["SuperAdmin:Password"];
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pass)) return;
            if (await context.Users.AnyAsync(u => u.Role == "SuperAdmin")) return;

            var su = new User { UserName = name, Role = "SuperAdmin", IsActive = true, SecurityStamp = Guid.NewGuid() };
            su.PasswordHash = new PasswordHasher<User>().HashPassword(su, pass);
            context.Users.Add(su);
            await context.SaveChangesAsync();
        }

        private async Task AuditAsync(string userName, string? role, string result, string? detail = null)
        {
            context.AuthAuditLogs.Add(new AuthAuditLog
            {
                AtUtc = DateTime.UtcNow,
                UserName = userName,
                Role = role,
                Result = result,
                Issuer = keys.Issuer,
                NodeId = node.NodeId,
                Detail = detail
            });
            try { using (RestaurantSystem.Sync.SyncStampingInterceptor.Suppress()) await context.SaveChangesAsync(); }
            catch { /* audit is best-effort */ }
        }

        public async Task<string?> LoginAsync(UserDto request)
        {
            await EnsureDbSuperAdminAsync();

            var user = await context.Users
                .Include(u => u.Employee)
                .Include(u => u.Rider)
                .FirstOrDefaultAsync(u => u.UserName == request.UserName);

            // Config SuperAdmin only if NO DB SuperAdmin exists (bootstrap window).
            if (user is null)
            {
                var superAdminUserName = configuration["SuperAdmin:UserName"];
                var superAdminPassword = configuration["SuperAdmin:Password"];
                var dbSuperAdminExists = await context.Users.AnyAsync(u => u.Role == "SuperAdmin");
                if (!dbSuperAdminExists && !string.IsNullOrEmpty(superAdminUserName)
                    && request.UserName == superAdminUserName && request.Password == superAdminPassword)
                {
                    await AuditAsync(superAdminUserName, "SuperAdmin", "superadmin-config");
                    return CreateSuperAdminToken(superAdminUserName);
                }
                await AuditAsync(request.UserName ?? "", null, "bad-credentials");
                return null;
            }

            if (!user.IsActive)
            {
                await AuditAsync(user.UserName, user.Role, "disabled");
                return null;
            }

            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
                == PasswordVerificationResult.Failed)
            {
                await AuditAsync(user.UserName, user.Role, "bad-credentials");
                return null;
            }

            await AuditAsync(user.UserName, user.Role, "success");
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
                new Claim(ClaimTypes.Role, "SuperAdmin"),
                new Claim("node", node.NodeId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: keys.Issuer,                                   // Phase 8
                audience: configuration["Appsettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: keys.SigningCredentials            // RS256 if configured, else HS256
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
        new Claim("RiderId", user.RiderId?.ToString() ?? string.Empty),
        new Claim("stamp", user.SecurityStamp.ToString()),   // Phase 8: token invalidation on disable/pw-change
        new Claim("node", node.NodeId.ToString())
    };

            if (user.Employee != null)
            {
                claims.Add(new Claim("EmployeeName", user.Employee.Name));
            }

            if (user.Rider != null)
            {
                claims.Add(new Claim("RiderName", user.Rider.Name));
            }

            var token = new JwtSecurityToken(
                issuer: keys.Issuer,                                  // Phase 8
                audience: configuration["Appsettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: keys.SigningCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}
