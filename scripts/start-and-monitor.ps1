# Start Application and Monitor Logs
# This script starts the application and monitors logs in real-time

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Starting GRC Application" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Set environment variables
if (-not $env:JWT_SECRET) {
    $env:JWT_SECRET = "TempJwtSecretForTesting12345678901234567890123456789012345678901234567890"
    Write-Host "[INFO] JWT_SECRET not set, using temporary value" -ForegroundColor Yellow
}

if (-not $env:ConnectionStrings__GrcAuthDb) {
    $env:ConnectionStrings__GrcAuthDb = "Host=localhost;Database=GrcAuthDb;Username=shahin_admin;Password=Shahin@GRC2026!;Port=5432;SSL Mode=Disable"
    Write-Host "[INFO] Using default GrcAuthDb connection string" -ForegroundColor Yellow
}

$env:ASPNETCORE_ENVIRONMENT = "Production"

# Navigate to release directory
$releaseDir = Join-Path $PSScriptRoot "..\src\GrcMvc\bin\Release\net8.0"
if (-not (Test-Path $releaseDir)) {
    Write-Host "[ERROR] Release directory not found: $releaseDir" -ForegroundColor Red
    exit 1
}

Set-Location $releaseDir

# Clean up old log files
if (Test-Path "startup.log") { Remove-Item "startup.log" }
if (Test-Path "startup-errors.log") { Remove-Item "startup-errors.log" }

Write-Host "Starting application..." -ForegroundColor Yellow
Write-Host "Working Directory: $releaseDir" -ForegroundColor Gray
Write-Host ""

# Start application in background
$process = Start-Process -FilePath "dotnet" -ArgumentList "GrcMvc.dll" -NoNewWindow -PassThru -RedirectStandardOutput "startup.log" -RedirectStandardError "startup-errors.log"

Write-Host "Application process started (PID: $($process.Id))" -ForegroundColor Green
Write-Host "Monitoring logs for migration confirmation..." -ForegroundColor Cyan
Write-Host ""

# Wait a bit for application to start
Start-Sleep -Seconds 5

# Monitor logs
$maxWait = 30
$elapsed = 0
$migrationFound = $false

while ($elapsed -lt $maxWait) {
    if (Test-Path "startup.log") {
        $logContent = Get-Content "startup.log" -ErrorAction SilentlyContinue
        
        # Display recent log lines
        if ($logContent) {
            $recentLines = $logContent | Select-Object -Last 10
            Clear-Host
            Write-Host "=== Application Logs (Last 10 lines) ===" -ForegroundColor Cyan
            $recentLines | ForEach-Object { Write-Host $_ }
            Write-Host ""
            
            # Check for migration messages
            if ($logContent -match "Auth database migrations applied" -or $logContent -match "migrations applied") {
                Write-Host "[SUCCESS] Migration applied message found!" -ForegroundColor Green
                $migrationFound = $true
                break
            }
            
            if ($logContent -match "Applying Auth database migrations" -or $logContent -match "Applying.*migrations") {
                Write-Host "[INFO] Migration process detected..." -ForegroundColor Yellow
            }
        }
    }
    
    # Check for errors
    if (Test-Path "startup-errors.log") {
        $errors = Get-Content "startup-errors.log" -ErrorAction SilentlyContinue
        if ($errors) {
            Write-Host "[ERROR] Errors detected:" -ForegroundColor Red
            $errors | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
            break
        }
    }
    
    # Check if process is still running
    if ($process.HasExited) {
        Write-Host "[WARNING] Application process exited (Exit Code: $($process.ExitCode))" -ForegroundColor Yellow
        if (Test-Path "startup-errors.log") {
            Write-Host "Error log:" -ForegroundColor Red
            Get-Content "startup-errors.log" | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
        }
        break
    }
    
    Start-Sleep -Seconds 2
    $elapsed += 2
    Write-Host "Waiting... ($elapsed/$maxWait seconds)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Monitoring Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($migrationFound) {
    Write-Host "[SUCCESS] Migration confirmed!" -ForegroundColor Green
} else {
    Write-Host "[INFO] Full log file available at: startup.log" -ForegroundColor Yellow
    Write-Host "Search for 'migration' or 'Auth database' in the log file" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "To continue monitoring logs in real-time:" -ForegroundColor Cyan
Write-Host "  Get-Content startup.log -Wait -Tail 20" -ForegroundColor White
Write-Host ""
Write-Host "To view full log:" -ForegroundColor Cyan
Write-Host "  Get-Content startup.log" -ForegroundColor White
Write-Host ""
Write-Host "Application is running. Press Ctrl+C to stop monitoring." -ForegroundColor Yellow

# Keep monitoring if user wants
Write-Host ""
$response = Read-Host "Continue monitoring logs in real-time? (Y/N)"
if ($response -eq "Y" -or $response -eq "y") {
    Write-Host "Monitoring logs (Press Ctrl+C to stop)..." -ForegroundColor Cyan
    Get-Content "startup.log" -Wait -Tail 20
}
