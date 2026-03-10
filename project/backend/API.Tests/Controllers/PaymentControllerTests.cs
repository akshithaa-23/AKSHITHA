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
    public class PaymentControllerTests
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
        public async Task ProcessPayment_AcceptedQuote_ProcessesPaymentAndReturnsOk()
        {
            // Arrange
            var mockService = new Mock<IPaymentService>();
            int customerId = 600;

            var dto = new ProcessPaymentDto
            {
                QuoteId = 1,
                PaymentMethod = "Credit Card",
                CardNumber = "1234567890123456",
                CardHolderName = "John Doe"
            };

            var paymentResponse = new PaymentResponseDto
            {
                Id = 1,
                AmountPaid = 50000,
                CommissionAmount = 3500
            };

            mockService.Setup(s => s.ProcessPaymentAsync(customerId, dto)).ReturnsAsync(paymentResponse);
            
            var controller = new PaymentController(mockService.Object);
            SetupControllerUser(controller, customerId, "Customer");

            // Act
            var result = await controller.ProcessPayment(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaymentResponseDto>(okResult.Value);

            Assert.Equal(50000, response.AmountPaid);
            Assert.Equal(3500, response.CommissionAmount);
        }

        [Fact]
        public async Task ProcessPayment_PendingQuote_ReturnsBadRequest()
        {
            // Arrange
            var mockService = new Mock<IPaymentService>();
            int customerId = 602;

            var dto = new ProcessPaymentDto { QuoteId = 2, PaymentMethod = "Card", CardNumber = "1111", CardHolderName = "Test" };
            
            mockService.Setup(s => s.ProcessPaymentAsync(customerId, dto))
                .ThrowsAsync(new ArgumentException("Quote must be accepted before payment"));

            var controller = new PaymentController(mockService.Object);
            SetupControllerUser(controller, customerId, "Customer");

            // Act
            var result = await controller.ProcessPayment(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Quote must be accepted before payment", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task GetAgentCommissions_ReturnsOkWithCommissions()
        {
            // Arrange
            var mockService = new Mock<IPaymentService>();
            int agentId = 603;

            var commissions = new List<object>
            {
                new { Id = 1, InvoiceNumber = "INV-123", PolicyName = "Pol", CustomerName = "CustX", AmountPaid = 10000, CommissionRate = 5, CommissionAmount = 500, EarnedAt = DateTime.UtcNow }
            };

            mockService.Setup(s => s.GetAgentCommissionsAsync(agentId)).ReturnsAsync(commissions);

            var controller = new PaymentController(mockService.Object);
            SetupControllerUser(controller, agentId, "Agent");

            // Act
            var result = await controller.GetAgentCommissions();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultCommissions = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            
            Assert.Single(resultCommissions);
        }
    }
}
