#!/bin/bash

# Production Deployment Script for GRC System
# This script builds and deploys the GRC application for production

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
PROJECT_ROOT="/root/.cursor/worktrees/GRC__Workspace___SSH__doganconsult_/bsk"
APP_DIR="${PROJECT_ROOT}/src/GrcMvc"
BUILD_DIR="${PROJECT_ROOT}/publish"
ENVIRONMENT="Production"

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}GRC System Production Deployment${NC}"
echo -e "${GREEN}========================================${NC}"

# Step 1: Check prerequisites
echo -e "\n${YELLOW}Step 1: Checking prerequisites...${NC}"

if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}ERROR: .NET SDK not found. Please install .NET 8.0 SDK.${NC}"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo -e "${GREEN}✓ .NET SDK found: ${DOTNET_VERSION}${NC}"

if ! command -v docker &> /dev/null; then
    echo -e "${YELLOW}WARNING: Docker not found. Docker deployment will be skipped.${NC}"
    DOCKER_AVAILABLE=false
else
    DOCKER_AVAILABLE=true
    echo -e "${GREEN}✓ Docker found${NC}"
fi

# Step 2: Clean previous builds
echo -e "\n${YELLOW}Step 2: Cleaning previous builds...${NC}"
cd "${PROJECT_ROOT}"
dotnet clean src/GrcMvc/GrcMvc.csproj -c Release --verbosity quiet
rm -rf "${BUILD_DIR}"
echo -e "${GREEN}✓ Clean completed${NC}"

# Step 3: Restore dependencies
echo -e "\n${YELLOW}Step 3: Restoring dependencies...${NC}"
cd "${APP_DIR}"
dotnet restore GrcMvc.csproj --verbosity quiet
echo -e "${GREEN}✓ Dependencies restored${NC}"

# Step 4: Build for production
echo -e "\n${YELLOW}Step 4: Building for production (Release configuration)...${NC}"
dotnet build GrcMvc.csproj -c Release --no-restore
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Build successful${NC}"
else
    echo -e "${RED}✗ Build failed${NC}"
    exit 1
fi

# Step 5: Run tests (optional but recommended)
echo -e "\n${YELLOW}Step 5: Running tests...${NC}"
if [ -d "${PROJECT_ROOT}/tests" ]; then
    cd "${PROJECT_ROOT}/tests"
    if dotnet test --configuration Release --no-build --verbosity quiet; then
        echo -e "${GREEN}✓ Tests passed${NC}"
    else
        echo -e "${YELLOW}⚠ Tests failed or skipped${NC}"
    fi
else
    echo -e "${YELLOW}⚠ No tests directory found, skipping tests${NC}"
fi

# Step 6: Publish for production
echo -e "\n${YELLOW}Step 6: Publishing application...${NC}"
cd "${APP_DIR}"
mkdir -p "${BUILD_DIR}"
dotnet publish GrcMvc.csproj \
    -c Release \
    -o "${BUILD_DIR}" \
    --no-build \
    -p:PublishProfile=FolderProfile \
    -p:EnvironmentName=${ENVIRONMENT}

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Publish successful${NC}"
    echo -e "${GREEN}  Published to: ${BUILD_DIR}${NC}"
else
    echo -e "${RED}✗ Publish failed${NC}"
    exit 1
fi

# Step 7: Verify published files
echo -e "\n${YELLOW}Step 7: Verifying published files...${NC}"
if [ -f "${BUILD_DIR}/GrcMvc.dll" ]; then
    echo -e "${GREEN}✓ GrcMvc.dll found${NC}"
else
    echo -e "${RED}✗ GrcMvc.dll not found in publish directory${NC}"
    exit 1
fi

# Step 8: Create production configuration check
echo -e "\n${YELLOW}Step 8: Checking production configuration...${NC}"
if [ -f "${APP_DIR}/appsettings.Production.json" ]; then
    echo -e "${GREEN}✓ Production configuration file exists${NC}"
else
    echo -e "${YELLOW}⚠ Production configuration file not found${NC}"
fi

# Step 9: Docker build (if available)
if [ "$DOCKER_AVAILABLE" = true ]; then
    echo -e "\n${YELLOW}Step 9: Building Docker image...${NC}"
    cd "${PROJECT_ROOT}"
    docker build -t grcmvc:production -f src/GrcMvc/Dockerfile .
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Docker image built successfully${NC}"
        echo -e "${GREEN}  Image: grcmvc:production${NC}"
    else
        echo -e "${YELLOW}⚠ Docker build failed${NC}"
    fi
fi

# Step 10: Summary
echo -e "\n${GREEN}========================================${NC}"
echo -e "${GREEN}Deployment Summary${NC}"
echo -e "${GREEN}========================================${NC}"
echo -e "Environment: ${ENVIRONMENT}"
echo -e "Build Directory: ${BUILD_DIR}"
echo -e "Application DLL: ${BUILD_DIR}/GrcMvc.dll"
if [ "$DOCKER_AVAILABLE" = true ]; then
    echo -e "Docker Image: grcmvc:production"
fi
echo -e "\n${GREEN}✓ Production build completed successfully!${NC}"
echo -e "\n${YELLOW}Next steps:${NC}"
echo -e "1. Review appsettings.Production.json configuration"
echo -e "2. Set up database connection string"
echo -e "3. Configure JWT secret key"
echo -e "4. Run database migrations: dotnet ef database update"
echo -e "5. Start the application: dotnet ${BUILD_DIR}/GrcMvc.dll"
if [ "$DOCKER_AVAILABLE" = true ]; then
    echo -e "6. Or use Docker: docker-compose up -d"
fi
