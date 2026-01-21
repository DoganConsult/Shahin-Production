#!/bin/bash

# PostgreSQL Database Restore Script for GrcMvc
# Restores database from backup file

set -e

# Configuration
DB_NAME="GrcMvcDb"
DB_USER="postgres"
CONTAINER_NAME="grcmvc-db"

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Check if backup file is provided
if [ -z "$1" ]; then
    echo -e "${RED}❌ Error: No backup file specified!${NC}"
    echo ""
    echo "Usage: $0 <backup-file>"
    echo ""
    echo "Example:"
    echo "  $0 /backups/postgresql/grcmvc_20260120_020000.sql.gz"
    echo ""
    echo "Available backups:"
    ls -lh /backups/postgresql/grcmvc_*.sql.gz 2>/dev/null || echo "  No backups found"
    exit 1
fi

BACKUP_FILE="$1"

# Check if backup file exists
if [ ! -f "$BACKUP_FILE" ]; then
    echo -e "${RED}❌ Error: Backup file not found: $BACKUP_FILE${NC}"
    exit 1
fi

echo -e "${YELLOW}⚠️  WARNING: This will REPLACE the current database!${NC}"
echo "Database: $DB_NAME"
echo "Backup file: $BACKUP_FILE"
echo ""
read -p "Are you sure you want to continue? (yes/no): " CONFIRM

if [ "$CONFIRM" != "yes" ]; then
    echo "Restore cancelled."
    exit 0
fi

# Check if container is running
if ! docker ps | grep -q "$CONTAINER_NAME"; then
    echo -e "${RED}❌ Error: Container $CONTAINER_NAME is not running!${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}🗄️  Starting database restore...${NC}"

# Drop existing connections
echo -e "${YELLOW}🔌 Dropping existing connections...${NC}"
docker exec "$CONTAINER_NAME" psql -U "$DB_USER" -d postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '$DB_NAME' AND pid <> pg_backend_pid();" || true

# Drop and recreate database
echo -e "${YELLOW}🗑️  Dropping existing database...${NC}"
docker exec "$CONTAINER_NAME" psql -U "$DB_USER" -d postgres -c "DROP DATABASE IF EXISTS \"$DB_NAME\";"

echo -e "${YELLOW}🆕 Creating new database...${NC}"
docker exec "$CONTAINER_NAME" psql -U "$DB_USER" -d postgres -c "CREATE DATABASE \"$DB_NAME\";"

# Restore from backup
echo -e "${YELLOW}📥 Restoring from backup...${NC}"
if gunzip -c "$BACKUP_FILE" | docker exec -i "$CONTAINER_NAME" psql -U "$DB_USER" "$DB_NAME"; then
    echo -e "${GREEN}✅ Database restored successfully!${NC}"
else
    echo -e "${RED}❌ Restore failed!${NC}"
    exit 1
fi

# Verify restore
echo ""
echo -e "${GREEN}🔍 Verifying restore...${NC}"
TABLE_COUNT=$(docker exec "$CONTAINER_NAME" psql -U "$DB_USER" -d "$DB_NAME" -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';")
echo "Tables restored: $TABLE_COUNT"

echo ""
echo -e "${GREEN}🎉 Restore process complete!${NC}"
echo ""
echo "⚠️  IMPORTANT: Restart the application to ensure all connections are refreshed:"
echo "  docker-compose -f docker-compose.grcmvc.yml restart grcmvc-app"
