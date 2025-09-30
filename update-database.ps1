# =============================================
# ModularSys Database Update Script
# Updates database with Sync + CRM Integration
# =============================================

param(
    [string]$Server = "(localdb)\MSSQLLocalDB",
    [string]$Database = "ModularSys",
    [string]$ConnectionString = ""
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "ModularSys Database Update" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Build connection string if not provided
if ([string]::IsNullOrEmpty($ConnectionString)) {
    $ConnectionString = "Server=$Server;Database=$Database;Integrated Security=True;Encrypt=True;Trust Server Certificate=True"
}

Write-Host "Target Database: $Database" -ForegroundColor Yellow
Write-Host "Server: $Server" -ForegroundColor Yellow
Write-Host ""

# Test connection
Write-Host "Testing connection..." -ForegroundColor Yellow
try {
    $testQuery = "SELECT @@VERSION"
    $result = Invoke-Sqlcmd -ConnectionString $ConnectionString -Query $testQuery -ErrorAction Stop
    Write-Host "✓ Connection successful!" -ForegroundColor Green
    Write-Host ""
}
catch {
    Write-Host "✗ Connection failed!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please check:" -ForegroundColor Yellow
    Write-Host "1. SQL Server is running" -ForegroundColor Yellow
    Write-Host "2. Database '$Database' exists" -ForegroundColor Yellow
    Write-Host "3. Connection string is correct" -ForegroundColor Yellow
    exit 1
}

# Run the update script
Write-Host "Running database update script..." -ForegroundColor Yellow
Write-Host ""

try {
    $scriptPath = Join-Path $PSScriptRoot "DATABASE_UPDATE_SCRIPT.sql"
    
    if (-not (Test-Path $scriptPath)) {
        Write-Host "✗ Script file not found: $scriptPath" -ForegroundColor Red
        exit 1
    }
    
    Invoke-Sqlcmd -ConnectionString $ConnectionString -InputFile $scriptPath -Verbose
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "✓ Database update completed successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "New Features Available:" -ForegroundColor Cyan
    Write-Host "1. Offline-First Sync" -ForegroundColor White
    Write-Host "   - Navigate to /sync-management" -ForegroundColor Gray
    Write-Host "   - Configure sync settings" -ForegroundColor Gray
    Write-Host ""
    Write-Host "2. CRM Integration" -ForegroundColor White
    Write-Host "   - Link sales orders to customers" -ForegroundColor Gray
    Write-Host "   - View customer order history" -ForegroundColor Gray
    Write-Host "   - Track lifetime value" -ForegroundColor Gray
    Write-Host ""
    
}
catch {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "✗ Database update failed!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    exit 1
}

Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
