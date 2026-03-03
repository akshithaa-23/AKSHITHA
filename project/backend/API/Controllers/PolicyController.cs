using Application.DTOs;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PolicyController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PolicyController(AppDbContext context) { _context = context; }

        // GET api/policy - Active policies
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var policies = await _context.Policies
                .Where(p => p.IsActive)
                .OrderBy(p => p.PremiumPerEmployee)
                .Select(p => new PolicyDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    HealthCoverage = p.HealthCoverage,
                    LifeCoverageMultiplier = p.LifeCoverageMultiplier,
                    MaxLifeCoverageLimit = p.MaxLifeCoverageLimit,
                    AccidentCoverage = p.AccidentCoverage,
                    PremiumPerEmployee = p.PremiumPerEmployee,
                    MinEmployees = p.MinEmployees,
                    DurationYears = p.DurationYears,
                    IsPopular = p.IsPopular,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                }).ToListAsync();

            return Ok(policies);
        }

        // GET api/policy/all - Admin only
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAdmin()
        {
            var policies = await _context.Policies
                .OrderBy(p => p.PremiumPerEmployee)
                .Select(p => new PolicyDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    HealthCoverage = p.HealthCoverage,
                    LifeCoverageMultiplier = p.LifeCoverageMultiplier,
                    MaxLifeCoverageLimit = p.MaxLifeCoverageLimit,
                    AccidentCoverage = p.AccidentCoverage,
                    PremiumPerEmployee = p.PremiumPerEmployee,
                    MinEmployees = p.MinEmployees,
                    DurationYears = p.DurationYears,
                    IsPopular = p.IsPopular,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                }).ToListAsync();

            return Ok(policies);
        }

        // GET api/policy/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _context.Policies.FindAsync(id);
            if (p == null) return NotFound(new { message = "Policy not found" });

            return Ok(new PolicyDto
            {
                Id = p.Id,
                Name = p.Name,
                HealthCoverage = p.HealthCoverage,
                LifeCoverageMultiplier = p.LifeCoverageMultiplier,
                MaxLifeCoverageLimit = p.MaxLifeCoverageLimit,
                AccidentCoverage = p.AccidentCoverage,
                PremiumPerEmployee = p.PremiumPerEmployee,
                MinEmployees = p.MinEmployees,
                DurationYears = p.DurationYears,
                IsPopular = p.IsPopular,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            });
        }

        // POST api/policy - Admin only
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreatePolicyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Policy name is required" });

            if (dto.HealthCoverage <= 0 || dto.PremiumPerEmployee <= 0)
                return BadRequest(new { message = "Health coverage and premium must be greater than zero" });

            // Life validation
            if (dto.LifeCoverageMultiplier == null && dto.MaxLifeCoverageLimit != null)
                return BadRequest(new { message = "Max life coverage cannot exist without life multiplier" });

            if (dto.LifeCoverageMultiplier != null && dto.MaxLifeCoverageLimit == null)
                return BadRequest(new { message = "Max life coverage is required when life multiplier is provided" });

            if (await _context.Policies.AnyAsync(p => p.Name == dto.Name))
                return BadRequest(new { message = "Policy with this name already exists" });

            var policy = new Policy
            {
                Name = dto.Name,
                HealthCoverage = dto.HealthCoverage,
                LifeCoverageMultiplier = dto.LifeCoverageMultiplier,
                MaxLifeCoverageLimit = dto.MaxLifeCoverageLimit,
                AccidentCoverage = dto.AccidentCoverage,
                PremiumPerEmployee = dto.PremiumPerEmployee,
                MinEmployees = dto.MinEmployees,
                DurationYears = dto.DurationYears,
                IsPopular = dto.IsPopular,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Policies.Add(policy);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Policy created successfully", policyId = policy.Id });
        }

        // PUT api/policy/{id} - Admin only
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePolicyDto dto)
        {
            var policy = await _context.Policies.FindAsync(id);
            if (policy == null) return NotFound(new { message = "Policy not found" });

            if (dto.HealthCoverage <= 0 || dto.PremiumPerEmployee <= 0)
                return BadRequest(new { message = "Health coverage and premium must be greater than zero" });

            // Life validation
            if (dto.LifeCoverageMultiplier == null && dto.MaxLifeCoverageLimit != null)
                return BadRequest(new { message = "Max life coverage cannot exist without life multiplier" });

            if (dto.LifeCoverageMultiplier != null && dto.MaxLifeCoverageLimit == null)
                return BadRequest(new { message = "Max life coverage is required when life multiplier is provided" });

            policy.Name = dto.Name;
            policy.HealthCoverage = dto.HealthCoverage;
            policy.LifeCoverageMultiplier = dto.LifeCoverageMultiplier;
            policy.MaxLifeCoverageLimit = dto.MaxLifeCoverageLimit;
            policy.AccidentCoverage = dto.AccidentCoverage;
            policy.PremiumPerEmployee = dto.PremiumPerEmployee;
            policy.MinEmployees = dto.MinEmployees;
            policy.DurationYears = dto.DurationYears;
            policy.IsPopular = dto.IsPopular;
            policy.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Policy updated successfully" });
        }

        // DELETE api/policy/{id} - Admin soft delete
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var policy = await _context.Policies.FindAsync(id);
            if (policy == null) return NotFound(new { message = "Policy not found" });

            policy.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Policy deactivated successfully" });
        }
    }
}