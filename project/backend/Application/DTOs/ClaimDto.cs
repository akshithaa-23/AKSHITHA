using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    // ─── CLAIMS ───────────────────────────────────────────────────────────────────

    // Customer raises a Health claim for an employee
    public class RaiseHealthClaimDto
    {
        public int EmployeeId { get; set; }
        public decimal RequestedAmount { get; set; }   // must be <= HealthCoverageRemaining
        public string? DocumentUrl { get; set; }
    }

    // Customer raises a TermLife claim (amount auto-calculated from salary × multiplier, capped)
    public class RaiseTermLifeClaimDto
    {
        public int EmployeeId { get; set; }
        // No amount — backend calculates: salary × lifeCoverageMultiplier, capped at maxLifeCoverageLimit
        public string? DocumentUrl { get; set; }
    }

    // Customer raises an Accident claim
    public class RaiseAccidentClaimDto
    {
        public int EmployeeId { get; set; }
        public string AccidentType { get; set; } = string.Empty;  // "Complete" | "Partial"
        public decimal? AccidentPercentage { get; set; }           // only if Partial (0-100)
        public string? DocumentUrl { get; set; }
    }

    // Claims Manager: approve or reject a claim
    public class ProcessClaimDto
    {
        // "Approved" | "Rejected"
        public string Decision { get; set; } = string.Empty;
        public string? Note { get; set; }   // optional note / rejection reason
    }

    // Response DTO for any claim
    public class ClaimResponseDto
    {
        public int Id { get; set; }
        public string ClaimType { get; set; } = string.Empty;
        public decimal ClaimAmount { get; set; }
        public string? AccidentType { get; set; }
        public decimal? AccidentPercentage { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ClaimsManagerNote { get; set; }
        public string? DocumentUrl { get; set; }

        // Employee info
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public decimal EmployeeSalary { get; set; }
        public decimal? HealthCoverageRemaining { get; set; }

        // Policy info
        public string PolicyName { get; set; } = string.Empty;

        // Company info
        public string CompanyName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;

        // Claims manager info
        public string ClaimsManagerName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
