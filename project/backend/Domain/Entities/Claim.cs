namespace Domain.Entities
{
    public class Claim
    {
        public int Id { get; set; }
        public int CompanyPolicyId { get; set; }   // must have active policy
        public int EmployeeId { get; set; }          // claim is FOR a specific employee
        public int CustomerId { get; set; }          // raised BY the customer (company rep)
        public int ClaimsManagerId { get; set; }     // auto-assigned

        // "Health" | "TermLife" | "Accident"
        public string ClaimType { get; set; } = string.Empty;

        // Health: requested amount (must be <= remaining health coverage)
        // TermLife: auto-calculated (multiplier × salary, capped at max)
        // Accident: auto-calculated (full or partial %)
        public decimal ClaimAmount { get; set; }

        // For accident only: "Complete" | "Partial"
        public string? AccidentType { get; set; }

        // For partial accident: 0-100
        public decimal? AccidentPercentage { get; set; }

        // ── Health Claim specific fields ──
        public decimal? AgeFactor { get; set; } // E.g. 0.95 for 5% reduction
        public decimal? FrequencyFactor { get; set; } // E.g. 0.90 for 10% reduction
        public decimal? FinalApprovedAmount { get; set; } // Calculated approved amount

        // ── Term Life Claim specific fields ──
        // "Natural Causes" | "Suicide" | "Other"
        public string? CauseOfDeath { get; set; } 
        public string? CauseOfDeathDescription { get; set; }
        public DateTime? DateOfDeath { get; set; }
        public decimal? NormalPayout { get; set; }
        public decimal? AdjustedPayout { get; set; }
        public bool? SuicideExclusionFlag { get; set; }
        public int? DaysInCompany { get; set; }

        // ── Accident Claim specific fields ──
        public DateTime? AccidentDate { get; set; }

        // Accident claim documents
        public string? FirDocumentPath { get; set; }
        public string? HospitalReportPath { get; set; }

        // "Pending" | "Approved" | "Rejected"
        public string Status { get; set; } = "Pending";

        public string? RejectionReason { get; set; }
        public string? ClaimsManagerNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }

        public string? DocumentUrl { get; set; }

        // Navigation
        public CompanyPolicy CompanyPolicy { get; set; } = null!;
        public Employee Employee { get; set; } = null!;
        public User Customer { get; set; } = null!;
        public User ClaimsManager { get; set; } = null!;
    }
}