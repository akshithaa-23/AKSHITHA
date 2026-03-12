using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Quote
    {
        public int Id { get; set; }
        public int QuoteRequestId { get; set; }
        public int AgentId { get; set; }
        public int CustomerId { get; set; }
        public int PolicyId { get; set; }
        public int EmployeeCount { get; set; }
        public decimal PremiumPerEmployee { get; set; }
        public decimal BaseQuote { get; set; }
        public decimal IndustryFactor { get; set; }
        public decimal GeographyFactor { get; set; }
        public decimal PlanRiskFactor { get; set; }
        public decimal TotalPremium { get; set; }       // EmployeeCount × PremiumPerEmployee
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected, Paid
        public DateTime ValidUntil { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public QuoteRequest QuoteRequest { get; set; } = null!;
        public User Agent { get; set; } = null!;
        public User Customer { get; set; } = null!;
        public Policy Policy { get; set; } = null!;
        public Payment? Payment { get; set; }
    }
}
