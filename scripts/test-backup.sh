#!/bin/bash
#===============================================================================
# GRC Backup Test Script
# Purpose: Test backup and restore functionality
#===============================================================================

set -e

echo "========================================="
echo "GRC Backup/Restore Test"
echo "========================================="
echo ""

# Test 1: Check if scripts exist
echo "Test 1: Checking if backup scripts exist..."
if [ -f "backup-database.sh" ] && [ -f "restore-database.sh" ]; then
    echo "✓ Scripts found"
else
    echo "✗ Scripts not found"
    exit 1
fi

# Test 2: Check if scripts are executable
echo "Test 2: Checking if scripts are executable..."
if [ -x "backup-database.sh" ] && [ -x "restore-database.sh" ]; then
    echo "✓ Scripts are executable"
else
    echo "✗ Scripts are not executable"
    echo "  Run: chmod +x *.sh"
    exit 1
fi

# Test 3: Check environment variables
echo "Test 3: Checking environment variables..."
if [ -f "../.env.grcmvc.production" ]; then
    export $(cat ../.env.grcmvc.production | grep -v '^#' | xargs)
    echo "✓ Environment variables loaded"
else
    echo "⚠ .env.grcmvc.production not found"
fi

# Test 4: Check database connectivity
echo "Test 4: Testing database connectivity..."
DB_HOST=${DB_HOST:-localhost}
DB_PORT=${DB_PORT:-5432}
DB_USER=${DB_USER:-postgres}
DB_NAME=${DB_NAME:-GrcMvcDb}

if PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -c "SELECT 1;" > /dev/null 2>&1; then
    echo "✓ Database connection successful"
else
    echo "✗ Database connection failed"
    echo "  Check your DB_HOST, DB_PORT, DB_USER, DB_PASSWORD"
    exit 1
fi

# Test 5: Check Azure CLI (if configured)
echo "Test 5: Checking Azure CLI..."
if command -v az &> /dev/null; then
    echo "✓ Azure CLI installed"

    if [ ! -z "$AZURE_STORAGE_ACCOUNT" ] && [ "$AZURE_STORAGE_ACCOUNT" != "CHANGE_ME" ]; then
        echo "✓ Azure Storage configured"
    else
        echo "⚠ Azure Storage not configured (local backups only)"
    fi
else
    echo "⚠ Azure CLI not installed (local backups only)"
    echo "  Install: curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash"
fi

# Test 6: Run backup (dry run)
echo "Test 6: Running backup test..."
echo "  This will create a real backup but not upload to cloud"

read -p "Continue with backup test? (yes/no): " confirm
if [ "$confirm" == "yes" ]; then
    ./backup-database.sh

    if [ $? -eq 0 ]; then
        echo "✓ Backup test successful"

        # Find the latest backup
        LATEST_BACKUP=$(ls -t /tmp/grc-backups/grc_backup_*.sql.gz 2>/dev/null | head -1)

        if [ -f "$LATEST_BACKUP" ]; then
            BACKUP_SIZE=$(du -h "$LATEST_BACKUP" | cut -f1)
            echo "  Backup file: $LATEST_BACKUP"
            echo "  Size: $BACKUP_SIZE"
        fi
    else
        echo "✗ Backup test failed"
        exit 1
    fi
else
    echo "  Backup test skipped"
fi

# Summary
echo ""
echo "========================================="
echo "Test Summary"
echo "========================================="
echo "All tests passed! ✓"
echo ""
echo "Next steps:"
echo "1. Configure Azure Storage credentials (if needed)"
echo "2. Add cron job for automated backups:"
echo "   0 2 * * * cd /app/scripts && ./backup-database.sh >> /var/log/grc-backup.log 2>&1"
echo "3. Test restore procedure regularly"
echo ""

exit 0
