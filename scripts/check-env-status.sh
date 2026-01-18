#!/bin/bash
#
# Shahin AI - Environment Status Checker
# Checks status of all environments: Development, Staging, Production
#
# Usage: ./check-env-status.sh [environment]
#   - No argument: Check all environments
#   - dev: Check development only
#   - staging: Check staging only
#   - production: Check production only

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Print functions
print_header() { echo -e "\n${BLUE}════════════════════════════════════════════════════════${NC}"; echo -e "${BLUE}$1${NC}"; echo -e "${BLUE}════════════════════════════════════════════════════════${NC}\n"; }
print_success() { echo -e "${GREEN}✓${NC} $1"; }
print_error() { echo -e "${RED}✗${NC} $1"; }
print_warning() { echo -e "${YELLOW}⚠${NC} $1"; }
print_info() { echo -e "${CYAN}ℹ${NC} $1"; }

# Check Docker
check_docker() {
    if command -v docker &> /dev/null; then
        print_success "Docker installed: $(docker --version)"
        if docker ps &> /dev/null; then
            print_success "Docker daemon is running"
            return 0
        else
            print_error "Docker daemon is not running"
            return 1
        fi
    else
        print_error "Docker is not installed"
        return 1
    fi
}

# Check Docker Compose
check_docker_compose() {
    if command -v docker-compose &> /dev/null || docker compose version &> /dev/null; then
        print_success "Docker Compose available"
        return 0
    else
        print_error "Docker Compose is not installed"
        return 1
    fi
}

# Check environment containers
check_environment() {
    local env=$1
    local compose_file=$2
    local container_prefix=$3
    local port=$4
    
    print_header "Checking $env Environment"
    
    # Check if compose file exists
    if [ ! -f "$compose_file" ]; then
        print_warning "Compose file not found: $compose_file"
        return 1
    fi
    
    # Check containers
    print_info "Checking containers..."
    local containers=$(docker ps -a --filter "name=$container_prefix" --format "{{.Names}}" 2>/dev/null || echo "")
    
    if [ -z "$containers" ]; then
        print_warning "No containers found for $env"
        return 1
    fi
    
    local running=0
    local stopped=0
    
    for container in $containers; do
        if docker ps --filter "name=$container" --format "{{.Names}}" | grep -q "$container"; then
            print_success "Container running: $container"
            running=$((running + 1))
        else
            print_error "Container stopped: $container"
            stopped=$((stopped + 1))
        fi
    done
    
    echo ""
    print_info "Summary: $running running, $stopped stopped"
    
    # Check port
    if [ -n "$port" ]; then
        if netstat -tuln 2>/dev/null | grep -q ":$port " || ss -tuln 2>/dev/null | grep -q ":$port "; then
            print_success "Port $port is listening"
        else
            print_warning "Port $port is not listening"
        fi
    fi
    
    # Check health endpoint
    if [ -n "$port" ]; then
        print_info "Checking health endpoint..."
        local health_response=$(curl -s -o /dev/null -w "%{http_code}" "http://localhost:$port/health" 2>/dev/null || echo "000")
        if [ "$health_response" = "200" ]; then
            print_success "Health endpoint responding (HTTP $health_response)"
        else
            print_warning "Health endpoint not responding (HTTP $health_response)"
        fi
    fi
    
    return 0
}

# Check database connectivity
check_database() {
    local env=$1
    local db_container=$2
    
    print_info "Checking database connectivity..."
    
    if docker ps --filter "name=$db_container" --format "{{.Names}}" | grep -q "$db_container"; then
        print_success "Database container is running: $db_container"
        
        # Try to connect
        if docker exec "$db_container" pg_isready -U postgres &> /dev/null 2>&1 || \
           docker exec "$db_container" pg_isready &> /dev/null 2>&1; then
            print_success "Database is accepting connections"
            return 0
        else
            print_warning "Database container running but not accepting connections"
            return 1
        fi
    else
        print_error "Database container is not running: $db_container"
        return 1
    fi
}

# Check Redis
check_redis() {
    local env=$1
    local redis_container=$2
    
    print_info "Checking Redis..."
    
    if docker ps --filter "name=$redis_container" --format "{{.Names}}" | grep -q "$redis_container"; then
        print_success "Redis container is running: $redis_container"
        
        if docker exec "$redis_container" redis-cli ping &> /dev/null 2>&1; then
            print_success "Redis is responding"
            return 0
        else
            print_warning "Redis container running but not responding"
            return 1
        fi
    else
        print_warning "Redis container is not running: $redis_container"
        return 1
    fi
}

# Check environment variables
check_env_file() {
    local env_file=$1
    
    if [ -f "$env_file" ]; then
        print_success "Environment file exists: $env_file"
        
        # Check for required variables (without exposing values)
        local required_vars=("DB_PASSWORD" "JWT_SECRET")
        local missing_vars=()
        
        for var in "${required_vars[@]}"; do
            if ! grep -q "^${var}=" "$env_file" 2>/dev/null; then
                missing_vars+=("$var")
            fi
        done
        
        if [ ${#missing_vars[@]} -eq 0 ]; then
            print_success "Required environment variables are set"
        else
            print_warning "Missing environment variables: ${missing_vars[*]}"
        fi
        return 0
    else
        print_error "Environment file not found: $env_file"
        return 1
    fi
}

# Check disk space
check_disk_space() {
    print_info "Checking disk space..."
    local usage=$(df -h / | awk 'NR==2 {print $5}' | sed 's/%//')
    
    if [ "$usage" -lt 80 ]; then
        print_success "Disk space: ${usage}% used"
    elif [ "$usage" -lt 90 ]; then
        print_warning "Disk space: ${usage}% used (getting full)"
    else
        print_error "Disk space: ${usage}% used (CRITICAL)"
    fi
}

# Check memory
check_memory() {
    print_info "Checking memory..."
    if command -v free &> /dev/null; then
        local mem_total=$(free -m | awk 'NR==2{print $2}')
        local mem_used=$(free -m | awk 'NR==2{print $3}')
        local mem_percent=$((mem_used * 100 / mem_total))
        
        if [ "$mem_percent" -lt 80 ]; then
            print_success "Memory: ${mem_percent}% used (${mem_used}MB / ${mem_total}MB)"
        elif [ "$mem_percent" -lt 90 ]; then
            print_warning "Memory: ${mem_percent}% used (${mem_used}MB / ${mem_total}MB)"
        else
            print_error "Memory: ${mem_percent}% used (${mem_used}MB / ${mem_total}MB) - CRITICAL"
        fi
    fi
}

# Main execution
main() {
    local target_env=${1:-all}
    
    print_header "Shahin AI - Environment Status Check"
    echo "Checking: $target_env"
    echo "Date: $(date)"
    echo ""
    
    # Prerequisites
    print_header "Prerequisites"
    check_docker
    check_docker_compose
    check_disk_space
    check_memory
    echo ""
    
    # Check each environment
    if [ "$target_env" = "all" ] || [ "$target_env" = "dev" ] || [ "$target_env" = "development" ]; then
        check_environment "Development" "docker-compose.yml" "grcmvc" "5137"
        check_database "Development" "grcmvc-db"
        check_redis "Development" "grcmvc-redis"
        check_env_file ".env"
        echo ""
    fi
    
    if [ "$target_env" = "all" ] || [ "$target_env" = "staging" ]; then
        check_environment "Staging" "docker-compose.staging.yml" "shahin-grc-staging" "8080"
        check_database "Staging" "shahin-grc-db-staging"
        check_redis "Staging" "shahin-grc-redis-staging"
        check_env_file ".env.staging"
        echo ""
    fi
    
    if [ "$target_env" = "all" ] || [ "$target_env" = "production" ] || [ "$target_env" = "prod" ]; then
        check_environment "Production" "docker-compose.production.yml" "shahin-grc-production" "5000"
        check_database "Production" "shahin-grc-db-production"
        check_redis "Production" "shahin-grc-redis-production"
        check_env_file ".env.production"
        echo ""
    fi
    
    # Summary
    print_header "Summary"
    print_info "Total containers: $(docker ps -a --format '{{.Names}}' | wc -l)"
    print_info "Running containers: $(docker ps --format '{{.Names}}' | wc -l)"
    print_info "Stopped containers: $(docker ps -a --filter 'status=exited' --format '{{.Names}}' | wc -l)"
    
    echo ""
    print_info "To view detailed logs: docker-compose -f <compose-file> logs -f"
    print_info "To restart environment: docker-compose -f <compose-file> restart"
    print_info "To check specific service: docker logs <container-name>"
}

# Run main
main "$@"
