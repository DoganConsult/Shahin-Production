# Local Deployment & Testing Guide

## ✅ Pre-Deployment Verification

### 1. Build Status
```bash
✅ Build: SUCCESS (0 Warnings, 0 Errors)
✅ Tests: 117 tests PASSED
✅ Migrations: Applied successfully
```

### 2. Database Status
```bash
✅ Database: PostgreSQL running (grc-db container)
✅ Port: 5433 (mapped from 5432)
✅ Health: Healthy
```

### 3. Configuration
```bash
✅ .env file: EXISTS
✅ Connection String: Configured for PostgreSQL
✅ Development settings: Available
```

---

## 🚀 Local Deployment Steps

### Step 1: Start Database (if not running)
```bash
cd /home/dogan/grc-system
docker-compose up -d db
```

### Step 2: Apply Migrations
```bash
export PATH="$PATH:/usr/share/dotnet:$HOME/.dotnet/tools"
export ConnectionStrings__DefaultConnection="Host=localhost;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5432"

cd src/GrcMvc
dotnet ef database update
```

### Step 3: Run Application
```bash
cd /home/dogan/grc-system/src/GrcMvc
export ConnectionStrings__DefaultConnection="Host=localhost;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5432"
export ASPNETCORE_ENVIRONMENT=Development

dotnet run
```

### Step 4: Verify Application Starts
- Application should start on: `http://localhost:5000` or `https://localhost:5001`
- Check logs for:
  - ✅ "Starting application initialization..."
  - ✅ "Seeding catalogs..."
  - ✅ "Seeding workflow definitions..."
  - ✅ "Seeding RBAC system..."
  - ✅ "Seeding users..."
  - ✅ "Application initialization completed successfully"

### Step 5: Verify Seeding
After application starts, verify seeding worked:

```bash
# Connect to database
psql -h localhost -p 5433 -U postgres -d GrcMvcDb

# Check regulators
SELECT COUNT(*) FROM "RegulatorCatalogs"; -- Should be ~91

# Check frameworks
SELECT COUNT(*) FROM "FrameworkCatalogs"; -- Should be ~162

# Check controls
SELECT COUNT(*) FROM "ControlCatalogs"; -- Should be ~57,211

# Check workflow definitions
SELECT COUNT(*) FROM "WorkflowDefinitions"; -- Should be 7

# Check users
SELECT COUNT(*) FROM "AspNetUsers"; -- Should be at least 1 (admin)

# Check roles
SELECT COUNT(*) FROM "AspNetRoles"; -- Should be multiple roles
```

---

## 🧪 Testing Checklist

### Build & Compilation
- [x] ✅ Build succeeds (0 errors, 0 warnings)
- [x] ✅ All tests pass (117 tests)
- [x] ✅ No linter errors

### Database
- [x] ✅ Migrations applied
- [ ] ⏳ Database connection works
- [ ] ⏳ Seeding executes on startup
- [ ] ⏳ All tables created

### Seeding Verification
- [ ] ⏳ Regulators seeded (91 expected)
- [ ] ⏳ Frameworks seeded (162 expected)
- [ ] ⏳ Controls seeded (57,211 expected)
- [ ] ⏳ Workflow definitions seeded (7 expected)
- [ ] ⏳ RBAC system seeded
- [ ] ⏳ Users seeded (admin user)

### Application Startup
- [ ] ⏳ Application starts without errors
- [ ] ⏳ Health check endpoint responds
- [ ] ⏳ Database connection established
- [ ] ⏳ Seeding completes successfully

### Key Functionality
- [ ] ⏳ Login works
- [ ] ⏳ Dashboard loads
- [ ] ⏳ Menu items visible (RBAC-based)
- [ ] ⏳ Workflows accessible
- [ ] ⏳ API endpoints respond

---

## 📊 Expected Seeding Results

| Category | Expected Count | Status |
|----------|----------------|--------|
| Regulators | 91 | ⏳ Pending verification |
| Frameworks | 162 | ⏳ Pending verification |
| Controls | 57,211 | ⏳ Pending verification |
| Workflow Definitions | 7 | ⏳ Pending verification |
| Role Profiles | 15 | ⏳ Pending verification |
| Permissions | 40+ | ⏳ Pending verification |
| Features | 12 | ⏳ Pending verification |
| Identity Roles | Multiple | ⏳ Pending verification |
| Users | 1+ (admin) | ⏳ Pending verification |

---

## 🔍 Troubleshooting

### If Application Fails to Start

1. **Check Database Connection**
   ```bash
   psql -h localhost -p 5433 -U postgres -d GrcMvcDb -c "SELECT 1;"
   ```

2. **Check Connection String**
   ```bash
   echo $ConnectionStrings__DefaultConnection
   ```

3. **Check Logs**
   ```bash
   tail -f logs/grc-system-*.log
   ```

### If Seeding Fails

1. **Check CSV Files Exist**
   ```bash
   ls -la src/GrcMvc/Models/Entities/Catalogs/*.csv
   ```

2. **Check ApplicationInitializer Logs**
   - Look for "Starting application initialization..."
   - Check for any error messages

3. **Manually Trigger Seeding**
   ```bash
   # Via API (if available)
   curl -X POST http://localhost:5000/api/seed/catalogs
   ```

---

## ⚠️ Current Status

**NOT YET PRODUCTION READY** - Pending:
- [ ] Application startup verification
- [ ] Seeding verification
- [ ] Database connectivity test
- [ ] Functional testing
- [ ] Trial run completion

**Next Step**: Run the application and verify all seeding completes successfully.

---

**Last Updated**: 2026-01-22
**Status**: ⏳ **PENDING VERIFICATION**
