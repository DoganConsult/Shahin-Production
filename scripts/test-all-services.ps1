# Comprehensive Service Testing Script
# Tests all major services and endpoints

param(
    [string]$BaseUrl = "http://localhost:5000"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Comprehensive Service Testing" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Base URL: $BaseUrl" -ForegroundColor Gray
Write-Host ""

$results = @{
    Passed = 0
    Failed = 0
    Skipped = 0
}

function Test-Endpoint {
    param(
        [string]$Method = "GET",
        [string]$Endpoint,
        [string]$Description,
        [hashtable]$Headers = @{},
        [string]$Body = $null
    )
    
    $url = "$BaseUrl$Endpoint"
    Write-Host "Testing: $Description" -ForegroundColor Yellow
    Write-Host "  $Method $Endpoint" -ForegroundColor Gray
    
    try {
        $params = @{
            Uri = $url
            Method = $Method
            Headers = $Headers
            ErrorAction = "Stop"
            TimeoutSec = 10
        }
        
        if ($Body) {
            $params.Body = $Body
            $params.ContentType = "application/json"
        }
        
        $response = Invoke-WebRequest @params
        
        if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 300) {
            Write-Host "  ✓ SUCCESS (Status: $($response.StatusCode))" -ForegroundColor Green
            $script:results.Passed++
            return $true
        } elseif ($response.StatusCode -eq 401 -or $response.StatusCode -eq 403) {
            Write-Host "  ⚠ SKIPPED (Auth required - Status: $($response.StatusCode))" -ForegroundColor Yellow
            $script:results.Skipped++
            return $null
        } else {
            Write-Host "  ✗ FAILED (Status: $($response.StatusCode))" -ForegroundColor Red
            $script:results.Failed++
            return $false
        }
    } catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq 401 -or $statusCode -eq 403) {
            Write-Host "  ⚠ SKIPPED (Auth required - Status: $statusCode)" -ForegroundColor Yellow
            $script:results.Skipped++
            return $null
        } else {
            Write-Host "  ✗ FAILED ($($_.Exception.Message))" -ForegroundColor Red
            $script:results.Failed++
            return $false
        }
    }
    Write-Host ""
}

# 1. Health Checks
Write-Host "=== 1. Health & System Checks ===" -ForegroundColor Cyan
Test-Endpoint -Endpoint "/health" -Description "Basic Health Check"
Test-Endpoint -Endpoint "/health/ready" -Description "Readiness Check"
Test-Endpoint -Endpoint "/health/live" -Description "Liveness Check"
Test-Endpoint -Endpoint "/api/system-test/health" -Description "System Health Test"

# 2. Database & Schema
Write-Host "`n=== 2. Database & Schema Tests ===" -ForegroundColor Cyan
Test-Endpoint -Endpoint "/api/system-test/database" -Description "Database Connection Test"
Test-Endpoint -Endpoint "/api/schema-test/tables" -Description "List Database Tables"

# 3. Integration Health
Write-Host "`n=== 3. Integration Health Checks ===" -ForegroundColor Cyan
Test-Endpoint -Endpoint "/api/integration-health" -Description "All Integrations Health"
Test-Endpoint -Endpoint "/api/integration-health/claude" -Description "Claude AI Health"
Test-Endpoint -Endpoint "/api/integration-health/copilot" -Description "Copilot Health"
Test-Endpoint -Endpoint "/api/integration-health/email" -Description "Email Service Health"

# 4. Public API Endpoints (No Auth Required)
Write-Host "`n=== 4. Public API Endpoints ===" -ForegroundColor Cyan
Test-Endpoint -Endpoint "/api/controls" -Description "List Controls (Public)"
Test-Endpoint -Endpoint "/api/risks" -Description "List Risks (Public)"
Test-Endpoint -Endpoint "/api/evidence" -Description "List Evidence (Public)"
Test-Endpoint -Endpoint "/api/policies" -Description "List Policies (Public)"

# 5. Authentication Endpoints
Write-Host "`n=== 5. Authentication Endpoints ===" -ForegroundColor Cyan
$registerBody = @{
    Email = "test@example.com"
    Password = "Test123!@#"
    FirstName = "Test"
    LastName = "User"
} | ConvertTo-Json

Test-Endpoint -Method "POST" -Endpoint "/api/auth/register" -Description "User Registration" -Body $registerBody

# 6. Swagger/API Docs
Write-Host "`n=== 6. API Documentation ===" -ForegroundColor Cyan
Test-Endpoint -Endpoint "/swagger/v1/swagger.json" -Description "Swagger JSON"
Test-Endpoint -Endpoint "/api-docs" -Description "API Documentation UI"

# 7. Dashboard Endpoints (May require auth)
Write-Host "`n=== 7. Dashboard Endpoints ===" -ForegroundColor Cyan
Test-Endpoint -Endpoint "/api/dashboard/metrics" -Description "Dashboard Metrics"
Test-Endpoint -Endpoint "/api/dashboard/compliance-status" -Description "Compliance Status"

# 8. Assessment & Audit (May require auth)
Write-Host "`n=== 8. Assessment & Audit ===" -ForegroundColor Cyan
Test-Endpoint -Endpoint "/api/assessments" -Description "List Assessments"
Test-Endpoint -Endpoint "/api/audits" -Description "List Audits"

# Summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Test Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Passed:  $($results.Passed)" -ForegroundColor Green
Write-Host "Failed:  $($results.Failed)" -ForegroundColor $(if ($results.Failed -eq 0) { "Green" } else { "Red" })
Write-Host "Skipped: $($results.Skipped)" -ForegroundColor Yellow
Write-Host ""

if ($results.Failed -eq 0) {
    Write-Host "✅ All tests passed or skipped (auth required)" -ForegroundColor Green
    exit 0
} else {
    Write-Host "❌ Some tests failed" -ForegroundColor Red
    exit 1
}
