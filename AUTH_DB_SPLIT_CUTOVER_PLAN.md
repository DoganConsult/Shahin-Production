# Auth DB Split - Cutover Plan

## Overview
This document outlines the cutover procedure for splitting the authentication database from the main GRC application database.

**Source DB**: `GrcMvcDb` (main database)
**Target DB**: `GrcAuthDb` (new auth database)

---

## Pre-Cutover Checklist

### Code Changes (Completed)
- [x] Created `GrcAuthDbContext` for Identity entities
- [x] Updated `GrcDbContext` to remove `IdentityDbContext` inheritance
- [x] Created `IUserDirectoryService` for batch user lookups
- [x] Updated all services to use `IUserDirectoryService` instead of `_context.Users`
- [x] Removed hard FK constraints from RBAC models (soft links only)
- [x] Updated Identity configuration to use `GrcAuthDbContext`
- [x] Added `GrcAuthDb` connection string to `appsettings.json`

### Infrastructure (Completed)
- [x] Created `GrcAuthDb` database
- [x] Applied EF Core migrations to `GrcAuthDb`
- [x] Verified Identity tables exist in `GrcAuthDb`

### Migration Scripts (Completed)
- [x] `01-export-identity-data.sql` - Export from source
- [x] `02-import-identity-data.sql` - Import to target
- [x] `03-verify-integrity.sql` - Data integrity checks
- [x] `migrate-auth-data.sh` - Automated migration script

---

## Cutover Procedure

### Phase 1: Pre-Migration (T-1 hour)
1. **Announce maintenance window** to users
2. **Create full backup** of both databases:
   ```bash
   pg_dump GrcMvcDb > backup_grcmvcdb_$(date +%Y%m%d_%H%M%S).sql
   pg_dump GrcAuthDb > backup_grcauthdb_$(date +%Y%m%d_%H%M%S).sql
   ```
3. **Verify backup integrity** by restoring to test environment

### Phase 2: Freeze & Migrate (T-0)
1. **Stop application** to prevent writes:
   ```bash
   docker stop grc-app
   ```

2. **Run data migration**:
   ```bash
   cd /home/dogan/grc-system
   ./scripts/auth-db-migration/migrate-auth-data.sh
   ```

3. **Verify row counts match** between source and target

4. **Run integrity checks**:
   ```bash
   psql -d GrcAuthDb -f scripts/auth-db-migration/03-verify-integrity.sql
   ```

### Phase 3: Validation (T+15 min)
1. **Start application**:
   ```bash
   docker start grc-app
   ```

2. **Test critical auth flows**:
   - [ ] Login with existing user
   - [ ] Password reset
   - [ ] Role-based menu access
   - [ ] API authentication (JWT)
   - [ ] New user registration

3. **Monitor logs** for auth errors:
   ```bash
   docker logs -f grc-app | grep -i "auth\|identity\|login"
   ```

### Phase 4: Post-Cutover (T+1 hour)
1. **If successful**: Mark Identity tables in `GrcMvcDb` for removal
2. **Monitor** for 24-48 hours before dropping old tables
3. **Update documentation** and runbooks

---

## Rollback Procedure

### Immediate Rollback (< 1 hour)
If issues are detected during validation:

1. **Stop application**:
   ```bash
   docker stop grc-app
   ```

2. **Update connection string** to point Identity back to `GrcMvcDb`:
   ```json
   // In appsettings.json, temporarily use same connection for both
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Database=GrcMvcDb;...",
     "GrcAuthDb": "Host=localhost;Database=GrcMvcDb;..."  // Same as default
   }
   ```

3. **Restart application**:
   ```bash
   docker start grc-app
   ```

4. **Verify** auth works with original database

### Full Rollback (> 1 hour)
If data corruption is detected:

1. **Stop application**
2. **Restore from pre-migration backup**:
   ```bash
   psql -c "DROP DATABASE GrcMvcDb;"
   psql -c "CREATE DATABASE GrcMvcDb;"
   psql GrcMvcDb < backup_grcmvcdb_TIMESTAMP.sql
   ```
3. **Revert code changes** (git checkout to pre-split commit)
4. **Restart application**

---

## Acceptance Criteria

### Functional
- [ ] All existing users can log in
- [ ] Password hashes are preserved
- [ ] Role assignments work correctly
- [ ] MFA/2FA (if enabled) continues to work
- [ ] JWT tokens are issued correctly

### Data Integrity
- [ ] User count matches between source export and target
- [ ] Role count matches
- [ ] No orphaned UserIds in main DB
- [ ] All TenantUsers reference valid users

### Performance
- [ ] Login latency < 500ms
- [ ] No N+1 query regressions (batch lookups verified)
- [ ] Auth DB backups enabled

---

## Contacts

| Role | Name | Contact |
|------|------|---------|
| DBA | TBD | |
| Dev Lead | TBD | |
| On-Call | TBD | |

---

## Timeline

| Time | Action |
|------|--------|
| T-1h | Announce maintenance |
| T-30m | Create backups |
| T-0 | Stop app, run migration |
| T+15m | Start app, validate |
| T+1h | Confirm success or rollback |
| T+24h | Monitor complete |
| T+7d | Drop old Identity tables |

---

## Post-Migration Cleanup

After 7 days of successful operation:

1. **Rename old Identity tables** (safety):
   ```sql
   ALTER TABLE "AspNetUsers" RENAME TO "_old_AspNetUsers";
   ALTER TABLE "AspNetRoles" RENAME TO "_old_AspNetRoles";
   -- etc.
   ```

2. **After 30 days**, drop renamed tables:
   ```sql
   DROP TABLE "_old_AspNetUsers";
   DROP TABLE "_old_AspNetRoles";
   -- etc.
   ```

3. **Update EF migrations** to remove Identity from `GrcDbContext` snapshot
