# 🚀 Deploy DoganSystem to UpCloud Server
# Server: dogansystem (212.147.229.38) - Frankfurt
# SSH Key: shahin_grc_key (RSA 4096-bit)

param(
    [string]$ServerIP = "212.147.229.38",
    [string]$SshKeyPath = "c:\Shahin-ai\shahin_grc_key",
    [string]$LocalProjectPath = "D:\DoganSystem\DoganSystem",
    [string]$DockerUsername = "drdogan",
    [string]$DockerToken = $env:DOCKER_HUB_TOKEN,  # Set via: $env:DOCKER_HUB_TOKEN = "your-token"
    [switch]$SkipUpload,
    [switch]$OnlyBuild,
    [switch]$UseDockerHub,
    [switch]$TestOnly
)

Write-Host "🚀 DoganSystem UpCloud Deployment" -ForegroundColor Green
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Green
Write-Host "Server: dogansystem ($ServerIP)" -ForegroundColor Cyan
Write-Host "Location: Frankfurt, Germany" -ForegroundColor Cyan
Write-Host "Resources: 2 cores + 8GB RAM + 80GB SSD" -ForegroundColor Cyan
Write-Host ""

# Verify prerequisites
if (-not (Test-Path $SshKeyPath)) {
    Write-Host "❌ SSH key not found: $SshKeyPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $LocalProjectPath)) {
    Write-Host "❌ Project path not found: $LocalProjectPath" -ForegroundColor Red
    exit 1
}

Write-Host "✅ SSH Key: $SshKeyPath" -ForegroundColor Green
Write-Host "✅ Project: $LocalProjectPath" -ForegroundColor Green

# Step 1: Upload project to server (if not skipped)
if (-not $SkipUpload) {
    Write-Host "`n📦 Step 1: Uploading DoganSystem to UpCloud..." -ForegroundColor Yellow
    
    # Create remote directory
    ssh -i $SshKeyPath root@$ServerIP "mkdir -p /opt/dogansystem"
    
    # Upload entire project
    Write-Host "   Uploading project files..." -ForegroundColor Gray
    scp -i $SshKeyPath -r "$LocalProjectPath\*" root@${ServerIP}:/opt/dogansystem/
    
    Write-Host "✅ Project uploaded to /opt/dogansystem/" -ForegroundColor Green
}

# Step 2: Install dependencies on server
Write-Host "`n🔧 Step 2: Installing Dependencies..." -ForegroundColor Yellow

$installScript = @"
#!/bin/bash
set -e

echo '🔧 Installing Docker and dependencies on Ubuntu 24.04...'

# Update system
apt update && apt upgrade -y

# Install essential packages
apt install -y curl wget git htop iotop ufw net-tools

# Install Docker
if ! command -v docker &> /dev/null; then
    curl -fsSL https://get.docker.com -o get-docker.sh
    sh get-docker.sh
    systemctl start docker
    systemctl enable docker
    echo '✅ Docker installed'
else
    echo '✅ Docker already installed'
fi

# Install Docker Compose plugin
apt install -y docker-compose-plugin

# Verify installations
docker --version
docker compose version

# Setup Docker Hub login
echo '$DockerToken' | docker login -u $DockerUsername --password-stdin

echo '✅ Dependencies installed successfully'
echo '✅ Docker Hub configured'
"@

# Execute installation script on server
$installScript | ssh -i $SshKeyPath root@$ServerIP "cat > /tmp/install.sh && chmod +x /tmp/install.sh && /tmp/install.sh"

# Step 3: Configure environment
Write-Host "`n⚙️ Step 3: Configuring Environment..." -ForegroundColor Yellow

$envConfig = @"
# DoganSystem Environment Configuration for UpCloud
# Server: dogansystem (212.147.229.38) Frankfurt

# Database
POSTGRES_PASSWORD=DoganSystem_UpCloud_2024_Secure!
DB_CONNECTION_STRING=Host=postgres;Port=5432;Database=dogansystem;Username=dogansystem;Password=DoganSystem_UpCloud_2024_Secure!

# RabbitMQ
RABBITMQ_USER=dogansystem
RABBITMQ_PASSWORD=DoganSystem_UpCloud_2024_Secure!

# Redis
REDIS_PASSWORD=DoganSystem_UpCloud_2024_Secure!

# JWT Authentication
JWT_SECRET_KEY=DoganSystem-UpCloud-Frankfurt-Secret-Key-32-Characters-Long-2024
JWT_ISSUER=https://ds.doganconsult.com
JWT_AUDIENCE=DoganSystemAPI

# CORS (allow your domains)
CORS_ORIGINS=https://ds.doganconsult.com,https://api.doganconsult.com,https://doganconsult.com,http://212.147.229.38:5000,http://212.147.229.38:8080

# Email Configuration (update with your SMTP provider)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
SMTP_FROM=no-reply@doganconsult.com
SMTP_USE_SSL=true

# Cloudflare Tunnel (update with your token)
CLOUDFLARE_TUNNEL_TOKEN=your-cloudflare-tunnel-token-here

# Optional: AI Services
OPENAI_API_KEY=your-openai-key-here
AZURE_OPENAI_ENDPOINT=your-azure-endpoint-here
"@

# Upload environment configuration
$envConfig | ssh -i $SshKeyPath root@$ServerIP "cat > /opt/dogansystem/.env && chmod 600 /opt/dogansystem/.env"
Write-Host "✅ Environment configured" -ForegroundColor Green

# Step 4: Configure firewall
Write-Host "`n🔥 Step 4: Configuring Firewall..." -ForegroundColor Yellow

$firewallScript = @"
#!/bin/bash
echo '🔥 Configuring UFW firewall...'

# Reset firewall
ufw --force reset

# Default policies
ufw default deny incoming
ufw default allow outgoing

# Allow SSH
ufw allow 22/tcp comment 'SSH'

# Allow HTTP/HTTPS
ufw allow 80/tcp comment 'HTTP'
ufw allow 443/tcp comment 'HTTPS'

# Allow DoganSystem ports (for testing)
ufw allow 5000/tcp comment 'DoganSystem API'
ufw allow 8080/tcp comment 'DoganSystem Web'
ufw allow 3000/tcp comment 'DoganSystem Marketing'

# Allow RabbitMQ Management (restricted)
ufw allow from 10.4.4.0/24 to any port 15672 comment 'RabbitMQ Management (private)'

# Enable firewall
ufw --force enable

# Show status
ufw status numbered

echo '✅ Firewall configured'
"@

$firewallScript | ssh -i $SshKeyPath root@$ServerIP "cat > /tmp/firewall.sh && chmod +x /tmp/firewall.sh && /tmp/firewall.sh"

if (-not $OnlyBuild) {
    # Step 5: Deploy DoganSystem
    Write-Host "`n🚀 Step 5: Deploying DoganSystem Stack..." -ForegroundColor Yellow

    $deployScript = @"
#!/bin/bash
set -e

cd /opt/dogansystem

echo '🚀 Starting DoganSystem deployment...'

# Build and start optimized stack for 8GB RAM
docker compose -f docker/docker-compose.testing.yml up -d --build

echo '⏳ Waiting for services to start...'
sleep 30

# Check service status
docker ps --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}'

echo ''
echo '📊 Resource usage:'
docker stats --no-stream --format 'table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.MemPerc}}'

echo ''
echo '🧪 Testing service endpoints...'

# Test API
if curl -s http://localhost:5000/api/health > /dev/null; then
    echo '✅ API: Running (http://localhost:5000/api/health)'
else
    echo '❌ API: Not responding'
fi

# Test Web
if curl -s http://localhost:8080 > /dev/null; then
    echo '✅ Web: Running (http://localhost:8080)'
else
    echo '❌ Web: Not responding'
fi

# Test Qdrant
if curl -s http://localhost:6333/health > /dev/null; then
    echo '✅ Qdrant: Running (http://localhost:6333/health)'
else
    echo '❌ Qdrant: Not responding'
fi

# Test OPA
if curl -s http://localhost:8181/health > /dev/null; then
    echo '✅ OPA: Running (http://localhost:8181/health)'
else
    echo '❌ OPA: Not responding'
fi

echo ''
echo '🎉 DoganSystem deployment complete!'
echo ''
echo '📍 Access Points:'
echo '   API: http://212.147.229.38:5000/api'
echo '   Web: http://212.147.229.38:8080'
echo '   Marketing: http://212.147.229.38:3000'
echo '   RabbitMQ: http://212.147.229.38:15672'
echo '   Swagger: http://212.147.229.38:5000/swagger'
echo ''
echo '🌐 Public Access (configure DNS):'
echo '   API: https://api.doganconsult.com'
echo '   Web: https://ds.doganconsult.com'
echo '   Marketing: https://doganconsult.com'
echo ''
echo '📋 Next steps:'
echo '1. Configure DNS records to point to 212.147.229.38'
echo '2. Setup Cloudflare tunnel'
echo '3. Test public endpoints'
echo '4. Monitor logs: docker compose -f docker/docker-compose.testing.yml logs -f'
"@

    $deployScript | ssh -i $SshKeyPath root@$ServerIP "cat > /tmp/deploy.sh && chmod +x /tmp/deploy.sh && /tmp/deploy.sh"
}

if ($TestOnly) {
    # Step 6: Run integration tests
    Write-Host "`n🧪 Step 6: Running Integration Tests..." -ForegroundColor Yellow

    $testScript = @"
#!/bin/bash
cd /opt/dogansystem

echo '🧪 Testing DoganSystem integration...'

# Wait for all services to be ready
echo 'Waiting for services to stabilize...'
sleep 60

# Test public contact API
echo ''
echo '📧 Testing public contact API...'
curl -X POST http://localhost:5000/api/public/contact \
  -H 'Content-Type: application/json' \
  -H 'Idempotency-Key: test-upcloud-'`date +%s` \
  -d '{
    "name": "UpCloud Test User",
    "email": "test@doganconsult.com",
    "message": "Testing DoganSystem deployment on UpCloud Frankfurt server",
    "utmSource": "upcloud",
    "utmMedium": "deployment",
    "utmCampaign": "testing"
  }' \
  -w '\nHTTP Status: %{http_code}\nResponse Time: %{time_total}s\n'

# Check if message was processed
echo ''
echo '📋 Checking message processing...'
sleep 5

# Check worker logs
echo 'Worker logs (last 10 lines):'
docker logs dogansystem-worker-test --tail 10

# Check RabbitMQ queues
echo ''
echo '📊 RabbitMQ queue status:'
docker exec dogansystem-rabbitmq-test rabbitmqctl list_queues name messages

echo ''
echo '✅ Integration test complete!'
"@

    $testScript | ssh -i $SshKeyPath root@$ServerIP "cat > /tmp/test.sh && chmod +x /tmp/test.sh && /tmp/test.sh"
}

Write-Host ""
Write-Host "🎉 DoganSystem UpCloud Deployment Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Server Details:" -ForegroundColor Cyan
Write-Host "   Server: dogansystem" -ForegroundColor White
Write-Host "   IP: 212.147.229.38" -ForegroundColor White
Write-Host "   UUID: 00c35deb-b030-4c6c-86e1-0f238c692e46" -ForegroundColor White
Write-Host "   OS: Ubuntu 24.04 LTS" -ForegroundColor White
Write-Host "   Resources: 2 cores + 8GB RAM + 80GB SSD" -ForegroundColor White
Write-Host ""
Write-Host "🌐 Access Points:" -ForegroundColor Cyan
Write-Host "   API: http://212.147.229.38:5000/api/health" -ForegroundColor White
Write-Host "   Web: http://212.147.229.38:8080" -ForegroundColor White
Write-Host "   Marketing: http://212.147.229.38:3000" -ForegroundColor White
Write-Host "   RabbitMQ: http://212.147.229.38:15672" -ForegroundColor White
Write-Host ""
Write-Host "🔑 SSH Access:" -ForegroundColor Cyan
Write-Host "   ssh -i $SshKeyPath root@$ServerIP" -ForegroundColor White
Write-Host ""
Write-Host "📋 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Configure DNS records for your domains" -ForegroundColor White
Write-Host "2. Setup Cloudflare tunnel token" -ForegroundColor White
Write-Host "3. Test public endpoints" -ForegroundColor White
Write-Host "4. Monitor: ssh -i $SshKeyPath root@$ServerIP 'docker stats'" -ForegroundColor White