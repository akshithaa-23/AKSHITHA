using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Recommendation
    {
        public int Id { get; set; }
        public int QuoteRequestId { get; set; }
        public int AgentId { get; set; }
        public int CustomerId { get; set; }
        public string AgentMessage { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public QuoteRequest QuoteRequest { get; set; } = null!;
        public User Agent { get; set; } = null!;
        public User Customer { get; set; } = null!;
        public ICollection<RecommendationPolicy> RecommendationPolicies { get; set; } = new List<RecommendationPolicy>();
    }

    // Junction table — agent can recommend multiple policies
    public class RecommendationPolicy
    {
        public int Id { get; set; }
        public int RecommendationId { get; set; }
        public int PolicyId { get; set; }

        public Recommendation Recommendation { get; set; } = null!;
        public Policy Policy { get; set; } = null!;
    }
}
