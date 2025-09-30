using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularSys.Data.Common.Interfaces;

namespace ModularSys.Data.Common.Entities.CRM
{
    public class Customer : ISoftDeletable
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? Industry { get; set; }
        public int? CompanySize { get; set; }
        public string? Website { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = "Active"; // Active, Inactive, Prospect
        public string CustomerType { get; set; } = "Prospect"; // Prospect, Customer, Partner
        // Soft Delete Properties (ISoftDeletable)
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        // Audit Properties (ISoftDeletable)
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation Properties
        public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
        public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
        
        // Inventory Integration - Sales Orders linked to this customer
        // Note: Using fully qualified name to avoid circular reference
        public ICollection<ModularSys.Data.Common.Entities.Inventory.SalesOrder> SalesOrders { get; set; } = new List<ModularSys.Data.Common.Entities.Inventory.SalesOrder>();
    }
}
