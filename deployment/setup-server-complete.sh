#!/bin/bash
# ══════════════════════════════════════════════════════════════
# Complete Server Setup Script
# 1. Push all images to Docker Hub
# 2. Connect via SSH to server
# 3. Download and setup everything on server
# 4. Deploy all containers
# ══════════════════════════════════════════════════════════════

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

# Configuration
SERVER_IP="212.147.229.36"
SERVER_USER="root"
SSH_KEY="${SSH_KEY:-~/.ssh/id_ed25519}"
DOCKERHUB_USER="${DOCKERHUB_USER:-drdogan}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
DEPLOY_DIR="/opt/shahin-ai"

print_header() {
    echo ""
    echo -e "${CYAN}══════════════════════════════════════════════════════════════${NC}"
    echo -e "${CYAN}  $1${NC}"
    echo -e "${CYAN}══════════════════════════════════════════════════════════════${NC}"
    echo ""
}

print_step() {
    echo -e "${YELLOW}[$1] $2${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

# ════════════════════════════════════════════════════════════
# Step 1: Push to Docker Hub
# ════════════════════════════════════════════════════════════
push_to_dockerhub() {
    print_header "STEP 1: PUSH ALL IMAGES TO DOCKER HUB"
    
    # Check Docker login
    if ! docker info | grep -q "Username"; then
        print_step "1.0" "Logging into Docker Hub..."
        docker login
    fi
    
    # Build and push Landing
    print_step "1.1" "Building Landing Page..."
    cd "$PROJECT_ROOT/grc-frontend"
    docker build -t "$DOCKERHUB_USER/shahin-landing:latest" -f Dockerfile .
    docker tag "$DOCKERHUB_USER/shahin-landing:latest" "$DOCKERHUB_USER/shahin-landing:$(date +%Y%m%d)"
    
    print_step "1.2" "Pushing Landing to Docker Hub..."
    docker push "$DOCKERHUB_USER/shahin-landing:latest"
    docker push "$DOCKERHUB_USER/shahin-landing:$(date +%Y%m%d)"
    print_success "Landing Page pushed to Docker Hub"
    
    # Build and push Portal
    print_step "1.3" "Building Portal (Backend)..."
    cd "$PROJECT_ROOT/src/GrcMvc"
    docker build -t "$DOCKERHUB_USER/shahin-portal:latest" -f Dockerfile.production .
    docker tag "$DOCKERHUB_USER/shahin-portal:latest" "$DOCKERHUB_USER/shahin-portal:$(date +%Y%m%d)"
    
    print_step "1.4" "Pushing Portal to Docker Hub..."
    docker push "$DOCKERHUB_USER/shahin-portal:latest"
    docker push "$DOCKERHUB_USER/shahin-portal:$(date +%Y%m%d)"
    print_success "Portal pushed to Docker Hub"
    
    echo ""
    print_success "All images pushed to Docker Hub: $DOCKERHUB_USER"
    return 0
}

# ════════════════════════════════════════════════════════════
# Step 2: Setup Server via SSH
# ════════════════════════════════════════════════════════════
setup_server() {
    print_header "STEP 2: SETUP SERVER VIA SSH"
    
    # Test SSH connection
    print_step "2.1" "Testing SSH connection..."
    if ! ssh -i "$SSH_KEY" -o StrictHostKeyChecking=no "$SERVER_USER@$SERVER_IP" "echo 'SSH connection successful'" &>/dev/null; then
        print_error "Cannot connect to server. Check SSH key: $SSH_KEY"
        exit 1
    fi
    print_success "SSH connection verified"
    
    # Install Docker and Docker Compose
    print_step "2.2" "Installing Docker and Docker Compose..."
    ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" << 'ENDSSH'
        # Install Docker if not present
        if ! command -v docker &> /dev/null; then
            echo "Installing Docker..."
            curl -fsSL https://get.docker.com -o get-docker.sh
            sh get-docker.sh
            rm get-docker.sh
            systemctl enable docker
            systemctl start docker
        fi
        
        # Install Docker Compose if not present
        if ! docker compose version &> /dev/null; then
            echo "Installing Docker Compose..."
            apt-get update
            apt-get install -y docker-compose-plugin
        fi
        
        # Create deployment directory
        mkdir -p /opt/shahin-ai/{nginx,init-db,ssl}
        chmod 755 /opt/shahin-ai
        
        echo "✅ Docker and Docker Compose installed"
ENDSSH
    print_success "Server setup complete"
}

# ════════════════════════════════════════════════════════════
# Step 3: Download and Upload Files to Server
# ════════════════════════════════════════════════════════════
upload_files() {
    print_header "STEP 3: UPLOAD FILES TO SERVER"
    
    # Check environment file
    ENV_FILE="$SCRIPT_DIR/.env.production"
    if [ ! -f "$ENV_FILE" ]; then
        print_error "Environment file not found: $ENV_FILE"
        print_step "INFO" "Creating from template..."
        if [ -f "$SCRIPT_DIR/.env.production.template" ]; then
            cp "$SCRIPT_DIR/.env.production.template" "$ENV_FILE"
            print_error "Please edit .env.production with actual production keys before continuing"
            exit 1
        else
            print_error "Template file not found. Please create .env.production manually"
            exit 1
        fi
    fi
    
    # Verify critical environment variables
    print_step "3.1" "Verifying environment variables..."
    source "$ENV_FILE"
    
    if [ -z "$DB_PASSWORD" ] || [ "$DB_PASSWORD" = "CHANGE_ME_STRONG_PASSWORD_HERE" ]; then
        print_error "DB_PASSWORD not set in .env.production"
        exit 1
    fi
    
    if [ -z "$JWT_SECRET" ] || [ "$JWT_SECRET" = "CHANGE_ME_GENERATE_STRONG_SECRET_KEY_MIN_32_CHARS" ]; then
        print_error "JWT_SECRET not set in .env.production"
        exit 1
    fi
    
    print_success "Environment variables verified"
    
    # Create deployment package
    print_step "3.2" "Preparing deployment files..."
    TEMP_DIR=$(mktemp -d)
    
    # Copy files
    cp "$SCRIPT_DIR/docker-compose.production-server.yml" "$TEMP_DIR/docker-compose.yml"
    cp "$ENV_FILE" "$TEMP_DIR/.env"
    cp "$SCRIPT_DIR/nginx/production-212.147.229.36.conf" "$TEMP_DIR/nginx.conf"
    
    # Create init-db directory if needed
    mkdir -p "$TEMP_DIR/init-db"
    if [ -d "$SCRIPT_DIR/init-db" ]; then
        cp -r "$SCRIPT_DIR/init-db"/* "$TEMP_DIR/init-db/" 2>/dev/null || true
    fi
    
    # Upload to server
    print_step "3.3" "Uploading files to server..."
    scp -i "$SSH_KEY" -r "$TEMP_DIR"/* "$SERVER_USER@$SERVER_IP:$DEPLOY_DIR/"
    
    # Cleanup
    rm -rf "$TEMP_DIR"
    
    print_success "Files uploaded to server"
}

# ════════════════════════════════════════════════════════════
# Step 4: Deploy All Containers
# ════════════════════════════════════════════════════════════
deploy_containers() {
    print_header "STEP 4: DEPLOY ALL CONTAINERS"
    
    print_step "4.1" "Pulling images from Docker Hub..."
    ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" << ENDSSH
        cd $DEPLOY_DIR
        export DOCKERHUB_USER=$DOCKERHUB_USER
        docker-compose pull || true
ENDSSH
    
    print_step "4.2" "Stopping existing containers..."
    ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" "cd $DEPLOY_DIR && docker-compose down || true"
    
    print_step "4.3" "Starting all containers..."
    ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" << ENDSSH
        cd $DEPLOY_DIR
        export DOCKERHUB_USER=$DOCKERHUB_USER
        docker-compose up -d --build
        sleep 30
        docker-compose ps
ENDSSH
    
    print_success "All containers deployed"
}

# ════════════════════════════════════════════════════════════
# Step 5: Verify Deployment
# ════════════════════════════════════════════════════════════
verify_deployment() {
    print_header "STEP 5: VERIFY DEPLOYMENT"
    
    print_step "5.1" "Waiting for services to start..."
    sleep 20
    
    # Check container status
    print_step "5.2" "Checking container status..."
    ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" << 'ENDSSH'
        echo "Container Status:"
        docker ps --filter "name=shahin" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
        echo ""
        echo "All containers running:"
        docker ps --filter "name=shahin" --format "{{.Names}}" | wc -l
ENDSSH
    
    # Test endpoints
    print_step "5.3" "Testing endpoints..."
    ENDPOINTS=(
        "http://$SERVER_IP:3000/health|Landing Health"
        "http://$SERVER_IP:5000/health|Portal Health"
        "http://$SERVER_IP:5000/api/health|API Health"
        "http://$SERVER_IP:11434/api/tags|Ollama API"
    )
    
    FAILED=0
    for endpoint in "${ENDPOINTS[@]}"; do
        IFS='|' read -r url name <<< "$endpoint"
        HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 "$url" 2>/dev/null || echo "000")
        
        if [ "$HTTP_CODE" = "200" ] || [ "$HTTP_CODE" = "301" ] || [ "$HTTP_CODE" = "302" ]; then
            print_success "$name: HTTP $HTTP_CODE"
        else
            print_error "$name: HTTP $HTTP_CODE"
            FAILED=$((FAILED + 1))
        fi
    done
    
    # Test admin paths
    print_step "5.4" "Testing admin paths..."
    ADMIN_PATHS=(
        "http://$SERVER_IP:5000/admin/login|Admin Login"
        "http://$SERVER_IP:5000/platform-admin|Platform Admin"
    )
    
    for path in "${ADMIN_PATHS[@]}"; do
        IFS='|' read -r url name <<< "$path"
        HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 "$url" 2>/dev/null || echo "000")
        
        if [ "$HTTP_CODE" = "200" ] || [ "$HTTP_CODE" = "301" ] || [ "$HTTP_CODE" = "302" ]; then
            print_success "$name: HTTP $HTTP_CODE"
        else
            print_info "$name: HTTP $HTTP_CODE (may require authentication)"
        fi
    done
    
    # Database and Redis
    print_step "5.5" "Testing database and Redis..."
    DB_TEST=$(ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" "docker exec shahin-postgres-prod pg_isready -U shahin_admin 2>&1" || echo "failed")
    if echo "$DB_TEST" | grep -q "accepting connections"; then
        print_success "Database: Accepting connections"
    else
        print_error "Database: Connection failed"
        FAILED=$((FAILED + 1))
    fi
    
    REDIS_TEST=$(ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" "docker exec shahin-redis-prod redis-cli ping 2>&1" || echo "failed")
    if echo "$REDIS_TEST" | grep -q "PONG"; then
        print_success "Redis: Connected"
    else
        print_error "Redis: Connection failed"
        FAILED=$((FAILED + 1))
    fi
    
    if [ $FAILED -eq 0 ]; then
        print_success "All services verified"
        return 0
    else
        print_error "$FAILED service(s) failed verification"
        return 1
    fi
}

# ════════════════════════════════════════════════════════════
# Main Execution
# ════════════════════════════════════════════════════════════

print_header "COMPLETE SERVER SETUP AND DEPLOYMENT"

# Step 1: Push to Docker Hub
if ! push_to_dockerhub; then
    print_error "Docker Hub push failed"
    exit 1
fi
echo ""

# Step 2: Setup Server
if ! setup_server; then
    print_error "Server setup failed"
    exit 1
fi
echo ""

# Step 3: Upload Files
if ! upload_files; then
    print_error "File upload failed"
    exit 1
fi
echo ""

# Step 4: Deploy Containers
if ! deploy_containers; then
    print_error "Deployment failed"
    exit 1
fi
echo ""

# Step 5: Verify
if ! verify_deployment; then
    print_error "Verification failed - check logs"
    exit 1
fi
echo ""

# Final Summary
print_header "DEPLOYMENT COMPLETE"
echo -e "${GREEN}All services deployed and verified${NC}"
echo ""
echo -e "${BLUE}Access URLs:${NC}"
echo -e "  Landing:    http://$SERVER_IP:3000"
echo -e "  Portal:     http://$SERVER_IP:5000"
echo -e "  Admin:      http://$SERVER_IP:5000/admin/login"
echo -e "  Platform:   http://$SERVER_IP:5000/platform-admin"
echo -e "  API Docs:   http://$SERVER_IP:5000/api-docs"
echo -e "  Ollama:     http://$SERVER_IP:11434"
echo ""
echo -e "${BLUE}DNS Domains:${NC}"
echo -e "  www.shahin-ai.com    → Landing Page"
echo -e "  portal.shahin-ai.com → Portal/API"
echo -e "  admin.shahin-ai.com  → Admin Portal (/admin, /platform-admin)"
echo -e "  login.shahin-ai.com  → Login Page"
echo ""
echo -e "${GREEN}✅ Production deployment successful!${NC}"
echo ""
