-- Test Cloud Database Connection and Schema
-- Run this on your CLOUD database to verify everything is ready

-- 1. Check all tables exist
SELECT 'Tables Check' AS Test, COUNT(*) AS TableCount
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE';

-- 2. Check SyncMetadata structure
SELECT 'SyncMetadata Columns' AS Test, COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SyncMetadata'
ORDER BY ORDINAL_POSITION;

-- 3. Check SyncLogs structure
SELECT 'SyncLogs Columns' AS Test, COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SyncLogs'
ORDER BY ORDINAL_POSITION;

-- 4. Check if we can insert a test sync log
BEGIN TRY
    INSERT INTO SyncLogs (SyncSessionId, StartedAt, SyncType, Direction, Status, EntitiesSynced, EntitiesFailed, ConflictsDetected, ConflictsResolved)
    VALUES (NEWID(), GETUTCDATE(), 'Test', 'Bidirectional', 'InProgress', 0, 0, 0, 0);
    
    SELECT 'Insert Test' AS Test, 'SUCCESS - Can write to SyncLogs' AS Result;
    
    -- Clean up test record
    DELETE FROM SyncLogs WHERE SyncType = 'Test';
END TRY
BEGIN CATCH
    SELECT 'Insert Test' AS Test, 
           'FAILED - ' + ERROR_MESSAGE() AS Result;
END CATCH;

-- 5. Check SyncConfigurations
SELECT 'SyncConfigurations' AS Test, COUNT(*) AS ConfigCount
FROM SyncConfigurations;

-- 6. Check for any data in key tables
SELECT 'Data Check' AS Test,
       (SELECT COUNT(*) FROM Products) AS Products,
       (SELECT COUNT(*) FROM Categories) AS Categories,
       (SELECT COUNT(*) FROM Customers) AS Customers,
       (SELECT COUNT(*) FROM SalesOrders) AS SalesOrders;

-- 7. Check CustomerId column in SalesOrders
SELECT 'SalesOrders.CustomerId' AS Test, 
       COLUMN_NAME, 
       DATA_TYPE, 
       IS_NULLABLE,
       CASE WHEN IS_NULLABLE = 'YES' THEN 'OK' ELSE 'ERROR - Should be nullable' END AS Status
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SalesOrders' AND COLUMN_NAME = 'CustomerId';
