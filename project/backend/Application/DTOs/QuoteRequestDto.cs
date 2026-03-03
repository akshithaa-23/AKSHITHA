using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class QuoteRequestDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public string IndustryType { get; set; } = string.Empty;
        public int NumberOfEmployees { get; set; }
        public string Location { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
    }
    public class DirectBuyRequestDto
    {
        public int PolicyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string IndustryType { get; set; } = string.Empty;
        public int NumberOfEmployees { get; set; }
        public string Location { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
    }
    public class QuoteRequestResponseDto
    {
        public int Id { get; set; }
        public int? PolicyId { get; set; }
        public string? PolicyName { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string IndustryType { get; set; } = string.Empty;
        public int NumberOfEmployees { get; set; }
        public string Location { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AssignedAgentName { get; set; }
        public string? AssignedAgentEmail { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }



    public class SendQuoteDto
    {
        public int QuoteRequestId { get; set; }
        public int PolicyId { get; set; }
        public int EmployeeCount { get; set; }
    }

    public class QuoteResponseDto
    {
        public int Id { get; set; }
        public int QuoteRequestId { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public int PolicyId { get; set; }
        public int EmployeeCount { get; set; }
        public decimal PremiumPerEmployee { get; set; }
        public decimal TotalPremium { get; set; }
        public string Status { get; set; } = string.Empty;
        public string AgentName { get; set; } = string.Empty;
        public DateTime ValidUntil { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
