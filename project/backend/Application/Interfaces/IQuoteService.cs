using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IQuoteService
    {
        Task<object> SendQuoteAsync(int agentId, SendQuoteDto dto);
        Task<IEnumerable<QuoteResponseDto>> GetMyQuotesAsync(int customerId);
        Task<IEnumerable<QuoteResponseDto>> GetAgentQuotesAsync(int agentId);
        Task<string> AcceptQuoteAsync(int customerId, int quoteId);
        Task<string> RejectQuoteAsync(int customerId, int quoteId);
    }
}
