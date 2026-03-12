namespace Domain.Entities
{
    public class QuoteRequest
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int? PolicyId { get; set; }
        // Set by backend automatically — never by user
        // "Recommendation" | "DirectBuy"
        public string RequestType { get; set; } = "Recommendation";
        public string CompanyName { get; set; } = string.Empty;
        public string IndustryType { get; set; } = string.Empty;
        public int NumberOfEmployees { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? LocationCategory { get; set; }
        public string ContactName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        // Pending → Assigned → RecommendationSent → QuoteSent → Accepted → Completed
        // or Rejected
        public string Status { get; set; } = "Pending";
        public string? CustomIndustry { get; set; }
        public decimal? IndustryFactor { get; set; }
        public decimal? GeographyFactor { get; set; }
        public decimal? PlanRiskFactor { get; set; }
        public int? AssignedAgentId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User Customer { get; set; } = null!;
        public User? AssignedAgent { get; set; }
        public Policy? Policy { get; set; }
        public Recommendation? Recommendation { get; set; }
        public Quote? Quote { get; set; }
    }
}