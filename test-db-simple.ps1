# Simple Database Connection Test
# Tests PostgreSQL connection using Npgsql

param(
    [string]$ConnectionString = ""
)

$ErrorActionPreference = "Continue"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Database Connection Test" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if connection string is provided
if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    Write-Host "Usage: .\test-db-simple.ps1 -ConnectionString 'Host=localhost;Port=5432;Database=...;Username=...;Password=...'" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Or set environment variable:" -ForegroundColor Yellow
    Write-Host "  `$env:ConnectionStrings__DefaultConnection = 'Host=...;Database=...;Username=...;Password=...'" -ForegroundColor Gray
    Write-Host ""
    
    # Try to get from environment
    $ConnectionString = [Environment]::GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
        $ConnectionString = [Environment]::GetEnvironmentVariable("CONNECTION_STRING")
    }
    
    if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
        Write-Host "❌ No connection string provided or found in environment variables" -ForegroundColor Red
        Write-Host ""
        Write-Host "Please provide connection string:" -ForegroundColor Yellow
        Write-Host "  .\test-db-simple.ps1 -ConnectionString 'Host=localhost;Port=5432;Database=grcmvc;Username=postgres;Password=...'" -ForegroundColor Gray
        exit 1
    }
}

Write-Host "Testing connection..." -ForegroundColor Yellow
Write-Host "  Connection String: $(MaskConnectionString $ConnectionString)" -ForegroundColor Gray
Write-Host ""

# Parse connection string
$dbInfo = ParseConnectionString $ConnectionString
if ($dbInfo) {
    Write-Host "Connection Details:" -ForegroundColor Cyan
    Write-Host "  Host: $($dbInfo.Host)" -ForegroundColor White
    Write-Host "  Port: $($dbInfo.Port)" -ForegroundColor White
    Write-Host "  Database: $($dbInfo.Database)" -ForegroundColor White
    Write-Host "  User: $($dbInfo.User)" -ForegroundColor White
    Write-Host ""
}

# Test connection using .NET
Write-Host "Attempting connection..." -ForegroundColor Yellow

try {
    # Load Npgsql assembly (if available)
    $npgsqlPath = "Shahin-Jan-2026\src\GrcMvc\bin\Debug\net8.0\Npgsql.dll"
    if (Test-Path $npgsqlPath) {
        Add-Type -Path $npgsqlPath
    } else {
        Write-Host "⚠️  Npgsql.dll not found. Testing connection string format only." -ForegroundColor Yellow
        Write-Host ""
        Write-Host "To test actual connection, run the application:" -ForegroundColor Yellow
        Write-Host "  cd Shahin-Jan-2026\src\GrcMvc" -ForegroundColor Gray
        Write-Host "  dotnet run" -ForegroundColor Gray
        Write-Host ""
        Write-Host "Or build and run:" -ForegroundColor Yellow
        Write-Host "  dotnet build" -ForegroundColor Gray
        Write-Host "  dotnet run" -ForegroundColor Gray
        exit 0
    }
    
    # Create connection
    $conn = New-Object Npgsql.NpgsqlConnection($ConnectionString)
    $conn.Open()
    
    # Test query
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT version(), current_database(), current_user"
    $reader = $cmd.ExecuteReader()
    
    if ($reader.Read()) {
        $version = $reader.GetString(0)
        $database = $reader.GetString(1)
        $user = $reader.GetString(2)
        
        Write-Host "✅ Connection Successful!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Database Information:" -ForegroundColor Cyan
        Write-Host "  PostgreSQL Version: $version" -ForegroundColor White
        Write-Host "  Current Database: $database" -ForegroundColor White
        Write-Host "  Current User: $user" -ForegroundColor White
        Write-Host ""
        
        $reader.Close()
        $conn.Close()
        
        Write-Host "✅ Database connection test passed!" -ForegroundColor Green
        exit 0
    } else {
        Write-Host "❌ Connection opened but query returned no results" -ForegroundColor Red
        $conn.Close()
        exit 1
    }
    
} catch {
    Write-Host "❌ Connection Failed!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Error Details:" -ForegroundColor Yellow
    Write-Host "  Message: $($_.Exception.Message)" -ForegroundColor White
    
    if ($_.Exception.InnerException) {
        Write-Host "  Inner Exception: $($_.Exception.InnerException.Message)" -ForegroundColor White
    }
    
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "  1. Verify PostgreSQL server is running" -ForegroundColor Gray
    Write-Host "  2. Check connection string format" -ForegroundColor Gray
    Write-Host "  3. Verify database exists" -ForegroundColor Gray
    Write-Host "  4. Check user credentials and permissions" -ForegroundColor Gray
    Write-Host "  5. Verify firewall/network settings" -ForegroundColor Gray
    
    exit 1
}

function MaskConnectionString($connString) {
    if ([string]::IsNullOrWhiteSpace($connString)) {
        return "(empty)"
    }
    
    try {
        $parts = $connString -split ';'
        $masked = @()
        foreach ($part in $parts) {
            if ($part -match '^Password=(.+)$') {
                $masked += "Password=***"
            } elseif ($part -match '^Pwd=(.+)$') {
                $masked += "Pwd=***"
            } else {
                $masked += $part
            }
        }
        return ($masked -join ';')
    } catch {
        return "(error parsing)"
    }
}

function ParseConnectionString($connString) {
    try {
        $result = @{}
        $parts = $connString -split ';'
        foreach ($part in $parts) {
            if ($part -match '^Host=(.+)$') {
                $result.Host = $matches[1]
            } elseif ($part -match '^Port=(\d+)$') {
                $result.Port = $matches[1]
            } elseif ($part -match '^Database=(.+)$') {
                $result.Database = $matches[1]
            } elseif ($part -match '^Username=(.+)$' -or $part -match '^User Id=(.+)$') {
                $result.User = $matches[1]
            }
        }
        return $result
    } catch {
        return $null
    }
}
