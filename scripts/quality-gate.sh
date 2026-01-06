#!/bin/bash
# ============================================================
# Shahin GRC Quality Gate Script
# CI/CD integration for automated quality checks
# ============================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
MAGENTA='\033[0;35m'
NC='\033[0m'

# Default settings
FAIL_ON_WARNING=${FAIL_ON_WARNING:-false}
GENERATE_REPORT=${GENERATE_REPORT:-true}
REPORT_DIR="${PROJECT_ROOT}/quality-reports"

usage() {
    echo "Usage: $0 [OPTIONS] [COMMAND]"
    echo ""
    echo "Commands:"
    echo "  all         Run all quality checks (default)"
    echo "  build       Run build validation only"
    echo "  content     Run content validation only"
    echo "  quick       Quick validation (no build)"
    echo "  docker      Run checks in Docker container"
    echo "  ci          CI mode - strict validation"
    echo ""
    echo "Options:"
    echo "  --strict    Fail on warnings"
    echo "  --no-report Skip report generation"
    echo "  --help      Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0                    # Run all checks"
    echo "  $0 build              # Build validation only"
    echo "  $0 ci --strict        # CI mode with strict checking"
    echo "  $0 docker             # Run in Docker"
}

generate_report() {
    local status=$1
    local timestamp=$(date '+%Y%m%d_%H%M%S')

    mkdir -p "$REPORT_DIR"

    local report_file="$REPORT_DIR/quality-report-${timestamp}.json"

    # Count statistics
    local controllers=$(find "$PROJECT_ROOT/src/GrcMvc/Controllers" -name "*.cs" 2>/dev/null | wc -l)
    local views=$(find "$PROJECT_ROOT/src/GrcMvc/Views" -name "*.cshtml" 2>/dev/null | wc -l)
    local migrations=$(find "$PROJECT_ROOT/src/GrcMvc/Migrations" -name "*.cs" 2>/dev/null | wc -l)

    cat > "$report_file" << EOF
{
    "timestamp": "$(date -Iseconds)",
    "status": "$status",
    "commit": "$(git rev-parse HEAD 2>/dev/null || echo 'unknown')",
    "branch": "$(git rev-parse --abbrev-ref HEAD 2>/dev/null || echo 'unknown')",
    "statistics": {
        "controllers": $controllers,
        "views": $views,
        "migrations": $migrations
    },
    "modules": {
        "shahin_smart": $([ -f "$PROJECT_ROOT/src/GrcMvc/Controllers/OnboardingController.cs" ] && echo "true" || echo "false"),
        "shahin_map": $([ -f "$PROJECT_ROOT/src/GrcMvc/Controllers/ControlsController.cs" ] && echo "true" || echo "false"),
        "shahin_prove": $([ -f "$PROJECT_ROOT/src/GrcMvc/Controllers/EvidenceController.cs" ] && echo "true" || echo "false"),
        "shahin_watch": $([ -f "$PROJECT_ROOT/src/GrcMvc/Controllers/RiskIndicatorsController.cs" ] && echo "true" || echo "false"),
        "shahin_fix": $([ -f "$PROJECT_ROOT/src/GrcMvc/Controllers/ExceptionsController.cs" ] && echo "true" || echo "false"),
        "shahin_vault": $([ -f "$PROJECT_ROOT/src/GrcMvc/Controllers/VaultController.cs" ] && echo "true" || echo "false"),
        "shahin_flow": $([ -f "$PROJECT_ROOT/src/GrcMvc/Controllers/WorkflowUIController.cs" ] && echo "true" || echo "false"),
        "shahin_ai": $([ -f "$PROJECT_ROOT/src/GrcMvc/Controllers/ShahinAIIntegrationController.cs" ] && echo "true" || echo "false")
    }
}
EOF

    echo -e "${CYAN}Report saved: $report_file${NC}"
}

run_all() {
    echo -e "${MAGENTA}╔════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${MAGENTA}║         SHAHIN GRC QUALITY GATE                            ║${NC}"
    echo -e "${MAGENTA}╚════════════════════════════════════════════════════════════╝${NC}"
    echo ""

    cd "$PROJECT_ROOT"

    local exit_code=0

    # Run validation
    if [ -f "$SCRIPT_DIR/validate-all.sh" ]; then
        "$SCRIPT_DIR/validate-all.sh" || exit_code=$?
    else
        "$SCRIPT_DIR/validate-release.sh" || exit_code=$?
        "$SCRIPT_DIR/validate-content.sh" || exit_code=$?
    fi

    # Generate report
    if [ "$GENERATE_REPORT" = "true" ]; then
        if [ $exit_code -eq 0 ]; then
            generate_report "PASSED"
        else
            generate_report "FAILED"
        fi
    fi

    return $exit_code
}

run_build() {
    cd "$PROJECT_ROOT"
    "$SCRIPT_DIR/validate-release.sh"
}

run_content() {
    cd "$PROJECT_ROOT"
    "$SCRIPT_DIR/validate-content.sh"
}

run_quick() {
    echo -e "${CYAN}Quick Validation (no build)${NC}"
    echo ""

    cd "$PROJECT_ROOT"

    # Check critical files exist
    echo "Checking critical files..."

    local errors=0

    # Check controllers
    for controller in OnboardingController ControlsController EvidenceController WorkflowUIController; do
        if [ ! -f "src/GrcMvc/Controllers/${controller}.cs" ]; then
            echo -e "  ${RED}MISSING: ${controller}.cs${NC}"
            errors=$((errors + 1))
        fi
    done

    # Check views
    if [ ! -f "src/GrcMvc/Views/Shared/_Layout.cshtml" ]; then
        echo -e "  ${RED}MISSING: _Layout.cshtml${NC}"
        errors=$((errors + 1))
    fi

    if [ $errors -eq 0 ]; then
        echo -e "${GREEN}Quick validation passed${NC}"
        return 0
    else
        echo -e "${RED}Quick validation failed: $errors error(s)${NC}"
        return 1
    fi
}

run_docker() {
    echo -e "${CYAN}Running quality checks in Docker...${NC}"
    cd "$PROJECT_ROOT"
    docker-compose -f docker-compose.quality.yml up --build quality-check
}

run_ci() {
    echo -e "${MAGENTA}╔════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${MAGENTA}║         CI/CD QUALITY GATE                                 ║${NC}"
    echo -e "${MAGENTA}╚════════════════════════════════════════════════════════════╝${NC}"
    echo ""
    echo "Mode: CI (strict)"
    echo "Fail on warnings: $FAIL_ON_WARNING"
    echo ""

    GENERATE_REPORT=true
    run_all
    local result=$?

    if [ $result -ne 0 ]; then
        echo ""
        echo -e "${RED}╔════════════════════════════════════════════════════════════╗${NC}"
        echo -e "${RED}║  CI QUALITY GATE: FAILED                                   ║${NC}"
        echo -e "${RED}║  Fix errors before merging                                 ║${NC}"
        echo -e "${RED}╚════════════════════════════════════════════════════════════╝${NC}"
        exit 1
    fi

    echo ""
    echo -e "${GREEN}╔════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${GREEN}║  CI QUALITY GATE: PASSED                                   ║${NC}"
    echo -e "${GREEN}║  Ready to merge                                            ║${NC}"
    echo -e "${GREEN}╚════════════════════════════════════════════════════════════╝${NC}"
}

# Parse options
while [[ $# -gt 0 ]]; do
    case $1 in
        --strict)
            FAIL_ON_WARNING=true
            shift
            ;;
        --no-report)
            GENERATE_REPORT=false
            shift
            ;;
        --help|-h)
            usage
            exit 0
            ;;
        all|build|content|quick|docker|ci)
            COMMAND=$1
            shift
            ;;
        *)
            echo "Unknown option: $1"
            usage
            exit 1
            ;;
    esac
done

# Default command
COMMAND=${COMMAND:-all}

# Execute command
case $COMMAND in
    all)
        run_all
        ;;
    build)
        run_build
        ;;
    content)
        run_content
        ;;
    quick)
        run_quick
        ;;
    docker)
        run_docker
        ;;
    ci)
        run_ci
        ;;
esac
