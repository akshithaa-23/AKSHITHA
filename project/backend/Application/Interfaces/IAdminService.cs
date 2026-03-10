using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAdminService
    {
        Task<int> RegisterUserAsync(RegisterUserDto request);
        Task<IEnumerable<object>> GetAllUsersAsync();
    }
}
