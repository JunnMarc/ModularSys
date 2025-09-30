using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularSys.Data.Common.Interfaces;
using ModularSys.Data.Common.Entities.CRM;

namespace ModularSys.Data.Common.Entities.Inventory
{
    public class SalesOrder : ISoftDeletable
    {
        public int SalesOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty; // Auto-generated order number
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Processing, Shipped, Delivered, Cancelled
        public string? CancellationReason { get; set; } // Reason for cancellation if status is Cancelled
        public DateTime? CancelledAt { get; set; } // When the order was cancelled
        public string? CancelledBy { get; set; } // Who cancelled the order
        
        // CRM Integration - Optional link to Customer entity
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        
        // Legacy fields - kept for backward compatibility and quick orders without customer link
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public string? ShippingAddress { get; set; }
        public string? Notes { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxRate { get; set; } = 0.12m;
        public decimal DiscountAmount { get; set; } = 0;
        public decimal ShippingCost { get; set; } = 0;
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Partial, Refunded
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }

        // Soft Delete Properties
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public ICollection<SalesOrderLine> Lines { get; set; } = new List<SalesOrderLine>();

        [NotMapped]
        public decimal TotalAmount { get => SubTotal - DiscountAmount; private set { } }

        [NotMapped]
        public decimal TaxAmount => (SubTotal - DiscountAmount) * TaxRate;

        [NotMapped]
        public decimal GrandTotal => (SubTotal - DiscountAmount) + ((SubTotal - DiscountAmount) * TaxRate) + ShippingCost;
    }

}
