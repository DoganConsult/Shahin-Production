#!/bin/bash
# Golden Flow Validation Script
# Gate B: End-to-End Golden Flows - Production Readiness
# Reference: ProductionReadinessGates.GateB_GoldenFlows

BASE_URL="${TEST_API_URL:-http://localhost:5000}"
ADMIN_EMAIL="${TEST_ADMIN_EMAIL:-admin@grc.com}"
ADMIN_PASSWORD="${TEST_ADMIN_PASSWORD:-Admin@12345}"
OUTPUT_DIR="./Golden_Flow_Evidence"

# Create output directory
mkdir -p "$OUTPUT_DIR"

TIMESTAMP=$(date +%Y%m%d_%H%M%S)
REPORT_FILE="$OUTPUT_DIR/GoldenFlowReport_$TIMESTAMP.md"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Initialize report
cat > "$REPORT_FILE" << EOF
# Golden Flow Test Report

**Generated:** $(date -u +"%Y-%m-%d %H:%M:%S") UTC
**Base URL:** $BASE_URL
**Test Environment:** $(if [[ "$BASE_URL" == *"localhost"* ]]; then echo "Local"; else echo "Staging"; fi)

---

EOF

log() {
    echo -e "$1"
    echo "$1" | sed 's/\x1b\[[0-9;]*m//g' >> "$REPORT_FILE"
}

test_endpoint() {
    local method=$1
    local endpoint=$2
    local data=$3
    local description=$4
    local auth=$5

    local url="${BASE_URL}${endpoint}"
    local curl_args=(-s -w "\n%{http_code}" -X "$method")

    if [ -n "$data" ]; then
        curl_args+=(-H "Content-Type: application/json" -d "$data")
    fi

    if [ -n "$auth" ]; then
        curl_args+=(-H "Authorization: Bearer $auth")
    fi

    local response
    response=$(curl "${curl_args[@]}" "$url")

    local http_code
    http_code=$(echo "$response" | tail -n1)
    local body
    body=$(echo "$response" | sed '$d')

    if [[ "$http_code" =~ ^2 ]]; then
        echo "PASS:$http_code:$body"
    else
        echo "FAIL:$http_code:$body"
    fi
}

# Generate unique test data
TEST_ID=$(uuidgen 2>/dev/null || cat /proc/sys/kernel/random/uuid 2>/dev/null || date +%s)
TEST_ID=${TEST_ID:0:8}

echo "=============================================="
echo "  GOLDEN FLOW VALIDATION - GATE B"
echo "=============================================="
echo ""

# ============================================
# B1: Self Registration
# ============================================
log "## Flow B1: Self Registration"
log ""

TEST_EMAIL="test-user-${TEST_ID}@test.com"
TEST_PASSWORD="Test@12345678"

log "**Test Email:** $TEST_EMAIL"
log ""

# Register
B1_REG=$(test_endpoint "POST" "/api/auth/register" "{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\",\"fullName\":\"Test User B1\"}" "Register")
B1_REG_STATUS=$(echo "$B1_REG" | cut -d: -f1)
B1_REG_CODE=$(echo "$B1_REG" | cut -d: -f2)

# Login
B1_LOGIN=$(test_endpoint "POST" "/api/auth/login" "{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\"}" "Login")
B1_LOGIN_STATUS=$(echo "$B1_LOGIN" | cut -d: -f1)

log "| Step | Action | Status |"
log "|------|--------|--------|"
log "| 1 | POST /api/auth/register | $B1_REG_STATUS ($B1_REG_CODE) |"
log "| 2 | POST /api/auth/login | $B1_LOGIN_STATUS |"
log ""
log "**Expected Audit Events:** AM01_USER_CREATED, AM01_USER_REGISTERED"
log ""
log "---"
log ""

# ============================================
# B2: Trial Signup
# ============================================
log "## Flow B2: Trial Signup"
log ""

TRIAL_COMPANY="Test Company ${TEST_ID}"
TRIAL_EMAIL="trial-${TEST_ID}@testco.com"

log "**Company:** $TRIAL_COMPANY"
log "**Email:** $TRIAL_EMAIL"
log ""

B2_SIGNUP=$(test_endpoint "POST" "/api/trial/signup" "{\"companyName\":\"$TRIAL_COMPANY\",\"email\":\"$TRIAL_EMAIL\",\"fullName\":\"Trial Admin\",\"industry\":\"Technology\"}" "Trial Signup")
B2_STATUS=$(echo "$B2_SIGNUP" | cut -d: -f1)

log "| Step | Action | Status |"
log "|------|--------|--------|"
log "| 1 | POST /api/trial/signup | $B2_STATUS |"
log ""
log "**Expected Audit Events:** AM01_TRIAL_SIGNUP_INITIATED"
log ""
log "---"
log ""

# ============================================
# B3: Trial Provision
# ============================================
log "## Flow B3: Trial Provision"
log ""

PROVISION_EMAIL="provision-${TEST_ID}@testco.com"
PROVISION_PASSWORD="Provision@12345"

log "**Email:** $PROVISION_EMAIL"
log ""

# Signup first
B3_SIGNUP=$(test_endpoint "POST" "/api/trial/signup" "{\"companyName\":\"Provision Test ${TEST_ID}\",\"email\":\"$PROVISION_EMAIL\",\"fullName\":\"Provision Admin\",\"industry\":\"Technology\"}" "Trial Signup for Provision")
B3_SIGNUP_STATUS=$(echo "$B3_SIGNUP" | cut -d: -f1)

# Provision
B3_PROVISION=$(test_endpoint "POST" "/api/trial/provision" "{\"email\":\"$PROVISION_EMAIL\",\"password\":\"$PROVISION_PASSWORD\"}" "Trial Provision")
B3_PROVISION_STATUS=$(echo "$B3_PROVISION" | cut -d: -f1)

# Login
B3_LOGIN=$(test_endpoint "POST" "/api/auth/login" "{\"email\":\"$PROVISION_EMAIL\",\"password\":\"$PROVISION_PASSWORD\"}" "Login after provision")
B3_LOGIN_STATUS=$(echo "$B3_LOGIN" | cut -d: -f1)

log "| Step | Action | Status |"
log "|------|--------|--------|"
log "| 1 | POST /api/trial/signup | $B3_SIGNUP_STATUS |"
log "| 2 | POST /api/trial/provision | $B3_PROVISION_STATUS |"
log "| 3 | POST /api/auth/login | $B3_LOGIN_STATUS |"
log ""
log "**Expected Audit Events:** AM01_TENANT_CREATED, AM01_USER_CREATED, AM03_ROLE_ASSIGNED"
log ""
log "---"
log ""

# ============================================
# Admin Authentication for B4-B6
# ============================================
log "## Flows B4-B6: Require Admin Authentication"
log ""

ADMIN_LOGIN=$(test_endpoint "POST" "/api/auth/login" "{\"email\":\"$ADMIN_EMAIL\",\"password\":\"$ADMIN_PASSWORD\"}" "Admin Login")
ADMIN_STATUS=$(echo "$ADMIN_LOGIN" | cut -d: -f1)
ADMIN_BODY=$(echo "$ADMIN_LOGIN" | cut -d: -f3-)

ADMIN_TOKEN=""
TENANT_ID=""

if [ "$ADMIN_STATUS" = "PASS" ]; then
    # Extract token (basic JSON parsing)
    ADMIN_TOKEN=$(echo "$ADMIN_BODY" | grep -o '"token":"[^"]*"' | head -1 | cut -d'"' -f4)
    TENANT_ID=$(echo "$ADMIN_BODY" | grep -o '"tenantId":"[^"]*"' | head -1 | cut -d'"' -f4)
    log "Admin authentication: **PASS**"
else
    log "Admin authentication: **FAIL**"
fi
log ""

# ============================================
# B4: User Invite
# ============================================
log "## Flow B4: User Invite"
log ""

if [ -n "$ADMIN_TOKEN" ] && [ -n "$TENANT_ID" ]; then
    INVITE_EMAIL="invited-${TEST_ID}@test.com"
    log "**Invite Email:** $INVITE_EMAIL"
    log ""

    B4_INVITE=$(test_endpoint "POST" "/api/tenants/${TENANT_ID}/users/invite" "{\"email\":\"$INVITE_EMAIL\",\"roleCode\":\"TenantUser\",\"message\":\"Welcome\"}" "User Invite" "$ADMIN_TOKEN")
    B4_STATUS=$(echo "$B4_INVITE" | cut -d: -f1)

    log "| Step | Action | Status |"
    log "|------|--------|--------|"
    log "| 1 | POST /api/tenants/{id}/users/invite | $B4_STATUS |"
else
    log "**Note:** Requires admin authentication"
    B4_STATUS="SKIP"
fi
log ""
log "**Expected Audit Events:** AM01_USER_INVITED"
log ""
log "---"
log ""

# B5 and B6 require specific setup
log "## Flow B5: Accept Invite"
log ""
log "**Note:** Requires valid invitation token from B4"
log "Manual verification required with actual token"
log ""
log "**Expected Audit Events:** AM01_USER_CREATED, AM03_ROLE_ASSIGNED"
log ""
log "---"
log ""

log "## Flow B6: Role Change"
log ""
log "**Note:** Requires existing user ID"
log "Manual verification required with actual user"
log ""
log "**Expected Audit Events:** AM03_ROLE_ASSIGNED, AM03_ROLE_CHANGED"
log ""
log "---"
log ""

# ============================================
# Summary
# ============================================
log "## Summary"
log ""
log "| Flow | Description | Status |"
log "|------|-------------|--------|"
log "| B1 | Self Registration | $(if [ "$B1_REG_STATUS" = "PASS" ] && [ "$B1_LOGIN_STATUS" = "PASS" ]; then echo "PASS"; else echo "FAIL"; fi) |"
log "| B2 | Trial Signup | $B2_STATUS |"
log "| B3 | Trial Provision | $B3_SIGNUP_STATUS |"
log "| B4 | User Invite | $B4_STATUS |"
log "| B5 | Accept Invite | Manual |"
log "| B6 | Role Change | Manual |"
log ""
log "---"
log ""
log "*Report generated by test-golden-flows.sh*"

echo ""
echo -e "${GREEN}Report saved to: $REPORT_FILE${NC}"
echo ""
