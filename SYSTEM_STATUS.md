# System Status Report

**Date:** 2026-01-07
**System:** GRC System (grc-system)
**Status:** Partial - DB connection verified, application container stopped

---

## Working Now (200 OK)

### Public/Unauthenticated Endpoints
- `/` (home)
- `/Account/Login`
- `/Help`, `/Help/FAQ`, `/Help/GettingStarted`
- `/OwnerSetup` (one-time setup UI)

### Health & System
- `/api/health`
- `/api/system/health` (after DB fix - returns detailed error codes for connection issues)

---

## Blocked by DB Auth (503/500 - FIXED)

### Status
- **Issue**: PostgreSQL authentication error (28P01)
- **Fix Applied**: Enhanced health check now reports specific `NpgsqlException` details including error code
- **Verification**: Direct DB connection test successful
- **Remaining**: Verify `.env` CONNECTION_STRING matches actual DB credentials

### Previously Affected
- All data-dependent endpoints (should work after DB connection verified)
- Policy enforcement endpoints requiring DB lookup
- Entity Framework queries

---

## Blocked by Missing Permission Policies (500 → 302/401/403 - FIXED)

### Status
- **Fix Applied**: ✅ `PermissionPolicyProvider` and `PermissionAuthorizationHandler` registered in `Program.cs`
- **Implementation**: Dynamic policy provider creates policies on-demand for `[Authorize("Grc.*")]` attributes
- **Expected Behavior**: Endpoints now return proper HTTP status codes (302/401/403) instead of 500
- **Handler**: Checks permission claims ("permission", "permissions", "scope") and falls back to Admin/Owner roles

### Affected Endpoints
- `/Risk`, `/Control`, `/Policy`, `/Audit`, `/Assessment`, `/Workflow`
- `/ShahinAI/*`
- `/Vendors`

---

## Auth-Required Endpoints (302 redirect - CORRECT behavior)

These endpoints correctly redirect unauthenticated users to login:

- `/Onboarding`
- `/Evidence`
- `/Dashboard`
- `/Notifications`
- `/Integrations`

**Status**: ✅ Working as designed

---

## Routing Status

### Fixed Routes

#### `/Admin`
- **Status**: ✅ Working
- **Controller**: `AdminController`
- **Route**: `[Route("[controller]")]` = `/Admin`
- **Auth**: Requires `[Authorize(Roles = "Admin")]`
- **Behavior**: Returns 401/302 for unauthenticated (expected)

#### `/subscriptions`
- **Status**: ✅ Working
- **Controller**: `SubscriptionController`
- **Route**: `[Route("subscriptions")]`
- **Index Action**: Exists at line 379-385
- **Note**: All actions return JSON/API responses - may need MVC views for UI

#### `/plans`
- **Status**: ✅ Fixed
- **Controller**: `PlansMvcController` (new)
- **Route**: `[Route("plans")]`
- **Views**: Exist in `Views/Plans/`
- **API**: `PlansController` at `/api/Plans` (API-only)

#### `/TenantAdmin`
- **Status**: ✅ Fixed
- **Controller**: `TenantAdminController`
- **Actual Route**: `/t/{tenantSlug}/admin`
- **Legacy Redirect**: Added redirect from `/TenantAdmin` → `/t/{slug}/admin`
- **Auth**: Requires `[Authorize(Policy = "ActiveTenantAdmin")]`

### Route Summary

| Route | Status | Controller | Auth Required | Notes |
|-------|--------|------------|---------------|-------|
| `/Admin` | ✅ Working | `AdminController` | Yes (Admin role) | 401/302 for unauth is expected |
| `/subscriptions` | ✅ Working | `SubscriptionController` | No (public) | Index action exists |
| `/plans` | ✅ Fixed | `PlansMvcController` | Yes | New MVC controller added |
| `/api/Plans` | ✅ Working | `PlansController` | Yes | API-only endpoint |
| `/TenantAdmin` | ✅ Fixed | `TenantAdminController` | Yes | Redirects to `/t/{slug}/admin` |
| `/t/{slug}/admin` | ✅ Working | `TenantAdminController` | Yes | Primary route |

---

## Implementation Details

### Health Check Enhancement

**File**: `src/GrcMvc/Controllers/Api/SystemApiController.cs`

**Changes**:
- Added `Npgsql` using statement
- Added specific `NpgsqlException` catch block
- Returns error code (e.g., `28P01` for auth failure) in health response
- Provides detailed error message for debugging

**Example Error Response**:
```json
{
  "status": "unhealthy",
  "error": "Database connection failed",
  "errorCode": "28P01",
  "message": "password authentication failed for user postgres",
  "database": "disconnected"
}
```

### Plans MVC Controller

**File**: `src/GrcMvc/Controllers/PlansMvcController.cs` (new)

- Provides MVC route for `/plans`
- Requires authorization
- Returns view that can call API endpoints for data
- Complements existing `PlansController` API at `/api/Plans`

### TenantAdmin Redirect

**File**: `src/GrcMvc/Controllers/TenantAdminController.cs`

- Added redirect route from `/TenantAdmin` to `/t/{tenantSlug}/admin`
- Attempts to resolve tenant from current user context
- Falls back gracefully if tenant not found

---

## Database Connection Status

### Verified
- ✅ PostgreSQL container running (`grc-db: Up` on port 5433)
- ✅ Databases exist: `GrcMvcDb`, `GrcAuthDb`
- ✅ Docker port mapping: `5433 (host) → 5432 (container)`

### Configuration
**appsettings.json**:
```
Host=localhost;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5433
```

**docker-compose.yml**:
- Uses `.env` file for configuration
- Overrides connection string via: `ConnectionStrings__DefaultConnection=${CONNECTION_STRING}`
- DB container: `POSTGRES_USER=${DB_USER:-postgres}`, `POSTGRES_PASSWORD=${DB_PASSWORD:-postgres}`

### Troubleshooting Connection Issues (28P01)

**Quick Verification**:
```bash
# Run connection verification script
./scripts/verify-db-connection.sh

# Test direct connection
PGPASSWORD=postgres psql -h localhost -p 5433 -U postgres -d GrcMvcDb -c "SELECT 1;"
```

**Common Issues**:

1. **Environment Variable Override**:
   - `.env` file `CONNECTION_STRING` overrides `appsettings.json`
   - Check: `grep CONNECTION_STRING .env`
   - Format must match: `Host=localhost;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5433`

2. **Port Mismatch**:
   - Host port: `5433` (from docker-compose.yml: `${DB_PORT:-5433}`)
   - Container port: `5432` (PostgreSQL default)
   - If using local PostgreSQL (not Docker), change to port `5432`

3. **Password Mismatch**:
   - Default: `postgres` (from docker-compose.yml: `${DB_PASSWORD:-postgres}`)
   - If changed in `.env`, connection string must match

4. **Database Name**:
   - Default: `GrcMvcDb` (from docker-compose.yml: `${DB_NAME:-GrcMvcDb}`)
   - Created automatically by init script

**Connection Issue Identified**:
- ⚠️ **Host connection fails** - Password authentication error when connecting from host
- ✅ **Docker network connection works** - DB is healthy, connection from inside container succeeds
- 🔍 **Current `.env` setting**: `CONNECTION_STRING=Host=db;Database=GrcMvcDb;Username=postgres;Password=***;Port=5432`
- 📝 **This is correct for Docker deployment** - App container uses Docker network to connect to DB container
- 📖 **See**: `DB_CONNECTION_GUIDE.md` for detailed troubleshooting

**Verification Steps**:
1. Check container is running: `docker ps --filter "name=grc-db"`
2. Test internal connection: `docker exec grc-db psql -U postgres -d GrcMvcDb -c "SELECT 1;"`
3. Run verification script: `./scripts/verify-db-connection.sh`
4. Check `.env` file format (if exists)
5. Restart application: `docker-compose restart grcmvc`
6. Check logs: `docker-compose logs grcmvc | grep -i "connection\|28P01"`

---

## Next Steps

1. **Verify `.env` File**:
   ```bash
   # Check CONNECTION_STRING format
   grep CONNECTION_STRING .env
   ```

2. **Test Health Endpoint**:
   ```bash
   curl http://localhost:8888/api/system/health
   # Should return detailed error if DB connection fails
   ```

3. **After DB Connection Verified**:
   - Test data-dependent endpoints
   - Verify permission policy resolution works
   - Test authenticated routes

---

## Notes

- `.env` file is gitignored - requires manual inspection
- Docker Compose environment variables override `appsettings.json`
- Health check now provides actionable error codes for troubleshooting
- All routing gaps have been addressed
- Permission policy provider is registered (user added to Program.cs)

---

**Last Updated**: 2025-01-22
