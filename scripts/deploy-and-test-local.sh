#!/bin/bash
# Local Deployment and Testing Script for GRC System
# This script builds, tests, seeds, and runs the application locally

set -e  # Exit on error

echo "🚀 Starting Local Deployment and Testing..."

# Set PATH for .NET
export PATH="$PATH:/usr/share/dotnet:$HOME/.dotnet/tools"

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Step 1: Verify .NET is available
echo -e "${YELLOW}Step 1: Verifying .NET SDK...${NC}"
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ .NET SDK not found. Please install .NET 8.0 SDK${NC}"
    exit 1
fi
DOTNET_VERSION=$(dotnet --version)
echo -e "${GREEN}✅ .NET SDK found: $DOTNET_VERSION${NC}"

# Step 2: Verify Database is Running
echo -e "${YELLOW}Step 2: Verifying database is running...${NC}"
if ! docker ps | grep -q grc-db; then
    echo -e "${YELLOW}⚠️  Database container not running. Starting it...${NC}"
    cd /home/dogan/grc-system
    docker-compose up -d db
    echo "Waiting for database to be ready..."
    sleep 5
fi
echo -e "${GREEN}✅ Database is running${NC}"

# Step 3: Set Connection String
export ConnectionStrings__DefaultConnection="Host=localhost;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5432"
export ASPNETCORE_ENVIRONMENT=Development

# Step 4: Clean and Build
echo -e "${YELLOW}Step 3: Cleaning and building project...${NC}"
cd /home/dogan/grc-system
dotnet clean src/GrcMvc/GrcMvc.csproj
dotnet build src/GrcMvc/GrcMvc.csproj -c Release

if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Build failed!${NC}"
    exit 1
fi
echo -e "${GREEN}✅ Build succeeded${NC}"

# Step 5: Run Tests
echo -e "${YELLOW}Step 4: Running tests...${NC}"
dotnet test tests/GrcMvc.Tests/GrcMvc.Tests.csproj --no-build

if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Tests failed!${NC}"
    exit 1
fi
echo -e "${GREEN}✅ All tests passed${NC}"

# Step 6: Apply Migrations
echo -e "${YELLOW}Step 5: Applying database migrations...${NC}"
cd src/GrcMvc
dotnet ef database update --no-build

if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Migration failed!${NC}"
    exit 1
fi
echo -e "${GREEN}✅ Migrations applied${NC}"

# Step 7: Verify Database Tables Exist
echo -e "${YELLOW}Step 6: Verifying database tables...${NC}"
PGPASSWORD=postgres psql -h localhost -p 5433 -U postgres -d GrcMvcDb -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';" > /dev/null 2>&1

if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Cannot connect to database!${NC}"
    exit 1
fi
echo -e "${GREEN}✅ Database connection verified${NC}"

# Step 8: Check Seeding Status (before running app)
echo -e "${YELLOW}Step 7: Checking current seeding status...${NC}"
REGULATOR_COUNT=$(PGPASSWORD=postgres psql -h localhost -p 5433 -U postgres -d GrcMvcDb -t -c "SELECT COUNT(*) FROM \"RegulatorCatalogs\";" 2>/dev/null | xargs)
FRAMEWORK_COUNT=$(PGPASSWORD=postgres psql -h localhost -p 5433 -U postgres -d GrcMvcDb -t -c "SELECT COUNT(*) FROM \"FrameworkCatalogs\";" 2>/dev/null | xargs)
CONTROL_COUNT=$(PGPASSWORD=postgres psql -h localhost -p 5433 -U postgres -d GrcMvcDb -t -c "SELECT COUNT(*) FROM \"ControlCatalogs\";" 2>/dev/null | xargs)
WORKFLOW_COUNT=$(PGPASSWORD=postgres psql -h localhost -p 5433 -U postgres -d GrcMvcDb -t -c "SELECT COUNT(*) FROM \"WorkflowDefinitions\";" 2>/dev/null | xargs)

echo "  Regulators: $REGULATOR_COUNT (expected: 91)"
echo "  Frameworks: $FRAMEWORK_COUNT (expected: 162)"
echo "  Controls: $CONTROL_COUNT (expected: 57,211)"
echo "  Workflows: $WORKFLOW_COUNT (expected: 7)"

# Step 9: Run Application (background) and wait for startup
echo -e "${YELLOW}Step 8: Starting application...${NC}"
cd /home/dogan/grc-system/src/GrcMvc

# Start application in background
dotnet run > /tmp/grc-app.log 2>&1 &
APP_PID=$!

# Wait for application to start (check health endpoint)
echo "Waiting for application to start..."
for i in {1..30}; do
    sleep 2
    if curl -s http://localhost:5000/health > /dev/null 2>&1 || curl -s https://localhost:5001/health > /dev/null 2>&1; then
        echo -e "${GREEN}✅ Application started successfully${NC}"
        break
    fi
    if [ $i -eq 30 ]; then
        echo -e "${RED}❌ Application failed to start within 60 seconds${NC}"
        kill $APP_PID 2>/dev/null || true
        exit 1
    fi
done

# Step 10: Wait for Seeding to Complete
echo -e "${YELLOW}Step 9: Waiting for seeding to complete (30 seconds)...${NC}"
sleep 30

# Step 11: Verify Seeding After Application Start
echo -e "${YELLOW}Step 10: Verifying seeding after application start...${NC}"
REGULATOR_COUNT_AFTER=$(PGPASSWORD=postgres psql -h localhost -p 5433 -U postgres -d GrcMvcDb -t -c "SELECT COUNT(*) FROM \"RegulatorCatalogs\";" 2>/dev/null | xargs)
FRAMEWORK_COUNT_AFTER=$(PGPASSWORD=postgres psql -h localhost -p 5433 -U postgres -d GrcMvcDb -t -c "SELECT COUNT(*) FROM \"FrameworkCatalogs\";" 2>/dev/null | xargs)
CONTROL_COUNT_AFTER=$(PGPASSWORD=postgres psql -h localhost -p 5433 -U postgres -d GrcMvcDb -t -c "SELECT COUNT(*) FROM \"ControlCatalogs\";" 2>/dev/null | xargs)
WORKFLOW_COUNT_AFTER=$(PGPASSWORD=postgres psql -h localhost -p 5433 -U postgres -d GrcMvcDb -t -c "SELECT COUNT(*) FROM \"WorkflowDefinitions\";" 2>/dev/null | xargs)

echo ""
echo "═══════════════════════════════════════════════════════════"
echo "  SEEDING VERIFICATION RESULTS"
echo "═══════════════════════════════════════════════════════════"
echo "  Regulators: $REGULATOR_COUNT_AFTER (expected: 91)"
echo "  Frameworks: $FRAMEWORK_COUNT_AFTER (expected: 162)"
echo "  Controls: $CONTROL_COUNT_AFTER (expected: 57,211)"
echo "  Workflows: $WORKFLOW_COUNT_AFTER (expected: 7)"
echo "═══════════════════════════════════════════════════════════"

# Check if seeding is complete
SEEDING_OK=true
if [ "$REGULATOR_COUNT_AFTER" -lt 50 ]; then
    echo -e "${RED}❌ Regulators not fully seeded${NC}"
    SEEDING_OK=false
fi
if [ "$FRAMEWORK_COUNT_AFTER" -lt 100 ]; then
    echo -e "${RED}❌ Frameworks not fully seeded${NC}"
    SEEDING_OK=false
fi
if [ "$WORKFLOW_COUNT_AFTER" -lt 5 ]; then
    echo -e "${RED}❌ Workflows not fully seeded${NC}"
    SEEDING_OK=false
fi

if [ "$SEEDING_OK" = true ]; then
    echo -e "${GREEN}✅ Seeding appears to be working${NC}"
else
    echo -e "${YELLOW}⚠️  Seeding may be incomplete. Check application logs.${NC}"
fi

# Step 12: Test Health Endpoint
echo -e "${YELLOW}Step 11: Testing health endpoint...${NC}"
HEALTH_RESPONSE=$(curl -s http://localhost:5000/health 2>/dev/null || curl -s https://localhost:5001/health 2>/dev/null)
if [ -n "$HEALTH_RESPONSE" ]; then
    echo -e "${GREEN}✅ Health endpoint responding${NC}"
    echo "  Response: $HEALTH_RESPONSE"
else
    echo -e "${RED}❌ Health endpoint not responding${NC}"
fi

# Step 13: Display Application Logs (last 20 lines)
echo ""
echo "═══════════════════════════════════════════════════════════"
echo "  APPLICATION LOGS (Last 20 lines)"
echo "═══════════════════════════════════════════════════════════"
tail -20 /tmp/grc-app.log 2>/dev/null || echo "No logs available yet"

echo ""
echo "═══════════════════════════════════════════════════════════"
echo "  DEPLOYMENT SUMMARY"
echo "═══════════════════════════════════════════════════════════"
echo "  ✅ Build: SUCCESS"
echo "  ✅ Tests: PASSED"
echo "  ✅ Migrations: APPLIED"
echo "  ✅ Database: CONNECTED"
echo "  ✅ Application: RUNNING (PID: $APP_PID)"
echo ""
echo "  Application URL: http://localhost:5000 or https://localhost:5001"
echo "  Health Check: http://localhost:5000/health"
echo ""
echo "  To stop the application: kill $APP_PID"
echo "  To view logs: tail -f /tmp/grc-app.log"
echo "═══════════════════════════════════════════════════════════"

# Note: Application continues running in background
echo ""
echo -e "${YELLOW}⚠️  Application is running in background.${NC}"
echo -e "${YELLOW}⚠️  Review logs and verify seeding before marking as production ready.${NC}"
echo ""
echo "Next steps:"
echo "  1. Check application logs: tail -f /tmp/grc-app.log"
echo "  2. Verify seeding completed: Check database counts"
echo "  3. Test login and key functionality"
echo "  4. Verify all 12 modules are accessible"
echo "  5. Test workflow execution"
echo ""
echo -e "${RED}⚠️  STATUS: NOT YET PRODUCTION READY - Pending verification${NC}"
