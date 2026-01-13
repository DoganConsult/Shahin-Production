#!/bin/bash
# ══════════════════════════════════════════════════════════════
# Shahin AI - Full Stack Production Deployment
# ══════════════════════════════════════════════════════════════
# Usage: ./deploy-full-stack.sh [command]
# Commands:
#   deploy    - Full deployment (default)
#   landing   - Deploy landing page only
#   portal    - Deploy portal only
#   status    - Check service status
#   logs      - View logs
#   stop      - Stop all services
#   restart   - Restart all services
#   backup    - Backup database
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
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
COMPOSE_FILE="$SCRIPT_DIR/docker-compose.production.yml"
ENV_FILE="$SCRIPT_DIR/.env"
BACKUP_DIR="/var/backups/shahin"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

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
    print_step "PRE" "Checking requirements..."

    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed"
        exit 1
    fi

    if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
        print_error "Docker Compose is not installed"
        exit 1
    fi

    if [ ! -f "$ENV_FILE" ]; then
        print_error "Environment file not found: $ENV_FILE"
        echo "  Copy .env.production.template to .env and configure it"
        exit 1
    fi

    print_success "All requirements met"
}

# ════════════════════════════════════════════════════════════
# Deployment Functions
# ════════════════════════════════════════════════════════════

deploy_full() {
    print_header "FULL STACK DEPLOYMENT"
    check_requirements

    # Pull latest images
    print_step "1/5" "Pulling latest images..."
    docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" pull

    # Create network if not exists
    print_step "2/5" "Creating network..."
    docker network create shahin-network 2>/dev/null || true

    # Start infrastructure (postgres, redis)
    print_step "3/5" "Starting infrastructure..."
    docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d postgres redis
    sleep 10

    # Start applications
    print_step "4/5" "Starting applications..."
    docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d landing portal

    # Health checks
    print_step "5/5" "Running health checks..."
    sleep 15

    LANDING_HEALTH=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:3000/health 2>/dev/null || echo "000")
    PORTAL_HEALTH=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/health 2>/dev/null || echo "000")

    echo ""
    echo -e "${BLUE}Service Status:${NC}"
    echo -e "  Landing (3000): $([ "$LANDING_HEALTH" = "200" ] && echo -e "${GREEN}HEALTHY${NC}" || echo -e "${RED}UNHEALTHY ($LANDING_HEALTH)${NC}")"
    echo -e "  Portal  (5000): $([ "$PORTAL_HEALTH" = "200" ] && echo -e "${GREEN}HEALTHY${NC}" || echo -e "${RED}UNHEALTHY ($PORTAL_HEALTH)${NC}")"

    print_header "DEPLOYMENT COMPLETE"
    echo -e "  ${BLUE}Landing:${NC}  https://www.shahin-ai.com"
    echo -e "  ${BLUE}Portal:${NC}   https://portal.shahin-ai.com"
    echo -e "  ${BLUE}API Docs:${NC} https://portal.shahin-ai.com/api-docs"
    echo ""
}

deploy_landing() {
    print_header "LANDING PAGE DEPLOYMENT"
    check_requirements

    print_step "1/3" "Pulling landing page image..."
    docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" pull landing

    print_step "2/3" "Deploying landing page..."
    docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d landing

    print_step "3/3" "Health check..."
    sleep 5
    curl -sf http://localhost:3000/health && print_success "Landing page is healthy" || print_error "Health check failed"
}

deploy_portal() {
    print_header "PORTAL DEPLOYMENT"
    check_requirements

    print_step "1/4" "Pulling portal image..."
    docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" pull portal

    print_step "2/4" "Ensuring infrastructure..."
    docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d postgres redis
    sleep 5

    print_step "3/4" "Deploying portal..."
    docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d portal

    print_step "4/4" "Health check..."
    sleep 15
    curl -sf http://localhost:5000/health && print_success "Portal is healthy" || print_error "Health check failed"
}

show_status() {
    print_header "SERVICE STATUS"

    echo -e "${BLUE}Docker Containers:${NC}"
    docker ps --filter "name=shahin" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

    echo ""
    echo -e "${BLUE}Health Checks:${NC}"

    services=("landing:3000" "portal:5000" "postgres:5432" "redis:6379")
    for service in "${services[@]}"; do
        name="${service%%:*}"
        port="${service##*:}"

        if [ "$name" = "postgres" ]; then
            status=$(docker exec shahin-postgres pg_isready -U shahin_admin 2>/dev/null && echo "healthy" || echo "unhealthy")
        elif [ "$name" = "redis" ]; then
            status=$(docker exec shahin-redis redis-cli ping 2>/dev/null | grep -q "PONG" && echo "healthy" || echo "unhealthy")
        else
            http_code=$(curl -s -o /dev/null -w "%{http_code}" "http://localhost:$port/health" 2>/dev/null || echo "000")
            status=$([ "$http_code" = "200" ] && echo "healthy" || echo "unhealthy ($http_code)")
        fi

        color=$([ "$status" = "healthy" ] && echo "$GREEN" || echo "$RED")
        echo -e "  $name ($port): ${color}$status${NC}"
    done
}

show_logs() {
    service="${1:-}"
    if [ -n "$service" ]; then
        docker-compose -f "$COMPOSE_FILE" logs -f "$service"
    else
        docker-compose -f "$COMPOSE_FILE" logs -f
    fi
}

stop_services() {
    print_header "STOPPING SERVICES"
    docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" down
    print_success "All services stopped"
}

restart_services() {
    print_header "RESTARTING SERVICES"
    docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" restart
    print_success "All services restarted"
}

backup_database() {
    print_header "DATABASE BACKUP"

    mkdir -p "$BACKUP_DIR"
    BACKUP_FILE="$BACKUP_DIR/shahin_grc_$TIMESTAMP.sql.gz"

    print_step "1/2" "Creating backup..."
    docker exec shahin-postgres pg_dump -U shahin_admin shahin_grc | gzip > "$BACKUP_FILE"

    print_step "2/2" "Cleanup old backups (keeping last 10)..."
    ls -t "$BACKUP_DIR"/*.sql.gz 2>/dev/null | tail -n +11 | xargs -r rm

    print_success "Backup created: $BACKUP_FILE"
    echo "  Size: $(du -h "$BACKUP_FILE" | cut -f1)"
}

# ════════════════════════════════════════════════════════════
# Main
# ════════════════════════════════════════════════════════════

COMMAND="${1:-deploy}"

case "$COMMAND" in
    deploy)
        deploy_full
        ;;
    landing)
        deploy_landing
        ;;
    portal)
        deploy_portal
        ;;
    status)
        show_status
        ;;
    logs)
        show_logs "$2"
        ;;
    stop)
        stop_services
        ;;
    restart)
        restart_services
        ;;
    backup)
        backup_database
        ;;
    *)
        echo "Usage: $0 {deploy|landing|portal|status|logs|stop|restart|backup}"
        echo ""
        echo "Commands:"
        echo "  deploy    - Full stack deployment (default)"
        echo "  landing   - Deploy landing page only"
        echo "  portal    - Deploy portal only"
        echo "  status    - Show service status"
        echo "  logs      - View logs (optional: service name)"
        echo "  stop      - Stop all services"
        echo "  restart   - Restart all services"
        echo "  backup    - Backup database"
        exit 1
        ;;
esac
