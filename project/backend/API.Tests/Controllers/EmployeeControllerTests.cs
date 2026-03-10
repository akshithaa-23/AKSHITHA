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
    public class EmployeeControllerTests
    {
        private Mock<IEmployeeService> _mockEmployeeService;
        private EmployeeController _controller;

        public EmployeeControllerTests()
        {
            _mockEmployeeService = new Mock<IEmployeeService>();
            _controller = new EmployeeController(_mockEmployeeService.Object);
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
        public async Task GetMyEmployees_ReturnsOkWithEmployees()
        {
            // Arrange
            int customerId = 500;
            var employees = new List<EmployeeDto>
            {
                new EmployeeDto { Id = 1, FullName = "Emp 1" },
                new EmployeeDto { Id = 2, FullName = "Emp 2" }
            };

            _mockEmployeeService.Setup(s => s.GetMyEmployeesAsync(customerId))
                .ReturnsAsync(employees);

            SetupControllerUser(_controller, customerId, "Customer");

            // Act
            var result = await _controller.GetMyEmployees();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedEmployees = Assert.IsAssignableFrom<IEnumerable<EmployeeDto>>(okResult.Value);
            Assert.Equal(2, returnedEmployees.Count());
        }

        [Fact]
        public async Task GetMyEmployees_WhenCompanyNotRegistered_ReturnsBadRequest()
        {
            // Arrange
            int customerId = 500;
            _mockEmployeeService.Setup(s => s.GetMyEmployeesAsync(customerId))
                .ThrowsAsync(new InvalidOperationException("No company registered yet"));

            SetupControllerUser(_controller, customerId, "Customer");

            // Act
            var result = await _controller.GetMyEmployees();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("No company registered yet", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task AddEmployee_WithValidData_ReturnsOkResult()
        {
            // Arrange
            int customerId = 501;
            var dto = new AddEmployeeDto { EmployeeCode = "NEW-E01", FullName = "New Employee" };

            _mockEmployeeService.Setup(s => s.AddEmployeeAsync(customerId, dto))
                .ReturnsAsync(1); // Returns new employeeId

            SetupControllerUser(_controller, customerId, "Customer");

            // Act
            var result = await _controller.AddEmployee(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Employee added successfully", okResult.Value.ToString());
        }

        [Fact]
        public async Task AddEmployee_WithDuplicateCode_ReturnsBadRequest()
        {
            // Arrange
            int customerId = 502;
            var dto = new AddEmployeeDto { EmployeeCode = "DUP-E01" };

            _mockEmployeeService.Setup(s => s.AddEmployeeAsync(customerId, dto))
                .ThrowsAsync(new InvalidOperationException("Employee code already exists in your company"));

            SetupControllerUser(_controller, customerId, "Customer");

            // Act
            var result = await _controller.AddEmployee(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Employee code already exists", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task DeactivateEmployee_WhenValid_ReturnsOkResult()
        {
            // Arrange
            int customerId = 503;
            int employeeId = 4;

            _mockEmployeeService.Setup(s => s.DeactivateEmployeeAsync(customerId, employeeId))
                .Returns(Task.CompletedTask);

            SetupControllerUser(_controller, customerId, "Customer");

            // Act
            var result = await _controller.DeactivateEmployee(employeeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Employee deactivated", okResult.Value.ToString());
        }
    }
}
