#!/bin/bash
# Shahin AI GRC - Environment Management Script

COMPOSE_FILE="docker-compose.all.yml"

case "$1" in
  # Start specific environment
  dev)
    echo "Starting Development..."
    docker-compose -f $COMPOSE_FILE up -d grcmvc-dev db-dev redis-dev
    echo "Dev running at http://localhost:8888"
    ;;
  staging)
    echo "Starting Staging..."
    docker-compose -f $COMPOSE_FILE up -d grcmvc-staging db-staging redis-staging
    echo "Staging running at http://localhost:8080"
    ;;
  prod)
    echo "Starting Production..."
    docker-compose -f $COMPOSE_FILE up -d grcmvc-prod db-prod redis-prod
    echo "Production running at http://localhost:80"
    ;;
  all)
    echo "Starting ALL environments..."
    docker-compose -f $COMPOSE_FILE up -d
    echo "Dev: http://localhost:8888"
    echo "Staging: http://localhost:8080"
    echo "Prod: http://localhost:80"
    ;;
  
  # Stop specific environment
  stop-dev)
    docker-compose -f $COMPOSE_FILE stop grcmvc-dev db-dev redis-dev
    ;;
  stop-staging)
    docker-compose -f $COMPOSE_FILE stop grcmvc-staging db-staging redis-staging
    ;;
  stop-prod)
    docker-compose -f $COMPOSE_FILE stop grcmvc-prod db-prod redis-prod
    ;;
  stop-all)
    docker-compose -f $COMPOSE_FILE down
    ;;

  # Build
  build)
    docker-compose -f $COMPOSE_FILE build --no-cache
    ;;
  
  # Status
  status)
    echo "=== Container Status ==="
    docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep shahin
    ;;
  
  # Logs
  logs-dev)
    docker logs -f shahin-dev-app
    ;;
  logs-staging)
    docker logs -f shahin-staging-app
    ;;
  logs-prod)
    docker logs -f shahin-prod-app
    ;;

  *)
    echo "Shahin AI GRC - Environment Manager"
    echo ""
    echo "Usage: ./manage.sh [command]"
    echo ""
    echo "Commands:"
    echo "  dev          Start development environment (port 8888)"
    echo "  staging      Start staging environment (port 8080)"
    echo "  prod         Start production environment (port 80)"
    echo "  all          Start all environments"
    echo ""
    echo "  stop-dev     Stop development"
    echo "  stop-staging Stop staging"
    echo "  stop-prod    Stop production"
    echo "  stop-all     Stop all environments"
    echo ""
    echo "  build        Build all containers"
    echo "  status       Show container status"
    echo ""
    echo "  logs-dev     View dev logs"
    echo "  logs-staging View staging logs"
    echo "  logs-prod    View prod logs"
    ;;
esac
