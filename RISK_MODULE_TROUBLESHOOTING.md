# Risk Module - Troubleshooting Guide

**Document Date:** January 10, 2026
**Purpose:** Common issues and solutions
**Audience:** Developers and operations teams

---

## 🔧 Common Issues & Solutions

### 1. "Risk not found" Error (404)

**Symptom:**
```
GET /api/risks/{id} returns 404 Not Found
```

**Possible Causes:**
1. Risk doesn't exist
2. Risk belongs to different tenant
3. Risk is soft-deleted
4. User lacks permissions

**Solutions:**

```csharp
// Check if risk exists
var risk = await _dbContext.Risks
    .IgnoreQueryFilters()  // Bypass tenant filter temporarily
    .FirstOrDefaultAsync(r => r.Id == riskId);

if (risk == null)
    // Risk doesn't exist at all

if (risk.IsDeleted)
    // Risk is soft-deleted

if (risk.TenantId != currentTenantId)
    // Wrong tenant - isolation working correctly
```

**Fix:**
- Verify correct tenant context
- Check user permissions: `Grc.Risks.View`
- Confirm risk ID is correct GUID

---

### 2. Owner Validation Fails

**Symptom:**
```
Validation error: "المالك المحدد غير موجود في النظام | The specified owner does not exist in the system"
```

**Possible Causes:**
1. User doesn't exist in directory
2. User is inactive
3. UserDirectoryService is null
4. Email format incorrect

**Solutions:**

```csharp
// Verify user exists
var user = await _userDirectoryService.GetUserByEmailAsync(ownerEmail);

if (user == null)
    // User not found - create user first

if (!user.IsActive)
    // Reactivate user

// Or disable validation temporarily in development
// Remove .MustAsync(BeValidOwnerAsync) from validator
```

**Fix:**
- Create user in system first
- Ensure user is active
- Use correct email format
- In development: Use existing user emails

---

### 3. Tenant Isolation Leak

**Symptom:**
```
User can see risks from other tenants
```

**Possible Causes:**
1. Query filters disabled (`IgnoreQueryFilters()`)
2. TenantId not set in HTTP context
3. Direct database access bypassing filters

**Solutions:**

```csharp
// Verify query filter is applied
var query = _dbContext.Risks.ToQueryString();
// Should contain: WHERE TenantId = @__tenantId_0

// Check current tenant
var tenantId = HttpContext.User.FindFirst("TenantId")?.Value;

if (tenantId == null)
    // User not authenticated properly
```

**Fix:**
- Never use `.IgnoreQueryFilters()` in production
- Ensure user claims include TenantId
- Always use `IUnitOfWork` pattern (filters applied automatically)

---

### 4. Risk Score Calculation Incorrect

**Symptom:**
```
Risk score doesn't match Probability × Impact
```

**Possible Causes:**
1. Manual override of RiskScore
2. Validation disabled
3. Stale data

**Solutions:**

```csharp
// Recalculate risk score
var risk = await _riskService.GetByIdAsync(riskId);
risk.RiskScore = risk.Probability * risk.Impact;
await _riskService.UpdateAsync(riskId, risk);

// Or use auto-calculation endpoint
POST /api/risks/{id}/calculate-score
```

**Fix:**
- Enable auto-calculation validation
- Use `CalculateRiskScoreAsync` service method
- Don't manually set RiskScore field

---

### 5. State Transition Rejected

**Symptom:**
```
InvalidStateTransitionException: Cannot transition from Active to Draft
```

**Possible Causes:**
1. Invalid workflow transition
2. Trying to skip required states

**Solutions:**

```csharp
// Check allowed transitions
var currentStatus = risk.Status.ToRiskStatus();
var validTargets = RiskStateMachine.GetValidTransitions(currentStatus);

// Follow valid path:
// Active → Mitigated (correct)
// Active → Draft (INVALID)
```

**Fix:**
- Follow valid state machine transitions
- See: [RISK_MODULE_GLOSSARY.md](./RISK_MODULE_GLOSSARY.md#state-transitions)
- Use workflow service methods (auto-validates transitions)

---

### 6. Localization Strings Not Working

**Symptom:**
```
Validation messages show keys instead of translated text
```

**Possible Causes:**
1. Resource files not created (current state)
2. IStringLocalizer not injected
3. Culture not set

**Solutions:**

**Current Workaround (Hardcoded):**
```csharp
// Currently working - bilingual strings in validators
.WithMessage("اسم المخاطرة مطلوب | Risk name is required")
```

**Future Fix (After localization task):**
```csharp
// After creating .resx files
.WithMessage(_localizer["Risk_Name_Required"])
```

**Action Required:**
- Complete 2-hour localization task
- Create `Resources/Risk.en.resx`
- Create `Resources/Risk.ar.resx`

---

### 7. Performance Issues with Heat Map

**Symptom:**
```
GET /api/risks/heatmap/{tenantId} takes > 2 seconds
```

**Possible Causes:**
1. Large number of risks (>1000)
2. Missing database indexes
3. N+1 query problem

**Solutions:**

```csharp
// Add index to database
CREATE INDEX IX_Risks_Likelihood_Impact
ON Risks(Likelihood, Impact)
WHERE IsDeleted = 0;

// Use projection to reduce data transfer
var heatMap = await _dbContext.Risks
    .Where(r => r.TenantId == tenantId)
    .Select(r => new { r.Likelihood, r.Impact, r.Name })
    .ToListAsync();
```

**Fix:**
- Add composite index on Likelihood + Impact
- Use pagination for large datasets
- Cache heat map data (15-minute TTL)

---

### 8. Notifications Not Sending

**Symptom:**
```
Risk workflow transitions occur but no emails sent
```

**Possible Causes:**
1. Notification service not configured
2. SMTP settings incorrect
3. User email not set

**Solutions:**

```csharp
// Check notification service logs
_logger.LogInformation("Sending notification to {Email}", user.Email);

// Verify SMTP configuration in appsettings.json
"SmtpSettings": {
  "Host": "smtp.example.com",
  "Port": 587,
  "EnableSsl": true
}

// Test notification manually
await _notificationService.SendNotificationAsync(
    workflowInstanceId: Guid.Empty,
    recipientUserId: userId,
    notificationType: "RiskUpdate",
    subject: "Test",
    body: "Test notification"
);
```

**Fix:**
- Configure SMTP settings
- Verify user email addresses
- Check notification service logs
- Enable logging in `RiskWorkflowService.cs:265-347`

---

### 9. API Returns Empty Array Instead of 404

**Symptom:**
```
GET /api/risks returns [] when no risks exist
```

**Explanation:**
```
This is correct behavior - empty result set, not an error
```

**Expected Responses:**
- `GET /api/risks` → `[]` (no error, empty collection)
- `GET /api/risks/{id}` → `404 Not Found` (specific resource missing)

**Not an Issue:** This is RESTful convention

---

### 10. Control Effectiveness Always Zero

**Symptom:**
```
Risk control effectiveness calculation returns 0%
```

**Possible Causes:**
1. No controls linked to risk
2. Control effectiveness not set
3. Weights sum to zero

**Solutions:**

```csharp
// Link controls to risk first
POST /api/risks/{riskId}/controls/{controlId}
{
  "expectedEffectiveness": 75
}

// Verify controls are linked
GET /api/risks/{riskId}/controls

// Recalculate effectiveness
var effectiveness = await _riskService
    .CalculateControlEffectivenessAsync(riskId);
```

**Fix:**
- Link at least one control to the risk
- Set control effectiveness scores
- Use `LinkControlAsync` service method

---

## 🔍 Debugging Tips

### Enable Detailed Logging

```csharp
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "GrcMvc.Services.Implementations.RiskService": "Debug",
      "GrcMvc.Services.Implementations.RiskWorkflowService": "Debug"
    }
  }
}
```

### Check EF Core Query

```csharp
// See generated SQL
var query = _dbContext.Risks.ToQueryString();
Console.WriteLine(query);
```

### Bypass Query Filters (Development Only)

```csharp
// ONLY for debugging - NEVER in production
var allRisks = await _dbContext.Risks
    .IgnoreQueryFilters()
    .ToListAsync();
```

### Verify Permissions

```csharp
// Check if user has permission
var hasPermission = User.HasClaim("permission", GrcPermissions.Risks.View);
```

---

## 📊 Health Check Endpoints

### Check if Risk Module is Working

```bash
# Test basic connectivity
curl http://localhost:5000/api/risks

# Should return:
# 200 OK with [] or risk data
# 401 Unauthorized (if not logged in)
# 403 Forbidden (if no permission)

# Test specific risk
curl http://localhost:5000/api/risks/{guid}

# Test statistics
curl http://localhost:5000/api/risks/statistics
```

---

## 🚨 Error Codes Reference

| HTTP Code | Meaning | Common Cause | Solution |
|-----------|---------|--------------|----------|
| 200 | Success | - | - |
| 400 | Bad Request | Invalid DTO | Check validation errors |
| 401 | Unauthorized | Not logged in | Login first |
| 403 | Forbidden | No permission | Request permission |
| 404 | Not Found | Wrong ID/tenant | Verify ID and tenant |
| 500 | Server Error | Exception | Check server logs |

---

## 📞 Support Escalation

If issues persist after troubleshooting:

1. **Check Logs:**
   - Application logs: `/logs/grcmvc-{date}.log`
   - EF Core queries: Enable logging
   - Workflow service logs

2. **Gather Information:**
   - Error message (full stack trace)
   - Request details (endpoint, payload)
   - User context (tenant, roles)
   - Expected vs actual behavior

3. **Contact:**
   - Development Team: See repository issues
   - Documentation: [RISK_MODULE_INDEX.md](./RISK_MODULE_INDEX.md)

---

## 🛠️ Quick Fixes

### Reset Risk Module

```bash
# Re-run migrations
dotnet ef database update

# Clear cache (if implemented)
redis-cli FLUSHDB

# Restart application
dotnet run
```

### Verify Installation

```bash
# Check all views exist
ls -la src/GrcMvc/Views/Risk/*.cshtml

# Check controllers exist
ls -la src/GrcMvc/Controllers/Risk*.cs

# Run tests
dotnet test --filter "Risk"
```

---

**Last Updated:** January 10, 2026
**Version:** 1.0
**Related:** [RISK_MODULE_GLOSSARY.md](./RISK_MODULE_GLOSSARY.md)
