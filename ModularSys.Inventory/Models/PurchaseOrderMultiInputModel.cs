using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSys.Inventory.Models
{
    public class PurchaseOrderMultiInputModel
    {
        public string? SupplierName { get; set; }
        public string? SupplierEmail { get; set; }
        public string? SupplierPhone { get; set; }
        public string? SupplierAddress { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? Notes { get; set; }
        public string? PaymentTerms { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal ShippingCost { get; set; } = 0;

        public List<PurchaseOrderLineInputModel> Lines { get; set; } = new();

        public decimal SubTotal => Lines.Sum(l => l.LineTotal);
        public decimal TaxRate => 0.12m;
        public decimal TaxAmount => (SubTotal - DiscountAmount) * TaxRate;
        public decimal GrandTotal => (SubTotal - DiscountAmount) + ((SubTotal - DiscountAmount) * TaxRate) + ShippingCost;
    }
}
