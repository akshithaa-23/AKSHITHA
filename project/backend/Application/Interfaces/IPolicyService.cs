using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPolicyService
    {
        Task<IEnumerable<PolicyDto>> GetAllActiveAsync();
        Task<IEnumerable<PolicyDto>> GetAllAdminAsync();
        Task<PolicyDto> GetByIdAsync(int id);
        Task<int> CreateAsync(CreatePolicyDto dto);
        Task UpdateAsync(int id, UpdatePolicyDto dto);
        Task DeleteAsync(int id);
    }
}
