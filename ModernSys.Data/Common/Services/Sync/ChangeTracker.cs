using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularSys.Data.Common.Interfaces;
using ModularSys.Data.Common.Interfaces.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ModularSys.Data.Common.Services.Sync
{
    /// <summary>
    /// Tracks changes to entities for synchronization using timestamp-based approach
    /// </summary>
    public class ChangeTracker : IChangeTracker
    {
        private readonly IConnectionManager _connectionManager;
        private readonly ILogger<ChangeTracker> _logger;

        public ChangeTracker(
            IConnectionManager connectionManager,
            ILogger<ChangeTracker> logger)
        {
            _connectionManager = connectionManager;
            _logger = logger;
        }

        public async Task<List<ChangeRecord>> GetChangedEntitiesAsync<TEntity>(DateTime? since = null) where TEntity : class
        {
            var changes = new List<ChangeRecord>();
            var entityName = typeof(TEntity).Name;

            try
            {
                using var context = _connectionManager.GetLocalContext();
                var dbSet = context.Set<TEntity>();
                
                IQueryable<TEntity> query = dbSet;

                // Filter by ISoftDeletable entities that are not deleted
                if (typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
                {
                    query = query.Cast<ISoftDeletable>()
                        .Where(e => !e.IsDeleted)
                        .Cast<TEntity>();
                }

                // Apply timestamp filter if provided
                if (since.HasValue && typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
                {
                    query = query.Cast<ISoftDeletable>()
                        .Where(e => e.UpdatedAt > since || (e.UpdatedAt == null && e.CreatedAt > since))
                        .Cast<TEntity>();
                }

                var entities = await query.ToListAsync();

                foreach (var entity in entities)
                {
                    var changeType = ChangeType.Updated;
                    DateTime changedAt = DateTime.UtcNow;

                    if (entity is ISoftDeletable softDeletable)
                    {
                        // Determine if it's a new entity or updated
                        if (softDeletable.CreatedAt.HasValue && 
                            (!softDeletable.UpdatedAt.HasValue || softDeletable.UpdatedAt == softDeletable.CreatedAt))
                        {
                            changeType = ChangeType.Created;
                            changedAt = softDeletable.CreatedAt.Value;
                        }
                        else if (softDeletable.UpdatedAt.HasValue)
                        {
                            changedAt = softDeletable.UpdatedAt.Value;
                        }
                    }

                    var entityId = GetEntityId(entity);
                    
                    changes.Add(new ChangeRecord
                    {
                        EntityName = entityName,
                        EntityId = entityId,
                        ChangeType = changeType,
                        ChangedAt = changedAt,
                        Entity = entity,
                        DataHash = ComputeEntityHash(entity)
                    });
                }

                _logger.LogInformation("Found {Count} changed {EntityName} entities since {Since}", 
                    changes.Count, entityName, since);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting changed entities for {EntityName}", entityName);
                throw;
            }

            return changes;
        }

        public async Task<List<ChangeRecord>> GetDeletedEntitiesAsync<TEntity>(DateTime? since = null) where TEntity : class
        {
            var changes = new List<ChangeRecord>();
            var entityName = typeof(TEntity).Name;

            try
            {
                if (!typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
                {
                    _logger.LogWarning("{EntityName} does not implement ISoftDeletable, cannot track deletions", entityName);
                    return changes;
                }

                using var context = _connectionManager.GetLocalContext();
                var dbSet = context.Set<TEntity>();
                
                // Query for soft-deleted entities
                var query = dbSet.IgnoreQueryFilters()
                    .Cast<ISoftDeletable>()
                    .Where(e => e.IsDeleted);

                // Apply timestamp filter if provided
                if (since.HasValue)
                {
                    query = query.Where(e => e.DeletedAt > since);
                }

                var entities = await query.Cast<TEntity>().ToListAsync();

                foreach (var entity in entities)
                {
                    var softDeletable = entity as ISoftDeletable;
                    var entityId = GetEntityId(entity);
                    
                    changes.Add(new ChangeRecord
                    {
                        EntityName = entityName,
                        EntityId = entityId,
                        ChangeType = ChangeType.Deleted,
                        ChangedAt = softDeletable?.DeletedAt ?? DateTime.UtcNow,
                        Entity = entity,
                        DataHash = ComputeEntityHash(entity)
                    });
                }

                _logger.LogInformation("Found {Count} deleted {EntityName} entities since {Since}", 
                    changes.Count, entityName, since);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deleted entities for {EntityName}", entityName);
                throw;
            }

            return changes;
        }

        public async Task MarkAsSyncedAsync(string entityName, string entityId, DateTime syncTime)
        {
            try
            {
                using var context = _connectionManager.GetLocalContext();
                
                // This would update the SyncMetadata table
                // Implementation depends on having SyncMetadata DbSet in context
                _logger.LogDebug("Marked {EntityName}:{EntityId} as synced at {SyncTime}", 
                    entityName, entityId, syncTime);
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking entity as synced: {EntityName}:{EntityId}", 
                    entityName, entityId);
                throw;
            }
        }

        public async Task<DateTime?> GetLastSyncTimeAsync(string entityName)
        {
            try
            {
                using var context = _connectionManager.GetLocalContext();
                
                // This would query the SyncMetadata table
                // For now, return null to indicate no previous sync
                await Task.CompletedTask;
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting last sync time for {EntityName}", entityName);
                return null;
            }
        }

        public string ComputeEntityHash(object entity)
        {
            try
            {
                // Serialize entity to JSON and compute SHA256 hash
                var json = JsonSerializer.Serialize(entity);
                using var sha256 = SHA256.Create();
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
                return Convert.ToBase64String(hashBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error computing entity hash");
                return string.Empty;
            }
        }

        private string GetEntityId(object entity)
        {
            // Use reflection to get the primary key value
            var type = entity.GetType();
            
            // Common primary key property names
            var pkProperties = new[] { "Id", $"{type.Name}Id", "ProductId", "SalesOrderId", "PurchaseOrderId" };
            
            foreach (var propName in pkProperties)
            {
                var prop = type.GetProperty(propName);
                if (prop != null)
                {
                    var value = prop.GetValue(entity);
                    return value?.ToString() ?? string.Empty;
                }
            }

            _logger.LogWarning("Could not find primary key for entity type {EntityType}", type.Name);
            return string.Empty;
        }
    }
}
