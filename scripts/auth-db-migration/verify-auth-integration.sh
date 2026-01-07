#!/bin/bash
# Auth DB Integration Verification Script
# Run this after cutover to verify all integrations work

set -e

DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5433}"
APP_URL="${APP_URL:-http://localhost:5000}"

echo "=========================================="
echo "Auth DB Integration Verification"
echo "=========================================="
echo "DB: $DB_HOST:$DB_PORT"
echo "App: $APP_URL"
echo "=========================================="
echo ""

PASS=0
FAIL=0

check() {
    local name="$1"
    local result="$2"
    if [ "$result" = "PASS" ]; then
        echo "✅ $name"
        ((PASS++))
    else
        echo "❌ $name - $result"
        ((FAIL++))
    fi
}

# 1. Database Checks
echo "=== Database Checks ==="

# Check GrcAuthDb exists
if docker exec grc-db psql -U postgres -lqt 2>/dev/null | grep -q GrcAuthDb; then
    check "GrcAuthDb exists" "PASS"
else
    check "GrcAuthDb exists" "Database not found"
fi

# Check Identity tables exist
TABLE_COUNT=$(docker exec grc-db psql -U postgres -d GrcAuthDb -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='public' AND table_name LIKE 'AspNet%';" 2>/dev/null | xargs || echo "0")
if [ "$TABLE_COUNT" -ge 6 ]; then
    check "Identity tables ($TABLE_COUNT)" "PASS"
else
    check "Identity tables" "Only $TABLE_COUNT tables found"
fi

# Check users exist
USER_COUNT=$(docker exec grc-db psql -U postgres -d GrcAuthDb -t -c "SELECT COUNT(*) FROM \"AspNetUsers\";" 2>/dev/null | xargs || echo "0")
if [ "$USER_COUNT" -gt 0 ]; then
    check "Users migrated ($USER_COUNT)" "PASS"
else
    check "Users migrated" "No users found - run migration"
fi

# Check roles exist
ROLE_COUNT=$(docker exec grc-db psql -U postgres -d GrcAuthDb -t -c "SELECT COUNT(*) FROM \"AspNetRoles\";" 2>/dev/null | xargs || echo "0")
if [ "$ROLE_COUNT" -gt 0 ]; then
    check "Roles exist ($ROLE_COUNT)" "PASS"
else
    check "Roles exist" "No roles found"
fi

# Check user-role assignments
UR_COUNT=$(docker exec grc-db psql -U postgres -d GrcAuthDb -t -c "SELECT COUNT(*) FROM \"AspNetUserRoles\";" 2>/dev/null | xargs || echo "0")
if [ "$UR_COUNT" -gt 0 ]; then
    check "User-Role assignments ($UR_COUNT)" "PASS"
else
    check "User-Role assignments" "No assignments found"
fi

echo ""
echo "=== API Checks ==="

# Check health endpoint
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" "$APP_URL/health" 2>/dev/null || echo "000")
if [ "$HTTP_CODE" = "200" ]; then
    check "Health endpoint" "PASS"
else
    check "Health endpoint" "HTTP $HTTP_CODE"
fi

# Check login endpoint responds
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$APP_URL/api/auth/login" -H "Content-Type: application/json" -d '{}' 2>/dev/null || echo "000")
if [ "$HTTP_CODE" = "400" ] || [ "$HTTP_CODE" = "401" ]; then
    check "Login endpoint responds" "PASS"
else
    check "Login endpoint responds" "HTTP $HTTP_CODE"
fi

# Check protected endpoint requires auth
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" "$APP_URL/api/tenants" 2>/dev/null || echo "000")
if [ "$HTTP_CODE" = "401" ]; then
    check "Protected endpoints require auth" "PASS"
else
    check "Protected endpoints require auth" "HTTP $HTTP_CODE (expected 401)"
fi

echo ""
echo "=== UI Route Checks ==="

# Check homepage
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" "$APP_URL/" 2>/dev/null || echo "000")
if [ "$HTTP_CODE" = "200" ] || [ "$HTTP_CODE" = "302" ]; then
    check "Homepage accessible" "PASS"
else
    check "Homepage accessible" "HTTP $HTTP_CODE"
fi

# Check login page
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" "$APP_URL/Account/Login" 2>/dev/null || echo "000")
if [ "$HTTP_CODE" = "200" ]; then
    check "Login page accessible" "PASS"
else
    check "Login page accessible" "HTTP $HTTP_CODE"
fi

echo ""
echo "=== Cross-DB Reference Checks ==="

# Check no orphaned TenantUsers
ORPHAN_COUNT=$(docker exec grc-db psql -U postgres -d GrcMvcDb -t -c "
SELECT COUNT(*) FROM \"TenantUsers\" tu
WHERE NOT EXISTS (
    SELECT 1 FROM dblink('dbname=GrcAuthDb', 'SELECT \"Id\" FROM \"AspNetUsers\"') AS auth(id text)
    WHERE auth.id = tu.\"UserId\"
);" 2>/dev/null | xargs || echo "N/A")
if [ "$ORPHAN_COUNT" = "0" ]; then
    check "No orphaned TenantUsers" "PASS"
elif [ "$ORPHAN_COUNT" = "N/A" ]; then
    check "No orphaned TenantUsers" "Could not check (dblink not available)"
else
    check "No orphaned TenantUsers" "$ORPHAN_COUNT orphaned records"
fi

echo ""
echo "=========================================="
echo "Results: $PASS passed, $FAIL failed"
echo "=========================================="

if [ "$FAIL" -gt 0 ]; then
    echo ""
    echo "⚠️  Some checks failed. Review the issues above."
    echo "See AUTH_DB_SPLIT_FLAGS.md for troubleshooting."
    exit 1
else
    echo ""
    echo "✅ All checks passed! Auth DB integration verified."
    exit 0
fi
