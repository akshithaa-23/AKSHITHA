using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAppDbContext _context;

        public AdminService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<int> RegisterUserAsync(RegisterUserDto request)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
                throw new ArgumentException("Email already exists");

            if (request.Role != "Agent" && request.Role != "ClaimsManager")
                throw new ArgumentException("Invalid role. Only Agent or ClaimsManager allowed");

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = Enum.Parse<UserRole>(request.Role),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user.Id;
        }

        public async Task<IEnumerable<object>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Where(u => u.Role != UserRole.Admin)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    Role = u.Role.ToString(),
                    u.IsActive,
                    u.CreatedAt
                })
                .ToListAsync();

            return users;
        }
    }
}
