# ══════════════════════════════════════════════════════════════
# PowerShell Deployment Script - Direct Execution
# Pushes to Docker Hub, connects via SSH, and deploys
# ══════════════════════════════════════════════════════════════

$ErrorActionPreference = "Stop"

# Configuration
$SERVER_IP = "212.147.229.36"
$SERVER_USER = "root"
$SSH_KEY = "$env:USERPROFILE\.ssh\id_ed25519"
$DOCKERHUB_USER = if ($env:DOCKERHUB_USER) { $env:DOCKERHUB_USER } else { "drdogan" }
$SCRIPT_DIR = $PSScriptRoot
$PROJECT_ROOT = Split-Path $SCRIPT_DIR -Parent
$DEPLOY_DIR = "/opt/shahin-ai"

function Write-Header($text) {
    Write-Host ""
    Write-Host "══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  $text" -ForegroundColor Cyan
    Write-Host "══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Step($step, $text) {
    Write-Host "[$step] $text" -ForegroundColor Yellow
}

function Write-Success($text) {
    Write-Host "✅ $text" -ForegroundColor Green
}

function Write-Error($text) {
    Write-Host "❌ $text" -ForegroundColor Red
}

# ════════════════════════════════════════════════════════════
# Step 1: Push to Docker Hub
# ════════════════════════════════════════════════════════════
function Push-ToDockerHub {
    Write-Header "STEP 1: PUSH TO DOCKER HUB"
    
    Write-Step "1.1" "Building Landing Page..."
    Push-Location "$PROJECT_ROOT\grc-frontend"
    docker build -t "$DOCKERHUB_USER/shahin-landing:latest" -f Dockerfile .
    docker tag "$DOCKERHUB_USER/shahin-landing:latest" "$DOCKERHUB_USER/shahin-landing:$(Get-Date -Format 'yyyyMMdd')"
    
    Write-Step "1.2" "Pushing Landing to Docker Hub..."
    docker push "$DOCKERHUB_USER/shahin-landing:latest"
    Write-Success "Landing Page pushed"
    
    Write-Step "1.3" "Building Portal (Backend)..."
    Push-Location "$PROJECT_ROOT\src\GrcMvc"
    docker build -t "$DOCKERHUB_USER/shahin-portal:latest" -f Dockerfile.production .
    docker tag "$DOCKERHUB_USER/shahin-portal:latest" "$DOCKERHUB_USER/shahin-portal:$(Get-Date -Format 'yyyyMMdd')"
    
    Write-Step "1.4" "Pushing Portal to Docker Hub..."
    docker push "$DOCKERHUB_USER/shahin-portal:latest"
    Write-Success "Portal pushed"
    
    Pop-Location
    Pop-Location
}

# ════════════════════════════════════════════════════════════
# Step 2: Setup Server
# ════════════════════════════════════════════════════════════
function Setup-Server {
    Write-Header "STEP 2: SETUP SERVER VIA SSH"
    
    Write-Step "2.1" "Testing SSH connection..."
    $testResult = ssh -i $SSH_KEY -o StrictHostKeyChecking=no "$SERVER_USER@$SERVER_IP" "echo 'SSH OK'" 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Success "SSH connection verified"
    } else {
        Write-Error "SSH connection failed"
        exit 1
    }
    
    Write-Step "2.2" "Installing Docker and Docker Compose..."
    $setupScript = @"
if ! command -v docker &> /dev/null; then
    curl -fsSL https://get.docker.com -o get-docker.sh
    sh get-docker.sh
    rm get-docker.sh
    systemctl enable docker
    systemctl start docker
fi
if ! docker compose version &> /dev/null; then
    apt-get update
    apt-get install -y docker-compose-plugin
fi
mkdir -p $DEPLOY_DIR/{nginx,init-db,ssl}
chmod 755 $DEPLOY_DIR
echo "Setup complete"
"@
    
    ssh -i $SSH_KEY "$SERVER_USER@$SERVER_IP" $setupScript
    Write-Success "Server setup complete"
}

# ════════════════════════════════════════════════════════════
# Step 3: Upload Files
# ════════════════════════════════════════════════════════════
function Upload-Files {
    Write-Header "STEP 3: UPLOAD FILES TO SERVER"
    
    $envFile = Join-Path $SCRIPT_DIR ".env.production"
    if (-not (Test-Path $envFile)) {
        Write-Error ".env.production not found"
        exit 1
    }
    
    Write-Step "3.1" "Uploading deployment files..."
    scp -i $SSH_KEY "$SCRIPT_DIR\docker-compose.production-server.yml" "${SERVER_USER}@${SERVER_IP}:${DEPLOY_DIR}/docker-compose.yml"
    scp -i $SSH_KEY $envFile "${SERVER_USER}@${SERVER_IP}:${DEPLOY_DIR}/.env"
    scp -i $SSH_KEY "$SCRIPT_DIR\nginx\production-212.147.229.36.conf" "${SERVER_USER}@${SERVER_IP}:${DEPLOY_DIR}/nginx.conf"
    
    Write-Success "Files uploaded"
}

# ════════════════════════════════════════════════════════════
# Step 4: Deploy Containers
# ════════════════════════════════════════════════════════════
function Deploy-Containers {
    Write-Header "STEP 4: DEPLOY ALL CONTAINERS"
    
    Write-Step "4.1" "Pulling images from Docker Hub..."
    ssh -i $SSH_KEY "$SERVER_USER@$SERVER_IP" "cd $DEPLOY_DIR && export DOCKERHUB_USER=$DOCKERHUB_USER && docker-compose pull"
    
    Write-Step "4.2" "Stopping existing containers..."
    ssh -i $SSH_KEY "$SERVER_USER@$SERVER_IP" "cd $DEPLOY_DIR && docker-compose down"
    
    Write-Step "4.3" "Starting all containers..."
    ssh -i $SSH_KEY "$SERVER_USER@$SERVER_IP" "cd $DEPLOY_DIR && export DOCKERHUB_USER=$DOCKERHUB_USER && docker-compose up -d --build"
    
    Start-Sleep -Seconds 30
    
    Write-Step "4.4" "Container status:"
    ssh -i $SSH_KEY "$SERVER_USER@$SERVER_IP" "cd $DEPLOY_DIR && docker-compose ps"
    
    Write-Success "All containers deployed"
}

# ════════════════════════════════════════════════════════════
# Step 5: Verify
# ════════════════════════════════════════════════════════════
function Verify-Deployment {
    Write-Header "STEP 5: VERIFY DEPLOYMENT"
    
    Start-Sleep -Seconds 20
    
    Write-Step "5.1" "Testing endpoints..."
    $endpoints = @(
        @{Url="http://${SERVER_IP}:3000/health"; Name="Landing Health"},
        @{Url="http://${SERVER_IP}:5000/health"; Name="Portal Health"},
        @{Url="http://${SERVER_IP}:5000/api/health"; Name="API Health"},
        @{Url="http://${SERVER_IP}:11434/api/tags"; Name="Ollama API"}
    )
    
    foreach ($ep in $endpoints) {
        try {
            $response = Invoke-WebRequest -Uri $ep.Url -TimeoutSec 10 -UseBasicParsing -ErrorAction SilentlyContinue
            if ($response.StatusCode -eq 200) {
                Write-Success "$($ep.Name): HTTP 200"
            }
        } catch {
            Write-Host "⚠️  $($ep.Name): Not responding yet" -ForegroundColor Yellow
        }
    }
    
    Write-Step "5.2" "Checking containers..."
    ssh -i $SSH_KEY "$SERVER_USER@$SERVER_IP" "docker ps --filter 'name=shahin' --format 'table {{.Names}}\t{{.Status}}'"
    
    Write-Success "Verification complete"
}

# ════════════════════════════════════════════════════════════
# Main Execution
# ════════════════════════════════════════════════════════════

Write-Header "COMPLETE SERVER SETUP AND DEPLOYMENT"

try {
    Push-ToDockerHub
    Setup-Server
    Upload-Files
    Deploy-Containers
    Verify-Deployment
    
    Write-Header "DEPLOYMENT COMPLETE"
    Write-Host "Access URLs:" -ForegroundColor Blue
    Write-Host "  Landing:    http://$SERVER_IP:3000" -ForegroundColor White
    Write-Host "  Portal:     http://$SERVER_IP:5000" -ForegroundColor White
    Write-Host "  Admin:      http://$SERVER_IP:5000/admin/login" -ForegroundColor White
    Write-Host "  Platform:   http://$SERVER_IP:5000/platform-admin" -ForegroundColor White
    Write-Host ""
    Write-Success "Production deployment successful!"
    
} catch {
    Write-Error "Deployment failed: $_"
    exit 1
}
