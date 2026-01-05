#!/bin/bash

# Test script for Risks Module Integration
# This script tests the Risks module functionality after migration

set -e

echo "=========================================="
echo "Risks Module Integration Test"
echo "=========================================="
echo ""

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
APP_URL="${APP_URL:-http://localhost:5137}"
RISKS_URL="${APP_URL}/risks"
CREATE_URL="${APP_URL}/risks/create"

echo -e "${YELLOW}Step 1: Verify Migration Applied${NC}"
echo "Checking if Risk table has new columns..."
echo ""

# Check if we can connect to database (if psql is available)
if command -v psql &> /dev/null; then
    echo "Checking database connection..."
    # This would require database credentials - skip for now
    echo "  ⚠️  Database check skipped (requires credentials)"
else
    echo "  ⚠️  psql not available - skipping database check"
fi

echo ""
echo -e "${YELLOW}Step 2: Test Application Endpoints${NC}"
echo ""

# Test 1: Check if Risks index page is accessible
echo "Test 1: Risks Index Page"
if curl -s -o /dev/null -w "%{http_code}" "${RISKS_URL}" | grep -q "200\|302"; then
    echo -e "  ${GREEN}✓${NC} Risks page is accessible"
else
    echo -e "  ${RED}✗${NC} Risks page returned error"
    echo "  Response: $(curl -s -w "\nHTTP Code: %{http_code}" "${RISKS_URL}" | tail -1)"
fi

echo ""

# Test 2: Check if Create page is accessible
echo "Test 2: Risks Create Page"
if curl -s -o /dev/null -w "%{http_code}" "${CREATE_URL}" | grep -q "200\|302"; then
    echo -e "  ${GREEN}✓${NC} Create page is accessible"
else
    echo -e "  ${RED}✗${NC} Create page returned error"
    echo "  Response: $(curl -s -w "\nHTTP Code: %{http_code}" "${CREATE_URL}" | tail -1)"
fi

echo ""
echo -e "${YELLOW}Step 3: Manual Testing Checklist${NC}"
echo ""
echo "Please test the following manually in the browser:"
echo ""
echo "1. ${GREEN}Create a Risk${NC}"
echo "   - Navigate to: ${CREATE_URL}"
echo "   - Fill in all required fields:"
echo "     * Title: 'Test Risk - Data Breach'"
echo "     * Category: 'Compliance'"
echo "     * Description: 'Test description'"
echo "     * Inherent Score: 20"
echo "     * Residual Score: 15"
echo "     * Impact: 'High'"
echo "     * Likelihood: 'Medium'"
echo "     * Responsible Party: 'Security Team'"
echo "     * Identified Date: Today's date"
echo "   - Click 'Register Risk'"
echo "   - Verify: Risk appears in the list"
echo "   - Verify: No error messages displayed"
echo ""

echo "2. ${GREEN}View Risks List${NC}"
echo "   - Navigate to: ${RISKS_URL}"
echo "   - Verify: Risks are displayed in table"
echo "   - Verify: Summary cards show correct counts"
echo "   - Verify: Risk numbers are generated (RISK-XXXX format)"
echo ""

echo "3. ${GREEN}Filter Risks${NC}"
echo "   - On Risks list page:"
echo "     * Select 'Open' from Status filter"
echo "     * Verify: Only Open risks are shown"
echo "     * Select 'High' from Rating filter"
echo "     * Verify: Only High-rated risks are shown"
echo "     * Click 'Reset Filters'"
echo "     * Verify: All risks are shown again"
echo ""

echo "4. ${GREEN}Edit a Risk${NC}"
echo "   - Click 'Edit' on any risk"
echo "   - Modify:"
echo "     * Title: 'Updated Test Risk'"
echo "     * Status: 'Mitigated'"
echo "     * Residual Score: 10"
echo "   - Click 'Save Changes'"
echo "   - Verify: Changes are saved"
echo "   - Verify: Risk appears updated in list"
echo ""

echo "5. ${GREEN}Error Handling${NC}"
echo "   - Test error scenarios:"
echo "     * Try to create risk with missing required fields"
echo "     * Verify: Error message is displayed"
echo "     * Verify: ErrorAlert component shows error"
echo "     * Try to edit non-existent risk (invalid ID)"
echo "     * Verify: Error message is displayed"
echo ""

echo ""
echo -e "${YELLOW}Step 4: Database Verification${NC}"
echo ""
echo "To verify migration was applied, run:"
echo ""
echo "  psql -U postgres -d GrcMvcDb -c \""
echo "    SELECT column_name, data_type, is_nullable"
echo "    FROM information_schema.columns"
echo "    WHERE table_name = 'Risks'"
echo "    AND column_name IN ('Title', 'RiskNumber', 'IdentifiedDate', 'ResponsibleParty', 'ConsequenceArea');"
echo "  \""
echo ""

echo "=========================================="
echo "Test script completed"
echo "=========================================="
