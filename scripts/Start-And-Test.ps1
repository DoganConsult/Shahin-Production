# Start-And-Test.ps1
# Starts infrastructure and runs Golden Flow tests

param(
    [int]$WaitSeconds = 30
)

$ErrorActionPreference = "Continue"
$RootDir = Split-Path -Parent $PSScriptRoot

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  SHAHIN GRC - START AND TEST SCRIPT" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Start Docker containers
Write-Host "[1/5] Starting Docker infrastructure..." -ForegroundColor Yellow
Set-Location $RootDir
docker compose -f docker-compose.dev.yml up -d
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Docker compose failed. Please ensure Docker Desktop is running." -ForegroundColor Red
    exit 1
}

Write-Host "[2/5] Waiting for PostgreSQL to be healthy..." -ForegroundColor Yellow
$attempts = 0
$maxAttempts = 30
while ($attempts -lt $maxAttempts) {
    $health = docker inspect shahin-postgres --format='{{.State.Health.Status}}' 2>$null
    if ($health -eq "healthy") {
        Write-Host "PostgreSQL is healthy!" -ForegroundColor Green
        break
    }
    $attempts++
    Write-Host "  Waiting... ($attempts/$maxAttempts)"
    Start-Sleep -Seconds 2
}

if ($attempts -ge $maxAttempts) {
    Write-Host "WARNING: PostgreSQL health check timed out. Continuing anyway..." -ForegroundColor Yellow
}

# Step 2: Start the API server
Write-Host "[3/5] Starting GrcMvc API server..." -ForegroundColor Yellow
$serverJob = Start-Job -ScriptBlock {
    Set-Location "$using:RootDir\src\GrcMvc"
    dotnet run --launch-profile http 2>&1
}

Write-Host "Waiting $WaitSeconds seconds for server to start..."
Start-Sleep -Seconds $WaitSeconds

# Check if server is responding
Write-Host "[4/5] Checking server status..." -ForegroundColor Yellow
$serverReady = $false
$attempts = 0
while ($attempts -lt 10 -and -not $serverReady) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
        $serverReady = $true
        Write-Host "Server is ready!" -ForegroundColor Green
    }
    catch {
        $attempts++
        Write-Host "  Server not ready yet... ($attempts/10)"
        Start-Sleep -Seconds 5
    }
}

if (-not $serverReady) {
    Write-Host ""
    Write-Host "Server may not be fully ready. Checking job output..." -ForegroundColor Yellow
    $jobOutput = Receive-Job -Job $serverJob -Keep 2>&1 | Select-Object -Last 20
    Write-Host $jobOutput
    Write-Host ""
    Write-Host "Would you like to continue with tests anyway? (y/n)" -ForegroundColor Yellow
    $continue = Read-Host
    if ($continue -ne "y") {
        Stop-Job -Job $serverJob
        exit 1
    }
}

# Step 3: Run Golden Flow tests
Write-Host "[5/5] Running Golden Flow tests..." -ForegroundColor Yellow
& "$PSScriptRoot\Test-GoldenFlows.ps1" -BaseUrl "http://localhost:5000"

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  TESTS COMPLETE" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Server is still running in background."
Write-Host "To stop it: Stop-Job -Id $($serverJob.Id); Remove-Job -Id $($serverJob.Id)"
Write-Host ""
Write-Host "Check results in: $RootDir\Golden_Flow_Evidence\" -ForegroundColor Green
