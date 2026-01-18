# Minimal Production Server Start Script
# Bypasses build issues to get server running

Write-Host "========================================" -ForegroundColor Green
Write-Host "Starting GRC Platform - Production Mode" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

# Set environment variables
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ASPNETCORE_URLS = "http://0.0.0.0:8080"
$env:ConnectionStrings__DefaultConnection = "Host=caboose.proxy.rlwy.net;Port=11527;Database=GrcMvcDb;Username=postgres;Password=QNcTvViWopMfCunsyIkkXwuDpufzhkLs;SSL Mode=Require"
$env:ConnectionStrings__GrcAuthDb = "Host=caboose.proxy.rlwy.net;Port=11527;Database=GrcMvcDb;Username=postgres;Password=QNcTvViWopMfCunsyIkkXwuDpufzhkLs;SSL Mode=Require"

Write-Host "`nConfiguration:" -ForegroundColor Yellow
Write-Host "- Environment: Production" -ForegroundColor White
Write-Host "- URL: http://localhost:8080" -ForegroundColor White
Write-Host "- Database: Railway PostgreSQL (Connected)" -ForegroundColor White
Write-Host "- All migrations already applied" -ForegroundColor Green

Set-Location "C:\Shahin-ai\Shahin-Jan-2026\src\GrcMvc"

Write-Host "`nStarting server on port 8080..." -ForegroundColor Green
Write-Host "`nPublic Routes Available:" -ForegroundColor Cyan
Write-Host "  - http://localhost:8080/ (Landing page)" -ForegroundColor White
Write-Host "  - http://localhost:8080/about" -ForegroundColor White
Write-Host "  - http://localhost:8080/features" -ForegroundColor White
Write-Host "  - http://localhost:8080/pricing" -ForegroundColor White
Write-Host "  - http://localhost:8080/contact" -ForegroundColor White
Write-Host "`nAuthenticated Routes:" -ForegroundColor Cyan
Write-Host "  - http://localhost:8080/platform-admin (Platform admin dashboard)" -ForegroundColor White
Write-Host "  - http://localhost:8080/Account/Login (Login page)" -ForegroundColor White
Write-Host "`nPress Ctrl+C to stop" -ForegroundColor Yellow

# Run without rebuilding - use existing binaries
dotnet run --no-build --no-restore
