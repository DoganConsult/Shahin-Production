# ═══════════════════════════════════════════════════════════════
# Shahin AI GRC Platform - Docker Hub Build & Push Script
# Builds and pushes images to Docker Hub
# ═══════════════════════════════════════════════════════════════

param(
    [string]$DockerHubUsername = "drdogan",
    [string]$Tag = "latest",
    [switch]$BuildOnly,
    [switch]$PushOnly,
    [switch]$SkipFrontend,
    [switch]$SkipBackend
)

$ErrorActionPreference = "Stop"

Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " Shahin AI GRC - Docker Hub Build & Push" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Configuration
$BackendImage = "$DockerHubUsername/grc-backend"
$FrontendImage = "$DockerHubUsername/grc-frontend"
$ProjectRoot = Split-Path -Parent $PSScriptRoot

Write-Host "Configuration:" -ForegroundColor Yellow
Write-Host "  Docker Hub User: $DockerHubUsername"
Write-Host "  Tag: $Tag"
Write-Host "  Backend Image: ${BackendImage}:${Tag}"
Write-Host "  Frontend Image: ${FrontendImage}:${Tag}"
Write-Host ""

# Check Docker is running
try {
    docker version | Out-Null
    Write-Host "[OK] Docker is running" -ForegroundColor Green
} catch {
    Write-Host "[ERROR] Docker is not running or not installed" -ForegroundColor Red
    exit 1
}

# Login to Docker Hub (if not push only)
if (-not $BuildOnly) {
    Write-Host "`n[1/4] Logging in to Docker Hub..." -ForegroundColor Yellow
    Write-Host "Please enter your Docker Hub credentials:" -ForegroundColor Gray
    docker login
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[ERROR] Docker Hub login failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "[OK] Logged in to Docker Hub" -ForegroundColor Green
}

# Build Backend
if (-not $SkipBackend -and -not $PushOnly) {
    Write-Host "`n[2/4] Building Backend Image..." -ForegroundColor Yellow
    Set-Location "$ProjectRoot\src\GrcMvc"

    docker build `
        -f Dockerfile.production `
        -t "${BackendImage}:${Tag}" `
        -t "${BackendImage}:latest" `
        --label "org.opencontainers.image.source=https://github.com/DoganConsult/Shahin-Production" `
        --label "org.opencontainers.image.description=Shahin AI GRC Backend" `
        --label "org.opencontainers.image.version=$Tag" `
        .

    if ($LASTEXITCODE -ne 0) {
        Write-Host "[ERROR] Backend build failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "[OK] Backend image built: ${BackendImage}:${Tag}" -ForegroundColor Green
}

# Build Frontend
if (-not $SkipFrontend -and -not $PushOnly) {
    Write-Host "`n[3/4] Building Frontend Image..." -ForegroundColor Yellow
    Set-Location "$ProjectRoot\grc-frontend"

    docker build `
        -f Dockerfile.production `
        -t "${FrontendImage}:${Tag}" `
        -t "${FrontendImage}:latest" `
        --build-arg NEXT_PUBLIC_API_URL=http://localhost:5010 `
        --build-arg NEXT_PUBLIC_OIDC_AUTHORITY=http://localhost:5010 `
        --build-arg NEXT_PUBLIC_OIDC_CLIENT_ID=grc-web `
        --label "org.opencontainers.image.source=https://github.com/DoganConsult/Shahin-Production" `
        --label "org.opencontainers.image.description=Shahin AI GRC Frontend" `
        --label "org.opencontainers.image.version=$Tag" `
        .

    if ($LASTEXITCODE -ne 0) {
        Write-Host "[ERROR] Frontend build failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "[OK] Frontend image built: ${FrontendImage}:${Tag}" -ForegroundColor Green
}

# Push images
if (-not $BuildOnly) {
    Write-Host "`n[4/4] Pushing Images to Docker Hub..." -ForegroundColor Yellow

    if (-not $SkipBackend) {
        Write-Host "  Pushing backend..." -ForegroundColor Gray
        docker push "${BackendImage}:${Tag}"
        docker push "${BackendImage}:latest"
        Write-Host "[OK] Backend pushed" -ForegroundColor Green
    }

    if (-not $SkipFrontend) {
        Write-Host "  Pushing frontend..." -ForegroundColor Gray
        docker push "${FrontendImage}:${Tag}"
        docker push "${FrontendImage}:latest"
        Write-Host "[OK] Frontend pushed" -ForegroundColor Green
    }
}

Set-Location $ProjectRoot

Write-Host "`n═══════════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host " Build & Push Complete!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "Images available on Docker Hub:" -ForegroundColor Yellow
Write-Host "  Backend:  https://hub.docker.com/r/$DockerHubUsername/grc-backend"
Write-Host "  Frontend: https://hub.docker.com/r/$DockerHubUsername/grc-frontend"
Write-Host ""
Write-Host "To run with Docker Hub images:" -ForegroundColor Yellow
Write-Host "  docker-compose -f docker-compose.dockerhub.yml up -d"
Write-Host ""
