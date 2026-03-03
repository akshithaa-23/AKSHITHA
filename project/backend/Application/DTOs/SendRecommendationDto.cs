using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class SendRecommendationDto
    {
        public int QuoteRequestId { get; set; }
        public string AgentMessage { get; set; } = string.Empty;
        // PolicyIds NOT entered by agent — auto-determined by employee count
    }

    public class RecommendationResponseDto
    {
        public int Id { get; set; }
        public int QuoteRequestId { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public string AgentMessage { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public int NumberOfEmployees { get; set; }
        public List<PolicyDto> RecommendedPolicies { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
