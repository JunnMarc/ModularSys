using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSys.Data.Common.Entities.Finance
{
    public class RevenueTransaction
    {
        public int RevenueTransactionId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Source { get; set; } = string.Empty;     // "Sale", "Purchase", etc.
        public string Reference { get; set; } = string.Empty;  // e.g. "SO-1001"
        public decimal Amount { get; set; }                    // + for income, - for expense
    }
}
