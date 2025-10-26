using System;

namespace ModularSys.Data.Common.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        
        // User Information
        public string? Username { get; set; }
        public int? UserId { get; set; }
        
        // Action Information
        public required string Action { get; set; } // Create, Update, Delete, Login, Logout, PasswordChange, etc.
        public required string EntityName { get; set; } // User, Product, Order, etc.
        public string? EntityId { get; set; } // ID of the affected entity
        
        // Change Details
        public string? OldValues { get; set; } // JSON of old values
        public string? NewValues { get; set; } // JSON of new values
        public string? Changes { get; set; } // Summary of changes
        
        // Request Information
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? RequestPath { get; set; }
        
        // Additional Context
        public string? Description { get; set; }
        public string? Category { get; set; } // Security, Data, System, User, etc.
        public string? Severity { get; set; } // Info, Warning, Error, Critical
        
        // Timestamp
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        // Success/Failure
        public bool Success { get; set; } = true;
        public string? ErrorMessage { get; set; }
    }
}
