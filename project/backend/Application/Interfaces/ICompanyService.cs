using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICompanyService
    {
        Task<object> GetMyCompanyAsync(int customerId);
        Task<IEnumerable<object>> GetAllAsync();
        Task<IEnumerable<object>> GetByAgentAsync();
        Task<IEnumerable<object>> GetPolicySummaryAsync();
    }
}
