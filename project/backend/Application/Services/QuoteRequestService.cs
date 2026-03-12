using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class QuoteRequestService : IQuoteRequestService
    {
        private readonly IAppDbContext _context;
        private readonly IPremiumCalculationService _premiumCalculationService;

        public QuoteRequestService(IAppDbContext context, IPremiumCalculationService premiumCalculationService)
        {
            _context = context;
            _premiumCalculationService = premiumCalculationService;
        }

        private async Task<int?> GetOrAssignAgentAsync(int customerId)
        {
            var existingAgentId = await _context.QuoteRequests
                .Where(q => q.CustomerId == customerId && q.AssignedAgentId != null)
                .Select(q => q.AssignedAgentId)
                .FirstOrDefaultAsync();

            if (existingAgentId != null)
                return existingAgentId;

            var agent = await _context.Users
                .Where(u => u.Role == UserRole.Agent && u.IsActive)
                .OrderBy(u => _context.QuoteRequests
                    .Count(q => q.AssignedAgentId == u.Id))
                .ThenBy(u => u.CreatedAt)
                .FirstOrDefaultAsync();

            return agent?.Id;
        }

        private async Task SaveCompanyProfileAsync(int customerId, int? agentId, string companyName,
            string industryType, int numberOfEmployees, string contactName, string contactEmail)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (company == null)
            {
                _context.Companies.Add(new Company
                {
                    CustomerId = customerId,
                    CompanyName = companyName,
                    Size = numberOfEmployees,
                    Domain = industryType,
                    RepresentativeName = contactName,
                    RepresentativeEmail = contactEmail,
                    AgentId = agentId,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                // Update existing company with agent if not already assigned
                if (company.AgentId == null && agentId != null)
                {
                    company.AgentId = agentId;
                }

                // Also update other details as they might have been refined in the form
                company.CompanyName = companyName;
                company.Size = numberOfEmployees;
                company.Domain = industryType;
                company.RepresentativeName = contactName;
                company.RepresentativeEmail = contactEmail;

                _context.Companies.Update(company);
            }
        }

        public async Task<object> RequestRecommendationAsync(int customerId, QuoteRequestDto dto)
        {
            int? agentId = await GetOrAssignAgentAsync(customerId);
            await SaveCompanyProfileAsync(customerId, agentId, dto.CompanyName,
                dto.IndustryType, dto.NumberOfEmployees, dto.ContactName, dto.ContactEmail);

            var request = new QuoteRequest
            {
                CustomerId = customerId,
                RequestType = "Recommendation",
                PolicyId = null,
                CompanyName = dto.CompanyName,
                IndustryType = dto.IndustryType,
                CustomIndustry = dto.CustomIndustry,
                IndustryFactor = _premiumCalculationService.GetIndustryFactor(dto.IndustryType, dto.CustomIndustry),
                NumberOfEmployees = dto.NumberOfEmployees,
                Location = dto.Location,
                LocationCategory = dto.LocationCategory,
                GeographyFactor = _premiumCalculationService.GetGeographyFactor(dto.Location, dto.LocationCategory),
                ContactName = dto.ContactName,
                ContactEmail = dto.ContactEmail,
                ContactPhone = dto.ContactPhone,
                Status = agentId != null ? "Assigned" : "Pending",
                AssignedAgentId = agentId,
                CreatedAt = DateTime.UtcNow
            };

            _context.QuoteRequests.Add(request);
            await _context.SaveChangesAsync();

            var agentName = agentId.HasValue
                ? (await _context.Users.FindAsync(agentId))?.FullName ?? "Pending"
                : "Will be assigned shortly";

            return new
            {
                message = "Recommendation request submitted. Your agent will contact you soon.",
                quoteRequestId = request.Id,
                assignedAgent = agentName
            };
        }

        public async Task<object> DirectBuyAsync(int customerId, DirectBuyRequestDto dto)
        {
            var policy = await _context.Policies.FindAsync(dto.PolicyId);
            if (policy == null) throw new KeyNotFoundException("Policy not found");

            if (dto.NumberOfEmployees < policy.MinEmployees)
                throw new ArgumentException($"This policy requires a minimum of {policy.MinEmployees} employees");

            int? agentId = await GetOrAssignAgentAsync(customerId);
            await SaveCompanyProfileAsync(customerId, agentId, dto.CompanyName,
                dto.IndustryType, dto.NumberOfEmployees, dto.ContactName, dto.ContactEmail);

            var request = new QuoteRequest
            {
                CustomerId = customerId,
                RequestType = "DirectBuy",
                PolicyId = dto.PolicyId,
                CompanyName = dto.CompanyName,
                IndustryType = dto.IndustryType,
                CustomIndustry = dto.CustomIndustry,
                IndustryFactor = _premiumCalculationService.GetIndustryFactor(dto.IndustryType, dto.CustomIndustry),
                PlanRiskFactor = _premiumCalculationService.GetPlanRiskFactor(dto.PolicyId),
                NumberOfEmployees = dto.NumberOfEmployees,
                Location = dto.Location,
                LocationCategory = dto.LocationCategory,
                GeographyFactor = _premiumCalculationService.GetGeographyFactor(dto.Location, dto.LocationCategory),
                ContactName = dto.ContactName,
                ContactEmail = dto.ContactEmail,
                ContactPhone = dto.ContactPhone,
                Status = agentId != null ? "Assigned" : "Pending",
                AssignedAgentId = agentId,
                CreatedAt = DateTime.UtcNow
            };

            _context.QuoteRequests.Add(request);
            await _context.SaveChangesAsync();

            var agentName = agentId.HasValue
                ? (await _context.Users.FindAsync(agentId))?.FullName ?? "Pending"
                : "Will be assigned shortly";

            return new
            {
                message = "Quote request submitted. Your agent will calculate and send the quote.",
                quoteRequestId = request.Id,
                assignedAgent = agentName
            };
        }

        public async Task<IEnumerable<QuoteRequestResponseDto>> GetMyRequestsAsync(int customerId)
        {
            var requests = await _context.QuoteRequests
                .Include(q => q.AssignedAgent)
                .Include(q => q.Policy)
                .Where(q => q.CustomerId == customerId)
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new QuoteRequestResponseDto
                {
                    Id = q.Id,
                    PolicyId = q.PolicyId,
                    PolicyName = q.Policy != null ? q.Policy.Name : null,
                    RequestType = q.RequestType,
                    CompanyName = q.CompanyName,
                    IndustryType = q.IndustryType,
                    NumberOfEmployees = q.NumberOfEmployees,
                    Location = q.Location,
                    LocationCategory = q.LocationCategory,
                    CustomIndustry = q.CustomIndustry,
                    IndustryRiskFactor = q.IndustryFactor,
                    GeographyRiskFactor = q.GeographyFactor,
                    PlanRiskFactor = q.PlanRiskFactor,
                    ContactName = q.ContactName,
                    ContactEmail = q.ContactEmail,
                    ContactPhone = q.ContactPhone,
                    Status = q.Status,
                    AssignedAgentName = q.AssignedAgent != null ? q.AssignedAgent.FullName : null,
                    AssignedAgentEmail = q.AssignedAgent != null ? q.AssignedAgent.Email : null,
                    CreatedAt = q.CreatedAt
                }).ToListAsync();

            return requests;
        }

        public async Task<IEnumerable<QuoteRequestResponseDto>> GetAllAsync()
        {
            var requests = await _context.QuoteRequests
                .Include(q => q.Customer)
                .Include(q => q.AssignedAgent)
                .Include(q => q.Policy)
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new QuoteRequestResponseDto
                {
                    Id = q.Id,
                    PolicyId = q.PolicyId,
                    PolicyName = q.Policy != null ? q.Policy.Name : null,
                    RequestType = q.RequestType,
                    CompanyName = q.CompanyName,
                    IndustryType = q.IndustryType,
                    NumberOfEmployees = q.NumberOfEmployees,
                    Location = q.Location,
                    LocationCategory = q.LocationCategory,
                    CustomIndustry = q.CustomIndustry,
                    IndustryRiskFactor = q.IndustryFactor,
                    GeographyRiskFactor = q.GeographyFactor,
                    PlanRiskFactor = q.PlanRiskFactor,
                    ContactName = q.ContactName,
                    ContactEmail = q.ContactEmail,
                    ContactPhone = q.ContactPhone,
                    Status = q.Status,
                    AssignedAgentName = q.AssignedAgent != null ? q.AssignedAgent.FullName : null,
                    AssignedAgentEmail = q.AssignedAgent != null ? q.AssignedAgent.Email : null,
                    CustomerName = q.Customer.FullName,
                    CreatedAt = q.CreatedAt
                }).ToListAsync();

            return requests;
        }

        public async Task<IEnumerable<QuoteRequestResponseDto>> GetAgentRequestsAsync(int agentId)
        {
            var requests = await _context.QuoteRequests
                .Include(q => q.Customer)
                .Include(q => q.Policy)
                .Where(q => q.AssignedAgentId == agentId)
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new QuoteRequestResponseDto
                {
                    Id = q.Id,
                    PolicyId = q.PolicyId,
                    PolicyName = q.Policy != null ? q.Policy.Name : null,
                    RequestType = q.RequestType,
                    CompanyName = q.CompanyName,
                    IndustryType = q.IndustryType,
                    NumberOfEmployees = q.NumberOfEmployees,
                    Location = q.Location,
                    LocationCategory = q.LocationCategory,
                    CustomIndustry = q.CustomIndustry,
                    IndustryRiskFactor = q.IndustryFactor,
                    GeographyRiskFactor = q.GeographyFactor,
                    PlanRiskFactor = q.PlanRiskFactor,
                    ContactName = q.ContactName,
                    ContactEmail = q.ContactEmail,
                    ContactPhone = q.ContactPhone,
                    Status = q.Status,
                    CustomerName = q.Customer.FullName,
                    CreatedAt = q.CreatedAt
                }).ToListAsync();

            return requests;
        }
    }
}
