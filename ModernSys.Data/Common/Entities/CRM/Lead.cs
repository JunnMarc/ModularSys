using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularSys.Data.Common.Interfaces;

namespace ModularSys.Data.Common.Entities.CRM
{
    public class Lead : ISoftDeletable
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string LeadSource { get; set; } = string.Empty; // Website, Referral, Cold Call, etc.
        public string? Industry { get; set; }
        public decimal? EstimatedValue { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = "New"; // New, Contacted, Qualified, Converted, Lost
        public string Priority { get; set; } = "Medium"; // High, Medium, Low
        public DateTime? FollowUpDate { get; set; }
        public string? AssignedTo { get; set; }

        // Soft Delete Properties (ISoftDeletable)
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        // Audit Properties (ISoftDeletable)
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
