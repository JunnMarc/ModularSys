@echo off
echo ========================================
echo ModularSys Database Update
echo ========================================
echo.

REM Check if sqlcmd is available
where sqlcmd >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo Warning: sqlcmd not found. Using PowerShell method...
    echo.
    powershell -ExecutionPolicy Bypass -File "%~dp0update-database.ps1"
) else (
    echo Using sqlcmd to update database...
    echo.
    sqlcmd -S "(localdb)\MSSQLLocalDB" -d ModularSys -i "%~dp0DATABASE_UPDATE_SCRIPT.sql"
    
    if %ERRORLEVEL% EQU 0 (
        echo.
        echo ========================================
        echo Database update completed successfully!
        echo ========================================
        echo.
    ) else (
        echo.
        echo ========================================
        echo Database update failed!
        echo ========================================
        echo.
    )
)

pause
