#!/bin/bash
# ============================================================
# Shahin GRC Release Validation Script
# Validates completeness and compliance of new releases
# ============================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
MVC_PATH="$PROJECT_ROOT/src/GrcMvc"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

ERRORS=0
WARNINGS=0

echo "============================================================"
echo "  SHAHIN GRC RELEASE VALIDATION"
echo "  Date: $(date '+%Y-%m-%d %H:%M:%S')"
echo "============================================================"
echo ""

# ============================================================
# 1. BUILD VALIDATION
# ============================================================
echo -e "${BLUE}[1/7] BUILD VALIDATION${NC}"
echo "------------------------------------------------------------"

cd "$MVC_PATH"

# Clean and build
echo "  Cleaning project..."
dotnet clean GrcMvc.csproj --verbosity quiet 2>/dev/null || true

echo "  Building project..."
BUILD_OUTPUT=$(dotnet build GrcMvc.csproj 2>&1)
BUILD_ERRORS=$(echo "$BUILD_OUTPUT" | grep -c "error CS" || true)
BUILD_WARNINGS=$(echo "$BUILD_OUTPUT" | grep -c "warning CS" || true)

if [ "$BUILD_ERRORS" -gt 0 ]; then
    echo -e "  ${RED}FAIL: $BUILD_ERRORS build error(s) found${NC}"
    echo "$BUILD_OUTPUT" | grep "error CS"
    ERRORS=$((ERRORS + BUILD_ERRORS))
else
    echo -e "  ${GREEN}PASS: Build successful${NC}"
fi

if [ "$BUILD_WARNINGS" -gt 0 ]; then
    echo -e "  ${YELLOW}WARN: $BUILD_WARNINGS build warning(s)${NC}"
    WARNINGS=$((WARNINGS + BUILD_WARNINGS))
fi
echo ""

# ============================================================
# 2. CONTROLLER-VIEW MAPPING VALIDATION
# ============================================================
echo -e "${BLUE}[2/7] CONTROLLER-VIEW MAPPING${NC}"
echo "------------------------------------------------------------"

MISSING_VIEWS=0

# Get all controllers
for controller_file in "$MVC_PATH/Controllers"/*.cs; do
    controller_name=$(basename "$controller_file" .cs)

    # Skip API controllers and base classes
    if [[ "$controller_name" == *"Api"* ]] || [[ "$controller_name" == "BaseController" ]]; then
        continue
    fi

    # Extract view folder name (remove "Controller" suffix)
    view_folder="${controller_name%Controller}"
    view_path="$MVC_PATH/Views/$view_folder"

    # Get action methods that return View()
    actions=$(grep -oP '(?<=public\s+(IActionResult|ActionResult|async\s+Task<IActionResult>)\s+)\w+' "$controller_file" 2>/dev/null || true)

    for action in $actions; do
        # Skip common non-view actions
        if [[ "$action" == "Redirect"* ]] || [[ "$action" == "Json"* ]]; then
            continue
        fi

        # Check if view exists
        if [ ! -f "$view_path/$action.cshtml" ]; then
            echo -e "  ${RED}MISSING: $view_folder/$action.cshtml${NC}"
            MISSING_VIEWS=$((MISSING_VIEWS + 1))
        fi
    done
done

if [ "$MISSING_VIEWS" -eq 0 ]; then
    echo -e "  ${GREEN}PASS: All views present${NC}"
else
    echo -e "  ${RED}FAIL: $MISSING_VIEWS missing view(s)${NC}"
    ERRORS=$((ERRORS + MISSING_VIEWS))
fi
echo ""

# ============================================================
# 3. LAYOUT AND SHARED VIEWS VALIDATION
# ============================================================
echo -e "${BLUE}[3/7] LAYOUT & SHARED VIEWS${NC}"
echo "------------------------------------------------------------"

REQUIRED_SHARED=(
    "_Layout.cshtml"
    "Error.cshtml"
    "_ValidationScriptsPartial.cshtml"
)

REQUIRED_VIEWS_ROOT=(
    "_ViewStart.cshtml"
    "_ViewImports.cshtml"
)

SHARED_PATH="$MVC_PATH/Views/Shared"
VIEWS_PATH="$MVC_PATH/Views"
SHARED_MISSING=0

# Check Shared folder files
for shared in "${REQUIRED_SHARED[@]}"; do
    if [ -f "$SHARED_PATH/$shared" ]; then
        echo -e "  ${GREEN}OK: Shared/$shared${NC}"
    else
        echo -e "  ${RED}MISSING: Shared/$shared${NC}"
        SHARED_MISSING=$((SHARED_MISSING + 1))
    fi
done

# Check Views root files
for viewfile in "${REQUIRED_VIEWS_ROOT[@]}"; do
    if [ -f "$VIEWS_PATH/$viewfile" ]; then
        echo -e "  ${GREEN}OK: $viewfile${NC}"
    else
        echo -e "  ${RED}MISSING: $viewfile${NC}"
        SHARED_MISSING=$((SHARED_MISSING + 1))
    fi
done

if [ "$SHARED_MISSING" -gt 0 ]; then
    ERRORS=$((ERRORS + SHARED_MISSING))
fi
echo ""

# ============================================================
# 4. SERVICE REGISTRATION VALIDATION
# ============================================================
echo -e "${BLUE}[4/7] SERVICE REGISTRATION${NC}"
echo "------------------------------------------------------------"

# Check Program.cs for required services
PROGRAM_FILE="$MVC_PATH/Program.cs"
REQUIRED_SERVICES=(
    "AddDbContext"
    "AddControllersWithViews"
    "AddAuthentication"
)

SERVICE_MISSING=0
for service in "${REQUIRED_SERVICES[@]}"; do
    if grep -q "$service" "$PROGRAM_FILE" 2>/dev/null; then
        echo -e "  ${GREEN}OK: $service registered${NC}"
    else
        echo -e "  ${YELLOW}WARN: $service not found in Program.cs${NC}"
        SERVICE_MISSING=$((SERVICE_MISSING + 1))
    fi
done

WARNINGS=$((WARNINGS + SERVICE_MISSING))
echo ""

# ============================================================
# 5. STATIC ASSETS VALIDATION
# ============================================================
echo -e "${BLUE}[5/7] STATIC ASSETS${NC}"
echo "------------------------------------------------------------"

WWWROOT="$MVC_PATH/wwwroot"
REQUIRED_ASSETS=(
    "css"
    "js"
    "lib"
)

ASSET_MISSING=0
for asset in "${REQUIRED_ASSETS[@]}"; do
    if [ -d "$WWWROOT/$asset" ]; then
        count=$(find "$WWWROOT/$asset" -type f | wc -l)
        echo -e "  ${GREEN}OK: $asset/ ($count files)${NC}"
    else
        echo -e "  ${RED}MISSING: $asset/${NC}"
        ASSET_MISSING=$((ASSET_MISSING + 1))
    fi
done

if [ "$ASSET_MISSING" -gt 0 ]; then
    ERRORS=$((ERRORS + ASSET_MISSING))
fi
echo ""

# ============================================================
# 6. DATABASE MIGRATION VALIDATION
# ============================================================
echo -e "${BLUE}[6/7] DATABASE MIGRATIONS${NC}"
echo "------------------------------------------------------------"

MIGRATIONS_PATH="$MVC_PATH/Migrations"
if [ -d "$MIGRATIONS_PATH" ]; then
    MIGRATION_COUNT=$(find "$MIGRATIONS_PATH" -name "*.cs" -type f | wc -l)
    echo -e "  ${GREEN}OK: $MIGRATION_COUNT migration file(s) found${NC}"

    # Check for pending migrations (if database is available)
    # dotnet ef migrations list --no-build 2>/dev/null | grep -c "pending" || true
else
    echo -e "  ${YELLOW}WARN: No migrations folder found${NC}"
    WARNINGS=$((WARNINGS + 1))
fi
echo ""

# ============================================================
# 7. CONFIGURATION VALIDATION
# ============================================================
echo -e "${BLUE}[7/7] CONFIGURATION FILES${NC}"
echo "------------------------------------------------------------"

REQUIRED_CONFIG=(
    "appsettings.json"
    "appsettings.Development.json"
)

CONFIG_MISSING=0
for config in "${REQUIRED_CONFIG[@]}"; do
    if [ -f "$MVC_PATH/$config" ]; then
        echo -e "  ${GREEN}OK: $config${NC}"
    else
        echo -e "  ${YELLOW}WARN: $config not found${NC}"
        CONFIG_MISSING=$((CONFIG_MISSING + 1))
    fi
done

WARNINGS=$((WARNINGS + CONFIG_MISSING))
echo ""

# ============================================================
# SUMMARY
# ============================================================
echo "============================================================"
echo "  VALIDATION SUMMARY"
echo "============================================================"
echo ""
echo -e "  Total Errors:   ${RED}$ERRORS${NC}"
echo -e "  Total Warnings: ${YELLOW}$WARNINGS${NC}"
echo ""

if [ "$ERRORS" -eq 0 ]; then
    echo -e "  ${GREEN}========================================${NC}"
    echo -e "  ${GREEN}  RELEASE VALIDATION: PASSED${NC}"
    echo -e "  ${GREEN}========================================${NC}"
    exit 0
else
    echo -e "  ${RED}========================================${NC}"
    echo -e "  ${RED}  RELEASE VALIDATION: FAILED${NC}"
    echo -e "  ${RED}========================================${NC}"
    exit 1
fi
