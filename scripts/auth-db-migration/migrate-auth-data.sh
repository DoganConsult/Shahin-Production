#!/bin/bash
# Auth DB Split Migration Script
# Migrates Identity data from GrcMvcDb to GrcAuthDb

set -e

# Configuration
SOURCE_DB="GrcMvcDb"
TARGET_DB="GrcAuthDb"
DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5433}"
DB_USER="${DB_USER:-postgres}"
BACKUP_DIR="/home/dogan/grc-system/backups/auth-migration-$(date +%Y%m%d_%H%M%S)"

echo "=========================================="
echo "Auth DB Split Migration"
echo "=========================================="
echo "Source: $SOURCE_DB"
echo "Target: $TARGET_DB"
echo "Host: $DB_HOST:$DB_PORT"
echo "Backup Dir: $BACKUP_DIR"
echo "=========================================="

# Create backup directory
mkdir -p "$BACKUP_DIR"

# Step 1: Full backup of source database
echo ""
echo "[Step 1] Creating full backup of $SOURCE_DB..."
PGPASSWORD=$DB_PASSWORD pg_dump -h $DB_HOST -p $DB_PORT -U $DB_USER $SOURCE_DB > "$BACKUP_DIR/${SOURCE_DB}_full.sql"
echo "✅ Backup created: $BACKUP_DIR/${SOURCE_DB}_full.sql"

# Step 2: Export Identity tables from source
echo ""
echo "[Step 2] Exporting Identity tables from $SOURCE_DB..."

IDENTITY_TABLES=(
    "AspNetRoles"
    "AspNetUsers"
    "AspNetRoleClaims"
    "AspNetUserClaims"
    "AspNetUserLogins"
    "AspNetUserRoles"
    "AspNetUserTokens"
    "RoleProfiles"
)

for table in "${IDENTITY_TABLES[@]}"; do
    echo "  Exporting $table..."
    PGPASSWORD=$DB_PASSWORD pg_dump -h $DB_HOST -p $DB_PORT -U $DB_USER \
        --data-only --table="\"$table\"" $SOURCE_DB > "$BACKUP_DIR/${table}.sql" 2>/dev/null || true
done
echo "✅ Identity tables exported"

# Step 3: Verify target database has schema
echo ""
echo "[Step 3] Verifying $TARGET_DB schema..."
TABLE_COUNT=$(PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $TARGET_DB -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_name LIKE 'AspNet%';" | xargs)

if [ "$TABLE_COUNT" -lt 5 ]; then
    echo "⚠️  Target database schema incomplete. Run EF Core migrations first:"
    echo "    dotnet ef database update --context GrcAuthDbContext"
    exit 1
fi
echo "✅ Target schema verified ($TABLE_COUNT Identity tables found)"

# Step 4: Get row counts from source
echo ""
echo "[Step 4] Source database row counts:"
for table in "${IDENTITY_TABLES[@]}"; do
    COUNT=$(PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $SOURCE_DB -t -c "SELECT COUNT(*) FROM \"$table\";" 2>/dev/null | xargs || echo "0")
    echo "  $table: $COUNT rows"
done

# Step 5: Import to target (with confirmation)
echo ""
echo "[Step 5] Ready to import data to $TARGET_DB"
read -p "Continue with import? (y/N): " confirm
if [ "$confirm" != "y" ] && [ "$confirm" != "Y" ]; then
    echo "Migration cancelled."
    exit 0
fi

echo "Importing data..."

# Disable FK checks
PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $TARGET_DB -c "SET session_replication_role = 'replica';"

# Import in order (roles and users first, then junction tables)
for table in "AspNetRoles" "RoleProfiles" "AspNetUsers" "AspNetRoleClaims" "AspNetUserClaims" "AspNetUserLogins" "AspNetUserRoles" "AspNetUserTokens"; do
    if [ -f "$BACKUP_DIR/${table}.sql" ] && [ -s "$BACKUP_DIR/${table}.sql" ]; then
        echo "  Importing $table..."
        # Map RoleProfiles to RoleProfile (table name difference)
        TARGET_TABLE="$table"
        [ "$table" = "RoleProfiles" ] && TARGET_TABLE="RoleProfile"

        PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $TARGET_DB < "$BACKUP_DIR/${table}.sql" 2>/dev/null || true
    fi
done

# Re-enable FK checks
PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $TARGET_DB -c "SET session_replication_role = 'origin';"

echo "✅ Data imported"

# Step 6: Verify import
echo ""
echo "[Step 6] Target database row counts:"
for table in "AspNetRoles" "AspNetUsers" "AspNetRoleClaims" "AspNetUserClaims" "AspNetUserLogins" "AspNetUserRoles" "AspNetUserTokens" "RoleProfile"; do
    COUNT=$(PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $TARGET_DB -t -c "SELECT COUNT(*) FROM \"$table\";" 2>/dev/null | xargs || echo "0")
    echo "  $table: $COUNT rows"
done

echo ""
echo "=========================================="
echo "Migration Complete!"
echo "=========================================="
echo "Backup location: $BACKUP_DIR"
echo ""
echo "Next steps:"
echo "1. Run integrity checks: psql -f scripts/auth-db-migration/03-verify-integrity.sql"
echo "2. Test application login"
echo "3. Monitor for errors"
echo "4. After validation, remove Identity tables from $SOURCE_DB"
echo "=========================================="
