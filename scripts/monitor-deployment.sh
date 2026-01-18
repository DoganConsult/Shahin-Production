#!/bin/bash
set -euo pipefail

# Deployment Monitoring Script
# Usage: ./scripts/monitor-deployment.sh <canary|pilot|partial|full> [--minutes 60]

if [ $# -lt 1 ]; then
    echo "Usage: $0 <canary|pilot|partial|full> [--minutes 60]"
    exit 1
fi

STAGE=$1
MINUTES="${3:-60}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OPS_DIR="${SCRIPT_DIR}/../ops/gradual"

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Load environment
ENV_FILE="${OPS_DIR}/targets.env"
if [ -f "$ENV_FILE" ]; then
    source "$ENV_FILE"
fi

# Determine base URL for stage
case $STAGE in
    canary) BASE_URL="${CANARY_BASE_URL:-http://localhost:5000}" ;;
    pilot) BASE_URL="${PILOT_BASE_URL:-http://localhost:5001}" ;;
    partial) BASE_URL="${PARTIAL_BASE_URL:-http://localhost:5002}" ;;
    full) BASE_URL="${FULL_BASE_URL:-http://localhost:5003}" ;;
    *)
        echo -e "${RED}Invalid stage: $STAGE${NC}"
        exit 1
        ;;
esac

# Monitoring parameters
POLL_INTERVAL=60  # seconds
MAX_CONSECUTIVE_FAILURES=5
LATENCY_THRESHOLD_MS=500
ERROR_RATE_THRESHOLD=0.01  # 1%

# Counters
TOTAL_CHECKS=0
FAILED_CHECKS=0
CONSECUTIVE_FAILURES=0
LATENCY_SAMPLES=()

echo "================================================"
echo -e "${BLUE}DEPLOYMENT MONITORING - ${STAGE^^}${NC}"
echo "================================================"
echo "Stage: $STAGE"
echo "Base URL: $BASE_URL"
echo "Duration: $MINUTES minutes"
echo "Poll Interval: ${POLL_INTERVAL}s"
echo "------------------------------------------------"
echo "Thresholds:"
echo "  • Max consecutive failures: $MAX_CONSECUTIVE_FAILURES"
echo "  • Latency threshold: ${LATENCY_THRESHOLD_MS}ms"
echo "  • Error rate threshold: ${ERROR_RATE_THRESHOLD} (1%)"
echo "------------------------------------------------"
echo ""

# Calculate end time
END_TIME=$(($(date +%s) + MINUTES * 60))

# Monitoring loop
while [ $(date +%s) -lt $END_TIME ]; do
    TIMESTAMP=$(date '+%Y-%m-%d %H:%M:%S')
    echo -n "[$TIMESTAMP] Checking $STAGE... "
    
    # Measure latency and check health
    START_TIME=$(date +%s%N)
    if RESPONSE=$(curl -f -s -w "\n%{http_code}" "$BASE_URL/health" 2>/dev/null); then
        END_TIME_NS=$(date +%s%N)
        LATENCY_MS=$(((END_TIME_NS - START_TIME) / 1000000))
        HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
        
        # Success
        echo -e "${GREEN}✓${NC} (${LATENCY_MS}ms, HTTP ${HTTP_CODE})"
        CONSECUTIVE_FAILURES=0
        ((TOTAL_CHECKS++))
        LATENCY_SAMPLES+=($LATENCY_MS)
        
        # Check latency threshold
        if [ $LATENCY_MS -gt $LATENCY_THRESHOLD_MS ]; then
            echo -e "  ${YELLOW}⚠ High latency: ${LATENCY_MS}ms > ${LATENCY_THRESHOLD_MS}ms${NC}"
        fi
        
        # Additional endpoint checks (non-blocking)
        for endpoint in "/health/ready" "/health/live"; do
            if curl -f -s "$BASE_URL$endpoint" >/dev/null 2>&1; then
                echo -e "  ${endpoint}: ${GREEN}✓${NC}"
            fi
        done
    else
        # Failure
        echo -e "${RED}✗ FAIL${NC}"
        ((CONSECUTIVE_FAILURES++))
        ((FAILED_CHECKS++))
        ((TOTAL_CHECKS++))
        
        echo -e "  ${RED}Consecutive failures: $CONSECUTIVE_FAILURES${NC}"
        
        # Check consecutive failure threshold
        if [ $CONSECUTIVE_FAILURES -ge $MAX_CONSECUTIVE_FAILURES ]; then
            echo ""
            echo -e "${RED}🚨 ALERT: $CONSECUTIVE_FAILURES consecutive failures!${NC}"
            echo "Deployment appears unhealthy. Immediate action required."
            echo ""
            echo "Recommended action:"
            echo "  ${SCRIPT_DIR}/rollback-deployment.sh $STAGE \"$CONSECUTIVE_FAILURES consecutive health check failures\""
            echo ""
            exit 2
        fi
    fi
    
    # Calculate and check error rate
    if [ $TOTAL_CHECKS -gt 0 ]; then
        ERROR_RATE=$(awk "BEGIN {printf \"%.4f\", $FAILED_CHECKS / $TOTAL_CHECKS}")
        
        # Display stats every 5 checks
        if [ $((TOTAL_CHECKS % 5)) -eq 0 ]; then
            echo ""
            echo "  📊 Stats: Checks=$TOTAL_CHECKS, Failed=$FAILED_CHECKS, Error Rate=$ERROR_RATE"
            
            # Calculate P95 latency (approximate)
            if [ ${#LATENCY_SAMPLES[@]} -gt 0 ]; then
                IFS=$'\n' SORTED=($(sort -n <<<"${LATENCY_SAMPLES[*]}"))
                P95_INDEX=$((${#SORTED[@]} * 95 / 100))
                P95_LATENCY="${SORTED[$P95_INDEX]}"
                echo "  📈 P95 Latency: ${P95_LATENCY}ms"
                
                # Check P95 threshold
                if [ "$P95_LATENCY" -gt $LATENCY_THRESHOLD_MS ]; then
                    echo -e "  ${YELLOW}⚠ P95 latency exceeds threshold!${NC}"
                fi
            fi
            echo ""
        fi
        
        # Check error rate threshold
        if (( $(awk "BEGIN {print ($ERROR_RATE > $ERROR_RATE_THRESHOLD)}") )); then
            echo ""
            echo -e "${RED}🚨 ALERT: Error rate ${ERROR_RATE} exceeds threshold ${ERROR_RATE_THRESHOLD}!${NC}"
            echo "Deployment health degraded. Consider rollback."
            echo ""
            echo "Recommended action:"
            echo "  ${SCRIPT_DIR}/rollback-deployment.sh $STAGE \"Error rate ${ERROR_RATE} exceeded threshold\""
            echo ""
            exit 2
        fi
    fi
    
    # Sleep before next check
    sleep $POLL_INTERVAL
done

# Final summary
echo ""
echo "================================================"
echo "MONITORING SUMMARY"
echo "------------------------------------------------"
echo "Duration: $MINUTES minutes"
echo "Total Checks: $TOTAL_CHECKS"
echo "Failed Checks: $FAILED_CHECKS"

if [ $TOTAL_CHECKS -gt 0 ]; then
    FINAL_ERROR_RATE=$(awk "BEGIN {printf \"%.4f\", $FAILED_CHECKS / $TOTAL_CHECKS}")
    SUCCESS_RATE=$(awk "BEGIN {printf \"%.2f\", 100 * (1 - $FINAL_ERROR_RATE)}")
    echo "Success Rate: ${SUCCESS_RATE}%"
    echo "Error Rate: $FINAL_ERROR_RATE"
fi

if [ ${#LATENCY_SAMPLES[@]} -gt 0 ]; then
    # Calculate average latency
    SUM=0
    for lat in "${LATENCY_SAMPLES[@]}"; do
        SUM=$((SUM + lat))
    done
    AVG_LATENCY=$((SUM / ${#LATENCY_SAMPLES[@]}))
    echo "Average Latency: ${AVG_LATENCY}ms"
fi

echo "------------------------------------------------"

if [ $FAILED_CHECKS -eq 0 ]; then
    echo -e "${GREEN}✅ Deployment healthy - no issues detected${NC}"
    exit 0
else
    echo -e "${YELLOW}⚠ Deployment completed with $FAILED_CHECKS failures${NC}"
    exit 0
fi
