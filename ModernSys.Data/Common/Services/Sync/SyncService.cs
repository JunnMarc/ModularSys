using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities.Finance;
using ModularSys.Data.Common.Entities.Inventory;
using ModularSys.Data.Common.Interfaces;
using ModularSys.Data.Common.Interfaces.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModularSys.Data.Common.Services.Sync
{
    /// <summary>
    /// Main synchronization service that orchestrates bidirectional sync between local and cloud databases
    /// </summary>
    public class SyncService : ISyncService
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IChangeTracker _changeTracker;
        private readonly IConflictResolver _conflictResolver;
        private readonly ILogger<SyncService> _logger;

        // List of entity types to sync (in priority order)
        private readonly List<Type> _syncableEntities = new()
        {
            // Core entities first
            typeof(Category),
            typeof(Product),
            
            // Transactional entities
            typeof(SalesOrder),
            typeof(SalesOrderLine),
            typeof(PurchaseOrder),
            typeof(PurchaseOrderLine),
            typeof(InventoryTransaction),
            
            // Finance
            typeof(RevenueTransaction)
        };

        public SyncService(
            IConnectionManager connectionManager,
            IChangeTracker changeTracker,
            IConflictResolver conflictResolver,
            ILogger<SyncService> logger)
        {
            _connectionManager = connectionManager;
            _changeTracker = changeTracker;
            _conflictResolver = conflictResolver;
            _logger = logger;
        }

        public async Task<SyncResult> SyncAllAsync(CancellationToken cancellationToken = default)
        {
            var sessionId = Guid.NewGuid();
            var result = new SyncResult
            {
                SessionId = sessionId,
                StartedAt = DateTime.UtcNow
            };

            try
            {
                _logger.LogInformation("Starting full sync session {SessionId}", sessionId);

                // Check if cloud is reachable
                if (!await IsOnlineAsync())
                {
                    result.Success = false;
                    result.ErrorMessage = "Cloud database is not reachable. Operating in offline mode.";
                    _logger.LogWarning("Sync aborted: Cloud not reachable");
                    return result;
                }

                // Sync each entity type
                foreach (var entityType in _syncableEntities)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        result.ErrorMessage = "Sync cancelled by user";
                        break;
                    }

                    try
                    {
                        var syncMethod = typeof(SyncService)
                            .GetMethod(nameof(SyncEntityInternalAsync), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.MakeGenericMethod(entityType);

                        if (syncMethod != null)
                        {
                            var entityResult = await (Task<EntitySyncResult>)syncMethod.Invoke(this, new object[] { cancellationToken })!;
                            
                            result.EntitiesSynced += entityResult.Synced;
                            result.EntitiesFailed += entityResult.Failed;
                            result.ConflictsDetected += entityResult.ConflictsDetected;
                            result.ConflictsResolved += entityResult.ConflictsResolved;
                            result.EntityCounts[entityType.Name] = entityResult.Synced;

                            if (entityResult.Errors.Any())
                            {
                                result.Errors.AddRange(entityResult.Errors);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error syncing entity type {EntityType}", entityType.Name);
                        result.Errors.Add($"{entityType.Name}: {ex.Message}");
                        result.EntitiesFailed++;
                    }
                }

                result.Success = result.EntitiesFailed == 0;
                result.CompletedAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "Sync session {SessionId} completed. Synced: {Synced}, Failed: {Failed}, Conflicts: {Conflicts}",
                    sessionId, result.EntitiesSynced, result.EntitiesFailed, result.ConflictsDetected);
                
                // Log sync result to database
                await LogSyncResultAsync(result, "Full");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error during sync session {SessionId}", sessionId);
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.CompletedAt = DateTime.UtcNow;
                
                // Log failed sync to database
                await LogSyncResultAsync(result, "Full");
            }

            return result;
        }

        public async Task<SyncResult> SyncIncrementalAsync(CancellationToken cancellationToken = default)
        {
            var sessionId = Guid.NewGuid();
            var result = new SyncResult
            {
                SessionId = sessionId,
                StartedAt = DateTime.UtcNow
            };

            try
            {
                _logger.LogInformation("Starting incremental sync session {SessionId}", sessionId);

                if (!await IsOnlineAsync())
                {
                    result.Success = false;
                    result.ErrorMessage = "Cloud database is not reachable";
                    return result;
                }

                var lastSyncTime = await GetLastSyncTimeAsync();
                _logger.LogInformation("Last sync was at {LastSyncTime}", lastSyncTime);

                // Sync only changed entities since last sync
                foreach (var entityType in _syncableEntities)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        result.ErrorMessage = "Sync cancelled";
                        break;
                    }

                    try
                    {
                        var syncMethod = typeof(SyncService)
                            .GetMethod(nameof(SyncEntityIncrementalInternalAsync), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.MakeGenericMethod(entityType);

                        if (syncMethod != null)
                        {
                            var entityResult = await (Task<EntitySyncResult>)syncMethod.Invoke(this, new object[] { lastSyncTime, cancellationToken })!;
                            
                            result.EntitiesSynced += entityResult.Synced;
                            result.EntitiesFailed += entityResult.Failed;
                            result.ConflictsDetected += entityResult.ConflictsDetected;
                            result.ConflictsResolved += entityResult.ConflictsResolved;
                            result.EntityCounts[entityType.Name] = entityResult.Synced;

                            if (entityResult.Errors.Any())
                            {
                                result.Errors.AddRange(entityResult.Errors);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error syncing entity type {EntityType}", entityType.Name);
                        result.Errors.Add($"{entityType.Name}: {ex.Message}");
                        result.EntitiesFailed++;
                    }
                }

                result.Success = result.EntitiesFailed == 0;
                result.CompletedAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "Incremental sync session {SessionId} completed. Synced: {Synced}, Failed: {Failed}",
                    sessionId, result.EntitiesSynced, result.EntitiesFailed);
                
                // Log sync result to database
                await LogSyncResultAsync(result, "Incremental");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error during incremental sync {SessionId}", sessionId);
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.CompletedAt = DateTime.UtcNow;
                
                // Log failed sync to database
                await LogSyncResultAsync(result, "Incremental");
            }

            return result;
        }

        public async Task<SyncResult> SyncEntityAsync<TEntity>(CancellationToken cancellationToken = default) where TEntity : class
        {
            var sessionId = Guid.NewGuid();
            var result = new SyncResult
            {
                SessionId = sessionId,
                StartedAt = DateTime.UtcNow
            };

            try
            {
                if (!await IsOnlineAsync())
                {
                    result.Success = false;
                    result.ErrorMessage = "Cloud database is not reachable";
                    return result;
                }

                var entityResult = await SyncEntityInternalAsync<TEntity>(cancellationToken);
                
                result.EntitiesSynced = entityResult.Synced;
                result.EntitiesFailed = entityResult.Failed;
                result.ConflictsDetected = entityResult.ConflictsDetected;
                result.ConflictsResolved = entityResult.ConflictsResolved;
                result.EntityCounts[typeof(TEntity).Name] = entityResult.Synced;
                result.Errors = entityResult.Errors;
                result.Success = entityResult.Failed == 0;
                result.CompletedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing entity {EntityType}", typeof(TEntity).Name);
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.CompletedAt = DateTime.UtcNow;
            }

            return result;
        }

        public async Task<SyncResult> PushToCloudAsync(CancellationToken cancellationToken = default)
        {
            var sessionId = Guid.NewGuid();
            var result = new SyncResult
            {
                SessionId = sessionId,
                StartedAt = DateTime.UtcNow
            };

            try
            {
                _logger.LogInformation("Pushing local changes to cloud {SessionId}", sessionId);

                if (!await IsOnlineAsync())
                {
                    result.Success = false;
                    result.ErrorMessage = "Cloud database is not reachable";
                    return result;
                }

                foreach (var entityType in _syncableEntities)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    try
                    {
                        var pushMethod = typeof(SyncService)
                            .GetMethod(nameof(PushEntityToCloudAsync), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.MakeGenericMethod(entityType);

                        if (pushMethod != null)
                        {
                            var count = await (Task<int>)pushMethod.Invoke(this, new object[] { cancellationToken })!;
                            result.EntitiesSynced += count;
                            result.EntityCounts[entityType.Name] = count;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error pushing {EntityType} to cloud", entityType.Name);
                        result.Errors.Add($"{entityType.Name}: {ex.Message}");
                        result.EntitiesFailed++;
                    }
                }

                result.Success = result.EntitiesFailed == 0;
                result.CompletedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error pushing to cloud");
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.CompletedAt = DateTime.UtcNow;
            }

            return result;
        }

        public async Task<SyncResult> PullFromCloudAsync(CancellationToken cancellationToken = default)
        {
            var sessionId = Guid.NewGuid();
            var result = new SyncResult
            {
                SessionId = sessionId,
                StartedAt = DateTime.UtcNow
            };

            try
            {
                _logger.LogInformation("Pulling cloud changes to local {SessionId}", sessionId);

                if (!await IsOnlineAsync())
                {
                    result.Success = false;
                    result.ErrorMessage = "Cloud database is not reachable";
                    return result;
                }

                foreach (var entityType in _syncableEntities)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    try
                    {
                        var pullMethod = typeof(SyncService)
                            .GetMethod(nameof(PullEntityFromCloudAsync), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.MakeGenericMethod(entityType);

                        if (pullMethod != null)
                        {
                            var count = await (Task<int>)pullMethod.Invoke(this, new object[] { cancellationToken })!;
                            result.EntitiesSynced += count;
                            result.EntityCounts[entityType.Name] = count;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error pulling {EntityType} from cloud", entityType.Name);
                        result.Errors.Add($"{entityType.Name}: {ex.Message}");
                        result.EntitiesFailed++;
                    }
                }

                result.Success = result.EntitiesFailed == 0;
                result.CompletedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error pulling from cloud");
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.CompletedAt = DateTime.UtcNow;
            }

            return result;
        }

        public async Task<bool> IsOnlineAsync()
        {
            return await _connectionManager.IsCloudReachableAsync();
        }

        public async Task<DateTime?> GetLastSyncTimeAsync()
        {
            try
            {
                var localContext = _connectionManager.GetLocalContext();
                var lastSync = await localContext.Set<Entities.Sync.SyncLog>()
                    .Where(s => s.Status == "Completed" && s.CompletedAt.HasValue)
                    .OrderByDescending(s => s.CompletedAt)
                    .FirstOrDefaultAsync();
                
                return lastSync?.CompletedAt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting last sync time");
                return null;
            }
        }

        public async Task<int> GetPendingSyncCountAsync()
        {
            // Count entities that have changed since last sync but haven't been synced yet
            var count = 0;
            var lastSync = await GetLastSyncTimeAsync();

            try
            {
                var localContext = _connectionManager.GetLocalContext();
                
                foreach (var entityType in _syncableEntities)
                {
                    try
                    {
                        var getChangesMethod = typeof(IChangeTracker)
                            .GetMethod(nameof(IChangeTracker.GetChangedEntitiesAsync))
                            ?.MakeGenericMethod(entityType);

                        if (getChangesMethod != null)
                        {
                            var changes = await (Task<List<ChangeRecord>>)getChangesMethod.Invoke(_changeTracker, new object[] { lastSync })!;
                            
                            // Filter out entities that are already synced (have recent SyncMetadata)
                            foreach (var change in changes)
                            {
                                var metadata = await localContext.Set<Entities.Sync.SyncMetadata>()
                                    .FirstOrDefaultAsync(m => m.EntityName == change.EntityName && 
                                                            m.EntityId == change.EntityId &&
                                                            m.Status == "Completed");
                                
                                // Only count if not synced OR entity changed after last sync
                                if (metadata == null || metadata.LastSyncedAt < change.ChangedAt)
                                {
                                    count++;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error counting pending changes for {EntityType}", entityType.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPendingSyncCountAsync");
            }

            return count;
        }

        public async Task ResolveConflictAsync(int syncMetadataId, ConflictResolutionStrategy strategy, CancellationToken cancellationToken = default)
        {
            // Implementation would load the conflict from SyncMetadata and resolve it
            await Task.CompletedTask;
            _logger.LogInformation("Manually resolved conflict {MetadataId} with strategy {Strategy}", 
                syncMetadataId, strategy);
        }

        #region Private Helper Methods

        private async Task<EntitySyncResult> SyncEntityInternalAsync<TEntity>(CancellationToken cancellationToken) where TEntity : class
        {
            var result = new EntitySyncResult();
            var entityName = typeof(TEntity).Name;

            try
            {
                _logger.LogInformation("Syncing entity type: {EntityName}", entityName);

                // Get changes from both local and cloud
                var localChanges = await _changeTracker.GetChangedEntitiesAsync<TEntity>();
                var cloudChanges = await GetCloudChangesAsync<TEntity>();

                // Push local changes to cloud
                foreach (var localChange in localChanges)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    try
                    {
                        await PushEntityChangeToCloudAsync(localChange);
                        result.Synced++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error pushing {EntityName} to cloud", entityName);
                        result.Failed++;
                        var innerMsg = ex.InnerException?.Message ?? ex.Message;
                        result.Errors.Add($"Push failed [{entityName}]: {innerMsg}");
                    }
                }

                // Pull cloud changes to local
                foreach (var cloudChange in cloudChanges)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    try
                    {
                        await PullEntityChangeToLocalAsync(cloudChange);
                        result.Synced++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error pulling {EntityName} from cloud", entityName);
                        result.Failed++;
                        var innerMsg = ex.InnerException?.Message ?? ex.Message;
                        result.Errors.Add($"Pull failed [{entityName}]: {innerMsg}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SyncEntityInternalAsync for {EntityName}", entityName);
                result.Failed++;
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        private async Task<EntitySyncResult> SyncEntityIncrementalInternalAsync<TEntity>(DateTime? since, CancellationToken cancellationToken) where TEntity : class
        {
            var result = new EntitySyncResult();
            var entityName = typeof(TEntity).Name;

            try
            {
                _logger.LogInformation("Incremental sync for {EntityName} since {Since}", entityName, since);

                // Get only changed entities since last sync
                var localChanges = await _changeTracker.GetChangedEntitiesAsync<TEntity>(since);
                var cloudChanges = await GetCloudChangesAsync<TEntity>(since);

                // Sync changes
                foreach (var localChange in localChanges)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    try
                    {
                        await PushEntityChangeToCloudAsync(localChange);
                        result.Synced++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in incremental push for {EntityName}", entityName);
                        result.Failed++;
                    }
                }

                foreach (var cloudChange in cloudChanges)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    try
                    {
                        await PullEntityChangeToLocalAsync(cloudChange);
                        result.Synced++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in incremental pull for {EntityName}", entityName);
                        result.Failed++;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in incremental sync for {EntityName}", entityName);
                result.Failed++;
            }

            return result;
        }

        private async Task<int> PushEntityToCloudAsync<TEntity>(CancellationToken cancellationToken) where TEntity : class
        {
            var changes = await _changeTracker.GetChangedEntitiesAsync<TEntity>();
            var count = 0;

            foreach (var change in changes)
            {
                if (cancellationToken.IsCancellationRequested) break;
                await PushEntityChangeToCloudAsync(change);
                count++;
            }

            return count;
        }

        private async Task<int> PullEntityFromCloudAsync<TEntity>(CancellationToken cancellationToken) where TEntity : class
        {
            var changes = await GetCloudChangesAsync<TEntity>();
            var count = 0;

            foreach (var change in changes)
            {
                if (cancellationToken.IsCancellationRequested) break;
                await PullEntityChangeToLocalAsync(change);
                count++;
            }

            return count;
        }

        private async Task PushEntityChangeToCloudAsync(ChangeRecord change)
        {
            using var cloudContext = _connectionManager.GetCloudContext();
            using var localContext = _connectionManager.GetLocalContext();

            // Check if entity exists in cloud
            var existingEntity = await FindEntityByIdAsync(cloudContext, change.Entity, change.EntityId);

            if (change.ChangeType == ChangeType.Deleted)
            {
                if (existingEntity != null)
                {
                    cloudContext.Remove(existingEntity);
                }
            }
            else if (existingEntity != null)
            {
                // Update existing
                cloudContext.Entry(existingEntity).CurrentValues.SetValues(change.Entity);
            }
            else
            {
                // Insert new - use IDENTITY_INSERT for entities with explicit IDs
                var entityType = change.Entity.GetType();
                var tableName = cloudContext.Model.FindEntityType(entityType)?.GetTableName();
                
                if (!string.IsNullOrEmpty(tableName))
                {
                    var strategy = cloudContext.Database.CreateExecutionStrategy();
                    await strategy.ExecuteAsync(async () =>
                    {
                        using var transaction = await cloudContext.Database.BeginTransactionAsync();
                        try
                        {
                            // Enable IDENTITY_INSERT
                            await cloudContext.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{tableName}] ON");
                            
                            cloudContext.Add(change.Entity);
                            await cloudContext.SaveChangesAsync();
                            
                            // Disable IDENTITY_INSERT
                            await cloudContext.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{tableName}] OFF");
                            
                            await transaction.CommitAsync();
                        }
                        catch
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    });
                }
                else
                {
                    cloudContext.Add(change.Entity);
                    await cloudContext.SaveChangesAsync();
                }
                return;
            }

            await cloudContext.SaveChangesAsync();
            
            // Update SyncMetadata to track this entity was synced
            await UpdateSyncMetadataAsync(localContext, change);
        }

        private async Task PullEntityChangeToLocalAsync(ChangeRecord change)
        {
            using var localContext = _connectionManager.GetLocalContext();
            
            var existingEntity = await FindEntityByIdAsync(localContext, change.Entity, change.EntityId);

            if (change.ChangeType == ChangeType.Deleted)
            {
                if (existingEntity != null)
                {
                    localContext.Remove(existingEntity);
                }
            }
            else if (existingEntity != null)
            {
                // Check for conflicts
                if (_conflictResolver.HasConflict(existingEntity, change.Entity))
                {
                    var resolution = await _conflictResolver.ResolveConflictAsync(
                        existingEntity, 
                        change.Entity, 
                        ConflictResolutionStrategy.LastWriteWins);

                    if (resolution.Resolved && resolution.ResolvedEntity != null)
                    {
                        localContext.Entry(existingEntity).CurrentValues.SetValues(resolution.ResolvedEntity);
                    }
                }
                else
                {
                    localContext.Entry(existingEntity).CurrentValues.SetValues(change.Entity);
                }
            }
            else
            {
                // Insert new - use IDENTITY_INSERT for entities with explicit IDs
                var entityType = change.Entity.GetType();
                var tableName = localContext.Model.FindEntityType(entityType)?.GetTableName();
                
                if (!string.IsNullOrEmpty(tableName))
                {
                    var strategy = localContext.Database.CreateExecutionStrategy();
                    await strategy.ExecuteAsync(async () =>
                    {
                        using var transaction = await localContext.Database.BeginTransactionAsync();
                        try
                        {
                            // Enable IDENTITY_INSERT
                            await localContext.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{tableName}] ON");
                            
                            localContext.Add(change.Entity);
                            await localContext.SaveChangesAsync();
                            
                            // Disable IDENTITY_INSERT
                            await localContext.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{tableName}] OFF");
                            
                            await transaction.CommitAsync();
                        }
                        catch
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    });
                }
                else
                {
                    localContext.Add(change.Entity);
                    await localContext.SaveChangesAsync();
                }
                return;
            }

            await localContext.SaveChangesAsync();
            
            // Update SyncMetadata to track this entity was synced
            await UpdateSyncMetadataAsync(localContext, change);
        }

        private async Task UpdateSyncMetadataAsync(ModularSysDbContext context, ChangeRecord change)
        {
            try
            {
                var metadata = await context.Set<Entities.Sync.SyncMetadata>()
                    .FirstOrDefaultAsync(m => m.EntityName == change.EntityName && m.EntityId == change.EntityId);

                if (metadata == null)
                {
                    metadata = new Entities.Sync.SyncMetadata
                    {
                        EntityName = change.EntityName,
                        EntityId = change.EntityId,
                        LastSyncedAt = DateTime.UtcNow,
                        DataHash = change.DataHash,
                        SyncDirection = "Bidirectional",
                        Status = "Completed",
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Set<Entities.Sync.SyncMetadata>().Add(metadata);
                }
                else
                {
                    metadata.LastSyncedAt = DateTime.UtcNow;
                    metadata.DataHash = change.DataHash;
                    metadata.Status = "Completed";
                    metadata.UpdatedAt = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update SyncMetadata for {EntityName} {EntityId}", change.EntityName, change.EntityId);
                // Don't throw - metadata update failure shouldn't break sync
            }
        }

        private async Task<List<ChangeRecord>> GetCloudChangesAsync<TEntity>(DateTime? since = null) where TEntity : class
        {
            var changes = new List<ChangeRecord>();

            try
            {
                using var cloudContext = _connectionManager.GetCloudContext();
                var dbSet = cloudContext.Set<TEntity>();
                
                IQueryable<TEntity> query = dbSet;

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
                        EntityName = typeof(TEntity).Name,
                        EntityId = entityId,
                        ChangeType = changeType,
                        ChangedAt = changedAt,
                        Entity = entity,
                        DataHash = _changeTracker.ComputeEntityHash(entity)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cloud changes for {EntityType}", typeof(TEntity).Name);
            }

            return changes;
        }

        private async Task<object?> FindEntityByIdAsync(ModularSysDbContext context, object entity, string entityId)
        {
            var entityType = entity.GetType();
            
            // Use reflection to call the generic Find method on the specific DbContext
            var setMethod = typeof(ModularSysDbContext).GetMethod(nameof(ModularSysDbContext.Set), Type.EmptyTypes);
            if (setMethod == null) return null;
            
            var genericSetMethod = setMethod.MakeGenericMethod(entityType);
            var dbSet = genericSetMethod.Invoke(context, null);
            
            if (dbSet == null) return null;

            var findMethod = dbSet.GetType().GetMethod("FindAsync", new[] { typeof(object[]) });
            if (findMethod == null) return null;

            // FindAsync returns ValueTask, not Task
            var valueTask = findMethod.Invoke(dbSet, new object[] { new object[] { int.Parse(entityId) } });
            if (valueTask == null) return null;

            // Get AsTask() method to convert ValueTask to Task
            var asTaskMethod = valueTask.GetType().GetMethod("AsTask", Type.EmptyTypes);
            if (asTaskMethod == null) return null;

            var task = (Task)asTaskMethod.Invoke(valueTask, null)!;
            await task.ConfigureAwait(false);

            var resultProperty = task.GetType().GetProperty("Result");
            return resultProperty?.GetValue(task);
        }

        private string GetEntityId(object entity)
        {
            var type = entity.GetType();
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

            return string.Empty;
        }

        #endregion

        #region Sync Logging

        /// <summary>
        /// Logs sync result to SyncLogs table in local database
        /// </summary>
        private async Task LogSyncResultAsync(SyncResult result, string syncType)
        {
            try
            {
                var localContext = _connectionManager.GetLocalContext();
                
                var syncLog = new Entities.Sync.SyncLog
                {
                    SyncSessionId = result.SessionId,
                    StartedAt = result.StartedAt,
                    CompletedAt = result.CompletedAt,
                    SyncType = syncType,
                    Direction = "Bidirectional",
                    Status = result.Success ? "Completed" : "Failed",
                    EntitiesSynced = result.EntitiesSynced,
                    EntitiesFailed = result.EntitiesFailed,
                    ConflictsDetected = result.ConflictsDetected,
                    ConflictsResolved = result.ConflictsResolved,
                    ErrorMessage = result.ErrorMessage,
                    Details = result.Errors.Any() ? string.Join("; ", result.Errors) : null
                };

                localContext.Set<Entities.Sync.SyncLog>().Add(syncLog);
                await localContext.SaveChangesAsync();
                
                _logger.LogInformation("Sync log saved for session {SessionId}", result.SessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log sync result for session {SessionId}", result.SessionId);
                // Don't throw - logging failure shouldn't break the sync
            }
        }

        #endregion

        private class EntitySyncResult
        {
            public int Synced { get; set; }
            public int Failed { get; set; }
            public int ConflictsDetected { get; set; }
            public int ConflictsResolved { get; set; }
            public List<string> Errors { get; set; } = new();
        }
    }
}
