using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSys.Inventory.Models
{
    public class SalesOrderMultiInputModel
    {
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public string? ShippingAddress { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? Notes { get; set; }
        public string? PaymentMethod { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal ShippingCost { get; set; } = 0;

        public List<SalesOrderLineInputModel> Lines { get; set; } = new();

        public decimal SubTotal => Lines.Sum(l => l.LineTotal);
        public decimal TaxRate => 0.12m;
        public decimal TaxAmount => (SubTotal - DiscountAmount) * TaxRate;
        public decimal GrandTotal => (SubTotal - DiscountAmount) + ((SubTotal - DiscountAmount) * TaxRate) + ShippingCost;
    }
}
