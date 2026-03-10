using Domain.Enums;
using System;
using Xunit;

namespace Domain.Tests.Enums
{
    public class UserRoleTests
    {
        [Theory]
        [InlineData("Admin", UserRole.Admin)]
        [InlineData("Customer", UserRole.Customer)]
        [InlineData("Agent", UserRole.Agent)]
        [InlineData("ClaimsManager", UserRole.ClaimsManager)]
        public void UserRole_Enum_ShouldParseCorrectly(string roleString, UserRole expectedRole)
        {
            // Act
            bool success = Enum.TryParse<UserRole>(roleString, out var parsedRole);

            // Assert
            Assert.True(success);
            Assert.Equal(expectedRole, parsedRole);
        }

        [Fact]
        public void UserRole_Enum_ShouldHaveExpectedValues()
        {
            // Assert
            Assert.Equal(1, (int)UserRole.Admin);
            Assert.Equal(2, (int)UserRole.Agent);
            Assert.Equal(3, (int)UserRole.ClaimsManager);
            Assert.Equal(4, (int)UserRole.Customer);
        }
    }
}
