#!/bin/bash
set -euo pipefail

# Phase 1 Validation Script - Foundation & Security Prerequisites
# Usage: ./scripts/validate-phase1.sh [--base-url http://localhost:5000]

BASE_URL="${2:-http://localhost:5000}"
DB_CONTAINER="${DB_CONTAINER:-shahin-postgres}"
DB_USER="${DB_USER:-shahin_admin}"
DB_NAME="${DB_NAME:-shahin_grc}"

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "================================================"
echo "Phase 1 Validation - Foundation & Security"
echo "================================================"
echo "Base URL: $BASE_URL"
echo "Database: $DB_CONTAINER / $DB_USER@$DB_NAME"
echo "------------------------------------------------"

# Track failures
FAILED=0
CHECKS_PASSED=0
CHECKS_TOTAL=7

# 1. Docker reachable
echo -n "[1/7] Docker daemon check... "
if docker info >/dev/null 2>&1; then
    echo -e "${GREEN}✓ PASS${NC}"
    ((CHECKS_PASSED++))
else
    echo -e "${RED}✗ FAIL: Docker not reachable${NC}"
    echo "  -> Ensure Docker Desktop is running"
    ((FAILED++))
fi

# 2. Container running
echo -n "[2/7] PostgreSQL container check... "
if docker ps --format "{{.Names}}" | grep -q "^${DB_CONTAINER}$"; then
    echo -e "${GREEN}✓ PASS${NC}"
    ((CHECKS_PASSED++))
else
    echo -e "${RED}✗ FAIL: Container '$DB_CONTAINER' not running${NC}"
    echo "  -> Run: docker start $DB_CONTAINER"
    docker ps -a --format "table {{.Names}}\t{{.Status}}" | grep -i "$DB_CONTAINER" || true
    ((FAILED++))
fi

# 3. Database connectivity
echo -n "[3/7] Database connectivity check... "
if docker exec "$DB_CONTAINER" psql -U "$DB_USER" -d "$DB_NAME" -c "SELECT 1;" >/dev/null 2>&1; then
    echo -e "${GREEN}✓ PASS${NC}"
    ((CHECKS_PASSED++))
else
    echo -e "${RED}✗ FAIL: Cannot connect to database${NC}"
    echo "  -> Check credentials: $DB_USER@$DB_NAME"
    ((FAILED++))
fi

# 4. Identity tables exist
echo -n "[4/7] Identity tables check... "
TABLES=$(docker exec "$DB_CONTAINER" psql -U "$DB_USER" -d "$DB_NAME" -t -c "\dt" 2>/dev/null | grep -c "AspNetUsers" || echo "0")
if [ "$TABLES" -gt 0 ]; then
    echo -e "${GREEN}✓ PASS${NC}"
    ((CHECKS_PASSED++))
else
    echo -e "${RED}✗ FAIL: Identity tables not found${NC}"
    echo "  -> Run migrations: dotnet ef database update"
    ((FAILED++))
fi

# 5. Migrations applied
echo -n "[5/7] EF Migrations check... "
MIGRATION_COUNT=$(docker exec "$DB_CONTAINER" psql -U "$DB_USER" -d "$DB_NAME" -t -c "SELECT COUNT(*) FROM \"__EFMigrationsHistory\";" 2>/dev/null | xargs || echo "0")
if [ "$MIGRATION_COUNT" -gt 0 ]; then
    echo -e "${GREEN}✓ PASS (${MIGRATION_COUNT} migrations)${NC}"
    ((CHECKS_PASSED++))
else
    echo -e "${RED}✗ FAIL: No migrations found${NC}"
    echo "  -> Run: dotnet ef database update --context GrcDbContext"
    echo "  -> Run: dotnet ef database update --context GrcAuthDbContext"
    ((FAILED++))
fi

# 6. ApplicationUser columns
echo -n "[6/7] ApplicationUser columns check... "
REQUIRED_COLUMNS=("FirstName" "LastName" "Department" "JobTitle" "Abilities")
MISSING_COLUMNS=()

for col in "${REQUIRED_COLUMNS[@]}"; do
    COL_EXISTS=$(docker exec "$DB_CONTAINER" psql -U "$DB_USER" -d "$DB_NAME" -t -c "
        SELECT COUNT(*) FROM information_schema.columns 
        WHERE table_name = 'AspNetUsers' 
        AND column_name = '$col';" 2>/dev/null | xargs || echo "0")
    
    if [ "$COL_EXISTS" -eq 0 ]; then
        MISSING_COLUMNS+=("$col")
    fi
done

if [ ${#MISSING_COLUMNS[@]} -eq 0 ]; then
    echo -e "${GREEN}✓ PASS${NC}"
    ((CHECKS_PASSED++))
else
    echo -e "${RED}✗ FAIL: Missing columns: ${MISSING_COLUMNS[*]}${NC}"
    echo "  -> ApplicationUser schema incomplete"
    echo "  -> Check migrations or recreate Identity tables"
    ((FAILED++))
fi

# 7. Application health endpoints
echo -n "[7/7] Application health check... "
# Try main health endpoint
if curl -f -s "$BASE_URL/health" >/dev/null 2>&1; then
    echo -e "${GREEN}✓ PASS${NC}"
    ((CHECKS_PASSED++))
    
    # Check additional endpoints (non-blocking)
    echo "  Checking additional endpoints:"
    for endpoint in "/health/ready" "/health/live"; do
        echo -n "    $endpoint: "
        if curl -f -s "$BASE_URL$endpoint" >/dev/null 2>&1; then
            echo -e "${GREEN}✓${NC}"
        else
            echo -e "${YELLOW}not available${NC}"
        fi
    done
else
    echo -e "${RED}✗ FAIL: Application not responding${NC}"
    echo "  -> Start app: dotnet run --project src/GrcMvc/GrcMvc.csproj"
    echo "  -> Base URL: $BASE_URL"
    ((FAILED++))
fi

# Summary
echo "================================================"
echo "VALIDATION SUMMARY"
echo "------------------------------------------------"
echo "Checks Passed: $CHECKS_PASSED / $CHECKS_TOTAL"

if [ $FAILED -eq 0 ]; then
    echo -e "${GREEN}✅ PHASE 1 READY FOR PRODUCTION${NC}"
    echo "All foundation and security prerequisites met!"
    exit 0
else
    echo -e "${RED}❌ PHASE 1 NOT READY${NC}"
    echo "$FAILED critical check(s) failed. Fix issues above."
    exit 1
fi
