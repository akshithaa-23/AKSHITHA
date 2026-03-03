using Application.DTOs;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecommendationController : ControllerBase
    {
        private readonly AppDbContext _context;
        public RecommendationController(AppDbContext context) { _context = context; }

        // POST api/recommendation
        // Agent sends recommendation — policies auto-selected by employee count
        // 10-80    → Essential (id 1,2,3)
        // 81-200   → Enhanced  (id 4,5,6)
        // 201+     → Enterprise (id 7,8,9)
        [HttpPost]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> Send([FromBody] SendRecommendationDto dto)
        {
            int agentId = GetUserId();

            var quoteRequest = await _context.QuoteRequests
                .FirstOrDefaultAsync(q => q.Id == dto.QuoteRequestId);

            if (quoteRequest == null)
                return NotFound(new { message = "Quote request not found" });

            if (quoteRequest.AssignedAgentId != agentId)
                return Forbid();

            if (quoteRequest.RequestType != "Recommendation")
                return BadRequest(new { message = "This request is not a recommendation request" });

            // Auto-determine which policies to recommend based on employee count
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

            return Ok(new
            {
                message = "Recommendation sent successfully",
                recommendationId = recommendation.Id,
                policiesRecommended = policies.Select(p => p.Name).ToList()
            });
        }

        // GET api/recommendation/my — Customer views recommendations
        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyRecommendations()
        {
            int customerId = GetUserId();

            var recommendations = await _context.Recommendations
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

            return Ok(recommendations);
        }

        // GET api/recommendation/agent — Agent views what they sent
        [HttpGet("agent")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> GetAgentRecommendations()
        {
            int agentId = GetUserId();

            var recommendations = await _context.Recommendations
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

            return Ok(recommendations);
        }

        private int GetUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}