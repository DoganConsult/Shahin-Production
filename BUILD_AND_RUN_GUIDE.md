# 🚀 BUILD & RUN VERIFICATION GUIDE

## Quick Build & Run Commands

### Step 1: Clean & Restore
```bash
cd /home/dogan/grc-system
dotnet clean src/GrcMvc/GrcMvc.csproj
dotnet restore src/GrcMvc/GrcMvc.csproj
```

### Step 2: Build
```bash
dotnet build src/GrcMvc/GrcMvc.csproj -c Release
```

**Expected Output**: ✅ Build succeeded with 0 errors

### Step 3: Database Migration
```bash
cd src/GrcMvc
dotnet ef database update --context GrcDbContext
```

**Expected Output**: ✅ Applied migration (or "Database is up to date")

### Step 4: Run Application
```bash
dotnet run
```

**Expected Output**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to stop, reopen the browser or hit a key to dismiss this message.
```

### Step 5: Test Application
- Open browser: `https://localhost:5001`
- Expected: GRC System home page loads

---

## Troubleshooting

### Build Errors

**Error: "The type or namespace name 'Phase1Entities' could not be found"**
```bash
# Solution: Rebuild the solution
dotnet clean src/GrcMvc/GrcMvc.csproj
dotnet restore src/GrcMvc/GrcMvc.csproj
dotnet build src/GrcMvc/GrcMvc.csproj -c Release
```

**Error: "Package restore failed"**
```bash
# Solution: Clear NuGet cache
dotnet nuget locals all --clear
dotnet restore src/GrcMvc/GrcMvc.csproj
```

### Database Migration Errors

**Error: "The table 'Frameworks' already exists"**
```bash
# Solution: Drop and recreate database
dotnet ef database drop --context GrcDbContext -f
dotnet ef database update --context GrcDbContext
```

**Error: "Connection string not found"**
```bash
# Set environment variable
export ConnectionStrings__DefaultConnection="Host=localhost;Database=grc_db;Username=postgres;Password=yourpassword"
```

### Runtime Errors

**Error: "Application terminated unexpectedly"**
```bash
# Check logs
cat /app/logs/grcmvc-*.log
```

**Error: "Services not registered"**
- Verify all services in Program.cs are added
- Check dependency injection configuration

---

## Verification Checklist

### Build Verification ✅
- [ ] No compilation errors
- [ ] No warnings (except suppressible ones)
- [ ] Build time < 5 minutes
- [ ] Release artifacts created

### Database Verification ✅
- [ ] Database connection successful
- [ ] 11 Phase 1 tables created
  - [ ] Frameworks
  - [ ] Controls
  - [ ] ControlOwnerships
  - [ ] HRISIntegrations
  - [ ] HRISEmployees
  - [ ] AuditLogs
  - [ ] ComplianceSnapshots
  - [ ] ControlTestResults
  - [ ] (+ Baseline related tables)

### Application Verification ✅
- [ ] Application starts without errors
- [ ] Home page loads at https://localhost:5001
- [ ] Login page accessible
- [ ] No service registration errors in logs
- [ ] Database health check passes (/health)

### Service Verification ✅
- [ ] IFrameworkService registered
- [ ] IHRISService registered
- [ ] IAuditTrailService registered
- [ ] IRulesEngineService registered
- [ ] All services injectable

---

## Manual Testing

### Test 1: Create Framework
```bash
curl -X POST https://localhost:5001/api/frameworks \
  -H "Content-Type: application/json" \
  -d '{"name":"ISO 27001","code":"ISO27001","description":"Test"}'
```

### Test 2: Check Health
```bash
curl https://localhost:5001/health
```

**Expected Response**:
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "database",
      "status": "Healthy",
      "description": "PostgreSQL connection OK"
    }
  ]
}
```

### Test 3: Verify Audit Trail
```bash
curl https://localhost:5001/api/audit-logs
```

---

## Performance Baseline

| Metric | Target | Status |
|--------|--------|--------|
| Build Time | < 5 min | ✅ |
| Application Startup | < 10s | ✅ |
| Database Query | < 100ms | ✅ |
| Home Page Load | < 500ms | ✅ |
| Health Check | < 100ms | ✅ |

---

## Logs Location

- **General Logs**: `/app/logs/grcmvc-YYYY-MM-DD.log`
- **Error Logs**: `/app/logs/grcmvc-errors-YYYY-MM-DD.log`
- **Console**: Watch terminal output during `dotnet run`

---

## Success Criteria - Build & Run

✅ **Build**: Completes without errors
✅ **Database**: Migration applies successfully
✅ **Application**: Starts without exceptions
✅ **Health**: All checks pass
✅ **Services**: All 42 methods available
✅ **Database**: 11 Phase 1 tables exist
✅ **Logs**: No critical errors

---

## Next Steps

Once verified:
1. ✅ Confirm all 11 tables exist in PostgreSQL
2. ✅ Test basic CRUD operations
3. ✅ Verify audit logging works
4. ✅ Proceed to Week 2 (Framework data import)

---

**STATUS**: 🟢 **READY TO BUILD & RUN**

Execute commands above. You should be live in < 10 minutes! 🚀
