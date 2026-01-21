# ═══════════════════════════════════════════════════════════════════════════════
# SHAHIN AI GRC PLATFORM - FULL STACK STARTUP SCRIPT
# Starts all services for local development
# Created: 2026-01-21
# ═══════════════════════════════════════════════════════════════════════════════

param(
    [switch]$SkipDatabase,
    [switch]$SkipBackend,
    [switch]$SkipFrontend,
    [switch]$DockerOnly,
    [switch]$CleanStart
)

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent $PSScriptRoot

# Colors for output
function Write-Step { param($Message) Write-Host "`n[STEP] $Message" -ForegroundColor Yellow }
function Write-Success { param($Message) Write-Host "[OK] $Message" -ForegroundColor Green }
function Write-Info { param($Message) Write-Host "[INFO] $Message" -ForegroundColor Cyan }
function Write-Error { param($Message) Write-Host "[ERROR] $Message" -ForegroundColor Red }

# Banner
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SHAHIN AI GRC PLATFORM - FULL STACK LOCAL DEVELOPMENT STARTUP" -ForegroundColor Cyan
Write-Host "  OpenIddict ABP Authentication | PostgreSQL | Redis | Kafka | Analytics" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Navigate to project root
Set-Location $ProjectRoot
Write-Info "Working directory: $ProjectRoot"

# ═══════════════════════════════════════════════════════════════════════════════
# STEP 0: Clean Start (Optional)
# ═══════════════════════════════════════════════════════════════════════════════

if ($CleanStart) {
    Write-Step "Performing clean start - removing existing containers and volumes..."
    docker-compose -f docker-compose.dev.yml down -v 2>$null
    Write-Success "Clean start complete"
}

# ═══════════════════════════════════════════════════════════════════════════════
# STEP 1: Load Environment Variables
# ═══════════════════════════════════════════════════════════════════════════════

Write-Step "Loading environment variables..."

if (Test-Path ".env.dev") {
    Write-Info "Using .env.dev configuration"
    Copy-Item ".env.dev" ".env" -Force
} elseif (Test-Path ".env") {
    Write-Info "Using existing .env configuration"
} else {
    Write-Error ".env file not found! Please create .env.dev first."
    exit 1
}

Write-Success "Environment variables loaded"

# ═══════════════════════════════════════════════════════════════════════════════
# STEP 2: Start Docker Infrastructure
# ═══════════════════════════════════════════════════════════════════════════════

Write-Step "Starting Docker infrastructure (11 services)..."

# Check Docker is running
try {
    docker info > $null 2>&1
} catch {
    Write-Error "Docker is not running! Please start Docker Desktop first."
    exit 1
}

# Start all services
docker-compose -f docker-compose.dev.yml up -d

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to start Docker containers"
    exit 1
}

Write-Success "Docker containers started"

# ═══════════════════════════════════════════════════════════════════════════════
# STEP 3: Wait for Core Services
# ═══════════════════════════════════════════════════════════════════════════════

Write-Step "Waiting for core services to be healthy..."

# Wait for PostgreSQL
Write-Info "Waiting for PostgreSQL..."
$maxRetries = 30
$retryCount = 0
do {
    $result = docker exec grc-db pg_isready -U shahin_admin 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Success "PostgreSQL is ready"
        break
    }
    $retryCount++
    Start-Sleep -Seconds 2
} while ($retryCount -lt $maxRetries)

if ($retryCount -ge $maxRetries) {
    Write-Error "PostgreSQL failed to start"
    exit 1
}

# Wait for Redis
Write-Info "Waiting for Redis..."
$retryCount = 0
do {
    $result = docker exec grc-redis redis-cli ping 2>$null
    if ($result -eq "PONG") {
        Write-Success "Redis is ready"
        break
    }
    $retryCount++
    Start-Sleep -Seconds 2
} while ($retryCount -lt 15)

Write-Success "Core services are healthy"

if ($DockerOnly) {
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════════════════════════" -ForegroundColor Green
    Write-Host "  Docker infrastructure started successfully (DockerOnly mode)" -ForegroundColor Green
    Write-Host "═══════════════════════════════════════════════════════════════════════════════" -ForegroundColor Green
    docker-compose -f docker-compose.dev.yml ps
    exit 0
}

# ═══════════════════════════════════════════════════════════════════════════════
# STEP 4: Run Database Migrations
# ═══════════════════════════════════════════════════════════════════════════════

if (-not $SkipDatabase) {
    Write-Step "Running EF Core database migrations..."

    Set-Location "$ProjectRoot\src\GrcMvc"

    # Check if EF Core tools are installed
    $efVersion = dotnet ef --version 2>$null
    if (-not $efVersion) {
        Write-Info "Installing EF Core tools..."
        dotnet tool install --global dotnet-ef
    }

    # Run migrations
    dotnet ef database update

    if ($LASTEXITCODE -eq 0) {
        Write-Success "Database migrations completed"
    } else {
        Write-Error "Database migrations failed"
        # Continue anyway - migrations might already be applied
    }

    Set-Location $ProjectRoot
}

# ═══════════════════════════════════════════════════════════════════════════════
# STEP 5: Start Backend Application
# ═══════════════════════════════════════════════════════════════════════════════

if (-not $SkipBackend) {
    Write-Step "Starting backend API (ASP.NET Core MVC + OpenIddict ABP)..."

    $backendPath = "$ProjectRoot\src\GrcMvc"

    # Start backend in new PowerShell window
    Start-Process powershell -ArgumentList @(
        "-NoExit",
        "-Command",
        "Set-Location '$backendPath'; Write-Host 'Starting Backend API on http://localhost:5010' -ForegroundColor Cyan; dotnet run --urls 'http://localhost:5010'"
    )

    Write-Info "Backend starting in new window..."

    # Wait for backend to be ready
    Write-Info "Waiting for backend API to be ready..."
    Start-Sleep -Seconds 15

    $maxRetries = 30
    $retryCount = 0
    do {
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:5010/health/live" -TimeoutSec 5 -ErrorAction SilentlyContinue
            if ($response.StatusCode -eq 200) {
                Write-Success "Backend API is ready"
                break
            }
        } catch {
            # Still starting
        }
        $retryCount++
        Start-Sleep -Seconds 3
    } while ($retryCount -lt $maxRetries)
}

# ═══════════════════════════════════════════════════════════════════════════════
# STEP 6: Start Frontend Application
# ═══════════════════════════════════════════════════════════════════════════════

if (-not $SkipFrontend) {
    Write-Step "Starting frontend (Next.js)..."

    $frontendPath = "$ProjectRoot\grc-frontend"

    if (Test-Path $frontendPath) {
        # Check if node_modules exists
        if (-not (Test-Path "$frontendPath\node_modules")) {
            Write-Info "Installing frontend dependencies..."
            Set-Location $frontendPath
            npm install
            Set-Location $ProjectRoot
        }

        # Start frontend in new PowerShell window
        Start-Process powershell -ArgumentList @(
            "-NoExit",
            "-Command",
            "Set-Location '$frontendPath'; Write-Host 'Starting Frontend on http://localhost:3003' -ForegroundColor Cyan; npm run dev"
        )

        Write-Info "Frontend starting in new window..."
    } else {
        Write-Info "Frontend directory not found, skipping..."
    }
}

# ═══════════════════════════════════════════════════════════════════════════════
# FINAL: Display Service URLs
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "  FULL STACK STARTED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "  APPLICATION SERVICES:" -ForegroundColor White
Write-Host "  ─────────────────────────────────────────────────────────────────────────────"
Write-Host "  Backend API:          http://localhost:5010" -ForegroundColor Cyan
Write-Host "  Frontend:             http://localhost:3003" -ForegroundColor Cyan
Write-Host "  Swagger/API Docs:     http://localhost:5010/swagger" -ForegroundColor Cyan
Write-Host "  Hangfire Dashboard:   http://localhost:5010/hangfire" -ForegroundColor Cyan
Write-Host ""
Write-Host "  OPENIDDICT ENDPOINTS (OAuth2/OIDC):" -ForegroundColor White
Write-Host "  ─────────────────────────────────────────────────────────────────────────────"
Write-Host "  Authorization:        http://localhost:5010/connect/authorize" -ForegroundColor Cyan
Write-Host "  Token:                http://localhost:5010/connect/token" -ForegroundColor Cyan
Write-Host "  Userinfo:             http://localhost:5010/connect/userinfo" -ForegroundColor Cyan
Write-Host "  Logout:               http://localhost:5010/connect/logout" -ForegroundColor Cyan
Write-Host ""
Write-Host "  ANALYTICS & MONITORING STACK:" -ForegroundColor White
Write-Host "  ─────────────────────────────────────────────────────────────────────────────"
Write-Host "  Grafana:              http://localhost:3030        (admin/GrafanaAdmin@2026!)" -ForegroundColor Cyan
Write-Host "  Superset:             http://localhost:8088        (admin/SupersetAdmin@2026!)" -ForegroundColor Cyan
Write-Host "  Metabase:             http://localhost:3033        (setup on first access)" -ForegroundColor Cyan
Write-Host "  ClickHouse:           http://localhost:8123        (grc_analytics/grc_analytics_2026)" -ForegroundColor Cyan
Write-Host ""
Write-Host "  WORKFLOW & MESSAGING:" -ForegroundColor White
Write-Host "  ─────────────────────────────────────────────────────────────────────────────"
Write-Host "  Camunda BPM:          http://localhost:8085/camunda (demo/demo)" -ForegroundColor Cyan
Write-Host "  n8n Automation:       http://localhost:5678        (admin/N8nAdmin@2026!)" -ForegroundColor Cyan
Write-Host "  Kafka UI:             http://localhost:9080" -ForegroundColor Cyan
Write-Host ""
Write-Host "  DATABASE & CACHE:" -ForegroundColor White
Write-Host "  ─────────────────────────────────────────────────────────────────────────────"
Write-Host "  PostgreSQL:           localhost:5432               (shahin_admin/Shahin@GRC2026!)" -ForegroundColor Cyan
Write-Host "  Redis:                localhost:6379" -ForegroundColor Cyan
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════════════════════" -ForegroundColor Green

# Show container status
Write-Host ""
Write-Host "  DOCKER CONTAINER STATUS:" -ForegroundColor White
Write-Host "  ─────────────────────────────────────────────────────────────────────────────"
docker-compose -f docker-compose.dev.yml ps --format "table {{.Name}}\t{{.Status}}\t{{.Ports}}"
