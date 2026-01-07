-- ============================================================================
-- GRC Analytics Database - ClickHouse OLAP Schema
-- ============================================================================

CREATE DATABASE IF NOT EXISTS grc_analytics;

-- ============================================================================
-- DASHBOARD SNAPSHOTS - Pre-aggregated dashboard metrics (hourly)
-- ============================================================================
CREATE TABLE IF NOT EXISTS grc_analytics.dashboard_snapshots
(
    tenant_id UUID,
    snapshot_date Date,
    snapshot_hour DateTime,

    -- Compliance metrics
    total_controls UInt32 DEFAULT 0,
    compliant_controls UInt32 DEFAULT 0,
    partial_controls UInt32 DEFAULT 0,
    non_compliant_controls UInt32 DEFAULT 0,
    not_started_controls UInt32 DEFAULT 0,
    compliance_score Float32 DEFAULT 0,

    -- Risk metrics
    total_risks UInt32 DEFAULT 0,
    critical_risks UInt32 DEFAULT 0,
    high_risks UInt32 DEFAULT 0,
    medium_risks UInt32 DEFAULT 0,
    low_risks UInt32 DEFAULT 0,
    open_risks UInt32 DEFAULT 0,
    mitigated_risks UInt32 DEFAULT 0,
    risk_score_avg Float32 DEFAULT 0,

    -- Task metrics
    total_tasks UInt32 DEFAULT 0,
    pending_tasks UInt32 DEFAULT 0,
    in_progress_tasks UInt32 DEFAULT 0,
    completed_tasks UInt32 DEFAULT 0,
    overdue_tasks UInt32 DEFAULT 0,
    due_this_week UInt32 DEFAULT 0,

    -- Evidence metrics
    total_evidence UInt32 DEFAULT 0,
    evidence_submitted UInt32 DEFAULT 0,
    evidence_approved UInt32 DEFAULT 0,
    evidence_rejected UInt32 DEFAULT 0,
    evidence_pending UInt32 DEFAULT 0,

    -- Assessment metrics
    total_assessments UInt32 DEFAULT 0,
    active_assessments UInt32 DEFAULT 0,
    completed_assessments UInt32 DEFAULT 0,

    -- Plan metrics
    total_plans UInt32 DEFAULT 0,
    active_plans UInt32 DEFAULT 0,
    completed_plans UInt32 DEFAULT 0,
    overall_plan_progress Float32 DEFAULT 0,

    -- Timestamps
    created_at DateTime DEFAULT now(),
    updated_at DateTime DEFAULT now()
)
ENGINE = ReplacingMergeTree(updated_at)
PARTITION BY toYYYYMM(snapshot_date)
ORDER BY (tenant_id, snapshot_date, snapshot_hour)
TTL snapshot_date + INTERVAL 2 YEAR;

-- ============================================================================
-- COMPLIANCE TRENDS - Time-series compliance data
-- ============================================================================
CREATE TABLE IF NOT EXISTS grc_analytics.compliance_trends
(
    tenant_id UUID,
    framework_code String,
    baseline_code String,
    measure_date Date,
    measure_hour DateTime,

    compliance_score Float32,
    total_controls UInt32,
    compliant_controls UInt32,
    partial_controls UInt32,
    non_compliant_controls UInt32,

    delta_from_previous Float32 DEFAULT 0,

    created_at DateTime DEFAULT now()
)
ENGINE = MergeTree()
PARTITION BY toYYYYMM(measure_date)
ORDER BY (tenant_id, framework_code, measure_date, measure_hour)
TTL measure_date + INTERVAL 3 YEAR;

-- ============================================================================
-- RISK HEATMAP - 5x5 Risk Matrix aggregations
-- ============================================================================
CREATE TABLE IF NOT EXISTS grc_analytics.risk_heatmap
(
    tenant_id UUID,
    snapshot_date Date,

    likelihood UInt8,  -- 1-5
    impact UInt8,      -- 1-5
    risk_count UInt32,

    risk_ids Array(UUID),

    created_at DateTime DEFAULT now()
)
ENGINE = SummingMergeTree(risk_count)
PARTITION BY toYYYYMM(snapshot_date)
ORDER BY (tenant_id, snapshot_date, likelihood, impact);

-- ============================================================================
-- TASK METRICS BY ROLE - Tasks grouped by role/team
-- ============================================================================
CREATE TABLE IF NOT EXISTS grc_analytics.task_metrics_by_role
(
    tenant_id UUID,
    snapshot_date Date,
    role_code String,
    team_id UUID,

    total_tasks UInt32 DEFAULT 0,
    pending_tasks UInt32 DEFAULT 0,
    in_progress_tasks UInt32 DEFAULT 0,
    completed_tasks UInt32 DEFAULT 0,
    overdue_tasks UInt32 DEFAULT 0,

    avg_completion_days Float32 DEFAULT 0,
    sla_compliance_rate Float32 DEFAULT 0,

    created_at DateTime DEFAULT now()
)
ENGINE = MergeTree()
PARTITION BY toYYYYMM(snapshot_date)
ORDER BY (tenant_id, snapshot_date, role_code);

-- ============================================================================
-- EVIDENCE COLLECTION METRICS
-- ============================================================================
CREATE TABLE IF NOT EXISTS grc_analytics.evidence_metrics
(
    tenant_id UUID,
    snapshot_date Date,
    evidence_type String,
    control_domain String,

    total_required UInt32 DEFAULT 0,
    total_collected UInt32 DEFAULT 0,
    total_approved UInt32 DEFAULT 0,
    total_rejected UInt32 DEFAULT 0,
    total_expired UInt32 DEFAULT 0,

    collection_rate Float32 DEFAULT 0,
    approval_rate Float32 DEFAULT 0,
    avg_review_days Float32 DEFAULT 0,

    created_at DateTime DEFAULT now()
)
ENGINE = MergeTree()
PARTITION BY toYYYYMM(snapshot_date)
ORDER BY (tenant_id, snapshot_date, evidence_type);

-- ============================================================================
-- AUDIT EVENTS STREAM - Raw events from Kafka
-- ============================================================================
CREATE TABLE IF NOT EXISTS grc_analytics.events_raw
(
    event_id UUID,
    tenant_id UUID,
    event_type String,
    entity_type String,
    entity_id String,
    action String,
    actor String,

    payload String,  -- JSON payload

    event_timestamp DateTime,
    ingested_at DateTime DEFAULT now()
)
ENGINE = MergeTree()
PARTITION BY toYYYYMM(event_timestamp)
ORDER BY (tenant_id, event_timestamp, event_type)
TTL event_timestamp + INTERVAL 1 YEAR;

-- ============================================================================
-- TOP ACTIONS - Prioritized next actions per tenant
-- ============================================================================
CREATE TABLE IF NOT EXISTS grc_analytics.top_actions
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
ENGINE = ReplacingMergeTree(created_at)
PARTITION BY toYYYYMM(snapshot_date)
ORDER BY (tenant_id, snapshot_date, action_rank);

-- ============================================================================
-- FRAMEWORK COMPARISON - Cross-framework compliance view
-- ============================================================================
CREATE TABLE IF NOT EXISTS grc_analytics.framework_comparison
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

    compliance_score Float32,
    maturity_level UInt8,

    trend_7d Float32 DEFAULT 0,
    trend_30d Float32 DEFAULT 0,

    created_at DateTime DEFAULT now()
)
ENGINE = ReplacingMergeTree(created_at)
PARTITION BY toYYYYMM(snapshot_date)
ORDER BY (tenant_id, snapshot_date, framework_code);

-- ============================================================================
-- USER ACTIVITY METRICS
-- ============================================================================
CREATE TABLE IF NOT EXISTS grc_analytics.user_activity
(
    tenant_id UUID,
    user_id String,
    activity_date Date,

    login_count UInt32 DEFAULT 0,
    tasks_completed UInt32 DEFAULT 0,
    evidence_submitted UInt32 DEFAULT 0,
    assessments_worked UInt32 DEFAULT 0,
    approvals_given UInt32 DEFAULT 0,

    session_minutes UInt32 DEFAULT 0,
    last_activity DateTime,

    created_at DateTime DEFAULT now()
)
ENGINE = SummingMergeTree((login_count, tasks_completed, evidence_submitted, assessments_worked, approvals_given, session_minutes))
PARTITION BY toYYYYMM(activity_date)
ORDER BY (tenant_id, activity_date, user_id);
