namespace Application.DTOs
{
    public class PolicyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal HealthCoverage { get; set; }

        public int? LifeCoverageMultiplier { get; set; }
        public decimal? MaxLifeCoverageLimit { get; set; }

        public decimal? AccidentCoverage { get; set; }

        public decimal PremiumPerEmployee { get; set; }
        public int MinEmployees { get; set; }
        public int DurationYears { get; set; }
        public bool IsPopular { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePolicyDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal HealthCoverage { get; set; }

        public int? LifeCoverageMultiplier { get; set; }
        public decimal? MaxLifeCoverageLimit { get; set; }

        public decimal? AccidentCoverage { get; set; }

        public decimal PremiumPerEmployee { get; set; }
        public int MinEmployees { get; set; }
        public int DurationYears { get; set; } = 1;
        public bool IsPopular { get; set; } = false;
    }

    public class UpdatePolicyDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal HealthCoverage { get; set; }

        public int? LifeCoverageMultiplier { get; set; }
        public decimal? MaxLifeCoverageLimit { get; set; }

        public decimal? AccidentCoverage { get; set; }

        public decimal PremiumPerEmployee { get; set; }
        public int MinEmployees { get; set; }
        public int DurationYears { get; set; }
        public bool IsPopular { get; set; }
        public bool IsActive { get; set; }
    }
}