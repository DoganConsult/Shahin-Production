#!/bin/bash
# ══════════════════════════════════════════════════════════════════════════════
# SHAHIN AI GRC - DATABASE BACKUP SCRIPT
# ══════════════════════════════════════════════════════════════════════════════
# Purpose: Automated PostgreSQL backups with encryption and retention
# Schedule: Daily at 2 AM (configurable via BACKUP_SCHEDULE)
# Retention: 30 days (configurable via BACKUP_RETENTION_DAYS)
# ══════════════════════════════════════════════════════════════════════════════

set -e

# Configuration
BACKUP_DIR="/backups"
POSTGRES_HOST="${POSTGRES_HOST:-postgres}"
POSTGRES_PORT="${POSTGRES_PORT:-5432}"
POSTGRES_DB="${POSTGRES_DB:-grc_production}"
POSTGRES_USER="${POSTGRES_USER:-postgres}"
BACKUP_RETENTION_DAYS="${BACKUP_RETENTION_DAYS:-30}"
ENCRYPTION_KEY="${BACKUP_ENCRYPTION_KEY:-ShahinAIBackup2026}"

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${GREEN}[INFO]${NC} $(date '+%Y-%m-%d %H:%M:%S') - $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $(date '+%Y-%m-%d %H:%M:%S') - $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $(date '+%Y-%m-%d %H:%M:%S') - $1"
}

# ══════════════════════════════════════════════════════════════════════════════
# FUNCTION: Create Backup
# ══════════════════════════════════════════════════════════════════════════════
create_backup() {
    local timestamp=$(date '+%Y%m%d_%H%M%S')
    local backup_file="${BACKUP_DIR}/grc_backup_${timestamp}.sql"
    local compressed_file="${backup_file}.gz"
    local encrypted_file="${compressed_file}.enc"

    log_info "Starting backup for database: ${POSTGRES_DB}"

    # Wait for PostgreSQL to be ready
    log_info "Waiting for PostgreSQL to be ready..."
    until pg_isready -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER"; do
        log_warn "PostgreSQL is not ready yet. Retrying in 5 seconds..."
        sleep 5
    done

    log_info "PostgreSQL is ready. Creating backup..."

    # Create SQL dump
    if PGPASSWORD="$POSTGRES_PASSWORD" pg_dump \
        -h "$POSTGRES_HOST" \
        -p "$POSTGRES_PORT" \
        -U "$POSTGRES_USER" \
        -d "$POSTGRES_DB" \
        --format=plain \
        --no-owner \
        --no-acl \
        --verbose \
        > "$backup_file" 2>&1; then

        log_info "Database dump created successfully: $(basename "$backup_file")"
    else
        log_error "Failed to create database dump"
        return 1
    fi

    # Compress the backup
    log_info "Compressing backup..."
    if gzip "$backup_file"; then
        log_info "Backup compressed: $(basename "$compressed_file")"
    else
        log_error "Failed to compress backup"
        return 1
    fi

    # Encrypt the backup
    log_info "Encrypting backup..."
    if openssl enc -aes-256-cbc -salt -pbkdf2 \
        -in "$compressed_file" \
        -out "$encrypted_file" \
        -k "$ENCRYPTION_KEY"; then

        log_info "Backup encrypted: $(basename "$encrypted_file")"
        rm "$compressed_file"  # Remove unencrypted compressed file
    else
        log_error "Failed to encrypt backup"
        return 1
    fi

    # Calculate file size and checksum
    local file_size=$(du -h "$encrypted_file" | cut -f1)
    local checksum=$(sha256sum "$encrypted_file" | cut -d' ' -f1)

    log_info "Backup completed successfully!"
    log_info "  - File: $(basename "$encrypted_file")"
    log_info "  - Size: $file_size"
    log_info "  - SHA256: $checksum"

    # Write metadata
    cat > "${encrypted_file}.meta" <<EOF
Backup Metadata
===============
Date: $(date '+%Y-%m-%d %H:%M:%S')
Database: ${POSTGRES_DB}
Host: ${POSTGRES_HOST}
Size: ${file_size}
SHA256: ${checksum}
EOF

    return 0
}

# ══════════════════════════════════════════════════════════════════════════════
# FUNCTION: Cleanup Old Backups
# ══════════════════════════════════════════════════════════════════════════════
cleanup_old_backups() {
    log_info "Cleaning up backups older than ${BACKUP_RETENTION_DAYS} days..."

    local deleted_count=0
    while IFS= read -r -d '' file; do
        log_info "Deleting old backup: $(basename "$file")"
        rm -f "$file" "${file}.meta"
        ((deleted_count++))
    done < <(find "$BACKUP_DIR" -name "grc_backup_*.sql.gz.enc" -type f -mtime +$BACKUP_RETENTION_DAYS -print0)

    if [ $deleted_count -gt 0 ]; then
        log_info "Deleted $deleted_count old backup(s)"
    else
        log_info "No old backups to delete"
    fi
}

# ══════════════════════════════════════════════════════════════════════════════
# FUNCTION: List Backups
# ══════════════════════════════════════════════════════════════════════════════
list_backups() {
    log_info "Available backups:"
    echo ""
    printf "%-40s %-12s %-20s\n" "Filename" "Size" "Date"
    printf "%-40s %-12s %-20s\n" "========" "====" "===="

    find "$BACKUP_DIR" -name "grc_backup_*.sql.gz.enc" -type f -printf "%f %s %TY-%Tm-%Td %TH:%TM\n" | \
    while read -r filename size date time; do
        size_human=$(numfmt --to=iec-i --suffix=B $size 2>/dev/null || echo "${size}B")
        printf "%-40s %-12s %-20s\n" "$filename" "$size_human" "$date $time"
    done
    echo ""
}

# ══════════════════════════════════════════════════════════════════════════════
# FUNCTION: Verify Backup
# ══════════════════════════════════════════════════════════════════════════════
verify_backup() {
    local backup_file="$1"

    if [ ! -f "$backup_file" ]; then
        log_error "Backup file not found: $backup_file"
        return 1
    fi

    log_info "Verifying backup: $(basename "$backup_file")"

    # Check if metadata exists
    if [ -f "${backup_file}.meta" ]; then
        local stored_checksum=$(grep "SHA256:" "${backup_file}.meta" | cut -d' ' -f2)
        local current_checksum=$(sha256sum "$backup_file" | cut -d' ' -f1)

        if [ "$stored_checksum" == "$current_checksum" ]; then
            log_info "✓ Checksum verification passed"
            return 0
        else
            log_error "✗ Checksum verification failed"
            log_error "  Expected: $stored_checksum"
            log_error "  Got: $current_checksum"
            return 1
        fi
    else
        log_warn "No metadata file found. Cannot verify checksum."
        return 1
    fi
}

# ══════════════════════════════════════════════════════════════════════════════
# FUNCTION: Restore Backup
# ══════════════════════════════════════════════════════════════════════════════
restore_backup() {
    local encrypted_file="$1"

    if [ ! -f "$encrypted_file" ]; then
        log_error "Backup file not found: $encrypted_file"
        return 1
    fi

    log_warn "⚠️  WARNING: This will restore the database and may overwrite existing data!"
    log_info "Restoring from: $(basename "$encrypted_file")"

    local compressed_file="${encrypted_file%.enc}"
    local sql_file="${compressed_file%.gz}"

    # Decrypt
    log_info "Decrypting backup..."
    if ! openssl enc -aes-256-cbc -d -pbkdf2 \
        -in "$encrypted_file" \
        -out "$compressed_file" \
        -k "$ENCRYPTION_KEY"; then
        log_error "Failed to decrypt backup"
        return 1
    fi

    # Decompress
    log_info "Decompressing backup..."
    if ! gunzip "$compressed_file"; then
        log_error "Failed to decompress backup"
        return 1
    fi

    # Restore to database
    log_info "Restoring to database..."
    if PGPASSWORD="$POSTGRES_PASSWORD" psql \
        -h "$POSTGRES_HOST" \
        -p "$POSTGRES_PORT" \
        -U "$POSTGRES_USER" \
        -d "$POSTGRES_DB" \
        < "$sql_file"; then

        log_info "Database restored successfully!"
        rm "$sql_file"
        return 0
    else
        log_error "Failed to restore database"
        return 1
    fi
}

# ══════════════════════════════════════════════════════════════════════════════
# MAIN EXECUTION
# ══════════════════════════════════════════════════════════════════════════════

case "${1:-schedule}" in
    schedule)
        log_info "Starting backup scheduler..."
        log_info "Schedule: ${BACKUP_SCHEDULE:-0 2 * * *} (Daily at 2 AM)"
        log_info "Retention: $BACKUP_RETENTION_DAYS days"

        # Install cronie if not present
        if ! command -v crond &> /dev/null; then
            apk add --no-cache dcron
        fi

        # Create cron job
        echo "${BACKUP_SCHEDULE:-0 2 * * *} /backup/backup-entrypoint.sh backup >> /var/log/backup.log 2>&1" | crontab -

        # Start cron in foreground
        crond -f -l 2
        ;;

    backup)
        create_backup
        cleanup_old_backups
        ;;

    list)
        list_backups
        ;;

    verify)
        if [ -z "$2" ]; then
            log_error "Usage: $0 verify <backup_file>"
            exit 1
        fi
        verify_backup "$2"
        ;;

    restore)
        if [ -z "$2" ]; then
            log_error "Usage: $0 restore <backup_file>"
            exit 1
        fi
        restore_backup "$2"
        ;;

    *)
        echo "Usage: $0 {schedule|backup|list|verify <file>|restore <file>}"
        exit 1
        ;;
esac
