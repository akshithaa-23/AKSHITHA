using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ClaimService : IClaimService
    {
        private readonly IAppDbContext _context;

        public ClaimService(IAppDbContext context)
        {
            _context = context;
        }

        private async Task<int> GetOrAssignClaimsManagerAsync(int companyId)
        {
            var company = await _context.Companies.FindAsync(companyId);
            if (company!.ClaimsManagerId.HasValue)
                return company.ClaimsManagerId.Value;

            var manager = await _context.Users
                .Where(u => u.Role == UserRole.ClaimsManager && u.IsActive)
                .OrderBy(u => _context.Companies.Count(c => c.ClaimsManagerId == u.Id))
                .ThenBy(u => u.CreatedAt)
                .FirstOrDefaultAsync();

            if (manager == null)
                throw new InvalidOperationException("No claims manager available");

            company.ClaimsManagerId = manager.Id;
            await _context.SaveChangesAsync();

            return manager.Id;
        }

        private static ClaimResponseDto MapClaim(Claim c) => new()
        {
            Id = c.Id,
            ClaimType = c.ClaimType,
            ClaimAmount = c.ClaimAmount,
            AccidentType = c.AccidentType,
            AccidentPercentage = c.AccidentPercentage,
            Status = c.Status,
            ClaimsManagerNote = c.ClaimsManagerNote,
            EmployeeId = c.EmployeeId,
            EmployeeName = c.Employee.FullName,
            EmployeeCode = c.Employee.EmployeeCode,
            EmployeeSalary = c.Employee.Salary,
            HealthCoverageRemaining = c.Employee.HealthCoverageRemaining,
            DocumentUrl = c.DocumentUrl,
            PolicyName = c.CompanyPolicy.Policy.Name,
            CompanyName = c.CompanyPolicy.Company.CompanyName,
            CustomerName = c.Customer.FullName,
            ClaimsManagerName = c.ClaimsManager.FullName,
            CreatedAt = c.CreatedAt,
            ProcessedAt = c.ProcessedAt
        };

        public async Task<object> GetAllowedClaimTypesAsync(int customerId)
        {
            var companyPolicy = await _context.CompanyPolicies
                .Include(cp => cp.Policy)
                .Include(cp => cp.Company)
                .Where(cp => cp.Company.CustomerId == customerId && cp.Status == "Active")
                .FirstOrDefaultAsync();

            if (companyPolicy == null)
                throw new InvalidOperationException("No active policy found. You must have an active policy to raise claims.");

            var policy = companyPolicy.Policy;
            var allowed = new List<string> { "Health" }; 

            if (policy.LifeCoverageMultiplier.HasValue && policy.LifeCoverageMultiplier > 0)
                allowed.Add("TermLife");

            if (policy.AccidentCoverage.HasValue && policy.AccidentCoverage > 0)
                allowed.Add("Accident");

            return new
            {
                policyName = policy.Name,
                allowedClaimTypes = allowed,
                healthCoverage = policy.HealthCoverage,
                lifeCoverageMultiplier = policy.LifeCoverageMultiplier,
                maxLifeCoverageLimit = policy.MaxLifeCoverageLimit,
                accidentCoverage = policy.AccidentCoverage
            };
        }

        public async Task<object> RaiseHealthClaimAsync(int customerId, RaiseHealthClaimDto dto)
        {
            var companyPolicy = await _context.CompanyPolicies
                .Include(cp => cp.Policy)
                .Include(cp => cp.Company)
                .Where(cp => cp.Company.CustomerId == customerId && cp.Status == "Active")
                .FirstOrDefaultAsync();

            if (companyPolicy == null)
                throw new InvalidOperationException("No active policy. Purchase a policy before raising claims.");

            var policy = companyPolicy.Policy;

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId && e.CompanyId == companyPolicy.CompanyId);

            if (employee == null)
                throw new KeyNotFoundException("Employee not found in your company.");

            if (!employee.IsActive)
                throw new InvalidOperationException("Cannot raise a claim for an inactive employee.");

            if (employee.HealthCoverageRemaining == null)
                employee.HealthCoverageRemaining = policy.HealthCoverage;

            if (dto.RequestedAmount > employee.HealthCoverageRemaining)
                return new
                {
                    message = $"Claim amount ₹{dto.RequestedAmount:N0} exceeds remaining health coverage ₹{employee.HealthCoverageRemaining:N0} for this employee. Claim auto-rejected.",
                    remainingCoverage = employee.HealthCoverageRemaining,
                    requestedAmount = dto.RequestedAmount,
                    autoRejected = true
                };

            int managerId = await GetOrAssignClaimsManagerAsync(companyPolicy.CompanyId);

            var claim = new Claim
            {
                CompanyPolicyId = companyPolicy.Id,
                EmployeeId = dto.EmployeeId,
                CustomerId = customerId,
                ClaimsManagerId = managerId,
                ClaimType = "Health",
                ClaimAmount = dto.RequestedAmount,
                DocumentUrl = dto.DocumentUrl,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            return new
            {
                message = "Health claim submitted successfully. Awaiting claims manager review.",
                claimId = claim.Id,
                claimAmount = claim.ClaimAmount,
                remainingCoverageBeforeApproval = employee.HealthCoverageRemaining
            };
        }

        public async Task<object> RaiseTermLifeClaimAsync(int customerId, RaiseTermLifeClaimDto dto)
        {
            var companyPolicy = await _context.CompanyPolicies
                .Include(cp => cp.Policy)
                .Include(cp => cp.Company)
                .Where(cp => cp.Company.CustomerId == customerId && cp.Status == "Active")
                .FirstOrDefaultAsync();

            if (companyPolicy == null)
                throw new InvalidOperationException("No active policy found.");

            var policy = companyPolicy.Policy;

            if (!policy.LifeCoverageMultiplier.HasValue || policy.LifeCoverageMultiplier == 0)
                throw new InvalidOperationException("Your current policy does not include Term Life coverage.");

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId && e.CompanyId == companyPolicy.CompanyId);

            if (employee == null)
                throw new KeyNotFoundException("Employee not found in your company.");

            if (!employee.IsActive)
                throw new InvalidOperationException("A Term Life claim already exists for this employee (employee is inactive).");

            var existingLifeClaim = await _context.Claims
                .AnyAsync(c => c.EmployeeId == dto.EmployeeId && c.ClaimType == "TermLife"
                               && (c.Status == "Pending" || c.Status == "Approved"));

            if (existingLifeClaim)
                throw new InvalidOperationException("A Term Life claim has already been raised for this employee.");

            decimal rawPayout = employee.Salary * policy.LifeCoverageMultiplier.Value;
            decimal claimAmount = policy.MaxLifeCoverageLimit.HasValue
                ? Math.Min(rawPayout, policy.MaxLifeCoverageLimit.Value)
                : rawPayout;

            int managerId = await GetOrAssignClaimsManagerAsync(companyPolicy.CompanyId);

            var claim = new Claim
            {
                CompanyPolicyId = companyPolicy.Id,
                EmployeeId = dto.EmployeeId,
                CustomerId = customerId,
                ClaimsManagerId = managerId,
                ClaimType = "TermLife",
                ClaimAmount = claimAmount,
                DocumentUrl = dto.DocumentUrl,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            return new
            {
                message = "Term Life claim submitted. If approved, the employee will be marked inactive.",
                claimId = claim.Id,
                claimAmount = claimAmount,
                breakdown = new
                {
                    salary = employee.Salary,
                    multiplier = policy.LifeCoverageMultiplier,
                    rawPayout,
                    cappedAt = policy.MaxLifeCoverageLimit,
                    finalPayout = claimAmount
                }
            };
        }

        public async Task<object> RaiseAccidentClaimAsync(int customerId, RaiseAccidentClaimDto dto)
        {
            var companyPolicy = await _context.CompanyPolicies
                .Include(cp => cp.Policy)
                .Include(cp => cp.Company)
                .Where(cp => cp.Company.CustomerId == customerId && cp.Status == "Active")
                .FirstOrDefaultAsync();

            if (companyPolicy == null)
                throw new InvalidOperationException("No active policy found.");

            var policy = companyPolicy.Policy;

            if (!policy.AccidentCoverage.HasValue || policy.AccidentCoverage == 0)
                throw new InvalidOperationException("Your current policy does not include Accident coverage.");

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId && e.CompanyId == companyPolicy.CompanyId);

            if (employee == null)
                throw new KeyNotFoundException("Employee not found in your company.");

            if (!employee.IsActive)
                throw new InvalidOperationException("Cannot raise a claim for an inactive employee.");

            if (employee.AccidentClaimRaised)
                throw new InvalidOperationException("An accident claim has already been raised for this employee. Only one accident claim is allowed per employee.");

            if (dto.AccidentType != "Complete" && dto.AccidentType != "Partial")
                throw new InvalidOperationException("AccidentType must be 'Complete' or 'Partial'.");

            if (dto.AccidentType == "Partial")
            {
                if (!dto.AccidentPercentage.HasValue || dto.AccidentPercentage <= 0 || dto.AccidentPercentage > 100)
                    throw new InvalidOperationException("For Partial accident, AccidentPercentage must be between 1 and 100.");
            }

            decimal claimAmount = dto.AccidentType == "Complete"
                ? policy.AccidentCoverage.Value
                : policy.AccidentCoverage.Value * (dto.AccidentPercentage!.Value / 100m);

            int managerId = await GetOrAssignClaimsManagerAsync(companyPolicy.CompanyId);

            var claim = new Claim
            {
                CompanyPolicyId = companyPolicy.Id,
                EmployeeId = dto.EmployeeId,
                CustomerId = customerId,
                ClaimsManagerId = managerId,
                ClaimType = "Accident",
                ClaimAmount = claimAmount,
                AccidentType = dto.AccidentType,
                AccidentPercentage = dto.AccidentType == "Partial" ? dto.AccidentPercentage : null,
                DocumentUrl = dto.DocumentUrl,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            employee.AccidentClaimRaised = true;

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            return new
            {
                message = "Accident claim submitted successfully.",
                claimId = claim.Id,
                accidentType = dto.AccidentType,
                claimAmount,
                breakdown = dto.AccidentType == "Partial"
                    ? new { accidentCoverage = policy.AccidentCoverage, percentage = dto.AccidentPercentage, payout = claimAmount }
                    : new { accidentCoverage = policy.AccidentCoverage, percentage = (decimal?)100, payout = claimAmount }
            };
        }

        public async Task<IEnumerable<ClaimResponseDto>> GetMyClaimsAsync(int customerId)
        {
            var claims = await _context.Claims
                .Include(c => c.Employee)
                .Include(c => c.CompanyPolicy).ThenInclude(cp => cp.Policy)
                .Include(c => c.CompanyPolicy).ThenInclude(cp => cp.Company)
                .Include(c => c.Customer)
                .Include(c => c.ClaimsManager)
                .Where(c => c.CustomerId == customerId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return claims.Select(MapClaim);
        }

        public async Task<IEnumerable<ClaimResponseDto>> GetManagerClaimsAsync(int managerId)
        {
            var claims = await _context.Claims
                .Include(c => c.Employee)
                .Include(c => c.CompanyPolicy).ThenInclude(cp => cp.Policy)
                .Include(c => c.CompanyPolicy).ThenInclude(cp => cp.Company)
                .Include(c => c.Customer)
                .Include(c => c.ClaimsManager)
                .Where(c => c.ClaimsManagerId == managerId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return claims.Select(MapClaim);
        }

        public async Task<object> ProcessClaimAsync(int managerId, int id, ProcessClaimDto dto)
        {
            if (dto.Decision != "Approved" && dto.Decision != "Rejected")
                throw new InvalidOperationException("Decision must be 'Approved' or 'Rejected'.");

            var claim = await _context.Claims
                .Include(c => c.Employee)
                .Include(c => c.CompanyPolicy).ThenInclude(cp => cp.Policy)
                .FirstOrDefaultAsync(c => c.Id == id && c.ClaimsManagerId == managerId);

            if (claim == null)
                throw new KeyNotFoundException("Claim not found or not assigned to you.");

            if (claim.Status != "Pending")
                throw new InvalidOperationException("This claim has already been processed.");

            claim.Status = dto.Decision;
            claim.ClaimsManagerNote = dto.Note;
            claim.ProcessedAt = DateTime.UtcNow;

            if (dto.Decision == "Approved")
            {
                var employee = claim.Employee;
                var policy = claim.CompanyPolicy.Policy;

                switch (claim.ClaimType)
                {
                    case "Health":
                        if (employee.HealthCoverageRemaining == null)
                            employee.HealthCoverageRemaining = policy.HealthCoverage;

                        employee.HealthCoverageRemaining -= claim.ClaimAmount;
                        break;

                    case "TermLife":
                        employee.IsActive = false;
                        break;

                    case "Accident":
                        break;
                }
            }

            await _context.SaveChangesAsync();

            string resultMsg = dto.Decision == "Approved"
                ? claim.ClaimType switch
                {
                    "Health" => $"Claim approved. ₹{claim.ClaimAmount:N0} payout processed. Employee health coverage updated.",
                    "TermLife" => $"Claim approved. ₹{claim.ClaimAmount:N0} payout processed. Employee marked as inactive.",
                    "Accident" => $"Claim approved. ₹{claim.ClaimAmount:N0} payout processed.",
                    _ => "Claim approved."
                }
                : "Claim rejected.";

            return new { message = resultMsg, claimId = id, decision = dto.Decision };
        }

        public async Task<IEnumerable<ClaimResponseDto>> GetAllClaimsAsync()
        {
            var claims = await _context.Claims
                .Include(c => c.Employee)
                .Include(c => c.CompanyPolicy).ThenInclude(cp => cp.Policy)
                .Include(c => c.CompanyPolicy).ThenInclude(cp => cp.Company)
                .Include(c => c.Customer)
                .Include(c => c.ClaimsManager)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return claims.Select(MapClaim);
        }

        public async Task<string> UploadClaimDocumentAsync(IFormFile file, string scheme, string host)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("No file provided.");

            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(ext))
                throw new InvalidOperationException("Only PDF, JPG, PNG files allowed.");

            if (file.Length > 5 * 1024 * 1024)
                throw new InvalidOperationException("File size cannot exceed 5MB.");

            var fileName = $"claim_{Guid.NewGuid()}{ext}";
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "claims");
            Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"{scheme}://{host}/uploads/claims/{fileName}";
        }
    }
}
