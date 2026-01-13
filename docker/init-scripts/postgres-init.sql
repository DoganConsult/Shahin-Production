-- ============================================================================
-- SHAHIN GRC PLATFORM - PostgreSQL Initialization Script
-- Created: January 13, 2026
-- Purpose: Initialize fresh database with multi-tenant support
-- ============================================================================

-- Create additional databases for multi-tenant architecture
CREATE DATABASE shahin_auth;
CREATE DATABASE shahin_hangfire;

-- Grant permissions
GRANT ALL PRIVILEGES ON DATABASE shahin_grc TO shahin_admin;
GRANT ALL PRIVILEGES ON DATABASE shahin_auth TO shahin_admin;
GRANT ALL PRIVILEGES ON DATABASE shahin_hangfire TO shahin_admin;

-- Connect to main GRC database
\c shahin_grc;

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- Create schemas for multi-tenancy
CREATE SCHEMA IF NOT EXISTS tenant_shared;
CREATE SCHEMA IF NOT EXISTS audit_log;
CREATE SCHEMA IF NOT EXISTS analytics;

-- Grant schema permissions
GRANT ALL ON SCHEMA tenant_shared TO shahin_admin;
GRANT ALL ON SCHEMA audit_log TO shahin_admin;
GRANT ALL ON SCHEMA analytics TO shahin_admin;

-- Create tenant registry table
CREATE TABLE IF NOT EXISTS tenant_shared.tenants (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(255) NOT NULL,
    slug VARCHAR(100) NOT NULL UNIQUE,
    connection_string TEXT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    metadata JSONB DEFAULT '{}'::jsonb
);

-- Create audit log table
CREATE TABLE IF NOT EXISTS audit_log.events (
    id BIGSERIAL PRIMARY KEY,
    tenant_id UUID,
    event_type VARCHAR(100) NOT NULL,
    entity_type VARCHAR(100),
    entity_id VARCHAR(100),
    user_id UUID,
    user_name VARCHAR(255),
    action VARCHAR(50) NOT NULL,
    old_values JSONB,
    new_values JSONB,
    ip_address INET,
    user_agent TEXT,
    correlation_id UUID,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Create index for audit log performance
CREATE INDEX idx_audit_tenant_created ON audit_log.events (tenant_id, created_at DESC);
CREATE INDEX idx_audit_entity ON audit_log.events (entity_type, entity_id);
CREATE INDEX idx_audit_user ON audit_log.events (user_id);

-- Create session tracking table
CREATE TABLE IF NOT EXISTS tenant_shared.user_sessions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    tenant_id UUID NOT NULL,
    token_hash VARCHAR(512) NOT NULL,
    ip_address INET,
    user_agent TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    is_revoked BOOLEAN DEFAULT false
);

CREATE INDEX idx_sessions_user ON tenant_shared.user_sessions (user_id);
CREATE INDEX idx_sessions_token ON tenant_shared.user_sessions (token_hash);

-- Create system configuration table
CREATE TABLE IF NOT EXISTS tenant_shared.system_config (
    key VARCHAR(255) PRIMARY KEY,
    value TEXT NOT NULL,
    description TEXT,
    is_encrypted BOOLEAN DEFAULT false,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_by UUID
);

-- Insert initial system configuration
INSERT INTO tenant_shared.system_config (key, value, description) VALUES
('system.version', '1.0.0', 'Current system version'),
('system.initialized_at', NOW()::TEXT, 'System initialization timestamp'),
('security.password_min_length', '12', 'Minimum password length'),
('security.lockout_threshold', '5', 'Failed login attempts before lockout'),
('security.lockout_duration_minutes', '30', 'Account lockout duration'),
('multitenancy.isolation_level', 'database', 'Tenant isolation strategy'),
('features.onboarding_enabled', 'true', 'Enable onboarding wizard'),
('features.ai_agents_enabled', 'true', 'Enable AI agent features');

-- ============================================================================
-- Connect to Auth Database and initialize
-- ============================================================================
\c shahin_auth;

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- OpenIddict tables will be created by EF Core migrations
-- This just ensures the database is ready

-- ============================================================================
-- Connect to Hangfire Database and initialize
-- ============================================================================
\c shahin_hangfire;

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Hangfire tables will be created automatically by Hangfire.PostgreSql

RAISE NOTICE '✅ PostgreSQL initialization complete!';
