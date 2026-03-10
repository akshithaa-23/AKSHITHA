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
    public class ClaimControllerTests
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
        public async Task GetAllowedClaimTypes_WithActivePolicy_ReturnsTypes()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            
            // Seed Customer
            int customerId = 100;
            context.Users.Add(new User { Id = customerId, Email = "customer@example.com", PasswordHash = "hash", FullName = "Customer", Role = UserRole.Customer, IsActive = true, CreatedAt = DateTime.UtcNow });
            
            // Seed Policy
            var policy = new Policy { Id = 1, Name = "Test Policy", HealthCoverage = 100000, LifeCoverageMultiplier = 2, IsActive = true, CreatedAt = DateTime.UtcNow };
            context.Policies.Add(policy);
            
            // Seed Company
            var company = new Company { Id = 1, CustomerId = customerId, CompanyName = "Test Co", Size = 10, CreatedAt = DateTime.UtcNow };
            context.Companies.Add(company);
            
            // Seed CompanyPolicy
            context.CompanyPolicies.Add(new CompanyPolicy { Id = 1, CompanyId = 1, PolicyId = 1, Status = "Active", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddYears(1) });
            await context.SaveChangesAsync();

            var claimService = new ClaimService(context);
            var controller = new ClaimController(claimService);
            SetupControllerUser(controller, customerId, "Customer");

            // Act
            var result = await controller.GetAllowedClaimTypes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseData = okResult.Value;
            var json = System.Text.Json.JsonSerializer.Serialize(responseData);
            
            // Should contain Health and TermLife since LifeCoverageMultiplier is set
            Assert.Contains("Health", json);
            Assert.Contains("TermLife", json);
            Assert.DoesNotContain("Accident", json);
        }

        [Fact]
        public async Task RaiseHealthClaim_WithValidData_CreatesClaimAndReturnsOk()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            int customerId = 101;
            int claimsManagerId = 200;

            context.Users.Add(new User { Id = customerId, Email = "customer@test.com", PasswordHash = "hash", FullName = "Customer", Role = UserRole.Customer, IsActive = true, CreatedAt = DateTime.UtcNow });
            context.Users.Add(new User { Id = claimsManagerId, Email = "manager@test.com", PasswordHash = "hash", FullName = "Manager", Role = UserRole.ClaimsManager, IsActive = true, CreatedAt = DateTime.UtcNow });
            
            context.Policies.Add(new Policy { Id = 1, Name = "Test Policy", HealthCoverage = 50000, IsActive = true, CreatedAt = DateTime.UtcNow });
            context.Companies.Add(new Company { Id = 1, CustomerId = customerId, ClaimsManagerId = claimsManagerId, CompanyName = "Test Co", Size = 10, CreatedAt = DateTime.UtcNow });
            context.CompanyPolicies.Add(new CompanyPolicy { Id = 1, CompanyId = 1, PolicyId = 1, Status = "Active", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddYears(1) });
            
            context.Employees.Add(new Employee { Id = 10, CompanyId = 1, FullName = "Emp 1", Email = "emp@test.com", EmployeeCode = "EMP1", Salary = 50000, IsActive = true, CreatedAt = DateTime.UtcNow });
            
            await context.SaveChangesAsync();

            var claimService = new ClaimService(context);
            var controller = new ClaimController(claimService);
            SetupControllerUser(controller, customerId, "Customer");

            var dto = new RaiseHealthClaimDto
            {
                EmployeeId = 10,
                RequestedAmount = 10000,
                DocumentUrl = "http://localhost/doc.pdf"
            };

            // Act
            var result = await controller.RaiseHealthClaim(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Health claim submitted successfully", okResult.Value.ToString());

            var claimInDb = await context.Claims.FirstOrDefaultAsync(c => c.EmployeeId == 10);
            Assert.NotNull(claimInDb);
            Assert.Equal("Health", claimInDb.ClaimType);
            Assert.Equal("Pending", claimInDb.Status);
            Assert.Equal(10000, claimInDb.ClaimAmount);
        }

        [Fact]
        public async Task ProcessClaim_ApproveHealthClaim_UpdatesStatusAndCoverage()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            int managerId = 201;

            context.Users.Add(new User { Id = managerId, Email = "manager2@test.com", PasswordHash = "hash", FullName = "Manager2", Role = UserRole.ClaimsManager, IsActive = true, CreatedAt = DateTime.UtcNow });
            context.Policies.Add(new Policy { Id = 1, Name = "Test Policy", HealthCoverage = 50000, IsActive = true, CreatedAt = DateTime.UtcNow });
            context.CompanyPolicies.Add(new CompanyPolicy { Id = 1, CompanyId = 1, PolicyId = 1, Status = "Active", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddYears(1) });
            context.Employees.Add(new Employee { Id = 11, CompanyId = 1, FullName = "Emp", Email = "e@t.com", EmployeeCode = "E1", Salary = 50000, IsActive = true, HealthCoverageRemaining = 50000, CreatedAt = DateTime.UtcNow });
            
            context.Claims.Add(new Domain.Entities.Claim
            {
                Id = 50,
                CompanyPolicyId = 1,
                EmployeeId = 11,
                CustomerId = 1,
                ClaimsManagerId = managerId,
                ClaimType = "Health",
                ClaimAmount = 20000,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var claimService = new ClaimService(context);
            var controller = new ClaimController(claimService);
            SetupControllerUser(controller, managerId, "ClaimsManager");

            var processDto = new ProcessClaimDto
            {
                Decision = "Approved",
                Note = "Looks good"
            };

            // Act
            var result = await controller.ProcessClaim(50, processDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var updatedClaim = await context.Claims.FindAsync(50);
            Assert.Equal("Approved", updatedClaim!.Status);
            Assert.Equal("Looks good", updatedClaim.ClaimsManagerNote);
            Assert.NotNull(updatedClaim.ProcessedAt);

            var updatedEmployee = await context.Employees.FindAsync(11);
            Assert.Equal(30000, updatedEmployee!.HealthCoverageRemaining); // 50000 - 20000
        }
    }
}
