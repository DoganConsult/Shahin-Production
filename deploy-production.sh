#!/bin/bash
# ══════════════════════════════════════════════════════════════
# Shahin AI - Automated Production Deployment Script
# ══════════════════════════════════════════════════════════════
# This script automates the complete production deployment
# Usage: ./deploy-production.sh [options]
#
# Options:
#   --full          Full deployment (default)
#   --landing       Deploy landing page only
#   --portal        Deploy portal only
#   --database      Deploy database only
#   --nginx         Configure nginx only
#   --cloudflare    Setup Cloudflare tunnel only
#   --ssl           Setup SSL certificates only
#   --backup        Create database backup
#   --rollback      Rollback to previous version
#   --status        Show deployment status
#   --logs          Show logs
#   --help          Show this help
# ══════════════════════════════════════════════════════════════

set -e

# ════════════════════════════════════════════════════════════
# Configuration
# ════════════════════════════════════════════════════════════

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
ENV_FILE="$PROJECT_ROOT/.env.production"
BACKUP_DIR="/var/backups/shahin"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

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
    echo -e "${RED}   $2${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

check_root() {
    if [[ $EUID -eq 0 ]]; then
        print_error "This script should not be run as root" "Use a regular user with sudo access"
        exit 1
    fi
}

check_dependencies() {
    local deps=("curl" "wget" "git" "docker" "docker-compose")
    local missing=()

    for dep in "${deps[@]}"; do
        if ! command -v "$dep" &> /dev/null; then
            missing+=("$dep")
        fi
    done

    if [ ${#missing[@]} -ne 0 ]; then
        print_error "Missing dependencies" "Please install: ${missing[*]}"
        echo "  Ubuntu/Debian: sudo apt install -y ${missing[*]}"
        exit 1
    fi
}

check_env_file() {
    if [ ! -f "$ENV_FILE" ]; then
        print_error "Environment file not found" "Copy .env.production.template to .env.production and configure it"
        exit 1
    fi

    # Check for placeholder values
    if grep -q "CHANGE_THIS" "$ENV_FILE"; then
        print_error "Environment file contains placeholder values" "Please update all CHANGE_THIS values in .env.production"
        exit 1
    fi
}

validate_env_vars() {
    local required_vars=("DB_PASSWORD" "JWT_SECRET" "SMTP_PASSWORD")
    local missing_vars=()

    for var in "${required_vars[@]}"; do
        if ! grep -q "^${var}=" "$ENV_FILE" || grep -q "^${var}=CHANGE_THIS" "$ENV_FILE"; then
            missing_vars+=("$var")
        fi
    done

    if [ ${#missing_vars[@]} -ne 0 ]; then
        print_error "Missing required environment variables" "${missing_vars[*]}"
        exit 1
    fi
}

# ════════════════════════════════════════════════════════════
# Deployment Functions
# ════════════════════════════════════════════════════════════

setup_server() {
    print_step "SERVER" "Setting up server environment..."

    # Update system
    sudo apt update && sudo apt upgrade -y

    # Install required packages
    sudo apt install -y curl wget git unzip software-properties-common \
        apt-transport-https ca-certificates gnupg lsb-release nginx htop iotop ncdu

    # Install Docker if not present
    if ! command -v docker &> /dev/null; then
        curl -fsSL https://get.docker.com | sh
        sudo systemctl enable docker
        sudo systemctl start docker
        sudo usermod -aG docker "$USER"
    fi

    # Install Docker Compose if not present
    if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
        sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" \
            -o /usr/local/bin/docker-compose
        sudo chmod +x /usr/local/bin/docker-compose
    fi

    # Create directories
    sudo mkdir -p /var/www/shahin-ai/storage
    sudo mkdir -p /var/backups/shahin
    sudo mkdir -p /var/log/shahin-ai
    sudo chown -R "$USER:$USER" /var/www/shahin-ai
    sudo chown -R "$USER:$USER" /var/backups/shahin
    sudo chown -R "$USER:$USER" /var/log/shahin-ai

    print_success "Server setup complete"
}

deploy_database() {
    print_step "DATABASE" "Deploying PostgreSQL database..."

    # Create network if not exists
    docker network create shahin-network 2>/dev/null || true

    # Start database
    docker-compose -f "$PROJECT_ROOT/docker-compose.production.yml" --env-file "$ENV_FILE" up -d postgres

    # Wait for database to be ready
    print_step "DATABASE" "Waiting for database to be ready..."
    local retries=30
    while [ $retries -gt 0 ]; do
        if docker exec shahin-postgres pg_isready -U "$(grep "DB_USER=" "$ENV_FILE" | cut -d'=' -f2)" -d "$(grep "DB_NAME=" "$ENV_FILE" | cut -d'=' -f2)" &>/dev/null; then
            break
        fi
        sleep 2
        retries=$((retries - 1))
    done

    if [ $retries -eq 0 ]; then
        print_error "Database failed to start" "Check logs with: docker logs shahin-postgres"
        exit 1
    fi

    # Run migrations
    print_step "DATABASE" "Running database migrations..."
    docker-compose -f "$PROJECT_ROOT/docker-compose.production.yml" --env-file "$ENV_FILE" up -d portal
    sleep 10

    # Run EF migrations
    docker exec shahin-portal dotnet ef database update --context GrcDbContext
    docker exec shahin-portal dotnet ef database update --context GrcAuthDbContext

    print_success "Database deployment complete"
}

deploy_landing() {
    print_step "LANDING" "Deploying landing page..."

    # Build and start landing page
    docker-compose -f "$PROJECT_ROOT/docker-compose.production.yml" --env-file "$ENV_FILE" up -d --build landing

    # Health check
    local retries=10
    while [ $retries -gt 0 ]; do
        if curl -sf http://localhost:3000/health &>/dev/null; then
            print_success "Landing page deployment complete"
            return 0
        fi
        sleep 3
        retries=$((retries - 1))
    done

    print_error "Landing page health check failed" "Check logs with: docker logs shahin-landing"
    exit 1
}

deploy_portal() {
    print_step "PORTAL" "Deploying GRC portal..."

    # Build and start portal
    docker-compose -f "$PROJECT_ROOT/docker-compose.production.yml" --env-file "$ENV_FILE" up -d --build portal

    # Health check
    local retries=20
    while [ $retries -gt 0 ]; do
        if curl -sf http://localhost:5000/health &>/dev/null; then
            print_success "Portal deployment complete"
            return 0
        fi
        sleep 5
        retries=$((retries - 1))
    done

    print_error "Portal health check failed" "Check logs with: docker logs shahin-portal"
    exit 1
}

deploy_redis() {
    print_step "REDIS" "Deploying Redis cache..."

    docker-compose -f "$PROJECT_ROOT/docker-compose.production.yml" --env-file "$ENV_FILE" up -d redis

    # Health check
    local retries=10
    while [ $retries -gt 0 ]; do
        if docker exec shahin-redis redis-cli ping | grep -q "PONG"; then
            print_success "Redis deployment complete"
            return 0
        fi
        sleep 2
        retries=$((retries - 1))
    done

    print_error "Redis health check failed" "Check logs with: docker logs shahin-redis"
    exit 1
}

configure_nginx() {
    print_step "NGINX" "Configuring nginx reverse proxy..."

    # Copy nginx configuration
    sudo cp "$PROJECT_ROOT/deployment/nginx/shahin-all-domains.conf" /etc/nginx/sites-available/shahin-ai

    # Enable site
    sudo ln -sf /etc/nginx/sites-available/shahin-ai /etc/nginx/sites-enabled/

    # Remove default site
    sudo rm -f /etc/nginx/sites-enabled/default

    # Test configuration
    if ! sudo nginx -t; then
        print_error "Nginx configuration test failed" "Check syntax errors"
        exit 1
    fi

    # Reload nginx
    sudo systemctl reload nginx
    sudo systemctl enable nginx

    print_success "Nginx configuration complete"
}

setup_cloudflare() {
    print_step "CLOUDFLARE" "Setting up Cloudflare tunnel..."

    # Install cloudflared if not present
    if ! command -v cloudflared &> /dev/null; then
        curl -L --output cloudflared.deb https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64.deb
        sudo dpkg -i cloudflared.deb
    fi

    # Check if tunnel token is configured
    if ! grep -q "CLOUDFLARE_TUNNEL_TOKEN=" "$ENV_FILE" || grep -q "CLOUDFLARE_TUNNEL_TOKEN=CHANGE_THIS" "$ENV_FILE"; then
        print_warning "Cloudflare tunnel token not configured"
        echo "  Please set CLOUDFLARE_TUNNEL_TOKEN in .env.production"
        echo "  Get token from: https://dash.cloudflare.com/profile/api-tokens"
        return 1
    fi

    # Create tunnel configuration
    sudo mkdir -p /etc/cloudflared
    sudo tee /etc/cloudflared/config.yml > /dev/null << EOF
tunnel: shahin-production
credentials-file: /root/.cloudflared/shahin-production.json
warp-routing:
  enabled: true

ingress:
  - hostname: www.shahin-ai.com
    service: http://localhost:3000
  - hostname: shahin-ai.com
    service: http://localhost:3000
  - hostname: portal.shahin-ai.com
    service: http://localhost:5000
  - service: http_status:404
EOF

    # Install as service
    sudo cloudflared service install

    # Start service
    sudo systemctl start cloudflared
    sudo systemctl enable cloudflared

    print_success "Cloudflare tunnel setup complete"
}

setup_ssl() {
    print_step "SSL" "Setting up SSL certificates..."

    # Install certbot
    sudo apt install -y certbot python3-certbot-nginx

    # Note: SSL is handled by Cloudflare, but we can set up local certificates for direct access
    print_warning "SSL certificates are handled by Cloudflare"
    echo "  If you need direct SSL (not through Cloudflare), run:"
    echo "  sudo certbot --nginx -d www.shahin-ai.com -d portal.shahin-ai.com"

    print_success "SSL setup complete (Cloudflare handles certificates)"
}

configure_firewall() {
    print_step "FIREWALL" "Configuring firewall..."

    # Enable UFW
    sudo ufw --force enable

    # Allow required ports
    sudo ufw allow ssh
    sudo ufw allow 80
    sudo ufw allow 443

    # Allow Docker internal communication
    sudo ufw allow from 172.17.0.0/16 to any port 5432 proto tcp  # PostgreSQL
    sudo ufw allow from 172.17.0.0/16 to any port 6379 proto tcp  # Redis

    # Reload firewall
    sudo ufw reload

    print_success "Firewall configuration complete"
}

create_backup() {
    print_step "BACKUP" "Creating database backup..."

    mkdir -p "$BACKUP_DIR"
    local backup_file="$BACKUP_DIR/shahin_backup_$TIMESTAMP.sql.gz"

    docker exec shahin-postgres pg_dump -U "$(grep "DB_USER=" "$ENV_FILE" | cut -d'=' -f2)" \
        "$(grep "DB_NAME=" "$ENV_FILE" | cut -d'=' -f2)" | gzip > "$backup_file"

    # Cleanup old backups (keep last 10)
    ls -t "$BACKUP_DIR"/*.sql.gz 2>/dev/null | tail -n +11 | xargs -r rm

    print_success "Backup created: $backup_file"
    echo "  Size: $(du -h "$backup_file" | cut -f1)"
}

show_status() {
    print_header "DEPLOYMENT STATUS"

    echo -e "${BLUE}Docker Containers:${NC}"
    docker ps --filter "name=shahin" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

    echo ""
    echo -e "${BLUE}Service Health:${NC}"

    # Check each service
    services=("landing:3000" "portal:5000" "postgres:5432" "redis:6379")
    for service in "${services[@]}"; do
        local name="${service%%:*}"
        local port="${service##*:}"

        local status="unhealthy"
        if [ "$name" = "postgres" ]; then
            if docker exec shahin-postgres pg_isready -U "$(grep "DB_USER=" "$ENV_FILE" | cut -d'=' -f2)" \
                -d "$(grep "DB_NAME=" "$ENV_FILE" | cut -d'=' -f2)" &>/dev/null; then
                status="healthy"
            fi
        elif [ "$name" = "redis" ]; then
            if docker exec shahin-redis redis-cli ping 2>/dev/null | grep -q "PONG"; then
                status="healthy"
            fi
        else
            if curl -sf "http://localhost:$port/health" &>/dev/null; then
                status="healthy"
            fi
        fi

        local color=$([ "$status" = "healthy" ] && echo "$GREEN" || echo "$RED")
        echo -e "  $name ($port): ${color}$status${NC}"
    done

    echo ""
    echo -e "${BLUE}System Resources:${NC}"
    echo "  CPU: $(uptime | awk '{print $NF}')"
    echo "  Memory: $(free -h | awk 'NR==2{printf "%.1f%% used", $3*100/$2}')"
    echo "  Disk: $(df -h / | awk 'NR==2{print $5 " used"}')"
}

show_logs() {
    local service="${1:-}"
    if [ -n "$service" ]; then
        docker-compose -f "$PROJECT_ROOT/docker-compose.production.yml" --env-file "$ENV_FILE" logs -f "$service"
    else
        docker-compose -f "$PROJECT_ROOT/docker-compose.production.yml" --env-file "$ENV_FILE" logs -f
    fi
}

rollback_deployment() {
    print_step "ROLLBACK" "Rolling back to previous deployment..."

    # Stop current deployment
    docker-compose -f "$PROJECT_ROOT/docker-compose.production.yml" --env-file "$ENV_FILE" down

    # TODO: Implement rollback logic
    # This would involve:
    # 1. Restoring from backup
    # 2. Starting previous container versions
    # 3. Updating nginx configuration

    print_warning "Rollback not fully implemented yet"
    echo "  Manual rollback required:"
    echo "  1. Restore database from backup"
    echo "  2. Deploy previous container versions"
    echo "  3. Update nginx configuration"
}

# ════════════════════════════════════════════════════════════
# Main Deployment Logic
# ════════════════════════════════════════════════════════════

main() {
    local command="${1:-full}"

    case "$command" in
        --full|full)
            print_header "FULL PRODUCTION DEPLOYMENT"
            check_root
            check_dependencies
            check_env_file
            validate_env_vars
            setup_server
            configure_firewall
            deploy_database
            deploy_redis
            deploy_landing
            deploy_portal
            configure_nginx
            setup_cloudflare
            setup_ssl
            create_backup
            print_header "DEPLOYMENT COMPLETE"
            echo -e "  ${BLUE}Landing Page:${NC} https://www.shahin-ai.com"
            echo -e "  ${BLUE}GRC Portal:${NC}   https://portal.shahin-ai.com"
            echo -e "  ${BLUE}API Docs:${NC}    https://portal.shahin-ai.com/api-docs"
            ;;

        --landing|landing)
            check_env_file
            deploy_landing
            ;;

        --portal|portal)
            check_env_file
            deploy_portal
            ;;

        --database|database)
            check_env_file
            deploy_database
            ;;

        --nginx|nginx)
            configure_nginx
            ;;

        --cloudflare|cloudflare)
            check_env_file
            setup_cloudflare
            ;;

        --ssl|ssl)
            setup_ssl
            ;;

        --backup|backup)
            check_env_file
            create_backup
            ;;

        --rollback|rollback)
            rollback_deployment
            ;;

        --status|status)
            show_status
            ;;

        --logs|logs)
            show_logs "$2"
            ;;

        --help|help|-h)
            echo "Shahin AI Production Deployment Script"
            echo ""
            echo "Usage: $0 [command]"
            echo ""
            echo "Commands:"
            echo "  --full, full          Full production deployment (default)"
            echo "  --landing, landing    Deploy landing page only"
            echo "  --portal, portal      Deploy portal only"
            echo "  --database, database  Deploy database only"
            echo "  --nginx, nginx        Configure nginx only"
            echo "  --cloudflare          Setup Cloudflare tunnel only"
            echo "  --ssl, ssl            Setup SSL certificates only"
            echo "  --backup, backup      Create database backup"
            echo "  --rollback            Rollback to previous version"
            echo "  --status, status      Show deployment status"
            echo "  --logs, logs [svc]    Show logs (optional service name)"
            echo "  --help, help, -h      Show this help"
            ;;

        *)
            print_error "Unknown command: $command" "Use --help for available commands"
            exit 1
            ;;
    esac
}

# Run main function with all arguments
main "$@"
