#!/bin/bash

# Production Startup Script for GRC System
# This script starts the GRC application in production mode

set -e

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

PROJECT_ROOT="/root/.cursor/worktrees/GRC__Workspace___SSH__doganconsult_/bsk"
PUBLISH_DIR="${PROJECT_ROOT}/publish"

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Starting GRC System (Production)${NC}"
echo -e "${GREEN}========================================${NC}"

# Check if publish directory exists
if [ ! -d "${PUBLISH_DIR}" ]; then
    echo -e "${RED}ERROR: Publish directory not found: ${PUBLISH_DIR}${NC}"
    echo -e "${YELLOW}Please run deploy-production.sh first${NC}"
    exit 1
fi

# Check if DLL exists
if [ ! -f "${PUBLISH_DIR}/GrcMvc.dll" ]; then
    echo -e "${RED}ERROR: GrcMvc.dll not found in ${PUBLISH_DIR}${NC}"
    exit 1
fi

# Set environment
export ASPNETCORE_ENVIRONMENT=Production
export DOTNET_ENVIRONMENT=Production

# Create logs directory if it doesn't exist
mkdir -p "${PUBLISH_DIR}/logs"

echo -e "${YELLOW}Starting application...${NC}"
echo -e "Environment: ${ASPNETCORE_ENVIRONMENT}"
echo -e "Working Directory: ${PUBLISH_DIR}"
echo -e ""

# Start the application
cd "${PUBLISH_DIR}"
dotnet GrcMvc.dll
