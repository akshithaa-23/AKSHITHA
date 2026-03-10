using API.Controllers;
using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace API.Tests.Controllers
{
    public class QuoteRequestControllerTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private void SetupControllerUser(ControllerBase controller, int userId, string role)
        {
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new System.Security.Claims.Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task RequestRecommendation_ValidData_CreatesQuoteRequestAndConnectsAgent()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            int customerId = 800;
            int agentId = 801;

            context.Users.Add(new User { Id = customerId, Email = "c@example.com", PasswordHash = "h", FullName = "Cust", Role = UserRole.Customer, IsActive = true, CreatedAt = DateTime.UtcNow });
            context.Users.Add(new User { Id = agentId, Email = "a@example.com", PasswordHash = "h", FullName = "Agent One", Role = UserRole.Agent, IsActive = true, CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var quoteRequestService = new QuoteRequestService(context);
            var controller = new QuoteRequestController(quoteRequestService);
            SetupControllerUser(controller, customerId, "Customer");

            var dto = new QuoteRequestDto
            {
                CompanyName = "New Co",
                IndustryType = "IT",
                NumberOfEmployees = 20,
                ContactName = "John",
                ContactEmail = "j@newco.com",
                ContactPhone = "123456"
            };

            // Act
            var result = await controller.RequestRecommendation(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Recommendation request submitted", okResult.Value.ToString());

            var requests = await context.QuoteRequests.ToListAsync();
            Assert.Single(requests);
            Assert.Equal("Recommendation", requests[0].RequestType);
            Assert.Equal(agentId, requests[0].AssignedAgentId);
            Assert.Equal("Assigned", requests[0].Status);

            var company = await context.Companies.FirstOrDefaultAsync(c => c.CustomerId == customerId);
            Assert.NotNull(company);
            Assert.Equal("New Co", company.CompanyName);
        }

        [Fact]
        public async Task DirectBuy_ActivePolicyExists_ReturnsBadRequest()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            int customerId = 802;

            context.Users.Add(new User { Id = customerId, Email = "c2@example.com", PasswordHash = "h", FullName = "Cust2", Role = UserRole.Customer, IsActive = true, CreatedAt = DateTime.UtcNow });
            context.Companies.Add(new Company { Id = 10, CustomerId = customerId, CompanyName = "Old Co", CreatedAt = DateTime.UtcNow });
            context.CompanyPolicies.Add(new CompanyPolicy { Id = 1, CompanyId = 10, PolicyId = 1, Status = "Active", CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var quoteRequestService = new QuoteRequestService(context);
            var controller = new QuoteRequestController(quoteRequestService);
            SetupControllerUser(controller, customerId, "Customer");

            var dto = new DirectBuyRequestDto { PolicyId = 2 };

            // Act
            var result = await controller.DirectBuy(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Your company already has an active policy", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task GetMyRequests_ReturnsCustomerRequests()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            int customerId = 803;

            context.Users.Add(new User { Id = customerId, Email = "c3@example.com", PasswordHash = "h", FullName = "Cust3", Role = UserRole.Customer, IsActive = true, CreatedAt = DateTime.UtcNow });
            context.QuoteRequests.Add(new QuoteRequest { Id = 50, CustomerId = customerId, RequestType = "Recommendation", CompanyName = "C3 Co", CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var quoteRequestService = new QuoteRequestService(context);
            var controller = new QuoteRequestController(quoteRequestService);
            SetupControllerUser(controller, customerId, "Customer");

            // Act
            var result = await controller.GetMyRequests();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var items = Assert.IsAssignableFrom<IEnumerable<QuoteRequestResponseDto>>(okResult.Value);
            Assert.Single(items);
        }
    }
}
