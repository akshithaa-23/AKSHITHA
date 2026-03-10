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
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace API.Tests.Controllers
{
    public class RecommendationControllerTests
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
        public async Task Send_WithValidRequest_ReturnsOkAndCreatesRecommendation()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            int agentId = 900;
            int customerId = 901;

            context.Users.Add(new User { Id = agentId, Email = "a@test.com", PasswordHash = "h", FullName = "Agent", Role = UserRole.Agent, IsActive = true, CreatedAt = DateTime.UtcNow });
            context.Users.Add(new User { Id = customerId, Email = "c@test.com", PasswordHash = "h", FullName = "Customer", Role = UserRole.Customer, IsActive = true, CreatedAt = DateTime.UtcNow });
            
            // Seed Policies required for Recommendation logic
            context.Policies.AddRange(
                new Policy { Id = 1, Name = "Essential 1", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Policy { Id = 2, Name = "Essential 2", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Policy { Id = 3, Name = "Essential 3", IsActive = true, CreatedAt = DateTime.UtcNow }
            );

            context.QuoteRequests.Add(new QuoteRequest { Id = 20, CustomerId = customerId, AssignedAgentId = agentId, RequestType = "Recommendation", NumberOfEmployees = 50, Status = "Assigned", CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var recommendationService = new RecommendationService(context);
            var controller = new RecommendationController(recommendationService);
            SetupControllerUser(controller, agentId, "Agent");

            var dto = new SendRecommendationDto
            {
                QuoteRequestId = 20,
                AgentMessage = "Here are my recommendations."
            };

            // Act
            var result = await controller.Send(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Recommendation sent successfully", okResult.Value.ToString());

            var recommendation = await context.Recommendations
                .Include(r => r.RecommendationPolicies)
                .FirstOrDefaultAsync(r => r.QuoteRequestId == 20);
                
            Assert.NotNull(recommendation);
            Assert.Equal("Here are my recommendations.", recommendation.AgentMessage);
            Assert.Equal(3, recommendation.RecommendationPolicies.Count); // Should recommend the 3 Essential policies for 50 employees

            var quoteRequest = await context.QuoteRequests.FindAsync(20);
            Assert.Equal("RecommendationSent", quoteRequest!.Status);
        }

        [Fact]
        public async Task Send_WithWrongRequestType_ReturnsBadRequest()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            int agentId = 902;
            int customerId = 903;

            context.Users.Add(new User { Id = agentId, Email = "a2@t.com", PasswordHash = "h", FullName = "Agent2", Role = UserRole.Agent, IsActive = true, CreatedAt = DateTime.UtcNow });
            context.QuoteRequests.Add(new QuoteRequest { Id = 21, CustomerId = customerId, AssignedAgentId = agentId, RequestType = "DirectBuy", NumberOfEmployees = 50, Status = "Assigned", CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var recommendationService = new RecommendationService(context);
            var controller = new RecommendationController(recommendationService);
            SetupControllerUser(controller, agentId, "Agent");

            var dto = new SendRecommendationDto { QuoteRequestId = 21, AgentMessage = "Test" };

            // Act
            var result = await controller.Send(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("not a recommendation request", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task GetMyRecommendations_ReturnsCustomerRecommendations()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            int customerId = 904;

            context.Users.Add(new User { Id = customerId, Email = "c4@test.com", PasswordHash = "h", FullName = "Customer4", Role = UserRole.Customer, IsActive = true, CreatedAt = DateTime.UtcNow });
            context.Users.Add(new User { Id = 905, Email = "a5@test.com", PasswordHash = "h", FullName = "Agent5", Role = UserRole.Agent, IsActive = true, CreatedAt = DateTime.UtcNow });
            context.QuoteRequests.Add(new QuoteRequest { Id = 22, CustomerId = customerId, AssignedAgentId = 905, RequestType = "Recommendation", CompanyName = "Company", NumberOfEmployees = 50, Status = "RecommendationSent", CreatedAt = DateTime.UtcNow });
            context.Recommendations.Add(new Recommendation { Id = 5, QuoteRequestId = 22, AgentId = 905, CustomerId = customerId, AgentMessage = "Msg", CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var recommendationService = new RecommendationService(context);
            var controller = new RecommendationController(recommendationService);
            SetupControllerUser(controller, customerId, "Customer");

            // Act
            var result = await controller.GetMyRecommendations();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var items = Assert.IsAssignableFrom<IEnumerable<RecommendationResponseDto>>(okResult.Value);
            Assert.Single(items);
        }
    }
}
