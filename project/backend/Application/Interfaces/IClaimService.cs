using Application.DTOs;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IClaimService
    {
        Task<object> GetAllowedClaimTypesAsync(int customerId);
        Task<object> RaiseHealthClaimAsync(int customerId, RaiseHealthClaimDto dto);
        Task<object> RaiseTermLifeClaimAsync(int customerId, RaiseTermLifeClaimDto dto);
        Task<object> RaiseAccidentClaimAsync(int customerId, RaiseAccidentClaimDto dto);
        Task<IEnumerable<ClaimResponseDto>> GetMyClaimsAsync(int customerId);
        Task<IEnumerable<ClaimResponseDto>> GetManagerClaimsAsync(int managerId);
        Task<object> ProcessClaimAsync(int managerId, int id, ProcessClaimDto dto);
        Task<IEnumerable<ClaimResponseDto>> GetAllClaimsAsync();
        Task<string> UploadClaimDocumentAsync(IFormFile file, string scheme, string host);
    }
}
