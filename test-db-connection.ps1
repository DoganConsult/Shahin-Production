# Database Connection Test Script
Write-Host "========================================" -ForegroundColor Green
Write-Host "Testing Database Connection" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

# PostgreSQL connection string
$connectionString = "Host=caboose.proxy.rlwy.net;Port=11527;Database=GrcMvcDb;Username=postgres;Password=QNcTvViWopMfCunsyIkkXwuDpufzhkLs;SSL Mode=Require"

Write-Host "`nConnection Details:" -ForegroundColor Yellow
Write-Host "Host: caboose.proxy.rlwy.net" -ForegroundColor White
Write-Host "Port: 11527" -ForegroundColor White
Write-Host "Database: GrcMvcDb" -ForegroundColor White
Write-Host "SSL: Required" -ForegroundColor White

# Test using psql command if available
Write-Host "`n1. Testing with psql command..." -ForegroundColor Cyan
$env:PGPASSWORD = "QNcTvViWopMfCunsyIkkXwuDpufzhkLs"
psql -h caboose.proxy.rlwy.net -p 11527 -U postgres -d GrcMvcDb -c "SELECT version();" 2>$null

if ($LASTEXITCODE -ne 0) {
    Write-Host "psql not available or connection failed" -ForegroundColor Yellow
}

# Test using dotnet tool
Write-Host "`n2. Testing with Entity Framework..." -ForegroundColor Cyan
Set-Location "C:\Shahin-ai\Shahin-Jan-2026\src\GrcMvc"

# Set connection strings as environment variables
$env:ConnectionStrings__DefaultConnection = $connectionString
$env:ConnectionStrings__GrcAuthDb = $connectionString

# Run EF database connection test
dotnet ef database connection test --context GrcDbContext 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ GrcDbContext connection successful!" -ForegroundColor Green
} else {
    Write-Host "❌ GrcDbContext connection failed" -ForegroundColor Red
}

dotnet ef database connection test --context GrcAuthDbContext 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ GrcAuthDbContext connection successful!" -ForegroundColor Green
} else {
    Write-Host "❌ GrcAuthDbContext connection failed" -ForegroundColor Red
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "Connection test complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
