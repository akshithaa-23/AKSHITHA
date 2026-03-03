using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Policy
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public decimal HealthCoverage { get; set; }

        // Life Coverage (Optional)
        public int? LifeCoverageMultiplier { get; set; }
        public decimal? MaxLifeCoverageLimit { get; set; }

        // Accident Coverage (Only for Pro)
        public decimal? AccidentCoverage { get; set; }

        public decimal PremiumPerEmployee { get; set; }
        public int MinEmployees { get; set; }
        public int DurationYears { get; set; } = 1;

        public bool IsPopular { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<CompanyPolicy> CompanyPolicies { get; set; } = new List<CompanyPolicy>();
    }
}
