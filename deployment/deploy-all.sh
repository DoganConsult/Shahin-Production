#!/bin/bash
# ══════════════════════════════════════════════════════════════
# Complete Deployment Pipeline
# 1. Push to GitHub
# 2. Build and Push to Docker Hub
# 3. Deploy to Production Server (8 containers: 5 core + 3 LLM)
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

# ════════════════════════════════════════════════════════════
# Main Deployment Pipeline
# ════════════════════════════════════════════════════════════

print_header "COMPLETE DEPLOYMENT PIPELINE"

# Step 1: Push to GitHub
print_step "1/3" "Pushing to GitHub..."
if "$SCRIPT_DIR/push-to-github.sh"; then
    print_success "GitHub push complete"
else
    print_error "GitHub push failed"
    exit 1
fi
echo ""

# Step 2: Push to Docker Hub
print_step "2/3" "Building and pushing to Docker Hub..."
if "$SCRIPT_DIR/push-to-dockerhub.sh"; then
    print_success "Docker Hub push complete"
else
    print_error "Docker Hub push failed"
    exit 1
fi
echo ""

# Step 3: Deploy to Production Server
print_step "3/3" "Deploying to production server..."
if "$SCRIPT_DIR/deploy-to-production.sh" rebuild; then
    print_success "Production deployment complete"
else
    print_error "Production deployment failed"
    exit 1
fi
echo ""

# Summary
print_header "DEPLOYMENT COMPLETE"
echo -e "${GREEN}All 8 containers deployed:${NC}"
echo -e "  ${BLUE}Core Containers (5):${NC}"
echo -e "    1. landing    - Frontend (Next.js)"
echo -e "    2. portal     - Backend (ASP.NET Core)"
echo -e "    3. postgres   - Database (PostgreSQL)"
echo -e "    4. redis      - Cache (Redis)"
echo -e "    5. nginx      - Reverse Proxy"
echo ""
echo -e "  ${BLUE}LLM Containers (3):${NC}"
echo -e "    6. ollama     - LLM Server"
echo -e "    7. llm-model-1 - Llama 3:8b"
echo -e "    8. llm-model-2 - Mistral:7b"
echo -e "    9. llm-model-3 - Phi-3:mini"
echo ""
echo -e "${GREEN}Access URLs:${NC}"
echo -e "  Landing: http://212.147.229.36:3000"
echo -e "  Portal:  http://212.147.229.36:5000"
echo -e "  Ollama:  http://212.147.229.36:11434"
echo ""
