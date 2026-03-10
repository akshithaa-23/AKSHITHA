using Application.DTOs;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<int> RegisterCustomerAsync(RegisterCustomerDto request);
    }
}
