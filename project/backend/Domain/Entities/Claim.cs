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

        // "Pending" | "Approved" | "Rejected"
        public string Status { get; set; } = "Pending";

        public string? RejectionReason { get; set; }
        public string? ClaimsManagerNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }

        // Navigation
        public CompanyPolicy CompanyPolicy { get; set; } = null!;
        public Employee Employee { get; set; } = null!;
        public User Customer { get; set; } = null!;
        public User ClaimsManager { get; set; } = null!;
    }
}