using Microsoft.EntityFrameworkCore;
using ModularSys.Core.Interfaces;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities;
using System.Text.Json;

namespace ModularSys.Core.Services
{
    public class AuditService : IAuditService
    {
        private readonly IDbContextFactory<ModularSysDbContext> _contextFactory;
        private readonly IAuthService _authService;

        public AuditService(IDbContextFactory<ModularSysDbContext> contextFactory, IAuthService authService)
        {
            _contextFactory = contextFactory;
            _authService = authService;
        }

        public async Task LogAsync(string action, string entityName, string? entityId = null, string? description = null, string? category = null)
        {
            var auditLog = new AuditLog
            {
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Description = description,
                Category = category ?? "General",
                Username = _authService.CurrentUser,
                Timestamp = DateTime.UtcNow,
                Success = true
            };

            await LogAsync(auditLog);
        }

        public async Task LogAsync(AuditLog auditLog)
        {
            try
            {
                await using var db = _contextFactory.CreateDbContext();
                
                // Set username if not already set
                auditLog.Username ??= _authService.CurrentUser ?? "Anonymous";
                
                // Set timestamp if not already set
                if (auditLog.Timestamp == default)
                    auditLog.Timestamp = DateTime.UtcNow;

                db.AuditLogs.Add(auditLog);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log to console/file if database logging fails
                Console.WriteLine($"Failed to write audit log: {ex.Message}");
            }
        }

        public async Task LogCreateAsync(string entityName, string entityId, object newValues, string? description = null)
        {
            var auditLog = new AuditLog
            {
                Action = "Create",
                EntityName = entityName,
                EntityId = entityId,
                NewValues = JsonSerializer.Serialize(newValues),
                Description = description ?? $"Created {entityName} with ID {entityId}",
                Category = "Data",
                Severity = "Info",
                Username = _authService.CurrentUser,
                Timestamp = DateTime.UtcNow,
                Success = true
            };

            await LogAsync(auditLog);
        }

        public async Task LogUpdateAsync(string entityName, string entityId, object? oldValues, object newValues, string? description = null)
        {
            var auditLog = new AuditLog
            {
                Action = "Update",
                EntityName = entityName,
                EntityId = entityId,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = JsonSerializer.Serialize(newValues),
                Description = description ?? $"Updated {entityName} with ID {entityId}",
                Category = "Data",
                Severity = "Info",
                Username = _authService.CurrentUser,
                Timestamp = DateTime.UtcNow,
                Success = true
            };

            await LogAsync(auditLog);
        }

        public async Task LogDeleteAsync(string entityName, string entityId, object? oldValues, string? description = null)
        {
            var auditLog = new AuditLog
            {
                Action = "Delete",
                EntityName = entityName,
                EntityId = entityId,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                Description = description ?? $"Deleted {entityName} with ID {entityId}",
                Category = "Data",
                Severity = "Warning",
                Username = _authService.CurrentUser,
                Timestamp = DateTime.UtcNow,
                Success = true
            };

            await LogAsync(auditLog);
        }

        public async Task LogLoginAsync(string username, bool success, string? errorMessage = null, string? ipAddress = null)
        {
            var auditLog = new AuditLog
            {
                Action = success ? "Login" : "LoginFailed",
                EntityName = "User",
                Username = username,
                Description = success ? $"User {username} logged in successfully" : $"Failed login attempt for {username}",
                Category = "Security",
                Severity = success ? "Info" : "Warning",
                IpAddress = ipAddress,
                Success = success,
                ErrorMessage = errorMessage,
                Timestamp = DateTime.UtcNow
            };

            await LogAsync(auditLog);
        }

        public async Task LogLogoutAsync(string username, string? ipAddress = null)
        {
            var auditLog = new AuditLog
            {
                Action = "Logout",
                EntityName = "User",
                Username = username,
                Description = $"User {username} logged out",
                Category = "Security",
                Severity = "Info",
                IpAddress = ipAddress,
                Success = true,
                Timestamp = DateTime.UtcNow
            };

            await LogAsync(auditLog);
        }

        public async Task LogPasswordChangeAsync(string username, bool success, string? errorMessage = null)
        {
            var auditLog = new AuditLog
            {
                Action = "PasswordChange",
                EntityName = "User",
                Username = username,
                Description = success ? $"Password changed successfully for {username}" : $"Failed password change for {username}",
                Category = "Security",
                Severity = success ? "Info" : "Warning",
                Success = success,
                ErrorMessage = errorMessage,
                Timestamp = DateTime.UtcNow
            };

            await LogAsync(auditLog);
        }

        public async Task LogFailedLoginAsync(string username, string? errorMessage = null, string? ipAddress = null)
        {
            await LogLoginAsync(username, false, errorMessage, ipAddress);
        }

        public async Task<List<AuditLog>> GetAllAsync()
        {
            await using var db = _contextFactory.CreateDbContext();
            return await db.AuditLogs
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetByEntityAsync(string entityName, string? entityId = null)
        {
            await using var db = _contextFactory.CreateDbContext();
            var query = db.AuditLogs.Where(a => a.EntityName == entityName);

            if (!string.IsNullOrEmpty(entityId))
                query = query.Where(a => a.EntityId == entityId);

            return await query
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetByUserAsync(string username)
        {
            await using var db = _contextFactory.CreateDbContext();
            return await db.AuditLogs
                .Where(a => a.Username == username)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetByActionAsync(string action)
        {
            await using var db = _contextFactory.CreateDbContext();
            return await db.AuditLogs
                .Where(a => a.Action == action)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            await using var db = _contextFactory.CreateDbContext();
            return await db.AuditLogs
                .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetByCategoryAsync(string category)
        {
            await using var db = _contextFactory.CreateDbContext();
            return await db.AuditLogs
                .Where(a => a.Category == category)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<(List<AuditLog> Logs, int TotalCount)> GetPagedAsync(
            int page, 
            int pageSize, 
            string? searchTerm = null, 
            string? entityName = null, 
            string? action = null, 
            string? username = null)
        {
            await using var db = _contextFactory.CreateDbContext();
            var query = db.AuditLogs.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(a => 
                    a.Description!.Contains(searchTerm) ||
                    a.EntityName.Contains(searchTerm) ||
                    (a.Username != null && a.Username.Contains(searchTerm)) ||
                    (a.Action != null && a.Action.Contains(searchTerm)));
            }

            if (!string.IsNullOrWhiteSpace(entityName))
                query = query.Where(a => a.EntityName == entityName);

            if (!string.IsNullOrWhiteSpace(action))
                query = query.Where(a => a.Action == action);

            if (!string.IsNullOrWhiteSpace(username))
                query = query.Where(a => a.Username == username);

            var totalCount = await query.CountAsync();
            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (logs, totalCount);
        }

        public async Task<int> GetTotalCountAsync()
        {
            await using var db = _contextFactory.CreateDbContext();
            return await db.AuditLogs.CountAsync();
        }

        public async Task<Dictionary<string, int>> GetActionStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            await using var db = _contextFactory.CreateDbContext();
            var query = db.AuditLogs.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate.Value);

            return await query
                .GroupBy(a => a.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Action, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetEntityStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            await using var db = _contextFactory.CreateDbContext();
            var query = db.AuditLogs.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate.Value);

            return await query
                .GroupBy(a => a.EntityName)
                .Select(g => new { EntityName = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.EntityName, x => x.Count);
        }
    }
}
