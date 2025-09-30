-- Reset Sync State to Force Fresh Sync
-- Run this on BOTH local and cloud databases

-- Clear sync metadata (forces fresh sync)
DELETE FROM SyncMetadata WHERE EntityName = 'Opportunity';
DELETE FROM SyncMetadata WHERE EntityName = 'Lead';

-- Clear failed sync logs (optional - keeps history clean)
DELETE FROM SyncLogs WHERE Status = 'Failed' AND ErrorMessage LIKE '%Invalid column name%';

PRINT 'Sync state reset. Next sync will be a fresh full sync.';

-- Verify
SELECT 'SyncMetadata' AS TableName, COUNT(*) AS RemainingRecords FROM SyncMetadata
UNION ALL
SELECT 'SyncLogs', COUNT(*) FROM SyncLogs WHERE Status = 'Failed';
