#!/bin/bash
# ============================================================
# Shahin GRC Content Validation Script
# Validates compliance content, frameworks, and templates
# ============================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
MVC_PATH="$PROJECT_ROOT/src/GrcMvc"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

ERRORS=0
WARNINGS=0

echo "============================================================"
echo "  SHAHIN GRC CONTENT VALIDATION"
echo "  Date: $(date '+%Y-%m-%d %H:%M:%S')"
echo "============================================================"
echo ""

# ============================================================
# 1. REGULATORY FRAMEWORKS
# ============================================================
echo -e "${BLUE}[1/6] REGULATORY FRAMEWORKS${NC}"
echo "------------------------------------------------------------"

# Check for framework definitions in Models/Entities
FRAMEWORKS_PATH="$MVC_PATH/Models/Entities"
FRAMEWORK_FILES=(
    "Framework.cs"
    "Control.cs"
    "CanonicalControl.cs"
    "ControlMapping.cs"
)

echo "  Required Framework Entities:"
for file in "${FRAMEWORK_FILES[@]}"; do
    if [ -f "$FRAMEWORKS_PATH/$file" ]; then
        echo -e "    ${GREEN}OK: $file${NC}"
    else
        echo -e "    ${YELLOW}WARN: $file not found${NC}"
        WARNINGS=$((WARNINGS + 1))
    fi
done

# Check framework seed data
SEED_PATH="$MVC_PATH/Data/Seeds"
if [ -d "$SEED_PATH" ]; then
    SEED_COUNT=$(find "$SEED_PATH" -name "*.json" -o -name "*.cs" | wc -l)
    echo -e "  Seed Data Files: ${GREEN}$SEED_COUNT found${NC}"
else
    echo -e "  ${YELLOW}WARN: No seed data folder${NC}"
fi
echo ""

# ============================================================
# 2. SHAHIN MODULE COVERAGE
# ============================================================
echo -e "${BLUE}[2/6] SHAHIN MODULE COVERAGE${NC}"
echo "------------------------------------------------------------"

declare -A MODULES
MODULES=(
    ["Shahin-SMART"]="OnboardingController.cs"
    ["Shahin-MAP"]="ControlsController.cs"
    ["Shahin-PROVE"]="EvidenceController.cs"
    ["Shahin-WATCH"]="RiskIndicatorsController.cs"
    ["Shahin-FIX"]="ExceptionsController.cs"
    ["Shahin-VAULT"]="VaultController.cs"
    ["Shahin-FLOW"]="WorkflowUIController.cs"
    ["Shahin-AI"]="ShahinAIIntegrationController.cs"
)

CONTROLLER_PATH="$MVC_PATH/Controllers"
MODULE_MISSING=0

for module in "${!MODULES[@]}"; do
    controller="${MODULES[$module]}"
    if [ -f "$CONTROLLER_PATH/$controller" ]; then
        # Count action methods
        actions=$(grep -c "public.*IActionResult\|public.*async.*Task<IActionResult>" "$CONTROLLER_PATH/$controller" 2>/dev/null || echo "0")
        echo -e "  ${GREEN}OK: $module${NC} ($controller - $actions actions)"
    else
        echo -e "  ${RED}MISSING: $module${NC} ($controller)"
        MODULE_MISSING=$((MODULE_MISSING + 1))
    fi
done

if [ "$MODULE_MISSING" -gt 0 ]; then
    ERRORS=$((ERRORS + MODULE_MISSING))
fi
echo ""

# ============================================================
# 3. STAKEHOLDER DASHBOARDS
# ============================================================
echo -e "${BLUE}[3/6] STAKEHOLDER DASHBOARDS${NC}"
echo "------------------------------------------------------------"

VIEW_PATH="$MVC_PATH/Views"
DASHBOARDS=(
    "Dashboard"
    "ExecutiveDashboard"
    "ComplianceOfficerDashboard"
    "RiskManagerDashboard"
    "AuditorDashboard"
)

DASHBOARD_MISSING=0
for dashboard in "${DASHBOARDS[@]}"; do
    if [ -d "$VIEW_PATH/$dashboard" ] || [ -f "$VIEW_PATH/Dashboard/$dashboard.cshtml" ]; then
        echo -e "  ${GREEN}OK: $dashboard${NC}"
    else
        # Check if it's a view in Dashboard folder
        if [ -f "$VIEW_PATH/Dashboard/Index.cshtml" ] && [ "$dashboard" == "Dashboard" ]; then
            echo -e "  ${GREEN}OK: $dashboard${NC}"
        else
            echo -e "  ${YELLOW}OPTIONAL: $dashboard not found${NC}"
            WARNINGS=$((WARNINGS + 1))
        fi
    fi
done
echo ""

# ============================================================
# 4. API ENDPOINTS
# ============================================================
echo -e "${BLUE}[4/6] API ENDPOINTS${NC}"
echo "------------------------------------------------------------"

# Check for API controllers
API_CONTROLLERS=$(find "$CONTROLLER_PATH" -name "*Api*.cs" -o -name "*Controller.cs" | wc -l)
echo "  Total Controllers: $API_CONTROLLERS"

# Count total API endpoints
TOTAL_ENDPOINTS=0
for controller in "$CONTROLLER_PATH"/*.cs; do
    if [ -f "$controller" ]; then
        endpoints=$(grep -c "\[Http" "$controller" 2>/dev/null || true)
        if [ -n "$endpoints" ] && [ "$endpoints" -gt 0 ] 2>/dev/null; then
            TOTAL_ENDPOINTS=$((TOTAL_ENDPOINTS + endpoints))
        fi
    fi
done
echo -e "  ${GREEN}API Endpoints: $TOTAL_ENDPOINTS${NC}"

# Check for Swagger/OpenAPI
if grep -rq "Swagger\|OpenApi" "$MVC_PATH/Program.cs" 2>/dev/null; then
    echo -e "  ${GREEN}OK: Swagger/OpenAPI configured${NC}"
else
    echo -e "  ${YELLOW}WARN: Swagger not configured${NC}"
    WARNINGS=$((WARNINGS + 1))
fi
echo ""

# ============================================================
# 5. LOCALIZATION
# ============================================================
echo -e "${BLUE}[5/6] LOCALIZATION${NC}"
echo "------------------------------------------------------------"

RESOURCES_PATH="$MVC_PATH/Resources"
if [ -d "$RESOURCES_PATH" ]; then
    EN_COUNT=$(find "$RESOURCES_PATH" -name "*.en.resx" -o -name "*Resources.resx" | wc -l)
    AR_COUNT=$(find "$RESOURCES_PATH" -name "*.ar.resx" -o -name "*ar-SA.resx" | wc -l)
    echo -e "  English Resources: ${GREEN}$EN_COUNT files${NC}"
    echo -e "  Arabic Resources: ${GREEN}$AR_COUNT files${NC}"
else
    echo -e "  ${YELLOW}WARN: No Resources folder found${NC}"
    WARNINGS=$((WARNINGS + 1))
fi

# Check for RTL support in views
RTL_SUPPORT=$(grep -r "dir=\"rtl\"\|direction: rtl" "$VIEW_PATH" 2>/dev/null | wc -l || echo "0")
if [ "$RTL_SUPPORT" -gt 0 ]; then
    echo -e "  RTL Support: ${GREEN}Found in $RTL_SUPPORT locations${NC}"
else
    echo -e "  ${YELLOW}WARN: RTL support not detected${NC}"
    WARNINGS=$((WARNINGS + 1))
fi
echo ""

# ============================================================
# 6. TEMPLATE FILES
# ============================================================
echo -e "${BLUE}[6/6] TEMPLATES & DOCUMENTS${NC}"
echo "------------------------------------------------------------"

TEMPLATE_PATHS=(
    "$MVC_PATH/wwwroot/templates"
    "$MVC_PATH/Templates"
    "$PROJECT_ROOT/etc/templates"
)

TEMPLATE_FOUND=0
for tpath in "${TEMPLATE_PATHS[@]}"; do
    if [ -d "$tpath" ]; then
        count=$(find "$tpath" -type f | wc -l)
        echo -e "  ${GREEN}OK: $tpath ($count files)${NC}"
        TEMPLATE_FOUND=$((TEMPLATE_FOUND + count))
    fi
done

if [ "$TEMPLATE_FOUND" -eq 0 ]; then
    echo -e "  ${YELLOW}WARN: No template directories found${NC}"
    WARNINGS=$((WARNINGS + 1))
fi

# Check for policy files
POLICY_PATH="$PROJECT_ROOT/etc/policies"
if [ -d "$POLICY_PATH" ]; then
    policy_count=$(find "$POLICY_PATH" -name "*.yml" -o -name "*.yaml" -o -name "*.json" | wc -l)
    echo -e "  Policy Rules: ${GREEN}$policy_count files${NC}"
else
    echo -e "  ${YELLOW}WARN: No policy rules directory${NC}"
    WARNINGS=$((WARNINGS + 1))
fi
echo ""

# ============================================================
# SUMMARY
# ============================================================
echo "============================================================"
echo "  CONTENT VALIDATION SUMMARY"
echo "============================================================"
echo ""
echo -e "  Total Errors:   ${RED}$ERRORS${NC}"
echo -e "  Total Warnings: ${YELLOW}$WARNINGS${NC}"
echo ""

if [ "$ERRORS" -eq 0 ]; then
    if [ "$WARNINGS" -gt 0 ]; then
        echo -e "  ${YELLOW}========================================${NC}"
        echo -e "  ${YELLOW}  CONTENT VALIDATION: PASSED (with warnings)${NC}"
        echo -e "  ${YELLOW}========================================${NC}"
    else
        echo -e "  ${GREEN}========================================${NC}"
        echo -e "  ${GREEN}  CONTENT VALIDATION: PASSED${NC}"
        echo -e "  ${GREEN}========================================${NC}"
    fi
    exit 0
else
    echo -e "  ${RED}========================================${NC}"
    echo -e "  ${RED}  CONTENT VALIDATION: FAILED${NC}"
    echo -e "  ${RED}========================================${NC}"
    exit 1
fi
