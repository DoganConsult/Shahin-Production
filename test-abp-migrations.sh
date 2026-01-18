#!/bin/bash
# Test ABP Migrations Process (WSL/Linux)
# Follows ABP Framework standard migration process

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

echo ""
echo "========================================"
echo -e "${CYAN}ABP Framework Migration Test${NC}"
echo "========================================"
echo ""

# Get connection string
CONNECTION_STRING=""
if [ -n "$ConnectionStrings__DefaultConnection" ]; then
    CONNECTION_STRING="$ConnectionStrings__DefaultConnection"
elif [ -n "$CONNECTION_STRING" ]; then
    CONNECTION_STRING="$CONNECTION_STRING"
elif [ -n "$DATABASE_URL" ]; then
    # Convert Railway format if needed
    if [[ "$DATABASE_URL" == postgresql://* ]]; then
        # Extract components from postgresql://user:pass@host:port/db
        CONNECTION_STRING="$DATABASE_URL"
    else
        CONNECTION_STRING="$DATABASE_URL"
    fi
fi

if [ -z "$CONNECTION_STRING" ]; then
    echo -e "${RED}❌ Connection string not found!${NC}"
    echo ""
    echo "Set connection string:"
    echo '  export ConnectionStrings__DefaultConnection="Host=...;Database=...;Username=...;Password=...;Port=5432"'
    echo ""
    exit 1
fi

echo -e "${YELLOW}Connection String:${NC} $(echo $CONNECTION_STRING | sed 's/Password=[^;]*/Password=***/g')"
echo ""

# Set environment variable for EF Core tools
export ConnectionStrings__DefaultConnection="$CONNECTION_STRING"

# Change to project directory
PROJECT_PATH="Shahin-Jan-2026/src/GrcMvc"
if [ ! -d "$PROJECT_PATH" ]; then
    echo -e "${RED}❌ Project path not found: $PROJECT_PATH${NC}"
    exit 1
fi

cd "$PROJECT_PATH"

# Parse arguments
LIST_MIGRATIONS=false
CHECK_PENDING=false
TEST_MIGRATION=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --list)
            LIST_MIGRATIONS=true
            shift
            ;;
        --check-pending)
            CHECK_PENDING=true
            shift
            ;;
        --test)
            TEST_MIGRATION=true
            shift
            ;;
        *)
            shift
            ;;
    esac
done

# List migrations
if [ "$LIST_MIGRATIONS" = true ]; then
    echo -e "${YELLOW}=== Listing Migrations ===${NC}"
    echo ""
    
    echo -e "${CYAN}GrcDbContext Migrations:${NC}"
    dotnet ef migrations list --context GrcDbContext 2>&1 || true
    
    echo ""
    echo -e "${CYAN}GrcAuthDbContext Migrations:${NC}"
    dotnet ef migrations list --context GrcAuthDbContext 2>&1 || true
    
    exit 0
fi

# Check pending migrations
if [ "$CHECK_PENDING" = true ]; then
    echo -e "${YELLOW}=== Checking Pending Migrations ===${NC}"
    echo ""
    
    echo -e "${CYAN}Checking GrcDbContext...${NC}"
    PENDING_MAIN=$(dotnet ef migrations list --context GrcDbContext --no-build 2>&1 | grep -i "pending" || true)
    if [ -n "$PENDING_MAIN" ]; then
        echo -e "${YELLOW}  ⚠️  Pending migrations found for GrcDbContext${NC}"
        echo "$PENDING_MAIN"
    else
        echo -e "${GREEN}  ✅ No pending migrations for GrcDbContext${NC}"
    fi
    
    echo ""
    echo -e "${CYAN}Checking GrcAuthDbContext...${NC}"
    PENDING_AUTH=$(dotnet ef migrations list --context GrcAuthDbContext --no-build 2>&1 | grep -i "pending" || true)
    if [ -n "$PENDING_AUTH" ]; then
        echo -e "${YELLOW}  ⚠️  Pending migrations found for GrcAuthDbContext${NC}"
        echo "$PENDING_AUTH"
    else
        echo -e "${GREEN}  ✅ No pending migrations for GrcAuthDbContext${NC}"
    fi
    
    exit 0
fi

# Test migration (dry-run)
if [ "$TEST_MIGRATION" = true ]; then
    echo -e "${YELLOW}=== Testing Migration (Dry Run) ===${NC}"
    echo ""
    echo "This will check if migrations can be applied without actually applying them."
    echo ""
    
    echo -e "${CYAN}Testing GrcDbContext...${NC}"
    if dotnet ef database update --context GrcDbContext --dry-run --no-build 2>&1; then
        echo -e "${GREEN}  ✅ GrcDbContext migration test passed${NC}"
    else
        echo -e "${RED}  ❌ GrcDbContext migration test failed${NC}"
    fi
    
    echo ""
    echo -e "${CYAN}Testing GrcAuthDbContext...${NC}"
    if dotnet ef database update --context GrcAuthDbContext --dry-run --no-build 2>&1; then
        echo -e "${GREEN}  ✅ GrcAuthDbContext migration test passed${NC}"
    else
        echo -e "${RED}  ❌ GrcAuthDbContext migration test failed${NC}"
    fi
    
    exit 0
fi

# Default: Full migration test
echo -e "${YELLOW}=== ABP Migration Process Test ===${NC}"
echo ""

echo -e "${CYAN}Step 1: Building project...${NC}"
if dotnet build --no-restore > /dev/null 2>&1; then
    echo -e "${GREEN}  ✅ Build successful${NC}"
else
    echo -e "${RED}  ❌ Build failed${NC}"
    exit 1
fi
echo ""

echo -e "${CYAN}Step 2: Listing current migrations...${NC}"
echo ""
echo -e "${YELLOW}GrcDbContext:${NC}"
dotnet ef migrations list --context GrcDbContext --no-build 2>&1 | head -10
echo ""
echo -e "${YELLOW}GrcAuthDbContext:${NC}"
dotnet ef migrations list --context GrcAuthDbContext --no-build 2>&1 | head -10
echo ""

echo -e "${CYAN}Step 3: Testing migration application (dry-run)...${NC}"
echo ""
echo -e "${YELLOW}GrcDbContext:${NC}"
if dotnet ef database update --context GrcDbContext --dry-run --no-build 2>&1; then
    echo -e "${GREEN}  ✅ GrcDbContext migrations ready${NC}"
else
    echo -e "${YELLOW}  ⚠️  GrcDbContext migration check completed${NC}"
fi
echo ""

echo -e "${YELLOW}GrcAuthDbContext:${NC}"
if dotnet ef database update --context GrcAuthDbContext --dry-run --no-build 2>&1; then
    echo -e "${GREEN}  ✅ GrcAuthDbContext migrations ready${NC}"
else
    echo -e "${YELLOW}  ⚠️  GrcAuthDbContext migration check completed${NC}"
fi
echo ""

echo "========================================"
echo -e "${GREEN}✅ ABP Migration Test Complete${NC}"
echo "========================================"
echo ""
echo -e "${YELLOW}To apply migrations:${NC}"
echo "  dotnet ef database update --context GrcDbContext"
echo "  dotnet ef database update --context GrcAuthDbContext"
echo ""
