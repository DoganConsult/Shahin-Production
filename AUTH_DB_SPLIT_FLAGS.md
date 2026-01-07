# Auth DB Split - Flags & Highlights

## Build Status
- **Errors**: 0
- **Warnings**: 0

---

## Flags (Action Items)

### HIGH PRIORITY
1. **Data Migration Required** - Identity data still in `GrcMvcDb`, needs migration to `GrcAuthDb`
   - Run: `./scripts/auth-db-migration/migrate-auth-data.sh`
   - Location: `scripts/auth-db-migration/`

2. **Session Invalidation** - After cutover, all users must re-login
   - Existing JWT tokens will remain valid until expiry
   - Consider forcing logout on cutover

### MEDIUM PRIORITY
3. **RoleProfile Table Location** - `RoleProfile` is in both DBs after migration
   - Currently: In `GrcAuthDb` (via `ApplicationUser.RoleProfile` navigation)
   - Decision needed: Keep in Auth DB or move to Main DB?

4. **Batch Lookup Optimization** - Some services fetch all roles to filter
   - Files: `MenuService.cs`, `GrcMenuContributor.cs`
   - Pattern: `var allRoles = await _context.Set<IdentityRole>().ToListAsync();`
   - Optimization: Cache roles or use `IUserDirectoryService.GetRolesByIdsAsync()`

### LOW PRIORITY
5. **Cleanup Old Identity Tables** - After 7 days of successful operation
   - Tables to remove from `GrcMvcDb`: `AspNetUsers`, `AspNetRoles`, etc.
   - See cutover plan for procedure

---

## Highlights (What Changed)

### New Files Created
| File | Purpose |
|------|---------|
| `Data/GrcAuthDbContext.cs` | Dedicated DbContext for Identity |
| `Services/Interfaces/IUserDirectoryService.cs` | Batch user/role lookup interface |
| `Services/Implementations/UserDirectoryService.cs` | Implementation with caching |
| `Data/Migrations/Auth/` | Separate migrations for Auth DB |
| `scripts/auth-db-migration/*.sql` | Data migration scripts |
| `AUTH_DB_SPLIT_CUTOVER_PLAN.md` | Cutover procedure |

### Modified Files (Key Changes)
| File | Change |
|------|--------|
| `Data/GrcDbContext.cs` | Removed `IdentityDbContext` inheritance |
| `Models/Entities/RbacModels.cs` | Removed FK to `IdentityRole`/`ApplicationUser` |
| `Program.cs` | Added `GrcAuthDbContext` registration |
| `appsettings.json` | Added `GrcAuthDb` connection string |

### Services Updated to Use `IUserDirectoryService`
- `UserWorkspaceService.cs`
- `InboxService.cs`
- `NotificationService.cs`
- `NotificationDeliveryJob.cs`
- `WorkflowAssigneeResolver.cs`
- `OwnerSetupService.cs`
- `Phase1HRISAndAuditServices.cs`
- `RbacServices.cs` (UserRoleAssignmentService)

### Controllers Updated
- `AdminController.cs` (RoleDelegationController)
- `PlatformAdminController.cs`
- `RoleProfileController.cs`
- `CCMController.cs` (InvitationController)

---

## Architecture Notes

### Soft Links Pattern
All cross-DB references now use **soft links** (string IDs only, no FK constraint):
```csharp
// Before (hard FK)
[ForeignKey("UserId")]
public ApplicationUser User { get; set; }

// After (soft link)
public string UserId { get; set; } // Soft link to GrcAuthDb
```

### Batch Lookup Pattern
Replace navigation properties with batch lookups:
```csharp
// Before
var users = query.Include(x => x.User).ToList();

// After
var userIds = query.Select(x => x.UserId).ToList();
var users = await _userDirectory.GetUsersByIdsAsync(userIds);
```

---

## Testing Checklist
- [ ] Login with existing user
- [ ] Password reset flow
- [ ] New user registration
- [ ] Role assignment
- [ ] Menu visibility by role
- [ ] JWT token generation
- [ ] Tenant user access
- [ ] Workspace membership

---

## Integration Verification Guide

### How to Verify Auth DB Works with All APIs, UI & Routes

#### 1. API Endpoints Verification

**Authentication APIs** (use Auth DB directly):
```bash
# Test login endpoint
curl -X POST https://app.shahin-ai.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "test@example.com", "password": "Test123!"}'

# Test token refresh
curl -X POST https://app.shahin-ai.com/api/auth/refresh \
  -H "Authorization: Bearer <token>"

# Test user info
curl https://app.shahin-ai.com/api/auth/me \
  -H "Authorization: Bearer <token>"
```

**Protected APIs** (use IUserDirectoryService for user lookups):
```bash
# Test tenant APIs
curl https://app.shahin-ai.com/api/tenants \
  -H "Authorization: Bearer <token>"

# Test workflow APIs
curl https://app.shahin-ai.com/api/workflows \
  -H "Authorization: Bearer <token>"
```

#### 2. UI Routes Verification

| Route | Auth Requirement | What to Test |
|-------|------------------|--------------|
| `/` | None | Homepage loads |
| `/Account/Login` | None | Login form works |
| `/Account/Register` | None | Registration creates user in GrcAuthDb |
| `/Dashboard` | Authenticated | User data displays correctly |
| `/platform-admin` | SuperAdmin role | Role check works |
| `/Workflow/*` | Authenticated + Role | Menu items match user roles |
| `/RoleProfile/MyProfile` | Authenticated | User profile from Auth DB |

#### 3. Verification Commands

**Check Auth DB has users:**
```bash
docker exec grc-db psql -U postgres -d GrcAuthDb -c "SELECT COUNT(*) FROM \"AspNetUsers\";"
```

**Check user can authenticate:**
```bash
# In application logs, look for:
docker logs grc-app 2>&1 | grep -i "authenticated\|login\|identity"
```

**Check role-based access:**
```bash
# Verify roles exist
docker exec grc-db psql -U postgres -d GrcAuthDb -c "SELECT \"Name\" FROM \"AspNetRoles\";"

# Verify user-role assignments
docker exec grc-db psql -U postgres -d GrcAuthDb -c "SELECT * FROM \"AspNetUserRoles\" LIMIT 5;"
```

#### 4. Service Integration Points

These services now use `IUserDirectoryService` - verify they work:

| Service | Method to Test | Expected Behavior |
|---------|----------------|-------------------|
| `UserWorkspaceService` | Load dashboard | Shows user's workspace |
| `InboxService` | View inbox | Shows user's tasks |
| `MenuService` | Navigate app | Menu matches user roles |
| `NotificationService` | Trigger notification | Email sent to correct user |
| `RbacServices` | Assign role | Role persists in GrcAuthDb |

#### 5. Automated Integration Test

Run this script to verify all integrations:

```bash
#!/bin/bash
# verify-auth-integration.sh

echo "=== Auth DB Integration Verification ==="

# 1. Check databases exist
echo -n "GrcAuthDb exists: "
docker exec grc-db psql -U postgres -lqt | grep -q GrcAuthDb && echo "YES" || echo "NO"

# 2. Check Identity tables
echo -n "Identity tables: "
TABLE_COUNT=$(docker exec grc-db psql -U postgres -d GrcAuthDb -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_name LIKE 'AspNet%';")
echo "$TABLE_COUNT tables"

# 3. Check user count
echo -n "Users in GrcAuthDb: "
docker exec grc-db psql -U postgres -d GrcAuthDb -t -c "SELECT COUNT(*) FROM \"AspNetUsers\";"

# 4. Check roles
echo -n "Roles in GrcAuthDb: "
docker exec grc-db psql -U postgres -d GrcAuthDb -t -c "SELECT COUNT(*) FROM \"AspNetRoles\";"

# 5. Check app can connect
echo -n "App connection: "
curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/health || echo "FAIL"

# 6. Check login endpoint
echo -n "Login endpoint: "
curl -s -o /dev/null -w "%{http_code}" -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" -d '{}' || echo "FAIL"

echo ""
echo "=== Verification Complete ==="
```

#### 6. Common Issues & Fixes

| Issue | Symptom | Fix |
|-------|---------|-----|
| Users can't login | 401 Unauthorized | Check GrcAuthDb has users migrated |
| Menu not showing | Empty navigation | Verify `RoleFeatures` table has data |
| API returns 500 | Internal error on user lookup | Check `IUserDirectoryService` is registered |
| Roles not working | Access denied | Verify `AspNetUserRoles` has assignments |
| Profile empty | User details missing | Check `ApplicationUser` fields migrated |

#### 7. Connection String Verification

Ensure `appsettings.json` has both connections:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=GrcMvcDb;...",
  "GrcAuthDb": "Host=localhost;Database=GrcAuthDb;..."
}
```

#### 8. DI Registration Verification

In `Program.cs`, verify these are registered:
```csharp
// Auth DbContext
builder.Services.AddDbContext<GrcAuthDbContext>(...);

// Identity uses Auth DbContext
.AddEntityFrameworkStores<GrcAuthDbContext>()

// User Directory Service
builder.Services.AddScoped<IUserDirectoryService, UserDirectoryService>();
```

---

## Rollback
If issues occur, see `AUTH_DB_SPLIT_CUTOVER_PLAN.md` for rollback procedure.
Quick rollback: Point `GrcAuthDb` connection string back to `GrcMvcDb`.
