-- ============================================
-- GRC System Database Initialization Script
-- ============================================
-- Purpose: Initialize GRC databases with proper separation
-- Author: Shahin AI Team
-- Date: 2026-01-13
-- ============================================

-- ============================================
-- 1. Create GrcMvcDb (Main Application Database)
-- ============================================
-- This database contains all application data:
-- - Entities, Workflows, GRC data
-- - Assessments, Risks, Controls, Evidence
-- - Multi-tenant data with workspace scoping

SELECT 'Creating GrcMvcDb database...' AS Status;

-- Check if database exists
SELECT 'Checking if GrcMvcDb exists...' AS Status;
DO
$$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_database WHERE datname = 'GrcMvcDb') THEN
        -- Database creation must be done outside of transaction block
        -- This will be executed by the calling script
        RAISE NOTICE 'GrcMvcDb does not exist and needs to be created';
    ELSE
        RAISE NOTICE 'GrcMvcDb already exists';
    END IF;
END
$$;

-- Note: Database creation must be done in a separate statement
-- CREATE DATABASE "GrcMvcDb"
--     WITH OWNER = postgres
--     ENCODING = 'UTF8'
--     LC_COLLATE = 'en_US.utf8'
--     LC_CTYPE = 'en_US.utf8'
--     TEMPLATE = template0;


-- ============================================
-- 2. Create GrcAuthDb (Authentication Database)
-- ============================================
-- This database contains ONLY authentication/identity data:
-- - ASP.NET Identity tables (AspNetUsers, AspNetRoles, etc.)
-- - Security isolation from application data
-- - Separate for enhanced security and compliance

SELECT 'Creating GrcAuthDb database...' AS Status;

-- Check if database exists
DO
$$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_database WHERE datname = 'GrcAuthDb') THEN
        RAISE NOTICE 'GrcAuthDb does not exist and needs to be created';
    ELSE
        RAISE NOTICE 'GrcAuthDb already exists';
    END IF;
END
$$;

-- Note: Database creation must be done in a separate statement
-- CREATE DATABASE "GrcAuthDb"
--     WITH OWNER = postgres
--     ENCODING = 'UTF8'
--     LC_COLLATE = 'en_US.utf8'
--     LC_CTYPE = 'en_US.utf8'
--     TEMPLATE = template0;


-- ============================================
-- 3. Setup Extensions for GrcMvcDb
-- ============================================

\c GrcMvcDb

SELECT 'Setting up extensions for GrcMvcDb...' AS Status;

-- UUID extension for GUID support
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- PostgreSQL full-text search
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- PostGIS for geospatial data (if needed for location-based compliance)
-- CREATE EXTENSION IF NOT EXISTS "postgis";

SELECT 'GrcMvcDb extensions installed successfully' AS Status;


-- ============================================
-- 4. Setup Extensions for GrcAuthDb
-- ============================================

\c GrcAuthDb

SELECT 'Setting up extensions for GrcAuthDb...' AS Status;

-- UUID extension for GUID support
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- PostgreSQL full-text search
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

SELECT 'GrcAuthDb extensions installed successfully' AS Status;


-- ============================================
-- 5. Create Schemas (if needed)
-- ============================================

\c GrcMvcDb

-- Create audit schema for audit logging
CREATE SCHEMA IF NOT EXISTS audit;

-- Create analytics schema for reporting
CREATE SCHEMA IF NOT EXISTS analytics;

SELECT 'GrcMvcDb schemas created successfully' AS Status;


-- ============================================
-- 6. Grant Permissions
-- ============================================

\c GrcMvcDb

-- Grant all privileges to postgres user
GRANT ALL PRIVILEGES ON DATABASE "GrcMvcDb" TO postgres;
GRANT ALL PRIVILEGES ON SCHEMA public TO postgres;
GRANT ALL PRIVILEGES ON SCHEMA audit TO postgres;
GRANT ALL PRIVILEGES ON SCHEMA analytics TO postgres;

SELECT 'GrcMvcDb permissions granted successfully' AS Status;

\c GrcAuthDb

-- Grant all privileges to postgres user
GRANT ALL PRIVILEGES ON DATABASE "GrcAuthDb" TO postgres;
GRANT ALL PRIVILEGES ON SCHEMA public TO postgres;

SELECT 'GrcAuthDb permissions granted successfully' AS Status;


-- ============================================
-- 7. Summary
-- ============================================

SELECT '============================================' AS Summary;
SELECT 'GRC Database Initialization Complete' AS Summary;
SELECT '============================================' AS Summary;
SELECT '' AS Summary;
SELECT 'Created Databases:' AS Summary;
SELECT '  - GrcMvcDb (Main Application Database)' AS Summary;
SELECT '  - GrcAuthDb (Authentication Database)' AS Summary;
SELECT '' AS Summary;
SELECT 'Next Steps:' AS Summary;
SELECT '  1. Update appsettings.json connection strings' AS Summary;
SELECT '  2. Run EF Core migrations for GrcDbContext' AS Summary;
SELECT '  3. Run EF Core migrations for GrcAuthDbContext' AS Summary;
SELECT '  4. Verify database separation is working' AS Summary;
SELECT '============================================' AS Summary;


-- ============================================
-- END OF SCRIPT
-- ============================================
