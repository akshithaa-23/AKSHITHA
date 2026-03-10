using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Infrastructure.Tests.Data
{
    public class AppDbContextTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated(); // Explicitly call this to run OnModelCreating and Seed data
            return context;
        }

        [Fact]
        public async Task Can_Add_User_To_Database()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var user = new User
            {
                Id = 10,
                FullName = "Test User",
                Email = "testuser@example.com",
                PasswordHash = "hashed",
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Assert
            var savedUser = await context.Users.FindAsync(10);
            Assert.NotNull(savedUser);
            Assert.Equal("Test User", savedUser.FullName);
        }

        [Fact]
        public void DbContext_Has_Seeded_Policies()
        {
            // Arrange
            using var context = GetInMemoryDbContext();

            // Act
            var policiesCount = context.Policies.Count();

            // Assert
            Assert.True(policiesCount >= 9, "Seeded policies should be present in the database.");
            var essentialBasePolicy = context.Policies.FirstOrDefault(p => p.Name == "Essential Base");
            Assert.NotNull(essentialBasePolicy);
            Assert.Equal(200000, essentialBasePolicy.HealthCoverage);
        }

        [Fact]
        public async Task Can_Add_Company_With_Relations()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            
            var customer = new User { Id = 11, FullName = "Cust", Email = "cust11@test.com", PasswordHash = "h", Role = UserRole.Customer, IsActive = true, CreatedAt = DateTime.UtcNow };
            context.Users.Add(customer);
            await context.SaveChangesAsync();

            var company = new Company
            {
                Id = 1,
                CustomerId = 11,
                CompanyName = "Test Company",
                Size = 100,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            context.Companies.Add(company);
            await context.SaveChangesAsync();

            // Assert
            var savedCompany = await context.Companies.Include(c => c.Customer).FirstOrDefaultAsync(c => c.Id == 1);
            Assert.NotNull(savedCompany);
            Assert.NotNull(savedCompany.Customer);
            Assert.Equal(11, savedCompany.Customer.Id);
            Assert.Equal("Test Company", savedCompany.CompanyName);
        }
    }
}
