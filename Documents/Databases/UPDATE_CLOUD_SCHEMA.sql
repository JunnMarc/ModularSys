-- Update Cloud Database Schema to Match Local
-- Run this on your CLOUD database

USE db28466;
GO

PRINT 'Updating Opportunities table schema...';

-- Check and add missing columns to Opportunities
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Opportunities' AND COLUMN_NAME = 'Name')
    ALTER TABLE Opportunities ADD [Name] NVARCHAR(200) NOT NULL DEFAULT '';

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Opportunities' AND COLUMN_NAME = 'Value')
    ALTER TABLE Opportunities ADD [Value] DECIMAL(18,2) NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Opportunities' AND COLUMN_NAME = 'LeadSource')
    ALTER TABLE Opportunities ADD [LeadSource] NVARCHAR(100) NULL;

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Opportunities' AND COLUMN_NAME = 'Competitor')
    ALTER TABLE Opportunities ADD [Competitor] NVARCHAR(200) NULL;

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Opportunities' AND COLUMN_NAME = 'AssignedTo')
    ALTER TABLE Opportunities ADD [AssignedTo] NVARCHAR(100) NULL;

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Opportunities' AND COLUMN_NAME = 'Priority')
    ALTER TABLE Opportunities ADD [Priority] NVARCHAR(50) NOT NULL DEFAULT 'Medium';

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Opportunities' AND COLUMN_NAME = 'Status')
    ALTER TABLE Opportunities ADD [Status] NVARCHAR(50) NOT NULL DEFAULT 'Open';

PRINT '✓ Opportunities table updated';
GO

-- Verify the changes
SELECT 'Opportunities' AS TableName, COUNT(*) AS ColumnCount
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Opportunities';
GO

PRINT 'Cloud database schema updated successfully!';
