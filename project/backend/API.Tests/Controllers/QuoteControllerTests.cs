using API.Controllers;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace API.Tests.Controllers
{
    public class QuoteControllerTests
    {
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
        public async Task SendQuote_ValidData_ReturnsOkAndCreatesQuote()
        {
            // Arrange
            var mockService = new Mock<IQuoteService>();
            int agentId = 700;

            var dto = new SendQuoteDto
            {
                QuoteRequestId = 10,
                PolicyId = 1,
                EmployeeCount = 10
            };

            var quoteResponse = new
            {
                message = "Quote sent successfully",
                quoteId = 1,
                totalPremium = 10000m,
                validUntil = DateTime.UtcNow.AddDays(30)
            };

            mockService.Setup(s => s.SendQuoteAsync(agentId, dto)).ReturnsAsync(quoteResponse);
            
            var controller = new QuoteController(mockService.Object);
            SetupControllerUser(controller, agentId, "Agent");

            // Act
            var result = await controller.SendQuote(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Quote sent successfully", okResult.Value.ToString());
        }

        [Fact]
        public async Task SendQuote_EmployeeCountBelowMin_ReturnsBadRequest()
        {
            // Arrange
            var mockService = new Mock<IQuoteService>();
            int agentId = 702;

            var dto = new SendQuoteDto
            {
                QuoteRequestId = 11,
                PolicyId = 2,
                EmployeeCount = 10 // Below min of 50
            };

            mockService.Setup(s => s.SendQuoteAsync(agentId, dto))
                .ThrowsAsync(new ArgumentException("Minimum 50 employees required for this policy"));
            
            var controller = new QuoteController(mockService.Object);
            SetupControllerUser(controller, agentId, "Agent");

            // Act
            var result = await controller.SendQuote(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Minimum 50 employees required", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task Accept_PendingQuote_UpdatesStatusToAccepted()
        {
            // Arrange
            var mockService = new Mock<IQuoteService>();
            int customerId = 704;
            int quoteId = 5;

            var successMessage = "Quote accepted. Proceed to payment.";
            mockService.Setup(s => s.AcceptQuoteAsync(customerId, quoteId)).ReturnsAsync(successMessage);

            var controller = new QuoteController(mockService.Object);
            SetupControllerUser(controller, customerId, "Customer");

            // Act
            var result = await controller.Accept(quoteId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Quote accepted", okResult.Value.ToString());
        }
    }
}
