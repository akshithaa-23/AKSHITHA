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
    public class PolicyService : IPolicyService
    {
        private readonly IAppDbContext _context;

        public PolicyService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PolicyDto>> GetAllActiveAsync()
        {
            return await _context.Policies
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
        }

        public async Task<IEnumerable<PolicyDto>> GetAllAdminAsync()
        {
            return await _context.Policies
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
        }

        public async Task<PolicyDto> GetByIdAsync(int id)
        {
            var p = await _context.Policies.FindAsync(id);
            if (p == null) return null;

            return new PolicyDto
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
            };
        }

        public async Task<int> CreateAsync(CreatePolicyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Policy name is required");

            if (dto.HealthCoverage <= 0 || dto.PremiumPerEmployee <= 0)
                throw new ArgumentException("Health coverage and premium must be greater than zero");

            if (dto.LifeCoverageMultiplier == null && dto.MaxLifeCoverageLimit != null)
                throw new ArgumentException("Max life coverage cannot exist without life multiplier");

            if (dto.LifeCoverageMultiplier != null && dto.MaxLifeCoverageLimit == null)
                throw new ArgumentException("Max life coverage is required when life multiplier is provided");

            if (await _context.Policies.AnyAsync(p => p.Name == dto.Name))
                throw new InvalidOperationException("Policy with this name already exists");

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

            return policy.Id;
        }

        public async Task UpdateAsync(int id, UpdatePolicyDto dto)
        {
            var policy = await _context.Policies.FindAsync(id);
            if (policy == null) throw new KeyNotFoundException("Policy not found");

            if (dto.HealthCoverage <= 0 || dto.PremiumPerEmployee <= 0)
                throw new ArgumentException("Health coverage and premium must be greater than zero");

            if (dto.LifeCoverageMultiplier == null && dto.MaxLifeCoverageLimit != null)
                throw new ArgumentException("Max life coverage cannot exist without life multiplier");

            if (dto.LifeCoverageMultiplier != null && dto.MaxLifeCoverageLimit == null)
                throw new ArgumentException("Max life coverage is required when life multiplier is provided");

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
        }

        public async Task DeleteAsync(int id)
        {
            var policy = await _context.Policies.FindAsync(id);
            if (policy == null) throw new KeyNotFoundException("Policy not found");

            policy.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}
