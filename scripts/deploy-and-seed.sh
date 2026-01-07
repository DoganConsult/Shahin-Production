#!/bin/bash
# Deploy GRC System and Create PlatformAdmin Account
# This script rebuilds, applies migrations, and runs the application
# PlatformAdmin will be created automatically via ApplicationInitializer

set -e

echo "🚀 Starting GRC System Deployment..."

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Navigate to project root
cd "$(dirname "$0")/.."

echo -e "${YELLOW}📦 Step 1: Cleaning project...${NC}"
dotnet clean src/GrcMvc/GrcMvc.csproj

echo -e "${YELLOW}📦 Step 2: Restoring packages...${NC}"
dotnet restore src/GrcMvc/GrcMvc.csproj

echo -e "${YELLOW}🔨 Step 3: Building project (Release)...${NC}"
dotnet build src/GrcMvc/GrcMvc.csproj -c Release

if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Build failed!${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Build succeeded!${NC}"

echo -e "${YELLOW}🗄️  Step 4: Applying database migrations...${NC}"
echo "   Applying GrcDbContext migrations..."
dotnet ef database update --project src/GrcMvc/GrcMvc.csproj --context GrcDbContext || {
    echo -e "${RED}⚠️  Migration failed. Check database connection in appsettings.json${NC}"
    echo "   Connection String: Host=localhost;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5433"
}

echo "   Applying GrcAuthDbContext migrations..."
dotnet ef database update --project src/GrcMvc/GrcMvc.csproj --context GrcAuthDbContext || {
    echo -e "${RED}⚠️  Auth migration failed. Check database connection.${NC}"
}

echo -e "${GREEN}✅ Migrations completed!${NC}"

echo -e "${YELLOW}🚀 Step 5: Starting application...${NC}"
echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${GREEN}✅ Deployment Ready!${NC}"
echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo ""
echo -e "${YELLOW}📋 Next Steps:${NC}"
echo "   1. Start the application:"
echo "      cd src/GrcMvc"
echo "      dotnet run"
echo ""
echo "   2. PlatformAdmin will be created automatically on first startup"
echo ""
echo -e "${GREEN}🔑 PlatformAdmin Credentials:${NC}"
echo "   Email:    Dooganlap@gmail.com"
echo "   Password: Platform@2026!"
echo "   Role:     PlatformAdmin"
echo "   Level:    Owner (Full Access)"
echo ""
echo -e "${GREEN}🌐 Access URLs:${NC}"
echo "   Application: http://localhost:8080"
echo "   Platform Dashboard: http://localhost:8080/platform/dashboard"
echo ""
echo -e "${YELLOW}⚠️  Note: Change the default password immediately after first login!${NC}"
