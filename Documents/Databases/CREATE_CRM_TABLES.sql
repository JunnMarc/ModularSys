-- =============================================
-- Create CRM Tables for ModularSys
-- =============================================

USE ModularSys;
GO

PRINT 'Creating CRM tables...';
GO

-- =============================================
-- Table: Customers
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Customers')
BEGIN
    CREATE TABLE [dbo].[Customers] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [CompanyName] NVARCHAR(200) NOT NULL,
        [ContactName] NVARCHAR(200) NOT NULL,
        [Email] NVARCHAR(200) NOT NULL,
        [Phone] NVARCHAR(50) NULL,
        [Address] NVARCHAR(500) NULL,
        [City] NVARCHAR(100) NULL,
        [State] NVARCHAR(100) NULL,
        [PostalCode] NVARCHAR(20) NULL,
        [Country] NVARCHAR(100) NULL,
        [Industry] NVARCHAR(100) NULL,
        [CompanySize] INT NULL,
        [Website] NVARCHAR(200) NULL,
        [Notes] NVARCHAR(MAX) NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Active',
        [CustomerType] NVARCHAR(50) NOT NULL DEFAULT 'Prospect',
        
        -- Soft Delete Properties
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL,
        [DeletedBy] NVARCHAR(100) NULL,
        
        -- Audit Properties
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    
    CREATE INDEX [IX_Customers_Email] ON [Customers]([Email]);
    CREATE INDEX [IX_Customers_CompanyName] ON [Customers]([CompanyName]);
    CREATE INDEX [IX_Customers_Status] ON [Customers]([Status]);
    CREATE INDEX [IX_Customers_IsDeleted] ON [Customers]([IsDeleted]) WHERE [IsDeleted] = 0;
    
    PRINT '✓ Customers table created';
END
ELSE
BEGIN
    PRINT '  Customers table already exists';
END
GO

-- =============================================
-- Table: Contacts
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Contacts')
BEGIN
    CREATE TABLE [dbo].[Contacts] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [CustomerId] INT NOT NULL,
        [FirstName] NVARCHAR(100) NOT NULL,
        [LastName] NVARCHAR(100) NOT NULL,
        [Email] NVARCHAR(200) NOT NULL,
        [Phone] NVARCHAR(50) NULL,
        [JobTitle] NVARCHAR(100) NULL,
        [Department] NVARCHAR(100) NULL,
        [IsPrimary] BIT NOT NULL DEFAULT 0,
        [Notes] NVARCHAR(MAX) NULL,
        
        -- Soft Delete Properties
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL,
        [DeletedBy] NVARCHAR(100) NULL,
        
        -- Audit Properties
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        
        CONSTRAINT [FK_Contacts_Customers] FOREIGN KEY ([CustomerId]) 
            REFERENCES [Customers]([Id]) ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_Contacts_CustomerId] ON [Contacts]([CustomerId]);
    CREATE INDEX [IX_Contacts_Email] ON [Contacts]([Email]);
    
    PRINT '✓ Contacts table created';
END
ELSE
BEGIN
    PRINT '  Contacts table already exists';
END
GO

-- =============================================
-- Table: Leads
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Leads')
BEGIN
    CREATE TABLE [dbo].[Leads] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [CompanyName] NVARCHAR(200) NOT NULL,
        [ContactName] NVARCHAR(200) NOT NULL,
        [Email] NVARCHAR(200) NOT NULL,
        [Phone] NVARCHAR(50) NULL,
        [LeadSource] NVARCHAR(100) NOT NULL,
        [Industry] NVARCHAR(100) NULL,
        [EstimatedValue] DECIMAL(18,2) NULL,
        [Notes] NVARCHAR(MAX) NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'New',
        [Priority] NVARCHAR(50) NOT NULL DEFAULT 'Medium',
        [FollowUpDate] DATETIME2 NULL,
        [AssignedTo] NVARCHAR(100) NULL,
        
        -- Soft Delete Properties
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL,
        [DeletedBy] NVARCHAR(100) NULL,
        
        -- Audit Properties
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
    
    CREATE INDEX [IX_Leads_Email] ON [Leads]([Email]);
    CREATE INDEX [IX_Leads_Status] ON [Leads]([Status]);
    CREATE INDEX [IX_Leads_Priority] ON [Leads]([Priority]);
    
    PRINT '✓ Leads table created';
END
ELSE
BEGIN
    PRINT '  Leads table already exists';
END
GO

-- =============================================
-- Table: Opportunities
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Opportunities')
BEGIN
    CREATE TABLE [dbo].[Opportunities] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [CustomerId] INT NOT NULL,
        [OpportunityName] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [EstimatedValue] DECIMAL(18,2) NOT NULL,
        [Probability] INT NOT NULL DEFAULT 50,
        [Stage] NVARCHAR(50) NOT NULL DEFAULT 'Prospecting',
        [ExpectedCloseDate] DATETIME2 NULL,
        [ActualCloseDate] DATETIME2 NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Open',
        [Notes] NVARCHAR(MAX) NULL,
        
        -- Soft Delete Properties
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL,
        [DeletedBy] NVARCHAR(100) NULL,
        
        -- Audit Properties
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        
        CONSTRAINT [FK_Opportunities_Customers] FOREIGN KEY ([CustomerId]) 
            REFERENCES [Customers]([Id]) ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_Opportunities_CustomerId] ON [Opportunities]([CustomerId]);
    CREATE INDEX [IX_Opportunities_Stage] ON [Opportunities]([Stage]);
    CREATE INDEX [IX_Opportunities_Status] ON [Opportunities]([Status]);
    
    PRINT '✓ Opportunities table created';
END
ELSE
BEGIN
    PRINT '  Opportunities table already exists';
END
GO

-- =============================================
-- Now add CustomerId to SalesOrders (if not already done)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SalesOrders') AND name = 'CustomerId')
BEGIN
    ALTER TABLE [dbo].[SalesOrders]
    ADD [CustomerId] INT NULL;
    
    ALTER TABLE [dbo].[SalesOrders]
    ADD CONSTRAINT [FK_SalesOrders_Customers_CustomerId]
    FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([Id])
    ON DELETE SET NULL;
    
    CREATE INDEX [IX_SalesOrders_CustomerId] 
    ON [SalesOrders]([CustomerId]) 
    WHERE [CustomerId] IS NOT NULL;
    
    PRINT '✓ CustomerId added to SalesOrders';
END
ELSE
BEGIN
    PRINT '  CustomerId already exists in SalesOrders';
END
GO

-- =============================================
-- Insert Sample Data
-- =============================================
PRINT 'Inserting sample CRM data...';
GO

-- Sample Customers
IF NOT EXISTS (SELECT * FROM Customers)
BEGIN
    INSERT INTO [Customers] ([CompanyName], [ContactName], [Email], [Phone], [Address], [City], [State], [PostalCode], [Country], [Industry], [Status], [CustomerType])
    VALUES
        ('Acme Corporation', 'John Smith', 'john.smith@acme.com', '+1-555-0101', '123 Business St', 'New York', 'NY', '10001', 'USA', 'Technology', 'Active', 'Customer'),
        ('TechVault Solutions', 'Sarah Johnson', 'sarah.j@techvault.com', '+1-555-0102', '456 Tech Ave', 'San Francisco', 'CA', '94102', 'USA', 'Software', 'Active', 'Customer'),
        ('Global Enterprises', 'Michael Chen', 'mchen@globalent.com', '+1-555-0103', '789 Enterprise Blvd', 'Chicago', 'IL', '60601', 'USA', 'Manufacturing', 'Active', 'Customer'),
        ('StartUp Inc', 'Emily Davis', 'emily@startup.io', '+1-555-0104', '321 Innovation Dr', 'Austin', 'TX', '78701', 'USA', 'Technology', 'Active', 'Prospect'),
        ('Retail Plus', 'David Wilson', 'dwilson@retailplus.com', '+1-555-0105', '654 Commerce St', 'Seattle', 'WA', '98101', 'USA', 'Retail', 'Active', 'Customer');
    
    PRINT '✓ Sample customers inserted';
END
GO

-- Sample Leads
IF NOT EXISTS (SELECT * FROM Leads)
BEGIN
    INSERT INTO [Leads] ([CompanyName], [ContactName], [Email], [Phone], [LeadSource], [Industry], [EstimatedValue], [Status], [Priority])
    VALUES
        ('Future Tech LLC', 'Robert Brown', 'rbrown@futuretech.com', '+1-555-0201', 'Website', 'Technology', 50000.00, 'New', 'High'),
        ('Metro Solutions', 'Lisa Anderson', 'landerson@metro.com', '+1-555-0202', 'Referral', 'Consulting', 25000.00, 'Contacted', 'Medium'),
        ('Innovative Systems', 'James Taylor', 'jtaylor@innovative.com', '+1-555-0203', 'Cold Call', 'Software', 75000.00, 'Qualified', 'High'),
        ('Prime Industries', 'Jennifer White', 'jwhite@prime.com', '+1-555-0204', 'Trade Show', 'Manufacturing', 100000.00, 'New', 'Medium'),
        ('Digital Dynamics', 'Christopher Lee', 'clee@digitaldyn.com', '+1-555-0205', 'Website', 'Marketing', 30000.00, 'Contacted', 'Low');
    
    PRINT '✓ Sample leads inserted';
END
GO

-- =============================================
-- Verification
-- =============================================
PRINT '';
PRINT '==============================================';
PRINT 'CRM Tables Created Successfully!';
PRINT '==============================================';
PRINT '';

SELECT 
    TABLE_NAME,
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) AS ColumnCount
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_NAME IN ('Customers', 'Contacts', 'Leads', 'Opportunities')
ORDER BY TABLE_NAME;

PRINT '';
PRINT 'Sample Data:';
SELECT 'Customers' AS TableName, COUNT(*) AS RecordCount FROM Customers
UNION ALL
SELECT 'Leads', COUNT(*) FROM Leads
UNION ALL
SELECT 'Contacts', COUNT(*) FROM Contacts
UNION ALL
SELECT 'Opportunities', COUNT(*) FROM Opportunities;

PRINT '';
PRINT 'CRM tables are ready to use!';
GO
