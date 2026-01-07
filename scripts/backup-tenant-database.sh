#!/bin/bash
# Backup script for tenant-specific databases
# Usage: ./backup-tenant-database.sh <tenant-id> [backup-dir]

set -e

TENANT_ID="${1}"
BACKUP_DIR="${2:-/backups/tenants}"
DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5432}"
DB_USER="${DB_USER:-postgres}"
DB_PASSWORD="${DB_PASSWORD}"

if [ -z "$TENANT_ID" ]; then
    echo "Usage: $0 <tenant-id> [backup-dir]"
    echo "Example: $0 123e4567-e89b-12d3-a456-426614174000"
    exit 1
fi

# Sanitize tenant ID (remove hyphens for database name)
TENANT_ID_SANITIZED=$(echo "$TENANT_ID" | tr -d '-')
DB_NAME="grcmvc_tenant_${TENANT_ID_SANITIZED}"

# Create backup directory if it doesn't exist
mkdir -p "$BACKUP_DIR"

# Generate backup filename with timestamp
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="${BACKUP_DIR}/${DB_NAME}_${TIMESTAMP}.sql"
BACKUP_FILE_COMPRESSED="${BACKUP_FILE}.gz"

echo "=== Tenant Database Backup ==="
echo "Tenant ID: $TENANT_ID"
echo "Database: $DB_NAME"
echo "Backup File: $BACKUP_FILE_COMPRESSED"
echo ""

# Set PGPASSWORD for non-interactive authentication
export PGPASSWORD="$DB_PASSWORD"

# Check if database exists
DB_EXISTS=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -lqt -d postgres | cut -d \| -f 1 | grep -qw "$DB_NAME" && echo "yes" || echo "no")

if [ "$DB_EXISTS" != "yes" ]; then
    echo "ERROR: Database $DB_NAME does not exist"
    exit 1
fi

# Perform backup
echo "Creating backup..."
pg_dump -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" \
    --verbose \
    --clean \
    --if-exists \
    --create \
    --format=plain \
    --file="$BACKUP_FILE"

if [ $? -eq 0 ]; then
    echo "Compressing backup..."
    gzip "$BACKUP_FILE"
    
    BACKUP_SIZE=$(du -h "$BACKUP_FILE_COMPRESSED" | cut -f1)
    echo ""
    echo "✅ Backup completed successfully"
    echo "   File: $BACKUP_FILE_COMPRESSED"
    echo "   Size: $BACKUP_SIZE"
    
    # Keep only last 30 backups (optional cleanup)
    echo ""
    echo "Cleaning old backups (keeping last 30)..."
    ls -t "${BACKUP_DIR}/${DB_NAME}_"*.sql.gz 2>/dev/null | tail -n +31 | xargs rm -f 2>/dev/null || true
    
    exit 0
else
    echo "ERROR: Backup failed"
    exit 1
fi
