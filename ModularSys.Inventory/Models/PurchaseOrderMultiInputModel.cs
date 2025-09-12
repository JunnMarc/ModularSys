using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSys.Inventory.Models
{
    public class PurchaseOrderMultiInputModel
    {
        public List<PurchaseOrderLineInputModel> Lines { get; set; } = new();

        public decimal TotalAmount => Lines.Sum(l => l.LineTotal);
        public decimal Tax => TotalAmount * 0.12m;
        public decimal GrandTotal => TotalAmount + Tax;
    }
}
