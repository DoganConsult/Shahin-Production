# Start GRC Portal Script
# This script starts the ASP.NET Core GRC Portal on port 5001
# Port 5000 is occupied by Docker Desktop on Windows

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Starting GRC Portal Application" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if we're in the correct directory
$portalPath = "C:\Shahin-ai\Shahin-Jan-2026\src\GrcMvc"

if (-not (Test-Path $portalPath)) {
    Write-Host "ERROR: GRC Portal directory not found at $portalPath" -ForegroundColor Red
    exit 1
}

# Navigate to portal directory
Set-Location $portalPath
Write-Host "✓ Changed directory to: $portalPath" -ForegroundColor Green

# Check if port 5001 is available
Write-Host ""
Write-Host "Checking if port 5001 is available..." -ForegroundColor Yellow
$portCheck = netstat -ano | findstr ":5001"

if ($portCheck) {
    Write-Host "WARNING: Port 5001 is already in use!" -ForegroundColor Red
    Write-Host "Current processes using port 5001:" -ForegroundColor Yellow
    Write-Host $portCheck
    Write-Host ""
    $continue = Read-Host "Do you want to continue anyway? (y/n)"
    if ($continue -ne "y") {
        exit 0
    }
} else {
    Write-Host "✓ Port 5001 is available" -ForegroundColor Green
}

# Start the portal
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Starting ASP.NET Core Application" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "IMPORTANT: After the portal starts successfully:" -ForegroundColor Yellow
Write-Host "  1. Go to Cloudflare Zero Trust Dashboard" -ForegroundColor White
Write-Host "  2. Add Public Hostname: portal.shahin-ai.com → http://host.docker.internal:5001" -ForegroundColor White
Write-Host ""
Write-Host "The portal will be available at:" -ForegroundColor Green
Write-Host "  Local: http://localhost:5001" -ForegroundColor Cyan
Write-Host "  Public (after Cloudflare config): https://portal.shahin-ai.com" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Yellow
Write-Host ""

# Start the application on port 5001
dotnet run --urls "http://0.0.0.0:5001"
