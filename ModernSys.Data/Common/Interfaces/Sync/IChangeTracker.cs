using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModularSys.Data.Common.Interfaces.Sync
{
    /// <summary>
    /// Tracks changes to entities for synchronization
    /// </summary>
    public interface IChangeTracker
    {
        /// <summary>
        /// Gets all entities that have changed since the last sync
        /// </summary>
        Task<List<ChangeRecord>> GetChangedEntitiesAsync<TEntity>(DateTime? since = null) where TEntity : class;
        
        /// <summary>
        /// Gets all entities that have been deleted (soft delete) since the last sync
        /// </summary>
        Task<List<ChangeRecord>> GetDeletedEntitiesAsync<TEntity>(DateTime? since = null) where TEntity : class;
        
        /// <summary>
        /// Marks an entity as synced
        /// </summary>
        Task MarkAsSyncedAsync(string entityName, string entityId, DateTime syncTime);
        
        /// <summary>
        /// Gets the last sync time for an entity type
        /// </summary>
        Task<DateTime?> GetLastSyncTimeAsync(string entityName);
        
        /// <summary>
        /// Computes a hash of entity data for change detection
        /// </summary>
        string ComputeEntityHash(object entity);
    }
    
    /// <summary>
    /// Represents a change to an entity
    /// </summary>
    public class ChangeRecord
    {
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public ChangeType ChangeType { get; set; }
        public DateTime ChangedAt { get; set; }
        public object Entity { get; set; } = null!;
        public string? DataHash { get; set; }
    }
    
    /// <summary>
    /// Type of change
    /// </summary>
    public enum ChangeType
    {
        Created,
        Updated,
        Deleted
    }
}
