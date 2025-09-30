using System;

namespace ModularSys.Data.Common.Entities.Sync
{
    /// <summary>
    /// Audit log for all synchronization operations
    /// </summary>
    public class SyncLog
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Unique identifier for this sync session
        /// </summary>
        public Guid SyncSessionId { get; set; }
        
        /// <summary>
        /// When the sync operation started
        /// </summary>
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// When the sync operation completed
        /// </summary>
        public DateTime? CompletedAt { get; set; }
        
        /// <summary>
        /// Sync type: "Full", "Incremental", "Manual"
        /// </summary>
        public string SyncType { get; set; } = "Incremental";
        
        /// <summary>
        /// Sync direction: "LocalToCloud", "CloudToLocal", "Bidirectional"
        /// </summary>
        public string Direction { get; set; } = "Bidirectional";
        
        /// <summary>
        /// Overall status: "InProgress", "Completed", "Failed", "PartialSuccess"
        /// </summary>
        public string Status { get; set; } = "InProgress";
        
        /// <summary>
        /// Number of entities successfully synced
        /// </summary>
        public int EntitiesSynced { get; set; } = 0;
        
        /// <summary>
        /// Number of entities that failed to sync
        /// </summary>
        public int EntitiesFailed { get; set; } = 0;
        
        /// <summary>
        /// Number of conflicts detected
        /// </summary>
        public int ConflictsDetected { get; set; } = 0;
        
        /// <summary>
        /// Number of conflicts resolved
        /// </summary>
        public int ConflictsResolved { get; set; } = 0;
        
        /// <summary>
        /// Detailed error message if sync failed
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// Stack trace for debugging
        /// </summary>
        public string? ErrorStackTrace { get; set; }
        
        /// <summary>
        /// Additional details about the sync operation (JSON format)
        /// </summary>
        public string? Details { get; set; }
        
        /// <summary>
        /// Device/client identifier
        /// </summary>
        public string? DeviceId { get; set; }
        
        /// <summary>
        /// User who initiated the sync
        /// </summary>
        public string? InitiatedBy { get; set; }
    }
}
