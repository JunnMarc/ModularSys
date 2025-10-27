-- Update SyncConfigurations to Remove CRM Entities
-- Run this on BOTH local and cloud databases

USE ModularSys; -- Change to db28466 for cloud
GO

PRINT 'Removing CRM entities from SyncConfigurations...';

-- Delete CRM entity configurations
DELETE FROM SyncConfigurations 
WHERE EntityName IN ('Customer', 'Contact', 'Lead', 'Opportunity');

PRINT '✓ CRM entities removed from SyncConfigurations';
GO

-- Verify remaining configurations
SELECT EntityName, IsEnabled, Priority, Direction
FROM SyncConfigurations
ORDER BY Priority;
GO

PRINT 'Current SyncConfigurations:';
PRINT '----------------------------';
