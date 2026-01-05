-- Database initialization script
-- This script is run automatically when the PostgreSQL container starts
-- It creates necessary schemas and extensions needed for the GRC system

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Create schemas
CREATE SCHEMA IF NOT EXISTS public;

-- Log initialization
SELECT 'Database initialized with required extensions and schemas' as status;
