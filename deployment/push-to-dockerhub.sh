#!/bin/bash
# ══════════════════════════════════════════════════════════════
# Build and Push Images to Docker Hub
# ══════════════════════════════════════════════════════════════

set -e

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m'

# Configuration
DOCKERHUB_USER="${DOCKERHUB_USER:-drdogan}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo -e "${BLUE}══════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}  Building and Pushing to Docker Hub${NC}"
echo -e "${BLUE}  User: $DOCKERHUB_USER${NC}"
echo -e "${BLUE}══════════════════════════════════════════════════════════════${NC}"
echo ""

# Check Docker login
if ! docker info | grep -q "Username"; then
    echo -e "${YELLOW}Please login to Docker Hub:${NC}"
    docker login
fi

# ════════════════════════════════════════════════════════════
# 1. Build and Push Landing Page (Frontend)
# ════════════════════════════════════════════════════════════
echo -e "${YELLOW}[1/2] Building Landing Page...${NC}"
cd "$PROJECT_ROOT/grc-frontend"
docker build -t "$DOCKERHUB_USER/shahin-landing:latest" -f Dockerfile .
docker tag "$DOCKERHUB_USER/shahin-landing:latest" "$DOCKERHUB_USER/shahin-landing:$(date +%Y%m%d)"
echo -e "${YELLOW}Pushing Landing Page to Docker Hub...${NC}"
docker push "$DOCKERHUB_USER/shahin-landing:latest"
docker push "$DOCKERHUB_USER/shahin-landing:$(date +%Y%m%d)"
echo -e "${GREEN}✅ Landing Page pushed${NC}"
echo ""

# ════════════════════════════════════════════════════════════
# 2. Build and Push Portal (Backend)
# ════════════════════════════════════════════════════════════
echo -e "${YELLOW}[2/2] Building Portal (Backend)...${NC}"
cd "$PROJECT_ROOT/src/GrcMvc"
docker build -t "$DOCKERHUB_USER/shahin-portal:latest" -f Dockerfile.production .
docker tag "$DOCKERHUB_USER/shahin-portal:latest" "$DOCKERHUB_USER/shahin-portal:$(date +%Y%m%d)"
echo -e "${YELLOW}Pushing Portal to Docker Hub...${NC}"
docker push "$DOCKERHUB_USER/shahin-portal:latest"
docker push "$DOCKERHUB_USER/shahin-portal:$(date +%Y%m%d)"
echo -e "${GREEN}✅ Portal pushed${NC}"
echo ""

echo -e "${GREEN}══════════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}  All images pushed to Docker Hub${NC}"
echo -e "${GREEN}══════════════════════════════════════════════════════════════${NC}"
echo ""
echo -e "Images:"
echo -e "  ${BLUE}$DOCKERHUB_USER/shahin-landing:latest${NC}"
echo -e "  ${BLUE}$DOCKERHUB_USER/shahin-portal:latest${NC}"
echo ""
