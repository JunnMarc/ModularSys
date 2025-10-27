-- =============================================
-- ModularSys Database Update Script
-- Adds: Sync Tables + CRM Integration
-- =============================================

USE ModularSys;
GO

PRINT 'Starting database update...';
GO

-- =============================================
-- PART 1: SYNC TABLES
-- =============================================

PRINT 'Creating Sync tables...';
GO

-- Table: SyncMetadata
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SyncMetadata')
BEGIN
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
    PRINT '✓ SyncMetadata table created';
END
ELSE
BEGIN
    PRINT '  SyncMetadata table already exists';
END
GO

-- Table: SyncLog
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SyncLogs')
BEGIN
    CREATE TABLE [dbo].[SyncLogs] (
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
    PRINT '✓ SyncLogs table created';
END
ELSE
BEGIN
    PRINT '  SyncLogs table already exists';
END
GO

-- Table: SyncConfiguration
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SyncConfigurations')
BEGIN
    CREATE TABLE [dbo].[SyncConfigurations] (
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
    PRINT '✓ SyncConfigurations table created';
END
ELSE
BEGIN
    PRINT '  SyncConfigurations table already exists';
END
GO

-- Indexes for Sync tables
PRINT 'Creating Sync indexes...';
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SyncMetadata_EntityName')
    CREATE INDEX [IX_SyncMetadata_EntityName] ON [dbo].[SyncMetadata]([EntityName]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SyncMetadata_Status')
    CREATE INDEX [IX_SyncMetadata_Status] ON [dbo].[SyncMetadata]([Status]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SyncMetadata_LastSyncedAt')
    CREATE INDEX [IX_SyncMetadata_LastSyncedAt] ON [dbo].[SyncMetadata]([LastSyncedAt]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SyncLog_SyncSessionId')
    CREATE INDEX [IX_SyncLog_SyncSessionId] ON [dbo].[SyncLogs]([SyncSessionId]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SyncLog_StartedAt')
    CREATE INDEX [IX_SyncLog_StartedAt] ON [dbo].[SyncLogs]([StartedAt] DESC);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SyncLog_Status')
    CREATE INDEX [IX_SyncLog_Status] ON [dbo].[SyncLogs]([Status]);

PRINT '✓ Sync indexes created';
GO

-- Insert default sync configurations
PRINT 'Inserting default sync configurations...';
GO

IF NOT EXISTS (SELECT * FROM SyncConfigurations WHERE EntityName = 'Category')
BEGIN
    INSERT INTO [dbo].[SyncConfigurations] ([EntityName], [Priority], [ConflictResolution], [BatchSize])
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
    PRINT '✓ Default sync configurations inserted';
END
ELSE
BEGIN
    PRINT '  Sync configurations already exist';
END
GO

-- =============================================
-- PART 2: CRM INTEGRATION
-- =============================================

PRINT 'Adding CRM integration to SalesOrders...';
GO

-- Add CustomerId column to SalesOrders
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SalesOrders') AND name = 'CustomerId')
BEGIN
    ALTER TABLE [dbo].[SalesOrders]
    ADD [CustomerId] INT NULL;
    PRINT '✓ CustomerId column added to SalesOrders';
END
ELSE
BEGIN
    PRINT '  CustomerId column already exists in SalesOrders';
END
GO

-- Add foreign key constraint
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SalesOrders_Customers_CustomerId')
BEGIN
    ALTER TABLE [dbo].[SalesOrders]
    ADD CONSTRAINT [FK_SalesOrders_Customers_CustomerId]
    FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers]([Id])
    ON DELETE SET NULL;
    PRINT '✓ Foreign key constraint added';
END
ELSE
BEGIN
    PRINT '  Foreign key constraint already exists';
END
GO

-- Add index for performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SalesOrders_CustomerId')
BEGIN
    CREATE INDEX [IX_SalesOrders_CustomerId] 
    ON [dbo].[SalesOrders]([CustomerId]) 
    WHERE [CustomerId] IS NOT NULL;
    PRINT '✓ CustomerId index created';
END
ELSE
BEGIN
    PRINT '  CustomerId index already exists';
END
GO

-- =============================================
-- PART 3: PERFORMANCE INDEXES FOR SYNC
-- =============================================

PRINT 'Creating performance indexes for sync...';
GO

-- Products
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_UpdatedAt')
    CREATE INDEX [IX_Products_UpdatedAt] ON [dbo].[Products]([UpdatedAt]) 
    WHERE [UpdatedAt] IS NOT NULL;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_CreatedAt')
    CREATE INDEX [IX_Products_CreatedAt] ON [dbo].[Products]([CreatedAt]) 
    WHERE [CreatedAt] IS NOT NULL;

-- SalesOrders
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SalesOrders_UpdatedAt')
    CREATE INDEX [IX_SalesOrders_UpdatedAt] ON [dbo].[SalesOrders]([UpdatedAt]) 
    WHERE [UpdatedAt] IS NOT NULL;

-- PurchaseOrders
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PurchaseOrders_UpdatedAt')
    CREATE INDEX [IX_PurchaseOrders_UpdatedAt] ON [dbo].[PurchaseOrders]([UpdatedAt]) 
    WHERE [UpdatedAt] IS NOT NULL;

-- InventoryTransactions
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_InventoryTransactions_UpdatedAt')
    CREATE INDEX [IX_InventoryTransactions_UpdatedAt] ON [dbo].[InventoryTransactions]([UpdatedAt]) 
    WHERE [UpdatedAt] IS NOT NULL;

-- Customers
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_UpdatedAt')
    CREATE INDEX [IX_Customers_UpdatedAt] ON [dbo].[Customers]([UpdatedAt]) 
    WHERE [UpdatedAt] IS NOT NULL;

-- Soft delete indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_IsDeleted')
    CREATE INDEX [IX_Products_IsDeleted] ON [dbo].[Products]([IsDeleted]) 
    WHERE [IsDeleted] = 1;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SalesOrders_IsDeleted')
    CREATE INDEX [IX_SalesOrders_IsDeleted] ON [dbo].[SalesOrders]([IsDeleted]) 
    WHERE [IsDeleted] = 1;

PRINT '✓ Performance indexes created';
GO

-- =============================================
-- VERIFICATION
-- =============================================

PRINT '';
PRINT '==============================================';
PRINT 'Database Update Complete!';
PRINT '==============================================';
PRINT '';

-- Verify Sync tables
PRINT 'Sync Tables:';
SELECT 
    TABLE_NAME,
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) AS ColumnCount
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_NAME IN ('SyncMetadata', 'SyncLogs', 'SyncConfigurations')
ORDER BY TABLE_NAME;

-- Verify CRM integration
PRINT '';
PRINT 'CRM Integration:';
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SalesOrders') AND name = 'CustomerId')
    PRINT '✓ SalesOrders.CustomerId column exists';
ELSE
    PRINT '✗ SalesOrders.CustomerId column missing';

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SalesOrders_Customers_CustomerId')
    PRINT '✓ Foreign key constraint exists';
ELSE
    PRINT '✗ Foreign key constraint missing';

PRINT '';
PRINT 'You can now:';
PRINT '1. Use offline sync features';
PRINT '2. Link sales orders to CRM customers';
PRINT '3. Track customer order history and lifetime value';
PRINT '';
PRINT 'Next steps:';
PRINT '- Navigate to /sync-management to configure sync';
PRINT '- Create sales orders with customer selection';
PRINT '- View customer details with order history';
PRINT '';
PRINT '==============================================';
GO
