# Phase 1 Validation Script - PowerShell Version
# Usage: .\scripts\validate-phase1.ps1

param(
    [string]$BaseUrl = "http://localhost:5000"
)

$DbContainer = "shahin-postgres"
$DbUser = "shahin_admin"
$DbName = "shahin_grc"

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Phase 1 Validation - Foundation & Security" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Base URL: $BaseUrl"
Write-Host "Database: $DbContainer / $DbUser@$DbName"
Write-Host "------------------------------------------------"

$Failed = 0
$ChecksPassed = 0
$ChecksTotal = 7

# 1. Docker check
Write-Host -NoNewline "[1/7] Docker daemon check... "
try {
    $dockerInfo = docker info 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[PASS]" -ForegroundColor Green
        $ChecksPassed++
    } else {
        Write-Host "[FAIL] Docker not reachable" -ForegroundColor Red
        Write-Host "  -> Ensure Docker Desktop is running"
        $Failed++
    }
} catch {
    Write-Host "FAIL: Docker not installed" -ForegroundColor Red
    Write-Host "  -> Install Docker Desktop for Windows"
    $Failed++
}

# 2. Container running check
Write-Host -NoNewline "[2/7] PostgreSQL container check... "
try {
    $containers = docker ps --format "{{.Names}}" 2>&1
    if ($containers -match $DbContainer) {
        Write-Host "[PASS]" -ForegroundColor Green
        $ChecksPassed++
    } else {
        Write-Host "[FAIL] Container '$DbContainer' not running" -ForegroundColor Red
        Write-Host "  -> Run: docker start $DbContainer"
        $Failed++
    }
} catch {
    Write-Host "[FAIL] Cannot list containers" -ForegroundColor Red
    $Failed++
}

# 3. Database connectivity
Write-Host -NoNewline "[3/7] Database connectivity check... "
try {
    $result = docker exec $DbContainer psql -U $DbUser -d $DbName -c "SELECT 1;" 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[PASS]" -ForegroundColor Green
        $ChecksPassed++
    } else {
        Write-Host "[FAIL] Cannot connect to database" -ForegroundColor Red
        Write-Host "  -> Check credentials: $DbUser@$DbName"
        $Failed++
    }
} catch {
    Write-Host "[FAIL] Database connection error" -ForegroundColor Red
    $Failed++
}

# 4. Identity tables check
Write-Host -NoNewline "[4/7] Identity tables check... "
try {
    $tables = docker exec $DbContainer psql -U $DbUser -d $DbName -t -c "\dt" 2>&1
    if ($tables -match "AspNetUsers") {
        Write-Host "[PASS]" -ForegroundColor Green
        $ChecksPassed++
    } else {
        Write-Host "[FAIL] Identity tables not found" -ForegroundColor Red
        Write-Host "  -> Run migrations: dotnet ef database update"
        $Failed++
    }
} catch {
    Write-Host "[SKIP] Cannot check tables (Docker not available)" -ForegroundColor Yellow
}

# 5. Migrations check
Write-Host -NoNewline "[5/7] EF Migrations check... "
try {
    $migrations = docker exec $DbContainer psql -U $DbUser -d $DbName -t -c "SELECT COUNT(*) FROM `"`__EFMigrationsHistory`"`;" 2>&1
    $count = [int]($migrations -replace '\s', '')
    if ($count -gt 0) {
        Write-Host "[PASS] $count migrations found" -ForegroundColor Green
        $ChecksPassed++
    } else {
        Write-Host "[FAIL] No migrations found" -ForegroundColor Red
        Write-Host "  -> Run: dotnet ef database update"
        $Failed++
    }
} catch {
    Write-Host "[SKIP] Cannot check migrations" -ForegroundColor Yellow
}

# 6. ApplicationUser columns check
Write-Host -NoNewline "[6/7] ApplicationUser columns check... "
$RequiredColumns = @("FirstName", "LastName", "Department", "JobTitle", "Abilities")
$MissingColumns = @()

try {
    foreach ($col in $RequiredColumns) {
        $query = "SELECT COUNT(*) FROM information_schema.columns WHERE table_name = 'AspNetUsers' AND column_name = '$col';"
        $result = docker exec $DbContainer psql -U $DbUser -d $DbName -t -c $query 2>&1
        $count = [int]($result -replace '\s', '')
        if ($count -eq 0) {
            $MissingColumns += $col
        }
    }
    
    if ($MissingColumns.Count -eq 0) {
        Write-Host "[PASS]" -ForegroundColor Green
        $ChecksPassed++
    } else {
        Write-Host "[FAIL] Missing columns: $($MissingColumns -join ', ')" -ForegroundColor Red
        Write-Host "  -> ApplicationUser schema incomplete"
        $Failed++
    }
} catch {
    Write-Host "[SKIP] Cannot check columns" -ForegroundColor Yellow
}

# 7. Application health check
Write-Host -NoNewline "[7/7] Application health check... "
try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/health" -UseBasicParsing -TimeoutSec 5
    if ($response.StatusCode -eq 200) {
        Write-Host "[PASS]" -ForegroundColor Green
        $ChecksPassed++
        
        # Check additional endpoints
        Write-Host "  Checking additional endpoints:"
        @("/health/ready", "/health/live") | ForEach-Object {
            Write-Host -NoNewline "    ${_}: "
            try {
                $resp = Invoke-WebRequest -Uri "$BaseUrl$_" -UseBasicParsing -TimeoutSec 2
                Write-Host "OK" -ForegroundColor Green
            } catch {
                Write-Host "not available" -ForegroundColor Yellow
            }
        }
    } else {
        Write-Host "[FAIL] Application not healthy" -ForegroundColor Red
        $Failed++
    }
} catch {
    Write-Host "[FAIL] Application not responding" -ForegroundColor Red
    Write-Host "  -> Start app: dotnet run --project src/GrcMvc/GrcMvc.csproj"
    Write-Host "  -> Base URL: $BaseUrl"
    $Failed++
}

# Summary
Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "VALIDATION SUMMARY" -ForegroundColor Cyan
Write-Host "------------------------------------------------"
Write-Host "Checks Passed: $ChecksPassed / $ChecksTotal"

if ($Failed -eq 0) {
    Write-Host "[SUCCESS] PHASE 1 READY FOR PRODUCTION" -ForegroundColor Green
    Write-Host "All foundation and security prerequisites met!"
    exit 0
} else {
    Write-Host "[FAILURE] PHASE 1 NOT READY" -ForegroundColor Red
    Write-Host "$Failed critical check(s) failed. Fix issues above."
    exit 1
}
