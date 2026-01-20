#!/bin/bash
# ══════════════════════════════════════════════════════════════
# Shahin AI - Production Deployment Script
# Server: 212.147.229.36 (Ubuntu Server 24.04 LTS)
# UUID: 007d5050-ade4-432d-b2bb-abc85c74a3b2
# ══════════════════════════════════════════════════════════════
# Usage: ./deploy-to-production.sh [command]
# Commands:
#   build     - Build all 5 containers locally
#   deploy     - Deploy to production server via SSH
#   rebuild    - Rebuild and deploy all containers
#   status     - Check container status on server
#   logs       - View logs from server
#   stop       - Stop all containers on server
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
DEPLOY_DIR="/opt/shahin-ai"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
COMPOSE_FILE="$SCRIPT_DIR/docker-compose.production-server.yml"
ENV_FILE="$SCRIPT_DIR/.env.production"

# ════════════════════════════════════════════════════════════
# Helper Functions
# ════════════════════════════════════════════════════════════

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

check_requirements() {
    print_step "CHECK" "Verifying requirements..."

    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed locally"
        exit 1
    fi

    if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
        print_error "Docker Compose is not installed locally"
        exit 1
    fi

    if [ ! -f "$ENV_FILE" ]; then
        print_error "Environment file not found: $ENV_FILE"
        echo "  Creating from template..."
        create_env_file
    fi

    # Check SSH connection
    print_step "CHECK" "Testing SSH connection to $SERVER_IP..."
    if ssh -i "$SSH_KEY" -o StrictHostKeyChecking=no "$SERVER_USER@$SERVER_IP" "echo 'SSH connection successful'" &>/dev/null; then
        print_success "SSH connection successful"
    else
        print_error "Cannot connect to server. Check SSH key and network."
        exit 1
    fi

    print_success "All requirements met"
}

create_env_file() {
    if [ ! -f "$ENV_FILE" ]; then
        cat > "$ENV_FILE" << EOF
# Production Environment Variables
# Server: 212.147.229.36

# Database
DB_USER=shahin_admin
DB_PASSWORD=CHANGE_ME_STRONG_PASSWORD
DB_NAME=shahin_grc

# JWT
JWT_SECRET=CHANGE_ME_GENERATE_STRONG_SECRET_KEY

# Redis
REDIS_PASSWORD=

# SMTP (Optional)
SMTP_SERVER=
SMTP_PORT=587
SMTP_USERNAME=
SMTP_PASSWORD=

# Claude AI
CLAUDE_API_KEY=CHANGE_ME
EOF
        print_success "Created $ENV_FILE - Please update with your values"
    fi
}

# ════════════════════════════════════════════════════════════
# Build Functions
# ════════════════════════════════════════════════════════════

build_containers() {
    print_header "BUILDING ALL 5 CONTAINERS"

    check_requirements

    print_step "1/5" "Building Landing Page (Frontend)..."
    cd "$PROJECT_ROOT/grc-frontend"
    docker build -t shahin-landing:latest -f Dockerfile .

    print_step "2/5" "Building Portal (Backend)..."
    cd "$PROJECT_ROOT/src/GrcMvc"
    docker build -t shahin-portal:latest -f Dockerfile.production .

    print_step "3/5" "Pulling PostgreSQL..."
    docker pull postgres:16-alpine

    print_step "4/5" "Pulling Redis..."
    docker pull redis:7-alpine

    print_step "5/5" "Pulling Nginx..."
    docker pull nginx:alpine

    print_success "All containers built/pulled successfully"
}

# ════════════════════════════════════════════════════════════
# Deployment Functions
# ════════════════════════════════════════════════════════════

setup_server() {
    print_step "SETUP" "Setting up server environment..."

    ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" << 'ENDSSH'
        # Install Docker if not present
        if ! command -v docker &> /dev/null; then
            echo "Installing Docker..."
            curl -fsSL https://get.docker.com -o get-docker.sh
            sh get-docker.sh
            rm get-docker.sh
        fi

        # Install Docker Compose if not present
        if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
            echo "Installing Docker Compose..."
            apt-get update
            apt-get install -y docker-compose-plugin
        fi

        # Create deployment directory
        mkdir -p /opt/shahin-ai/{nginx,init-db}
        chmod 755 /opt/shahin-ai
ENDSSH

    print_success "Server setup complete"
}

deploy_files() {
    print_step "DEPLOY" "Uploading files to server..."

    # Create deployment package
    TEMP_DIR=$(mktemp -d)
    cp "$COMPOSE_FILE" "$TEMP_DIR/docker-compose.yml"
    cp "$ENV_FILE" "$TEMP_DIR/.env"
    cp "$SCRIPT_DIR/nginx/production-212.147.229.36.conf" "$TEMP_DIR/nginx.conf"

    # Upload to server
    scp -i "$SSH_KEY" -r "$TEMP_DIR"/* "$SERVER_USER@$SERVER_IP:$DEPLOY_DIR/"

    # Cleanup
    rm -rf "$TEMP_DIR"

    print_success "Files uploaded"
}

deploy_containers() {
    print_header "DEPLOYING TO PRODUCTION SERVER"

    check_requirements
    setup_server
    deploy_files

    print_step "1/4" "Stopping existing containers..."
    ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" "cd $DEPLOY_DIR && docker-compose down || true"

    print_step "2/4" "Pulling/updating images..."
    ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" "cd $DEPLOY_DIR && docker-compose pull || true"

    print_step "3/4" "Building and starting containers..."
    ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" << ENDSSH
        cd $DEPLOY_DIR
        docker-compose up -d --build
        docker-compose ps
ENDSSH

    print_step "4/4" "Waiting for services to start..."
    sleep 15

    # Health checks
    print_step "HEALTH" "Checking service health..."
    check_health

    print_header "DEPLOYMENT COMPLETE"
    echo -e "  ${BLUE}Server:${NC}    http://$SERVER_IP"
    echo -e "  ${BLUE}Landing:${NC}   http://$SERVER_IP:3000"
    echo -e "  ${BLUE}Portal:${NC}    http://$SERVER_IP:5000"
    echo -e "  ${BLUE}API Docs:${NC}  http://$SERVER_IP:5000/api-docs"
    echo ""
}

rebuild_all() {
    print_header "REBUILDING ALL CONTAINERS"
    build_containers
    deploy_containers
}

check_health() {
    ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" << 'ENDSSH'
        echo "Container Status:"
        docker ps --filter "name=shahin" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
        echo ""
        echo "Health Checks:"
        curl -sf http://localhost:3000/health && echo "✅ Landing (3000): HEALTHY" || echo "❌ Landing (3000): UNHEALTHY"
        curl -sf http://localhost:5000/health && echo "✅ Portal (5000): HEALTHY" || echo "❌ Portal (5000): UNHEALTHY"
ENDSSH
}

show_status() {
    print_header "CONTAINER STATUS"
    ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" << 'ENDSSH'
        cd /opt/shahin-ai
        docker-compose ps
        echo ""
        docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}"
ENDSSH
}

show_logs() {
    SERVICE="${1:-}"
    if [ -n "$SERVICE" ]; then
        ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" "cd $DEPLOY_DIR && docker-compose logs -f $SERVICE"
    else
        ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" "cd $DEPLOY_DIR && docker-compose logs -f"
    fi
}

stop_containers() {
    print_header "STOPPING CONTAINERS"
    ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" "cd $DEPLOY_DIR && docker-compose down"
    print_success "All containers stopped"
}

# ════════════════════════════════════════════════════════════
# Main
# ════════════════════════════════════════════════════════════

COMMAND="${1:-rebuild}"

case "$COMMAND" in
    build)
        build_containers
        ;;
    deploy)
        deploy_containers
        ;;
    rebuild)
        rebuild_all
        ;;
    status)
        show_status
        ;;
    logs)
        show_logs "$2"
        ;;
    stop)
        stop_containers
        ;;
    *)
        echo "Usage: $0 {build|deploy|rebuild|status|logs|stop}"
        echo ""
        echo "Commands:"
        echo "  build     - Build all 5 containers locally"
        echo "  deploy    - Deploy to production server"
        echo "  rebuild   - Rebuild and deploy all containers (default)"
        echo "  status    - Show container status on server"
        echo "  logs      - View logs (optional: service name)"
        echo "  stop      - Stop all containers on server"
        exit 1
        ;;
esac
