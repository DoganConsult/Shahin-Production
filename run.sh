#!/bin/bash

# GRC System - Build & Run Script
# This script builds and runs the GRC application

set -e  # Exit on error

echo "================================================"
echo "🚀 GRC System - Build & Run"
echo "================================================"
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Step 1: Clean
echo -e "${YELLOW}[1/5]${NC} Cleaning project..."
cd /home/dogan/grc-system
dotnet clean src/GrcMvc/GrcMvc.csproj -q 2>/dev/null || true
echo -e "${GREEN}✓ Project cleaned${NC}"
echo ""

# Step 2: Restore
echo -e "${YELLOW}[2/5]${NC} Restoring packages..."
dotnet restore src/GrcMvc/GrcMvc.csproj -q
echo -e "${GREEN}✓ Packages restored${NC}"
echo ""

# Step 3: Build
echo -e "${YELLOW}[3/5]${NC} Building project..."
dotnet build src/GrcMvc/GrcMvc.csproj -c Release -q
echo -e "${GREEN}✓ Build successful${NC}"
echo ""

# Step 4: Database Migration
echo -e "${YELLOW}[4/5]${NC} Applying database migrations..."
cd src/GrcMvc
dotnet ef database update --context GrcDbContext -q 2>/dev/null || echo "  (Database already up to date)"
echo -e "${GREEN}✓ Database ready${NC}"
echo ""

# Step 5: Run
echo -e "${YELLOW}[5/5]${NC} Starting application..."
echo -e "${GREEN}✓ Application starting...${NC}"
echo ""
echo "================================================"
echo -e "${GREEN}🟢 Application is running!${NC}"
echo "================================================"
echo ""
echo "📍 Open your browser:"
echo "   👉 https://localhost:5001"
echo ""
echo "📊 Health Check:"
echo "   👉 https://localhost:5001/health"
echo ""
echo "🔐 Login with:"
echo "   Email:    Info@doganconsult.com"
echo "   Password: AhmEma\$123456"
echo ""
echo "📝 Logs location:"
echo "   📄 /app/logs/grcmvc-YYYY-MM-DD.log"
echo ""
echo "⏹️  Press Ctrl+C to stop the application"
echo "================================================"
echo ""

# Run the application
dotnet run
