using API.Controllers;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace API.Tests.Controllers
{
    public class AdminControllerTests
    {
        [Fact]
        public async Task RegisterUser_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var mockService = new Mock<IAdminService>();
            var request = new RegisterUserDto
            {
                FullName = "Test Agent",
                Email = "agent@test.com",
                Password = "Password123",
                Role = "Agent"
            };

            mockService.Setup(s => s.RegisterUserAsync(request)).ReturnsAsync(1);
            var controller = new AdminController(mockService.Object);

            // Act
            var result = await controller.RegisterUser(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Agent registered successfully", okResult.Value.ToString());
        }

        [Fact]
        public async Task RegisterUser_WithExistingEmail_ReturnsBadRequest()
        {
            // Arrange
            var mockService = new Mock<IAdminService>();
            var request = new RegisterUserDto
            {
                FullName = "New User",
                Email = "existing@test.com",
                Password = "Password123",
                Role = "Agent"
            };

            mockService.Setup(s => s.RegisterUserAsync(request)).ThrowsAsync(new ArgumentException("Email already exists"));
            var controller = new AdminController(mockService.Object);

            // Act
            var result = await controller.RegisterUser(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Email already exists", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task RegisterUser_WithInvalidRole_ReturnsBadRequest()
        {
            // Arrange
            var mockService = new Mock<IAdminService>();
            var request = new RegisterUserDto
            {
                FullName = "Test Admin",
                Email = "admin@test.com",
                Password = "Password123",
                Role = "Admin"
            };

            mockService.Setup(s => s.RegisterUserAsync(request)).ThrowsAsync(new ArgumentException("Invalid role. Only Agent or ClaimsManager allowed"));
            var controller = new AdminController(mockService.Object);

            // Act
            var result = await controller.RegisterUser(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid role", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task GetAllUsers_ReturnsOkResultWithUsers()
        {
            // Arrange
            var mockService = new Mock<IAdminService>();
            var users = new List<object>
            {
                new { Id = 1, FullName = "Agent User", Email = "agent@test.com", Role = "Agent", IsActive = true, CreatedAt = DateTime.UtcNow },
                new { Id = 2, FullName = "Claims Manager", Email = "cm@test.com", Role = "ClaimsManager", IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            mockService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(users);
            var controller = new AdminController(mockService.Object);

            // Act
            var result = await controller.GetAllUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUsers = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Equal(2, returnedUsers.Count());
        }
    }
}
