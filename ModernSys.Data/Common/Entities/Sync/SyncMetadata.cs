using System;

namespace ModularSys.Data.Common.Entities.Sync
{
    /// <summary>
    /// Tracks synchronization state for each entity type
    /// </summary>
    public class SyncMetadata
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Name of the entity table (e.g., "Products", "SalesOrders")
        /// </summary>
        public string EntityName { get; set; } = string.Empty;
        
        /// <summary>
        /// Primary key value of the synced entity
        /// </summary>
        public string EntityId { get; set; } = string.Empty;
        
        /// <summary>
        /// Timestamp of last successful sync for this entity
        /// </summary>
        public DateTime LastSyncedAt { get; set; }
        
        /// <summary>
        /// Hash of entity data at last sync (for detecting changes)
        /// </summary>
        public string? DataHash { get; set; }
        
        /// <summary>
        /// Sync direction: "LocalToCloud", "CloudToLocal", "Bidirectional"
        /// </summary>
        public string SyncDirection { get; set; } = "Bidirectional";
        
        /// <summary>
        /// Sync status: "Pending", "InProgress", "Completed", "Failed", "Conflict"
        /// </summary>
        public string Status { get; set; } = "Pending";
        
        /// <summary>
        /// Error message if sync failed
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// Number of retry attempts
        /// </summary>
        public int RetryCount { get; set; } = 0;
        
        /// <summary>
        /// Next retry time (for exponential backoff)
        /// </summary>
        public DateTime? NextRetryAt { get; set; }
        
        /// <summary>
        /// Indicates if this entity was created locally (not yet in cloud)
        /// </summary>
        public bool IsLocalOnly { get; set; } = false;
        
        /// <summary>
        /// Conflict resolution strategy used: "LastWriteWins", "Manual", "Skip"
        /// </summary>
        public string? ConflictResolution { get; set; }
        
        /// <summary>
        /// Timestamp when conflict was detected
        /// </summary>
        public DateTime? ConflictDetectedAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
