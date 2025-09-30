using Microsoft.Extensions.Logging;
using ModularSys.Data.Common.Interfaces;
using ModularSys.Data.Common.Interfaces.Sync;
using System;
using System.Threading.Tasks;

namespace ModularSys.Data.Common.Services.Sync
{
    /// <summary>
    /// Resolves conflicts between local and cloud data using various strategies
    /// </summary>
    public class ConflictResolver : IConflictResolver
    {
        private readonly ILogger<ConflictResolver> _logger;

        public ConflictResolver(ILogger<ConflictResolver> logger)
        {
            _logger = logger;
        }

        public async Task<ConflictResolution> ResolveConflictAsync<TEntity>(
            TEntity localEntity, 
            TEntity cloudEntity, 
            ConflictResolutionStrategy strategy) where TEntity : class
        {
            try
            {
                _logger.LogInformation("Resolving conflict for {EntityType} using strategy {Strategy}", 
                    typeof(TEntity).Name, strategy);

                var resolution = new ConflictResolution
                {
                    StrategyUsed = strategy
                };

                switch (strategy)
                {
                    case ConflictResolutionStrategy.LastWriteWins:
                        resolution = await ResolveLastWriteWinsAsync(localEntity, cloudEntity);
                        break;

                    case ConflictResolutionStrategy.FirstWriteWins:
                        resolution = await ResolveFirstWriteWinsAsync(localEntity, cloudEntity);
                        break;

                    case ConflictResolutionStrategy.KeepLocal:
                        resolution.Resolved = true;
                        resolution.ResolvedEntity = localEntity;
                        resolution.Message = "Kept local version";
                        break;

                    case ConflictResolutionStrategy.KeepCloud:
                        resolution.Resolved = true;
                        resolution.ResolvedEntity = cloudEntity;
                        resolution.Message = "Kept cloud version";
                        break;

                    case ConflictResolutionStrategy.Manual:
                        resolution.Resolved = false;
                        resolution.Message = "Manual resolution required";
                        break;

                    default:
                        resolution.Resolved = false;
                        resolution.Message = $"Unknown strategy: {strategy}";
                        break;
                }

                return resolution;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving conflict for {EntityType}", typeof(TEntity).Name);
                return new ConflictResolution
                {
                    Resolved = false,
                    Message = $"Error: {ex.Message}",
                    StrategyUsed = strategy
                };
            }
        }

        public bool HasConflict<TEntity>(TEntity localEntity, TEntity cloudEntity) where TEntity : class
        {
            if (localEntity == null || cloudEntity == null)
            {
                return false;
            }

            // Check if both entities implement ISoftDeletable
            if (localEntity is ISoftDeletable localSoft && cloudEntity is ISoftDeletable cloudSoft)
            {
                // Conflict exists if both have been modified and timestamps differ
                if (localSoft.UpdatedAt.HasValue && cloudSoft.UpdatedAt.HasValue)
                {
                    return localSoft.UpdatedAt != cloudSoft.UpdatedAt;
                }

                // Conflict if one is deleted and the other is modified
                if (localSoft.IsDeleted != cloudSoft.IsDeleted)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<ConflictResolution> ResolveLastWriteWinsAsync<TEntity>(
            TEntity localEntity, 
            TEntity cloudEntity) where TEntity : class
        {
            var resolution = new ConflictResolution
            {
                StrategyUsed = ConflictResolutionStrategy.LastWriteWins
            };

            if (localEntity is ISoftDeletable localSoft && cloudEntity is ISoftDeletable cloudSoft)
            {
                DateTime localTime = localSoft.UpdatedAt ?? localSoft.CreatedAt ?? DateTime.MinValue;
                DateTime cloudTime = cloudSoft.UpdatedAt ?? cloudSoft.CreatedAt ?? DateTime.MinValue;

                if (localTime > cloudTime)
                {
                    resolution.Resolved = true;
                    resolution.ResolvedEntity = localEntity;
                    resolution.Message = $"Local version is newer ({localTime:yyyy-MM-dd HH:mm:ss} > {cloudTime:yyyy-MM-dd HH:mm:ss})";
                    _logger.LogInformation("Resolved conflict: Local wins (newer timestamp)");
                }
                else if (cloudTime > localTime)
                {
                    resolution.Resolved = true;
                    resolution.ResolvedEntity = cloudEntity;
                    resolution.Message = $"Cloud version is newer ({cloudTime:yyyy-MM-dd HH:mm:ss} > {localTime:yyyy-MM-dd HH:mm:ss})";
                    _logger.LogInformation("Resolved conflict: Cloud wins (newer timestamp)");
                }
                else
                {
                    // Same timestamp - prefer local
                    resolution.Resolved = true;
                    resolution.ResolvedEntity = localEntity;
                    resolution.Message = "Same timestamp, kept local version";
                    _logger.LogInformation("Resolved conflict: Same timestamp, keeping local");
                }
            }
            else
            {
                // If entities don't implement ISoftDeletable, default to local
                resolution.Resolved = true;
                resolution.ResolvedEntity = localEntity;
                resolution.Message = "Entity does not support timestamp comparison, kept local version";
            }

            await Task.CompletedTask;
            return resolution;
        }

        private async Task<ConflictResolution> ResolveFirstWriteWinsAsync<TEntity>(
            TEntity localEntity, 
            TEntity cloudEntity) where TEntity : class
        {
            var resolution = new ConflictResolution
            {
                StrategyUsed = ConflictResolutionStrategy.FirstWriteWins
            };

            if (localEntity is ISoftDeletable localSoft && cloudEntity is ISoftDeletable cloudSoft)
            {
                DateTime localTime = localSoft.CreatedAt ?? DateTime.MaxValue;
                DateTime cloudTime = cloudSoft.CreatedAt ?? DateTime.MaxValue;

                if (localTime < cloudTime)
                {
                    resolution.Resolved = true;
                    resolution.ResolvedEntity = localEntity;
                    resolution.Message = $"Local version was created first ({localTime:yyyy-MM-dd HH:mm:ss})";
                }
                else
                {
                    resolution.Resolved = true;
                    resolution.ResolvedEntity = cloudEntity;
                    resolution.Message = $"Cloud version was created first ({cloudTime:yyyy-MM-dd HH:mm:ss})";
                }
            }
            else
            {
                // Default to local
                resolution.Resolved = true;
                resolution.ResolvedEntity = localEntity;
                resolution.Message = "Entity does not support timestamp comparison, kept local version";
            }

            await Task.CompletedTask;
            return resolution;
        }
    }
}
