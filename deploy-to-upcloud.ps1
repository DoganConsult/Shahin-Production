# Deploy Shahin AI GRC Platform to UpCloud Server
# Usage: .\deploy-to-upcloud.ps1
# Requirements: Docker, SSH key at c:\Shahin-ai\shahin_grc_key

param(
    [string]$ServerIP = "95.111.222.132",
    [string]$SshKeyPath = "c:\Shahin-ai\shahin_grc_key",
    [string]$SshUser = "root",
    [string]$DockerImage = "drdogan/shahin-grc:latest",
    [string]$RemoteDir = "/opt/shahin-grc",
    [switch]$SkipBuild,
    [switch]$SkipPush
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Shahin AI GRC - UpCloud Deployment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Build Docker image
if (-not $SkipBuild) {
    Write-Host "[1/5] Building Docker image..." -ForegroundColor Yellow
    Push-Location "c:\Shahin-ai\Shahin-Jan-2026\src\GrcMvc"
    docker build -f Dockerfile.production -t $DockerImage .
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Docker build failed" -ForegroundColor Red
        Pop-Location
        exit 1
    }
    Pop-Location
    Write-Host "Docker image built successfully" -ForegroundColor Green
} else {
    Write-Host "[1/5] Skipping Docker build" -ForegroundColor DarkGray
}

# Step 2: Push to Docker Hub
if (-not $SkipPush) {
    Write-Host "[2/5] Pushing to Docker Hub..." -ForegroundColor Yellow
    docker push $DockerImage
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Docker push failed. Run 'docker login' first." -ForegroundColor Red
        exit 1
    }
    Write-Host "Image pushed to Docker Hub" -ForegroundColor Green
} else {
    Write-Host "[2/5] Skipping Docker push" -ForegroundColor DarkGray
}

# Step 3: Copy configuration files
Write-Host "[3/5] Copying configuration files..." -ForegroundColor Yellow
scp -i $SshKeyPath "c:\Shahin-ai\Shahin-Jan-2026\docker-compose.production.yml" "${SshUser}@${ServerIP}:${RemoteDir}/docker-compose.yml"
scp -i $SshKeyPath "c:\Shahin-ai\Shahin-Jan-2026\.env.production" "${SshUser}@${ServerIP}:${RemoteDir}/.env.production"
scp -i $SshKeyPath "c:\Shahin-ai\Shahin-Jan-2026\scripts\init-db.sql" "${SshUser}@${ServerIP}:${RemoteDir}/scripts/init-db.sql"
Write-Host "Configuration files copied" -ForegroundColor Green

# Step 4: Pull and restart on server
Write-Host "[4/5] Pulling and restarting on server..." -ForegroundColor Yellow
ssh -i $SshKeyPath "${SshUser}@${ServerIP}" "cd ${RemoteDir} && docker compose pull && docker compose up -d --force-recreate grcmvc-prod"
Write-Host "Application restarted" -ForegroundColor Green

# Step 5: Verify deployment
Write-Host "[5/5] Verifying deployment..." -ForegroundColor Yellow
Start-Sleep -Seconds 20

$healthUrl = "http://${ServerIP}/health"
try {
    $response = Invoke-RestMethod -Uri $healthUrl -TimeoutSec 30
    if ($response.status -eq "Healthy" -or $response.checks.name -contains "self") {
        Write-Host "Health check passed!" -ForegroundColor Green
    } else {
        Write-Host "Health check returned status: $($response.status)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "Warning: Health check failed - $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Deployment Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Application URL: http://${ServerIP}" -ForegroundColor White
Write-Host "Health Check:    http://${ServerIP}/health" -ForegroundColor White
Write-Host ""
