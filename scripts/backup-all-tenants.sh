#!/bin/bash
# Backup all tenant databases
# Usage: ./backup-all-tenants.sh [backup-dir]

set -e

BACKUP_DIR="${1:-/backups/tenants}"
DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5432}"
DB_USER="${DB_USER:-postgres}"
DB_PASSWORD="${DB_PASSWORD}"

echo "=== Backup All Tenant Databases ==="
echo "Backup Directory: $BACKUP_DIR"
echo ""

# Create backup directory
mkdir -p "$BACKUP_DIR"

# Set PGPASSWORD
export PGPASSWORD="$DB_PASSWORD"

# Get list of all tenant databases
echo "Discovering tenant databases..."
TENANT_DBS=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres -t -c \
    "SELECT datname FROM pg_database WHERE datname LIKE 'grcmvc_tenant_%'")

if [ -z "$TENANT_DBS" ]; then
    echo "No tenant databases found"
    exit 0
fi

TENANT_COUNT=$(echo "$TENANT_DBS" | wc -l)
echo "Found $TENANT_COUNT tenant database(s)"
echo ""

# Backup each tenant database
SUCCESS_COUNT=0
FAIL_COUNT=0

while IFS= read -r DB_NAME; do
    DB_NAME=$(echo "$DB_NAME" | xargs) # Trim whitespace
    
    if [ -z "$DB_NAME" ]; then
        continue
    fi
    
    # Extract tenant ID from database name
    TENANT_ID_SANITIZED=$(echo "$DB_NAME" | sed 's/grcmvc_tenant_//')
    
    echo "Backing up: $DB_NAME"
    
    # Generate backup filename
    TIMESTAMP=$(date +%Y%m%d_%H%M%S)
    BACKUP_FILE="${BACKUP_DIR}/${DB_NAME}_${TIMESTAMP}.sql.gz"
    
    # Perform backup
    if pg_dump -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" \
        --verbose --clean --if-exists --create --format=plain | gzip > "$BACKUP_FILE"; then
        
        BACKUP_SIZE=$(du -h "$BACKUP_FILE" | cut -f1)
        echo "  ✅ Success - Size: $BACKUP_SIZE"
        ((SUCCESS_COUNT++))
    else
        echo "  ❌ Failed"
        ((FAIL_COUNT++))
    fi
    echo ""
done <<< "$TENANT_DBS"

# Summary
echo "=== Backup Summary ==="
echo "Successful: $SUCCESS_COUNT"
echo "Failed: $FAIL_COUNT"
echo "Total: $TENANT_COUNT"
echo ""

if [ $FAIL_COUNT -eq 0 ]; then
    echo "✅ All backups completed successfully"
    exit 0
else
    echo "⚠️  Some backups failed"
    exit 1
fi
