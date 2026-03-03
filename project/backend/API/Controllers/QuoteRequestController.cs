using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
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
    public class QuoteRequestController : ControllerBase
    {
        private readonly AppDbContext _context;
        public QuoteRequestController(AppDbContext context) { _context = context; }

        // ── Shared agent assignment logic ──────────────────────────────
        private async Task<int?> GetOrAssignAgentAsync(int customerId)
        {
            // If customer already has an agent from any previous request, reuse it
            var existingAgentId = await _context.QuoteRequests
                .Where(q => q.CustomerId == customerId && q.AssignedAgentId != null)
                .Select(q => q.AssignedAgentId)
                .FirstOrDefaultAsync();

            if (existingAgentId != null)
                return existingAgentId;

            // New customer — assign agent with least customers, tiebreak by earliest created
            var agent = await _context.Users
                .Where(u => u.Role == UserRole.Agent && u.IsActive)
                .OrderBy(u => _context.QuoteRequests
                    .Count(q => q.AssignedAgentId == u.Id))
                .ThenBy(u => u.CreatedAt)
                .FirstOrDefaultAsync();

            return agent?.Id;
        }

        // ── Save/update company profile ────────────────────────────────
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
        }

        // POST api/quoterequest/recommendation
        // Called when customer clicks "View Recommendation"
        // Backend auto-sets RequestType = "Recommendation"
        [HttpPost("recommendation")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RequestRecommendation([FromBody] QuoteRequestDto dto)
        {
            int customerId = GetUserId();

            // Check: company already has an active policy
            var hasActivePolicy = await _context.CompanyPolicies
                .Include(cp => cp.Company)
                .AnyAsync(cp => cp.Company.CustomerId == customerId && cp.Status == "Active");

            if (hasActivePolicy)
                return BadRequest(new { message = "Your company already has an active policy" });

            int? agentId = await GetOrAssignAgentAsync(customerId);
            await SaveCompanyProfileAsync(customerId, agentId, dto.CompanyName,
                dto.IndustryType, dto.NumberOfEmployees, dto.ContactName, dto.ContactEmail);

            var request = new QuoteRequest
            {
                CustomerId = customerId,
                RequestType = "Recommendation",  // set by backend
                PolicyId = null,
                CompanyName = dto.CompanyName,
                IndustryType = dto.IndustryType,
                NumberOfEmployees = dto.NumberOfEmployees,
                Location = dto.Location,
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

            return Ok(new
            {
                message = "Recommendation request submitted. Your agent will contact you soon.",
                quoteRequestId = request.Id,
                assignedAgent = agentName
            });
        }

        // POST api/quoterequest/direct-buy
        // Called when customer clicks "Buy Now" on any policy
        // Backend auto-sets RequestType = "DirectBuy"
        [HttpPost("direct-buy")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> DirectBuy([FromBody] DirectBuyRequestDto dto)
        {
            int customerId = GetUserId();

            // Check: company already has an active policy
            var hasActivePolicy = await _context.CompanyPolicies
                .Include(cp => cp.Company)
                .AnyAsync(cp => cp.Company.CustomerId == customerId && cp.Status == "Active");

            if (hasActivePolicy)
                return BadRequest(new { message = "Your company already has an active policy" });

            // Validate policy exists
            var policy = await _context.Policies.FindAsync(dto.PolicyId);
            if (policy == null) return NotFound(new { message = "Policy not found" });

            // Validate employee count meets policy minimum
            if (dto.NumberOfEmployees < policy.MinEmployees)
                return BadRequest(new
                {
                    message = $"This policy requires a minimum of {policy.MinEmployees} employees"
                });

            int? agentId = await GetOrAssignAgentAsync(customerId);
            await SaveCompanyProfileAsync(customerId, agentId, dto.CompanyName,
                dto.IndustryType, dto.NumberOfEmployees, dto.ContactName, dto.ContactEmail);

            var request = new QuoteRequest
            {
                CustomerId = customerId,
                RequestType = "DirectBuy",  // set by backend
                PolicyId = dto.PolicyId,
                CompanyName = dto.CompanyName,
                IndustryType = dto.IndustryType,
                NumberOfEmployees = dto.NumberOfEmployees,
                Location = dto.Location,
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

            return Ok(new
            {
                message = "Quote request submitted. Your agent will calculate and send the quote.",
                quoteRequestId = request.Id,
                assignedAgent = agentName
            });
        }

        // GET api/quoterequest/my — customer views their requests
        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyRequests()
        {
            int customerId = GetUserId();
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
                    ContactName = q.ContactName,
                    ContactEmail = q.ContactEmail,
                    ContactPhone = q.ContactPhone,
                    Status = q.Status,
                    AssignedAgentName = q.AssignedAgent != null ? q.AssignedAgent.FullName : null,
                    AssignedAgentEmail = q.AssignedAgent != null ? q.AssignedAgent.Email : null,
                    CreatedAt = q.CreatedAt
                }).ToListAsync();

            return Ok(requests);
        }

        // GET api/quoterequest/all — admin sees all
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
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
                    ContactName = q.ContactName,
                    ContactEmail = q.ContactEmail,
                    ContactPhone = q.ContactPhone,
                    Status = q.Status,
                    AssignedAgentName = q.AssignedAgent != null ? q.AssignedAgent.FullName : null,
                    AssignedAgentEmail = q.AssignedAgent != null ? q.AssignedAgent.Email : null,
                    CustomerName = q.Customer.FullName,
                    CreatedAt = q.CreatedAt
                }).ToListAsync();

            return Ok(requests);
        }

        // GET api/quoterequest/agent — agent sees their assigned requests
        [HttpGet("agent")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> GetAgentRequests()
        {
            int agentId = GetUserId();
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
                    ContactName = q.ContactName,
                    ContactEmail = q.ContactEmail,
                    ContactPhone = q.ContactPhone,
                    Status = q.Status,
                    CustomerName = q.Customer.FullName,
                    CreatedAt = q.CreatedAt
                }).ToListAsync();

            return Ok(requests);
        }

        private int GetUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}