#!/bin/bash

# GRC Workflow & Onboarding API Test Script
# Tests the REST API endpoints for workflow and onboarding

BASE_URL="http://localhost:6000"
API_URL="$BASE_URL/api"

echo "🔍 GRC System API Test Report"
echo "=============================="
echo "Base URL: $BASE_URL"
echo "API URL: $API_URL"
echo ""

# Color codes
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to test endpoint
test_endpoint() {
    local method=$1
    local endpoint=$2
    local description=$3
    
    echo -n "Testing $method $endpoint ... "
    
    response=$(curl -s -w "\n%{http_code}" -X $method "$API_URL$endpoint" -H "Content-Type: application/json" 2>/dev/null)
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    if [ "$http_code" -ge 200 ] && [ "$http_code" -lt 300 ]; then
        echo -e "${GREEN}✓${NC} ($http_code)"
        return 0
    elif [ "$http_code" -ge 400 ] && [ "$http_code" -lt 500 ]; then
        echo -e "${YELLOW}⚠${NC} ($http_code) - $description"
        return 0
    else
        echo -e "${RED}✗${NC} ($http_code)"
        return 1
    fi
}

echo "📋 Testing Workflow API Endpoints"
echo "---------------------------------"
test_endpoint "GET" "/workflow" "List workflows"
test_endpoint "GET" "/workflow/00000000-0000-0000-0000-000000000001" "Get workflow details"

echo ""
echo "📋 Testing Approval API Endpoints"
echo "---------------------------------"
test_endpoint "GET" "/approval-workflow/00000000-0000-0000-0000-000000000001" "Get approval workflow"

echo ""
echo "📋 Testing Inbox API Endpoints"
echo "--------------------------------"
test_endpoint "GET" "/inbox" "Get inbox"

echo ""
echo "✅ API Test Complete"
echo ""
echo "Note: If endpoints return 404 or 500, verify:"
echo "  1. Application is running on $BASE_URL"
echo "  2. API controllers are registered in Program.cs"
echo "  3. Database is accessible"
