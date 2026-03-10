using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPaymentService
    {
        Task<Application.DTOs.PaymentResponseDto> ProcessPaymentAsync(int customerId, Application.DTOs.ProcessPaymentDto dto);
        Task<IEnumerable<Application.DTOs.PaymentResponseDto>> GetMyPaymentsAsync(int customerId);
        Task<IEnumerable<object>> GetAgentCommissionsAsync(int agentId);
    }
}
