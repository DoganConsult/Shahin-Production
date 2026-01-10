#!/bin/bash
# =============================================================================
# SHAHIN AI - Production Deployment Verification Script
# End-to-end verification of landing page to GRC platform flow
# =============================================================================

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

LANDING_URL="${LANDING_URL:-https://shahin-ai.com}"
APP_URL="${APP_URL:-https://app.shahin-ai.com}"

echo "=========================================="
echo "  Shahin AI Production Verification"
echo "=========================================="
echo ""
echo "Landing URL: $LANDING_URL"
echo "App URL: $APP_URL"
echo ""

PASS=0
FAIL=0

check() {
    local name="$1"
    local result="$2"
    if [ "$result" = "true" ]; then
        echo -e "${GREEN}[PASS]${NC} $name"
        ((PASS++))
    else
        echo -e "${RED}[FAIL]${NC} $name"
        ((FAIL++))
    fi
}

# =============================================================================
# PHASE 1: Landing Page Checks
# =============================================================================
echo ""
echo "--- PHASE 1: Landing Page ---"

# Check landing page is accessible
LANDING_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$LANDING_URL" 2>/dev/null || echo "000")
check "Landing page accessible ($LANDING_URL)" "$([ "$LANDING_STATUS" = "200" ] && echo true || echo false)"

# Check SSL certificate
SSL_CHECK=$(echo | openssl s_client -connect ${LANDING_URL#https://}:443 2>/dev/null | grep -c "Verify return code: 0" || echo "0")
check "SSL certificate valid" "$([ "$SSL_CHECK" -ge "1" ] && echo true || echo false)"

# =============================================================================
# PHASE 2: API Endpoint Checks
# =============================================================================
echo ""
echo "--- PHASE 2: API Endpoints ---"

# Check app health endpoint
APP_HEALTH=$(curl -s -o /dev/null -w "%{http_code}" "$APP_URL/health" 2>/dev/null || echo "000")
check "App health endpoint" "$([ "$APP_HEALTH" = "200" ] && echo true || echo false)"

# Check trial signup endpoint (OPTIONS for CORS)
CORS_CHECK=$(curl -s -I -X OPTIONS "$APP_URL/api/Landing/StartTrial" \
    -H "Origin: $LANDING_URL" \
    -H "Access-Control-Request-Method: POST" 2>/dev/null | grep -ci "access-control-allow" || echo "0")
check "CORS headers present" "$([ "$CORS_CHECK" -ge "1" ] && echo true || echo false)"

# Check login page
LOGIN_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$APP_URL/Account/Login" 2>/dev/null || echo "000")
check "Login page accessible" "$([ "$LOGIN_STATUS" = "200" ] && echo true || echo false)"

# =============================================================================
# PHASE 3: Trial Signup Flow
# =============================================================================
echo ""
echo "--- PHASE 3: Trial Signup Flow ---"

# Test trial signup endpoint with sample data
TRIAL_RESPONSE=$(curl -s -X POST "$APP_URL/api/Landing/StartTrial" \
    -H "Content-Type: application/json" \
    -H "Origin: $LANDING_URL" \
    -d '{"Email":"test-verify@example.com","FullName":"Verification Test","CompanyName":"Verify Corp"}' 2>/dev/null || echo "{}")

# Check if response contains expected fields
TRIAL_SUCCESS=$(echo "$TRIAL_RESPONSE" | grep -c '"success"' || echo "0")
check "Trial signup API responds" "$([ "$TRIAL_SUCCESS" -ge "1" ] && echo true || echo false)"

# =============================================================================
# PHASE 4: Onboarding Endpoints
# =============================================================================
echo ""
echo "--- PHASE 4: Onboarding Endpoints ---"

# Check onboarding signup page
SIGNUP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$APP_URL/Onboarding/Signup" 2>/dev/null || echo "000")
check "Onboarding signup page" "$([ "$SIGNUP_STATUS" = "200" ] && echo true || echo false)"

# Check trial registration page
TRIAL_PAGE=$(curl -s -o /dev/null -w "%{http_code}" "$APP_URL/grc-free-trial" 2>/dev/null || echo "000")
check "Trial registration page" "$([ "$TRIAL_PAGE" = "200" ] || [ "$TRIAL_PAGE" = "302" ] && echo true || echo false)"

# =============================================================================
# PHASE 5: Static Assets
# =============================================================================
echo ""
echo "--- PHASE 5: Static Assets ---"

# Check favicon
FAVICON=$(curl -s -o /dev/null -w "%{http_code}" "$LANDING_URL/favicon.ico" 2>/dev/null || echo "000")
check "Favicon accessible" "$([ "$FAVICON" = "200" ] && echo true || echo false)"

# =============================================================================
# Summary
# =============================================================================
echo ""
echo "=========================================="
echo "  Verification Summary"
echo "=========================================="
echo -e "  ${GREEN}Passed: $PASS${NC}"
echo -e "  ${RED}Failed: $FAIL${NC}"
echo "=========================================="

if [ "$FAIL" -gt "0" ]; then
    echo ""
    echo -e "${YELLOW}Some checks failed. Please review and fix issues.${NC}"
    exit 1
else
    echo ""
    echo -e "${GREEN}All checks passed! Production deployment verified.${NC}"
    exit 0
fi
