using ModularSys.Data.Common.Entities;

namespace ModularSys.Core.Interfaces
{
    public interface IAuditService
    {
        // Log specific actions
        Task LogAsync(string action, string entityName, string? entityId = null, string? description = null, string? category = null);
        Task LogAsync(AuditLog auditLog);
        
        // Log CRUD operations
        Task LogCreateAsync(string entityName, string entityId, object newValues, string? description = null);
        Task LogUpdateAsync(string entityName, string entityId, object? oldValues, object newValues, string? description = null);
        Task LogDeleteAsync(string entityName, string entityId, object? oldValues, string? description = null);
        
        // Log user actions
        Task LogLoginAsync(string username, bool success, string? errorMessage = null, string? ipAddress = null);
        Task LogLogoutAsync(string username, string? ipAddress = null);
        Task LogPasswordChangeAsync(string username, bool success, string? errorMessage = null);
        Task LogFailedLoginAsync(string username, string? errorMessage = null, string? ipAddress = null);
        
        // Query audit logs
        Task<List<AuditLog>> GetAllAsync();
        Task<List<AuditLog>> GetByEntityAsync(string entityName, string? entityId = null);
        Task<List<AuditLog>> GetByUserAsync(string username);
        Task<List<AuditLog>> GetByActionAsync(string action);
        Task<List<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<AuditLog>> GetByCategoryAsync(string category);
        Task<(List<AuditLog> Logs, int TotalCount)> GetPagedAsync(int page, int pageSize, string? searchTerm = null, string? entityName = null, string? action = null, string? username = null);
        
        // Statistics
        Task<int> GetTotalCountAsync();
        Task<Dictionary<string, int>> GetActionStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<string, int>> GetEntityStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}
