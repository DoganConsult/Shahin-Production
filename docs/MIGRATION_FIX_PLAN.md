# Database Migration Fix - Detailed Implementation Plan

**Date:** 2026-01-16  
**Status:** 📋 **PLAN READY FOR IMPLEMENTATION**  
**Priority:** 🔴 **CRITICAL**  
**Estimated Time:** 2-3 hours

---

## Executive Summary

This plan fixes the database migration hang/not completing issue by:
1. Replacing `EnsureCreated()` with `Migrate()` for proper migration tracking
2. Adding connection timeout and retry configuration
3. Implementing proper error handling and progress reporting
4. Following Identity Schema Safeguards for Auth DB
5. Adding verification and rollback procedures

---

## Phase 1: Pre-Implementation Verification

### Step 1.1: Check Current Database State

**Action:** Verify what state the databases are in before making changes.

```bash
# Connect to PostgreSQL
psql -U postgres -d GrcMvcDb

# Check migration history (should be empty or incomplete)
SELECT COUNT(*) as migration_count FROM "__EFMigrationsHistory";

# List all tables (should be ~230+)
SELECT COUNT(*) as table_count 
FROM information_schema.tables 
WHERE table_schema = 'public' AND table_type = 'BASE TABLE';

# Check if __EFMigrationsHistory table exists
SELECT EXISTS (
    SELECT FROM information_schema.tables 
    WHERE table_schema = 'public' 
    AND table_name = '__EFMigrationsHistory'
);

# Exit
\q
```

**Expected Results:**
- If `migration_count = 0`: `EnsureCreated()` was used (no history)
- If `table_count < 230`: Incomplete schema
- If `__EFMigrationsHistory` doesn't exist: Confirms `EnsureCreated()` was used

**Document:** Record results in `docs/MIGRATION_FIX_PLAN.md` under "Pre-Implementation State"

---

### Step 1.2: Check Auth Database Schema

**Action:** Verify ApplicationUser custom columns exist.

```bash
psql -U postgres -d GrcAuthDb

# Check ApplicationUser custom columns
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'AspNetUsers'
AND column_name IN ('FirstName', 'LastName', 'Abilities', 'Department', 'JobTitle', 'KsaCompetencyLevel')
ORDER BY column_name;

# Check migration history
SELECT COUNT(*) FROM "__EFMigrationsHistory";
```

**Expected Results:**
- **If columns missing:** `EnsureCreated()` was used (incomplete schema)
- **If migration_count = 0:** No migration history

**Document:** Record missing columns

---

### Step 1.3: Backup Current Databases

**Action:** Create full backups before making changes.

```bash
# Create backup directory
mkdir -p backups/migration-fix-$(date +%Y%m%d)

# Backup main database
pg_dump -U postgres -d GrcMvcDb -F c -f backups/migration-fix-$(date +%Y%m%d)/GrcMvcDb_$(date +%Y%m%d_%H%M%S).backup

# Backup auth database
pg_dump -U postgres -d GrcAuthDb -F c -f backups/migration-fix-$(date +%Y%m%d)/GrcAuthDb_$(date +%Y%m%d_%H%M%S).backup

# Verify backups
ls -lh backups/migration-fix-$(date +%Y%m%d)/
```

**Success Criteria:**
- ✅ Both backup files created
- ✅ Backup file size > 0
- ✅ Backup location documented

---

## Phase 2: Code Changes

### Step 2.1: Update DbContext Registration with Timeout

**File:** `src/GrcMvc/Extensions/ServiceCollectionExtensions.cs`

**Current Code (Line 77-78):**
```csharp
services.AddDbContext<GrcAuthDbContext>(options =>
    options.UseNpgsql(authConnectionString), ServiceLifetime.Scoped);
```

**Change Required:**
```csharp
services.AddDbContext<GrcAuthDbContext>(options =>
{
    options.UseNpgsql(authConnectionString, npgsqlOptions =>
    {
        // Command timeout for migrations (large schema needs more time)
        npgsqlOptions.CommandTimeout(300); // 5 minutes for migration operations
        
        // Retry logic for transient failures
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    });
}, ServiceLifetime.Scoped);
```

**Also Check:** `GrcDbContext` registration in `GrcMvcAbpModule.cs`

**Action:**
1. Locate `AddAbpDbContext<GrcDbContext>` registration
2. Add timeout and retry configuration similar to above
3. Verify connection string is properly configured

**Verification:**
- ✅ Timeout set to 300 seconds (5 minutes)
- ✅ Retry enabled with 3 attempts
- ✅ Both contexts configured

---

### Step 2.2: Fix Migration Method in WebApplicationExtensions

**File:** `src/GrcMvc/Extensions/WebApplicationExtensions.cs`

**Current Code (Lines 222-258):**
```csharp
public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
{
    // ... uses EnsureCreated() - WRONG
}
```

**Replace With:**
```csharp
/// <summary>
/// Apply database migrations and seed data
/// CRITICAL: Always use Migrate() not EnsureCreated() to track migration history
/// </summary>
public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var dbContext = services.GetRequiredService<GrcDbContext>();
        var authContext = services.GetRequiredService<GrcAuthDbContext>();

        // ===================================================================
        // MAIN DATABASE (GrcDbContext)
        // ===================================================================
        if (app.Environment.IsProduction())
        {
            logger.LogInformation("🔄 Applying database migrations (Production)...");
            
            // Check for pending migrations
            var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
            if (pendingMigrations.Any())
            {
                logger.LogInformation("📦 Found {Count} pending migrations for main database", pendingMigrations.Count);
                logger.LogInformation("Pending migrations: {Migrations}", string.Join(", ", pendingMigrations));
                
                // Apply migrations with timeout handling
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("✅ Main database migrations applied successfully");
            }
            else
            {
                // No pending migrations - verify database exists
                var canConnect = await dbContext.Database.CanConnectAsync();
                if (canConnect)
                {
                    logger.LogInformation("✅ Main database schema is up-to-date (no pending migrations)");
                }
                else
                {
                    logger.LogWarning("⚠️ Database exists but cannot connect - may need manual intervention");
                }
            }
        }
        else
        {
            // DEVELOPMENT: Use Migrate() for consistency (not EnsureCreated)
            logger.LogInformation("🔄 Applying database migrations (Development)...");
            
            var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
            if (pendingMigrations.Any())
            {
                logger.LogInformation("📦 Found {Count} pending migrations", pendingMigrations.Count);
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("✅ Development database migrations applied");
            }
            else
            {
                logger.LogInformation("✅ Development database is up-to-date");
            }
        }

        // ===================================================================
        // AUTH DATABASE (GrcAuthDbContext)
        // CRITICAL: ALWAYS use Migrate() - never EnsureCreated()
        // See: docs/IDENTITY_SCHEMA_SAFEGUARDS.md
        // ===================================================================
        logger.LogInformation("🔄 Applying Auth database migrations...");
        
        var authPendingMigrations = authContext.Database.GetPendingMigrations().ToList();
        if (authPendingMigrations.Any())
        {
            logger.LogInformation("📦 Found {Count} pending migrations for Auth database", authPendingMigrations.Count);
            logger.LogInformation("Pending migrations: {Migrations}", string.Join(", ", authPendingMigrations));
            
            await authContext.Database.MigrateAsync();
            logger.LogInformation("✅ Auth database migrations applied successfully");
        }
        else
        {
            // Verify ApplicationUser schema is complete
            var canConnect = await authContext.Database.CanConnectAsync();
            if (canConnect)
            {
                // Check if critical columns exist (quick validation)
                try
                {
                    var hasCustomColumns = await authContext.Database.ExecuteSqlRawAsync(
                        "SELECT 1 FROM information_schema.columns " +
                        "WHERE table_name = 'AspNetUsers' AND column_name = 'FirstName'");
                    
                    if (hasCustomColumns > 0)
                    {
                        logger.LogInformation("✅ Auth database schema verified (custom columns present)");
                    }
                    else
                    {
                        logger.LogWarning("⚠️ Auth database exists but missing custom columns - may need manual migration");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "⚠️ Could not verify Auth database schema - proceeding anyway");
                }
            }
            else
            {
                logger.LogWarning("⚠️ Auth database cannot connect - may need manual intervention");
            }
        }
    }
    catch (NpgsqlException ex) when (ex.SqlState == "57P01" || ex.SqlState == "57P03")
    {
        // Database connection errors
        logger.LogError(ex, "❌ Database connection failed during migration. Check connection string and database availability.");
        throw;
    }
    catch (TimeoutException ex)
    {
        // Command timeout
        logger.LogError(ex, "❌ Migration timed out. Database may be locked or schema too large. Consider increasing CommandTimeout.");
        throw;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Database migration failed: {Message}", ex.Message);
        throw;
    }
}
```

**Key Changes:**
1. ✅ Replaced `EnsureCreated()` with `MigrateAsync()`
2. ✅ Added pending migration check before applying
3. ✅ Added logging for pending migrations
4. ✅ Added schema verification for Auth DB
5. ✅ Added specific exception handling (connection, timeout)
6. ✅ Added progress reporting

**Verification:**
- ✅ No `EnsureCreated()` calls remain
- ✅ Both contexts use `MigrateAsync()`
- ✅ Auth DB always uses `Migrate()` (per safeguards)
- ✅ Error handling covers all failure scenarios

---

### Step 2.3: Add Migration Health Check

**File:** `src/GrcMvc/Extensions/WebApplicationExtensions.cs` (add new method)

**Add After `ApplyDatabaseMigrationsAsync`:**

```csharp
/// <summary>
/// Verify migration health after application startup
/// </summary>
public static async Task VerifyMigrationHealthAsync(this WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var dbContext = services.GetRequiredService<GrcDbContext>();
        var authContext = services.GetRequiredService<GrcAuthDbContext>();

        // Check main DB
        var mainMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
        logger.LogInformation("✅ Main database: {Count} migrations applied", mainMigrations.Count());

        // Check auth DB
        var authMigrations = await authContext.Database.GetAppliedMigrationsAsync();
        logger.LogInformation("✅ Auth database: {Count} migrations applied", authMigrations.Count());

        // Verify critical tables exist
        var mainTableCount = await dbContext.Database.ExecuteSqlRawAsync(
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE'");
        logger.LogInformation("✅ Main database: {Count} tables exist", mainTableCount);

        // Verify ApplicationUser custom columns
        var hasFirstName = await authContext.Database.ExecuteSqlRawAsync(
            "SELECT COUNT(*) FROM information_schema.columns WHERE table_name = 'AspNetUsers' AND column_name = 'FirstName'");
        
        if (hasFirstName > 0)
        {
            logger.LogInformation("✅ Auth database: ApplicationUser custom columns verified");
        }
        else
        {
            logger.LogWarning("⚠️ Auth database: ApplicationUser custom columns missing - migration may be incomplete");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Migration health check failed");
        // Don't throw - this is just verification
    }
}
```

**Update Program.cs:**
```csharp
// After ApplyDatabaseMigrationsAsync()
await app.ApplyDatabaseMigrationsAsync();
await app.VerifyMigrationHealthAsync(); // Add this line
```

---

## Phase 3: Testing & Verification

### Step 3.1: Test Migration on Clean Database

**Action:** Test the fix on a fresh database.

```bash
# Create test databases
createdb -U postgres GrcMvcDb_test
createdb -U postgres GrcAuthDb_test

# Update connection strings temporarily
export ConnectionStrings__DefaultConnection="Host=localhost;Database=GrcMvcDb_test;Username=postgres;Password=postgres;Port=5432"
export ConnectionStrings__GrcAuthDb="Host=localhost;Database=GrcAuthDb_test;Username=postgres;Password=postgres;Port=5432"

# Run application
cd src/GrcMvc
dotnet run

# Check logs for:
# - "Applying database migrations..."
# - "Found X pending migrations"
# - "✅ Main database migrations applied successfully"
# - "✅ Auth database migrations applied successfully"
```

**Success Criteria:**
- ✅ Application starts without hanging
- ✅ All migrations applied
- ✅ Migration history table populated
- ✅ All tables created (230+ for main, 7+ for auth)
- ✅ ApplicationUser has custom columns

---

### Step 3.2: Test Migration on Existing Database

**Action:** Test migration on existing database (simulates production).

```bash
# Use existing databases
export ConnectionStrings__DefaultConnection="Host=localhost;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5432"
export ConnectionStrings__GrcAuthDb="Host=localhost;Database=GrcAuthDb;Username=postgres;Password=postgres;Port=5432"

# Run application
cd src/GrcMvc
dotnet run

# Check logs for:
# - "Main database schema is up-to-date" OR "Found X pending migrations"
# - No hanging or timeout errors
```

**Success Criteria:**
- ✅ Application starts without hanging
- ✅ Detects existing schema correctly
- ✅ Applies only pending migrations (if any)
- ✅ No errors or warnings

---

### Step 3.3: Verify Migration History

**Action:** Confirm migration history is tracked.

```sql
-- Main database
SELECT "MigrationId", "ProductVersion" 
FROM "__EFMigrationsHistory" 
ORDER BY "MigrationId";

-- Auth database
SELECT "MigrationId", "ProductVersion" 
FROM "__EFMigrationsHistory" 
ORDER BY "MigrationId";
```

**Success Criteria:**
- ✅ `__EFMigrationsHistory` table exists in both databases
- ✅ Lists all applied migrations
- ✅ Migration IDs match migration files in `Migrations/` folder

---

### Step 3.4: Verify Schema Completeness

**Action:** Verify all tables and columns exist.

```sql
-- Main DB: Count tables (should be ~230+)
SELECT COUNT(*) as table_count
FROM information_schema.tables 
WHERE table_schema = 'public' AND table_type = 'BASE TABLE';

-- Auth DB: Check ApplicationUser columns
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'AspNetUsers'
AND column_name IN (
    'FirstName', 'LastName', 'Abilities', 'Department', 
    'JobTitle', 'KsaCompetencyLevel', 'IsActive', 'CreatedDate'
)
ORDER BY column_name;
```

**Success Criteria:**
- ✅ Main DB: 230+ tables
- ✅ Auth DB: All 8 custom columns exist
- ✅ No missing critical tables/columns

---

## Phase 4: Rollback Procedure

### Step 4.1: If Migration Fails

**Action:** Restore from backup.

```bash
# Stop application
# (Ctrl+C or kill process)

# Restore main database
pg_restore -U postgres -d GrcMvcDb -c backups/migration-fix-YYYYMMDD/GrcMvcDb_YYYYMMDD_HHMMSS.backup

# Restore auth database
pg_restore -U postgres -d GrcAuthDb -c backups/migration-fix-YYYYMMDD/GrcAuthDb_YYYYMMDD_HHMMSS.backup

# Verify restore
psql -U postgres -d GrcMvcDb -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';"
```

---

### Step 4.2: If Schema is Inconsistent

**Action:** Manual migration application.

```bash
cd src/GrcMvc

# Apply migrations manually (bypasses startup code)
dotnet ef database update --context GrcDbContext --connection "Host=localhost;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5432"

dotnet ef database update --context GrcAuthDbContext --connection "Host=localhost;Database=GrcAuthDb;Username=postgres;Password=postgres;Port=5432"
```

---

## Phase 5: Production Deployment

### Step 5.1: Pre-Deployment Checklist

- [ ] All code changes committed
- [ ] Tests passed on development
- [ ] Backups created
- [ ] Migration files reviewed
- [ ] Connection strings verified
- [ ] Timeout values appropriate for production load

---

### Step 5.2: Deployment Steps

1. **Backup Production Databases**
   ```bash
   pg_dump -U postgres -d GrcMvcDb -F c -f prod_backup_GrcMvcDb_$(date +%Y%m%d_%H%M%S).backup
   pg_dump -U postgres -d GrcAuthDb -F c -f prod_backup_GrcAuthDb_$(date +%Y%m%d_%H%M%S).backup
   ```

2. **Deploy Code**
   ```bash
   # Build and deploy application
   dotnet publish -c Release
   # (deploy to production server)
   ```

3. **Apply Migrations Manually (Recommended)**
   ```bash
   # On production server
   cd /path/to/deployed/app
   dotnet ef database update --context GrcDbContext
   dotnet ef database update --context GrcAuthDbContext
   ```

4. **Start Application**
   ```bash
   # Application will verify migrations on startup
   dotnet GrcMvc.dll
   ```

5. **Verify Health**
   ```bash
   # Check logs for migration success
   tail -f logs/grc-system-*.log | grep -i "migration\|database"
   ```

---

## Phase 6: Monitoring & Validation

### Step 6.1: Post-Deployment Verification

**Check Application Logs:**
```bash
grep -i "migration\|database" logs/grc-system-*.log | tail -20
```

**Expected Log Messages:**
- ✅ "Applying database migrations..."
- ✅ "Found X pending migrations" OR "database is up-to-date"
- ✅ "✅ Main database migrations applied successfully"
- ✅ "✅ Auth database migrations applied successfully"
- ✅ "✅ Migration health check passed"

**Check Database:**
```sql
-- Verify migration history
SELECT COUNT(*) FROM "__EFMigrationsHistory";

-- Verify tables
SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';
```

---

### Step 6.2: Functional Testing

**Test Scenarios:**
1. ✅ User registration (tests Auth DB)
2. ✅ User login (tests Auth DB)
3. ✅ Create tenant (tests Main DB)
4. ✅ Create risk/control (tests Main DB)
5. ✅ Onboarding wizard (tests Main DB)

**Success Criteria:**
- ✅ All operations complete without errors
- ✅ No database connection timeouts
- ✅ No missing column errors
- ✅ Application responsive

---

## Implementation Checklist

### Code Changes
- [ ] **Step 2.1:** Add timeout/retry to DbContext registration
- [ ] **Step 2.2:** Replace `EnsureCreated()` with `MigrateAsync()` in `WebApplicationExtensions.cs`
- [ ] **Step 2.3:** Add `VerifyMigrationHealthAsync()` method
- [ ] **Step 2.4:** Update `Program.cs` to call health check

### Testing
- [ ] **Step 3.1:** Test on clean database
- [ ] **Step 3.2:** Test on existing database
- [ ] **Step 3.3:** Verify migration history
- [ ] **Step 3.4:** Verify schema completeness

### Documentation
- [ ] Update `docs/IDENTITY_SCHEMA_SAFEGUARDS.md` (if needed)
- [ ] Update `docs/production/04-data-plan.md` (reflect actual implementation)
- [ ] Document timeout values and retry logic

### Deployment
- [ ] Create production backups
- [ ] Deploy code changes
- [ ] Apply migrations manually (recommended)
- [ ] Verify application health
- [ ] Monitor logs for 24 hours

---

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Migration fails on existing database | Medium | High | Backup before changes, manual migration option |
| Timeout still occurs | Low | Medium | Increase timeout to 600s if needed |
| Schema inconsistency | Low | High | Health check verifies schema |
| Data loss | Very Low | Critical | Full backups before any changes |

---

## Success Metrics

**After Implementation:**
- ✅ Application starts in < 60 seconds (no hanging)
- ✅ All migrations tracked in `__EFMigrationsHistory`
- ✅ 230+ tables in main database
- ✅ All ApplicationUser custom columns in auth database
- ✅ Zero migration-related errors in logs
- ✅ User registration/login works
- ✅ All GRC operations functional

---

## Timeline Estimate

| Phase | Duration | Dependencies |
|-------|----------|--------------|
| Phase 1: Verification | 30 min | None |
| Phase 2: Code Changes | 60 min | Phase 1 complete |
| Phase 3: Testing | 45 min | Phase 2 complete |
| Phase 4: Rollback Prep | 15 min | Parallel with Phase 2 |
| Phase 5: Production | 30 min | Phases 1-3 complete |
| Phase 6: Monitoring | Ongoing | Phase 5 complete |

**Total Estimated Time:** 2-3 hours

---

## References

1. **Identity Schema Safeguards:** `docs/IDENTITY_SCHEMA_SAFEGUARDS.md`
2. **Migration Hang Analysis:** `docs/MIGRATION_HANG_ANALYSIS.md`
3. **Data Plan:** `docs/production/04-data-plan.md`
4. **Correct Implementation:** `src/GrcMvc/Program.old.bak` (lines 1850-1885)

---

**Plan Status:** ✅ **READY FOR IMPLEMENTATION**  
**Next Action:** Begin Phase 1 - Pre-Implementation Verification
