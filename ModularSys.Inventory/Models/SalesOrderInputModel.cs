using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSys.Inventory.Models
{
    public class SalesOrderInputModel
    {
        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required, Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        public decimal TotalAmount => Quantity * UnitPrice;
        public decimal Tax => TotalAmount * 0.12m;
        public decimal GrandTotal => TotalAmount + Tax;
    }

}
