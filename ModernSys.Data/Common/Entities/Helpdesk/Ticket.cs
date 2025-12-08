using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ModularSys.Data.Common.Entities.CRM;
using ModularSys.Data.Common.Interfaces;

namespace ModularSys.Data.Common.Entities.Helpdesk
{
    public class Ticket : ISoftDeletable
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Required]
        public string Status { get; set; } = "Open"; // Open, InProgress, Resolved, Closed
        
        [Required]
        public string Priority { get; set; } = "Medium"; // Critical, High, Medium, Low
        
        public string Channel { get; set; } = "Web"; // Web, Email, Phone, Chat
        
        // Relationships
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = default!;
        
        public int? AssignedToId { get; set; } // User ID
        // Note: We might link to User entity if it's in the same context, 
        // usually User is in Core/Auth but we'll assume ID reference for now or User linkage if possible.
        // Given ModularSysDbContext has Users, we can link it:
        public User? AssignedTo { get; set; }

        public int? CategoryId { get; set; }
        public TicketCategory? Category { get; set; }
        
        public int? SLAId { get; set; }
        public SLA? SLA { get; set; }
        
        // SLA Tracking
        public DateTime? FirstResponseDueAt { get; set; }
        public DateTime? ResolutionDueAt { get; set; }
        public DateTime? FirstResponseAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        
        public bool FirstResponseBreached { get; set; }
        public bool ResolutionBreached { get; set; }
        
        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();

        // ISoftDeletable
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
