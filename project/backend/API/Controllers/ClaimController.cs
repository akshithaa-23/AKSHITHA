using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DomainClaim = Domain.Entities.Claim;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClaimController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ClaimController(AppDbContext context) { _context = context; }

        // ─── SHARED: auto-assign claims manager ──────────────────────────────
        // Rule: least number of active company assignments; tiebreak by earliest created
        private async Task<int> GetOrAssignClaimsManagerAsync(int companyId)
        {
            // If company already has a claims manager, reuse
            var company = await _context.Companies.FindAsync(companyId);
            if (company!.ClaimsManagerId.HasValue)
                return company.ClaimsManagerId.Value;

            // Assign: claims manager with fewest assigned companies
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

        // ─── SHARED: build ClaimResponseDto ──────────────────────────────────
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
            PolicyName = c.CompanyPolicy.Policy.Name,
            CompanyName = c.CompanyPolicy.Company.CompanyName,
            CustomerName = c.Customer.FullName,
            ClaimsManagerName = c.ClaimsManager.FullName,
            CreatedAt = c.CreatedAt,
            ProcessedAt = c.ProcessedAt
        };

        // ─── CUSTOMER: check what claim types are allowed ─────────────────────
        // GET api/claim/allowed-types
        // Returns which claims are available for this company's policy
        [HttpGet("allowed-types")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetAllowedClaimTypes()
        {
            int customerId = GetUserId();

            var companyPolicy = await _context.CompanyPolicies
                .Include(cp => cp.Policy)
                .Include(cp => cp.Company)
                .Where(cp => cp.Company.CustomerId == customerId && cp.Status == "Active")
                .FirstOrDefaultAsync();

            if (companyPolicy == null)
                return BadRequest(new { message = "No active policy found. You must have an active policy to raise claims." });

            var policy = companyPolicy.Policy;
            var allowed = new List<string> { "Health" }; // Health always included

            if (policy.LifeCoverageMultiplier.HasValue && policy.LifeCoverageMultiplier > 0)
                allowed.Add("TermLife");

            if (policy.AccidentCoverage.HasValue && policy.AccidentCoverage > 0)
                allowed.Add("Accident");

            return Ok(new
            {
                policyName = policy.Name,
                allowedClaimTypes = allowed,
                healthCoverage = policy.HealthCoverage,
                lifeCoverageMultiplier = policy.LifeCoverageMultiplier,
                maxLifeCoverageLimit = policy.MaxLifeCoverageLimit,
                accidentCoverage = policy.AccidentCoverage
            });
        }

        // ─── CUSTOMER: raise HEALTH claim ────────────────────────────────────
        // POST api/claim/health
        [HttpPost("health")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RaiseHealthClaim([FromBody] RaiseHealthClaimDto dto)
        {
            int customerId = GetUserId();

            // Get active company policy
            var companyPolicy = await _context.CompanyPolicies
                .Include(cp => cp.Policy)
                .Include(cp => cp.Company)
                .Where(cp => cp.Company.CustomerId == customerId && cp.Status == "Active")
                .FirstOrDefaultAsync();

            if (companyPolicy == null)
                return BadRequest(new { message = "No active policy. Purchase a policy before raising claims." });

            // Validate policy covers Health (always does, but double-check)
            var policy = companyPolicy.Policy;

            // Get employee — must belong to this company
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId && e.CompanyId == companyPolicy.CompanyId);

            if (employee == null)
                return NotFound(new { message = "Employee not found in your company." });

            if (!employee.IsActive)
                return BadRequest(new { message = "Cannot raise a claim for an inactive employee." });

            // Initialize health coverage remaining on first claim for this employee
            if (employee.HealthCoverageRemaining == null)
                employee.HealthCoverageRemaining = policy.HealthCoverage;

            // Check: requested amount vs remaining coverage
            if (dto.RequestedAmount > employee.HealthCoverageRemaining)
                return BadRequest(new
                {
                    message = $"Claim amount ₹{dto.RequestedAmount:N0} exceeds remaining health coverage ₹{employee.HealthCoverageRemaining:N0} for this employee. Claim auto-rejected.",
                    remainingCoverage = employee.HealthCoverageRemaining,
                    requestedAmount = dto.RequestedAmount,
                    autoRejected = true
                });

            int managerId = await GetOrAssignClaimsManagerAsync(companyPolicy.CompanyId);

            var claim = new Claim
            {
                CompanyPolicyId = companyPolicy.Id,
                EmployeeId = dto.EmployeeId,
                CustomerId = customerId,
                ClaimsManagerId = managerId,
                ClaimType = "Health",
                ClaimAmount = dto.RequestedAmount,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Health claim submitted successfully. Awaiting claims manager review.",
                claimId = claim.Id,
                claimAmount = claim.ClaimAmount,
                remainingCoverageBeforeApproval = employee.HealthCoverageRemaining
            });
        }

        // ─── CUSTOMER: raise TERM LIFE claim ─────────────────────────────────
        // POST api/claim/term-life
        [HttpPost("term-life")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RaiseTermLifeClaim([FromBody] RaiseTermLifeClaimDto dto)
        {
            int customerId = GetUserId();

            var companyPolicy = await _context.CompanyPolicies
                .Include(cp => cp.Policy)
                .Include(cp => cp.Company)
                .Where(cp => cp.Company.CustomerId == customerId && cp.Status == "Active")
                .FirstOrDefaultAsync();

            if (companyPolicy == null)
                return BadRequest(new { message = "No active policy found." });

            var policy = companyPolicy.Policy;

            // Check policy includes life coverage
            if (!policy.LifeCoverageMultiplier.HasValue || policy.LifeCoverageMultiplier == 0)
                return BadRequest(new { message = "Your current policy does not include Term Life coverage." });

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId && e.CompanyId == companyPolicy.CompanyId);

            if (employee == null)
                return NotFound(new { message = "Employee not found in your company." });

            if (!employee.IsActive)
                return BadRequest(new { message = "A Term Life claim already exists for this employee (employee is inactive)." });

            // Check: no previous term life claim pending/approved for same employee
            var existingLifeClaim = await _context.Claims
                .AnyAsync(c => c.EmployeeId == dto.EmployeeId && c.ClaimType == "TermLife"
                               && (c.Status == "Pending" || c.Status == "Approved"));

            if (existingLifeClaim)
                return BadRequest(new { message = "A Term Life claim has already been raised for this employee." });

            // Calculate payout: salary × multiplier, capped at max
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
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            return Ok(new
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
            });
        }

        // ─── CUSTOMER: raise ACCIDENT claim ──────────────────────────────────
        // POST api/claim/accident
        [HttpPost("accident")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RaiseAccidentClaim([FromBody] RaiseAccidentClaimDto dto)
        {
            int customerId = GetUserId();

            var companyPolicy = await _context.CompanyPolicies
                .Include(cp => cp.Policy)
                .Include(cp => cp.Company)
                .Where(cp => cp.Company.CustomerId == customerId && cp.Status == "Active")
                .FirstOrDefaultAsync();

            if (companyPolicy == null)
                return BadRequest(new { message = "No active policy found." });

            var policy = companyPolicy.Policy;

            // Check policy includes accident coverage
            if (!policy.AccidentCoverage.HasValue || policy.AccidentCoverage == 0)
                return BadRequest(new { message = "Your current policy does not include Accident coverage." });

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId && e.CompanyId == companyPolicy.CompanyId);

            if (employee == null)
                return NotFound(new { message = "Employee not found in your company." });

            if (!employee.IsActive)
                return BadRequest(new { message = "Cannot raise a claim for an inactive employee." });

            // Check: accident claim already raised for this employee (one per employee per policy)
            if (employee.AccidentClaimRaised)
                return BadRequest(new { message = "An accident claim has already been raised for this employee. Only one accident claim is allowed per employee." });

            // Validate accident type
            if (dto.AccidentType != "Complete" && dto.AccidentType != "Partial")
                return BadRequest(new { message = "AccidentType must be 'Complete' or 'Partial'." });

            if (dto.AccidentType == "Partial")
            {
                if (!dto.AccidentPercentage.HasValue || dto.AccidentPercentage <= 0 || dto.AccidentPercentage > 100)
                    return BadRequest(new { message = "For Partial accident, AccidentPercentage must be between 1 and 100." });
            }

            // Calculate payout
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
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            // Mark accident claim raised immediately (prevent duplicates)
            employee.AccidentClaimRaised = true;

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Accident claim submitted successfully.",
                claimId = claim.Id,
                accidentType = dto.AccidentType,
                claimAmount,
                breakdown = dto.AccidentType == "Partial"
                    ? new { accidentCoverage = policy.AccidentCoverage, percentage = dto.AccidentPercentage, payout = claimAmount }
                    : new { accidentCoverage = policy.AccidentCoverage, percentage = (decimal?)100, payout = claimAmount }
            });
        }

        // ─── CUSTOMER: view all my claims ────────────────────────────────────
        // GET api/claim/my
        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyClaims()
        {
            int customerId = GetUserId();

            var claims = await _context.Claims
                .Include(c => c.Employee)
                .Include(c => c.CompanyPolicy).ThenInclude(cp => cp.Policy)
                .Include(c => c.CompanyPolicy).ThenInclude(cp => cp.Company)
                .Include(c => c.Customer)
                .Include(c => c.ClaimsManager)
                .Where(c => c.CustomerId == customerId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(claims.Select(MapClaim));
        }

        // ─── CLAIMS MANAGER: view all assigned claims ─────────────────────────
        // GET api/claim/manager
        [HttpGet("manager")]
        [Authorize(Roles = "ClaimsManager")]
        public async Task<IActionResult> GetManagerClaims()
        {
            int managerId = GetUserId();

            var claims = await _context.Claims
                .Include(c => c.Employee)
                .Include(c => c.CompanyPolicy).ThenInclude(cp => cp.Policy)
                .Include(c => c.CompanyPolicy).ThenInclude(cp => cp.Company)
                .Include(c => c.Customer)
                .Include(c => c.ClaimsManager)
                .Where(c => c.ClaimsManagerId == managerId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(claims.Select(MapClaim));
        }

        // ─── CLAIMS MANAGER: approve or reject a claim ───────────────────────
        // PUT api/claim/{id}/process
        [HttpPut("{id}/process")]
        [Authorize(Roles = "ClaimsManager")]
        public async Task<IActionResult> ProcessClaim(int id, [FromBody] ProcessClaimDto dto)
        {
            int managerId = GetUserId();

            if (dto.Decision != "Approved" && dto.Decision != "Rejected")
                return BadRequest(new { message = "Decision must be 'Approved' or 'Rejected'." });

            var claim = await _context.Claims
                .Include(c => c.Employee)
                .Include(c => c.CompanyPolicy).ThenInclude(cp => cp.Policy)
                .FirstOrDefaultAsync(c => c.Id == id && c.ClaimsManagerId == managerId);

            if (claim == null)
                return NotFound(new { message = "Claim not found or not assigned to you." });

            if (claim.Status != "Pending")
                return BadRequest(new { message = "This claim has already been processed." });

            claim.Status = dto.Decision;
            claim.ClaimsManagerNote = dto.Note;
            claim.ProcessedAt = DateTime.UtcNow;

            // ── Side effects on APPROVAL ──────────────────────────────────────

            if (dto.Decision == "Approved")
            {
                var employee = claim.Employee;
                var policy = claim.CompanyPolicy.Policy;

                switch (claim.ClaimType)
                {
                    case "Health":
                        // Deduct from remaining health coverage
                        if (employee.HealthCoverageRemaining == null)
                            employee.HealthCoverageRemaining = policy.HealthCoverage;

                        employee.HealthCoverageRemaining -= claim.ClaimAmount;
                        break;

                    case "TermLife":
                        // Employee is deceased — mark inactive
                        employee.IsActive = false;
                        break;

                    case "Accident":
                        // AccidentClaimRaised was already set when raised — no extra action needed
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

            return Ok(new { message = resultMsg, claimId = id, decision = dto.Decision });
        }

        // ─── ADMIN: view all claims ───────────────────────────────────────────
        // GET api/claim/all
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllClaims()
        {
            var claims = await _context.Claims
                .Include(c => c.Employee)
                .Include(c => c.CompanyPolicy).ThenInclude(cp => cp.Policy)
                .Include(c => c.CompanyPolicy).ThenInclude(cp => cp.Company)
                .Include(c => c.Customer)
                .Include(c => c.ClaimsManager)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(claims.Select(MapClaim));
        }

        private int GetUserId() =>
     int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}