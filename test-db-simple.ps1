# Simple Database Connection Test
Write-Host "Testing Railway PostgreSQL Connection..." -ForegroundColor Cyan

# Install Npgsql if needed
if (!(Get-Package Npgsql -ErrorAction SilentlyContinue)) {
    Write-Host "Installing Npgsql..." -ForegroundColor Yellow
    Install-Package Npgsql -Source NuGet.org -Force -Scope CurrentUser
}

# Connection parameters
$host = "caboose.proxy.rlwy.net"
$port = 11527
$database = "GrcMvcDb"
$username = "postgres"
$password = "QNcTvViWopMfCunsyIkkXwuDpufzhkLs"

$connectionString = "Host=$host;Port=$port;Database=$database;Username=$username;Password=$password;SSL Mode=Require;Trust Server Certificate=true"

Write-Host "`nConnection String:" -ForegroundColor Yellow
Write-Host $connectionString -ForegroundColor Gray

try {
    Add-Type -Path "$env:USERPROFILE\.nuget\packages\npgsql\*\lib\net8.0\Npgsql.dll" -ErrorAction SilentlyContinue
    
    $connection = New-Object Npgsql.NpgsqlConnection($connectionString)
    
    Write-Host "`nAttempting connection..." -ForegroundColor Yellow
    $connection.Open()
    
    Write-Host "✅ CONNECTION SUCCESSFUL!" -ForegroundColor Green
    
    # Test query
    $command = $connection.CreateCommand()
    $command.CommandText = "SELECT version(), current_database(), current_user"
    $reader = $command.ExecuteReader()
    
    if ($reader.Read()) {
        Write-Host "`nDatabase Info:" -ForegroundColor Cyan
        Write-Host "Version: $($reader[0])" -ForegroundColor White
        Write-Host "Database: $($reader[1])" -ForegroundColor White
        Write-Host "User: $($reader[2])" -ForegroundColor White
    }
    $reader.Close()
    
    # Check tables
    $command.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public'"
    $tableCount = $command.ExecuteScalar()
    Write-Host "`nPublic schema tables: $tableCount" -ForegroundColor White
    
    # List some tables
    $command.CommandText = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' LIMIT 10"
    $reader = $command.ExecuteReader()
    
    Write-Host "`nSample tables:" -ForegroundColor Cyan
    while ($reader.Read()) {
        Write-Host "  - $($reader[0])" -ForegroundColor White
    }
    $reader.Close()
    
    $connection.Close()
    
} catch {
    Write-Host "❌ CONNECTION FAILED!" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
}
