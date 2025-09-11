using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSys.Data.Common.Entities.Inventory
{
    public class InventoryTransaction
    {
        public int InventoryTransactionId { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = default!;
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public string TransactionType { get; set; } = string.Empty; // Sale, Purchase, Adjustment
        public int QuantityChange { get; set; } // Negative for sales, positive for purchases
        public decimal Amount { get; set; } // Money earned or spent
    }

}
