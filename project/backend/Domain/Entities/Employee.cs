namespace Domain.Entities
{
    public class Employee
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CoverageStartDate { get; set; } = DateTime.UtcNow;

        public DateTime DateOfBirth { get; set; }
        public DateTime EmployeeJoinDate { get; set; }

        // Nominee info
        public string NomineeName { get; set; } = string.Empty;
        public string NomineeRelationship { get; set; } = string.Empty;
        public string NomineePhone { get; set; } = string.Empty;

        // ── CLAIMS tracking ───────────────────────────────────────
        // Starts = policy's HealthCoverage; decremented per approved health claim
        public decimal? HealthCoverageRemaining { get; set; }

        // Once a TermLife claim is raised (and approved), employee becomes inactive
        // Once an Accident claim is raised, no more accident claims allowed
        public bool AccidentClaimRaised { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Company Company { get; set; } = null!;
        public ICollection<Claim> Claims { get; set; } = new List<Claim>();
    }
}