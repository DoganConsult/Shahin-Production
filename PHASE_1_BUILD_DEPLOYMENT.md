# PHASE 1 BUILD & DEPLOYMENT CHECKLIST

## Pre-Build Verification

### Database Configuration
- [ ] PostgreSQL connection string verified
- [ ] Database exists or will be created
- [ ] User has proper permissions
- [ ] Backup of existing database created

### Project Structure
- [ ] All new files in correct paths:
  - [ ] Models: `/src/GrcMvc/Models/Phase1Entities.cs`
  - [ ] Interfaces: `/src/GrcMvc/Services/Interfaces/IPhase1Services.cs`
  - [ ] Services: `/src/GrcMvc/Services/Implementations/Phase1*.cs` (3 files)
  - [ ] Migration: `/src/GrcMvc/Migrations/20250115_*.cs`
  - [ ] Program.cs: Updated with DI registrations

### NuGet Dependencies
- [ ] EntityFramework Core (latest)
- [ ] Microsoft.Extensions.Logging (present)
- [ ] System.Linq (standard)

---

## Build Steps

### Step 1: Clean Solution
```bash
cd /home/dogan/grc-system
dotnet clean src/GrcMvc/GrcMvc.csproj
```

### Step 2: Restore Packages
```bash
dotnet restore src/GrcMvc/GrcMvc.csproj
```

### Step 3: Build Project
```bash
dotnet build src/GrcMvc/GrcMvc.csproj -c Release
```

**Expected Output**: ✅ Build succeeded with 0 errors

### Step 4: Run Migrations
```bash
cd src/GrcMvc
dotnet ef database update --context GrcDbContext
```

**Expected Output**: ✅ Applied migration '20250115_Phase1FrameworkHRISAuditTables'

### Step 5: Verify Database
```bash
# Connect to PostgreSQL and verify tables created
\dt  -- List all tables
```

**Expected Tables**:
- ✅ public."Frameworks"
- ✅ public."Controls"
- ✅ public."ControlOwnerships"
- ✅ public."ControlEvidences"
- ✅ public."Baselines"
- ✅ public."BaselineControls"
- ✅ public."HRISIntegrations"
- ✅ public."HRISEmployees"
- ✅ public."AuditLogs"
- ✅ public."ComplianceSnapshots"
- ✅ public."ControlTestResults"

---

## Deployment Verification

### Test Service Initialization
```csharp
// In a test or startup verification
var frameworkService = serviceProvider.GetRequiredService<IFrameworkService>();
var hrisService = serviceProvider.GetRequiredService<IHRISService>();
var auditTrail = serviceProvider.GetRequiredService<IAuditTrailService>();
var rulesEngine = serviceProvider.GetRequiredService<IRulesEngineService>();

// Services should initialize without errors
```

### Test Framework Operations
```csharp
// Test creating a framework
var framework = await frameworkService.CreateFrameworkAsync(
    tenantId: YOUR_TENANT_ID,
    name: "ISO 27001",
    code: "ISO27001",
    description: "Information Security Management"
);

// Verify audit log was created
var auditLogs = await auditTrail.GetEntityAuditHistoryAsync(framework.FrameworkId);
Assert.True(auditLogs.Any(al => al.Action == "Created"));
```

### Test HRIS Operations
```csharp
// Test creating HRIS integration
var integration = await hrisService.CreateIntegrationAsync(
    tenantId: YOUR_TENANT_ID,
    system: "SAP",
    endpoint: "https://sap.example.com/api",
    authType: "OAuth2"
);

// Verify integration was created
var retrieved = await hrisService.GetIntegrationAsync(YOUR_TENANT_ID);
Assert.NotNull(retrieved);
```

### Test Rules Engine
```csharp
// Test framework derivation
var frameworks = await rulesEngine.DeriveApplicableFrameworksAsync(
    tenantId: YOUR_TENANT_ID,
    country: "Saudi Arabia",
    sector: "Finance",
    dataType: "PII"
);

// Should include SAMA, PDPL, SOC2, GDPR, ISO27001
Assert.True(frameworks.Count > 0);
```

---

## Rollback Plan

If build or deployment fails:

### Rollback Steps
```bash
# 1. Revert database migration
cd src/GrcMvc
dotnet ef database update 0  # This will revert all migrations - use previous migration name instead

# 2. Restore from backup
# Use PostgreSQL backup to restore previous state

# 3. Remove new files (if not in source control)
rm src/GrcMvc/Models/Phase1Entities.cs
rm src/GrcMvc/Services/Interfaces/IPhase1Services.cs
rm src/GrcMvc/Services/Implementations/Phase1*.cs
rm src/GrcMvc/Migrations/20250115_*.cs

# 4. Undo Program.cs changes
# Remove Phase 1 service registrations

# 5. Rebuild
dotnet clean && dotnet build
```

---

## Post-Deployment Validation

### Health Check
- [ ] Application starts without errors
- [ ] Services are registered and can be resolved
- [ ] Database connection is active
- [ ] Migration applied successfully
- [ ] All 11 tables exist in database

### Data Validation
- [ ] Can create Framework
- [ ] Can create Control
- [ ] Can create ControlOwnership
- [ ] Can assign Control to user
- [ ] Audit logs are created on every change
- [ ] HRIS integration can be created
- [ ] Rules engine works correctly

### Performance Validation
- [ ] Querying 500+ controls completes in < 100ms
- [ ] HRIS employee sync can handle 100+ employees
- [ ] Audit log queries perform well with indexes
- [ ] Database indexes are properly configured

---

## Monitoring & Logging

### Application Logs to Monitor
```
[INF] Framework created: ISO 27001 for tenant {tenantId}
[INF] Control created: A.5.1 in framework {frameworkId}
[INF] Audit: Created - Framework {frameworkId} by user {userId}
[INF] HRIS integration created: SAP for tenant {tenantId}
```

### Database Logs to Monitor
```sql
-- Monitor audit log entries
SELECT COUNT(*) FROM "AuditLogs" WHERE "Action" = 'Created';

-- Monitor control data
SELECT COUNT(*) FROM "Controls" WHERE "TenantId" = '{tenantId}';

-- Monitor HRIS sync status
SELECT * FROM "HRISIntegrations" WHERE "TenantId" = '{tenantId}';
```

---

## Sign-Off Checklist

- [ ] Build completed successfully
- [ ] Database migration applied
- [ ] All tables created correctly
- [ ] Services are registered
- [ ] Basic functionality tested
- [ ] Audit trail working
- [ ] HRIS integration working
- [ ] Rules engine working
- [ ] Performance acceptable
- [ ] Monitoring configured
- [ ] Backup taken
- [ ] Team notified

---

## Contacts & Escalation

### Issues During Build
- [ ] Check compilation errors
- [ ] Verify NuGet package versions
- [ ] Check database connection string
- [ ] Review migration SQL in output

### Issues During Migration
- [ ] Verify PostgreSQL is running
- [ ] Check user permissions
- [ ] Review migration file for SQL errors
- [ ] Check for existing table conflicts

### Issues During Runtime
- [ ] Check service registration
- [ ] Verify dependency injection
- [ ] Review application logs
- [ ] Check database logs

---

## Timeline

- **Build Time**: ~2 minutes
- **Migration Time**: ~30 seconds
- **Total Deployment**: ~5 minutes
- **Go-Live**: When all validation passes ✅

---

**STATUS**: 🟢 READY FOR BUILD & DEPLOYMENT

Execute build steps above. Phase 1 should be live in < 10 minutes.
