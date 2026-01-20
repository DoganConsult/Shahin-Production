# 🐳 Docker Hub Setup for DoganSystem Deployment
# Personal Access Token for drdogan account

param(
    [string]$DockerUsername = "drdogan",
    [string]$DockerToken = $env:DOCKER_HUB_TOKEN,  # Set via: $env:DOCKER_HUB_TOKEN = "your-token"
    [string]$ServerIP = "212.147.229.38",
    [string]$SshKeyPath = "c:\Shahin-ai\shahin_grc_key"
)

Write-Host "🐳 Docker Hub Setup for DoganSystem" -ForegroundColor Green
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Green
Write-Host "Username: $DockerUsername" -ForegroundColor Cyan
Write-Host "Token Expires: Apr 19, 2026" -ForegroundColor Cyan
Write-Host "Permissions: Read, Write, Delete" -ForegroundColor Cyan
Write-Host ""

# Step 1: Login to Docker Hub locally
Write-Host "🔐 Step 1: Logging into Docker Hub locally..." -ForegroundColor Yellow

try {
    # Login with token
    echo $DockerToken | docker login -u $DockerUsername --password-stdin
    Write-Host "✅ Docker Hub login successful" -ForegroundColor Green
}
catch {
    Write-Host "❌ Docker Hub login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Build and push DoganSystem images
Write-Host "`n🏗️ Step 2: Building and Pushing DoganSystem Images..." -ForegroundColor Yellow

$images = @(
    @{Name="dogansystem-api"; Dockerfile="docker/Dockerfile.api"; Context="D:\DoganSystem\DoganSystem"},
    @{Name="dogansystem-web"; Dockerfile="docker/Dockerfile.web"; Context="D:\DoganSystem\DoganSystem"},
    @{Name="dogansystem-worker"; Dockerfile="docker/Dockerfile.worker"; Context="D:\DoganSystem\DoganSystem"}
)

foreach ($image in $images) {
    $imageName = "$DockerUsername/$($image.Name):latest"
    $taggedName = "$DockerUsername/$($image.Name):upcloud-$(Get-Date -Format 'yyyyMMdd')"
    
    Write-Host "   Building $($image.Name)..." -ForegroundColor Gray
    
    # Build image
    docker build -f "$($image.Context)/$($image.Dockerfile)" -t $imageName "$($image.Context)"
    
    # Tag for versioning
    docker tag $imageName $taggedName
    
    # Push to Docker Hub
    Write-Host "   Pushing $imageName..." -ForegroundColor Gray
    docker push $imageName
    docker push $taggedName
    
    Write-Host "✅ $($image.Name) pushed to Docker Hub" -ForegroundColor Green
}

# Step 3: Setup Docker Hub on UpCloud server
Write-Host "`n🖥️ Step 3: Setting up Docker Hub on UpCloud Server..." -ForegroundColor Yellow

$serverSetupScript = @"
#!/bin/bash
set -e

echo '🐳 Setting up Docker Hub on UpCloud server...'

# Login to Docker Hub on server
echo '$DockerToken' | docker login -u $DockerUsername --password-stdin

echo '✅ Docker Hub configured on server'

# Create production docker-compose with Docker Hub images
cat > /opt/dogansystem/docker-compose.upcloud.yml << 'COMPOSE_EOF'
version: "3.8"

# DoganSystem Production Deployment on UpCloud
# Using Docker Hub images for reliable deployment

services:
  postgres:
    image: postgres:16-alpine
    container_name: dogansystem-postgres
    restart: unless-stopped
    environment:
      POSTGRES_USER: dogansystem
      POSTGRES_PASSWORD: \${POSTGRES_PASSWORD}
      POSTGRES_DB: dogansystem
      PGDATA: /var/lib/postgresql/data/pgdata
      # Memory optimization for 8GB server
      POSTGRES_SHARED_BUFFERS: 512MB
      POSTGRES_EFFECTIVE_CACHE_SIZE: 2GB
      POSTGRES_WORK_MEM: 4MB
      POSTGRES_MAX_CONNECTIONS: 50
    volumes:
      - postgres_data:/var/lib/postgresql/data
    deploy:
      resources:
        limits:
          cpus: '1.0'
          memory: 2.5G
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U dogansystem -d dogansystem"]
      interval: 15s
      timeout: 5s
      retries: 3
    networks: [dogansystem]

  redis:
    image: redis:7-alpine
    container_name: dogansystem-redis
    restart: unless-stopped
    command: redis-server --maxmemory 400mb --maxmemory-policy allkeys-lru --requirepass \${REDIS_PASSWORD}
    volumes:
      - redis_data:/data
    deploy:
      resources:
        limits:
          cpus: '0.3'
          memory: 512M
    healthcheck:
      test: ["CMD", "redis-cli", "-a", "\${REDIS_PASSWORD}", "ping"]
      interval: 15s
      timeout: 5s
      retries: 3
    networks: [dogansystem]

  rabbitmq:
    image: rabbitmq:3.12-management-alpine
    container_name: dogansystem-rabbitmq
    restart: unless-stopped
    environment:
      RABBITMQ_DEFAULT_USER: \${RABBITMQ_USER:-dogansystem}
      RABBITMQ_DEFAULT_PASS: \${RABBITMQ_PASSWORD}
      RABBITMQ_VM_MEMORY_HIGH_WATERMARK: 0.6
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
      - ./docker/rabbitmq/rabbitmq.conf:/etc/rabbitmq/rabbitmq.conf:ro
      - ./docker/rabbitmq/definitions.json:/etc/rabbitmq/definitions.json:ro
    deploy:
      resources:
        limits:
          cpus: '0.5'
          memory: 1G
    ports:
      - "5672:5672"
      - "15672:15672"
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "-q", "ping"]
      interval: 15s
      timeout: 5s
      retries: 3
    networks: [dogansystem]

  opa:
    image: openpolicyagent/opa:0.60.0
    container_name: dogansystem-opa
    restart: unless-stopped
    command: ["run", "--server", "--addr=0.0.0.0:8181", "--log-level=warn", "/policies"]
    volumes:
      - ./policy:/policies:ro
    deploy:
      resources:
        limits:
          cpus: '0.2'
          memory: 256M
    ports:
      - "8181:8181"
    healthcheck:
      test: ["CMD", "wget", "-q", "--spider", "http://localhost:8181/health"]
      interval: 15s
      timeout: 5s
      retries: 3
    networks: [dogansystem]

  qdrant:
    image: qdrant/qdrant:v1.7.4
    container_name: dogansystem-qdrant
    restart: unless-stopped
    environment:
      # Memory optimization for 8GB server
      QDRANT__SERVICE__GRPC_PORT: 6334
      QDRANT__SERVICE__MAX_REQUEST_SIZE_MB: 32
      QDRANT__STORAGE__OPTIMIZERS__MEMMAP_THRESHOLD_KB: 100000
      QDRANT__STORAGE__OPTIMIZERS__INDEXING_THRESHOLD_KB: 50000
    volumes:
      - qdrant_data:/qdrant/storage
    deploy:
      resources:
        limits:
          cpus: '0.5'
          memory: 1G
    ports:
      - "6333:6333"
      - "6334:6334"
    healthcheck:
      test: ["CMD", "wget", "-q", "--spider", "http://localhost:6333/health"]
      interval: 15s
      timeout: 5s
      retries: 3
    networks: [dogansystem]

  api:
    image: $DockerUsername/dogansystem-api:latest
    container_name: dogansystem-api
    restart: unless-stopped
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: http://+:80
      DOTNET_GCHeapHardLimit: 1500000000
      DOTNET_GCConserveMemory: 9
      
      # Database
      ConnectionStrings__Default: \${DB_CONNECTION_STRING}
      
      # Services
      OPA__BaseUrl: http://opa:8181
      RabbitMq__Host: rabbitmq
      RabbitMq__Username: \${RABBITMQ_USER:-dogansystem}
      RabbitMq__Password: \${RABBITMQ_PASSWORD}
      Redis__Host: redis
      Redis__Password: \${REDIS_PASSWORD}
      Qdrant__Host: qdrant
      Qdrant__Port: "6333"
      
      # Auth
      Jwt__SecretKey: \${JWT_SECRET_KEY}
      Jwt__Issuer: \${JWT_ISSUER}
      Jwt__Audience: \${JWT_AUDIENCE}
      Cors__Origins: \${CORS_ORIGINS}

    deploy:
      resources:
        limits:
          cpus: '0.5'
          memory: 1.5G
    depends_on:
      postgres: { condition: service_healthy }
      rabbitmq: { condition: service_healthy }
      redis: { condition: service_healthy }
      opa: { condition: service_healthy }
      qdrant: { condition: service_healthy }
    ports:
      - "5000:80"
    networks: [dogansystem]

  web:
    image: $DockerUsername/dogansystem-web:latest
    container_name: dogansystem-web
    restart: unless-stopped
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: http://+:80
      DOTNET_GCHeapHardLimit: 1000000000
      DOTNET_GCConserveMemory: 9
      
      ConnectionStrings__Default: \${DB_CONNECTION_STRING}
      OPA__BaseUrl: http://opa:8181
      Redis__Host: redis
      Redis__Password: \${REDIS_PASSWORD}

    deploy:
      resources:
        limits:
          cpus: '0.3'
          memory: 1G
    depends_on:
      api: { condition: service_started }
    ports:
      - "8080:80"
    networks: [dogansystem]

  worker:
    image: $DockerUsername/dogansystem-worker:latest
    container_name: dogansystem-worker
    restart: unless-stopped
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      DOTNET_GCHeapHardLimit: 1000000000
      
      ConnectionStrings__Default: \${DB_CONNECTION_STRING}
      RabbitMq__Host: rabbitmq
      RabbitMq__Username: \${RABBITMQ_USER:-dogansystem}
      RabbitMq__Password: \${RABBITMQ_PASSWORD}
      Redis__Host: redis
      Redis__Password: \${REDIS_PASSWORD}
      
      # Email settings
      Smtp__Host: \${SMTP_HOST}
      Smtp__Port: \${SMTP_PORT}
      Smtp__Username: \${SMTP_USERNAME}
      Smtp__Password: \${SMTP_PASSWORD}
      Smtp__From: \${SMTP_FROM}

    deploy:
      resources:
        limits:
          cpus: '0.2'
          memory: 1G
    depends_on:
      postgres: { condition: service_healthy }
      rabbitmq: { condition: service_healthy }
      redis: { condition: service_healthy }
    networks: [dogansystem]

networks:
  dogansystem:
    driver: bridge

volumes:
  postgres_data:
  redis_data:
  rabbitmq_data:
  qdrant_data:
COMPOSE_EOF

echo '✅ Docker Hub compose file created'
"@

$serverSetupScript | ssh -i $SshKeyPath root@$ServerIP "cat > /tmp/docker-setup.sh && chmod +x /tmp/docker-setup.sh && /tmp/docker-setup.sh"

# Step 4: Deploy using Docker Hub images
Write-Host "`n🚀 Step 4: Deploying from Docker Hub..." -ForegroundColor Yellow

$deployFromHubScript = @"
#!/bin/bash
cd /opt/dogansystem

echo '🚀 Deploying DoganSystem from Docker Hub...'

# Pull latest images
docker compose -f docker-compose.upcloud.yml pull

# Start services
docker compose -f docker-compose.upcloud.yml up -d

echo '✅ Deployment complete!'
echo ''
echo '📊 Service Status:'
docker ps --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}'

echo ''
echo '📊 Resource Usage:'
docker stats --no-stream --format 'table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}'
"@

$deployFromHubScript | ssh -i $SshKeyPath root@$ServerIP "cat > /tmp/deploy-hub.sh && chmod +x /tmp/deploy-hub.sh && /tmp/deploy-hub.sh"

Write-Host ""
Write-Host "🎉 Docker Hub Deployment Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "🐳 Docker Hub Details:" -ForegroundColor Cyan
Write-Host "   Username: drdogan" -ForegroundColor White
Write-Host "   Token Expires: Apr 19, 2026" -ForegroundColor White
Write-Host "   Permissions: Read, Write, Delete" -ForegroundColor White
Write-Host ""
Write-Host "📦 Images Published:" -ForegroundColor Cyan
Write-Host "   drdogan/dogansystem-api:latest" -ForegroundColor White
Write-Host "   drdogan/dogansystem-web:latest" -ForegroundColor White
Write-Host "   drdogan/dogansystem-worker:latest" -ForegroundColor White
Write-Host ""
Write-Host "🌐 Server Access:" -ForegroundColor Cyan
Write-Host "   API: http://212.147.229.38:5000/api/health" -ForegroundColor White
Write-Host "   Web: http://212.147.229.38:8080" -ForegroundColor White
Write-Host "   RabbitMQ: http://212.147.229.38:15672" -ForegroundColor White