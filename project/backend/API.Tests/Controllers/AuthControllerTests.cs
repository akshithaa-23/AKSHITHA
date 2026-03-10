using API.Controllers;
using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Security;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace API.Tests.Controllers
{
    public class AuthControllerTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private JwtTokenService GetJwtTokenService()
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"JwtSettings:SecretKey", "super_secret_key_that_is_long_enough_for_hmacsha256_algorithm"},
                {"JwtSettings:Issuer", "TestIssuer"},
                {"JwtSettings:Audience", "TestAudience"},
                {"JwtSettings:ExpiryHours", "1"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            return new JwtTokenService(configuration);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123");
            context.Users.Add(new User
            {
                FullName = "Test User",
                Email = "test@test.com",
                PasswordHash = passwordHash,
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var jwtService = GetJwtTokenService();
            var authService = new AuthService(context, jwtService);
            var controller = new AuthController(authService);

            var request = new LoginRequestDto
            {
                Email = "test@test.com",
                Password = "Password123"
            };

            // Act
            var result = await controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseDto = Assert.IsType<LoginResponseDto>(okResult.Value);
            
            Assert.NotNull(responseDto.Token);
            Assert.Equal("Test User", responseDto.FullName);
            Assert.Equal("test@test.com", responseDto.Email);
            Assert.Equal("Customer", responseDto.Role);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123");
            context.Users.Add(new User
            {
                FullName = "Test User",
                Email = "test@test.com",
                PasswordHash = passwordHash,
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var jwtService = GetJwtTokenService();
            var authService = new AuthService(context, jwtService);
            var controller = new AuthController(authService);

            var request = new LoginRequestDto
            {
                Email = "test@test.com",
                Password = "WrongPassword"
            };

            // Act
            var result = await controller.Login(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Contains("Invalid email or password", unauthorizedResult.Value.ToString());
        }

        [Fact]
        public async Task RegisterCustomer_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var jwtService = GetJwtTokenService();
            var authService = new AuthService(context, jwtService);
            var controller = new AuthController(authService);

            var request = new RegisterCustomerDto
            {
                FullName = "New Customer",
                Email = "new_customer@test.com",
                Password = "Password123"
            };

            // Act
            var result = await controller.RegisterCustomer(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Customer registered successfully", okResult.Value.ToString());

            var userInDb = await context.Users.FirstOrDefaultAsync(u => u.Email == "new_customer@test.com");
            Assert.NotNull(userInDb);
            Assert.Equal(UserRole.Customer, userInDb.Role);
        }
    }
}
