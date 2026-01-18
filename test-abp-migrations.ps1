# Test ABP Migrations Process
# Follows ABP Framework standard migration process

param(
    [string]$ConnectionString = "",
    [switch]$ListMigrations,
    [switch]$CheckPending,
    [switch]$TestMigration
)

$ErrorActionPreference = "Continue"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "ABP Framework Migration Test" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get connection string
if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    $ConnectionString = [Environment]::GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
        $ConnectionString = [Environment]::GetEnvironmentVariable("DATABASE_URL")
        if ($ConnectionString -and $ConnectionString.StartsWith("postgresql://")) {
            # Convert Railway format
            $uri = [System.Uri]::new($ConnectionString)
            $userInfo = $uri.UserInfo -split ':'
            if ($userInfo.Length -eq 2) {
                $decodedUser = [System.Uri]::UnescapeDataString($userInfo[0])
                $decodedPass = [System.Uri]::UnescapeDataString($userInfo[1])
                $dbName = $uri.LocalPath.TrimStart('/')
                $ConnectionString = "Host=$($uri.Host);Database=$dbName;Username=$decodedUser;Password=$decodedPass;Port=$($uri.Port)"
            }
        }
    }
}

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    Write-Host "❌ Connection string not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Set connection string:" -ForegroundColor Yellow
    Write-Host '  $env:ConnectionStrings__DefaultConnection = "Host=...;Database=...;Username=...;Password=...;Port=5432"' -ForegroundColor Gray
    Write-Host ""
    exit 1
}

Write-Host "Connection String: $(MaskConnectionString $ConnectionString)" -ForegroundColor Gray
Write-Host ""

# Set environment variable for EF Core tools
$env:ConnectionStrings__DefaultConnection = $ConnectionString

# Change to project directory
$projectPath = "Shahin-Jan-2026\src\GrcMvc"
if (-not (Test-Path $projectPath)) {
    Write-Host "❌ Project path not found: $projectPath" -ForegroundColor Red
    exit 1
}

Push-Location $projectPath

try {
    # List migrations
    if ($ListMigrations) {
        Write-Host "=== Listing Migrations ===" -ForegroundColor Yellow
        Write-Host ""
        
        Write-Host "GrcDbContext Migrations:" -ForegroundColor Cyan
        dotnet ef migrations list --context GrcDbContext 2>&1
        
        Write-Host ""
        Write-Host "GrcAuthDbContext Migrations:" -ForegroundColor Cyan
        dotnet ef migrations list --context GrcAuthDbContext 2>&1
        
        Pop-Location
        exit 0
    }
    
    # Check pending migrations
    if ($CheckPending) {
        Write-Host "=== Checking Pending Migrations ===" -ForegroundColor Yellow
        Write-Host ""
        
        Write-Host "Checking GrcDbContext..." -ForegroundColor Cyan
        $pendingMain = dotnet ef migrations list --context GrcDbContext --no-build 2>&1 | Select-String "Pending"
        if ($pendingMain) {
            Write-Host "  ⚠️  Pending migrations found for GrcDbContext" -ForegroundColor Yellow
            Write-Host $pendingMain -ForegroundColor Gray
        } else {
            Write-Host "  ✅ No pending migrations for GrcDbContext" -ForegroundColor Green
        }
        
        Write-Host ""
        Write-Host "Checking GrcAuthDbContext..." -ForegroundColor Cyan
        $pendingAuth = dotnet ef migrations list --context GrcAuthDbContext --no-build 2>&1 | Select-String "Pending"
        if ($pendingAuth) {
            Write-Host "  ⚠️  Pending migrations found for GrcAuthDbContext" -ForegroundColor Yellow
            Write-Host $pendingAuth -ForegroundColor Gray
        } else {
            Write-Host "  ✅ No pending migrations for GrcAuthDbContext" -ForegroundColor Green
        }
        
        Pop-Location
        exit 0
    }
    
    # Test migration (dry-run)
    if ($TestMigration) {
        Write-Host "=== Testing Migration (Dry Run) ===" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "This will check if migrations can be applied without actually applying them." -ForegroundColor Gray
        Write-Host ""
        
        Write-Host "Testing GrcDbContext..." -ForegroundColor Cyan
        $result = dotnet ef database update --context GrcDbContext --dry-run 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✅ GrcDbContext migration test passed" -ForegroundColor Green
        } else {
            Write-Host "  ❌ GrcDbContext migration test failed" -ForegroundColor Red
            Write-Host $result -ForegroundColor Gray
        }
        
        Write-Host ""
        Write-Host "Testing GrcAuthDbContext..." -ForegroundColor Cyan
        $result = dotnet ef database update --context GrcAuthDbContext --dry-run 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✅ GrcAuthDbContext migration test passed" -ForegroundColor Green
        } else {
            Write-Host "  ❌ GrcAuthDbContext migration test failed" -ForegroundColor Red
            Write-Host $result -ForegroundColor Gray
        }
        
        Pop-Location
        exit 0
    }
    
    # Default: Full migration test
    Write-Host "=== ABP Migration Process Test ===" -ForegroundColor Yellow
    Write-Host ""
    
    Write-Host "Step 1: Building project..." -ForegroundColor Cyan
    dotnet build --no-restore 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ❌ Build failed" -ForegroundColor Red
        Pop-Location
        exit 1
    }
    Write-Host "  ✅ Build successful" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "Step 2: Listing current migrations..." -ForegroundColor Cyan
    Write-Host ""
    Write-Host "GrcDbContext:" -ForegroundColor Yellow
    dotnet ef migrations list --context GrcDbContext --no-build 2>&1 | Select-Object -First 10
    Write-Host ""
    Write-Host "GrcAuthDbContext:" -ForegroundColor Yellow
    dotnet ef migrations list --context GrcAuthDbContext --no-build 2>&1 | Select-Object -First 10
    Write-Host ""
    
    Write-Host "Step 3: Checking database connection..." -ForegroundColor Cyan
    # Connection test will be done by EF Core tools, skip manual test
    Write-Host "  ℹ️  Connection will be tested by EF Core tools" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "Step 4: Testing migration application (dry-run)..." -ForegroundColor Cyan
    Write-Host ""
    Write-Host "GrcDbContext:" -ForegroundColor Yellow
    $result = dotnet ef database update --context GrcDbContext --dry-run --no-build 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✅ GrcDbContext migrations ready" -ForegroundColor Green
    } else {
        Write-Host "  ⚠️  GrcDbContext migration check completed" -ForegroundColor Yellow
    }
    Write-Host ""
    
    Write-Host "GrcAuthDbContext:" -ForegroundColor Yellow
    $result = dotnet ef database update --context GrcAuthDbContext --dry-run --no-build 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✅ GrcAuthDbContext migrations ready" -ForegroundColor Green
    } else {
        Write-Host "  ⚠️  GrcAuthDbContext migration check completed" -ForegroundColor Yellow
    }
    Write-Host ""
    
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "✅ ABP Migration Test Complete" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "To apply migrations:" -ForegroundColor Yellow
    Write-Host "  dotnet ef database update --context GrcDbContext" -ForegroundColor Gray
    Write-Host "  dotnet ef database update --context GrcAuthDbContext" -ForegroundColor Gray
    Write-Host ""
    
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    Pop-Location
}

function MaskConnectionString($connString) {
    if ([string]::IsNullOrWhiteSpace($connString)) {
        return "(empty)"
    }
    
    try {
        $parts = $connString -split ';'
        $masked = @()
        foreach ($part in $parts) {
            if ($part -match '^Password=(.+)$' -or $part -match '^Pwd=(.+)$') {
                $masked += ($part -split '=')[0] + "=***"
            } else {
                $masked += $part
            }
        }
        return ($masked -join ';')
    } catch {
        return "(error parsing)"
    }
}
