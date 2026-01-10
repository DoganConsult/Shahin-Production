#!/bin/bash
#===============================================================================
# GRC Database Backup Script
# Purpose: Backup PostgreSQL database to Azure Blob Storage
# Usage: ./backup-database.sh
#===============================================================================

set -e  # Exit on error

# Load environment variables
if [ -f ../.env.grcmvc.production ]; then
    export $(cat ../.env.grcmvc.production | grep -v '^#' | xargs)
fi

# Configuration
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="grc_backup_${TIMESTAMP}.sql.gz"
TEMP_DIR="/tmp/grc-backups"
DB_NAME=${DB_NAME:-GrcMvcDb}
DB_USER=${DB_USER:-postgres}
DB_HOST=${DB_HOST:-localhost}
DB_PORT=${DB_PORT:-5432}
CONTAINER_NAME=${BACKUP_CONTAINER:-grc-backups}
RETENTION_DAYS=${BACKUP_RETENTION_DAYS:-30}

# Colors for output
RED='\033[0:31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}GRC Database Backup Started${NC}"
echo -e "${GREEN}========================================${NC}"
echo "Timestamp: $(date)"
echo "Database: $DB_NAME"
echo "Backup file: $BACKUP_FILE"
echo ""

# Create temp directory
mkdir -p $TEMP_DIR

# Step 1: Create database backup
echo -e "${YELLOW}Step 1: Creating database backup...${NC}"
PGPASSWORD=$DB_PASSWORD pg_dump -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME --format=custom --verbose | gzip > $TEMP_DIR/$BACKUP_FILE

if [ $? -eq 0 ]; then
    BACKUP_SIZE=$(du -h $TEMP_DIR/$BACKUP_FILE | cut -f1)
    echo -e "${GREEN}✓ Backup created successfully (Size: $BACKUP_SIZE)${NC}"
else
    echo -e "${RED}✗ Backup failed!${NC}"
    exit 1
fi

# Step 2: Upload to Azure Blob Storage (if configured)
if [ ! -z "$AZURE_STORAGE_ACCOUNT" ] && [ "$AZURE_STORAGE_ACCOUNT" != "CHANGE_ME" ]; then
    echo -e "${YELLOW}Step 2: Uploading to Azure Blob Storage...${NC}"

    az storage blob upload \
        --account-name $AZURE_STORAGE_ACCOUNT \
        --container-name $CONTAINER_NAME \
        --name $BACKUP_FILE \
        --file $TEMP_DIR/$BACKUP_FILE \
        --auth-mode key \
        --account-key $AZURE_STORAGE_KEY

    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Uploaded to Azure successfully${NC}"
    else
        echo -e "${RED}✗ Upload failed!${NC}"
        exit 1
    fi

    # Step 3: Cleanup old backups (keep last N days)
    echo -e "${YELLOW}Step 3: Cleaning up old backups (retention: ${RETENTION_DAYS} days)...${NC}"

    CUTOFF_DATE=$(date -d "$RETENTION_DAYS days ago" +%Y%m%d)

    az storage blob list \
        --account-name $AZURE_STORAGE_ACCOUNT \
        --container-name $CONTAINER_NAME \
        --auth-mode key \
        --account-key $AZURE_STORAGE_KEY \
        --query "[?properties.creationTime<'${CUTOFF_DATE}'].name" \
        --output tsv | while read blob; do

        echo "Deleting old backup: $blob"
        az storage blob delete \
            --account-name $AZURE_STORAGE_ACCOUNT \
            --container-name $CONTAINER_NAME \
            --name $blob \
            --auth-mode key \
            --account-key $AZURE_STORAGE_KEY
    done

    echo -e "${GREEN}✓ Old backups cleaned up${NC}"
else
    echo -e "${YELLOW}⚠ Azure Storage not configured - backup saved locally only${NC}"
    echo -e "${YELLOW}  Configure AZURE_STORAGE_ACCOUNT and AZURE_STORAGE_KEY to enable cloud backups${NC}"
fi

# Step 4: Local cleanup (keep last 7 days locally)
echo -e "${YELLOW}Step 4: Cleaning up local temp files...${NC}"
find $TEMP_DIR -name "grc_backup_*.sql.gz" -mtime +7 -delete
echo -e "${GREEN}✓ Local cleanup complete${NC}"

# Summary
echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Backup Completed Successfully!${NC}"
echo -e "${GREEN}========================================${NC}"
echo "Backup file: $BACKUP_FILE"
echo "Location: $TEMP_DIR/$BACKUP_FILE"
if [ ! -z "$AZURE_STORAGE_ACCOUNT" ] && [ "$AZURE_STORAGE_ACCOUNT" != "CHANGE_ME" ]; then
    echo "Cloud backup: Azure Blob Storage ($CONTAINER_NAME)"
fi
echo "Timestamp: $(date)"
echo ""

# Exit successfully
exit 0
