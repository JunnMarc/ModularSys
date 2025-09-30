using System.Threading.Tasks;

namespace ModularSys.Data.Common.Interfaces.Sync
{
    /// <summary>
    /// Resolves conflicts between local and cloud data
    /// </summary>
    public interface IConflictResolver
    {
        /// <summary>
        /// Resolves a conflict between local and cloud entities
        /// </summary>
        Task<ConflictResolution> ResolveConflictAsync<TEntity>(
            TEntity localEntity, 
            TEntity cloudEntity, 
            ConflictResolutionStrategy strategy) where TEntity : class;
        
        /// <summary>
        /// Detects if there's a conflict between two entities
        /// </summary>
        bool HasConflict<TEntity>(TEntity localEntity, TEntity cloudEntity) where TEntity : class;
    }
    
    /// <summary>
    /// Result of conflict resolution
    /// </summary>
    public class ConflictResolution
    {
        public bool Resolved { get; set; }
        public object? ResolvedEntity { get; set; }
        public ConflictResolutionStrategy StrategyUsed { get; set; }
        public string? Message { get; set; }
    }
}
