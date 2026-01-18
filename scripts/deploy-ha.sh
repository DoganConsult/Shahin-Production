#!/bin/bash
# ══════════════════════════════════════════════════════════════════════════════
# SHAHIN AI GRC - HIGH AVAILABILITY DEPLOYMENT SCRIPT
# ══════════════════════════════════════════════════════════════════════════════
# Usage: ./scripts/deploy-ha.sh [command]
# Commands: start, stop, restart, status, logs, scale, backup, health
# ══════════════════════════════════════════════════════════════════════════════

set -e

# Configuration
COMPOSE_FILE="docker-compose.ha-cloudflare.yml"
PROJECT_NAME="shahin-grc"
DEFAULT_REPLICAS=3

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Functions
print_header() {
    echo -e "${BLUE}"
    echo "══════════════════════════════════════════════════════════════"
    echo "  SHAHIN AI GRC - High Availability Deployment"
    echo "══════════════════════════════════════════════════════════════"
    echo -e "${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

check_requirements() {
    print_info "Checking requirements..."
    
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed"
        exit 1
    fi
    
    if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
        print_error "Docker Compose is not installed"
        exit 1
    fi
    
    if [ ! -f ".env" ]; then
        print_warning ".env file not found. Creating from template..."
        if [ -f ".env.production.template" ]; then
            cp .env.production.template .env
            print_warning "Please edit .env with your production values!"
            exit 1
        else
            print_error ".env.production.template not found"
            exit 1
        fi
    fi
    
    print_success "All requirements met"
}

start_services() {
    print_info "Starting services..."
    
    # Create network if not exists
    docker network create shahin-grc-network 2>/dev/null || true
    
    # Build and start
    docker compose -f $COMPOSE_FILE -p $PROJECT_NAME build
    docker compose -f $COMPOSE_FILE -p $PROJECT_NAME up -d
    
    print_success "Services started"
    show_status
}

stop_services() {
    print_info "Stopping services..."
    docker compose -f $COMPOSE_FILE -p $PROJECT_NAME down
    print_success "Services stopped"
}

restart_services() {
    print_info "Restarting services..."
    docker compose -f $COMPOSE_FILE -p $PROJECT_NAME restart
    print_success "Services restarted"
}

show_status() {
    print_info "Service Status:"
    echo ""
    docker compose -f $COMPOSE_FILE -p $PROJECT_NAME ps
    echo ""
    
    # Health check
    print_info "Health Check:"
    if curl -s -o /dev/null -w "%{http_code}" http://localhost/api/health/live | grep -q "200"; then
        print_success "Application is healthy"
    else
        print_warning "Application may not be ready yet"
    fi
}

show_logs() {
    SERVICE=${2:-""}
    if [ -n "$SERVICE" ]; then
        docker compose -f $COMPOSE_FILE -p $PROJECT_NAME logs -f $SERVICE
    else
        docker compose -f $COMPOSE_FILE -p $PROJECT_NAME logs -f
    fi
}

scale_service() {
    REPLICAS=${2:-$DEFAULT_REPLICAS}
    print_info "Scaling grcmvc to $REPLICAS replicas..."
    docker compose -f $COMPOSE_FILE -p $PROJECT_NAME up -d --scale grcmvc=$REPLICAS
    print_success "Scaled to $REPLICAS replicas"
}

run_backup() {
    print_info "Running database backup..."
    BACKUP_FILE="backups/shahin_grc_$(date +%Y%m%d_%H%M%S).dump"
    
    mkdir -p backups
    
    docker compose -f $COMPOSE_FILE -p $PROJECT_NAME exec -T pg-primary \
        pg_dump -Fc -U ${DB_USER:-shahin_admin} ${DB_NAME:-shahin_grc} > $BACKUP_FILE
    
    print_success "Backup saved to $BACKUP_FILE"
}

health_check() {
    print_info "Running comprehensive health check..."
    echo ""
    
    # Application health
    echo "📱 Application:"
    if curl -s http://localhost/api/health | jq . 2>/dev/null; then
        print_success "Application responding"
    else
        print_error "Application not responding"
    fi
    echo ""
    
    # Database health
    echo "🗄️  Database:"
    if docker compose -f $COMPOSE_FILE -p $PROJECT_NAME exec -T pg-primary pg_isready -U ${DB_USER:-shahin_admin} 2>/dev/null; then
        print_success "PostgreSQL Primary healthy"
    else
        print_error "PostgreSQL Primary unhealthy"
    fi
    echo ""
    
    # Redis health
    echo "📦 Redis:"
    if docker compose -f $COMPOSE_FILE -p $PROJECT_NAME exec -T redis-master redis-cli ping 2>/dev/null | grep -q "PONG"; then
        print_success "Redis healthy"
    else
        print_error "Redis unhealthy"
    fi
    echo ""
    
    # Cloudflare tunnel
    echo "🌐 Cloudflare Tunnel:"
    if docker compose -f $COMPOSE_FILE -p $PROJECT_NAME ps cloudflared | grep -q "Up"; then
        print_success "Cloudflare Tunnel running"
    else
        print_warning "Cloudflare Tunnel not running"
    fi
    echo ""
    
    # Integration health
    echo "🔗 Integration Health:"
    curl -s http://localhost/api/health/integrations 2>/dev/null | jq '.integrations[] | {name, status, responseTimeMs}' || print_warning "Cannot fetch integration health"
}

show_ports() {
    print_info "Port Allocation:"
    echo ""
    echo "┌─────────────────────────────────────────────────────────┐"
    echo "│ Service              │ Port    │ Protocol │ Status     │"
    echo "├─────────────────────────────────────────────────────────┤"
    echo "│ GRC Portal           │ 80/443  │ HTTP/S   │ Traefik    │"
    echo "│ Landing Page         │ 3000    │ HTTP     │ Internal   │"
    echo "│ PostgreSQL Primary   │ 5432    │ TCP      │ Internal   │"
    echo "│ PgBouncer            │ 6432    │ TCP      │ Exposed    │"
    echo "│ Redis Master         │ 6379    │ TCP      │ Internal   │"
    echo "│ Traefik Dashboard    │ 8080    │ HTTP     │ Exposed    │"
    echo "│ Prometheus           │ 9090    │ HTTP     │ Exposed    │"
    echo "│ Grafana              │ 3030    │ HTTP     │ Exposed    │"
    echo "└─────────────────────────────────────────────────────────┘"
}

show_help() {
    echo "Usage: $0 [command] [options]"
    echo ""
    echo "Commands:"
    echo "  start       Start all services"
    echo "  stop        Stop all services"
    echo "  restart     Restart all services"
    echo "  status      Show service status"
    echo "  logs [svc]  Show logs (optionally for specific service)"
    echo "  scale [n]   Scale grcmvc to n replicas (default: 3)"
    echo "  backup      Run database backup"
    echo "  health      Run comprehensive health check"
    echo "  ports       Show port allocation"
    echo "  help        Show this help"
    echo ""
    echo "Examples:"
    echo "  $0 start"
    echo "  $0 scale 5"
    echo "  $0 logs grcmvc"
    echo "  $0 health"
}

# Main
print_header

case "${1:-}" in
    start)
        check_requirements
        start_services
        ;;
    stop)
        stop_services
        ;;
    restart)
        restart_services
        ;;
    status)
        show_status
        ;;
    logs)
        show_logs "$@"
        ;;
    scale)
        scale_service "$@"
        ;;
    backup)
        run_backup
        ;;
    health)
        health_check
        ;;
    ports)
        show_ports
        ;;
    help|--help|-h)
        show_help
        ;;
    *)
        show_help
        exit 1
        ;;
esac
