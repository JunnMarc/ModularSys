using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularSys.Data.Common.Interfaces;

namespace ModularSys.Data.Common.Entities.Inventory
{
    public class PurchaseOrder : ISoftDeletable
    {
        public int PurchaseOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty; // Auto-generated PO number
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Ordered, Received, Cancelled
        public string? SupplierName { get; set; }
        public string? SupplierEmail { get; set; }
        public string? SupplierPhone { get; set; }
        public string? SupplierAddress { get; set; }
        public string? Notes { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxRate { get; set; } = 0.12m;
        public decimal DiscountAmount { get; set; } = 0;
        public decimal ShippingCost { get; set; } = 0;
        public string? PaymentTerms { get; set; }
        public string? PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Partial
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }

        // Soft Delete Properties
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();

        [NotMapped]
        public decimal TotalAmount { get => SubTotal - DiscountAmount; private set { } }

        [NotMapped]
        public decimal TaxAmount => (SubTotal - DiscountAmount) * TaxRate;

        [NotMapped]
        public decimal GrandTotal => (SubTotal - DiscountAmount) + ((SubTotal - DiscountAmount) * TaxRate) + ShippingCost;
    }

}
