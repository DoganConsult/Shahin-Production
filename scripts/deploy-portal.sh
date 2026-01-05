#!/bin/bash
###############################################################################
# GrcMvc Portal Deployment Script
# Domain: portal.shahin-ai.com
# Purpose: Deploy/Update GrcMvc application to production
###############################################################################

set -e  # Exit on error

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}GrcMvc Portal Deployment${NC}"
echo -e "${GREEN}Domain: portal.shahin-ai.com${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Check if running from correct directory
if [ ! -f "docker-compose.grcmvc.yml" ]; then
    echo -e "${RED}Error: Must run from /home/dogan/grc-system directory${NC}"
    exit 1
fi

# Check if .env file exists
if [ ! -f ".env.grcmvc.production" ]; then
    echo -e "${RED}Error: .env.grcmvc.production file not found${NC}"
    exit 1
fi

# Pull latest code (if using git)
if [ -d ".git" ]; then
    echo -e "${YELLOW}Pulling latest code from repository...${NC}"
    git pull origin main || echo "Warning: Git pull failed or no changes"
fi

# Build the application
echo -e "${YELLOW}Building Docker image...${NC}"
docker compose -f docker-compose.grcmvc.yml build grcmvc

# Stop old containers
echo -e "${YELLOW}Stopping old containers...${NC}"
docker compose -f docker-compose.grcmvc.yml down

# Start new containers
echo -e "${YELLOW}Starting new containers...${NC}"
docker compose -f docker-compose.grcmvc.yml up -d

# Wait for application to be ready
echo -e "${YELLOW}Waiting for application to start...${NC}"
sleep 5

# Check container status
echo -e "${YELLOW}Checking container status...${NC}"
docker compose -f docker-compose.grcmvc.yml ps

# Test application
echo ""
echo -e "${YELLOW}Testing application...${NC}"
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -H "Host: portal.shahin-ai.com" http://localhost/)

if [ "$HTTP_CODE" = "200" ]; then
    echo -e "${GREEN}✓ Application is responding correctly (HTTP $HTTP_CODE)${NC}"
else
    echo -e "${YELLOW}⚠ Application returned HTTP $HTTP_CODE${NC}"
fi

# Test external access
echo -e "${YELLOW}Testing external access...${NC}"
EXT_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://portal.shahin-ai.com/ || echo "000")
echo -e "${GREEN}✓ External access: HTTP $EXT_CODE${NC}"

# Show logs
echo ""
echo -e "${YELLOW}Recent application logs:${NC}"
docker logs grcmvc-app --tail 20

echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Deployment Complete!${NC}"
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Application URL: http://portal.shahin-ai.com${NC}"
echo -e "${GREEN}Local URL: http://localhost:5137${NC}"
echo ""
echo -e "${YELLOW}Useful commands:${NC}"
echo "  View logs:    docker logs grcmvc-app -f"
echo "  Stop app:     docker compose -f docker-compose.grcmvc.yml down"
echo "  Restart app:  docker compose -f docker-compose.grcmvc.yml restart grcmvc"
echo "  Status:       docker compose -f docker-compose.grcmvc.yml ps"
echo ""
