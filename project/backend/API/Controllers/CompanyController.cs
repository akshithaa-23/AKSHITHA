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
    public class CompanyController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CompanyController(AppDbContext context) { _context = context; }

        // GET api/company/my — Customer views their own company profile
        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyCompany()
        {
            int customerId = GetUserId();

            var company = await _context.Companies
                .Include(c => c.Agent)
                .Include(c => c.CompanyPolicies).ThenInclude(cp => cp.Policy)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (company == null)
                return NotFound(new { message = "No company registered yet" });

            var activePolicy = company.CompanyPolicies
                .FirstOrDefault(cp => cp.Status == "Active");

            return Ok(new
            {
                company.Id,
                company.CompanyName,
                company.Size,
                company.Domain,
                company.RepresentativeName,
                company.RepresentativeEmail,
                company.CreatedAt,
                AgentName = company.Agent?.FullName,
                AgentEmail = company.Agent?.Email,
                ActivePolicy = activePolicy == null ? null : new
                {
                    activePolicy.Policy.Id,
                    activePolicy.Policy.Name,
                    activePolicy.Policy.PremiumPerEmployee,
                    activePolicy.EmployeeCount,
                    activePolicy.TotalPremium,
                    activePolicy.StartDate,
                    activePolicy.EndDate,
                    activePolicy.Status
                }
            });
        }

        // GET api/company/all — Admin: all companies with policy + agent info
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var companies = await _context.Companies
                .Include(c => c.Customer)
                .Include(c => c.Agent)
                .Include(c => c.CompanyPolicies).ThenInclude(cp => cp.Policy)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.CompanyName,
                    c.Size,
                    c.Domain,
                    c.RepresentativeName,
                    c.RepresentativeEmail,
                    c.CreatedAt,
                    CustomerName = c.Customer.FullName,
                    CustomerEmail = c.Customer.Email,
                    AgentName = c.Agent != null ? c.Agent.FullName : "Unassigned",
                    AgentEmail = c.Agent != null ? c.Agent.Email : null,
                    ActivePolicy = c.CompanyPolicies
                        .Where(cp => cp.Status == "Active")
                        .Select(cp => new
                        {
                            cp.Policy.Id,
                            cp.Policy.Name,
                            cp.Policy.PremiumPerEmployee,
                            cp.EmployeeCount,
                            cp.TotalPremium,
                            cp.StartDate,
                            cp.EndDate,
                            cp.Status
                        }).FirstOrDefault(),
                    HasActivePolicy = c.CompanyPolicies.Any(cp => cp.Status == "Active")
                })
                .ToListAsync();

            return Ok(companies);
        }

        // GET api/company/by-agent — Admin: which agent handles which customers
        [HttpGet("by-agent")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByAgent()
        {
            var agents = await _context.Users
                .Where(u => u.Role == Domain.Enums.UserRole.Agent)
                .Select(agent => new
                {
                    AgentId = agent.Id,
                    AgentName = agent.FullName,
                    AgentEmail = agent.Email,
                    TotalCustomers = _context.Companies
                        .Count(c => c.AgentId == agent.Id),
                    Customers = _context.Companies
                        .Include(c => c.Customer)
                        .Include(c => c.CompanyPolicies).ThenInclude(cp => cp.Policy)
                        .Where(c => c.AgentId == agent.Id)
                        .Select(c => new
                        {
                            c.Id,
                            c.CompanyName,
                            c.Size,
                            c.Domain,
                            CustomerName = c.Customer.FullName,
                            CustomerEmail = c.Customer.Email,
                            ActivePolicy = c.CompanyPolicies
                                .Where(cp => cp.Status == "Active")
                                .Select(cp => cp.Policy.Name)
                                .FirstOrDefault() ?? "No active policy",
                            c.CreatedAt
                        }).ToList()
                })
                .ToListAsync();

            return Ok(agents);
        }

        // GET api/company/policy-summary — Admin: policy purchase summary
        [HttpGet("policy-summary")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPolicySummary()
        {
            var summary = await _context.CompanyPolicies
                .Include(cp => cp.Company).ThenInclude(c => c.Customer)
                .Include(cp => cp.Company).ThenInclude(c => c.Agent)
                .Include(cp => cp.Policy)
                .OrderByDescending(cp => cp.CreatedAt)
                .Select(cp => new
                {
                    cp.Id,
                    CompanyName = cp.Company.CompanyName,
                    CustomerName = cp.Company.Customer.FullName,
                    CustomerEmail = cp.Company.Customer.Email,
                    AgentName = cp.Company.Agent != null ? cp.Company.Agent.FullName : "Unassigned",
                    PolicyName = cp.Policy.Name,
                    cp.EmployeeCount,
                    cp.TotalPremium,
                    cp.Status,
                    cp.StartDate,
                    cp.EndDate,
                    cp.CreatedAt
                })
                .ToListAsync();

            return Ok(summary);
        }

        private int GetUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}