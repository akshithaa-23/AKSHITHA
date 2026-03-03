using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ProcessPaymentDto
    {
        public int QuoteId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // Card | NetBanking | UPI
        public string CardHolderName { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;    // we only store last 4
    }

    public class PaymentResponseDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string PolicyName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string MaskedCardNumber { get; set; } = string.Empty;
        public string CardHolderName { get; set; } = string.Empty;
        public DateTime PaidAt { get; set; }
        public decimal CommissionAmount { get; set; }
        public string AgentName { get; set; } = string.Empty;
    }
}
