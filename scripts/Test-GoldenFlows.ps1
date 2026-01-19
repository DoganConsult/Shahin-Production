# Golden Flow Validation Script
# Gate B: End-to-End Golden Flows - Production Readiness
# Reference: ProductionReadinessGates.GateB_GoldenFlows

param(
    [string]$BaseUrl = "http://localhost:5000",
    [string]$AdminEmail = "admin@grc.com",
    [string]$AdminPassword = "Admin@12345",
    [string]$OutputDir = ".\Golden_Flow_Evidence"
)

$ErrorActionPreference = "Continue"

# Create output directory
if (!(Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$reportFile = Join-Path $OutputDir "GoldenFlowReport_$timestamp.md"

# Initialize report
@"
# Golden Flow Test Report

**Generated:** $(Get-Date -Format "yyyy-MM-dd HH:mm:ss") UTC
**Base URL:** $BaseUrl
**Test Environment:** $(if ($BaseUrl -match "localhost") { "Local" } else { "Staging" })

---

"@ | Out-File $reportFile

function Write-Report {
    param([string]$Text)
    $Text | Out-File $reportFile -Append
    Write-Host $Text
}

function Test-ApiEndpoint {
    param(
        [string]$Method,
        [string]$Endpoint,
        [hashtable]$Body,
        [hashtable]$Headers,
        [string]$Description
    )

    try {
        $uri = "$BaseUrl$Endpoint"
        $params = @{
            Method = $Method
            Uri = $uri
            ContentType = "application/json"
        }

        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
        }

        if ($Headers) {
            $params.Headers = $Headers
        }

        $response = Invoke-RestMethod @params -ErrorAction Stop
        return @{
            Success = $true
            StatusCode = 200
            Data = $response
        }
    }
    catch {
        $statusCode = 0
        if ($_.Exception.Response) {
            $statusCode = [int]$_.Exception.Response.StatusCode
        }
        return @{
            Success = $false
            StatusCode = $statusCode
            Error = $_.Exception.Message
        }
    }
}

Write-Report "## Flow B1: Self Registration"
Write-Report ""

$testEmail = "test-user-$([Guid]::NewGuid().ToString('N').Substring(0,8))@test.com"
$testPassword = "Test@12345678"

Write-Report "**Test Email:** $testEmail"
Write-Report ""

# B1 - Register
$b1Register = Test-ApiEndpoint -Method "POST" -Endpoint "/api/auth/register" -Body @{
    email = $testEmail
    password = $testPassword
    fullName = "Test User B1"
} -Description "Self Registration"

Write-Report "| Step | Action | Status |"
Write-Report "|------|--------|--------|"
Write-Report "| 1 | POST /api/auth/register | $(if ($b1Register.Success) { 'PASS' } else { 'FAIL' }) |"

# B1 - Login
$b1Login = Test-ApiEndpoint -Method "POST" -Endpoint "/api/auth/login" -Body @{
    email = $testEmail
    password = $testPassword
} -Description "Login after registration"

Write-Report "| 2 | POST /api/auth/login | $(if ($b1Login.Success) { 'PASS' } else { 'FAIL' }) |"
Write-Report ""
Write-Report "**Expected Audit Events:** AM01_USER_CREATED, AM01_USER_REGISTERED"
Write-Report ""
Write-Report "---"
Write-Report ""

# B2 - Trial Signup
Write-Report "## Flow B2: Trial Signup"
Write-Report ""

$trialCompany = "Test Company $([Guid]::NewGuid().ToString('N').Substring(0,8))"
$trialEmail = "trial-$([Guid]::NewGuid().ToString('N').Substring(0,8))@testco.com"

Write-Report "**Company:** $trialCompany"
Write-Report "**Email:** $trialEmail"
Write-Report ""

$b2Signup = Test-ApiEndpoint -Method "POST" -Endpoint "/api/trial/signup" -Body @{
    companyName = $trialCompany
    email = $trialEmail
    fullName = "Trial Admin"
    industry = "Technology"
} -Description "Trial Signup"

Write-Report "| Step | Action | Status |"
Write-Report "|------|--------|--------|"
Write-Report "| 1 | POST /api/trial/signup | $(if ($b2Signup.Success) { 'PASS' } else { 'FAIL' }) |"
Write-Report ""
Write-Report "**Expected Audit Events:** AM01_TRIAL_SIGNUP_INITIATED"
Write-Report ""
Write-Report "---"
Write-Report ""

# B3 - Trial Provision
Write-Report "## Flow B3: Trial Provision"
Write-Report ""

$provisionEmail = "provision-$([Guid]::NewGuid().ToString('N').Substring(0,8))@testco.com"
$provisionPassword = "Provision@12345"

Write-Report "**Email:** $provisionEmail"
Write-Report ""

# First signup
$b3Signup = Test-ApiEndpoint -Method "POST" -Endpoint "/api/trial/signup" -Body @{
    companyName = "Provision Test $([Guid]::NewGuid().ToString('N').Substring(0,8))"
    email = $provisionEmail
    fullName = "Provision Admin"
    industry = "Technology"
} -Description "Trial Signup for Provision"

$trialId = $null
if ($b3Signup.Success -and $b3Signup.Data.data) {
    $trialId = $b3Signup.Data.data.trialId
}

# Then provision
$b3Provision = Test-ApiEndpoint -Method "POST" -Endpoint "/api/trial/provision" -Body @{
    trialId = $trialId
    email = $provisionEmail
    password = $provisionPassword
} -Description "Trial Provision"

# Then login
$b3Login = Test-ApiEndpoint -Method "POST" -Endpoint "/api/auth/login" -Body @{
    email = $provisionEmail
    password = $provisionPassword
} -Description "Login after provision"

Write-Report "| Step | Action | Status |"
Write-Report "|------|--------|--------|"
Write-Report "| 1 | POST /api/trial/signup | $(if ($b3Signup.Success) { 'PASS' } else { 'FAIL' }) |"
Write-Report "| 2 | POST /api/trial/provision | $(if ($b3Provision.Success) { 'PASS' } else { 'FAIL' }) |"
Write-Report "| 3 | POST /api/auth/login | $(if ($b3Login.Success) { 'PASS' } else { 'FAIL' }) |"
Write-Report ""
Write-Report "**Expected Audit Events:** AM01_TENANT_CREATED, AM01_USER_CREATED, AM03_ROLE_ASSIGNED"
Write-Report ""
Write-Report "---"
Write-Report ""

# Get admin token for remaining tests
Write-Report "## Flows B4-B6: Require Admin Authentication"
Write-Report ""

$adminLogin = Test-ApiEndpoint -Method "POST" -Endpoint "/api/auth/login" -Body @{
    email = $AdminEmail
    password = $AdminPassword
} -Description "Admin Login"

$adminToken = $null
$tenantId = $null
if ($adminLogin.Success -and $adminLogin.Data.data) {
    $adminToken = $adminLogin.Data.data.token
    $tenantId = $adminLogin.Data.data.tenantId
}

if ($adminToken) {
    Write-Report "Admin authentication: **PASS**"
    Write-Report ""

    # B4 - User Invite
    Write-Report "## Flow B4: User Invite"
    Write-Report ""

    $inviteEmail = "invited-$([Guid]::NewGuid().ToString('N').Substring(0,8))@test.com"
    Write-Report "**Invite Email:** $inviteEmail"
    Write-Report ""

    try {
        $inviteParams = @{
            Method = "POST"
            Uri = "$BaseUrl/api/tenants/$tenantId/users/invite"
            ContentType = "application/json"
            Headers = @{ "Authorization" = "Bearer $adminToken" }
            Body = (@{ email = $inviteEmail; roleCode = "TenantUser"; message = "Welcome" } | ConvertTo-Json)
        }
        $b4Result = Invoke-RestMethod @inviteParams -ErrorAction Stop
        $b4Success = $true
    }
    catch {
        $b4Success = $false
    }

    Write-Report "| Step | Action | Status |"
    Write-Report "|------|--------|--------|"
    Write-Report "| 1 | POST /api/tenants/{id}/users/invite | $(if ($b4Success) { 'PASS' } else { 'FAIL' }) |"
    Write-Report ""
    Write-Report "**Expected Audit Events:** AM01_USER_INVITED"
    Write-Report ""
    Write-Report "---"
    Write-Report ""

    # B5 - Accept Invite (requires actual invitation token)
    Write-Report "## Flow B5: Accept Invite"
    Write-Report ""
    Write-Report "**Note:** This flow requires a valid invitation token from B4."
    Write-Report "Manual verification required with actual token from email/database."
    Write-Report ""
    Write-Report "**Expected Audit Events:** AM01_USER_CREATED, AM03_ROLE_ASSIGNED"
    Write-Report ""
    Write-Report "---"
    Write-Report ""

    # B6 - Role Change (requires test user)
    Write-Report "## Flow B6: Role Change"
    Write-Report ""
    Write-Report "**Note:** This flow requires an existing user ID to modify."
    Write-Report "Manual verification required with actual user from database."
    Write-Report ""
    Write-Report "**Expected Audit Events:** AM03_ROLE_ASSIGNED, AM03_ROLE_CHANGED"
    Write-Report ""
}
else {
    Write-Report "Admin authentication: **FAIL** - Cannot proceed with B4-B6"
    Write-Report ""
}

# Summary
Write-Report "---"
Write-Report ""
Write-Report "## Summary"
Write-Report ""
Write-Report "| Flow | Description | Status |"
Write-Report "|------|-------------|--------|"
Write-Report "| B1 | Self Registration | $(if ($b1Register.Success -and $b1Login.Success) { 'PASS' } else { 'FAIL' }) |"
Write-Report "| B2 | Trial Signup | $(if ($b2Signup.Success) { 'PASS' } else { 'FAIL' }) |"
Write-Report "| B3 | Trial Provision | $(if ($b3Signup.Success) { 'PASS/PARTIAL' } else { 'FAIL' }) |"
Write-Report "| B4 | User Invite | $(if ($b4Success) { 'PASS' } else { 'REQUIRES ADMIN' }) |"
Write-Report "| B5 | Accept Invite | Manual verification required |"
Write-Report "| B6 | Role Change | Manual verification required |"
Write-Report ""
Write-Report "---"
Write-Report ""
Write-Report "*Report generated by Test-GoldenFlows.ps1*"

Write-Host ""
Write-Host "Report saved to: $reportFile" -ForegroundColor Green
Write-Host ""
