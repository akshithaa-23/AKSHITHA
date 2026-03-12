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
        public string CauseOfDeath { get; set; } = string.Empty; // "Natural Causes" | "Suicide" | "Other"
        public string? CauseOfDeathDescription { get; set; }
        public DateTime DateOfDeath { get; set; }
        // No amount — backend calculates: salary × lifeCoverageMultiplier, capped at maxLifeCoverageLimit
        public string? DocumentUrl { get; set; }
    }

    // Customer raises an Accident claim
    public class RaiseAccidentClaimDto
    {
        public int EmployeeId { get; set; }
        public string AccidentType { get; set; } = string.Empty;  // "Complete" | "Partial"
        public decimal? AccidentPercentage { get; set; }           // only if Partial (0-100)
        public DateTime AccidentDate { get; set; }
        public string? FirDocumentUrl { get; set; }       // required: URL to uploaded FIR copy
        public string? HospitalReportUrl { get; set; }   // required: URL to uploaded hospital report
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
        
        public decimal? AgeFactor { get; set; }
        public decimal? FrequencyFactor { get; set; }
        public decimal? FinalApprovedAmount { get; set; }
        
        // Term Life Specific breakdown fields
        public string? CauseOfDeath { get; set; }
        public string? CauseOfDeathDescription { get; set; }
        public DateTime? DateOfDeath { get; set; }
        public decimal? NormalPayout { get; set; }
        public decimal? AdjustedPayout { get; set; }
        public bool? SuicideExclusionFlag { get; set; }
        public int? DaysInCompany { get; set; }
        public DateTime? EmployeeJoinDate { get; set; }

        public DateTime? AccidentDate { get; set; }
        // Accident claim specific response fields
        public int? DaysSinceAccident { get; set; }
        public DateTime? ClaimDeadline { get; set; }
        public string? FirDocumentUrl { get; set; }
        public string? HospitalReportUrl { get; set; }

        public decimal? RequestedAmount { get; set; }
        public int? EmployeeAge { get; set; }
        public int? ClaimNumberInYear { get; set; }

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
