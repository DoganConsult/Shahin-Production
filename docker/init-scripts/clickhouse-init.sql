-- ============================================================================
-- SHAHIN GRC PLATFORM - ClickHouse Analytics Initialization
-- Created: January 13, 2026
-- Purpose: Initialize OLAP database for analytics and reporting
-- ============================================================================

-- Create analytics database
CREATE DATABASE IF NOT EXISTS shahin_analytics;

-- Use the analytics database
USE shahin_analytics;

-- ============================================================================
-- Compliance Metrics Table - Time-series data
-- ============================================================================
CREATE TABLE IF NOT EXISTS compliance_metrics (
    tenant_id UUID,
    metric_date Date,
    metric_timestamp DateTime64(3),
    framework_id UUID,
    framework_name String,
    control_id UUID,
    control_name String,
    compliance_score Float32,
    risk_score Float32,
    maturity_level UInt8,
    evidence_count UInt32,
    open_findings UInt32,
    closed_findings UInt32,
    created_at DateTime64(3) DEFAULT now64(3)
) ENGINE = MergeTree()
PARTITION BY toYYYYMM(metric_date)
ORDER BY (tenant_id, metric_date, framework_id, control_id)
TTL metric_date + INTERVAL 3 YEAR;

-- ============================================================================
-- Risk Events Table - Risk tracking over time
-- ============================================================================
CREATE TABLE IF NOT EXISTS risk_events (
    id UUID DEFAULT generateUUIDv4(),
    tenant_id UUID,
    event_date Date,
    event_timestamp DateTime64(3),
    risk_id UUID,
    risk_name String,
    risk_category String,
    severity Enum8('Critical' = 1, 'High' = 2, 'Medium' = 3, 'Low' = 4, 'Info' = 5),
    likelihood Float32,
    impact Float32,
    risk_score Float32,
    status String,
    owner_id UUID,
    owner_name String,
    created_at DateTime64(3) DEFAULT now64(3)
) ENGINE = MergeTree()
PARTITION BY toYYYYMM(event_date)
ORDER BY (tenant_id, event_date, risk_id)
TTL event_date + INTERVAL 5 YEAR;

-- ============================================================================
-- Audit Trail Analytics - Aggregated audit data
-- ============================================================================
CREATE TABLE IF NOT EXISTS audit_analytics (
    tenant_id UUID,
    audit_date Date,
    hour UInt8,
    user_id UUID,
    user_name String,
    action String,
    entity_type String,
    event_count UInt64,
    unique_entities UInt64,
    avg_response_time_ms Float32,
    created_at DateTime64(3) DEFAULT now64(3)
) ENGINE = SummingMergeTree()
PARTITION BY toYYYYMM(audit_date)
ORDER BY (tenant_id, audit_date, hour, user_id, action, entity_type);

-- ============================================================================
-- User Activity Metrics
-- ============================================================================
CREATE TABLE IF NOT EXISTS user_activity (
    tenant_id UUID,
    activity_date Date,
    user_id UUID,
    user_name String,
    session_count UInt32,
    page_views UInt32,
    actions_performed UInt32,
    time_spent_seconds UInt64,
    last_activity DateTime64(3),
    created_at DateTime64(3) DEFAULT now64(3)
) ENGINE = ReplacingMergeTree(created_at)
PARTITION BY toYYYYMM(activity_date)
ORDER BY (tenant_id, activity_date, user_id);

-- ============================================================================
-- Dashboard Metrics - Pre-aggregated for fast dashboard loading
-- ============================================================================
CREATE TABLE IF NOT EXISTS dashboard_metrics (
    tenant_id UUID,
    metric_date Date,
    metric_name String,
    metric_value Float64,
    metric_metadata String DEFAULT '',
    created_at DateTime64(3) DEFAULT now64(3)
) ENGINE = ReplacingMergeTree(created_at)
PARTITION BY toYYYYMM(metric_date)
ORDER BY (tenant_id, metric_date, metric_name);

-- ============================================================================
-- Materialized Views for Real-time Aggregations
-- ============================================================================

-- Daily compliance summary
CREATE MATERIALIZED VIEW IF NOT EXISTS mv_daily_compliance_summary
ENGINE = SummingMergeTree()
PARTITION BY toYYYYMM(metric_date)
ORDER BY (tenant_id, metric_date, framework_id)
AS SELECT
    tenant_id,
    metric_date,
    framework_id,
    framework_name,
    avg(compliance_score) as avg_compliance_score,
    avg(risk_score) as avg_risk_score,
    sum(evidence_count) as total_evidence,
    sum(open_findings) as total_open_findings,
    sum(closed_findings) as total_closed_findings,
    count() as control_count
FROM compliance_metrics
GROUP BY tenant_id, metric_date, framework_id, framework_name;

-- ============================================================================
-- Insert sample system metrics
-- ============================================================================
INSERT INTO dashboard_metrics (tenant_id, metric_date, metric_name, metric_value, metric_metadata)
VALUES 
    (generateUUIDv4(), today(), 'system.initialized', 1, 'ClickHouse analytics ready'),
    (generateUUIDv4(), today(), 'system.version', 1.0, 'Initial deployment');

SELECT '✅ ClickHouse analytics initialization complete!' as status;
