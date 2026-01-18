# Quick Database Connection Test
# Tests the connection string configuration and actual database connection

param(
    [string]$ConnectionString = ""
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Database Connection Test" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Check current configuration
Write-Host "Step 1: Checking Connection String Configuration..." -ForegroundColor Yellow
Write-Host ""

$foundConnection = $false
$connectionString = ""

# Check environment variables
$envVars = @(
    "ConnectionStrings__DefaultConnection",
    "CONNECTION_STRING",
    "DATABASE_URL"
)

foreach ($varName in $envVars) {
    $value = [Environment]::GetEnvironmentVariable($varName)
    if ($value) {
        Write-Host "  ✅ Found: $varName" -ForegroundColor Green
        if ($varName -eq "DATABASE_URL") {
            Write-Host "     Format: Railway/PostgreSQL URL" -ForegroundColor Gray
            # Convert Railway format if needed
            if ($value -match "^postgresql://") {
                Write-Host "     ℹ️  Will be auto-converted to PostgreSQL connection string" -ForegroundColor Cyan
            }
        }
        $connectionString = $value
        $foundConnection = $true
        break
    } else {
        Write-Host "  ❌ Not set: $varName" -ForegroundColor Red
    }
}

# Check appsettings.json
if (-not $foundConnection) {
    $appsettingsPath = "src\GrcMvc\appsettings.json"
    if (Test-Path $appsettingsPath) {
        $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
        if ($appsettings.ConnectionStrings.DefaultConnection -and 
            $appsettings.ConnectionStrings.DefaultConnection.Trim() -ne "") {
            Write-Host "  ✅ Found in appsettings.json" -ForegroundColor Green
            $connectionString = $appsettings.ConnectionStrings.DefaultConnection
            $foundConnection = $true
        } else {
            Write-Host "  ❌ appsettings.json: ConnectionStrings.DefaultConnection is empty" -ForegroundColor Red
        }
    }
}

Write-Host ""

# If connection string provided as parameter, use it
if ($ConnectionString -and $ConnectionString.Trim() -ne "") {
    $connectionString = $ConnectionString
    $foundConnection = $true
    Write-Host "  ✅ Using provided connection string" -ForegroundColor Green
    Write-Host ""
}

# Step 2: If no connection string found, show instructions
if (-not $foundConnection) {
    Write-Host "❌ No connection string configured!" -ForegroundColor Red
    Write-Host ""
    Write-Host "To test the connection, set one of the following:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Option 1: Set environment variable (PowerShell)" -ForegroundColor Cyan
    Write-Host '  $env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=postgres"' -ForegroundColor Gray
    Write-Host ""
    Write-Host "Option 2: Set environment variable (Railway format)" -ForegroundColor Cyan
    Write-Host '  $env:DATABASE_URL = "postgresql://postgres:password@host.railway.app:5432/railway"' -ForegroundColor Gray
    Write-Host ""
    Write-Host "Option 3: Test with parameter" -ForegroundColor Cyan
    Write-Host '  .\test-connection-now.ps1 -ConnectionString "Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=postgres"' -ForegroundColor Gray
    Write-Host ""
    Write-Host "Then run this script again or use:" -ForegroundColor Yellow
    Write-Host "  cd src\GrcMvc" -ForegroundColor Gray
    Write-Host "  dotnet run -- TestDb" -ForegroundColor Gray
    Write-Host ""
    exit 1
}

# Step 3: Validate connection string format
Write-Host "Step 2: Validating Connection String Format..." -ForegroundColor Yellow
Write-Host ""

# Mask password for display
$maskedConn = $connectionString
if ($maskedConn -match "Password=([^;]+)") {
    $maskedConn = $maskedConn -replace "Password=[^;]+", "Password=***"
}
if ($maskedConn -match "Pwd=([^;]+)") {
    $maskedConn = $maskedConn -replace "Pwd=[^;]+", "Pwd=***"
}

Write-Host "  Connection String: $maskedConn" -ForegroundColor White
Write-Host ""

# Parse connection string
$connParts = @{}
$parts = $connectionString -split ';'
foreach ($part in $parts) {
    if ($part -match "^([^=]+)=(.*)$") {
        $key = $matches[1].Trim()
        $value = $matches[2].Trim()
        $connParts[$key] = $value
    }
}

# Check required fields
$required = @("Host", "Database")
$missing = @()
foreach ($req in $required) {
    if (-not $connParts.ContainsKey($req)) {
        $missing += $req
    }
}

if ($missing.Count -gt 0) {
    Write-Host "  ❌ Missing required fields: $($missing -join ', ')" -ForegroundColor Red
    Write-Host ""
    exit 1
} else {
    Write-Host "  ✅ Connection string format valid" -ForegroundColor Green
    Write-Host "     Host: $($connParts['Host'])" -ForegroundColor Gray
    Write-Host "     Database: $($connParts['Database'])" -ForegroundColor Gray
    if ($connParts.ContainsKey('Port')) {
        Write-Host "     Port: $($connParts['Port'])" -ForegroundColor Gray
    }
    if ($connParts.ContainsKey('Username') -or $connParts.ContainsKey('User Id')) {
        $user = if ($connParts.ContainsKey('Username')) { $connParts['Username'] } else { $connParts['User Id'] }
        Write-Host "     Username: $user" -ForegroundColor Gray
    }
    Write-Host ""
}

# Step 4: Test actual connection (if Npgsql available)
Write-Host "Step 3: Testing Database Connection..." -ForegroundColor Yellow
Write-Host ""

# Try to find Npgsql.dll
$npgsqlPaths = @(
    "src\GrcMvc\bin\Debug\net8.0\Npgsql.dll",
    "src\GrcMvc\bin\Release\net8.0\Npgsql.dll",
    "src\GrcMvc\bin\Debug\net7.0\Npgsql.dll"
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
        Write-Host "  Loading Npgsql from: $npgsqlPath" -ForegroundColor Gray
        Add-Type -Path $npgsqlPath
        
        # Convert Railway URL if needed
        if ($connectionString -match "^postgresql://") {
            Write-Host "  Converting Railway DATABASE_URL format..." -ForegroundColor Gray
            $uri = [System.Uri]::new($connectionString)
            $userInfo = $uri.UserInfo -split ':'
            if ($userInfo.Length -eq 2) {
                $decodedUser = [System.Uri]::UnescapeDataString($userInfo[0])
                $decodedPass = [System.Uri]::UnescapeDataString($userInfo[1])
                $dbName = $uri.LocalPath.TrimStart('/')
                $connectionString = "Host=$($uri.Host);Database=$dbName;Username=$decodedUser;Password=$decodedPass;Port=$($uri.Port)"
                Write-Host "  ✅ Converted to PostgreSQL connection string" -ForegroundColor Green
            }
        }
        
        Write-Host "  Attempting connection..." -ForegroundColor Gray
        $conn = New-Object Npgsql.NpgsqlConnection($connectionString)
        $conn.Open()
        
        # Test query
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = "SELECT version(), current_database(), current_user"
        $reader = $cmd.ExecuteReader()
        
        if ($reader.Read()) {
            $version = $reader.GetString(0)
            $database = $reader.GetString(1)
            $user = $reader.GetString(2)
            
            Write-Host ""
            Write-Host "  ✅ Connection Successful!" -ForegroundColor Green
            Write-Host ""
            Write-Host "  Database Information:" -ForegroundColor Cyan
            Write-Host "    Database: $database" -ForegroundColor White
            Write-Host "    User: $user" -ForegroundColor White
            Write-Host "    PostgreSQL: $($version.Substring(0, [Math]::Min(60, $version.Length)))..." -ForegroundColor White
            Write-Host ""
            
            $reader.Close()
            $conn.Close()
            
            Write-Host "✅ All tests passed!" -ForegroundColor Green
            Write-Host ""
            exit 0
        }
        
        $reader.Close()
        $conn.Close()
        
    } catch {
        Write-Host ""
        Write-Host "  ❌ Connection Failed!" -ForegroundColor Red
        Write-Host "     Error: $($_.Exception.Message)" -ForegroundColor White
        Write-Host ""
        Write-Host "  Troubleshooting:" -ForegroundColor Yellow
        Write-Host "    1. Verify PostgreSQL server is running" -ForegroundColor Gray
        Write-Host "    2. Check host/port is correct" -ForegroundColor Gray
        Write-Host "    3. Verify database exists" -ForegroundColor Gray
        Write-Host "    4. Check user credentials" -ForegroundColor Gray
        Write-Host "    5. Verify firewall/network settings" -ForegroundColor Gray
        Write-Host ""
        exit 1
    }
} else {
    Write-Host "  ⚠️  Npgsql.dll not found. Cannot test actual connection." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  To test actual connection:" -ForegroundColor Yellow
    Write-Host "    1. Build the project: cd src\GrcMvc && dotnet build" -ForegroundColor Gray
    Write-Host "    2. Run test: dotnet run -- TestDb" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  Or set connection string and run:" -ForegroundColor Yellow
    Write-Host "    cd src\GrcMvc" -ForegroundColor Gray
    Write-Host "    dotnet run -- TestDb" -ForegroundColor Gray
    Write-Host ""
    Write-Host "✅ Connection string format is valid!" -ForegroundColor Green
    Write-Host ""
    exit 0
}
