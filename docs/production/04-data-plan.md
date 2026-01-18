# Database Plan: PostgreSQL + EF Core Migrations

**Status:** ✅ READY (with one critical discrepancy noted)

---

## 1. Database Architecture

### Databases

| Database Name | DbContext | Purpose | Connection String Key |
|--------------|-----------|---------|----------------------|
| `GrcMvcDb` | `GrcDbContext` | Main GRC application data | `ConnectionStrings__DefaultConnection` |
| `GrcMvcDb_auth` | `GrcAuthDbContext` | Identity & Authentication | `ConnectionStrings__GrcAuthDb` |

**Note:** The auth database name is `GrcMvcDb_auth` (with underscore suffix), but the connection string key is `GrcAuthDb` (without underscore). This is intentional and matches the pattern in `docker-compose.production.yml`.

---

## 2. EF Core Migrations

### Migration Commands

**Manual Migration (Pre-Deploy or Standalone):**

```bash
# Main database (GrcDbContext)
dotnet ef database update --project src/GrcMvc/GrcMvc.csproj --context GrcDbContext

# Auth database (GrcAuthDbContext)
dotnet ef database update --project src/GrcMvc/GrcMvc.csproj --context GrcAuthDbContext
```

**Source:** `scripts/deploy-and-seed.sh` (lines 30-40)

### Automatic Migration on Startup

**Location:** `src/GrcMvc/Program.cs` (lines 1840-1885)

**Behavior:**

- **Production:** Uses `Migrate()` for both contexts
  - Checks for pending migrations before applying
  - Falls back to `EnsureCreated()` for main DB if no pending migrations (safety check)
  - **CRITICAL:** Auth DB always uses `Migrate()` (never `EnsureCreated()`)
  
- **Development:** Uses `EnsureCreated()` for main DB (faster), `Migrate()` for auth DB

**Code Reference:**
```csharp
// Production: Main DB
var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
if (pendingMigrations.Any())
{
    dbContext.Database.Migrate();
}
else
{
    dbContext.Database.EnsureCreated(); // Safety check
}

// Production: Auth DB (ALWAYS Migrate)
authContext.Database.Migrate();
```

### Migration Timing

**Recommended Approach:** Pre-deploy migrations (before container restart)

**Rationale:**
- Prevents startup failures if migrations are incompatible
- Allows rollback of application code without affecting database
- Better error visibility during deployment

**Alternative:** Post-deploy (current behavior)
- Migrations run automatically on startup
- Simpler deployment flow
- Risk: Application may fail to start if migrations fail

**Recommendation:** Keep automatic migrations but add pre-deploy migration step to deployment runbook for critical releases.

---

## 3. Database Backups

### Backup Script

**File:** `scripts/backup-db.sh`

**Method:** `pg_dump` with custom format (`-Fc`) + gzip compression

**Databases Backed Up:**
- `GrcMvcDb` (main)
- `GrcAuthDb` (auth) — **⚠️ DISCREPANCY NOTED BELOW**

**Container:** `grc-db` (Docker Compose service name)

**Backup Format:**
```bash
pg_dump -U postgres -Fc <database> | gzip > backups/<database>_<timestamp>.sql.gz
```

**Storage Location:** `$PROJECT_ROOT/backups/` (relative to script directory)

**Retention:** 30 days (configurable via `RETENTION_DAYS` variable)

### ⚠️ CRITICAL DISCREPANCY

**Issue:** `backup-db.sh` references `GrcAuthDb` (line 62), but the actual database name is `GrcMvcDb_auth`.

**Current Script:**
```bash
docker exec grc-db pg_dump -U postgres -Fc GrcAuthDb | gzip > ...
```

**Should Be:**
```bash
docker exec grc-db pg_dump -U postgres -Fc GrcMvcDb_auth | gzip > ...
```

**Impact:** Auth database backups will fail silently or backup the wrong database.

**Status:** 🔴 **MISSING** — Fix required before production deployment.

**Proposed Fix:**
```bash
# Use environment variable or derive from DB_NAME
AUTH_DB_NAME="${DB_NAME:-GrcMvcDb}_auth"
docker exec grc-db pg_dump -U postgres -Fc "$AUTH_DB_NAME" | gzip > ...
```

---

### Backup Schedule

**Recommended:** Daily at 02:00 UTC (low traffic window)

**Implementation Options:**

1. **Cron Job (Host Level):**
   ```bash
   # Add to crontab
   0 2 * * * /path/to/project/scripts/backup-db.sh >> /var/log/grc-backup.log 2>&1
   ```

2. **Docker Cron Container:**
   ```yaml
   # Add to docker-compose.production.yml
   backup-cron:
     image: alpine:latest
     volumes:
       - ./scripts:/scripts:ro
       - ./backups:/backups
       - /var/run/docker.sock:/var/run/docker.sock:ro
     command: >
       sh -c "
         apk add --no-cache docker-cli bash &&
         echo '0 2 * * * /scripts/backup-db.sh' | crontab - &&
         crond -f
       "
   ```

3. **Systemd Timer (Recommended for VPS):**
   ```ini
   # /etc/systemd/system/grc-backup.service
   [Unit]
   Description=GRC Database Backup
   Requires=docker.service
   After=docker.service

   [Service]
   Type=oneshot
   ExecStart=/path/to/project/scripts/backup-db.sh
   User=root
   ```

   ```ini
   # /etc/systemd/system/grc-backup.timer
   [Unit]
   Description=Daily GRC Database Backup
   Requires=grc-backup.service

   [Timer]
   OnCalendar=daily
   OnCalendar=02:00
   Persistent=true

   [Install]
   WantedBy=timers.target
   ```

**Enable Timer:**
```bash
sudo systemctl enable grc-backup.timer
sudo systemctl start grc-backup.timer
```

---

### Backup Retention Policy

**Local Backups:**
- **Retention:** 30 days (configurable via `RETENTION_DAYS` in script)
- **Storage:** `$PROJECT_ROOT/backups/`
- **Cleanup:** Automatic (via `find -mtime +$RETENTION_DAYS -delete`)

**Offsite Backups (Optional):**
- **Azure Blob Storage:** See `scripts/backup-database.sh` (supports Azure upload)
- **S3/Other:** Not currently implemented
- **Recommendation:** Implement offsite backups for production (see MISSING items)

---

## 4. Restore Procedure

### Restore Script

**File:** `scripts/restore-database.sh`

**Current Status:** 🔴 **MISSING** — Placeholder only, needs implementation

**Proposed Implementation:**

```bash
#!/bin/bash
# GRC Database Restore Script
# Usage: ./restore-database.sh <backup_file> <target_database_name> [container_name]

set -euo pipefail

if [ $# -lt 2 ]; then
    echo "Usage: $0 <backup_file> <target_database_name> [container_name]"
    echo "Example: $0 backups/grcmvcdb_20260112_020000.sql.gz GrcMvcDb grc-db"
    exit 1
fi

BACKUP_FILE="$1"
TARGET_DB="$2"
CONTAINER="${3:-grc-db}"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

log_info() { echo -e "${GREEN}[INFO]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1" >&2; }

# Validate backup file
if [ ! -f "$BACKUP_FILE" ]; then
    log_error "Backup file not found: $BACKUP_FILE"
    exit 1
fi

# Check if container is running
if ! docker ps | grep -q "$CONTAINER"; then
    log_error "Container $CONTAINER is not running"
    exit 1
fi

log_warn "⚠️  WARNING: This will DROP and recreate database $TARGET_DB"
log_warn "Press Ctrl+C to cancel, or Enter to continue..."
read -r

# Drop existing database (if exists)
log_info "Dropping existing database $TARGET_DB (if exists)..."
docker exec "$CONTAINER" psql -U postgres -c "DROP DATABASE IF EXISTS \"$TARGET_DB\";" || true

# Create empty database
log_info "Creating empty database $TARGET_DB..."
docker exec "$CONTAINER" psql -U postgres -c "CREATE DATABASE \"$TARGET_DB\";"

# Restore from backup
log_info "Restoring from $BACKUP_FILE..."
if [[ "$BACKUP_FILE" == *.gz ]]; then
    gunzip -c "$BACKUP_FILE" | docker exec -i "$CONTAINER" pg_restore -U postgres -d "$TARGET_DB" --verbose --clean --if-exists
else
    docker exec -i "$CONTAINER" pg_restore -U postgres -d "$TARGET_DB" --verbose --clean --if-exists < "$BACKUP_FILE"
fi

if [ $? -eq 0 ]; then
    log_info "✅ Restore completed successfully"
else
    log_error "❌ Restore failed"
    exit 1
fi
```

**Usage Examples:**

```bash
# Restore main database
./scripts/restore-database.sh backups/grcmvcdb_20260112_020000.sql.gz GrcMvcDb grc-db

# Restore auth database
./scripts/restore-database.sh backups/grcauthdb_20260112_020000.sql.gz GrcMvcDb_auth grc-db
```

---

### Restore Test Checklist

**Frequency:** Monthly (or after major schema changes)

**Procedure:**

1. **Pre-Restore:**
   - [ ] Verify backup file exists and is readable
   - [ ] Check backup file size (should be > 0)
   - [ ] Verify target database name matches backup
   - [ ] **CRITICAL:** Create a backup of current production database before restore test
   - [ ] Document current database size and row counts (for verification)

2. **Restore:**
   - [ ] Run restore script on a **test/staging** database (never on production)
   - [ ] Verify restore completes without errors
   - [ ] Check database size matches expected size
   - [ ] Verify row counts match expected counts (sample tables)

3. **Post-Restore Verification:**
   - [ ] Connect to restored database: `docker exec -it grc-db psql -U postgres -d <dbname>`
   - [ ] Run sample queries:
     ```sql
     -- Check table counts
     SELECT COUNT(*) FROM "Tenants";
     SELECT COUNT(*) FROM "AspNetUsers";
     
     -- Verify recent data exists
     SELECT MAX("CreatedAt") FROM "Tenants";
     SELECT MAX("CreatedAt") FROM "AspNetUsers";
     ```
   - [ ] Test application connectivity to restored database
   - [ ] Verify authentication works (if auth DB restored)

4. **Documentation:**
   - [ ] Record restore test date and results
   - [ ] Document any issues encountered
   - [ ] Update restore procedure if needed

**Test Database Setup:**
```bash
# Create test database
docker exec grc-db psql -U postgres -c "CREATE DATABASE grcmvcdb_test;"
docker exec grc-db psql -U postgres -c "CREATE DATABASE grcmvcdb_auth_test;"

# Restore to test databases
./scripts/restore-database.sh backups/grcmvcdb_20260112_020000.sql.gz grcmvcdb_test grc-db
./scripts/restore-database.sh backups/grcauthdb_20260112_020000.sql.gz grcmvcdb_auth_test grc-db
```

---

## 5. Database Extensions & Schemas

### PostgreSQL Extensions

**Enabled in:** `scripts/init-db.sql`

- `uuid-ossp` — UUID generation
- `pg_trgm` — Trigram matching (full-text search)
- `pgcrypto` — Cryptographic functions

**Auto-Enabled:** Yes (via `init-db.sql` on container startup)

### Custom Schemas

**Created in:** `scripts/init-db.sql`

- `audit` — Audit logging tables
- `analytics` — Analytics data
- `integration` — Integration data
- `hangfire` — Hangfire job storage

**Note:** Tables are created by EF Core migrations, not by `init-db.sql`.

---

## 6. Data Persistence

### Docker Volumes

**Volume Name:** `grc-db-data` (from `docker-compose.production.yml`)

**Mount Point:** `/var/lib/postgresql/data` (inside container)

**Host Path:** Managed by Docker (default location: `/var/lib/docker/volumes/grc-db-data/_data`)

**Backup Strategy:**
- **Primary:** `pg_dump` based backups (recommended)
- **Alternative:** Volume snapshot (if using Docker volume plugins)

**Volume Backup (Optional):**
```bash
# Create volume backup
docker run --rm -v grc-db-data:/data -v $(pwd)/backups:/backup alpine tar czf /backup/grc-db-volume-$(date +%Y%m%d_%H%M%S).tar.gz /data

# Restore volume backup
docker run --rm -v grc-db-data:/data -v $(pwd)/backups:/backup alpine tar xzf /backup/grc-db-volume-<timestamp>.tar.gz -C /
```

---

## 7. Migration Best Practices

### Pre-Deploy Checklist

- [ ] Review migration files for breaking changes
- [ ] Test migrations on staging database
- [ ] Verify migration rollback plan (if applicable)
- [ ] Check disk space (migrations may require temporary space)
- [ ] Backup database before applying migrations

### Migration Rollback

**EF Core does not support automatic rollback.** To rollback:

1. **Option 1:** Restore from backup (if migration already applied)
2. **Option 2:** Create a new migration that reverses changes (if migration not yet applied)
3. **Option 3:** Manual SQL script to reverse changes (advanced)

**Recommendation:** Always backup before applying migrations in production.

---

## 8. Missing Items & Recommendations

### 🔴 CRITICAL

1. **Backup Script Database Name Mismatch**
   - **Issue:** `backup-db.sh` references `GrcAuthDb` but actual database is `GrcMvcDb_auth`
   - **Fix:** Update script to use `GrcMvcDb_auth` or derive from `DB_NAME` environment variable
   - **File:** `scripts/backup-db.sh` (line 62)

2. **Restore Script Implementation**
   - **Issue:** `restore-database.sh` is a placeholder
   - **Fix:** Implement full restore procedure (see proposed implementation above)
   - **File:** `scripts/restore-database.sh`

### ⚠️ RECOMMENDED

3. **Offsite Backup Storage**
   - **Current:** Local backups only
   - **Recommendation:** Implement Azure Blob Storage or S3 backup (see `backup-database.sh` for Azure example)
   - **Priority:** High for production

4. **Backup Verification**
   - **Current:** No automated backup verification
   - **Recommendation:** Add backup integrity check (test restore to temporary database)
   - **Priority:** Medium

5. **Backup Monitoring**
   - **Current:** No alerting on backup failures
   - **Recommendation:** Add email/notification on backup failure
   - **Priority:** Medium

6. **Migration Pre-Deploy Step**
   - **Current:** Migrations run automatically on startup
   - **Recommendation:** Add optional pre-deploy migration step to deployment runbook
   - **Priority:** Low (current approach is acceptable)

---

## 9. Quick Reference

### Backup Command

```bash
# Manual backup
./scripts/backup-db.sh

# Check backup status
ls -lh backups/
```

### Restore Command

```bash
# Restore main database
./scripts/restore-database.sh backups/grcmvcdb_<timestamp>.sql.gz GrcMvcDb grc-db

# Restore auth database
./scripts/restore-database.sh backups/grcauthdb_<timestamp>.sql.gz GrcMvcDb_auth grc-db
```

### Migration Commands

```bash
# Check pending migrations
dotnet ef migrations list --project src/GrcMvc/GrcMvc.csproj --context GrcDbContext
dotnet ef migrations list --project src/GrcMvc/GrcMvc.csproj --context GrcAuthDbContext

# Apply migrations manually
dotnet ef database update --project src/GrcMvc/GrcMvc.csproj --context GrcDbContext
dotnet ef database update --project src/GrcMvc/GrcMvc.csproj --context GrcAuthDbContext
```

### Database Connection Test

```bash
# Test main database
docker exec grc-db psql -U postgres -d GrcMvcDb -c "SELECT 1;"

# Test auth database
docker exec grc-db psql -U postgres -d GrcMvcDb_auth -c "SELECT 1;"
```

---

**Next:** [STEP 6 — DEPLOYMENT ARTIFACTS](./05-deployment.md)
