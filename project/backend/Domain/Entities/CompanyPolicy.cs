using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class CompanyPolicy
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int PolicyId { get; set; }
        public string Status { get; set; } = "Active"; // Active, Expired, Cancelled
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int EmployeeCount { get; set; }
        public decimal TotalPremium { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Company Company { get; set; } = null!;
        public Policy Policy { get; set; } = null!;
    }
}
