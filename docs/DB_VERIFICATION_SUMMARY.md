# Database Connection, Indexes, Relations, and Query Verification

## ✅ Status: INSTRUMENTED FOR VERIFICATION

All critical database operations have been instrumented with `[DB_CHECK]` logging tags.

---

## 🔍 Hypotheses Being Tested

### Hypothesis A: Database Connection Issues
**Test:** `Database.CanConnectAsync()` check in `ProcessPostLoginAsync`
**Expected:** Connection succeeds, logs `CanConnect=true`
**If fails:** Connection string misconfigured or database unavailable

### Hypothesis B: Missing Index on TenantUser.UserId
**Test:** Query performance measurement for `TenantUsers.FirstOrDefaultAsync(tu => tu.UserId == user.Id)`
**Expected:** Query duration < 50ms (with index)
**If slow (>100ms):** Missing index on `UserId` column (currently only composite index on `(TenantId, UserId)`)

### Hypothesis C: Missing Index on Tenant.OnboardingStatus
**Test:** Query performance for `Tenants.FirstOrDefaultAsync(t => t.Id == tenantId && !t.IsDeleted)`
**Expected:** Query duration < 20ms
**If slow:** Missing index on `OnboardingStatus` (frequently queried column)

### Hypothesis D: Relationship Navigation Not Working
**Test:** Check if `tenantUser.Tenant` navigation property is loaded via `Include()`
**Expected:** `HasTenantNav=true` in logs (relationship loaded)
**If false:** EF Core relationship configuration issue

### Hypothesis E: Query Filter (IsDeleted) Causing Issues
**Test:** Verify queries respect `HasQueryFilter(e => !e.IsDeleted)`
**Expected:** Deleted records are automatically excluded
**If fails:** Query filter not applied or incorrectly configured

---

## 📊 Current Index Configuration

### ✅ Existing Indexes

#### Tenant Entity
- ✅ `TenantSlug` - UNIQUE index (line 703)
- ✅ `AdminEmail` - Non-unique index (line 704)
- ❌ **MISSING:** `OnboardingStatus` - No index (frequently queried)

#### TenantUser Entity
- ✅ `(TenantId, UserId)` - Composite UNIQUE index (line 715)
- ❌ **MISSING:** `UserId` - No standalone index (query uses `UserId` alone)

#### Query Patterns
1. **ProcessPostLoginAsync:**
   ```csharp
   .FirstOrDefaultAsync(tu => tu.UserId == user.Id && !tu.IsDeleted)
   ```
   - Uses `UserId` alone (no `TenantId` in WHERE clause)
   - Composite index `(TenantId, UserId)` may not be optimal
   - **Recommendation:** Add standalone index on `UserId`

2. **OnboardingRedirectMiddleware:**
   ```csharp
   .FirstOrDefaultAsync(t => t.Id == tenantId && !t.IsDeleted)
   ```
   - Uses `Id` (primary key) - already indexed
   - No performance concern

3. **OnboardingStatus Checks:**
   - Frequently queried but no index
   - **Recommendation:** Add index on `OnboardingStatus`

---

## 🔗 Relationship Configuration

### ✅ TenantUser → Tenant Relationship
**Configuration (line 718-721):**
```csharp
entity.HasOne(e => e.Tenant)
    .WithMany(t => t.Users)
    .HasForeignKey(e => e.TenantId)
    .OnDelete(DeleteBehavior.Cascade);
```

**Status:** ✅ Properly configured
- Foreign key: `TenantUser.TenantId` → `Tenant.Id`
- Cascade delete: Deleting tenant deletes all tenant users
- Navigation: `TenantUser.Tenant` and `Tenant.Users` both configured

### ✅ Query Filter Configuration
**Tenant (line 705):**
```csharp
entity.HasQueryFilter(e => !e.IsDeleted);
```

**TenantUser (line 716):**
```csharp
entity.HasQueryFilter(e => !e.IsDeleted);
```

**Status:** ✅ Properly configured
- All queries automatically exclude deleted records
- No need to add `&& !e.IsDeleted` manually (but it's safe to include)

---

## 📝 Log Tags to Monitor

All database checks are prefixed with `[DB_CHECK]` for easy filtering.

### Connection Health
```
[DB_CHECK] Database connection test. CanConnect={CanConnect}, UserId={UserId}
```

### Query Performance
```
[DB_CHECK] TenantUser query executed. UserId={UserId}, Duration={Duration}ms, Found={Found}, TenantId={TenantId}, HasTenantNav={HasTenantNav}
[DB_CHECK] Tenant query executed. TenantId={TenantId}, Duration={Duration}ms, Found={Found}, Source={Source}
[DB_CHECK] Middleware tenant query. TenantId={TenantId}, Duration={Duration}ms, Found={Found}, OnboardingStatus={Status}
```

---

## 🧪 How to Test

1. **Start the application** and monitor logs
2. **Login** with a tenant admin user
3. **Filter logs** by `[DB_CHECK]` tag
4. **Verify:**
   - `CanConnect=true` (database connection works)
   - Query durations < 100ms (acceptable performance)
   - `HasTenantNav=true` (relationship navigation works)
   - All queries return expected results

---

## 🚨 Performance Thresholds

| Query Type | Expected Duration | Warning Threshold | Critical Threshold |
|------------|------------------|-------------------|-------------------|
| Database Connection | < 50ms | 50-200ms | > 200ms |
| TenantUser Query (with Include) | < 50ms | 50-100ms | > 100ms |
| Tenant Query (by Id) | < 20ms | 20-50ms | > 50ms |
| Middleware Tenant Query | < 20ms | 20-50ms | > 50ms |

---

## 🔧 Recommended Index Additions

### 1. Add Index on TenantUser.UserId
**Reason:** Query in `ProcessPostLoginAsync` filters by `UserId` alone
**Impact:** High - This query runs on every login

```csharp
// In GrcDbContext.OnModelCreating, TenantUser configuration:
entity.HasIndex(e => e.UserId); // Add this line
```

### 2. Add Index on Tenant.OnboardingStatus
**Reason:** Frequently queried in middleware and post-login checks
**Impact:** Medium - Improves middleware performance

```csharp
// In GrcDbContext.OnModelCreating, Tenant configuration:
entity.HasIndex(e => e.OnboardingStatus); // Add this line
```

### 3. Migration Required
After adding indexes, create and apply migration:
```bash
dotnet ef migrations add AddTenantUserUserIdIndex AddTenantOnboardingStatusIndex
dotnet ef database update
```

---

## 📋 Database Connection Verification

### Connection String Sources (Priority Order)
1. Environment variable: `ConnectionStrings__DefaultConnection`
2. Environment variable: `CONNECTION_STRING`
3. Configuration file: `appsettings.json`
4. ABP Settings (encrypted, after ABP initialization)

### Connection String Format
```
Host=localhost;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5433
```

### Health Check
- Health check endpoint: `/health`
- Database health check: `AddNpgSql()` registered in `Program.cs`

---

## 🔗 Related Files

- `Data/GrcDbContext.cs` - Entity configurations and indexes
- `Controllers/AccountController.cs` - ProcessPostLoginAsync with DB checks
- `Middleware/OnboardingRedirectMiddleware.cs` - Middleware tenant query
- `Models/Entities/Tenant.cs` - Tenant entity definition
- `Models/Entities/TenantUser.cs` - TenantUser entity definition

---

**Last Updated:** 2026-01-12
**Status:** ✅ Instrumented and ready for testing
