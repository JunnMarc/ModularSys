using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSys.Data.Common.Entities.Inventory
{
    public class PurchaseOrder
    {
        public int PurchaseOrderId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending"; // Pending, Received, Cancelled
        public decimal TotalAmount { get; set; }
        public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
        [NotMapped]
        public decimal Tax => TotalAmount * 0.12m;

        [NotMapped]
        public decimal GrandTotal => TotalAmount + Tax;

    }

}
