using Application.DTOs;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IAppDbContext _context;

        public CompanyService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetMyCompanyAsync(int customerId)
        {
            var company = await _context.Companies
                .Include(c => c.Agent)
                .Include(c => c.CompanyPolicies).ThenInclude(cp => cp.Policy)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (company == null) return null;

            var activePolicy = company.CompanyPolicies
                .FirstOrDefault(cp => cp.Status == "Active");

            return new
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
                    activePolicy.Status,
                    activePolicy.Policy.LifeCoverageMultiplier,
                    activePolicy.Policy.MaxLifeCoverageLimit
                }
            };
        }

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            return await _context.Companies
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
        }

        public async Task<IEnumerable<object>> GetByAgentAsync()
        {
            return await _context.Users
                .Where(u => u.Role == Domain.Enums.UserRole.Agent)
                .Select(agent => new
                {
                    AgentId = agent.Id,
                    AgentName = agent.FullName,
                    AgentEmail = agent.Email,
                    TotalCustomers = _context.Companies
                        .Count(c => c.AgentId == agent.Id),
                    Customers = _context.Companies
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
        }

        public async Task<IEnumerable<object>> GetPolicySummaryAsync()
        {
            return await _context.CompanyPolicies
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
        }
    }
}
