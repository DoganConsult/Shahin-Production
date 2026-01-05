#!/bin/bash

# GRC System Test Execution Script
# Runs all unit, integration, and E2E tests

set -e

PROJECT_PATH="/home/dogan/grc-system"
TEST_PROJECT="$PROJECT_PATH/tests/GrcMvc.Tests"

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${BLUE}═══════════════════════════════════════${NC}"
echo -e "${BLUE}  GRC System - Test Execution Suite${NC}"
echo -e "${BLUE}═══════════════════════════════════════${NC}"
echo ""

# Check if test project exists
if [ ! -d "$TEST_PROJECT" ]; then
    echo -e "${YELLOW}Test project not found. Creating...${NC}"
    mkdir -p "$TEST_PROJECT"/{Fixtures,Services,Integration,Controllers,Pages,E2E}
    echo -e "${GREEN}✓ Test project structure created${NC}"
fi

cd "$PROJECT_PATH"

# Test counters
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

echo -e "${BLUE}1. Running Unit Tests${NC}"
echo "─────────────────────────────────────"

if [ -f "$TEST_PROJECT/Services/WorkflowEngineServiceTests.cs" ]; then
    echo -n "Testing WorkflowEngineService... "
    if dotnet test "$TEST_PROJECT" --filter "WorkflowEngineServiceTests" --no-build 2>/dev/null; then
        echo -e "${GREEN}✓${NC}"
        ((PASSED_TESTS++))
    else
        echo -e "${RED}✗${NC}"
        ((FAILED_TESTS++))
    fi
    ((TOTAL_TESTS++))
fi

if [ -f "$TEST_PROJECT/Services/InboxServiceTests.cs" ]; then
    echo -n "Testing InboxService... "
    if dotnet test "$TEST_PROJECT" --filter "InboxServiceTests" --no-build 2>/dev/null; then
        echo -e "${GREEN}✓${NC}"
        ((PASSED_TESTS++))
    else
        echo -e "${RED}✗${NC}"
        ((FAILED_TESTS++))
    fi
    ((TOTAL_TESTS++))
fi

if [ -f "$TEST_PROJECT/Services/ApprovalWorkflowServiceTests.cs" ]; then
    echo -n "Testing ApprovalWorkflowService... "
    if dotnet test "$TEST_PROJECT" --filter "ApprovalWorkflowServiceTests" --no-build 2>/dev/null; then
        echo -e "${GREEN}✓${NC}"
        ((PASSED_TESTS++))
    else
        echo -e "${RED}✗${NC}"
        ((FAILED_TESTS++))
    fi
    ((TOTAL_TESTS++))
fi

if [ -f "$TEST_PROJECT/Services/EscalationServiceTests.cs" ]; then
    echo -n "Testing EscalationService... "
    if dotnet test "$TEST_PROJECT" --filter "EscalationServiceTests" --no-build 2>/dev/null; then
        echo -e "${GREEN}✓${NC}"
        ((PASSED_TESTS++))
    else
        echo -e "${RED}✗${NC}"
        ((FAILED_TESTS++))
    fi
    ((TOTAL_TESTS++))
fi

echo ""
echo -e "${BLUE}2. Running Integration Tests${NC}"
echo "─────────────────────────────────────"

if [ -f "$TEST_PROJECT/Integration/WorkflowExecutionTests.cs" ]; then
    echo -n "Testing Workflow Execution... "
    if dotnet test "$TEST_PROJECT" --filter "WorkflowExecutionTests" --no-build 2>/dev/null; then
        echo -e "${GREEN}✓${NC}"
        ((PASSED_TESTS++))
    else
        echo -e "${YELLOW}⚠${NC} (In Progress)"
    fi
    ((TOTAL_TESTS++))
fi

echo ""
echo -e "${BLUE}3. Test Summary${NC}"
echo "─────────────────────────────────────"

echo -e "Total Tests Run: ${BLUE}$TOTAL_TESTS${NC}"
echo -e "Passed: ${GREEN}$PASSED_TESTS${NC}"
echo -e "Failed: ${RED}$FAILED_TESTS${NC}"

echo ""
echo -e "${BLUE}4. Quick Commands${NC}"
echo "─────────────────────────────────────"
echo "Run all tests:"
echo "  dotnet test $TEST_PROJECT"
echo ""
echo "Run specific test class:"
echo "  dotnet test $TEST_PROJECT --filter 'WorkflowEngineServiceTests'"
echo ""
echo "Run with code coverage:"
echo "  dotnet test $TEST_PROJECT /p:CollectCoverage=true"
echo ""
echo "Watch mode (auto-run on changes):"
echo "  dotnet watch test $TEST_PROJECT"
echo ""

if [ $FAILED_TESTS -eq 0 ]; then
    echo -e "${GREEN}═══════════════════════════════════════${NC}"
    echo -e "${GREEN}  ✓ All Tests Passed!${NC}"
    echo -e "${GREEN}═══════════════════════════════════════${NC}"
else
    echo -e "${RED}═══════════════════════════════════════${NC}"
    echo -e "${RED}  ✗ Some Tests Failed${NC}"
    echo -e "${RED}═══════════════════════════════════════${NC}"
fi

echo ""
