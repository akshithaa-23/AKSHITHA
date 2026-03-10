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
    public class RecommendationService : IRecommendationService
    {
        private readonly IAppDbContext _context;

        public RecommendationService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<object> SendRecommendationAsync(int agentId, SendRecommendationDto dto)
        {
            var quoteRequest = await _context.QuoteRequests
                .FirstOrDefaultAsync(q => q.Id == dto.QuoteRequestId);

            if (quoteRequest == null)
                throw new KeyNotFoundException("Quote request not found");

            if (quoteRequest.AssignedAgentId != agentId)
                throw new UnauthorizedAccessException();

            if (quoteRequest.RequestType != "Recommendation")
                throw new InvalidOperationException("This request is not a recommendation request");

            List<int> policyIds;
            int employeeCount = quoteRequest.NumberOfEmployees;

            if (employeeCount <= 80)
                policyIds = new List<int> { 1, 2, 3 };       // Essential
            else if (employeeCount <= 200)
                policyIds = new List<int> { 4, 5, 6 };       // Enhanced
            else
                policyIds = new List<int> { 7, 8, 9 };       // Enterprise

            var policies = await _context.Policies
                .Where(p => policyIds.Contains(p.Id) && p.IsActive)
                .ToListAsync();

            var recommendation = new Recommendation
            {
                QuoteRequestId = dto.QuoteRequestId,
                AgentId = agentId,
                CustomerId = quoteRequest.CustomerId,
                AgentMessage = dto.AgentMessage,
                CreatedAt = DateTime.UtcNow,
                RecommendationPolicies = policies.Select(p => new RecommendationPolicy
                {
                    PolicyId = p.Id
                }).ToList()
            };

            quoteRequest.Status = "RecommendationSent";

            _context.Recommendations.Add(recommendation);
            await _context.SaveChangesAsync();

            return new
            {
                message = "Recommendation sent successfully",
                recommendationId = recommendation.Id,
                policiesRecommended = policies.Select(p => p.Name).ToList()
            };
        }

        public async Task<IEnumerable<RecommendationResponseDto>> GetMyRecommendationsAsync(int customerId)
        {
            return await _context.Recommendations
                .Include(r => r.Agent)
                .Include(r => r.QuoteRequest)
                .Include(r => r.RecommendationPolicies).ThenInclude(rp => rp.Policy)
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new RecommendationResponseDto
                {
                    Id = r.Id,
                    QuoteRequestId = r.QuoteRequestId,
                    AgentName = r.Agent.FullName,
                    AgentMessage = r.AgentMessage,
                    CompanyName = r.QuoteRequest.CompanyName,
                    NumberOfEmployees = r.QuoteRequest.NumberOfEmployees,
                    RecommendedPolicies = r.RecommendationPolicies.Select(rp => new PolicyDto
                    {
                        Id = rp.Policy.Id,
                        Name = rp.Policy.Name,
                        HealthCoverage = rp.Policy.HealthCoverage,
                        LifeCoverageMultiplier = rp.Policy.LifeCoverageMultiplier,
                        MaxLifeCoverageLimit = rp.Policy.MaxLifeCoverageLimit,
                        AccidentCoverage = rp.Policy.AccidentCoverage,
                        PremiumPerEmployee = rp.Policy.PremiumPerEmployee,
                        MinEmployees = rp.Policy.MinEmployees,
                        DurationYears = rp.Policy.DurationYears,
                        IsPopular = rp.Policy.IsPopular,
                        IsActive = rp.Policy.IsActive,
                        CreatedAt = rp.Policy.CreatedAt
                    }).ToList(),
                    CreatedAt = r.CreatedAt
                }).ToListAsync();
        }

        public async Task<IEnumerable<RecommendationResponseDto>> GetAgentRecommendationsAsync(int agentId)
        {
            return await _context.Recommendations
                .Include(r => r.Customer)
                .Include(r => r.QuoteRequest)
                .Include(r => r.RecommendationPolicies).ThenInclude(rp => rp.Policy)
                .Where(r => r.AgentId == agentId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new RecommendationResponseDto
                {
                    Id = r.Id,
                    QuoteRequestId = r.QuoteRequestId,
                    AgentName = r.Customer.FullName,
                    AgentMessage = r.AgentMessage,
                    CompanyName = r.QuoteRequest.CompanyName,
                    NumberOfEmployees = r.QuoteRequest.NumberOfEmployees,
                    RecommendedPolicies = r.RecommendationPolicies.Select(rp => new PolicyDto
                    {
                        Id = rp.Policy.Id,
                        Name = rp.Policy.Name,
                        HealthCoverage = rp.Policy.HealthCoverage,
                        LifeCoverageMultiplier = rp.Policy.LifeCoverageMultiplier,
                        MaxLifeCoverageLimit = rp.Policy.MaxLifeCoverageLimit,
                        AccidentCoverage = rp.Policy.AccidentCoverage,
                        PremiumPerEmployee = rp.Policy.PremiumPerEmployee,
                        MinEmployees = rp.Policy.MinEmployees,
                        DurationYears = rp.Policy.DurationYears,
                        IsPopular = rp.Policy.IsPopular,
                        IsActive = rp.Policy.IsActive,
                        CreatedAt = rp.Policy.CreatedAt
                    }).ToList(),
                    CreatedAt = r.CreatedAt
                }).ToListAsync();
        }
    }
}
