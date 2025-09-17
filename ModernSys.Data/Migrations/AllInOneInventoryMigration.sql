-- ========================================
-- ModularSys Complete Inventory System Migration
-- All-in-One Database Setup Script
-- ========================================

-- Run this script to set up the complete inventory system
-- Usage: Execute this script against your ModularSys database

USE [ModularSys]
GO

-- ========================================
-- CATEGORIES TABLE
-- ========================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Categories]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Categories](
        [CategoryId] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](100) NOT NULL,
        [Description] [nvarchar](500) NULL,
        [Icon] [nvarchar](50) NULL,
        [Color] [nvarchar](20) NULL,
        [ParentCategoryId] [int] NULL,
        [IsRevenueCritical] [bit] NOT NULL DEFAULT 0,
        [IsDeleted] [bit] NOT NULL DEFAULT 0,
        [DeletedAt] [datetime2](7) NULL,
        [DeletedBy] [nvarchar](100) NULL,
        [CreatedAt] [datetime2](7) NULL,
        [CreatedBy] [nvarchar](100) NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [UpdatedBy] [nvarchar](100) NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED ([CategoryId] ASC),
        CONSTRAINT [FK_Categories_Categories_ParentCategoryId] FOREIGN KEY([ParentCategoryId]) REFERENCES [dbo].[Categories] ([CategoryId])
    )
    
    CREATE NONCLUSTERED INDEX [IX_Categories_ParentCategoryId] ON [dbo].[Categories] ([ParentCategoryId] ASC)
    CREATE NONCLUSTERED INDEX [IX_Categories_IsDeleted] ON [dbo].[Categories] ([IsDeleted] ASC)
END

-- ========================================
-- PRODUCTS TABLE
-- ========================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Products](
        [ProductId] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](200) NOT NULL,
        [Description] [nvarchar](1000) NULL,
        [SKU] [nvarchar](50) NULL,
        [Barcode] [nvarchar](50) NULL,
        [BatchNumber] [nvarchar](50) NULL,
        [ExpiryDate] [datetime2](7) NULL,
        [Supplier] [nvarchar](200) NULL,
        [CategoryId] [int] NOT NULL,
        [UnitPrice] [decimal](18, 2) NOT NULL,
        [QuantityOnHand] [int] NOT NULL DEFAULT 0,
        [MinStockLevel] [int] NOT NULL DEFAULT 0,
        [MaxStockLevel] [int] NOT NULL DEFAULT 1000,
        [ReorderLevel] [int] NOT NULL DEFAULT 10,
        [IsDeleted] [bit] NOT NULL DEFAULT 0,
        [DeletedAt] [datetime2](7) NULL,
        [DeletedBy] [nvarchar](100) NULL,
        [CreatedAt] [datetime2](7) NULL,
        [CreatedBy] [nvarchar](100) NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [UpdatedBy] [nvarchar](100) NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([ProductId] ASC),
        CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY([CategoryId]) REFERENCES [dbo].[Categories] ([CategoryId]) ON DELETE CASCADE
    )
    
    CREATE NONCLUSTERED INDEX [IX_Products_CategoryId] ON [dbo].[Products] ([CategoryId] ASC)
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Products_SKU] ON [dbo].[Products] ([SKU] ASC) WHERE ([SKU] IS NOT NULL AND [IsDeleted] = 0)
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Products_Barcode] ON [dbo].[Products] ([Barcode] ASC) WHERE ([Barcode] IS NOT NULL AND [IsDeleted] = 0)
    CREATE NONCLUSTERED INDEX [IX_Products_IsDeleted] ON [dbo].[Products] ([IsDeleted] ASC)
END

-- ========================================
-- SALES ORDERS TABLE
-- ========================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SalesOrders]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SalesOrders](
        [SalesOrderId] [int] IDENTITY(1,1) NOT NULL,
        [OrderNumber] [nvarchar](50) NOT NULL,
        [OrderDate] [datetime2](7) NOT NULL,
        [DeliveryDate] [datetime2](7) NULL,
        [Status] [nvarchar](20) NOT NULL DEFAULT 'Pending',
        [CustomerName] [nvarchar](200) NULL,
        [CustomerEmail] [nvarchar](100) NULL,
        [CustomerPhone] [nvarchar](20) NULL,
        [ShippingAddress] [nvarchar](500) NULL,
        [Notes] [nvarchar](1000) NULL,
        [SubTotal] [decimal](18, 2) NOT NULL DEFAULT 0,
        [TaxRate] [decimal](5, 4) NOT NULL DEFAULT 0.12,
        [DiscountAmount] [decimal](18, 2) NOT NULL DEFAULT 0,
        [ShippingCost] [decimal](18, 2) NOT NULL DEFAULT 0,
        [PaymentMethod] [nvarchar](50) NULL,
        [PaymentStatus] [nvarchar](20) NULL DEFAULT 'Pending',
        [IsDeleted] [bit] NOT NULL DEFAULT 0,
        [DeletedAt] [datetime2](7) NULL,
        [DeletedBy] [nvarchar](100) NULL,
        [CreatedAt] [datetime2](7) NULL,
        [CreatedBy] [nvarchar](100) NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [UpdatedBy] [nvarchar](100) NULL,
        CONSTRAINT [PK_SalesOrders] PRIMARY KEY CLUSTERED ([SalesOrderId] ASC)
    )
    
    CREATE UNIQUE NONCLUSTERED INDEX [IX_SalesOrders_OrderNumber] ON [dbo].[SalesOrders] ([OrderNumber] ASC)
    CREATE NONCLUSTERED INDEX [IX_SalesOrders_OrderDate] ON [dbo].[SalesOrders] ([OrderDate] ASC)
    CREATE NONCLUSTERED INDEX [IX_SalesOrders_Status] ON [dbo].[SalesOrders] ([Status] ASC)
    CREATE NONCLUSTERED INDEX [IX_SalesOrders_IsDeleted] ON [dbo].[SalesOrders] ([IsDeleted] ASC)
END

-- ========================================
-- PURCHASE ORDERS TABLE
-- ========================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PurchaseOrders]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PurchaseOrders](
        [PurchaseOrderId] [int] IDENTITY(1,1) NOT NULL,
        [OrderNumber] [nvarchar](50) NOT NULL,
        [OrderDate] [datetime2](7) NOT NULL,
        [ExpectedDeliveryDate] [datetime2](7) NULL,
        [ActualDeliveryDate] [datetime2](7) NULL,
        [Status] [nvarchar](20) NOT NULL DEFAULT 'Pending',
        [SupplierName] [nvarchar](200) NULL,
        [SupplierEmail] [nvarchar](100) NULL,
        [SupplierPhone] [nvarchar](20) NULL,
        [SupplierAddress] [nvarchar](500) NULL,
        [Notes] [nvarchar](1000) NULL,
        [SubTotal] [decimal](18, 2) NOT NULL DEFAULT 0,
        [TaxRate] [decimal](5, 4) NOT NULL DEFAULT 0.12,
        [DiscountAmount] [decimal](18, 2) NOT NULL DEFAULT 0,
        [ShippingCost] [decimal](18, 2) NOT NULL DEFAULT 0,
        [PaymentTerms] [nvarchar](100) NULL,
        [PaymentStatus] [nvarchar](20) NULL DEFAULT 'Pending',
        [IsDeleted] [bit] NOT NULL DEFAULT 0,
        [DeletedAt] [datetime2](7) NULL,
        [DeletedBy] [nvarchar](100) NULL,
        [CreatedAt] [datetime2](7) NULL,
        [CreatedBy] [nvarchar](100) NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [UpdatedBy] [nvarchar](100) NULL,
        CONSTRAINT [PK_PurchaseOrders] PRIMARY KEY CLUSTERED ([PurchaseOrderId] ASC)
    )
    
    CREATE UNIQUE NONCLUSTERED INDEX [IX_PurchaseOrders_OrderNumber] ON [dbo].[PurchaseOrders] ([OrderNumber] ASC)
    CREATE NONCLUSTERED INDEX [IX_PurchaseOrders_OrderDate] ON [dbo].[PurchaseOrders] ([OrderDate] ASC)
    CREATE NONCLUSTERED INDEX [IX_PurchaseOrders_Status] ON [dbo].[PurchaseOrders] ([Status] ASC)
    CREATE NONCLUSTERED INDEX [IX_PurchaseOrders_IsDeleted] ON [dbo].[PurchaseOrders] ([IsDeleted] ASC)
END

-- ========================================
-- SALES ORDER LINES TABLE
-- ========================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SalesOrderLines]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SalesOrderLines](
        [SalesOrderLineId] [int] IDENTITY(1,1) NOT NULL,
        [SalesOrderId] [int] NOT NULL,
        [ProductId] [int] NOT NULL,
        [Quantity] [int] NOT NULL,
        [UnitPrice] [decimal](18, 2) NOT NULL,
        [IsDeleted] [bit] NOT NULL DEFAULT 0,
        [DeletedAt] [datetime2](7) NULL,
        [DeletedBy] [nvarchar](100) NULL,
        [CreatedAt] [datetime2](7) NULL,
        [CreatedBy] [nvarchar](100) NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [UpdatedBy] [nvarchar](100) NULL,
        CONSTRAINT [PK_SalesOrderLines] PRIMARY KEY CLUSTERED ([SalesOrderLineId] ASC),
        CONSTRAINT [FK_SalesOrderLines_Products_ProductId] FOREIGN KEY([ProductId]) REFERENCES [dbo].[Products] ([ProductId]) ON DELETE CASCADE,
        CONSTRAINT [FK_SalesOrderLines_SalesOrders_SalesOrderId] FOREIGN KEY([SalesOrderId]) REFERENCES [dbo].[SalesOrders] ([SalesOrderId]) ON DELETE CASCADE
    )
    
    CREATE NONCLUSTERED INDEX [IX_SalesOrderLines_SalesOrderId] ON [dbo].[SalesOrderLines] ([SalesOrderId] ASC)
    CREATE NONCLUSTERED INDEX [IX_SalesOrderLines_ProductId] ON [dbo].[SalesOrderLines] ([ProductId] ASC)
    CREATE NONCLUSTERED INDEX [IX_SalesOrderLines_IsDeleted] ON [dbo].[SalesOrderLines] ([IsDeleted] ASC)
END

-- ========================================
-- PURCHASE ORDER LINES TABLE
-- ========================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PurchaseOrderLines]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PurchaseOrderLines](
        [PurchaseOrderLineId] [int] IDENTITY(1,1) NOT NULL,
        [PurchaseOrderId] [int] NOT NULL,
        [ProductId] [int] NOT NULL,
        [Quantity] [int] NOT NULL,
        [UnitCost] [decimal](18, 2) NOT NULL,
        [IsDeleted] [bit] NOT NULL DEFAULT 0,
        [DeletedAt] [datetime2](7) NULL,
        [DeletedBy] [nvarchar](100) NULL,
        [CreatedAt] [datetime2](7) NULL,
        [CreatedBy] [nvarchar](100) NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [UpdatedBy] [nvarchar](100) NULL,
        CONSTRAINT [PK_PurchaseOrderLines] PRIMARY KEY CLUSTERED ([PurchaseOrderLineId] ASC),
        CONSTRAINT [FK_PurchaseOrderLines_Products_ProductId] FOREIGN KEY([ProductId]) REFERENCES [dbo].[Products] ([ProductId]) ON DELETE CASCADE,
        CONSTRAINT [FK_PurchaseOrderLines_PurchaseOrders_PurchaseOrderId] FOREIGN KEY([PurchaseOrderId]) REFERENCES [dbo].[PurchaseOrders] ([PurchaseOrderId]) ON DELETE CASCADE
    )
    
    CREATE NONCLUSTERED INDEX [IX_PurchaseOrderLines_PurchaseOrderId] ON [dbo].[PurchaseOrderLines] ([PurchaseOrderId] ASC)
    CREATE NONCLUSTERED INDEX [IX_PurchaseOrderLines_ProductId] ON [dbo].[PurchaseOrderLines] ([ProductId] ASC)
    CREATE NONCLUSTERED INDEX [IX_PurchaseOrderLines_IsDeleted] ON [dbo].[PurchaseOrderLines] ([IsDeleted] ASC)
END

-- ========================================
-- INVENTORY TRANSACTIONS TABLE
-- ========================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InventoryTransactions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[InventoryTransactions](
        [InventoryTransactionId] [int] IDENTITY(1,1) NOT NULL,
        [TransactionNumber] [nvarchar](50) NOT NULL,
        [ProductId] [int] NOT NULL,
        [TransactionType] [nvarchar](20) NOT NULL,
        [QuantityChange] [int] NOT NULL,
        [QuantityBefore] [int] NOT NULL,
        [QuantityAfter] [int] NOT NULL,
        [UnitCost] [decimal](18, 2) NOT NULL,
        [TotalAmount] [decimal](18, 2) NOT NULL,
        [BatchNumber] [nvarchar](50) NULL,
        [Reference] [nvarchar](100) NULL,
        [SalesOrderId] [int] NULL,
        [PurchaseOrderId] [int] NULL,
        [TransactionDate] [datetime2](7) NOT NULL,
        [Notes] [nvarchar](500) NULL,
        [IsDeleted] [bit] NOT NULL DEFAULT 0,
        [DeletedAt] [datetime2](7) NULL,
        [DeletedBy] [nvarchar](100) NULL,
        [CreatedAt] [datetime2](7) NULL,
        [CreatedBy] [nvarchar](100) NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [UpdatedBy] [nvarchar](100) NULL,
        CONSTRAINT [PK_InventoryTransactions] PRIMARY KEY CLUSTERED ([InventoryTransactionId] ASC),
        CONSTRAINT [FK_InventoryTransactions_Products_ProductId] FOREIGN KEY([ProductId]) REFERENCES [dbo].[Products] ([ProductId]) ON DELETE CASCADE,
        CONSTRAINT [FK_InventoryTransactions_SalesOrders_SalesOrderId] FOREIGN KEY([SalesOrderId]) REFERENCES [dbo].[SalesOrders] ([SalesOrderId]),
        CONSTRAINT [FK_InventoryTransactions_PurchaseOrders_PurchaseOrderId] FOREIGN KEY([PurchaseOrderId]) REFERENCES [dbo].[PurchaseOrders] ([PurchaseOrderId])
    )
    
    CREATE NONCLUSTERED INDEX [IX_InventoryTransactions_ProductId] ON [dbo].[InventoryTransactions] ([ProductId] ASC)
    CREATE NONCLUSTERED INDEX [IX_InventoryTransactions_TransactionDate] ON [dbo].[InventoryTransactions] ([TransactionDate] ASC)
    CREATE NONCLUSTERED INDEX [IX_InventoryTransactions_TransactionType] ON [dbo].[InventoryTransactions] ([TransactionType] ASC)
    CREATE UNIQUE NONCLUSTERED INDEX [IX_InventoryTransactions_TransactionNumber] ON [dbo].[InventoryTransactions] ([TransactionNumber] ASC)
    CREATE NONCLUSTERED INDEX [IX_InventoryTransactions_IsDeleted] ON [dbo].[InventoryTransactions] ([IsDeleted] ASC)
END

-- ========================================
-- REVENUE TRACKING TABLE
-- ========================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RevenueTracking]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RevenueTracking](
        [RevenueTrackingId] [int] IDENTITY(1,1) NOT NULL,
        [Date] [datetime2](7) NOT NULL,
        [Amount] [decimal](18, 2) NOT NULL,
        [Source] [nvarchar](50) NOT NULL,
        [Reference] [nvarchar](100) NULL,
        [IsDeleted] [bit] NOT NULL DEFAULT 0,
        [DeletedAt] [datetime2](7) NULL,
        [DeletedBy] [nvarchar](100) NULL,
        [CreatedAt] [datetime2](7) NULL,
        [CreatedBy] [nvarchar](100) NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [UpdatedBy] [nvarchar](100) NULL,
        CONSTRAINT [PK_RevenueTracking] PRIMARY KEY CLUSTERED ([RevenueTrackingId] ASC)
    )
    
    CREATE NONCLUSTERED INDEX [IX_RevenueTracking_Date] ON [dbo].[RevenueTracking] ([Date] ASC)
    CREATE NONCLUSTERED INDEX [IX_RevenueTracking_Source] ON [dbo].[RevenueTracking] ([Source] ASC)
    CREATE NONCLUSTERED INDEX [IX_RevenueTracking_IsDeleted] ON [dbo].[RevenueTracking] ([IsDeleted] ASC)
END

-- ========================================
-- SEED DATA - SAMPLE CATEGORIES
-- ========================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[Categories])
BEGIN
    INSERT INTO [dbo].[Categories] ([Name], [Description], [Icon], [Color], [IsRevenueCritical], [CreatedAt], [CreatedBy])
    VALUES 
        ('Electronics', 'Electronic devices and components', 'Memory', '#2196F3', 1, GETUTCDATE(), 'System'),
        ('Office Supplies', 'General office and administrative supplies', 'Business', '#4CAF50', 0, GETUTCDATE(), 'System'),
        ('Furniture', 'Office and workspace furniture', 'Chair', '#FF9800', 0, GETUTCDATE(), 'System'),
        ('Software', 'Software licenses and digital products', 'Computer', '#9C27B0', 1, GETUTCDATE(), 'System'),
        ('Hardware', 'Computer hardware and peripherals', 'DesktopMac', '#607D8B', 1, GETUTCDATE(), 'System')
END

-- ========================================
-- SEED DATA - SAMPLE PRODUCTS
-- ========================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[Products])
BEGIN
    INSERT INTO [dbo].[Products] ([Name], [Description], [SKU], [CategoryId], [UnitPrice], [QuantityOnHand], [MinStockLevel], [MaxStockLevel], [ReorderLevel], [CreatedAt], [CreatedBy])
    VALUES 
        ('Laptop Computer', 'High-performance business laptop', 'LAP-001', 1, 1200.00, 25, 5, 100, 10, GETUTCDATE(), 'System'),
        ('Office Chair', 'Ergonomic office chair with lumbar support', 'CHR-001', 3, 350.00, 15, 3, 50, 5, GETUTCDATE(), 'System'),
        ('Printer Paper', 'A4 white copy paper, 500 sheets', 'PPR-001', 2, 8.50, 200, 20, 500, 50, GETUTCDATE(), 'System'),
        ('Software License', 'Office productivity suite license', 'SFT-001', 4, 150.00, 100, 10, 200, 20, GETUTCDATE(), 'System'),
        ('Wireless Mouse', 'Bluetooth wireless optical mouse', 'MSE-001', 5, 45.00, 75, 10, 150, 25, GETUTCDATE(), 'System')
END

PRINT 'ModularSys Complete Inventory System Migration completed successfully!'
PRINT 'Database is ready for use with:'
PRINT '- Categories with hierarchy support'
PRINT '- Products with advanced inventory management'
PRINT '- Sales and Purchase Orders with comprehensive tracking'
PRINT '- Inventory Transactions with audit trail'
PRINT '- Revenue Tracking system'
PRINT '- Soft delete support across all entities'
PRINT '- Sample data for testing'

GO
