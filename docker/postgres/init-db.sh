#!/bin/bash
set -e

echo "Initializing Shahin GRC Database..."

# Create required extensions and schemas
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    -- Enable required extensions
    CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
    CREATE EXTENSION IF NOT EXISTS "pg_trgm";
    CREATE EXTENSION IF NOT EXISTS "pgcrypto";

    -- Create schemas for different domains
    CREATE SCHEMA IF NOT EXISTS audit;
    CREATE SCHEMA IF NOT EXISTS analytics;
    CREATE SCHEMA IF NOT EXISTS integration;
    CREATE SCHEMA IF NOT EXISTS hangfire;

    -- Grant permissions on schemas
    GRANT ALL ON SCHEMA public TO PUBLIC;
    GRANT ALL ON SCHEMA audit TO PUBLIC;
    GRANT ALL ON SCHEMA analytics TO PUBLIC;
    GRANT ALL ON SCHEMA integration TO PUBLIC;
    GRANT ALL ON SCHEMA hangfire TO PUBLIC;

    -- Set search path to include all schemas
    ALTER DATABASE "$POSTGRES_DB" SET search_path TO public, audit, analytics, integration, hangfire;
EOSQL

echo "Database initialization complete!"
echo "Extensions enabled: uuid-ossp, pg_trgm, pgcrypto"
echo "Schemas created: public, audit, analytics, integration, hangfire"
