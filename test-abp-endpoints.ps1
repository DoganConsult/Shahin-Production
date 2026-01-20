# Test ABP Tenant & Admin Creation Endpoints
# This script tests the new ABP-based endpoints

Write-Host "🧪 Testing ABP Tenant & Admin Creation Endpoints" -ForegroundColor Cyan
Write-Host "=" * 60 -ForegroundColor Cyan

$baseUrl = "http://localhost:5010"
if ($env:ASPNETCORE_URLS) {
    $baseUrl = ($env:ASPNETCORE_URLS -split ';')[0]
}

Write-Host "`nBase URL: $baseUrl" -ForegroundColor Yellow

# Test 1: Agent Tenant Creation API (ABP Way)
Write-Host "`n📋 Test 1: POST /api/agent/tenant/create (ABP Way)" -ForegroundColor Green
$testEmail = "test-$(Get-Date -Format 'yyyyMMddHHmmss')@example.com"
$testOrg = "Test Org $(Get-Date -Format 'HHmmss')"

$body = @{
    organizationName = $testOrg
    adminEmail = $testEmail
    adminPassword = "TestPass123!"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/agent/tenant/create" `
        -Method POST `
        -Body $body `
        -ContentType "application/json" `
        -ErrorAction Stop
    
    Write-Host "✅ SUCCESS" -ForegroundColor Green
    Write-Host "   Tenant ID: $($response.tenantId)" -ForegroundColor White
    Write-Host "   Admin User ID: $($response.adminUserId)" -ForegroundColor White
    Write-Host "   Organization: $($response.organizationName)" -ForegroundColor White
    Write-Host "   Redirect URL: $($response.redirectUrl)" -ForegroundColor White
    
    $tenantId = $response.tenantId
    $adminUserId = $response.adminUserId
} catch {
    Write-Host "❌ FAILED" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "   Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
    exit 1
}

# Test 2: Get Tenant Details
Write-Host "`n📋 Test 2: GET /api/agent/tenant/{tenantId}" -ForegroundColor Green
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/agent/tenant/$tenantId" `
        -Method GET `
        -ErrorAction Stop
    
    Write-Host "✅ SUCCESS" -ForegroundColor Green
    Write-Host "   Tenant ID: $($response.tenantId)" -ForegroundColor White
    Write-Host "   Organization: $($response.organizationName)" -ForegroundColor White
    Write-Host "   Admin User ID: $($response.adminUserId)" -ForegroundColor White
} catch {
    Write-Host "❌ FAILED" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Tenants API with Password (ABP Way)
Write-Host "`n📋 Test 3: POST /api/tenants (with password - ABP Way)" -ForegroundColor Green
$testEmail2 = "test2-$(Get-Date -Format 'yyyyMMddHHmmss')@example.com"
$testOrg2 = "Test Org 2 $(Get-Date -Format 'HHmmss')"

$body2 = @{
    organizationName = $testOrg2
    adminEmail = $testEmail2
    adminPassword = "TestPass123!"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/tenants" `
        -Method POST `
        -Body $body2 `
        -ContentType "application/json" `
        -ErrorAction Stop
    
    Write-Host "✅ SUCCESS" -ForegroundColor Green
    Write-Host "   Tenant ID: $($response.tenantId)" -ForegroundColor White
    Write-Host "   Admin User ID: $($response.adminUserId)" -ForegroundColor White
    Write-Host "   Organization: $($response.organizationName)" -ForegroundColor White
} catch {
    Write-Host "❌ FAILED" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "   Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
}

# Test 4: Validation Test (Invalid Password)
Write-Host "`n📋 Test 4: POST /api/agent/tenant/create (Invalid Password)" -ForegroundColor Green
$body3 = @{
    organizationName = "Test Org"
    adminEmail = "test@example.com"
    adminPassword = "short"  # Too short
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/agent/tenant/create" `
        -Method POST `
        -Body $body3 `
        -ContentType "application/json" `
        -ErrorAction Stop
    
    Write-Host "❌ FAILED - Should have rejected short password" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "✅ SUCCESS - Correctly rejected invalid password" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Unexpected error: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

Write-Host "`n" + ("=" * 60) -ForegroundColor Cyan
Write-Host "✅ Testing Complete!" -ForegroundColor Green
Write-Host "`nNext Steps:" -ForegroundColor Yellow
Write-Host "1. Check database for created tenants in AbpTenants table" -ForegroundColor White
Write-Host "2. Check database for created users in AbpUsers table" -ForegroundColor White
Write-Host "3. Verify tenant context isolation (users are tenant-scoped)" -ForegroundColor White
Write-Host "4. Test login with created admin credentials" -ForegroundColor White
