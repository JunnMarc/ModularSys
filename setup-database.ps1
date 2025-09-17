# ========================================
# ModularSys Database Setup Script
# ========================================
# This script sets up the complete ModularSys database with inventory system
# Run this script from the project root directory

param(
    [string]$ConnectionString = "Server=(localdb)\mssqllocaldb;Database=ModularSys;Trusted_Connection=true;MultipleActiveResultSets=true",
    [string]$DatabaseName = "ModularSys"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "ModularSys Database Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if SQL Server is available
Write-Host "Checking SQL Server connection..." -ForegroundColor Yellow
try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $ConnectionString
    $connection.Open()
    $connection.Close()
    Write-Host "✓ SQL Server connection successful" -ForegroundColor Green
}
catch {
    Write-Host "✗ SQL Server connection failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Please ensure SQL Server is running and connection string is correct." -ForegroundColor Yellow
    exit 1
}

# Run EF Core migrations first
Write-Host ""
Write-Host "Running Entity Framework Core migrations..." -ForegroundColor Yellow
try {
    Set-Location "ModularSys.EFHost"
    dotnet ef database update --verbose
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ EF Core migrations completed successfully" -ForegroundColor Green
    } else {
        Write-Host "✗ EF Core migrations failed" -ForegroundColor Red
        Set-Location ".."
        exit 1
    }
    Set-Location ".."
}
catch {
    Write-Host "✗ Error running EF migrations: $($_.Exception.Message)" -ForegroundColor Red
    Set-Location ".."
    exit 1
}

# Run the all-in-one inventory migration
Write-Host ""
Write-Host "Setting up inventory system..." -ForegroundColor Yellow
$sqlScript = Get-Content "ModernSys.Data\Migrations\AllInOneInventoryMigration.sql" -Raw

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $ConnectionString
    $connection.Open()
    
    $command = New-Object System.Data.SqlClient.SqlCommand
    $command.Connection = $connection
    $command.CommandText = $sqlScript
    $command.CommandTimeout = 300  # 5 minutes timeout
    
    $result = $command.ExecuteNonQuery()
    $connection.Close()
    
    Write-Host "✓ Inventory system setup completed successfully" -ForegroundColor Green
}
catch {
    Write-Host "✗ Error setting up inventory system: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Database Setup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Your ModularSys database is now ready with:" -ForegroundColor White
Write-Host "✓ User management and authentication system" -ForegroundColor Green
Write-Host "✓ Role-based permission system" -ForegroundColor Green
Write-Host "✓ Complete inventory management system" -ForegroundColor Green
Write-Host "✓ Categories with hierarchy support" -ForegroundColor Green
Write-Host "✓ Products with advanced inventory tracking" -ForegroundColor Green
Write-Host "✓ Sales and Purchase Orders" -ForegroundColor Green
Write-Host "✓ Inventory Transactions with audit trail" -ForegroundColor Green
Write-Host "✓ Revenue tracking system" -ForegroundColor Green
Write-Host "✓ Soft delete support across all entities" -ForegroundColor Green
Write-Host "✓ Sample data for testing" -ForegroundColor Green
Write-Host ""
Write-Host "You can now run your ModularSys application!" -ForegroundColor Cyan
Write-Host ""
Write-Host "Database: $DatabaseName" -ForegroundColor Gray
Write-Host "Connection: $ConnectionString" -ForegroundColor Gray
