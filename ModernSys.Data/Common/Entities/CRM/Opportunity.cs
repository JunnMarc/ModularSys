using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularSys.Data.Common.Interfaces;

namespace ModularSys.Data.Common.Entities.CRM
{
    public class Opportunity : ISoftDeletable
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        
        [Column("OpportunityName")]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Column("EstimatedValue")]
        public decimal Value { get; set; }
        public string Stage { get; set; } = "Prospecting"; // Prospecting, Qualification, Proposal, Negotiation, Won, Lost
        public int Probability { get; set; } = 10; // 0-100%
        public DateTime ExpectedCloseDate { get; set; }
        public DateTime? ActualCloseDate { get; set; }
        public string? LeadSource { get; set; }
        public string? Competitor { get; set; }
        public string? Notes { get; set; }
        public string? AssignedTo { get; set; }
        public string Priority { get; set; } = "Medium"; // High, Medium, Low

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
        public Customer Customer { get; set; } = default!;
    }
}
