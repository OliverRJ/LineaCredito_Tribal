using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrivalCreditoWebApi.Models
{
    public class Request
    {
        public Guid RequestId { get; set; } = Guid.NewGuid();
        public string FoundingType { get; set; }
        public decimal CashBalance { get; set; }
        public decimal MontlyRevenue { get; set; }
        public int RequestCreditLine { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.Now;
    }
}
