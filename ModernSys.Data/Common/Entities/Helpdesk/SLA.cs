using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ModularSys.Data.Common.Interfaces;

namespace ModularSys.Data.Common.Entities.Helpdesk
{
    public class SLA : ISoftDeletable
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        // Target response/resolution times in minutes
        public int ResponseTimeMinutes { get; set; } = 60; // Default 1 hour
        public int ResolutionTimeMinutes { get; set; } = 1440; // Default 24 hours
        
        public bool IsDefault { get; set; }

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
