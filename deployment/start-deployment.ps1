# ══════════════════════════════════════════════════════════════
# PowerShell Script to Start Deployment
# Connects to server and deploys all containers
# ══════════════════════════════════════════════════════════════

$ErrorActionPreference = "Stop"

# Configuration
$SERVER_IP = "212.147.229.36"
$SERVER_USER = "root"
$SSH_KEY = "$env:USERPROFILE\.ssh\id_ed25519"
$DOCKERHUB_USER = $env:DOCKERHUB_USER
$SCRIPT_DIR = $PSScriptRoot
$PROJECT_ROOT = Split-Path $SCRIPT_DIR -Parent

Write-Host ""
Write-Host "══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Starting Complete Deployment Process" -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Check prerequisites
Write-Host "[CHECK] Verifying prerequisites..." -ForegroundColor Yellow

# Check Docker
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Docker not found. Please install Docker Desktop." -ForegroundColor Red
    exit 1
}

# Check Docker Compose
if (-not (docker compose version 2>&1)) {
    Write-Host "❌ Docker Compose not found." -ForegroundColor Red
    exit 1
}

# Check SSH key
if (-not (Test-Path $SSH_KEY)) {
    Write-Host "⚠️  SSH key not found at: $SSH_KEY" -ForegroundColor Yellow
    Write-Host "   Please save your SSH private key to this location" -ForegroundColor Yellow
    Write-Host "   Or set SSH_KEY environment variable" -ForegroundColor Yellow
    exit 1
}

# Check environment file
$ENV_FILE = Join-Path $SCRIPT_DIR ".env.production"
if (-not (Test-Path $ENV_FILE)) {
    Write-Host "⚠️  .env.production not found" -ForegroundColor Yellow
    $TEMPLATE = Join-Path $SCRIPT_DIR ".env.production.template"
    if (Test-Path $TEMPLATE) {
        Copy-Item $TEMPLATE $ENV_FILE
        Write-Host "   Created .env.production from template" -ForegroundColor Yellow
        Write-Host "   ⚠️  Please edit .env.production with actual production keys!" -ForegroundColor Red
        Write-Host "   Press any key after editing..."
        $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    } else {
        Write-Host "❌ Template file not found" -ForegroundColor Red
        exit 1
    }
}

# Check Docker Hub username
if (-not $DOCKERHUB_USER) {
    $DOCKERHUB_USER = Read-Host "Enter your Docker Hub username"
    $env:DOCKERHUB_USER = $DOCKERHUB_USER
}

Write-Host "✅ Prerequisites verified" -ForegroundColor Green
Write-Host ""

# Test SSH connection
Write-Host "[SSH] Testing connection to server..." -ForegroundColor Yellow
try {
    $testResult = ssh -i $SSH_KEY -o StrictHostKeyChecking=no "$SERVER_USER@$SERVER_IP" "echo 'SSH connection successful'" 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ SSH connection successful" -ForegroundColor Green
    } else {
        Write-Host "❌ SSH connection failed" -ForegroundColor Red
        Write-Host $testResult
        exit 1
    }
} catch {
    Write-Host "❌ SSH connection failed: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Starting Deployment Script" -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Set environment variables for bash script
$env:SSH_KEY = $SSH_KEY
$env:DOCKERHUB_USER = $DOCKERHUB_USER

# Check if WSL is available (for running bash scripts)
if (Get-Command wsl -ErrorAction SilentlyContinue) {
    Write-Host "[INFO] Using WSL to run deployment script..." -ForegroundColor Yellow
    Write-Host ""
    
    # Convert paths for WSL
    $wslScriptPath = wsl wslpath -a (Join-Path $SCRIPT_DIR "setup-server-complete.sh")
    $wslProjectRoot = wsl wslpath -a $PROJECT_ROOT
    
    # Run the bash script via WSL
    wsl bash $wslScriptPath
} elseif (Get-Command bash -ErrorAction SilentlyContinue) {
    Write-Host "[INFO] Using Git Bash to run deployment script..." -ForegroundColor Yellow
    Write-Host ""
    bash (Join-Path $SCRIPT_DIR "setup-server-complete.sh")
} else {
    Write-Host "❌ Neither WSL nor Git Bash found." -ForegroundColor Red
    Write-Host "   Please install WSL or Git Bash to run the deployment script." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "   Alternatively, you can run the script manually:" -ForegroundColor Yellow
    Write-Host "   1. Push to Docker Hub: docker push ..." -ForegroundColor Yellow
    Write-Host "   2. SSH to server: ssh root@212.147.229.36" -ForegroundColor Yellow
    Write-Host "   3. Run deployment commands on server" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Deployment Process Complete" -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
