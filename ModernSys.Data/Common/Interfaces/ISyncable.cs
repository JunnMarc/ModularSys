namespace ModularSys.Data.Common.Interfaces;

/// <summary>
/// Marker interface for entities that support synchronization between local and cloud databases.
/// Entities implementing ISoftDeletable automatically support sync via their audit fields.
/// </summary>
public interface ISyncable
{
    /// <summary>
    /// Unique identifier for the entity (used for matching records across databases)
    /// </summary>
    object GetPrimaryKey();
    
    /// <summary>
    /// Last modification timestamp (used for conflict resolution)
    /// </summary>
    DateTime? GetLastModified();
    
    /// <summary>
    /// Indicates if the entity has been deleted (soft delete)
    /// </summary>
    bool IsDeleted { get; }
}
