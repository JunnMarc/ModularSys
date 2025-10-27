# Deployment Guide: Local + Cloud SQL Server Setup

## 🎯 Overview

This guide covers deploying ModularSys with dual database setup:
- **Local Database**: SQL Server LocalDB/Express (offline operation)
- **Cloud Database**: Azure SQL Database (online sync)

## 📋 Prerequisites

### Local Environment
- Windows 10/11
- SQL Server LocalDB (included with Visual Studio) or SQL Server Express
- .NET 9 SDK
- Visual Studio 2022 or VS Code

### Cloud Environment
- Azure subscription (or any cloud SQL Server provider)
- Azure SQL Database (Basic tier minimum, Standard recommended)
- Firewall rules configured for your IP

## 🚀 Deployment Steps

### Part 1: Local Database Setup

#### Option A: Using SQL Server LocalDB (Recommended for Development)

1. **Verify LocalDB Installation**
```powershell
sqllocaldb info
```

If not installed, download from: https://aka.ms/ssmsfullsetup

2. **Create Local Database**
```powershell
cd ModularSys.EFHost
dotnet ef database update --context ModularSysDbContext
```

3. **Verify Connection**
```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT name FROM sys.databases WHERE name = 'ModularSys'"
```

#### Option B: Using SQL Server Express (Recommended for Production)

1. **Install SQL Server Express**
   - Download from: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
   - Choose "Express" edition
   - Enable TCP/IP protocol during installation

2. **Update Connection String**

In `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "LocalConnection": "Server=localhost\\SQLEXPRESS;Database=ModularSys_Local;Integrated Security=True;Encrypt=True;Trust Server Certificate=True"
  }
}
```

3. **Create Database**
```powershell
dotnet ef database update --context ModularSysDbContext --connection "Server=localhost\\SQLEXPRESS;Database=ModularSys_Local;Integrated Security=True;Encrypt=True;Trust Server Certificate=True"
```

### Part 2: Cloud Database Setup (Azure SQL)

#### Step 1: Create Azure SQL Database

**Using Azure Portal:**

1. Go to https://portal.azure.com
2. Create a new resource → "SQL Database"
3. Configure:
   - **Database name**: `ModularSys_Cloud`
   - **Server**: Create new or use existing
   - **Pricing tier**: Standard S0 (minimum) or S1 (recommended)
   - **Backup redundancy**: Locally-redundant (cheaper) or Geo-redundant
   - **Compute + storage**: 10 DTUs minimum

4. Configure Firewall:
   - Add your client IP address
   - Enable "Allow Azure services to access server"

**Using Azure CLI:**

```bash
# Login to Azure
az login

# Create resource group
az group create --name ModularSys-RG --location eastus

# Create SQL Server
az sql server create \
  --name modularsys-sql-server \
  --resource-group ModularSys-RG \
  --location eastus \
  --admin-user sqladmin \
  --admin-password "YourStrongPassword123!"

# Create database
az sql db create \
  --resource-group ModularSys-RG \
  --server modularsys-sql-server \
  --name ModularSys_Cloud \
  --service-objective S0

# Configure firewall
az sql server firewall-rule create \
  --resource-group ModularSys-RG \
  --server modularsys-sql-server \
  --name AllowMyIP \
  --start-ip-address YOUR_IP \
  --end-ip-address YOUR_IP
```

#### Step 2: Get Connection String

From Azure Portal:
1. Go to your SQL Database
2. Click "Connection strings"
3. Copy the ADO.NET connection string
4. Replace `{your_password}` with your actual password

Example:
```
Server=tcp:modularsys-sql-server.database.windows.net,1433;Initial Catalog=ModularSys_Cloud;Persist Security Info=False;User ID=sqladmin;Password=YourStrongPassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

#### Step 3: Deploy Schema to Cloud

**Option A: Using EF Core Migrations**

```powershell
cd ModularSys.EFHost

# Set cloud connection string
$cloudConn = "Server=tcp:modularsys-sql-server.database.windows.net,1433;Initial Catalog=ModularSys_Cloud;User ID=sqladmin;Password=YourStrongPassword123!;Encrypt=True;"

# Apply migrations
dotnet ef database update --context ModularSysDbContext --connection $cloudConn
```

**Option B: Using SQL Script**

```powershell
# Generate SQL script
dotnet ef migrations script --context ModularSysDbContext --output deploy_cloud.sql

# Run script using sqlcmd
sqlcmd -S modularsys-sql-server.database.windows.net -U sqladmin -P YourStrongPassword123! -d ModularSys_Cloud -i deploy_cloud.sql
```

**Option C: Using SSMS**

1. Open SQL Server Management Studio
2. Connect to: `modularsys-sql-server.database.windows.net`
3. Open and execute the migration script
4. Verify tables were created

### Part 3: Configure Application

#### Update appsettings.json

Create `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "LocalConnection": "Server=localhost\\SQLEXPRESS;Database=ModularSys_Local;Integrated Security=True;Encrypt=True;Trust Server Certificate=True",
    "CloudConnection": "Server=tcp:modularsys-sql-server.database.windows.net,1433;Initial Catalog=ModularSys_Cloud;User ID=sqladmin;Password=YourStrongPassword123!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  },
  "SyncSettings": {
    "Enabled": true,
    "Mode": "Hybrid",
    "AutoSyncEnabled": true,
    "SyncIntervalMinutes": 15,
    "ConflictResolution": "LastWriteWins",
    "MaxRetries": 3,
    "RetryDelaySeconds": 5,
    "UseExponentialBackoff": true,
    "BatchSize": 100,
    "SyncDeletedRecords": true
  }
}
```

#### Secure Connection Strings

**For Development - Use User Secrets:**

```powershell
cd ModularSys
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:CloudConnection" "YOUR_CLOUD_CONNECTION_STRING"
```

**For Production - Use Environment Variables:**

Windows:
```powershell
setx ConnectionStrings__CloudConnection "YOUR_CLOUD_CONNECTION_STRING" /M
```

Linux/Mac:
```bash
export ConnectionStrings__CloudConnection="YOUR_CLOUD_CONNECTION_STRING"
```

**For Azure Deployment - Use App Configuration:**

```bash
az webapp config appsettings set \
  --name your-app-name \
  --resource-group ModularSys-RG \
  --settings ConnectionStrings__CloudConnection="YOUR_CLOUD_CONNECTION_STRING"
```

### Part 4: Register Sync Services

Update `MauiProgram.cs`:

```csharp
using ModularSys.Data.Common.Services.Sync;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
        // ... existing configuration ...
        
        // Load sync configuration
        builder.Configuration.AddJsonFile("appsettings.Sync.json", optional: true, reloadOnChange: true);
        
        // Register sync services
        builder.Services.AddSyncServices(builder.Configuration);
        
        // ... rest of configuration ...
        
        return builder.Build();
    }
}
```

### Part 5: Initial Data Sync

After deployment, perform initial sync:

```csharp
// In your startup code or admin panel
public async Task InitialSync()
{
    var syncService = serviceProvider.GetRequiredService<ISyncService>();
    
    // Check if cloud is reachable
    if (await syncService.IsOnlineAsync())
    {
        // Perform full sync
        var result = await syncService.SyncAllAsync();
        
        if (result.Success)
        {
            Console.WriteLine($"Initial sync completed: {result.EntitiesSynced} entities synced");
        }
        else
        {
            Console.WriteLine($"Initial sync failed: {result.ErrorMessage}");
        }
    }
    else
    {
        Console.WriteLine("Cloud not reachable. Will sync when online.");
    }
}
```

## 🔒 Security Best Practices

### 1. Connection String Security

❌ **DON'T** hardcode passwords in appsettings.json  
✅ **DO** use User Secrets (dev) or Azure Key Vault (prod)

### 2. Azure SQL Security

```sql
-- Create read-only user for reporting
CREATE USER [ReportUser] WITH PASSWORD = 'StrongPassword123!';
ALTER ROLE db_datareader ADD MEMBER [ReportUser];

-- Create app user with limited permissions
CREATE USER [AppUser] WITH PASSWORD = 'StrongPassword456!';
ALTER ROLE db_datareader ADD MEMBER [AppUser];
ALTER ROLE db_datawriter ADD MEMBER [AppUser];
GRANT EXECUTE TO [AppUser];
```

### 3. Enable Advanced Security

```bash
# Enable Advanced Threat Protection
az sql db threat-policy update \
  --resource-group ModularSys-RG \
  --server modularsys-sql-server \
  --name ModularSys_Cloud \
  --state Enabled

# Enable Auditing
az sql db audit-policy update \
  --resource-group ModularSys-RG \
  --server modularsys-sql-server \
  --name ModularSys_Cloud \
  --state Enabled \
  --storage-account your-storage-account
```

### 4. Network Security

- Use **Private Endpoints** for production
- Enable **Firewall rules** to restrict access
- Use **VPN** or **ExpressRoute** for on-premises connectivity

## 📊 Monitoring & Maintenance

### Monitor Sync Operations

```sql
-- View recent sync sessions
SELECT TOP 10
    SyncSessionId,
    StartedAt,
    CompletedAt,
    Status,
    EntitiesSynced,
    EntitiesFailed,
    ConflictsDetected
FROM SyncLogs
ORDER BY StartedAt DESC;

-- Check for failed syncs
SELECT * FROM SyncLogs
WHERE Status = 'Failed'
ORDER BY StartedAt DESC;

-- Monitor sync metadata
SELECT 
    EntityName,
    COUNT(*) as TotalRecords,
    SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END) as Pending,
    SUM(CASE WHEN Status = 'Failed' THEN 1 ELSE 0 END) as Failed,
    SUM(CASE WHEN Status = 'Conflict' THEN 1 ELSE 0 END) as Conflicts
FROM SyncMetadata
GROUP BY EntityName;
```

### Database Maintenance

```sql
-- Update statistics (run weekly)
EXEC sp_updatestats;

-- Rebuild indexes (run monthly)
ALTER INDEX ALL ON Products REBUILD;
ALTER INDEX ALL ON SalesOrders REBUILD;
ALTER INDEX ALL ON InventoryTransactions REBUILD;

-- Clean old sync logs (run monthly)
DELETE FROM SyncLogs
WHERE StartedAt < DATEADD(MONTH, -3, GETUTCDATE());
```

### Azure SQL Monitoring

```bash
# View database metrics
az monitor metrics list \
  --resource /subscriptions/{subscription-id}/resourceGroups/ModularSys-RG/providers/Microsoft.Sql/servers/modularsys-sql-server/databases/ModularSys_Cloud \
  --metric "dtu_consumption_percent" \
  --start-time 2024-01-01T00:00:00Z \
  --end-time 2024-01-02T00:00:00Z

# Set up alerts
az monitor metrics alert create \
  --name HighDTU \
  --resource-group ModularSys-RG \
  --scopes /subscriptions/{subscription-id}/resourceGroups/ModularSys-RG/providers/Microsoft.Sql/servers/modularsys-sql-server/databases/ModularSys_Cloud \
  --condition "avg dtu_consumption_percent > 80" \
  --description "Alert when DTU usage exceeds 80%"
```

## 🧪 Testing Deployment

### 1. Test Local Database

```powershell
# Test connection
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT @@VERSION"

# Verify tables
sqlcmd -S "(localdb)\MSSQLLocalDB" -d ModularSys -Q "SELECT name FROM sys.tables ORDER BY name"
```

### 2. Test Cloud Database

```powershell
# Test connection
sqlcmd -S modularsys-sql-server.database.windows.net -U sqladmin -P YourPassword -Q "SELECT @@VERSION"

# Verify tables
sqlcmd -S modularsys-sql-server.database.windows.net -U sqladmin -P YourPassword -d ModularSys_Cloud -Q "SELECT name FROM sys.tables ORDER BY name"
```

### 3. Test Sync

```csharp
// Create test record locally
var product = new Product
{
    Name = "Test Sync Product",
    SKU = "SYNC-TEST-001",
    UnitPrice = 99.99m,
    CreatedAt = DateTime.UtcNow
};

await localContext.Products.AddAsync(product);
await localContext.SaveChangesAsync();

// Trigger sync
var result = await syncService.SyncEntityAsync<Product>();

// Verify in cloud
var cloudProduct = await cloudContext.Products
    .FirstOrDefaultAsync(p => p.SKU == "SYNC-TEST-001");

Assert.NotNull(cloudProduct);
```

## 🚨 Troubleshooting

### Issue: Cannot connect to LocalDB

**Solution:**
```powershell
# Start LocalDB
sqllocaldb start MSSQLLocalDB

# Check status
sqllocaldb info MSSQLLocalDB
```

### Issue: Cannot connect to Azure SQL

**Solution:**
1. Check firewall rules in Azure Portal
2. Verify credentials
3. Test connection:
```powershell
Test-NetConnection -ComputerName modularsys-sql-server.database.windows.net -Port 1433
```

### Issue: Sync fails with timeout

**Solution:**
- Increase connection timeout in connection string: `Connection Timeout=60`
- Reduce batch size in sync settings
- Check network bandwidth

### Issue: High DTU usage on Azure SQL

**Solution:**
- Scale up to higher tier (S1, S2, etc.)
- Add indexes on frequently queried columns
- Optimize sync batch size
- Schedule heavy syncs during off-peak hours

## 💰 Cost Optimization

### Azure SQL Pricing Tiers

| Tier | DTUs | Storage | Monthly Cost* |
|------|------|---------|---------------|
| Basic | 5 | 2 GB | ~$5 |
| S0 | 10 | 250 GB | ~$15 |
| S1 | 20 | 250 GB | ~$30 |
| S2 | 50 | 250 GB | ~$75 |

*Prices are approximate and vary by region

### Cost Saving Tips

1. **Use Basic tier for development/testing**
2. **Scale down during off-hours** (if not 24/7)
3. **Use serverless tier** for variable workloads
4. **Enable auto-pause** for development databases
5. **Set up budget alerts**

```bash
# Create serverless database (auto-pause after 1 hour)
az sql db create \
  --resource-group ModularSys-RG \
  --server modularsys-sql-server \
  --name ModularSys_Cloud \
  --edition GeneralPurpose \
  --compute-model Serverless \
  --family Gen5 \
  --capacity 2 \
  --auto-pause-delay 60
```

## ✅ Deployment Checklist

- [ ] Local SQL Server installed and running
- [ ] Azure SQL Database created
- [ ] Firewall rules configured
- [ ] Connection strings configured
- [ ] Migrations applied to both databases
- [ ] Sync tables created
- [ ] Sync services registered in DI
- [ ] Initial sync completed successfully
- [ ] Monitoring and alerts configured
- [ ] Backup strategy implemented
- [ ] Security best practices applied
- [ ] Documentation updated

## 🎉 Success!

Your ModularSys application is now deployed with dual database setup and ready for offline-first operation with cloud synchronization!

**Next Steps:**
1. Monitor sync operations in production
2. Set up automated backups
3. Configure disaster recovery
4. Implement performance monitoring
5. Train users on offline capabilities
