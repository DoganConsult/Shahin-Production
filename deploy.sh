#!/bin/bash
# GRC System Deployment Script
# Usage: ./deploy.sh [local|staging|production]

set -e

ENVIRONMENT=${1:-local}
COMPOSE_FILE="docker-compose.grcmvc.yml"

echo "============================================"
echo "  GRC System Deployment - $ENVIRONMENT"
echo "============================================"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Step 1: Pre-flight checks
echo -e "\n${YELLOW}[1/6] Running pre-flight checks...${NC}"

# Check Docker
if ! command -v docker &> /dev/null; then
    echo -e "${RED}ERROR: Docker is not installed${NC}"
    exit 1
fi
echo "  ✓ Docker installed"

# Check Docker Compose
if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
    echo -e "${RED}ERROR: Docker Compose is not installed${NC}"
    exit 1
fi
echo "  ✓ Docker Compose installed"

# Step 2: Build check
echo -e "\n${YELLOW}[2/6] Verifying .NET build...${NC}"
cd src/GrcMvc
dotnet build GrcMvc.csproj -c Release --no-restore -v q
if [ $? -eq 0 ]; then
    echo "  ✓ Build successful"
else
    echo -e "${RED}ERROR: Build failed${NC}"
    exit 1
fi
cd ../..

# Step 3: Stop existing containers
echo -e "\n${YELLOW}[3/6] Stopping existing containers...${NC}"
docker compose -f $COMPOSE_FILE down 2>/dev/null || true
echo "  ✓ Containers stopped"

# Step 4: Build Docker images
echo -e "\n${YELLOW}[4/6] Building Docker images...${NC}"
docker compose -f $COMPOSE_FILE build --no-cache
echo "  ✓ Docker images built"

# Step 5: Start containers
echo -e "\n${YELLOW}[5/6] Starting containers...${NC}"
docker compose -f $COMPOSE_FILE up -d
echo "  ✓ Containers started"

# Step 6: Health check
echo -e "\n${YELLOW}[6/6] Waiting for health check...${NC}"
sleep 10

# Check if containers are running
if docker compose -f $COMPOSE_FILE ps | grep -q "running"; then
    echo "  ✓ Containers are running"
else
    echo -e "${RED}WARNING: Some containers may not be running${NC}"
    docker compose -f $COMPOSE_FILE ps
fi

# Final status
echo -e "\n${GREEN}============================================${NC}"
echo -e "${GREEN}  Deployment Complete!${NC}"
echo -e "${GREEN}============================================${NC}"
echo ""
echo "  Application URL: http://localhost:5137"
echo "  Health Check:    http://localhost:5137/health"
echo ""
echo "  View logs: docker compose -f $COMPOSE_FILE logs -f"
echo "  Stop:      docker compose -f $COMPOSE_FILE down"
echo ""

# Show container status
echo "Container Status:"
docker compose -f $COMPOSE_FILE ps
