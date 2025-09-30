using System;

namespace ModularSys.Data.Common.Entities.Sync
{
    /// <summary>
    /// Configuration settings for synchronization behavior
    /// </summary>
    public class SyncConfiguration
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Name of the entity table this configuration applies to
        /// </summary>
        public string EntityName { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether sync is enabled for this entity
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// Sync priority (1=highest, 10=lowest)
        /// </summary>
        public int Priority { get; set; } = 5;
        
        /// <summary>
        /// Sync direction: "LocalToCloud", "CloudToLocal", "Bidirectional"
        /// </summary>
        public string Direction { get; set; } = "Bidirectional";
        
        /// <summary>
        /// Conflict resolution strategy: "LastWriteWins", "FirstWriteWins", "Manual"
        /// </summary>
        public string ConflictResolution { get; set; } = "LastWriteWins";
        
        /// <summary>
        /// Maximum number of retry attempts
        /// </summary>
        public int MaxRetries { get; set; } = 3;
        
        /// <summary>
        /// Initial retry delay in seconds
        /// </summary>
        public int RetryDelaySeconds { get; set; } = 5;
        
        /// <summary>
        /// Whether to use exponential backoff for retries
        /// </summary>
        public bool UseExponentialBackoff { get; set; } = true;
        
        /// <summary>
        /// Batch size for syncing records
        /// </summary>
        public int BatchSize { get; set; } = 100;
        
        /// <summary>
        /// Whether to sync deleted records (soft deletes)
        /// </summary>
        public bool SyncDeleted { get; set; } = true;
        
        /// <summary>
        /// Custom filter expression (JSON format)
        /// </summary>
        public string? FilterExpression { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
