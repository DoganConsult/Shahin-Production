# Start GRC Platform in Production Mode Locally
# Serves on localhost:8080 for Cloudflare Tunnel

Write-Host "========================================" -ForegroundColor Green
Write-Host "Starting GRC Platform Production Server" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

# Set environment variables for production
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ASPNETCORE_URLS = "http://0.0.0.0:8080"
$env:ConnectionStrings__DefaultConnection = "Host=caboose.proxy.rlwy.net;Port=11527;Database=GrcMvcDb;Username=postgres;Password=QNcTvViWopMfCunsyIkkXwuDpufzhkLs;SSL Mode=Require"
$env:ConnectionStrings__GrcAuthDb = "Host=caboose.proxy.rlwy.net;Port=11527;Database=GrcMvcDb;Username=postgres;Password=QNcTvViWopMfCunsyIkkXwuDpufzhkLs;SSL Mode=Require"

Write-Host "`nEnvironment configured:" -ForegroundColor Yellow
Write-Host "- Mode: Production" -ForegroundColor White
Write-Host "- URL: http://localhost:8080" -ForegroundColor White
Write-Host "- Database: Railway PostgreSQL" -ForegroundColor White

# Navigate to publish directory
Set-Location "C:\Shahin-ai\Shahin-Jan-2026\src\GrcMvc\publish"

Write-Host "`nStarting server..." -ForegroundColor Green
Write-Host "Access locally at: http://localhost:8080" -ForegroundColor Cyan
Write-Host "Public access via Cloudflare Tunnel" -ForegroundColor Cyan
Write-Host "Press Ctrl+C to stop" -ForegroundColor Yellow

# Run the application
dotnet GrcMvc.dll
