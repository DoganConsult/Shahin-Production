# ═══════════════════════════════════════════════════════════════
# Shahin AI GRC Platform - Build and Deploy Script
# Attempts cloud build first, falls back to local build if needed
# ═══════════════════════════════════════════════════════════════

param(
    [string]$Tag = "latest",
    [switch]$LocalOnly,
    [switch]$CloudOnly,
    [switch]$SkipPush,
    [switch]$Deploy
)

$ErrorActionPreference = "Stop"
$ProjectRoot = "c:\Shahin-ai\Shahin-Jan-2026"

Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " Shahin AI GRC - Build and Deploy" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan

$CloudBuilder = "cloud-drdogan-dogancloudbuilder1"
$BackendImage = "drdogan/grc-backend"
$FrontendImage = "drdogan/grc-frontend"

function Build-WithCloud {
    param([string]$Context, [string]$Dockerfile, [string]$Image, [string]$Tag)

    Write-Host "  Attempting cloud build..." -ForegroundColor Gray

    $result = docker buildx build `
        --builder $CloudBuilder `
        --platform linux/amd64,linux/arm64 `
        -f "$Dockerfile" `
        -t "${Image}:${Tag}" `
        -t "${Image}:latest" `
        --push `
        "$Context" 2>&1

    return $LASTEXITCODE -eq 0
}

function Build-Locally {
    param([string]$Context, [string]$Dockerfile, [string]$Image, [string]$Tag)

    Write-Host "  Building locally..." -ForegroundColor Gray

    docker build -f "$Dockerfile" -t "${Image}:${Tag}" -t "${Image}:latest" "$Context"

    if ($LASTEXITCODE -eq 0 -and -not $SkipPush) {
        Write-Host "  Pushing to Docker Hub..." -ForegroundColor Gray
        docker push "${Image}:${Tag}"
        docker push "${Image}:latest"
    }

    return $LASTEXITCODE -eq 0
}

# Build Backend
Write-Host "`n[1/2] Building Backend..." -ForegroundColor Yellow
$backendContext = "$ProjectRoot\src\GrcMvc"
$backendDockerfile = "$ProjectRoot\src\GrcMvc\Dockerfile.production"

$backendSuccess = $false
if (-not $LocalOnly) {
    $backendSuccess = Build-WithCloud -Context $backendContext -Dockerfile $backendDockerfile -Image $BackendImage -Tag $Tag
}

if (-not $backendSuccess -and -not $CloudOnly) {
    Write-Host "  Cloud build failed, falling back to local..." -ForegroundColor Yellow
    $backendSuccess = Build-Locally -Context $backendContext -Dockerfile $backendDockerfile -Image $BackendImage -Tag $Tag
}

if ($backendSuccess) {
    Write-Host "[OK] Backend: ${BackendImage}:${Tag}" -ForegroundColor Green
} else {
    Write-Host "[FAILED] Backend build failed" -ForegroundColor Red
}

# Build Frontend
Write-Host "`n[2/2] Building Frontend..." -ForegroundColor Yellow
$frontendContext = "$ProjectRoot\grc-frontend"
$frontendDockerfile = "$ProjectRoot\grc-frontend\Dockerfile.production"

$frontendSuccess = $false
if (-not $LocalOnly) {
    $frontendSuccess = Build-WithCloud -Context $frontendContext -Dockerfile $frontendDockerfile -Image $FrontendImage -Tag $Tag
}

if (-not $frontendSuccess -and -not $CloudOnly) {
    Write-Host "  Cloud build failed, falling back to local..." -ForegroundColor Yellow
    $frontendSuccess = Build-Locally -Context $frontendContext -Dockerfile $frontendDockerfile -Image $FrontendImage -Tag $Tag
}

if ($frontendSuccess) {
    Write-Host "[OK] Frontend: ${FrontendImage}:${Tag}" -ForegroundColor Green
} else {
    Write-Host "[FAILED] Frontend build failed" -ForegroundColor Red
}

# Deploy if requested
if ($Deploy -and $backendSuccess -and $frontendSuccess) {
    Write-Host "`n[DEPLOY] Starting containers..." -ForegroundColor Yellow
    Set-Location $ProjectRoot
    docker-compose -f docker-compose.dockerhub.yml up -d

    Write-Host "`n[OK] Deployment complete!" -ForegroundColor Green
    Write-Host "  Backend:  http://localhost:5010" -ForegroundColor Cyan
    Write-Host "  Frontend: http://localhost:3003" -ForegroundColor Cyan
}

Write-Host "`n═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " Build Complete" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
