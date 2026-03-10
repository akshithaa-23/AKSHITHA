using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IAppDbContext _context;

        public PaymentService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Application.DTOs.PaymentResponseDto> ProcessPaymentAsync(int customerId, Application.DTOs.ProcessPaymentDto dto)
        {
            var quote = await _context.Quotes
                .Include(q => q.Policy)
                .Include(q => q.QuoteRequest)
                .FirstOrDefaultAsync(q => q.Id == dto.QuoteId && q.CustomerId == customerId);

            if (quote == null)
                throw new KeyNotFoundException("Quote not found");

            if (quote.Status == "Pending")
                throw new ArgumentException("Quote must be accepted before payment");

            if (quote.Status == "Rejected")
                throw new ArgumentException("Rejected quote cannot be paid");

            if (quote.Status == "Paid")
                throw new ArgumentException("Payment already completed for this quote");

            if (quote.Status != "Accepted")
                throw new ArgumentException($"Invalid quote status: {quote.Status}");

            // Commission by policy id range
            decimal commissionRate = GetCommissionRate(quote.PolicyId);
            decimal commissionAmount = Math.Round(quote.TotalPremium * commissionRate / 100, 2);

            string invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{quote.Id:D5}";
            string maskedCard = dto.CardNumber.Length >= 4
                ? $"**** **** **** {dto.CardNumber[^4..]}"
                : "****";

            var payment = new Payment
            {
                QuoteId = quote.Id,
                CustomerId = customerId,
                PolicyId = quote.PolicyId,
                PaymentMethod = dto.PaymentMethod,
                CardHolderName = dto.CardHolderName,
                MaskedCardNumber = maskedCard,
                AmountPaid = quote.TotalPremium,
                Status = "Success",
                InvoiceNumber = invoiceNumber,
                PaidAt = DateTime.UtcNow,
                AgentCommission = new AgentCommission
                {
                    AgentId = quote.AgentId,
                    CommissionRate = commissionRate,
                    CommissionAmount = commissionAmount,
                    CreatedAt = DateTime.UtcNow
                }
            };

            // Activate policy for company
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (company != null)
            {
                _context.CompanyPolicies.Add(new CompanyPolicy
                {
                    CompanyId = company.Id,
                    PolicyId = quote.PolicyId,
                    Status = "Active",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(quote.Policy.DurationYears),
                    EmployeeCount = quote.EmployeeCount,
                    TotalPremium = quote.TotalPremium,
                    CreatedAt = DateTime.UtcNow
                });
            }

            quote.Status = "Paid";
            quote.QuoteRequest.Status = "Completed";

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            var agent = await _context.Users.FindAsync(quote.AgentId);

            return new Application.DTOs.PaymentResponseDto
            {
                Id = payment.Id,
                InvoiceNumber = invoiceNumber,
                PolicyName = quote.Policy.Name,
                CompanyName = quote.QuoteRequest.CompanyName,
                EmployeeCount = quote.EmployeeCount,
                AmountPaid = quote.TotalPremium,
                PaymentMethod = dto.PaymentMethod,
                MaskedCardNumber = maskedCard,
                CardHolderName = dto.CardHolderName,
                PaidAt = payment.PaidAt,
                CommissionRate = commissionRate,
                CommissionAmount = commissionAmount,
                AgentName = agent?.FullName ?? ""
            };
        }

        public async Task<IEnumerable<Application.DTOs.PaymentResponseDto>> GetMyPaymentsAsync(int customerId)
        {
            var payments = await _context.Payments
                .Include(p => p.Policy)
                .Include(p => p.Quote).ThenInclude(q => q.QuoteRequest)
                .Include(p => p.Quote).ThenInclude(q => q.Agent)
                .Include(p => p.AgentCommission)
                .Where(p => p.CustomerId == customerId)
                .OrderByDescending(p => p.PaidAt)
                .Select(p => new Application.DTOs.PaymentResponseDto
                {
                    Id = p.Id,
                    InvoiceNumber = p.InvoiceNumber,
                    PolicyName = p.Policy.Name,
                    CompanyName = p.Quote.QuoteRequest.CompanyName,
                    EmployeeCount = p.Quote.EmployeeCount,
                    AmountPaid = p.AmountPaid,
                    PaymentMethod = p.PaymentMethod,
                    MaskedCardNumber = p.MaskedCardNumber,
                    CardHolderName = p.CardHolderName,
                    PaidAt = p.PaidAt,
                    CommissionRate = p.AgentCommission != null ? p.AgentCommission.CommissionRate : 0,
                    CommissionAmount = p.AgentCommission != null ? p.AgentCommission.CommissionAmount : 0,
                    AgentName = p.Quote.Agent.FullName
                }).ToListAsync();

            return payments;
        }

        public async Task<IEnumerable<object>> GetAgentCommissionsAsync(int agentId)
        {
            var commissions = await _context.AgentCommissions
                .Include(ac => ac.Payment).ThenInclude(p => p.Policy)
                .Include(ac => ac.Payment).ThenInclude(p => p.Customer)
                .Where(ac => ac.AgentId == agentId)
                .OrderByDescending(ac => ac.CreatedAt)
                .Select(ac => new
                {
                    ac.Id,
                    ac.Payment.InvoiceNumber,
                    PolicyName = ac.Payment.Policy.Name,
                    CustomerName = ac.Payment.Customer.FullName,
                    ac.Payment.AmountPaid,
                    ac.CommissionRate,
                    ac.CommissionAmount,
                    EarnedAt = ac.CreatedAt
                }).ToListAsync();

            return commissions;
        }

        private static decimal GetCommissionRate(int policyId)
        {
            if (policyId >= 1 && policyId <= 3) return 5m;
            if (policyId >= 4 && policyId <= 6) return 7m;
            if (policyId >= 7 && policyId <= 9) return 10m;
            return 7m;
        }
    }
}
