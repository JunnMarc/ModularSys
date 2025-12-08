using System;
using System;
using System.ComponentModel.DataAnnotations;
using ModularSys.Data.Common.Interfaces;

namespace ModularSys.Data.Common.Entities.Helpdesk
{
    public class TicketComment : ISoftDeletable
    {
        public int Id { get; set; }
        
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = default!;
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        public bool IsInternal { get; set; } // Internal note vs Public reply
        
        public string? AuthorName { get; set; } // Can be User or Customer

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
