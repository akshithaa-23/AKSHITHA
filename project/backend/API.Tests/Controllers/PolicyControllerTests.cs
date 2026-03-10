using API.Controllers;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace API.Tests.Controllers
{
    public class PolicyControllerTests
    {
        private Mock<IPolicyService> _mockPolicyService;
        private PolicyController _controller;

        public PolicyControllerTests()
        {
            _mockPolicyService = new Mock<IPolicyService>();
            _controller = new PolicyController(_mockPolicyService.Object);
        }

        private void SetupControllerUser(ControllerBase controller, int userId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task GetAll_ReturnsOnlyActivePolicies()
        {
            // Arrange
            var policies = new List<PolicyDto>
            {
                new PolicyDto { Id = 1, Name = "Active Policy 1", IsActive = true },
                new PolicyDto { Id = 3, Name = "Active Policy 2", IsActive = true }
            };

            _mockPolicyService.Setup(s => s.GetAllActiveAsync())
                .ReturnsAsync(policies);

            SetupControllerUser(_controller, 1, "Customer");

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPolicies = Assert.IsAssignableFrom<IEnumerable<PolicyDto>>(okResult.Value);
            
            Assert.Equal(2, returnedPolicies.Count());
            Assert.DoesNotContain(returnedPolicies, p => p.Name == "Inactive Policy");
        }

        [Fact]
        public async Task Create_WithValidData_ReturnsOkAndCreatesPolicy()
        {
            // Arrange
            SetupControllerUser(_controller, 1, "Admin");

            var dto = new CreatePolicyDto
            {
                Name = "New Comprehensive Policy",
                HealthCoverage = 500000,
                PremiumPerEmployee = 5000,
                DurationYears = 1,
                MinEmployees = 10,
                IsPopular = true
            };

            _mockPolicyService.Setup(s => s.CreateAsync(dto))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Policy created successfully", okResult.Value.ToString());
        }

        [Fact]
        public async Task Create_WithInvalidLifeValidation_ReturnsBadRequest()
        {
            // Arrange
            SetupControllerUser(_controller, 1, "Admin");

            var dto = new CreatePolicyDto
            {
                Name = "Faulty Policy",
                HealthCoverage = 500000,
                PremiumPerEmployee = 5000,
                LifeCoverageMultiplier = null,
                MaxLifeCoverageLimit = 1000000 // Invalid: max limit cannot exist without multiplier
            };

            _mockPolicyService.Setup(s => s.CreateAsync(dto))
                .ThrowsAsync(new ArgumentException("Max life coverage cannot exist without life multiplier"));

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Max life coverage cannot exist without life multiplier", badRequestResult.Value.ToString());
        }
    }
}
