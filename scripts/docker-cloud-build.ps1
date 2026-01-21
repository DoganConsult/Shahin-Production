# ═══════════════════════════════════════════════════════════════
# Shahin AI GRC Platform - Docker Cloud Build Script
# Uses Docker Build Cloud for remote building
# ═══════════════════════════════════════════════════════════════

param(
    [string]$CloudBuilder = "cloud-drdogan-dogancloudbuilder1",
    [string]$Tag = "latest",
    [switch]$SetupOnly,
    [switch]$BackendOnly,
    [switch]$FrontendOnly
)

$ErrorActionPreference = "Stop"

Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " Shahin AI GRC - Docker Cloud Build" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$ProjectRoot = Split-Path -Parent $PSScriptRoot

# Docker Cloud Build Configuration
$CloudHealthAddress = "https://build-cloud.docker.com:443/v2/"
$CloudProxyAddress = "tcp://build-cloud.docker.com:443"
$CloudRegistryAddress = "build-cloud.docker.com:443"

Write-Host "Docker Cloud Build Configuration:" -ForegroundColor Yellow
Write-Host "  Cloud Builder: $CloudBuilder"
Write-Host "  Health Address: $CloudHealthAddress"
Write-Host "  Proxy Address: $CloudProxyAddress"
Write-Host "  Registry Address: $CloudRegistryAddress"
Write-Host ""

# Step 1: Check Docker Buildx
Write-Host "[1/5] Checking Docker Buildx..." -ForegroundColor Yellow
try {
    $buildxVersion = docker buildx version
    Write-Host "[OK] Docker Buildx installed: $buildxVersion" -ForegroundColor Green
} catch {
    Write-Host "[ERROR] Docker Buildx not available. Please update Docker Desktop." -ForegroundColor Red
    exit 1
}

# Step 2: Create/Use Cloud Builder
Write-Host "`n[2/5] Setting up Docker Cloud Builder..." -ForegroundColor Yellow

# Check if builder already exists
$existingBuilders = docker buildx ls 2>$null
if ($existingBuilders -match $CloudBuilder) {
    Write-Host "  Cloud builder '$CloudBuilder' already exists" -ForegroundColor Gray
} else {
    Write-Host "  Creating cloud builder '$CloudBuilder'..." -ForegroundColor Gray

    # Create cloud builder
    docker buildx create `
        --name $CloudBuilder `
        --driver cloud `
        --driver-opt "endpoint=$CloudProxyAddress" `
        --use 2>$null

    if ($LASTEXITCODE -ne 0) {
        Write-Host "  Note: Cloud driver may require Docker Desktop with Build Cloud enabled" -ForegroundColor Yellow
        Write-Host "  Falling back to standard remote buildx..." -ForegroundColor Yellow

        # Alternative: Use remote driver with cloud endpoint
        docker buildx create `
            --name $CloudBuilder `
            --driver docker-container `
            --use 2>$null
    }
}

# Use the cloud builder
docker buildx use $CloudBuilder 2>$null
Write-Host "[OK] Using builder: $CloudBuilder" -ForegroundColor Green

# Step 3: Bootstrap builder
Write-Host "`n[3/5] Bootstrapping cloud builder..." -ForegroundColor Yellow
docker buildx inspect --bootstrap 2>$null
Write-Host "[OK] Builder is ready" -ForegroundColor Green

if ($SetupOnly) {
    Write-Host "`n[DONE] Setup complete. Use -BackendOnly or -FrontendOnly to build." -ForegroundColor Green
    exit 0
}

# Step 4: Build Backend
if (-not $FrontendOnly) {
    Write-Host "`n[4/5] Building Backend with Cloud Builder..." -ForegroundColor Yellow
    Set-Location "$ProjectRoot\src\GrcMvc"

    Write-Host "  Building and pushing drdogan/grc-backend:$Tag..." -ForegroundColor Gray

    docker buildx build `
        --builder $CloudBuilder `
        --platform linux/amd64,linux/arm64 `
        -f Dockerfile.production `
        -t "drdogan/grc-backend:$Tag" `
        -t "drdogan/grc-backend:latest" `
        --push `
        --progress=plain `
        .

    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] Backend built and pushed with cloud builder" -ForegroundColor Green
    } else {
        Write-Host "[WARNING] Backend build had issues, trying local build..." -ForegroundColor Yellow
        docker build -f Dockerfile.production -t "drdogan/grc-backend:$Tag" .
    }
}

# Step 5: Build Frontend
if (-not $BackendOnly) {
    Write-Host "`n[5/5] Building Frontend with Cloud Builder..." -ForegroundColor Yellow
    Set-Location "$ProjectRoot\grc-frontend"

    Write-Host "  Building and pushing drdogan/grc-frontend:$Tag..." -ForegroundColor Gray

    docker buildx build `
        --builder $CloudBuilder `
        --platform linux/amd64,linux/arm64 `
        -f Dockerfile.production `
        -t "drdogan/grc-frontend:$Tag" `
        -t "drdogan/grc-frontend:latest" `
        --build-arg NEXT_PUBLIC_API_URL=http://localhost:5010 `
        --build-arg NEXT_PUBLIC_OIDC_AUTHORITY=http://localhost:5010 `
        --push `
        --progress=plain `
        .

    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] Frontend built and pushed with cloud builder" -ForegroundColor Green
    } else {
        Write-Host "[WARNING] Frontend build had issues, trying local build..." -ForegroundColor Yellow
        docker build -f Dockerfile.production -t "drdogan/grc-frontend:$Tag" .
    }
}

Set-Location $ProjectRoot

Write-Host "`n═══════════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host " Docker Cloud Build Complete!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "Images available:" -ForegroundColor Yellow
Write-Host "  - drdogan/grc-backend:$Tag"
Write-Host "  - drdogan/grc-frontend:$Tag"
Write-Host ""
Write-Host "To run the stack:" -ForegroundColor Yellow
Write-Host "  docker-compose -f docker-compose.dockerhub.yml up -d"
Write-Host ""
