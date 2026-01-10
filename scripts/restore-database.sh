#!/bin/bash
#===============================================================================
# GRC Database Restore Script
# Purpose: Restore PostgreSQL database from backup
# Usage: ./restore-database.sh <backup-file>
#===============================================================================

set -e  # Exit on error

# Load environment variables
if [ -f ../.env.grcmvc.production ]; then
    export $(cat ../.env.grcmvc.production | grep -v '^#' | xargs)
fi

# Configuration
DB_NAME=${DB_NAME:-GrcMvcDb}
DB_USER=${DB_USER:-postgres}
DB_HOST=${DB_HOST:-localhost}
DB_PORT=${DB_PORT:-5432}
TEMP_DIR="/tmp/grc-backups"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Check if backup file provided
if [ -z "$1" ]; then
    echo -e "${RED}Error: No backup file specified${NC}"
    echo "Usage: ./restore-database.sh <backup-file>"
    echo ""
    echo "Available backups:"
    ls -lh $TEMP_DIR/grc_backup_*.sql.gz 2>/dev/null || echo "  No local backups found"
    exit 1
fi

BACKUP_FILE=$1

# Check if file exists locally
if [ ! -f "$BACKUP_FILE" ] && [ ! -f "$TEMP_DIR/$BACKUP_FILE" ]; then
    # Try to download from Azure
    if [ ! -z "$AZURE_STORAGE_ACCOUNT" ] && [ "$AZURE_STORAGE_ACCOUNT" != "CHANGE_ME" ]; then
        echo -e "${YELLOW}Downloading backup from Azure...${NC}"
        az storage blob download \
            --account-name $AZURE_STORAGE_ACCOUNT \
            --container-name ${BACKUP_CONTAINER:-grc-backups} \
            --name $BACKUP_FILE \
            --file $TEMP_DIR/$BACKUP_FILE \
            --auth-mode key \
            --account-key $AZURE_STORAGE_KEY

        BACKUP_FILE="$TEMP_DIR/$BACKUP_FILE"
    else
        echo -e "${RED}Error: Backup file not found and Azure not configured${NC}"
        exit 1
    fi
fi

# Resolve full path
if [ -f "$TEMP_DIR/$BACKUP_FILE" ]; then
    BACKUP_FILE="$TEMP_DIR/$BACKUP_FILE"
fi

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}GRC Database Restore Started${NC}"
echo -e "${GREEN}========================================${NC}"
echo "Timestamp: $(date)"
echo "Database: $DB_NAME"
echo "Backup file: $BACKUP_FILE"
echo ""

# Warning
echo -e "${RED}WARNING: This will overwrite the existing database!${NC}"
read -p "Are you sure you want to continue? (yes/no): " confirm

if [ "$confirm" != "yes" ]; then
    echo "Restore cancelled."
    exit 0
fi

# Step 1: Terminate all connections
echo -e "${YELLOW}Step 1: Terminating active connections...${NC}"
PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d postgres -c \
    "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '$DB_NAME' AND pid <> pg_backend_pid();"

# Step 2: Drop and recreate database
echo -e "${YELLOW}Step 2: Recreating database...${NC}"
PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d postgres -c "DROP DATABASE IF EXISTS $DB_NAME;"
PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d postgres -c "CREATE DATABASE $DB_NAME;"

# Step 3: Restore backup
echo -e "${YELLOW}Step 3: Restoring database from backup...${NC}"
gunzip < $BACKUP_FILE | PGPASSWORD=$DB_PASSWORD pg_restore -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME --verbose

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Database restored successfully${NC}"
else
    echo -e "${RED}✗ Restore failed!${NC}"
    exit 1
fi

# Summary
echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Restore Completed Successfully!${NC}"
echo -e "${GREEN}========================================${NC}"
echo "Database: $DB_NAME"
echo "Restored from: $BACKUP_FILE"
echo "Timestamp: $(date)"
echo ""

exit 0
