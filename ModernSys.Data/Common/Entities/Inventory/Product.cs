using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularSys.Data.Common.Interfaces;

namespace ModularSys.Data.Common.Entities.Inventory
{
    public class Product : ISoftDeletable
    {
        public int ProductId { get; set; }
        public string SKU { get; set; } = string.Empty; // Stock Keeping Unit
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Barcode { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; } = default!;
        public decimal UnitPrice { get; set; } // Selling price
        public decimal CostPrice { get; set; } // Purchase cost
        public int QuantityOnHand { get; set; }
        public int ReorderLevel { get; set; } // Low-stock threshold
        public int? MinStockLevel { get; set; } // Minimum stock before critical alert
        public int? MaxStockLevel { get; set; } // Maximum stock for optimal inventory
        public DateTime? ExpiryDate { get; set; }
        public string? BatchNumber { get; set; }
        public string? Supplier { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }

        // Soft Delete Properties
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation Properties
        public ICollection<SalesOrderLine>? SalesOrderLines { get; set; }
        public ICollection<PurchaseOrderLine>? PurchaseOrderLines { get; set; }
        public ICollection<InventoryTransaction>? InventoryTransactions { get; set; }
    }

}

