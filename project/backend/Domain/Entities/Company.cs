namespace Domain.Entities
{
    public class Company
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int Size { get; set; }
        public string Domain { get; set; } = string.Empty;
        public string RepresentativeName { get; set; } = string.Empty;
        public string RepresentativeEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Assigned agent (for quote/policy flow)
        public int? AgentId { get; set; }

        // Assigned claims manager (auto-assigned when first claim is raised)
        public int? ClaimsManagerId { get; set; }

        // Navigation
        public User Customer { get; set; } = null!;
        public User? Agent { get; set; }
        public User? ClaimsManager { get; set; }
        public ICollection<CompanyPolicy> CompanyPolicies { get; set; } = new List<CompanyPolicy>();
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}