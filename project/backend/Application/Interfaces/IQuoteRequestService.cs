using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IQuoteRequestService
    {
        Task<object> RequestRecommendationAsync(int customerId, QuoteRequestDto dto);
        Task<object> DirectBuyAsync(int customerId, DirectBuyRequestDto dto);
        Task<IEnumerable<QuoteRequestResponseDto>> GetMyRequestsAsync(int customerId);
        Task<IEnumerable<QuoteRequestResponseDto>> GetAllAsync();
        Task<IEnumerable<QuoteRequestResponseDto>> GetAgentRequestsAsync(int agentId);
    }
}
