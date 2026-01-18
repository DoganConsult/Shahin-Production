#!/bin/bash
set -euo pipefail

# Gradual Deployment Script
# Usage: ./scripts/deploy-gradual.sh <canary|pilot|partial|full> <version-tag>

if [ $# -ne 2 ]; then
    echo "Usage: $0 <canary|pilot|partial|full> <version-tag>"
    exit 1
fi

STAGE=$1
VERSION=$2
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OPS_DIR="${SCRIPT_DIR}/../ops/gradual"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Validate stage
case $STAGE in
    canary|pilot|partial|full) ;;
    *)
        echo -e "${RED}Invalid stage: $STAGE${NC}"
        echo "Valid stages: canary, pilot, partial, full"
        exit 1
        ;;
esac

echo "================================================"
echo -e "${BLUE}GRADUAL DEPLOYMENT - ${STAGE^^}${NC}"
echo "================================================"
echo "Stage: $STAGE"
echo "Version: $VERSION"
echo "Timestamp: $TIMESTAMP"
echo "------------------------------------------------"

# Load environment configuration
ENV_FILE="${OPS_DIR}/targets.env"
if [ -f "$ENV_FILE" ]; then
    echo "Loading environment from $ENV_FILE"
    source "$ENV_FILE"
else
    echo -e "${YELLOW}Warning: $ENV_FILE not found, using defaults${NC}"
    CANARY_BASE_URL="http://localhost:5000"
    PILOT_BASE_URL="http://localhost:5001"
    PARTIAL_BASE_URL="http://localhost:5002"
    FULL_BASE_URL="http://localhost:5003"
fi

# Determine base URL for stage
case $STAGE in
    canary) BASE_URL="${CANARY_BASE_URL:-http://localhost:5000}" ;;
    pilot) BASE_URL="${PILOT_BASE_URL:-http://localhost:5001}" ;;
    partial) BASE_URL="${PARTIAL_BASE_URL:-http://localhost:5002}" ;;
    full) BASE_URL="${FULL_BASE_URL:-http://localhost:5003}" ;;
esac

echo "Target URL: $BASE_URL"

# Create deployment log directory
DEPLOY_DIR="${OPS_DIR}/deployments"
mkdir -p "$DEPLOY_DIR"
LOG_FILE="${DEPLOY_DIR}/${TIMESTAMP}_${STAGE}_${VERSION}.log"

# Start logging
{
    echo "DEPLOYMENT LOG"
    echo "=============="
    echo "Date: $(date)"
    echo "Stage: $STAGE"
    echo "Version: $VERSION"
    echo "Base URL: $BASE_URL"
    echo "User: $(whoami)"
    echo ""
} > "$LOG_FILE"

# Run validation
echo ""
echo -e "${BLUE}Step 1: Pre-deployment Validation${NC}"
echo "------------------------------------------------"
if "${SCRIPT_DIR}/validate-phase1.sh" --base-url "$BASE_URL"; then
    echo -e "${GREEN}✓ Validation passed${NC}" | tee -a "$LOG_FILE"
else
    echo -e "${RED}✗ Validation failed - deployment aborted${NC}" | tee -a "$LOG_FILE"
    echo "See validation output above for details"
    exit 1
fi

# Load and display feature flags
echo ""
echo -e "${BLUE}Step 2: Feature Flags Configuration${NC}"
echo "------------------------------------------------"
FLAGS_FILE="${OPS_DIR}/feature-flags.json"
if [ -f "${OPS_DIR}/feature-flags.example.json" ] && [ ! -f "$FLAGS_FILE" ]; then
    cp "${OPS_DIR}/feature-flags.example.json" "$FLAGS_FILE"
fi

if [ -f "$FLAGS_FILE" ]; then
    echo "Current feature flags for stage '$STAGE':"
    case $STAGE in
        canary)
            echo "  ✅ foundation_security: true"
            echo "  ⬜ core_risk_control_assessment: false"
            echo "  ⬜ core_audit_policy_evidence_vendor: false"
            echo "  ⬜ workflow_automation: false"
            echo "  ⬜ integrations: false"
            echo "  ⬜ ai_analytics: false"
            ;;
        pilot)
            echo "  ✅ foundation_security: true"
            echo "  ✅ core_risk_control_assessment: true"
            echo "  ⬜ core_audit_policy_evidence_vendor: false"
            echo "  ⬜ workflow_automation: false"
            echo "  ⬜ integrations: false"
            echo "  ⬜ ai_analytics: false"
            ;;
        partial)
            echo "  ✅ foundation_security: true"
            echo "  ✅ core_risk_control_assessment: true"
            echo "  ✅ core_audit_policy_evidence_vendor: true"
            echo "  ✅ workflow_automation: true"
            echo "  ⬜ integrations: false"
            echo "  ⬜ ai_analytics: false"
            ;;
        full)
            echo "  ✅ foundation_security: true"
            echo "  ✅ core_risk_control_assessment: true"
            echo "  ✅ core_audit_policy_evidence_vendor: true"
            echo "  ✅ workflow_automation: true"
            echo "  ✅ integrations: true"
            echo "  ✅ ai_analytics: true"
            ;;
    esac
else
    echo -e "${YELLOW}Warning: Feature flags not configured${NC}"
fi

# Deployment steps (placeholder for now)
echo ""
echo -e "${BLUE}Step 3: Deployment Actions${NC}"
echo "------------------------------------------------"
echo "📋 MANUAL DEPLOYMENT STEPS REQUIRED:"
echo ""
echo "1. Build Docker image:"
echo "   docker build -t shahin-grc:$VERSION ."
echo ""
echo "2. Tag and push to registry:"
echo "   docker tag shahin-grc:$VERSION registry.shahin-grc.com/grc:$VERSION"
echo "   docker push registry.shahin-grc.com/grc:$VERSION"
echo ""
echo "3. Update deployment configuration for $STAGE:"
case $STAGE in
    canary)
        echo "   - Update canary environment (5% traffic)"
        echo "   - Target: $CANARY_BASE_URL"
        echo "   - Max users: 50"
        ;;
    pilot)
        echo "   - Update pilot environment (25% traffic)"
        echo "   - Target: $PILOT_BASE_URL"
        echo "   - Max users: 250"
        ;;
    partial)
        echo "   - Update partial environment (50% traffic)"
        echo "   - Target: $PARTIAL_BASE_URL"
        echo "   - Max users: 500"
        ;;
    full)
        echo "   - Update full production (100% traffic)"
        echo "   - Target: $FULL_BASE_URL"
        echo "   - All users"
        ;;
esac
echo ""
echo "4. Apply configuration:"
echo "   kubectl set image deployment/grc-$STAGE grc=registry.shahin-grc.com/grc:$VERSION"
echo "   kubectl rollout status deployment/grc-$STAGE"
echo ""
echo "5. Run post-deployment validation:"
echo "   ${SCRIPT_DIR}/validate-phase1.sh --base-url $BASE_URL"
echo ""
echo "6. Start monitoring:"
echo "   ${SCRIPT_DIR}/monitor-deployment.sh $STAGE"

# Log deployment record
{
    echo ""
    echo "DEPLOYMENT INITIATED"
    echo "===================="
    echo "Status: PENDING_MANUAL_STEPS"
    echo "Next: Execute manual steps above"
    echo "Monitor: ${SCRIPT_DIR}/monitor-deployment.sh $STAGE"
    echo "Rollback: ${SCRIPT_DIR}/rollback-deployment.sh $STAGE \"reason\""
} >> "$LOG_FILE"

echo ""
echo "================================================"
echo -e "${GREEN}Deployment record created: $LOG_FILE${NC}"
echo -e "${YELLOW}⚠️  Execute manual steps above to complete deployment${NC}"
echo "================================================"
