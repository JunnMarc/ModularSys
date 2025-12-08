using System;
using ModularSys.Data.Common.Interfaces;

namespace ModularSys.Data.Common.Entities.Helpdesk
{
    public class SLACompliance : ISoftDeletable
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = default!;
        
        public string SLAType { get; set; } = "Response"; // Response, Resolution
        
        public DateTime DueAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        public bool IsBreached { get; set; }
        
        public int TimeToCompleteMinutes { get; set; }

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
