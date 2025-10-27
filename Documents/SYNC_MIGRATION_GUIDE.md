# Database Migration Guide for Sync Tables

## Quick Start

### 1. Create Migration

```bash
cd ModularSys.EFHost
dotnet ef migrations add AddOfflineSyncSupport --context ModularSysDbContext
```

### 2. Update Local Database

```bash
dotnet ef database update --context ModularSysDbContext
```

### 3. Deploy to Cloud Database

```bash
# Option A: Using connection string
dotnet ef database update --context ModularSysDbContext --connection "Server=YOUR_CLOUD_SERVER.database.windows.net;Database=ModularSys_Cloud;User Id=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=True;"

# Option B: Generate SQL script and run manually
dotnet ef migrations script --context ModularSysDbContext --output sync_migration.sql
# Then run sync_migration.sql in Azure SQL Database using SSMS or Azure Portal
```

## Manual SQL Script (Alternative)

If you prefer to create tables manually, run this SQL script on both local and cloud databases:

```sql
-- =============================================
-- Sync Tables for ModularSys Offline-First Architecture
-- =============================================

-- Table: SyncMetadata
-- Tracks synchronization state for each entity
CREATE TABLE [dbo].[SyncMetadata] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [EntityName] NVARCHAR(100) NOT NULL,
    [EntityId] NVARCHAR(50) NOT NULL,
    [LastSyncedAt] DATETIME2 NOT NULL,
    [DataHash] NVARCHAR(500) NULL,
    [SyncDirection] NVARCHAR(50) NOT NULL DEFAULT 'Bidirectional',
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    [ErrorMessage] NVARCHAR(MAX) NULL,
    [RetryCount] INT NOT NULL DEFAULT 0,
    [NextRetryAt] DATETIME2 NULL,
    [IsLocalOnly] BIT NOT NULL DEFAULT 0,
    [ConflictResolution] NVARCHAR(50) NULL,
    [ConflictDetectedAt] DATETIME2 NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL,
    CONSTRAINT [UQ_SyncMetadata_EntityName_EntityId] UNIQUE ([EntityName], [EntityId])
);

-- Table: SyncLog
-- Audit log for all synchronization operations
CREATE TABLE [dbo].[SyncLog] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [SyncSessionId] UNIQUEIDENTIFIER NOT NULL,
    [StartedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CompletedAt] DATETIME2 NULL,
    [SyncType] NVARCHAR(50) NOT NULL DEFAULT 'Incremental',
    [Direction] NVARCHAR(50) NOT NULL DEFAULT 'Bidirectional',
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'InProgress',
    [EntitiesSynced] INT NOT NULL DEFAULT 0,
    [EntitiesFailed] INT NOT NULL DEFAULT 0,
    [ConflictsDetected] INT NOT NULL DEFAULT 0,
    [ConflictsResolved] INT NOT NULL DEFAULT 0,
    [ErrorMessage] NVARCHAR(MAX) NULL,
    [ErrorStackTrace] NVARCHAR(MAX) NULL,
    [Details] NVARCHAR(MAX) NULL,
    [DeviceId] NVARCHAR(100) NULL,
    [InitiatedBy] NVARCHAR(100) NULL
);

-- Table: SyncConfiguration
-- Configuration settings for synchronization behavior
CREATE TABLE [dbo].[SyncConfiguration] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [EntityName] NVARCHAR(100) NOT NULL UNIQUE,
    [IsEnabled] BIT NOT NULL DEFAULT 1,
    [Priority] INT NOT NULL DEFAULT 5,
    [Direction] NVARCHAR(50) NOT NULL DEFAULT 'Bidirectional',
    [ConflictResolution] NVARCHAR(50) NOT NULL DEFAULT 'LastWriteWins',
    [MaxRetries] INT NOT NULL DEFAULT 3,
    [RetryDelaySeconds] INT NOT NULL DEFAULT 5,
    [UseExponentialBackoff] BIT NOT NULL DEFAULT 1,
    [BatchSize] INT NOT NULL DEFAULT 100,
    [SyncDeleted] BIT NOT NULL DEFAULT 1,
    [FilterExpression] NVARCHAR(MAX) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL
);

-- Indexes for performance
CREATE INDEX [IX_SyncMetadata_EntityName] ON [dbo].[SyncMetadata]([EntityName]);
CREATE INDEX [IX_SyncMetadata_Status] ON [dbo].[SyncMetadata]([Status]);
CREATE INDEX [IX_SyncMetadata_LastSyncedAt] ON [dbo].[SyncMetadata]([LastSyncedAt]);
CREATE INDEX [IX_SyncLog_SyncSessionId] ON [dbo].[SyncLog]([SyncSessionId]);
CREATE INDEX [IX_SyncLog_StartedAt] ON [dbo].[SyncLog]([StartedAt] DESC);
CREATE INDEX [IX_SyncLog_Status] ON [dbo].[SyncLog]([Status]);

-- Insert default sync configurations
INSERT INTO [dbo].[SyncConfiguration] ([EntityName], [Priority], [ConflictResolution], [BatchSize])
VALUES 
    ('Category', 1, 'LastWriteWins', 100),
    ('Product', 2, 'LastWriteWins', 100),
    ('Customer', 3, 'LastWriteWins', 100),
    ('SalesOrder', 4, 'LastWriteWins', 50),
    ('SalesOrderLine', 5, 'LastWriteWins', 200),
    ('PurchaseOrder', 4, 'LastWriteWins', 50),
    ('PurchaseOrderLine', 5, 'LastWriteWins', 200),
    ('InventoryTransaction', 6, 'LastWriteWins', 100),
    ('Contact', 7, 'LastWriteWins', 100),
    ('Lead', 8, 'LastWriteWins', 100),
    ('Opportunity', 9, 'LastWriteWins', 100),
    ('RevenueTransaction', 10, 'LastWriteWins', 100);

GO

PRINT 'Sync tables created successfully!';
```

## Verify Installation

Run this query to verify tables were created:

```sql
SELECT 
    TABLE_NAME,
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) AS ColumnCount
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_NAME IN ('SyncMetadata', 'SyncLog', 'SyncConfiguration')
ORDER BY TABLE_NAME;
```

Expected output:
```
TABLE_NAME           ColumnCount
SyncConfiguration    13
SyncLog             14
SyncMetadata        15
```

## Performance Indexes

For optimal sync performance, ensure these indexes exist:

```sql
-- Existing entity tables - add indexes for sync performance
CREATE INDEX [IX_Products_UpdatedAt] ON [Products]([UpdatedAt]) 
    WHERE [UpdatedAt] IS NOT NULL;

CREATE INDEX [IX_Products_CreatedAt] ON [Products]([CreatedAt]) 
    WHERE [CreatedAt] IS NOT NULL;

CREATE INDEX [IX_SalesOrders_UpdatedAt] ON [SalesOrders]([UpdatedAt]) 
    WHERE [UpdatedAt] IS NOT NULL;

CREATE INDEX [IX_PurchaseOrders_UpdatedAt] ON [PurchaseOrders]([UpdatedAt]) 
    WHERE [UpdatedAt] IS NOT NULL;

CREATE INDEX [IX_InventoryTransactions_UpdatedAt] ON [InventoryTransactions]([UpdatedAt]) 
    WHERE [UpdatedAt] IS NOT NULL;

CREATE INDEX [IX_Customers_UpdatedAt] ON [Customers]([UpdatedAt]) 
    WHERE [UpdatedAt] IS NOT NULL;

-- Soft delete indexes
CREATE INDEX [IX_Products_IsDeleted] ON [Products]([IsDeleted]) 
    WHERE [IsDeleted] = 1;

CREATE INDEX [IX_SalesOrders_IsDeleted] ON [SalesOrders]([IsDeleted]) 
    WHERE [IsDeleted] = 1;
```

## Rollback (If Needed)

To remove sync tables:

```sql
DROP TABLE IF EXISTS [dbo].[SyncMetadata];
DROP TABLE IF EXISTS [dbo].[SyncLog];
DROP TABLE IF EXISTS [dbo].[SyncConfiguration];
```

Or using EF Core:

```bash
dotnet ef migrations remove --context ModularSysDbContext
```
