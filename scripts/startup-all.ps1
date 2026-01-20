# GRC Platform Startup Script
# Starts all services in proper sequence: Database → API → Frontend

param(
    [switch]$SkipDatabase,
    [switch]$SkipBackend,
    [switch]$SkipFrontend,
    [switch]$DockerOnly
)

$ErrorActionPreference = "Stop"

Write-Host "🚀 Starting GRC Platform..." -ForegroundColor Green
Write-Host ""

# Get script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir

# Step 1: Database
if (-not $SkipDatabase) {
    Write-Host "📊 Step 1: Starting Database & Redis..." -ForegroundColor Cyan
    Set-Location $projectRoot
    
    if ($DockerOnly) {
        docker-compose up -d db redis
    } else {
        docker-compose up -d db redis
    }
    
    Write-Host "   Waiting for database to be ready..." -ForegroundColor Yellow
    $dbReady = $false
    for ($i = 0; $i -lt 30; $i++) {
        try {
            $result = docker exec grc-db pg_isready -U postgres 2>&1
            if ($LASTEXITCODE -eq 0) {
                $dbReady = $true
                Write-Host "   ✅ Database is ready" -ForegroundColor Green
                break
            }
        } catch {
            # Continue waiting
        }
        Start-Sleep -Seconds 2
    }
    
    if (-not $dbReady) {
        Write-Host "   ⚠️  Database may not be ready, but continuing..." -ForegroundColor Yellow
    }
    
    # Check Redis
    try {
        $redisResult = docker exec grc-redis redis-cli ping 2>&1
        if ($redisResult -match "PONG") {
            Write-Host "   ✅ Redis is ready" -ForegroundColor Green
        }
    } catch {
        Write-Host "   ⚠️  Redis check failed, but continuing..." -ForegroundColor Yellow
    }
    
    Write-Host ""
} else {
    Write-Host "⏭️  Skipping database startup" -ForegroundColor Gray
    Write-Host ""
}

# Step 2: Backend API
if (-not $SkipBackend) {
    Write-Host "🔧 Step 2: Starting Backend API..." -ForegroundColor Cyan
    $backendPath = Join-Path $projectRoot "src\GrcMvc"
    
    if (Test-Path $backendPath) {
        # Start backend in new window
        Start-Process powershell -ArgumentList @(
            "-NoExit",
            "-Command",
            "cd '$backendPath'; Write-Host '🔧 Starting Backend API...' -ForegroundColor Cyan; dotnet run --urls 'http://localhost:5010'"
        )
        
        Write-Host "   Waiting for API server to start..." -ForegroundColor Yellow
        Start-Sleep -Seconds 10
        
        # Verify API
        $apiReady = $false
        for ($i = 0; $i -lt 30; $i++) {
            try {
                $response = Invoke-WebRequest -Uri "http://localhost:5010/health" -UseBasicParsing -TimeoutSec 2 -ErrorAction SilentlyContinue
                if ($response.StatusCode -eq 200) {
                    $apiReady = $true
                    Write-Host "   ✅ API server is ready" -ForegroundColor Green
                    break
                }
            } catch {
                # Continue waiting
            }
            Start-Sleep -Seconds 2
        }
        
        if (-not $apiReady) {
            Write-Host "   ⚠️  API server may not be ready yet, check the backend window" -ForegroundColor Yellow
        }
    } else {
        Write-Host "   ❌ Backend path not found: $backendPath" -ForegroundColor Red
    }
    
    Write-Host ""
} else {
    Write-Host "⏭️  Skipping backend startup" -ForegroundColor Gray
    Write-Host ""
}

# Step 3: Frontend
if (-not $SkipFrontend) {
    Write-Host "🌐 Step 3: Starting Frontend..." -ForegroundColor Cyan
    $frontendPath = Join-Path $projectRoot "grc-frontend"
    
    if (Test-Path $frontendPath) {
        # Start frontend in new window
        Start-Process powershell -ArgumentList @(
            "-NoExit",
            "-Command",
            "cd '$frontendPath'; Write-Host '🌐 Starting Frontend...' -ForegroundColor Cyan; npm run dev"
        )
        
        Write-Host "   ✅ Frontend starting in new window" -ForegroundColor Green
        Write-Host "   Waiting 5 seconds for frontend to initialize..." -ForegroundColor Yellow
        Start-Sleep -Seconds 5
    } else {
        Write-Host "   ❌ Frontend path not found: $frontendPath" -ForegroundColor Red
    }
    
    Write-Host ""
} else {
    Write-Host "⏭️  Skipping frontend startup" -ForegroundColor Gray
    Write-Host ""
}

# Summary
Write-Host "✅ Startup sequence completed!" -ForegroundColor Green
Write-Host ""
Write-Host "📌 Service URLs:" -ForegroundColor Yellow
Write-Host "   Database:    localhost:5432" -ForegroundColor White
Write-Host "   Redis:       localhost:6379" -ForegroundColor White
Write-Host "   Backend API: http://localhost:5010" -ForegroundColor White
Write-Host "   Frontend:    http://localhost:3003" -ForegroundColor White
Write-Host ""
Write-Host "💡 Tips:" -ForegroundColor Cyan
Write-Host "   - Check backend window for startup logs" -ForegroundColor Gray
Write-Host "   - Check frontend window for Next.js dev server" -ForegroundColor Gray
Write-Host "   - Use 'docker-compose logs -f db' to see database logs" -ForegroundColor Gray
Write-Host ""
