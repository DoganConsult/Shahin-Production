# Integration Test Plan - GRC Platform
Generated: January 17, 2026

## Executive Summary
This plan outlines the comprehensive testing approach for all implemented features before production deployment.

## Current Implementation Status

### ✅ Completed Features
1. **Database Migration System**
   - MigrateAsync() implementation
   - Timeout/retry configuration (300s timeout, 3 retries)
   - Health check verification
   - Separate Auth and Main databases

2. **ABP Settings Management**
   - Settings definition provider
   - Connection string management
   - Security settings configuration
   - Application settings UI

3. **Identity System**
   - ApplicationUser with custom columns
   - Password policies
   - Lockout configuration
   - Registration with Terms acceptance

4. **Middleware Pipeline**
   - OwnerSetupMiddleware
   - TenantResolutionMiddleware
   - Authentication/Authorization

## Phase 1: Database Verification (Priority: HIGH)

### Test 1.1: Database Connection
```bash
# Verify PostgreSQL container is running
docker ps | grep shahin-postgres

# Test connection
docker exec shahin-postgres psql -U shahin_admin -d shahin_grc -c "\dt"
```

**Expected Result:**
- Container running
- Tables listed successfully

### Test 1.2: Migration History
```sql
-- Check main database migrations
SELECT * FROM "__EFMigrationsHistory";

-- Check auth database migrations
\c shahin_grc
SELECT * FROM "__EFMigrationsHistory" WHERE "MigrationId" LIKE '%Auth%';
```

**Expected Result:**
- All migrations listed
- No pending migrations

### Test 1.3: ApplicationUser Schema
```sql
-- Verify ApplicationUser columns exist
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'AspNetUsers'
ORDER BY ordinal_position;
```

**Expected Columns:**
- Id, UserName, Email (standard Identity)
- FirstName, LastName, Department, JobTitle (custom)
- Abilities, AssignedScope, KnowledgeAreas, Skills
- RoleProfileId, KsaCompetencyLevel
- MustChangePassword, LastPasswordChangedAt
- RefreshToken, RefreshTokenExpiry

## Phase 2: Application Startup (Priority: HIGH)

### Test 2.1: Clean Startup
```bash
cd c:\Shahin-ai\Shahin-Jan-2026
dotnet build
dotnet run --project src/GrcMvc/GrcMvc.csproj
```

**Verification Points:**
- [ ] No migration errors
- [ ] Health check passes
- [ ] Seeding completes
- [ ] Application starts on port 5000/8080

### Test 2.2: Migration Health Check
Monitor logs for:
```
✅ Main database: X migrations applied
✅ Auth database: Y migrations applied
✅ Main database: Z tables exist
✅ Auth database: ApplicationUser custom columns verified
```

## Phase 3: Settings Management UI (Priority: MEDIUM)

### Test 3.1: Access Control
1. Login as Admin
2. Navigate to Manage → Settings
3. Verify page loads

**Test Cases:**
- [ ] Admin can access
- [ ] Non-admin redirected
- [ ] Form displays all sections

### Test 3.2: Connection Strings
1. View masked connection strings
2. Toggle visibility
3. Update a connection string
4. Save changes

**Verification:**
- [ ] Password fields masked
- [ ] Toggle button works
- [ ] Save shows success message
- [ ] Values persisted

### Test 3.3: Security Settings
1. Change password policy
2. Toggle registration
3. Adjust lockout settings

**Test Matrix:**
| Setting | Test Value | Expected |
|---------|------------|----------|
| AllowPublicRegistration | true/false | Registration page enabled/disabled |
| PasswordMinLength | 12 | Validation enforced |
| LockoutDuration | 30 | User locked for 30 min |

### Test 3.4: Application Settings
1. Change application name
2. Enable maintenance mode
3. Adjust log level

**Verification:**
- [ ] Name reflects in UI
- [ ] Maintenance page shown when enabled
- [ ] Log verbosity changes

## Phase 4: Registration Flow (Priority: HIGH)

### Test 4.1: Owner Setup
1. Start with fresh database
2. Access application
3. Complete owner setup

**Steps:**
```
1. Navigate to /OwnerSetup
2. Fill form:
   - FirstName: Test
   - LastName: Owner
   - Email: owner@test.com
   - Password: Test123!@#$
3. Submit
```

**Expected:**
- [ ] Owner created
- [ ] Admin role assigned
- [ ] Redirected to dashboard

### Test 4.2: Public Registration
1. Enable public registration in settings
2. Navigate to /Account/Register
3. Complete registration

**Test Data:**
```json
{
  "FirstName": "Test",
  "LastName": "User",
  "Email": "user@test.com",
  "Department": "IT",
  "JobTitle": "Developer",
  "Password": "User123!@#$",
  "AcceptTerms": true
}
```

**Verification:**
- [ ] Terms checkbox required
- [ ] All ApplicationUser fields saved
- [ ] User can login
- [ ] Profile shows custom fields

## Phase 5: ABP Integration (Priority: MEDIUM)

### Test 5.1: Setting Providers
```csharp
// Test in controller
var settingManager = services.GetService<ISettingManager>();
var value = await settingManager.GetOrNullGlobalAsync("GrcMvc.App.ApplicationName");
```

**Expected:**
- Returns "Shahin GRC Platform"

### Test 5.2: Setting Updates
```csharp
await settingManager.SetForGlobalAsync("GrcMvc.App.MaintenanceMode", "true");
var isEnabled = await settingManager.GetOrNullGlobalAsync("GrcMvc.App.MaintenanceMode");
```

**Expected:**
- Setting persisted
- Value retrievable

## Phase 6: End-to-End Scenarios

### Scenario 1: Fresh Installation
1. Drop all tables
2. Run application
3. Complete owner setup
4. Create test users
5. Configure settings

### Scenario 2: Upgrade Path
1. Backup database
2. Add new migration
3. Run application
4. Verify migration applied
5. Test rollback

### Scenario 3: Multi-Tenant
1. Create tenant
2. Switch context
3. Verify isolation
4. Test settings per tenant

## Phase 7: Performance Testing

### Test 7.1: Migration Timeout
```sql
-- Create large test data
INSERT INTO "TestTable" SELECT generate_series(1,1000000);
```

**Verification:**
- Migration completes within 300s
- Retry logic activates on failure

### Test 7.2: Concurrent Access
```bash
# Simulate 100 concurrent users
for i in {1..100}; do
  curl http://localhost:5000/Settings &
done
```

## Phase 8: Error Handling

### Test 8.1: Database Unavailable
1. Stop PostgreSQL container
2. Start application
3. Verify graceful failure

### Test 8.2: Invalid Migration
1. Create corrupt migration
2. Run application
3. Verify rollback procedure

## Test Execution Checklist

### Pre-Test Setup
- [ ] Docker running
- [ ] PostgreSQL container healthy
- [ ] Clean build successful
- [ ] Test data prepared

### Test Execution Order
1. [ ] Phase 1: Database Verification
2. [ ] Phase 2: Application Startup
3. [ ] Phase 4: Registration Flow (before settings)
4. [ ] Phase 3: Settings Management UI
5. [ ] Phase 5: ABP Integration
6. [ ] Phase 6: End-to-End Scenarios
7. [ ] Phase 7: Performance Testing
8. [ ] Phase 8: Error Handling

### Post-Test Validation
- [ ] All tests passed
- [ ] No data corruption
- [ ] Performance acceptable
- [ ] Logs clean

## Rollback Procedures

### If Migration Fails
```bash
# Revert last migration
dotnet ef database update PreviousMigration --context GrcDbContext

# Or manual SQL
docker exec shahin-postgres psql -U shahin_admin -d shahin_grc -c "
DELETE FROM \"__EFMigrationsHistory\" 
WHERE \"MigrationId\" = 'LastMigrationId';
"
```

### If Settings Corrupt
```sql
-- Reset to defaults
TRUNCATE TABLE "AbpSettings";
-- Restart application to repopulate
```

## Success Criteria

### Mandatory (Must Pass)
- ✅ Database migrations apply successfully
- ✅ ApplicationUser columns present
- ✅ Registration flow works
- ✅ Settings UI accessible by admin
- ✅ No startup errors

### Recommended (Should Pass)
- ✅ Performance under load
- ✅ Graceful error handling
- ✅ Rollback procedures work
- ✅ All custom fields persist

### Optional (Nice to Have)
- ✅ Multi-tenant isolation
- ✅ Advanced monitoring
- ✅ Automated test suite

## Test Report Template

```markdown
## Test Execution Report
Date: [DATE]
Tester: [NAME]
Environment: [DEV/STAGING/PROD]

### Phase Results
| Phase | Status | Issues | Notes |
|-------|--------|--------|-------|
| 1. Database | ✅/❌ | | |
| 2. Startup | ✅/❌ | | |
| 3. Settings | ✅/❌ | | |
| 4. Registration | ✅/❌ | | |
| 5. ABP | ✅/❌ | | |
| 6. E2E | ✅/❌ | | |
| 7. Performance | ✅/❌ | | |
| 8. Errors | ✅/❌ | | |

### Critical Issues
1. [Issue description]
   - Impact: [HIGH/MEDIUM/LOW]
   - Resolution: [Action taken]

### Recommendations
- [Recommendation 1]
- [Recommendation 2]

### Sign-off
- [ ] Dev Lead
- [ ] QA Lead
- [ ] Product Owner
```

## Next Steps

1. **Execute Phase 1** - Verify database state
2. **Run Phase 2** - Test application startup
3. **Complete Phase 4** - Test registration first
4. **Continue sequentially** through remaining phases
5. **Document results** using template
6. **Address any issues** before production

## Notes

- Always backup before testing
- Test in isolated environment first
- Document all deviations
- Keep logs for audit trail
- Escalate blockers immediately

---
End of Integration Test Plan
