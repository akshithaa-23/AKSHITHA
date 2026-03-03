using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class AgentCommission
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public int AgentId { get; set; }
        public decimal CommissionRate { get; set; }   // percentage
        public decimal CommissionAmount { get; set; } // calculated amount
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Payment Payment { get; set; } = null!;
        public User Agent { get; set; } = null!;
    }
}
