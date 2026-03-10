using Application.DTOs;
using System;
using Xunit;

namespace Application.Tests.DTOs
{
    public class DtoTests
    {
        [Fact]
        public void RegisterCustomerDto_CanBeInstantiated_WithProperties()
        {
            var dto = new RegisterCustomerDto
            {
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "Password123"
            };

            Assert.Equal("John Doe", dto.FullName);
            Assert.Equal("john@example.com", dto.Email);
            Assert.Equal("Password123", dto.Password);
        }

        [Fact]
        public void ProcessPaymentDto_CanBeInstantiated()
        {
            var dto = new ProcessPaymentDto
            {
                QuoteId = 1,
                PaymentMethod = "Credit Card",
                CardHolderName = "Jane Doe",
                CardNumber = "4444555566667777"
            };

            Assert.Equal(1, dto.QuoteId);
            Assert.Equal("Credit Card", dto.PaymentMethod);
            Assert.Equal("Jane Doe", dto.CardHolderName);
            Assert.Equal("4444555566667777", dto.CardNumber);
        }
    }
}
