# ModularSys Database Setup Guide

## 🚀 Quick Setup (Recommended)

### Option 1: PowerShell Script (Easiest)
Run the automated setup script from the project root:

```powershell
.\setup-database.ps1
```

This script will:
- ✅ Check SQL Server connection
- ✅ Run Entity Framework migrations
- ✅ Set up the complete inventory system
- ✅ Add sample data for testing

### Option 2: Manual SQL Script
If you prefer to run the SQL script manually:

1. Open SQL Server Management Studio (SSMS)
2. Connect to your SQL Server instance
3. Open the file: `ModernSys.Data\Migrations\AllInOneInventoryMigration.sql`
4. Execute the script against your ModularSys database

### Option 3: Entity Framework CLI
For development environments:

```bash
# Navigate to EFHost project
cd ModularSys.EFHost

# Update database with all migrations
dotnet ef database update
```

## 📋 Prerequisites

- SQL Server (LocalDB, Express, or Full)
- .NET 8 SDK
- Entity Framework Core tools

## 🗄️ What Gets Created

The all-in-one migration sets up:

### Core Tables
- **Categories** - Product categorization with hierarchy support
- **Products** - Complete product management with inventory tracking
- **SalesOrders** - Customer orders with comprehensive tracking
- **PurchaseOrders** - Supplier orders with delivery management
- **SalesOrderLines** - Individual line items for sales orders
- **PurchaseOrderLines** - Individual line items for purchase orders
- **InventoryTransactions** - Complete audit trail of stock movements
- **RevenueTracking** - Financial tracking and reporting

### Key Features
- ✅ **Soft Delete Support** - All entities support soft delete with audit trail
- ✅ **Advanced Inventory Management** - Stock levels, reorder points, batch tracking
- ✅ **Order Management** - Complete sales and purchase order workflows
- ✅ **Financial Tracking** - Revenue tracking and cost management
- ✅ **Audit Trail** - Complete tracking of who did what when
- ✅ **Performance Optimized** - Proper indexes for fast queries

### Sample Data
- 5 sample categories (Electronics, Office Supplies, Furniture, Software, Hardware)
- 5 sample products with realistic inventory data
- Ready for immediate testing and development

## 🔧 Configuration

### Connection String
Default connection string (modify in `appsettings.json` if needed):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ModularSys;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### Custom Database Name
To use a different database name:

```powershell
.\setup-database.ps1 -DatabaseName "YourDatabaseName"
```

### Custom Connection String
To use a different connection string:

```powershell
.\setup-database.ps1 -ConnectionString "Your connection string here"
```

## 🧪 Testing the Setup

After running the setup, you can verify everything works by:

1. **Start the application**:
   ```bash
   cd ModularSys
   dotnet run
   ```

2. **Check the inventory module**:
   - Navigate to the Inventory section
   - Verify you can see the sample categories and products
   - Try creating a new product or category
   - Test the sales and purchase order forms

3. **Verify database tables**:
   ```sql
   -- Check if tables exist
   SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
   WHERE TABLE_SCHEMA = 'dbo' 
   ORDER BY TABLE_NAME;
   
   -- Check sample data
   SELECT COUNT(*) as CategoryCount FROM Categories;
   SELECT COUNT(*) as ProductCount FROM Products;
   ```

## 🛠️ Troubleshooting

### Common Issues

**SQL Server Connection Failed**
- Ensure SQL Server is running
- Check if LocalDB is installed: `sqllocaldb info`
- Verify connection string is correct

**Migration Already Applied**
- The script is designed to be safe to run multiple times
- It checks for existing tables before creating them

**Permission Errors**
- Run PowerShell as Administrator if needed
- Ensure your user has database creation permissions

**EF Tools Not Found**
```bash
dotnet tool install --global dotnet-ef
```

### Reset Database
To start fresh:

```sql
-- Drop and recreate database
DROP DATABASE IF EXISTS ModularSys;
CREATE DATABASE ModularSys;
```

Then run the setup script again.

## 📚 Next Steps

After successful setup:

1. **Explore the UI** - Check out the enhanced inventory interfaces
2. **Add Your Data** - Replace sample data with your actual inventory
3. **Customize** - Modify categories and products to match your business
4. **Test Workflows** - Try creating sales and purchase orders
5. **Review Reports** - Check the inventory dashboard and reports

## 🎯 Features Ready to Use

- ✅ **Product Management** - Add, edit, delete products with full inventory tracking
- ✅ **Category Management** - Organize products with hierarchical categories
- ✅ **Sales Orders** - Complete customer order management workflow
- ✅ **Purchase Orders** - Supplier order management with delivery tracking
- ✅ **Inventory Dashboard** - Real-time analytics and stock alerts
- ✅ **Transaction History** - Complete audit trail of all inventory movements
- ✅ **Revenue Tracking** - Financial reporting and analytics

Your ModularSys inventory system is now ready for production use! 🎉
