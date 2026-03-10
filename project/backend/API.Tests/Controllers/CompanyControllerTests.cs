using API.Controllers;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace API.Tests.Controllers
{
    public class CompanyControllerTests
    {
        private Mock<ICompanyService> _mockCompanyService;
        private CompanyController _controller;

        public CompanyControllerTests()
        {
            _mockCompanyService = new Mock<ICompanyService>();
            _controller = new CompanyController(_mockCompanyService.Object);
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
        public async Task GetMyCompany_WhenCompanyExists_ReturnsOkResult()
        {
            // Arrange
            int customerId = 300;
            var companyObj = new { Id = 1, CompanyName = "My Tech Co" };

            _mockCompanyService.Setup(s => s.GetMyCompanyAsync(customerId))
                .ReturnsAsync(companyObj);

            SetupControllerUser(_controller, customerId, "Customer");

            // Act
            var result = await _controller.GetMyCompany();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("My Tech Co", okResult.Value.ToString());
        }

        [Fact]
        public async Task GetMyCompany_WhenCompanyDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            int customerId = 301; // No company associated with this ID

            _mockCompanyService.Setup(s => s.GetMyCompanyAsync(customerId))
                .ReturnsAsync((object)null);

            SetupControllerUser(_controller, customerId, "Customer");

            // Act
            var result = await _controller.GetMyCompany();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("No company registered yet", notFoundResult.Value.ToString());
        }

        [Fact]
        public async Task GetAll_ReturnsOkResultWithCompaniesList()
        {
            // Arrange
            var companiesList = new List<object>
            {
                new { Id = 1, CompanyName = "Company A" },
                new { Id = 2, CompanyName = "Company B" }
            };

            _mockCompanyService.Setup(s => s.GetAllAsync())
                .ReturnsAsync(companiesList);

            SetupControllerUser(_controller, 500, "Admin"); // Admin user calling the endpoint

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCompanies = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            
            Assert.Equal(2, returnedCompanies.Count());
        }
    }
}
