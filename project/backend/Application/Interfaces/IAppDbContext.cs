using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<Company> Companies { get; set; }
        DbSet<Employee> Employees { get; set; }
        DbSet<Policy> Policies { get; set; }
        DbSet<User> Users { get; set; }
        DbSet<CompanyPolicy> CompanyPolicies { get; set; }
        DbSet<Payment> Payments { get; set; }
        DbSet<AgentCommission> AgentCommissions { get; set; }
        DbSet<Quote> Quotes { get; set; }
        DbSet<QuoteRequest> QuoteRequests { get; set; }
        DbSet<Recommendation> Recommendations { get; set; }
        DbSet<RecommendationPolicy> RecommendationPolicies { get; set; }
        DbSet<Claim> Claims { get; set; }
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
