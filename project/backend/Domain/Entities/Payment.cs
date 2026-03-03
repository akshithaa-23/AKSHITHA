using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public int QuoteId { get; set; }
        public int CustomerId { get; set; }
        public int PolicyId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // Card, NetBanking, UPI
        public string CardHolderName { get; set; } = string.Empty;
        public string MaskedCardNumber { get; set; } = string.Empty; // last 4 digits only
        public decimal AmountPaid { get; set; }
        public string Status { get; set; } = "Success";
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Quote Quote { get; set; } = null!;
        public User Customer { get; set; } = null!;
        public Policy Policy { get; set; } = null!;
        public AgentCommission? AgentCommission { get; set; }
    }
}
