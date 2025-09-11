using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSys.Data.Common.Entities.Inventory
{
    public class Product
    {
        public int ProductId { get; set; }
        public string SKU { get; set; } = string.Empty; // Stock Keeping Unit
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public Category Category { get; set; } = default!;
        public decimal UnitPrice { get; set; } // Selling price
        public decimal CostPrice { get; set; } // Purchase cost
        public int QuantityOnHand { get; set; }
        public int ReorderLevel { get; set; } // Low-stock threshold
    }

}

