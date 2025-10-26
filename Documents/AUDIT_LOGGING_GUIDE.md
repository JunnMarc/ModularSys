# Audit Logging System - User Guide

## Overview

The ModularSys audit logging system provides comprehensive tracking of all system activities, user actions, and data changes. This guide explains how to use and configure the audit logging features.

## Features

### ✅ Automatic Logging
- **User Authentication**: Login, logout, failed login attempts
- **Password Changes**: Successful and failed password change attempts
- **Data Operations**: Create, Update, Delete operations (when implemented)

### ✅ Manual Logging
- Custom audit log entries for specific business logic
- Flexible categorization and severity levels

### ✅ Query & Reporting
- Filter by user, action, entity, category, date range
- Paginated results for large datasets
- Statistics and analytics

## Database Schema

### AuditLog Table

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| Username | string | User who performed the action |
| UserId | int? | User ID (if applicable) |
| Action | string | Action type (Create, Update, Delete, Login, etc.) |
| EntityName | string | Entity affected (User, Product, Order, etc.) |
| EntityId | string | ID of the affected entity |
| OldValues | string | JSON of old values (for updates/deletes) |
| NewValues | string | JSON of new values (for creates/updates) |
| Changes | string | Summary of changes |
| IpAddress | string | IP address of the request |
| UserAgent | string | Browser/client user agent |
| RequestPath | string | Request path/URL |
| Description | string | Human-readable description |
| Category | string | Category (Security, Data, System, etc.) |
| Severity | string | Severity level (Info, Warning, Error, Critical) |
| Timestamp | DateTime | When the action occurred (UTC) |
| Success | bool | Whether the action succeeded |
| ErrorMessage | string | Error message if failed |

### Indexes
- `IX_AuditLog_Timestamp` - For time-based queries
- `IX_AuditLog_Entity` - For entity-based queries
- `IX_AuditLog_Username` - For user-based queries
- `IX_AuditLog_Action` - For action-based queries

## Usage

### 1. Viewing Audit Logs

Navigate to **Audit Logs** page (`/audit-logs`) to view all system activities.

**Features:**
- **Statistics Dashboard**: View total logs, security events, data changes, and failed actions
- **Advanced Filters**: Filter by search term, action, entity, or category
- **Detailed View**: Click "Details" on any log entry to see full information including JSON data

### 2. Programmatic Logging

#### Inject IAuditService

```csharp
public class MyService
{
    private readonly IAuditService _auditService;
    
    public MyService(IAuditService auditService)
    {
        _auditService = auditService;
    }
}
```

#### Log CRUD Operations

```csharp
// Create
await _auditService.LogCreateAsync("Product", productId.ToString(), newProduct, "Created new product");

// Update
await _auditService.LogUpdateAsync("Product", productId.ToString(), oldProduct, newProduct, "Updated product details");

// Delete
await _auditService.LogDeleteAsync("Product", productId.ToString(), oldProduct, "Deleted product");
```

#### Log Custom Actions

```csharp
await _auditService.LogAsync(
    action: "ExportData",
    entityName: "Report",
    entityId: reportId.ToString(),
    description: "User exported sales report",
    category: "System"
);
```

#### Log Security Events

```csharp
// Login (automatically logged by AuthService)
await _auditService.LogLoginAsync(username, success: true);

// Failed login
await _auditService.LogFailedLoginAsync(username, "Invalid password", ipAddress);

// Password change
await _auditService.LogPasswordChangeAsync(username, success: true);
```

### 3. Querying Audit Logs

```csharp
// Get all logs
var allLogs = await _auditService.GetAllAsync();

// Get logs by entity
var productLogs = await _auditService.GetByEntityAsync("Product", productId);

// Get logs by user
var userLogs = await _auditService.GetByUserAsync("john.doe");

// Get logs by action
var deleteLogs = await _auditService.GetByActionAsync("Delete");

// Get logs by date range
var logs = await _auditService.GetByDateRangeAsync(startDate, endDate);

// Get logs by category
var securityLogs = await _auditService.GetByCategoryAsync("Security");

// Get paginated logs with filters
var (logs, totalCount) = await _auditService.GetPagedAsync(
    page: 1,
    pageSize: 20,
    searchTerm: "product",
    entityName: "Product",
    action: "Update",
    username: "admin"
);
```

### 4. Statistics & Analytics

```csharp
// Get total count
var totalLogs = await _auditService.GetTotalCountAsync();

// Get action statistics
var actionStats = await _auditService.GetActionStatisticsAsync(startDate, endDate);
// Returns: { "Login": 150, "Create": 45, "Update": 89, "Delete": 12 }

// Get entity statistics
var entityStats = await _auditService.GetEntityStatisticsAsync(startDate, endDate);
// Returns: { "User": 50, "Product": 120, "Order": 80 }
```

## Action Types

| Action | Description | Category |
|--------|-------------|----------|
| Create | Entity created | Data |
| Update | Entity updated | Data |
| Delete | Entity deleted | Data |
| Login | Successful login | Security |
| LoginFailed | Failed login attempt | Security |
| Logout | User logout | Security |
| PasswordChange | Password changed | Security |
| Export | Data exported | System |
| Import | Data imported | System |
| Custom | Custom action | General |

## Categories

- **Security**: Authentication, authorization, password changes
- **Data**: CRUD operations on entities
- **System**: System-level operations (exports, imports, configuration)
- **General**: Other activities

## Severity Levels

- **Info**: Normal operations
- **Warning**: Important but non-critical events
- **Error**: Errors that occurred
- **Critical**: Critical system events

## Best Practices

### 1. What to Log
✅ **DO Log:**
- All authentication attempts (success and failure)
- Password changes
- Data modifications (Create, Update, Delete)
- Permission changes
- Configuration changes
- Data exports
- Critical business operations

❌ **DON'T Log:**
- Passwords or sensitive credentials
- Personal identifiable information (PII) unless necessary
- High-frequency read operations (can bloat the database)

### 2. Performance Considerations
- Audit logging is asynchronous and won't block operations
- Indexes are configured for optimal query performance
- Consider archiving old logs periodically (e.g., after 1 year)

### 3. Security
- Audit logs should be **read-only** for most users
- Only administrators should have access to view audit logs
- Protect audit log data from tampering
- Consider separate database or table for audit logs in production

### 4. Retention Policy
- Define a retention policy (e.g., keep logs for 1-2 years)
- Archive old logs to separate storage
- Implement automated cleanup for very old logs

## Migration

To create the AuditLog table in your database:

```bash
# Navigate to the ModernSys.Data project
cd ModernSys.Data

# Create migration
dotnet ef migrations add AddAuditLogging --startup-project ../ModularSys

# Apply migration
dotnet ef database update --startup-project ../ModularSys
```

## Troubleshooting

### Logs Not Appearing
1. Check that `IAuditService` is registered in DI container (`MauiProgram.cs`)
2. Verify database connection
3. Check for exceptions in console/logs

### Performance Issues
1. Ensure indexes are created (check migration)
2. Consider adding pagination to queries
3. Archive old logs

### Circular Dependency Issues
- `AuthService` and `UserService` use lazy loading for `IAuditService` to avoid circular dependencies
- Use `SetAuditService()` method if needed

## Future Enhancements

- [ ] Real-time audit log streaming
- [ ] Email notifications for critical events
- [ ] Advanced analytics dashboard
- [ ] Audit log export (CSV, PDF)
- [ ] Automated archiving
- [ ] Compliance reports (GDPR, HIPAA, etc.)

## Support

For issues or questions, contact the development team or create an issue in the project repository.
