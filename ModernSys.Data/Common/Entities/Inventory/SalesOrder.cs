using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSys.Data.Common.Entities.Inventory
{
    public class SalesOrder
    {
        public int SalesOrderId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public ICollection<SalesOrderLine> Lines { get; set; } = new List<SalesOrderLine>();
        [NotMapped]
        public decimal Tax => TotalAmount * 0.12m;

        [NotMapped]
        public decimal GrandTotal => TotalAmount + Tax;
    }

}
