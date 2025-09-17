using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularSys.Data.Common.Interfaces;

namespace ModularSys.Data.Common.Entities.Inventory
{
    public class InventoryTransaction : ISoftDeletable
    {
        public int InventoryTransactionId { get; set; }
        public string TransactionNumber { get; set; } = string.Empty; // Auto-generated transaction number
        public int ProductId { get; set; }
        public Product Product { get; set; } = default!;
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } = string.Empty; // Sale, Purchase, Adjustment, Return, Transfer
        public int QuantityBefore { get; set; } // Stock before transaction
        public int QuantityChange { get; set; } // Negative for outbound, positive for inbound
        public int QuantityAfter { get; set; } // Stock after transaction
        public decimal UnitCost { get; set; } // Cost per unit at time of transaction
        public decimal Amount { get; set; } // Total money value of transaction
        public string? Reason { get; set; } // Reason for adjustment/transfer
        public string? BatchNumber { get; set; }
        public string? Reference { get; set; } // Reference to sales/purchase order
        public int? ReferenceId { get; set; } // ID of related order
        public string? Notes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }

        // Soft Delete Properties
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

}
