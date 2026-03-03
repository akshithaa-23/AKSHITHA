using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CompanyDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int Size { get; set; }
        public string Domain { get; set; } = string.Empty;
        public string RepresentativeName { get; set; } = string.Empty;
        public string RepresentativeEmail { get; set; } = string.Empty;
        public string? AgentName { get; set; }
        public string? AgentEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public bool HasActivePolicy { get; set; }
        public string? ActivePolicyName { get; set; }
    }
}
