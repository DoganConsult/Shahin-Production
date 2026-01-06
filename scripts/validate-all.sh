#!/bin/bash
# ============================================================
# Shahin GRC MASTER VALIDATION SCRIPT
# Comprehensive release validation for new versions
# ============================================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
MAGENTA='\033[0;35m'
NC='\033[0m'

echo ""
echo -e "${MAGENTA}╔════════════════════════════════════════════════════════════╗${NC}"
echo -e "${MAGENTA}║       SHAHIN GRC - COMPREHENSIVE RELEASE VALIDATION        ║${NC}"
echo -e "${MAGENTA}║                     Version Checker                        ║${NC}"
echo -e "${MAGENTA}╚════════════════════════════════════════════════════════════╝${NC}"
echo ""
echo "  Date: $(date '+%Y-%m-%d %H:%M:%S')"
echo ""

TOTAL_ERRORS=0
TOTAL_WARNINGS=0

# ============================================================
# PHASE 1: BUILD & STRUCTURE VALIDATION
# ============================================================
echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${CYAN}  PHASE 1: BUILD & STRUCTURE VALIDATION${NC}"
echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo ""

if [ -f "$SCRIPT_DIR/validate-release.sh" ]; then
    "$SCRIPT_DIR/validate-release.sh"
    RESULT=$?
    if [ $RESULT -ne 0 ]; then
        TOTAL_ERRORS=$((TOTAL_ERRORS + 1))
    fi
else
    echo -e "${RED}ERROR: validate-release.sh not found${NC}"
    TOTAL_ERRORS=$((TOTAL_ERRORS + 1))
fi

echo ""

# ============================================================
# PHASE 2: CONTENT & MODULE VALIDATION
# ============================================================
echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${CYAN}  PHASE 2: CONTENT & MODULE VALIDATION${NC}"
echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo ""

if [ -f "$SCRIPT_DIR/validate-content.sh" ]; then
    "$SCRIPT_DIR/validate-content.sh"
    RESULT=$?
    if [ $RESULT -ne 0 ]; then
        TOTAL_ERRORS=$((TOTAL_ERRORS + 1))
    fi
else
    echo -e "${RED}ERROR: validate-content.sh not found${NC}"
    TOTAL_ERRORS=$((TOTAL_ERRORS + 1))
fi

echo ""

# ============================================================
# FINAL SUMMARY
# ============================================================
echo -e "${MAGENTA}╔════════════════════════════════════════════════════════════╗${NC}"
echo -e "${MAGENTA}║              FINAL VALIDATION REPORT                       ║${NC}"
echo -e "${MAGENTA}╚════════════════════════════════════════════════════════════╝${NC}"
echo ""

if [ $TOTAL_ERRORS -eq 0 ]; then
    echo -e "  ${GREEN}┌─────────────────────────────────────────┐${NC}"
    echo -e "  ${GREEN}│  ✓ ALL VALIDATIONS PASSED               │${NC}"
    echo -e "  ${GREEN}│    Release is ready for deployment      │${NC}"
    echo -e "  ${GREEN}└─────────────────────────────────────────┘${NC}"
    echo ""
    exit 0
else
    echo -e "  ${RED}┌─────────────────────────────────────────┐${NC}"
    echo -e "  ${RED}│  ✗ VALIDATION FAILED                    │${NC}"
    echo -e "  ${RED}│    $TOTAL_ERRORS phase(s) have issues              │${NC}"
    echo -e "  ${RED}│    Review errors above before release   │${NC}"
    echo -e "  ${RED}└─────────────────────────────────────────┘${NC}"
    echo ""
    exit 1
fi
