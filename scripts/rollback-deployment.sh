#!/bin/bash
set -euo pipefail

# Rollback Deployment Script
# Usage: ./scripts/rollback-deployment.sh <canary|pilot|partial|full> "<reason>"

if [ $# -ne 2 ]; then
    echo "Usage: $0 <canary|pilot|partial|full> \"<reason>\""
    echo "Example: $0 canary \"High error rate detected\""
    exit 1
fi

STAGE=$1
REASON=$2
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
echo -e "${RED}ROLLBACK DEPLOYMENT - ${STAGE^^}${NC}"
echo "================================================"
echo "Stage: $STAGE"
echo "Reason: $REASON"
echo "Timestamp: $TIMESTAMP"
echo "Initiated by: $(whoami)"
echo "------------------------------------------------"

# Create rollback log directory
ROLLBACK_DIR="${OPS_DIR}/rollbacks"
mkdir -p "$ROLLBACK_DIR"
LOG_FILE="${ROLLBACK_DIR}/${TIMESTAMP}_${STAGE}.log"

# Start logging
{
    echo "ROLLBACK LOG"
    echo "============"
    echo "Date: $(date)"
    echo "Stage: $STAGE"
    echo "Reason: $REASON"
    echo "User: $(whoami)"
    echo ""
} > "$LOG_FILE"

# Find previous deployment version
DEPLOY_DIR="${OPS_DIR}/deployments"
echo ""
echo -e "${BLUE}Step 1: Finding Previous Version${NC}"
echo "------------------------------------------------"

if [ -d "$DEPLOY_DIR" ]; then
    # Get last two deployments for this stage
    DEPLOYMENTS=($(ls -1r "$DEPLOY_DIR" | grep "_${STAGE}_" | head -2))
    
    if [ ${#DEPLOYMENTS[@]} -ge 2 ]; then
        CURRENT_DEPLOY="${DEPLOYMENTS[0]}"
        PREVIOUS_DEPLOY="${DEPLOYMENTS[1]}"
        
        # Extract version from filename (format: timestamp_stage_version.log)
        CURRENT_VERSION=$(echo "$CURRENT_DEPLOY" | cut -d'_' -f3 | cut -d'.' -f1)
        PREVIOUS_VERSION=$(echo "$PREVIOUS_DEPLOY" | cut -d'_' -f3 | cut -d'.' -f1)
        
        echo "Current version: $CURRENT_VERSION"
        echo "Previous version: $PREVIOUS_VERSION"
        echo -e "${GREEN}✓ Previous version identified: $PREVIOUS_VERSION${NC}"
        
        echo "Current: $CURRENT_VERSION" >> "$LOG_FILE"
        echo "Previous: $PREVIOUS_VERSION" >> "$LOG_FILE"
    else
        echo -e "${YELLOW}⚠ Previous version not found in deployment history${NC}"
        echo "Manual version specification required"
        PREVIOUS_VERSION="unknown"
    fi
else
    echo -e "${YELLOW}⚠ No deployment history found${NC}"
    PREVIOUS_VERSION="unknown"
fi

# Rollback steps
echo ""
echo -e "${BLUE}Step 2: Rollback Actions${NC}"
echo "------------------------------------------------"
echo "📋 ROLLBACK STEPS TO EXECUTE:"
echo ""
echo "1. Stop current deployment:"
echo "   kubectl scale deployment/grc-$STAGE --replicas=0"
echo ""

if [ "$PREVIOUS_VERSION" != "unknown" ]; then
    echo "2. Rollback to previous version ($PREVIOUS_VERSION):"
    echo "   kubectl set image deployment/grc-$STAGE grc=registry.shahin-grc.com/grc:$PREVIOUS_VERSION"
else
    echo "2. Rollback to previous deployment revision:"
    echo "   kubectl rollout undo deployment/grc-$STAGE"
fi
echo ""

echo "3. Restore deployment replicas:"
case $STAGE in
    canary)
        echo "   kubectl scale deployment/grc-$STAGE --replicas=1"
        ;;
    pilot)
        echo "   kubectl scale deployment/grc-$STAGE --replicas=2"
        ;;
    partial)
        echo "   kubectl scale deployment/grc-$STAGE --replicas=3"
        ;;
    full)
        echo "   kubectl scale deployment/grc-$STAGE --replicas=5"
        ;;
esac
echo ""

echo "4. Wait for rollout to complete:"
echo "   kubectl rollout status deployment/grc-$STAGE"
echo ""

echo "5. Disable problematic features (if applicable):"
echo "   Update feature flags to disable new features"
echo "   Revert configuration changes"
echo ""

echo "6. Verify rollback success:"
echo "   ${SCRIPT_DIR}/validate-phase1.sh --base-url <stage-url>"
echo ""

echo "7. Database rollback (if needed):"
echo "   # Check migration history"
echo "   docker exec shahin-postgres psql -U shahin_admin -d shahin_grc \\"
echo "     -c \"SELECT * FROM \\\"__EFMigrationsHistory\\\" ORDER BY \\\"MigrationId\\\" DESC LIMIT 5;\""
echo ""
echo "   # Remove problematic migration (if safe)"
echo "   docker exec shahin-postgres psql -U shahin_admin -d shahin_grc \\"
echo "     -c \"DELETE FROM \\\"__EFMigrationsHistory\\\" WHERE \\\"MigrationId\\\" = '<migration-id>';\""
echo ""

# Log rollback actions
{
    echo ""
    echo "ROLLBACK INITIATED"
    echo "=================="
    echo "Status: PENDING_MANUAL_EXECUTION"
    echo "Rollback to: $PREVIOUS_VERSION"
    echo ""
    echo "Required Actions:"
    echo "1. Stop current deployment"
    echo "2. Rollback to previous version"
    echo "3. Restore replicas"
    echo "4. Verify health"
    echo ""
    echo "Reason for rollback:"
    echo "$REASON"
} >> "$LOG_FILE"

# Create incident report template
INCIDENT_FILE="${ROLLBACK_DIR}/${TIMESTAMP}_${STAGE}_incident.md"
cat > "$INCIDENT_FILE" << EOF
# Rollback Incident Report

## Incident Details
- **Date/Time**: $(date)
- **Stage**: $STAGE
- **Current Version**: ${CURRENT_VERSION:-unknown}
- **Rolled Back To**: ${PREVIOUS_VERSION:-unknown}
- **Initiated By**: $(whoami)

## Reason for Rollback
$REASON

## Actions Taken
- [ ] Deployment stopped
- [ ] Previous version restored
- [ ] Replicas scaled back
- [ ] Health checks verified
- [ ] Feature flags updated
- [ ] Database rollback (if applicable)

## Impact
- **Duration**: _To be filled_
- **Users Affected**: _To be filled_
- **Services Impacted**: _To be filled_

## Root Cause
_To be determined_

## Follow-up Actions
- [ ] Root cause analysis
- [ ] Fix implemented
- [ ] Tests added
- [ ] Documentation updated

## Lessons Learned
_To be filled after analysis_

---
Generated: $TIMESTAMP
EOF

# Notify
echo ""
echo -e "${BLUE}Step 3: Notifications${NC}"
echo "------------------------------------------------"
echo "📧 Send notifications to:"
echo "  • DevOps team: ops@shahin-grc.com"
echo "  • Engineering lead: engineering@shahin-grc.com"
echo "  • Product owner: product@shahin-grc.com"
echo ""
echo "Include:"
echo "  • Stage: $STAGE"
echo "  • Reason: $REASON"
echo "  • Rollback log: $LOG_FILE"
echo "  • Incident report: $INCIDENT_FILE"

echo ""
echo "================================================"
echo -e "${YELLOW}⚠️  ROLLBACK INITIATED${NC}"
echo "------------------------------------------------"
echo -e "${GREEN}✓ Rollback log created: $LOG_FILE${NC}"
echo -e "${GREEN}✓ Incident report created: $INCIDENT_FILE${NC}"
echo ""
echo -e "${RED}⚠️  Execute manual rollback steps above${NC}"
echo "================================================"
