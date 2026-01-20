# Query UpCloud PostgreSQL Database (assumes tunnel is running)
param(
    [string]$Host = "127.0.0.1",
    [int]$Port = 5433,
    [string]$Database = "GrcMvcDb",
    [string]$User = "postgres",
    [string]$Password = ""
)

Write-Host "🔍 Querying UpCloud PostgreSQL Database" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "Host: $Host`:$Port" -ForegroundColor Yellow
Write-Host "Database: $Database" -ForegroundColor Yellow
Write-Host ""

# Check if SSH tunnel is running
Write-Host "⏳ Checking SSH tunnel..." -ForegroundColor Cyan
$sshProcess = Get-Process ssh -ErrorAction SilentlyContinue
if ($sshProcess) {
    Write-Host "✅ SSH tunnel is running (PID: $($sshProcess.Id))" -ForegroundColor Green
} else {
    Write-Host "❌ SSH tunnel not found. Start it with: .\tunnel-upcloud-db.ps1" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Try with psql if available
if (Get-Command psql -ErrorAction SilentlyContinue) {
    Write-Host "📝 Using psql to query database..." -ForegroundColor Cyan
    Write-Host ""
    
    $env:PGPASSWORD = $Password
    psql -h $Host -p $Port -U $User -d $Database -c "SELECT version(); SELECT COUNT(*) as table_count FROM information_schema.tables WHERE table_schema='public';"
    $result = $LASTEXITCODE
    Remove-Item env:PGPASSWORD -ErrorAction SilentlyContinue
    
    if ($result -eq 0) {
        Write-Host ""
        Write-Host "✅ Database query successful!" -ForegroundColor Green
    } else {
        Write-Host "❌ Query failed (exit code: $result)" -ForegroundColor Red
    }
} else {
    Write-Host "⚠️  psql not found. Install PostgreSQL client tools." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Download from: https://www.postgresql.org/download/" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Or use connection string in your application:" -ForegroundColor Cyan
    Write-Host "  Host=$Host;Port=$Port;Database=$Database;Username=$User" -ForegroundColor Yellow
}
