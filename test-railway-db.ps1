# Test Railway Database Connection
# Specifically tests Railway DATABASE_URL format and conversion

param(
    [string]$DatabaseUrl = ""
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Railway Database Connection Test" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Get Railway DATABASE_URL
$railwayUrl = $DatabaseUrl
if ([string]::IsNullOrWhiteSpace($railwayUrl)) {
    $railwayUrl = [Environment]::GetEnvironmentVariable('DATABASE_URL')
}

if ([string]::IsNullOrWhiteSpace($railwayUrl)) {
    Write-Host "❌ DATABASE_URL not set!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Railway automatically sets DATABASE_URL when you deploy." -ForegroundColor Yellow
    Write-Host "To test locally, set it manually:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host '  $env:DATABASE_URL = "postgresql://postgres:password@host.railway.app:5432/railway"' -ForegroundColor Gray
    Write-Host ""
    Write-Host "Or provide it as parameter:" -ForegroundColor Yellow
    Write-Host '  .\test-railway-db.ps1 -DatabaseUrl "postgresql://postgres:password@host.railway.app:5432/railway"' -ForegroundColor Gray
    Write-Host ""
    exit 1
}

Write-Host "Step 1: Railway DATABASE_URL Found" -ForegroundColor Green
Write-Host "  Format: $($railwayUrl.Substring(0, [Math]::Min(70, $railwayUrl.Length)))..." -ForegroundColor Gray
Write-Host ""

# Step 2: Validate Railway URL Format
Write-Host "Step 2: Validating Railway URL Format..." -ForegroundColor Yellow

if (-not $railwayUrl.StartsWith("postgresql://")) {
    Write-Host "  ❌ Invalid format! Must start with 'postgresql://'" -ForegroundColor Red
    Write-Host "  Expected: postgresql://user:pass@host:port/database" -ForegroundColor Gray
    exit 1
}

Write-Host "  ✅ Format valid (postgresql://)" -ForegroundColor Green
Write-Host ""

# Step 3: Convert Railway URL to Connection String
Write-Host "Step 3: Converting Railway URL to PostgreSQL Connection String..." -ForegroundColor Yellow

try {
    $uri = [System.Uri]::new($railwayUrl)
    
    # Parse user info
    $userInfo = $uri.UserInfo -split ':'
    if ($userInfo.Length -ne 2) {
        Write-Host "  ❌ Invalid user info format" -ForegroundColor Red
        exit 1
    }
    
    $decodedUser = [System.Uri]::UnescapeDataString($userInfo[0])
    $decodedPass = [System.Uri]::UnescapeDataString($userInfo[1])
    $dbName = $uri.LocalPath.TrimStart('/')
    
    # Build connection string
    $connectionString = "Host=$($uri.Host);Database=$dbName;Username=$decodedUser;Password=$decodedPass;Port=$($uri.Port)"
    
    Write-Host "  ✅ Conversion successful!" -ForegroundColor Green
    Write-Host ""
    Write-Host "  Connection Details:" -ForegroundColor Cyan
    Write-Host "    Host: $($uri.Host)" -ForegroundColor White
    Write-Host "    Port: $($uri.Port)" -ForegroundColor White
    Write-Host "    Database: $dbName" -ForegroundColor White
    Write-Host "    Username: $decodedUser" -ForegroundColor White
    Write-Host "    Password: ***" -ForegroundColor White
    Write-Host ""
    Write-Host "  Connection String:" -ForegroundColor Cyan
    Write-Host "    Host=$($uri.Host);Database=$dbName;Username=$decodedUser;Password=***;Port=$($uri.Port)" -ForegroundColor Gray
    Write-Host ""
    
} catch {
    Write-Host "  ❌ Failed to parse DATABASE_URL" -ForegroundColor Red
    Write-Host "     Error: $($_.Exception.Message)" -ForegroundColor White
    Write-Host ""
    exit 1
}

# Step 4: Test Actual Connection
Write-Host "Step 4: Testing Database Connection..." -ForegroundColor Yellow

# Try to find Npgsql.dll
$npgsqlPaths = @(
    "Shahin-Jan-2026\src\GrcMvc\bin\Debug\net8.0\Npgsql.dll",
    "Shahin-Jan-2026\src\GrcMvc\bin\Release\net8.0\Npgsql.dll"
)

$npgsqlPath = $null
foreach ($path in $npgsqlPaths) {
    if (Test-Path $path) {
        $npgsqlPath = $path
        break
    }
}

if ($npgsqlPath) {
    try {
        Write-Host "  Loading Npgsql..." -ForegroundColor Gray
        Add-Type -Path $npgsqlPath
        
        Write-Host "  Connecting to Railway database..." -ForegroundColor Gray
        $conn = New-Object Npgsql.NpgsqlConnection($connectionString)
        $conn.Open()
        
        # Test query
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = "SELECT version(), current_database(), current_user, inet_server_addr(), inet_server_port()"
        $reader = $cmd.ExecuteReader()
        
        if ($reader.Read()) {
            $version = $reader.GetString(0)
            $database = $reader.GetString(1)
            $user = $reader.GetString(2)
            $serverAddr = if (-not $reader.IsDBNull(3)) { $reader.GetString(3) } else { "N/A" }
            $serverPort = if (-not $reader.IsDBNull(4)) { $reader.GetInt32(4) } else { "N/A" }
            
            Write-Host ""
            Write-Host "  ✅ Connection Successful!" -ForegroundColor Green
            Write-Host ""
            Write-Host "  Database Information:" -ForegroundColor Cyan
            Write-Host "    Database: $database" -ForegroundColor White
            Write-Host "    User: $user" -ForegroundColor White
            Write-Host "    Server Address: $serverAddr" -ForegroundColor White
            Write-Host "    Server Port: $serverPort" -ForegroundColor White
            Write-Host "    PostgreSQL: $($version.Substring(0, [Math]::Min(60, $version.Length)))..." -ForegroundColor White
            Write-Host ""
            
            # Count tables
            $reader.Close()
            $tableCmd = $conn.CreateCommand()
            $tableCmd.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE'"
            $tableCount = $tableCmd.ExecuteScalar()
            
            Write-Host "  Database Statistics:" -ForegroundColor Cyan
            Write-Host "    Tables: $tableCount" -ForegroundColor White
            Write-Host ""
            
            $conn.Close()
            
            Write-Host "========================================" -ForegroundColor Cyan
            Write-Host "✅ Railway Database Test PASSED!" -ForegroundColor Green
            Write-Host "========================================" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "Your Railway database connection is working correctly!" -ForegroundColor Green
            Write-Host ""
            exit 0
            
        } else {
            Write-Host "  ❌ Connection opened but query returned no results" -ForegroundColor Red
            $conn.Close()
            exit 1
        }
        
    } catch {
        Write-Host ""
        Write-Host "  ❌ Connection Failed!" -ForegroundColor Red
        Write-Host "     Error: $($_.Exception.Message)" -ForegroundColor White
        if ($_.Exception.InnerException) {
            Write-Host "     Inner: $($_.Exception.InnerException.Message)" -ForegroundColor White
        }
        Write-Host ""
        Write-Host "  Troubleshooting:" -ForegroundColor Yellow
        Write-Host "    1. Verify Railway database is running" -ForegroundColor Gray
        Write-Host "    2. Check DATABASE_URL is correct" -ForegroundColor Gray
        Write-Host "    3. Verify network connectivity to Railway" -ForegroundColor Gray
        Write-Host "    4. Check Railway service status in dashboard" -ForegroundColor Gray
        Write-Host ""
        exit 1
    }
} else {
    Write-Host "  ⚠️  Npgsql.dll not found. Cannot test actual connection." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  To test actual connection:" -ForegroundColor Yellow
    Write-Host "    1. Build the project: cd Shahin-Jan-2026\src\GrcMvc && dotnet build" -ForegroundColor Gray
    Write-Host "    2. Run test: dotnet run -- TestDb" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  Or set DATABASE_URL and run:" -ForegroundColor Yellow
    Write-Host "    cd Shahin-Jan-2026\src\GrcMvc" -ForegroundColor Gray
    Write-Host "    dotnet run -- TestDb" -ForegroundColor Gray
    Write-Host ""
    Write-Host "✅ Railway URL conversion is working!" -ForegroundColor Green
    Write-Host ""
    exit 0
}
