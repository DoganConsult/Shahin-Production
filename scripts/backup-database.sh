#!/bin/bash

# PostgreSQL Database Backup Script for GrcMvc
# Performs automated backups with retention policy

set -e

# Configuration
BACKUP_DIR="/backups/postgresql"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
DB_NAME="GrcMvcDb"
DB_USER="postgres"
CONTAINER_NAME="grcmvc-db"
RETENTION_DAYS=30

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${GREEN}🗄️  Starting PostgreSQL Backup...${NC}"
echo "Database: $DB_NAME"
echo "Timestamp: $TIMESTAMP"

# Create backup directory if it doesn't exist
mkdir -p "$BACKUP_DIR"

# Check if container is running
if ! docker ps | grep -q "$CONTAINER_NAME"; then
    echo -e "${RED}❌ Error: Container $CONTAINER_NAME is not running!${NC}"
    exit 1
fi

# Perform backup
echo -e "${YELLOW}📦 Creating backup...${NC}"
BACKUP_FILE="$BACKUP_DIR/grcmvc_${TIMESTAMP}.sql.gz"

if docker exec "$CONTAINER_NAME" pg_dump -U "$DB_USER" "$DB_NAME" | gzip > "$BACKUP_FILE"; then
    BACKUP_SIZE=$(du -h "$BACKUP_FILE" | cut -f1)
    echo -e "${GREEN}✅ Backup completed successfully!${NC}"
    echo "File: $BACKUP_FILE"
    echo "Size: $BACKUP_SIZE"
else
    echo -e "${RED}❌ Backup failed!${NC}"
    exit 1
fi

# Apply retention policy
echo -e "${YELLOW}🧹 Applying retention policy (${RETENTION_DAYS} days)...${NC}"
DELETED_COUNT=$(find "$BACKUP_DIR" -name "grcmvc_*.sql.gz" -mtime +$RETENTION_DAYS -delete -print | wc -l)

if [ "$DELETED_COUNT" -gt 0 ]; then
    echo -e "${GREEN}Deleted $DELETED_COUNT old backup(s)${NC}"
else
    echo "No old backups to delete"
fi

# List recent backups
echo ""
echo -e "${GREEN}📋 Recent backups:${NC}"
ls -lh "$BACKUP_DIR"/grcmvc_*.sql.gz | tail -5

# Calculate total backup size
TOTAL_SIZE=$(du -sh "$BACKUP_DIR" | cut -f1)
echo ""
echo -e "${GREEN}💾 Total backup size: $TOTAL_SIZE${NC}"

echo ""
echo -e "${GREEN}🎉 Backup process complete!${NC}"
