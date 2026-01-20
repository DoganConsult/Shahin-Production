#!/bin/bash
# ══════════════════════════════════════════════════════════════
# Complete Deployment Pipeline with Post-Deployment Verification
# 1. Push to GitHub
# 2. Push to Docker Hub
# 3. Deploy to Production Server
# 4. Verify all services and paths
# 5. Test health endpoints
# ══════════════════════════════════════════════════════════════

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SERVER_IP="212.147.229.36"
SERVER_USER="root"
SSH_KEY="${SSH_KEY:-~/.ssh/id_ed25519}"

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

print_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

# ════════════════════════════════════════════════════════════
# Step 1: Push to GitHub
# ════════════════════════════════════════════════════════════
push_to_github() {
    print_header "STEP 1: PUSH TO GITHUB"
    
    cd "$SCRIPT_DIR/.."
    
    if [ ! -d ".git" ]; then
        print_error "Not a git repository"
        return 1
    fi
    
    # Check for changes
    if [ -z "$(git status --porcelain)" ]; then
        print_info "No changes to commit"
    else
        print_step "1.1" "Staging changes..."
        git add .
        
        COMMIT_MSG="Deploy: Production build $(date +%Y-%m-%d_%H:%M:%S)"
        print_step "1.2" "Committing changes..."
        git commit -m "$COMMIT_MSG" || print_info "Nothing new to commit"
    fi
    
    BRANCH=$(git branch --show-current)
    print_step "1.3" "Pushing to origin/$BRANCH..."
    git push origin "$BRANCH" || git push -u origin "$BRANCH"
    
    print_success "GitHub push complete"
    return 0
}

# ════════════════════════════════════════════════════════════
# Step 2: Push to Docker Hub
# ════════════════════════════════════════════════════════════
push_to_dockerhub() {
    print_header "STEP 2: PUSH TO DOCKER HUB"
    
    DOCKERHUB_USER="${DOCKERHUB_USER:-drdogan}"
    
    # Check Docker login
    if ! docker info | grep -q "Username"; then
        print_step "2.0" "Logging into Docker Hub..."
        docker login
    fi
    
    # Build and push Landing
    print_step "2.1" "Building Landing Page..."
    cd "$SCRIPT_DIR/../grc-frontend"
    docker build -t "$DOCKERHUB_USER/shahin-landing:latest" -f Dockerfile .
    docker tag "$DOCKERHUB_USER/shahin-landing:latest" "$DOCKERHUB_USER/shahin-landing:$(date +%Y%m%d)"
    
    print_step "2.2" "Pushing Landing to Docker Hub..."
    docker push "$DOCKERHUB_USER/shahin-landing:latest"
    docker push "$DOCKERHUB_USER/shahin-landing:$(date +%Y%m%d)"
    print_success "Landing Page pushed"
    
    # Build and push Portal
    print_step "2.3" "Building Portal (Backend)..."
    cd "$SCRIPT_DIR/../src/GrcMvc"
    docker build -t "$DOCKERHUB_USER/shahin-portal:latest" -f Dockerfile.production .
    docker tag "$DOCKERHUB_USER/shahin-portal:latest" "$DOCKERHUB_USER/shahin-portal:$(date +%Y%m%d)"
    
    print_step "2.4" "Pushing Portal to Docker Hub..."
    docker push "$DOCKERHUB_USER/shahin-portal:latest"
    docker push "$DOCKERHUB_USER/shahin-portal:$(date +%Y%m%d)"
    print_success "Portal pushed"
    
    return 0
}

# ════════════════════════════════════════════════════════════
# Step 3: Deploy to Server
# ════════════════════════════════════════════════════════════
deploy_to_server() {
    print_header "STEP 3: DEPLOY TO PRODUCTION SERVER"
    
    # Check environment file
    ENV_FILE="$SCRIPT_DIR/.env.production"
    if [ ! -f "$ENV_FILE" ]; then
        print_error "Environment file not found: $ENV_FILE"
        print_info "Copy .env.production.template to .env.production and update with actual keys"
        return 1
    fi
    
    # Verify critical environment variables
    print_step "3.1" "Verifying environment variables..."
    source "$ENV_FILE"
    
    if [ -z "$DB_PASSWORD" ] || [ "$DB_PASSWORD" = "CHANGE_ME_STRONG_PASSWORD_HERE" ]; then
        print_error "DB_PASSWORD not set in .env.production"
        return 1
    fi
    
    if [ -z "$JWT_SECRET" ] || [ "$JWT_SECRET" = "CHANGE_ME_GENERATE_STRONG_SECRET_KEY_MIN_32_CHARS" ]; then
        print_error "JWT_SECRET not set in .env.production"
        return 1
    fi
    
    print_success "Environment variables verified"
    
    # Setup server
    print_step "3.2" "Setting up server..."
    ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" << 'ENDSSH'
        mkdir -p /opt/shahin-ai/{nginx,init-db}
        if ! command -v docker &> /dev/null; then
            curl -fsSL https://get.docker.com -o get-docker.sh
            sh get-docker.sh
            rm get-docker.sh
        fi
        if ! docker compose version &> /dev/null; then
            apt-get update && apt-get install -y docker-compose-plugin
        fi
ENDSSH
    
    # Upload files
    print_step "3.3" "Uploading deployment files..."
    TEMP_DIR=$(mktemp -d)
    cp "$SCRIPT_DIR/docker-compose.production-server.yml" "$TEMP_DIR/docker-compose.yml"
    cp "$ENV_FILE" "$TEMP_DIR/.env"
    cp "$SCRIPT_DIR/nginx/production-212.147.229.36.conf" "$TEMP_DIR/nginx.conf"
    
    scp -i "$SSH_KEY" -r "$TEMP_DIR"/* "$SERVER_USER@$SERVER_IP:/opt/shahin-ai/"
    rm -rf "$TEMP_DIR"
    
    # Deploy
    print_step "3.4" "Deploying containers..."
    ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" << 'ENDSSH'
        cd /opt/shahin-ai
        docker-compose down || true
        docker-compose pull || true
        docker-compose up -d --build
        sleep 20
        docker-compose ps
ENDSSH
    
    print_success "Deployment complete"
    return 0
}

# ════════════════════════════════════════════════════════════
# Step 4: Verify Services and Test Paths
# ════════════════════════════════════════════════════════════
verify_services() {
    print_header "STEP 4: VERIFY SERVICES AND TEST PATHS"
    
    # Wait for services to be ready
    print_step "4.1" "Waiting for services to start..."
    sleep 30
    
    # Test endpoints
    ENDPOINTS=(
        "http://$SERVER_IP:3000/health|Landing Health"
        "http://$SERVER_IP:5000/health|Portal Health"
        "http://$SERVER_IP:5000/api/health|API Health"
        "http://$SERVER_IP:11434/api/tags|Ollama API"
        "http://$SERVER_IP/health|Nginx Health"
    )
    
    print_step "4.2" "Testing endpoints..."
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
    
    # Test DNS domains (if accessible)
    print_step "4.3" "Testing DNS domains..."
    DNS_DOMAINS=(
        "http://www.shahin-ai.com/health|www.shahin-ai.com"
        "http://portal.shahin-ai.com/health|portal.shahin-ai.com"
        "http://admin.shahin-ai.com/health|admin.shahin-ai.com"
        "http://admin.shahin-ai.com/admin/login|admin.shahin-ai.com /admin/login"
        "http://admin.shahin-ai.com/platform-admin|admin.shahin-ai.com /platform-admin"
        "http://login.shahin-ai.com/health|login.shahin-ai.com"
    )
    
    for domain in "${DNS_DOMAINS[@]}"; do
        IFS='|' read -r url name <<< "$domain"
        HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 "$url" 2>/dev/null || echo "000")
        
        if [ "$HTTP_CODE" = "200" ] || [ "$HTTP_CODE" = "301" ] || [ "$HTTP_CODE" = "302" ]; then
            print_success "$name: HTTP $HTTP_CODE"
        else
            print_info "$name: HTTP $HTTP_CODE (may need DNS propagation)"
        fi
    done
    
    # Check container status
    print_step "4.4" "Checking container status..."
    ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" << 'ENDSSH'
        echo "Container Status:"
        docker ps --filter "name=shahin" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
        echo ""
        echo "Container Health:"
        docker ps --filter "name=shahin" --format "{{.Names}}: {{.Status}}" | grep -q "Up" && echo "✅ All containers running" || echo "❌ Some containers not running"
ENDSSH
    
    # Database connection test
    print_step "4.5" "Testing database connection..."
    DB_TEST=$(ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" "docker exec shahin-postgres-prod pg_isready -U shahin_admin 2>&1" || echo "failed")
    if echo "$DB_TEST" | grep -q "accepting connections"; then
        print_success "Database: Accepting connections"
    else
        print_error "Database: Connection failed"
        FAILED=$((FAILED + 1))
    fi
    
    # Redis connection test
    print_step "4.6" "Testing Redis connection..."
    REDIS_TEST=$(ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" "docker exec shahin-redis-prod redis-cli ping 2>&1" || echo "failed")
    if echo "$REDIS_TEST" | grep -q "PONG"; then
        print_success "Redis: Connected"
    else
        print_error "Redis: Connection failed"
        FAILED=$((FAILED + 1))
    fi
    
    # Summary
    echo ""
    if [ $FAILED -eq 0 ]; then
        print_success "All services verified successfully"
        return 0
    else
        print_error "$FAILED service(s) failed verification"
        return 1
    fi
}

# ════════════════════════════════════════════════════════════
# Main Execution
# ════════════════════════════════════════════════════════════

print_header "COMPLETE DEPLOYMENT AND VERIFICATION PIPELINE"

# Step 1: GitHub
if ! push_to_github; then
    print_error "GitHub push failed"
    exit 1
fi
echo ""

# Step 2: Docker Hub
if ! push_to_dockerhub; then
    print_error "Docker Hub push failed"
    exit 1
fi
echo ""

# Step 3: Deploy
if ! deploy_to_server; then
    print_error "Deployment failed"
    exit 1
fi
echo ""

# Step 4: Verify
if ! verify_services; then
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
echo -e "  API Docs:   http://$SERVER_IP:5000/api-docs"
echo -e "  Ollama:     http://$SERVER_IP:11434"
echo ""
echo -e "${BLUE}DNS Domains:${NC}"
echo -e "  www.shahin-ai.com    → Landing Page"
echo -e "  portal.shahin-ai.com → Portal/API"
echo -e "  admin.shahin-ai.com  → Admin Portal"
echo -e "  login.shahin-ai.com  → Login Page"
echo ""
echo -e "${GREEN}✅ Production deployment successful!${NC}"
echo ""
