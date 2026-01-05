#!/bin/bash

################################################################################
# GRC System - Automated Workflow Testing Script
# Tests all 10 predefined workflows end-to-end
# Version: 1.0
# Date: 2026-01-02
################################################################################

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
API_BASE_URL="${API_BASE_URL:-http://localhost:5000}"
API_URL="${API_BASE_URL}/api/app"
ADMIN_USERNAME="${ADMIN_USERNAME:-admin}"
ADMIN_PASSWORD="${ADMIN_PASSWORD:-1q2w3E*}"
TOKEN=""
TEST_RESULTS=()
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

################################################################################
# Utility Functions
################################################################################

print_header() {
    echo -e "\n${BLUE}========================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}========================================${NC}\n"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_info() {
    echo -e "${YELLOW}ℹ $1${NC}"
}

print_step() {
    echo -e "${BLUE}→ $1${NC}"
}

################################################################################
# Authentication
################################################################################

authenticate() {
    print_header "AUTHENTICATION"
    print_step "Authenticating as $ADMIN_USERNAME..."

    # ABP uses /api/account/login endpoint
    local response=$(curl -s -X POST "$API_BASE_URL/api/account/login" \
        -H "Content-Type: application/json" \
        -d "{
            \"userNameOrEmailAddress\": \"$ADMIN_USERNAME\",
            \"password\": \"$ADMIN_PASSWORD\"
        }")

    # Extract access token from response
    TOKEN=$(echo $response | grep -o '"access_token":"[^"]*' | cut -d'"' -f4)

    if [ -z "$TOKEN" ]; then
        print_error "Authentication failed!"
        print_info "Response: $response"
        print_info "Make sure the application is running at $API_BASE_URL"
        print_info "And credentials are correct: $ADMIN_USERNAME / $ADMIN_PASSWORD"
        exit 1
    fi

    print_success "Authentication successful"
    print_info "Token obtained (${#TOKEN} characters)"
}

################################################################################
# Helper Functions
################################################################################

make_api_call() {
    local method=$1
    local endpoint=$2
    local data=$3
    local description=$4

    print_step "$description"

    local response=$(curl -s -X $method "$API_URL/$endpoint" \
        -H "Authorization: Bearer $TOKEN" \
        -H "Content-Type: application/json" \
        -d "$data")

    echo "$response"
}

test_workflow() {
    local workflow_name=$1
    local test_description=$2

    TOTAL_TESTS=$((TOTAL_TESTS + 1))

    echo -e "\n${YELLOW}Test #$TOTAL_TESTS: $test_description${NC}"
}

record_result() {
    local test_name=$1
    local status=$2
    local message=$3

    if [ "$status" == "PASS" ]; then
        PASSED_TESTS=$((PASSED_TESTS + 1))
        print_success "$test_name - PASSED: $message"
        TEST_RESULTS+=("✓ $test_name")
    else
        FAILED_TESTS=$((FAILED_TESTS + 1))
        print_error "$test_name - FAILED: $message"
        TEST_RESULTS+=("✗ $test_name: $message")
    fi
}

check_response() {
    local response=$1
    local test_name=$2

    # Check if response contains error
    if echo "$response" | grep -q '"error"'; then
        local error_msg=$(echo "$response" | grep -o '"message":"[^"]*' | cut -d'"' -f4)
        record_result "$test_name" "FAIL" "API Error: $error_msg"
        return 1
    fi

    # Check if response contains an id (successful creation)
    if echo "$response" | grep -q '"id"'; then
        local id=$(echo "$response" | grep -o '"id":"[^"]*' | cut -d'"' -f4)
        record_result "$test_name" "PASS" "Created successfully with ID: $id"
        echo "$id"
        return 0
    fi

    # Check for empty response
    if [ -z "$response" ]; then
        record_result "$test_name" "FAIL" "Empty response from API"
        return 1
    fi

    record_result "$test_name" "PASS" "Request completed"
    return 0
}

################################################################################
# Workflow Tests
################################################################################

test_evidence_approval_workflow() {
    print_header "TEST 1: Evidence Approval Workflow"

    test_workflow "Evidence Approval" "Testing standard evidence approval (non-restricted)"

    local evidence_data='{
        "name": "Test Evidence - Automated Test",
        "description": "Automated test evidence for workflow validation",
        "evidenceType": "Documentation",
        "dataClassification": "internal",
        "owner": "admin"
    }'

    local response=$(make_api_call "POST" "evidence" "$evidence_data" "Creating evidence...")
    local evidence_id=$(check_response "$response" "Evidence Creation")

    if [ ! -z "$evidence_id" ]; then
        print_info "Evidence ID: $evidence_id"

        # Submit for approval (trigger workflow)
        print_step "Submitting evidence for approval (triggering workflow)..."
        local submit_response=$(make_api_call "POST" "evidence/$evidence_id/submit" "{}" "Triggering evidence.submitted event")
        check_response "$submit_response" "Evidence Workflow Trigger"
    fi

    # Test with restricted classification
    test_workflow "Evidence Approval" "Testing restricted evidence (full approval chain)"

    local restricted_evidence_data='{
        "name": "Test Restricted Evidence - Automated Test",
        "description": "Restricted evidence requiring full approval chain",
        "evidenceType": "Security Documentation",
        "dataClassification": "restricted",
        "owner": "admin"
    }'

    local response2=$(make_api_call "POST" "evidence" "$restricted_evidence_data" "Creating restricted evidence...")
    local evidence_id2=$(check_response "$response2" "Restricted Evidence Creation")

    if [ ! -z "$evidence_id2" ]; then
        local submit_response2=$(make_api_call "POST" "evidence/$evidence_id2/submit" "{}" "Triggering workflow for restricted evidence")
        check_response "$submit_response2" "Restricted Evidence Workflow Trigger"
    fi
}

test_assessment_review_workflow() {
    print_header "TEST 2: Assessment Review Workflow"

    test_workflow "Assessment Review" "Testing assessment review and approval"

    # First, get a framework ID
    print_step "Fetching regulatory frameworks..."
    local frameworks_response=$(make_api_call "GET" "regulatory-framework" "" "Getting frameworks list")

    local assessment_data='{
        "name": "Q1 2026 Compliance Assessment - Automated Test",
        "description": "Automated assessment for workflow testing",
        "assessmentType": "Compliance",
        "scope": "IT Department",
        "status": "Draft",
        "owner": "admin"
    }'

    local response=$(make_api_call "POST" "assessment" "$assessment_data" "Creating assessment...")
    local assessment_id=$(check_response "$response" "Assessment Creation")

    if [ ! -z "$assessment_id" ]; then
        print_step "Submitting assessment for review (triggering workflow)..."
        local submit_response=$(make_api_call "POST" "assessment/$assessment_id/submit" "{}" "Triggering assessment.submitted event")
        check_response "$submit_response" "Assessment Workflow Trigger"
    fi
}

test_risk_acceptance_workflow() {
    print_header "TEST 3: Risk Acceptance Workflow"

    # Test 1: Low/Medium risk
    test_workflow "Risk Acceptance" "Testing low/medium risk acceptance"

    local risk_data='{
        "name": "Outdated Software Version - Automated Test",
        "description": "Test risk for workflow validation",
        "riskCategory": "Technology",
        "severity": "Medium",
        "likelihood": "Medium",
        "impact": "Low",
        "riskScore": 6,
        "owner": "admin"
    }'

    local response=$(make_api_call "POST" "risk" "$risk_data" "Creating medium risk...")
    local risk_id=$(check_response "$response" "Risk Creation (Medium)")

    if [ ! -z "$risk_id" ]; then
        local submit_response=$(make_api_call "POST" "risk/$risk_id/submit" "{}" "Triggering risk.submitted event")
        check_response "$submit_response" "Risk Workflow Trigger (Medium)"
    fi

    # Test 2: High/Critical risk
    test_workflow "Risk Acceptance" "Testing high/critical risk acceptance (with CISO approval)"

    local critical_risk_data='{
        "name": "Unpatched Critical Vulnerability - Automated Test",
        "description": "Critical risk requiring CISO approval",
        "riskCategory": "Security",
        "severity": "Critical",
        "likelihood": "High",
        "impact": "Critical",
        "riskScore": 25,
        "owner": "admin"
    }'

    local response2=$(make_api_call "POST" "risk" "$critical_risk_data" "Creating critical risk...")
    local risk_id2=$(check_response "$response2" "Risk Creation (Critical)")

    if [ ! -z "$risk_id2" ]; then
        local submit_response2=$(make_api_call "POST" "risk/$risk_id2/submit" "{}" "Triggering workflow for critical risk")
        check_response "$submit_response2" "Critical Risk Workflow Trigger"
    fi
}

test_policy_approval_workflow() {
    print_header "TEST 4: Policy Approval Workflow"

    # Test 1: Policy without legal review
    test_workflow "Policy Approval" "Testing policy approval without legal review"

    local policy_data='{
        "name": "Password Policy v2.0 - Automated Test",
        "description": "Test policy for workflow validation",
        "policyType": "Security Policy",
        "version": "2.0",
        "requiresLegalReview": false,
        "owner": "admin"
    }'

    local response=$(make_api_call "POST" "policy-document" "$policy_data" "Creating policy...")
    local policy_id=$(check_response "$response" "Policy Creation (No Legal Review)")

    if [ ! -z "$policy_id" ]; then
        local submit_response=$(make_api_call "POST" "policy-document/$policy_id/submit" "{}" "Triggering policy.submitted event")
        check_response "$submit_response" "Policy Workflow Trigger (No Legal)"
    fi

    # Test 2: Policy with legal review
    test_workflow "Policy Approval" "Testing policy approval with legal review"

    local legal_policy_data='{
        "name": "Data Retention Policy - Automated Test",
        "description": "Policy requiring legal review",
        "policyType": "Data Governance",
        "version": "1.0",
        "requiresLegalReview": true,
        "owner": "admin"
    }'

    local response2=$(make_api_call "POST" "policy-document" "$legal_policy_data" "Creating policy with legal review...")
    local policy_id2=$(check_response "$response2" "Policy Creation (With Legal Review)")

    if [ ! -z "$policy_id2" ]; then
        local submit_response2=$(make_api_call "POST" "policy-document/$policy_id2/submit" "{}" "Triggering workflow with legal review")
        check_response "$submit_response2" "Policy Workflow Trigger (With Legal)"
    fi
}

test_action_plan_workflow() {
    print_header "TEST 5: Action Plan Approval Workflow"

    test_workflow "Action Plan Approval" "Testing action plan approval and task assignment"

    local action_plan_data='{
        "name": "Remediate Critical Vulnerability - Automated Test",
        "description": "Test action plan for workflow validation",
        "priority": "High",
        "dueDate": "2026-02-01",
        "owner": "admin"
    }'

    local response=$(make_api_call "POST" "action-plan" "$action_plan_data" "Creating action plan...")
    local action_plan_id=$(check_response "$response" "Action Plan Creation")

    if [ ! -z "$action_plan_id" ]; then
        local submit_response=$(make_api_call "POST" "action-plan/$action_plan_id/submit" "{}" "Triggering actionplan.submitted event")
        check_response "$submit_response" "Action Plan Workflow Trigger"
    fi
}

test_audit_review_workflow() {
    print_header "TEST 6: Audit Review Workflow"

    test_workflow "Audit Review" "Testing audit review and closure workflow"

    local audit_data='{
        "name": "Q4 2025 Internal Audit - Automated Test",
        "description": "Test audit for workflow validation",
        "auditType": "Internal",
        "startDate": "2025-12-01",
        "endDate": "2026-01-02",
        "findings": "Test findings for automated test",
        "recommendations": "Test recommendations for automated test",
        "owner": "admin"
    }'

    local response=$(make_api_call "POST" "audit" "$audit_data" "Creating audit...")
    local audit_id=$(check_response "$response" "Audit Creation")

    if [ ! -z "$audit_id" ]; then
        local complete_response=$(make_api_call "POST" "audit/$audit_id/complete" "{}" "Triggering audit.completed event")
        check_response "$complete_response" "Audit Workflow Trigger"
    fi
}

test_committee_decision_workflow() {
    print_header "TEST 7: Committee Decision Workflow"

    test_workflow "Committee Decision" "Testing committee decision workflow"

    print_info "Note: This workflow requires Committee module implementation"
    print_step "Creating committee meeting..."

    local meeting_data='{
        "name": "Cybersecurity Committee - Jan 2026 - Automated Test",
        "description": "Test committee meeting",
        "meetingDate": "2026-01-10",
        "agenda": "Risk Review and Approval"
    }'

    local response=$(make_api_call "POST" "committee/meeting" "$meeting_data" "Scheduling committee meeting...")

    # May not exist yet, so handle gracefully
    if echo "$response" | grep -q "404"; then
        record_result "Committee Meeting Workflow" "SKIP" "Committee module endpoint not found (expected for new system)"
    else
        check_response "$response" "Committee Meeting Creation"
    fi
}

test_training_assignment_workflow() {
    print_header "TEST 8: Training Assignment Workflow"

    test_workflow "Training Assignment" "Testing training assignment workflow"

    print_info "Note: This workflow requires Training module implementation"
    print_step "Creating training program..."

    local training_data='{
        "name": "Cybersecurity Awareness 2026 - Automated Test",
        "description": "Test training program",
        "trainingType": "Mandatory",
        "duration": 120,
        "dueDate": "2026-02-01"
    }'

    local response=$(make_api_call "POST" "training" "$training_data" "Creating training program...")

    # May not exist yet, so handle gracefully
    if echo "$response" | grep -q "404"; then
        record_result "Training Assignment Workflow" "SKIP" "Training module endpoint not found (expected for new system)"
    else
        check_response "$response" "Training Assignment Creation"
    fi
}

test_nca_assessment_workflow() {
    print_header "TEST 9: NCA Essential Cybersecurity Controls Assessment"

    test_workflow "NCA Assessment" "Testing complete NCA assessment workflow (8 steps)"

    print_info "Note: This is a complex 22-day workflow with 8 steps"
    print_step "Starting NCA assessment workflow..."

    local workflow_data='{
        "workflowName": "NCA Essential Cybersecurity Controls Assessment",
        "triggerEvent": "nca.assessment.started",
        "initiatedBy": "admin"
    }'

    local response=$(make_api_call "POST" "workflow/start-workflow" "$workflow_data" "Triggering NCA assessment...")

    if echo "$response" | grep -q "404"; then
        record_result "NCA Assessment Workflow" "SKIP" "Workflow engine endpoint not found (will be available after workflow module is fully implemented)"
    else
        local workflow_id=$(check_response "$response" "NCA Assessment Workflow Start")
        if [ ! -z "$workflow_id" ]; then
            print_info "NCA Workflow Instance ID: $workflow_id"
            print_info "This workflow will execute 8 steps over ~22 days"
        fi
    fi
}

test_sama_assessment_workflow() {
    print_header "TEST 10: SAMA Cybersecurity Framework Assessment"

    test_workflow "SAMA Assessment" "Testing complete SAMA assessment workflow (7 steps)"

    print_info "Note: This is a complex 23-day workflow with 7 steps"
    print_step "Starting SAMA assessment workflow..."

    local workflow_data='{
        "workflowName": "SAMA Cybersecurity Framework Assessment",
        "triggerEvent": "sama.assessment.started",
        "initiatedBy": "admin"
    }'

    local response=$(make_api_call "POST" "workflow/start-workflow" "$workflow_data" "Triggering SAMA assessment...")

    if echo "$response" | grep -q "404"; then
        record_result "SAMA Assessment Workflow" "SKIP" "Workflow engine endpoint not found (will be available after workflow module is fully implemented)"
    else
        local workflow_id=$(check_response "$response" "SAMA Assessment Workflow Start")
        if [ ! -z "$workflow_id" ]; then
            print_info "SAMA Workflow Instance ID: $workflow_id"
            print_info "This workflow will execute 7 steps over ~23 days"
        fi
    fi
}

################################################################################
# Test Summary
################################################################################

print_summary() {
    print_header "TEST EXECUTION SUMMARY"

    echo -e "${BLUE}Total Tests:${NC} $TOTAL_TESTS"
    echo -e "${GREEN}Passed:${NC} $PASSED_TESTS"
    echo -e "${RED}Failed:${NC} $FAILED_TESTS"

    local skip_tests=$((TOTAL_TESTS - PASSED_TESTS - FAILED_TESTS))
    if [ $skip_tests -gt 0 ]; then
        echo -e "${YELLOW}Skipped:${NC} $skip_tests (endpoints not yet implemented)"
    fi

    local success_rate=0
    if [ $TOTAL_TESTS -gt 0 ]; then
        success_rate=$((PASSED_TESTS * 100 / TOTAL_TESTS))
    fi
    echo -e "${BLUE}Success Rate:${NC} $success_rate%"

    echo -e "\n${BLUE}Detailed Results:${NC}"
    for result in "${TEST_RESULTS[@]}"; do
        echo "  $result"
    done

    echo ""
    if [ $FAILED_TESTS -eq 0 ]; then
        print_success "All tests passed! ✓"
        return 0
    else
        print_error "Some tests failed. Check the output above for details."
        return 1
    fi
}

################################################################################
# Database Verification
################################################################################

verify_workflows_seeded() {
    print_header "VERIFYING WORKFLOW SEEDING"

    print_step "Checking if workflows were seeded in database..."

    local response=$(make_api_call "GET" "workflow" "" "Fetching workflows list...")

    if echo "$response" | grep -q '"totalCount"'; then
        local count=$(echo "$response" | grep -o '"totalCount":[0-9]*' | cut -d':' -f2)
        if [ "$count" -ge 10 ]; then
            print_success "Workflows seeded successfully ($count workflows found, expected 10)"
        else
            print_error "Only $count workflows found (expected 10)"
            print_info "Run database migrations to seed workflows: cd src/Grc.DbMigrator && dotnet run"
        fi
    else
        print_info "Cannot verify workflow count (endpoint may not be implemented yet)"
    fi
}

################################################################################
# Main Execution
################################################################################

main() {
    clear
    print_header "GRC SYSTEM - AUTOMATED WORKFLOW TESTING"
    echo "Testing all 10 predefined workflows end-to-end"
    echo "API Base URL: $API_BASE_URL"
    echo ""

    # Check if application is running
    print_step "Checking if application is running..."
    if ! curl -s -o /dev/null -w "%{http_code}" "$API_BASE_URL/api/abp/application-configuration" | grep -q "200"; then
        print_error "Application is not responding at $API_BASE_URL"
        print_info "Please start the application first:"
        print_info "  cd src/Grc.HttpApi.Host && dotnet run"
        exit 1
    fi
    print_success "Application is running"

    # Authenticate
    authenticate

    # Verify workflows are seeded
    verify_workflows_seeded

    # Run all workflow tests
    test_evidence_approval_workflow
    test_assessment_review_workflow
    test_risk_acceptance_workflow
    test_policy_approval_workflow
    test_action_plan_workflow
    test_audit_review_workflow
    test_committee_decision_workflow
    test_training_assignment_workflow
    test_nca_assessment_workflow
    test_sama_assessment_workflow

    # Print summary
    print_summary

    local exit_code=$?

    print_header "NEXT STEPS"
    echo "1. Review the test results above"
    echo "2. Check the application logs for workflow execution details"
    echo "3. Verify workflow instances in database"
    echo "4. Test approval steps with different user roles"
    echo "5. Monitor notifications and audit trails"
    echo ""
    echo "For detailed test procedures, see: WORKFLOW_END_TO_END_TESTING.md"

    exit $exit_code
}

# Run main function
main "$@"
