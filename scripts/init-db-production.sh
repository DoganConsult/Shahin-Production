#!/bin/bash
# ══════════════════════════════════════════════════════════════════════════════
# SHAHIN AI GRC - Production Database Initialization
# ══════════════════════════════════════════════════════════════════════════════
# This script runs automatically when the PostgreSQL container starts
# for the first time (empty data volume).
# ══════════════════════════════════════════════════════════════════════════════

set -e

echo "════════════════════════════════════════════════════════════════════════════"
echo "SHAHIN AI GRC - Database Initialization"
echo "════════════════════════════════════════════════════════════════════════════"

# Use environment variables from docker-compose
DB_NAME="${POSTGRES_DB:-shahin_grc}"
DB_USER="${POSTGRES_USER:-shahin_admin}"

echo "Initializing database: $DB_NAME"
echo "User: $DB_USER"

# ──────────────────────────────────────────────────────────────────────────────
# Install extensions on the main database
# ──────────────────────────────────────────────────────────────────────────────
echo "Installing PostgreSQL extensions..."

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$DB_NAME" <<-EOSQL
    -- UUID extension for GUID support
    CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

    -- PostgreSQL full-text search trigram
    CREATE EXTENSION IF NOT EXISTS "pg_trgm";

    -- Create schemas for data organization
    CREATE SCHEMA IF NOT EXISTS audit;
    CREATE SCHEMA IF NOT EXISTS analytics;

    -- Grant privileges
    GRANT ALL PRIVILEGES ON SCHEMA public TO $DB_USER;
    GRANT ALL PRIVILEGES ON SCHEMA audit TO $DB_USER;
    GRANT ALL PRIVILEGES ON SCHEMA analytics TO $DB_USER;
EOSQL

echo "Extensions installed successfully."

# ──────────────────────────────────────────────────────────────────────────────
# Create GrcAuthDb if separate auth database is needed
# ──────────────────────────────────────────────────────────────────────────────
# Note: For production simplicity, we use a single database.
# Uncomment below if you need separate auth database.

# echo "Creating GrcAuthDb..."
# psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<-EOSQL
#     SELECT 'CREATE DATABASE "GrcAuthDb"'
#     WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'GrcAuthDb')\gexec
# EOSQL
#
# psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "GrcAuthDb" <<-EOSQL
#     CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
#     CREATE EXTENSION IF NOT EXISTS "pg_trgm";
# EOSQL

echo "════════════════════════════════════════════════════════════════════════════"
echo "Database initialization complete!"
echo "════════════════════════════════════════════════════════════════════════════"
echo ""
echo "Next steps:"
echo "  1. EF Core migrations will run automatically on app startup"
echo "  2. Verify with: docker exec -it shahin-grc-db psql -U $DB_USER -d $DB_NAME"
echo "════════════════════════════════════════════════════════════════════════════"
