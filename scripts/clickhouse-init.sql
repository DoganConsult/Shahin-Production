-- ClickHouse Analytics Database Initialization
-- Creates database, tables, and materialized views for GRC analytics

-- Create database
CREATE DATABASE IF NOT EXISTS grc_analytics;

USE grc_analytics;

-- Raw events table (receives CDC events from PostgreSQL)
CREATE TABLE IF NOT EXISTS events_raw
(
    event_id UUID,
    tenant_id UUID,
    event_type String,
    entity_type String,
    entity_id String,
    action String,
    actor String,
    payload String,
    event_timestamp DateTime,
    ingested_at DateTime DEFAULT now()
)
ENGINE = MergeTree()
PARTITION BY toYYYYMM(event_timestamp)
ORDER BY (tenant_id, event_timestamp, event_type)
TTL event_timestamp + INTERVAL 2 YEAR;

-- Dashboard snapshots (hourly aggregated metrics)
CREATE TABLE IF NOT EXISTS dashboard_snapshots
(
    tenant_id UUID,
    snapshot_date Date,
    snapshot_hour DateTime,
    
    -- Compliance metrics
    total_controls UInt32,
    compliant_controls UInt32,
    partial_controls UInt32,
    non_compliant_controls UInt32,
    not_started_controls UInt32,
    compliance_score Decimal(5, 2),
    
    -- Risk metrics
    total_risks UInt32,
    critical_risks UInt32,
    high_risks UInt32,
    medium_risks UInt32,
    low_risks UInt32,
    open_risks UInt32,
    mitigated_risks UInt32,
    risk_score_avg Decimal(5, 2),
    
    -- Task metrics
    total_tasks UInt32,
    pending_tasks UInt32,
    in_progress_tasks UInt32,
    completed_tasks UInt32,
    overdue_tasks UInt32,
    due_this_week UInt32,
    
    -- Evidence metrics
    total_evidence UInt32,
    evidence_submitted UInt32,
    evidence_approved UInt32,
    evidence_rejected UInt32,
    evidence_pending UInt32,
    
    -- Assessment metrics
    total_assessments UInt32,
    active_assessments UInt32,
    completed_assessments UInt32,
    
    -- Plan metrics
    total_plans UInt32,
    active_plans UInt32,
    completed_plans UInt32,
    overall_plan_progress Decimal(5, 2),
    
    created_at DateTime DEFAULT now(),
    updated_at DateTime DEFAULT now()
)
ENGINE = MergeTree()
PARTITION BY toYYYYMM(snapshot_date)
ORDER BY (tenant_id, snapshot_hour)
TTL snapshot_date + INTERVAL 1 YEAR;

-- Compliance trends (by framework and baseline)
CREATE TABLE IF NOT EXISTS compliance_trends
(
    tenant_id UUID,
    framework_code String,
    baseline_code String,
    measure_date Date,
    measure_hour DateTime,
    compliance_score Decimal(5, 2),
    total_controls UInt32,
    compliant_controls UInt32,
    partial_controls UInt32,
    non_compliant_controls UInt32,
    delta_from_previous Decimal(5, 2),
    created_at DateTime DEFAULT now()
)
ENGINE = MergeTree()
PARTITION BY toYYYYMM(measure_date)
ORDER BY (tenant_id, framework_code, measure_date)
TTL measure_date + INTERVAL 2 YEAR;

-- Risk heatmap (likelihood x impact matrix)
CREATE TABLE IF NOT EXISTS risk_heatmap
(
    tenant_id UUID,
    snapshot_date Date,
    likelihood UInt8,
    impact UInt8,
    risk_count UInt32,
    risk_ids Array(UUID),
    created_at DateTime DEFAULT now()
)
ENGINE = MergeTree()
PARTITION BY toYYYYMM(snapshot_date)
ORDER BY (tenant_id, snapshot_date, likelihood, impact)
TTL snapshot_date + INTERVAL 1 YEAR;

-- Framework comparison (multi-framework compliance scores)
CREATE TABLE IF NOT EXISTS framework_comparison
(
    tenant_id UUID,
    snapshot_date Date,
    framework_code String,
    framework_name String,
    total_requirements UInt32,
    compliant_count UInt32,
    partial_count UInt32,
    non_compliant_count UInt32,
    not_assessed_count UInt32,
    compliance_score Decimal(5, 2),
    maturity_level UInt8,
    trend_7d Decimal(5, 2),
    trend_30d Decimal(5, 2),
    created_at DateTime DEFAULT now()
)
ENGINE = MergeTree()
PARTITION BY toYYYYMM(snapshot_date)
ORDER BY (tenant_id, snapshot_date, framework_code)
TTL snapshot_date + INTERVAL 1 YEAR;

-- Task metrics by role
CREATE TABLE IF NOT EXISTS task_metrics_by_role
(
    tenant_id UUID,
    snapshot_date Date,
    role_code String,
    team_id UUID,
    total_tasks UInt32,
    pending_tasks UInt32,
    in_progress_tasks UInt32,
    completed_tasks UInt32,
    overdue_tasks UInt32,
    avg_completion_days Decimal(5, 2),
    sla_compliance_rate Decimal(5, 2),
    created_at DateTime DEFAULT now()
)
ENGINE = MergeTree()
PARTITION BY toYYYYMM(snapshot_date)
ORDER BY (tenant_id, snapshot_date, role_code)
TTL snapshot_date + INTERVAL 1 YEAR;

-- Evidence metrics
CREATE TABLE IF NOT EXISTS evidence_metrics
(
    tenant_id UUID,
    snapshot_date Date,
    evidence_type String,
    control_domain String,
    total_required UInt32,
    total_collected UInt32,
    total_approved UInt32,
    total_rejected UInt32,
    total_expired UInt32,
    collection_rate Decimal(5, 2),
    approval_rate Decimal(5, 2),
    avg_review_days Decimal(5, 2),
    created_at DateTime DEFAULT now()
)
ENGINE = MergeTree()
PARTITION BY toYYYYMM(snapshot_date)
ORDER BY (tenant_id, snapshot_date, evidence_type)
TTL snapshot_date + INTERVAL 1 YEAR;

-- Top actions (prioritized action items)
CREATE TABLE IF NOT EXISTS top_actions
(
    tenant_id UUID,
    snapshot_date Date,
    action_rank UInt8,
    action_type String,
    action_title String,
    action_description String,
    entity_type String,
    entity_id UUID,
    urgency String,
    due_date Nullable(DateTime),
    assigned_to String,
    created_at DateTime DEFAULT now()
)
ENGINE = MergeTree()
PARTITION BY toYYYYMM(snapshot_date)
ORDER BY (tenant_id, snapshot_date, action_rank)
TTL snapshot_date + INTERVAL 90 DAY;

-- User activity metrics
CREATE TABLE IF NOT EXISTS user_activity
(
    tenant_id UUID,
    user_id String,
    activity_date Date,
    login_count UInt32,
    tasks_completed UInt32,
    evidence_submitted UInt32,
    assessments_worked UInt32,
    approvals_given UInt32,
    session_minutes UInt32,
    last_activity DateTime,
    created_at DateTime DEFAULT now()
)
ENGINE = MergeTree()
PARTITION BY toYYYYMM(activity_date)
ORDER BY (tenant_id, activity_date, user_id)
TTL activity_date + INTERVAL 1 YEAR;

-- Materialized view: Real-time compliance score (last 24 hours)
CREATE MATERIALIZED VIEW IF NOT EXISTS compliance_score_realtime
ENGINE = SummingMergeTree()
PARTITION BY toYYYYMM(measure_date)
ORDER BY (tenant_id, framework_code, measure_hour)
AS SELECT
    tenant_id,
    framework_code,
    measure_date,
    measure_hour,
    avg(compliance_score) as avg_score,
    sum(total_controls) as total_controls,
    sum(compliant_controls) as compliant_controls
FROM compliance_trends
WHERE measure_date >= today() - 1
GROUP BY tenant_id, framework_code, measure_date, measure_hour;

-- Materialized view: Risk distribution by level
CREATE MATERIALIZED VIEW IF NOT EXISTS risk_distribution_realtime
ENGINE = SummingMergeTree()
PARTITION BY toYYYYMM(snapshot_date)
ORDER BY (tenant_id, snapshot_date)
AS SELECT
    tenant_id,
    snapshot_date,
    sum(critical_risks) as critical,
    sum(high_risks) as high,
    sum(medium_risks) as medium,
    sum(low_risks) as low
FROM dashboard_snapshots
WHERE snapshot_date >= today() - 7
GROUP BY tenant_id, snapshot_date;
