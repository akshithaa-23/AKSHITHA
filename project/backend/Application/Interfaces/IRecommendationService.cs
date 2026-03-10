using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IRecommendationService
    {
        Task<object> SendRecommendationAsync(int agentId, SendRecommendationDto dto);
        Task<IEnumerable<RecommendationResponseDto>> GetMyRecommendationsAsync(int customerId);
        Task<IEnumerable<RecommendationResponseDto>> GetAgentRecommendationsAsync(int agentId);
    }
}
