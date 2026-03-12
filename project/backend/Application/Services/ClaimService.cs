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

        private async Task<object?> CheckPendingClaimsAsync(int employeeId)
        {
            var pendingClaim = await _context.Claims
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(c => c.EmployeeId == employeeId && c.Status == "Pending");

            if (pendingClaim != null)
            {
                var employeeName = pendingClaim.Employee?.FullName ?? "Unknown";
                return new
                {
                    message = $"Employee {employeeName} already has a claim pending review. Please wait for the current claim to be approved or rejected before raising a new claim.",
                    reason = "PendingClaimExists",
                    pendingClaimId = pendingClaim.Id,
                    pendingClaimType = pendingClaim.ClaimType,
                    employeeId = employeeId,
                    autoRejected = true
                };
            }

            return null;
        }

        private static ClaimResponseDto MapClaim(Claim c)
        {
            int? daysSince = c.ClaimType == "Accident" && c.AccidentDate.HasValue
                ? (int)(DateTime.UtcNow - c.AccidentDate.Value).TotalDays
                : null;

            return new()
            {
                Id = c.Id,
                ClaimType = c.ClaimType,
                ClaimAmount = c.ClaimAmount,
                AccidentType = c.AccidentType,
                AccidentPercentage = c.AccidentPercentage,
                AgeFactor = c.AgeFactor,
                FrequencyFactor = c.FrequencyFactor,
                FinalApprovedAmount = c.FinalApprovedAmount,
                CauseOfDeath = c.CauseOfDeath,
                CauseOfDeathDescription = c.CauseOfDeathDescription,
                DateOfDeath = c.DateOfDeath,
                NormalPayout = c.NormalPayout,
                AdjustedPayout = c.AdjustedPayout,
                SuicideExclusionFlag = c.SuicideExclusionFlag,
                DaysInCompany = c.DaysInCompany,
                EmployeeJoinDate = c.Employee?.EmployeeJoinDate,
                AccidentDate = c.AccidentDate,
                DaysSinceAccident = daysSince,
                ClaimDeadline = c.AccidentDate.HasValue ? c.AccidentDate.Value.AddDays(90) : null,
                FirDocumentUrl = c.FirDocumentPath,
                HospitalReportUrl = c.HospitalReportPath,
                RequestedAmount = c.ClaimAmount,
                EmployeeAge = c.ClaimType == "Health" && c.Employee != null && c.Employee.DateOfBirth != default
                    ? DateTime.UtcNow.Year - c.Employee.DateOfBirth.Year - (DateTime.UtcNow.DayOfYear < c.Employee.DateOfBirth.DayOfYear ? 1 : 0)
                    : null,
                ClaimNumberInYear = c.ClaimType == "Health" && c.FrequencyFactor.HasValue
                    ? (c.FrequencyFactor.Value == 1.0m ? 1 : (c.FrequencyFactor.Value == 0.90m ? 2 : 3))
                    : null,
                Status = c.Status,
                ClaimsManagerNote = c.ClaimsManagerNote,
                EmployeeId = c.EmployeeId,
                EmployeeName = c.Employee?.FullName ?? "Unknown",
                EmployeeCode = c.Employee?.EmployeeCode ?? "Unknown",
                EmployeeSalary = c.Employee?.Salary ?? 0,
                HealthCoverageRemaining = c.Employee?.HealthCoverageRemaining,
                DocumentUrl = c.DocumentUrl,
                PolicyName = c.CompanyPolicy?.Policy?.Name ?? "Unknown Policy",
                CompanyName = c.CompanyPolicy?.Company?.CompanyName ?? "Unknown Company",
                CustomerName = c.Customer?.FullName ?? string.Empty,
                ClaimsManagerName = c.ClaimsManager?.FullName ?? string.Empty,
                CreatedAt = c.CreatedAt,
                ProcessedAt = c.ProcessedAt
            };
        }

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

            var pendingCheck = await CheckPendingClaimsAsync(dto.EmployeeId);
            if (pendingCheck != null) return pendingCheck;

            if (employee.HealthCoverageRemaining == null)
                employee.HealthCoverageRemaining = policy.HealthCoverage;

            // Calculate Age Factor
            int age = DateTime.UtcNow.Year - employee.DateOfBirth.Year - (DateTime.UtcNow.DayOfYear < employee.DateOfBirth.DayOfYear ? 1 : 0);
            decimal ageFactor = 1.0m;
            if (age >= 36 && age <= 45) ageFactor = 0.95m; // 5% reduction
            else if (age >= 46 && age <= 55) ageFactor = 0.90m; // 10% reduction
            else if (age > 55) ageFactor = 0.85m; // 15% reduction

            decimal ageAdjustedAmount = Math.Round(dto.RequestedAmount * ageFactor, 0);

            // Calculate Frequency Factor
            var policyStart = companyPolicy.StartDate;
            var policyEnd = policyStart.AddDays(365);
            var currentYearClaimsCount = await _context.Claims
                .CountAsync(c => c.EmployeeId == employee.Id && c.ClaimType == "Health" && c.Status == "Approved" && c.CreatedAt >= policyStart && c.CreatedAt <= policyEnd);
            
            decimal frequencyFactor = 1.0m;
            if (currentYearClaimsCount == 1) frequencyFactor = 0.90m; // 10% reduction for 2nd claim
            else if (currentYearClaimsCount >= 2) frequencyFactor = 0.80m; // 20% reduction for 3rd+ claim

            decimal finalApprovedAmount = Math.Round(ageAdjustedAmount * frequencyFactor, 0);

            if (finalApprovedAmount > employee.HealthCoverageRemaining)
                return new
                {
                    message = $"Approved amount ₹{finalApprovedAmount:N0} exceeds remaining health coverage ₹{employee.HealthCoverageRemaining:N0}",
                    remainingCoverage = employee.HealthCoverageRemaining,
                    approvedAmount = finalApprovedAmount,
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
                AgeFactor = ageFactor,
                FrequencyFactor = frequencyFactor,
                FinalApprovedAmount = finalApprovedAmount,
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
                requestedAmount = claim.ClaimAmount,
                ageFactor = claim.AgeFactor,
                frequencyFactor = claim.FrequencyFactor,
                finalApprovedAmount = claim.FinalApprovedAmount,
                remainingCoverage = employee.HealthCoverageRemaining
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
                throw new InvalidOperationException("Cannot raise a claim for an inactive employee.");
            
            var pendingCheck = await CheckPendingClaimsAsync(dto.EmployeeId);
            if (pendingCheck != null) return pendingCheck;

            // Age Eligibility Cap (Max 70 years) - Round DOWN calculating total days
            int age = (int)Math.Floor((DateTime.UtcNow - employee.DateOfBirth).TotalDays / 365.25);
            if (age > 70)
                return new
                {
                    message = $"Term life coverage is only valid for employees up to age 70. Employee age: {age}",
                    reason = "AgeEligibilityExceeded",
                    autoRejected = true
                };

            var existingLifeClaim = await _context.Claims
                .AnyAsync(c => c.EmployeeId == dto.EmployeeId && c.ClaimType == "TermLife"
                               && (c.Status == "Pending" || c.Status == "Approved"));

            if (existingLifeClaim)
                throw new InvalidOperationException("A Term Life claim has already been raised for this employee.");

            if (existingLifeClaim)
                throw new InvalidOperationException("A Term Life claim has already been raised for this employee.");

            decimal rawPayout = employee.Salary * policy.LifeCoverageMultiplier.Value;
            decimal normalPayout = policy.MaxLifeCoverageLimit.HasValue
                ? Math.Min(rawPayout, policy.MaxLifeCoverageLimit.Value)
                : rawPayout;

            // Suicide Exclusion (Within 1 year of policy/employee join date)
            int daysInCompany = (DateTime.UtcNow - employee.EmployeeJoinDate).Days;
            bool suicideExclusionFlag = false;
            decimal adjustedPayout = normalPayout;

            if (dto.CauseOfDeath.Equals("Suicide", StringComparison.OrdinalIgnoreCase) && daysInCompany < 365)
            {
                suicideExclusionFlag = true;
                adjustedPayout = Math.Round(normalPayout * 0.80m, 0);
            }

            int managerId = await GetOrAssignClaimsManagerAsync(companyPolicy.CompanyId);

            var claim = new Claim
            {
                CompanyPolicyId = companyPolicy.Id,
                EmployeeId = dto.EmployeeId,
                CustomerId = customerId,
                ClaimsManagerId = managerId,
                ClaimType = "TermLife",
                ClaimAmount = adjustedPayout,
                CauseOfDeath = dto.CauseOfDeath,
                CauseOfDeathDescription = dto.CauseOfDeath.Equals("Other", StringComparison.OrdinalIgnoreCase) ? dto.CauseOfDeathDescription : null,
                DateOfDeath = dto.DateOfDeath,
                NormalPayout = normalPayout,
                AdjustedPayout = adjustedPayout,
                SuicideExclusionFlag = suicideExclusionFlag,
                DaysInCompany = daysInCompany,
                DocumentUrl = dto.DocumentUrl,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Claims.Add(claim);

            // Inactivate employee IMMEDIATELY
            employee.IsActive = false;

            await _context.SaveChangesAsync();

            return new
            {
                claimId = claim.Id,
                employeeId = employee.Id,
                causeOfDeath = claim.CauseOfDeath,
                dateOfDeath = claim.DateOfDeath,
                daysInCompany = claim.DaysInCompany,
                suicideExclusionFlag = claim.SuicideExclusionFlag,
                status = claim.Status,
                breakdown = new
                {
                    salary = employee.Salary,
                    multiplier = policy.LifeCoverageMultiplier,
                    rawPayout = rawPayout,
                    cappedAt = policy.MaxLifeCoverageLimit,
                    normalPayout = normalPayout,
                    adjustedPayout = adjustedPayout
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

            var pendingCheck = await CheckPendingClaimsAsync(dto.EmployeeId);
            if (pendingCheck != null) return pendingCheck;

            if (employee.AccidentClaimRaised)
                throw new InvalidOperationException("An accident claim has already been raised for this employee. Only one accident claim is allowed per employee.");

            if (employee.AccidentClaimRaised)
                throw new InvalidOperationException("An accident claim has already been raised for this employee. Only one accident claim is allowed per employee.");

            // ── 90-Day Claim Window Validation ────────────────────────────────────
            var today = DateTime.UtcNow.Date;
            var accidentDate = dto.AccidentDate.Date;
            var daysSinceAccident = (today - accidentDate).Days;
            var claimDeadline = accidentDate.AddDays(90);

            if (accidentDate > today)
                return new
                {
                    message = "Accident date cannot be a future date",
                    reason = "InvalidAccidentDate",
                    autoRejected = true
                };

            if (daysSinceAccident > 90)
                return new
                {
                    message = $"Accident claim must be raised within 90 days of the accident date.\n" +
                              $"Accident date    : {accidentDate:yyyy-MM-dd}\n" +
                              $"Deadline was     : {claimDeadline:yyyy-MM-dd}\n" +
                              $"Days since accident: {daysSinceAccident} days",
                    reason = "ClaimWindowExpired",
                    deadlineDate = claimDeadline,
                    autoRejected = true
                };
            // ─────────────────────────────────────────────────────────────────────

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
                AccidentDate = dto.AccidentDate,
                FirDocumentPath = dto.FirDocumentUrl,
                HospitalReportPath = dto.HospitalReportUrl,
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
                accidentDate = claim.AccidentDate,
                daysSinceAccident,
                claimDeadline,
                firDocumentId = dto.FirDocumentUrl,
                hospitalReportId = dto.HospitalReportUrl,
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

                        employee.HealthCoverageRemaining -= claim.FinalApprovedAmount ?? claim.ClaimAmount;
                        break;

                    case "TermLife":
                        // Employee is already inactivated at submission logic. No action needed here.
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
