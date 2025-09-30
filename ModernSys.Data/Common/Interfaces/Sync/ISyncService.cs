using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModularSys.Data.Common.Interfaces.Sync
{
    /// <summary>
    /// Main synchronization service interface
    /// </summary>
    public interface ISyncService
    {
        /// <summary>
        /// Performs a full bidirectional sync between local and cloud databases
        /// </summary>
        Task<SyncResult> SyncAllAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Performs incremental sync (only changed records since last sync)
        /// </summary>
        Task<SyncResult> SyncIncrementalAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Syncs a specific entity type
        /// </summary>
        Task<SyncResult> SyncEntityAsync<TEntity>(CancellationToken cancellationToken = default) where TEntity : class;
        
        /// <summary>
        /// Pushes local changes to cloud
        /// </summary>
        Task<SyncResult> PushToCloudAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Pulls cloud changes to local
        /// </summary>
        Task<SyncResult> PullFromCloudAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Checks if internet connection is available
        /// </summary>
        Task<bool> IsOnlineAsync();
        
        /// <summary>
        /// Gets the last sync timestamp
        /// </summary>
        Task<DateTime?> GetLastSyncTimeAsync();
        
        /// <summary>
        /// Gets pending sync count (entities waiting to be synced)
        /// </summary>
        Task<int> GetPendingSyncCountAsync();
        
        /// <summary>
        /// Resolves a specific conflict manually
        /// </summary>
        Task ResolveConflictAsync(int syncMetadataId, ConflictResolutionStrategy strategy, CancellationToken cancellationToken = default);
    }
    
    /// <summary>
    /// Result of a synchronization operation
    /// </summary>
    public class SyncResult
    {
        public bool Success { get; set; }
        public Guid SessionId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public int EntitiesSynced { get; set; }
        public int EntitiesFailed { get; set; }
        public int ConflictsDetected { get; set; }
        public int ConflictsResolved { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> Errors { get; set; } = new();
        public Dictionary<string, int> EntityCounts { get; set; } = new();
    }
    
    /// <summary>
    /// Conflict resolution strategies
    /// </summary>
    public enum ConflictResolutionStrategy
    {
        LastWriteWins,
        FirstWriteWins,
        KeepLocal,
        KeepCloud,
        Manual
    }
}
