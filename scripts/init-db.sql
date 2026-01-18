-- ============================================
-- GRC System Database Initialization Script
-- ============================================
-- Purpose: Initialize GRC database with extensions
-- For Docker: This runs on the main database created by POSTGRES_DB
-- Tables are created by EF Core migrations on first startup
-- ============================================

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Create schemas for organization
CREATE SCHEMA IF NOT EXISTS audit;
CREATE SCHEMA IF NOT EXISTS analytics;
CREATE SCHEMA IF NOT EXISTS integration;

-- Create auth database (for Identity tables)
-- Note: POSTGRES_DB is created automatically by the container
-- We create the _auth database here
DO $$
BEGIN
    -- Check if we're in the main database and create auth DB
    IF current_database() != 'postgres' THEN
        -- Create auth database with same name + _auth suffix
        PERFORM dblink_exec('dbname=postgres', 
            format('CREATE DATABASE %I WITH OWNER = current_user', current_database() || '_auth'));
    END IF;
EXCEPTION
    WHEN duplicate_database THEN
        RAISE NOTICE 'Auth database already exists';
    WHEN undefined_function THEN
        RAISE NOTICE 'dblink not available - auth database must be created manually';
    WHEN OTHERS THEN
        RAISE NOTICE 'Could not create auth database: %', SQLERRM;
END
$$;

-- Grant permissions
GRANT ALL ON SCHEMA public TO PUBLIC;
GRANT ALL ON SCHEMA audit TO PUBLIC;
GRANT ALL ON SCHEMA analytics TO PUBLIC;
GRANT ALL ON SCHEMA integration TO PUBLIC;

-- Create Hangfire schema (required before EF migrations)
CREATE SCHEMA IF NOT EXISTS hangfire;
GRANT ALL ON SCHEMA hangfire TO PUBLIC;

-- Log completion
DO $$
BEGIN
    RAISE NOTICE '============================================';
    RAISE NOTICE 'GRC Database Initialization Complete';
    RAISE NOTICE '============================================';
    RAISE NOTICE 'Extensions: uuid-ossp, pg_trgm, pgcrypto';
    RAISE NOTICE 'Schemas: public, audit, analytics, integration, hangfire';
    RAISE NOTICE '';
    RAISE NOTICE 'Tables will be created by EF Core migrations on first startup';
    RAISE NOTICE '============================================';
END
$$;
