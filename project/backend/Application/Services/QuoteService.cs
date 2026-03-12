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
    public class QuoteService : IQuoteService
    {
        private readonly IAppDbContext _context;
        private readonly IPremiumCalculationService _premiumCalculationService;

        public QuoteService(IAppDbContext context, IPremiumCalculationService premiumCalculationService)
        {
            _context = context;
            _premiumCalculationService = premiumCalculationService;
        }

        public async Task<object> SendQuoteAsync(int agentId, SendQuoteDto dto)
        {
            var quoteRequest = await _context.QuoteRequests.FindAsync(dto.QuoteRequestId);
            if (quoteRequest == null)
                throw new KeyNotFoundException("Quote request not found");

            if (quoteRequest.AssignedAgentId != agentId)
                throw new UnauthorizedAccessException("Not authorized to act on this quote request");

            var policy = await _context.Policies.FindAsync(dto.PolicyId);
            if (policy == null)
                throw new KeyNotFoundException("Policy not found");

            if (dto.EmployeeCount < policy.MinEmployees)
                throw new ArgumentException($"Minimum {policy.MinEmployees} employees required for this policy");

            decimal baseQuote = dto.EmployeeCount * policy.PremiumPerEmployee;
            decimal industryFactor = quoteRequest.IndustryFactor ?? 1.00m;
            decimal geographyFactor = quoteRequest.GeographyFactor ?? 1.00m;
            decimal planRiskFactor = _premiumCalculationService.GetPlanRiskFactor(dto.PolicyId);
            decimal totalPremium = baseQuote * industryFactor * geographyFactor * planRiskFactor;

            quoteRequest.PlanRiskFactor = planRiskFactor;

            var quote = new Quote
            {
                QuoteRequestId = dto.QuoteRequestId,
                AgentId = agentId,
                CustomerId = quoteRequest.CustomerId,
                PolicyId = dto.PolicyId,
                EmployeeCount = dto.EmployeeCount,
                PremiumPerEmployee = policy.PremiumPerEmployee,
                BaseQuote = baseQuote,
                IndustryFactor = industryFactor,
                GeographyFactor = geographyFactor,
                PlanRiskFactor = planRiskFactor,
                TotalPremium = totalPremium,
                Status = "Pending",
                ValidUntil = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            };

            quoteRequest.Status = "QuoteSent";
            quoteRequest.PolicyId = dto.PolicyId;

            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();

            return new
            {
                message = "Quote sent successfully",
                quoteId = quote.Id,
                totalPremium,
                validUntil = quote.ValidUntil,
                breakdown = new PremiumBreakdownDto
                {
                    PerEmployeePremium = policy.PremiumPerEmployee,
                    EmployeeCount = dto.EmployeeCount,
                    BaseQuote = baseQuote,
                    IndustryMultiplier = industryFactor,
                    GeographyMultiplier = geographyFactor,
                    PlanMultiplier = planRiskFactor,
                    FinalPremium = totalPremium
                }
            };
        }

        public async Task<IEnumerable<QuoteResponseDto>> GetMyQuotesAsync(int customerId)
        {
            var quotes = await _context.Quotes
                .Include(q => q.Policy)
                .Include(q => q.Agent)
                .Include(q => q.QuoteRequest)
                .Where(q => q.CustomerId == customerId)
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new QuoteResponseDto
                {
                    Id = q.Id,
                    QuoteRequestId = q.QuoteRequestId,
                    PolicyId = q.PolicyId,
                    PolicyName = q.Policy.Name,
                    EmployeeCount = q.EmployeeCount,
                    PremiumPerEmployee = q.PremiumPerEmployee,
                    BaseQuote = q.BaseQuote,
                    IndustryFactor = q.IndustryFactor,
                    GeographyFactor = q.GeographyFactor,
                    PlanRiskFactor = q.PlanRiskFactor,
                    TotalPremium = q.TotalPremium,
                    Breakdown = new PremiumBreakdownDto
                    {
                        PerEmployeePremium = q.PremiumPerEmployee,
                        EmployeeCount = q.EmployeeCount,
                        BaseQuote = q.BaseQuote,
                        IndustryMultiplier = q.IndustryFactor,
                        GeographyMultiplier = q.GeographyFactor,
                        PlanMultiplier = q.PlanRiskFactor,
                        FinalPremium = q.TotalPremium
                    },
                    Status = q.Status,
                    AgentName = q.Agent.FullName,
                    ValidUntil = q.ValidUntil,
                    CreatedAt = q.CreatedAt
                }).ToListAsync();

            return quotes;
        }

        public async Task<IEnumerable<QuoteResponseDto>> GetAgentQuotesAsync(int agentId)
        {
            var quotes = await _context.Quotes
                .Include(q => q.Policy)
                .Include(q => q.Customer)
                .Where(q => q.AgentId == agentId)
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new QuoteResponseDto
                {
                    Id = q.Id,
                    QuoteRequestId = q.QuoteRequestId,
                    PolicyId = q.PolicyId,
                    PolicyName = q.Policy.Name,
                    EmployeeCount = q.EmployeeCount,
                    PremiumPerEmployee = q.PremiumPerEmployee,
                    BaseQuote = q.BaseQuote,
                    IndustryFactor = q.IndustryFactor,
                    GeographyFactor = q.GeographyFactor,
                    PlanRiskFactor = q.PlanRiskFactor,
                    TotalPremium = q.TotalPremium,
                    Breakdown = new PremiumBreakdownDto
                    {
                        PerEmployeePremium = q.PremiumPerEmployee,
                        EmployeeCount = q.EmployeeCount,
                        BaseQuote = q.BaseQuote,
                        IndustryMultiplier = q.IndustryFactor,
                        GeographyMultiplier = q.GeographyFactor,
                        PlanMultiplier = q.PlanRiskFactor,
                        FinalPremium = q.TotalPremium
                    },
                    Status = q.Status,
                    AgentName = q.Customer.FullName,
                    ValidUntil = q.ValidUntil,
                    CreatedAt = q.CreatedAt
                }).ToListAsync();

            return quotes;
        }

        public async Task<string> AcceptQuoteAsync(int customerId, int quoteId)
        {
            var quote = await _context.Quotes
                .FirstOrDefaultAsync(q => q.Id == quoteId && q.CustomerId == customerId);

            if (quote == null)
                throw new KeyNotFoundException("Quote not found");

            if (quote.Status != "Pending")
                throw new ArgumentException("Quote already processed");

            quote.Status = "Accepted";
            var qr = await _context.QuoteRequests.FindAsync(quote.QuoteRequestId);
            if (qr != null) qr.Status = "Accepted";

            await _context.SaveChangesAsync();
            return "Quote accepted. Proceed to payment.";
        }

        public async Task<string> RejectQuoteAsync(int customerId, int quoteId)
        {
            var quote = await _context.Quotes
                .FirstOrDefaultAsync(q => q.Id == quoteId && q.CustomerId == customerId);

            if (quote == null)
                throw new KeyNotFoundException("Quote not found");

            quote.Status = "Rejected";
            var qr = await _context.QuoteRequests.FindAsync(quote.QuoteRequestId);
            if (qr != null) qr.Status = "Rejected";

            await _context.SaveChangesAsync();
            return "Quote rejected";
        }
    }
}
