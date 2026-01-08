-- Initialize additional databases for analytics tools
-- Run this after main GRC database is created

-- Create Grafana database
CREATE DATABASE grafana;

-- Create n8n database  
CREATE DATABASE n8n;

-- Create Metabase database
CREATE DATABASE metabase;

-- Grant permissions
GRANT ALL PRIVILEGES ON DATABASE grafana TO postgres;
GRANT ALL PRIVILEGES ON DATABASE n8n TO postgres;
GRANT ALL PRIVILEGES ON DATABASE metabase TO postgres;

-- Create read-only user for analytics tools
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'grc_readonly') THEN
        CREATE ROLE grc_readonly WITH LOGIN PASSWORD 'grc_readonly_2026';
    END IF;
END
$$;

-- Grant read-only access to GRC database tables
GRANT CONNECT ON DATABASE "GrcMvcDb" TO grc_readonly;
GRANT USAGE ON SCHEMA public TO grc_readonly;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO grc_readonly;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO grc_readonly;

-- Create analytics views for Superset/Grafana
-- These views provide pre-aggregated data for dashboards

-- Compliance Summary View
CREATE OR REPLACE VIEW vw_compliance_summary AS
SELECT 
    t."Id" as tenant_id,
    t."Name" as tenant_name,
    COUNT(DISTINCT ca."Id") as total_assessments,
    COUNT(DISTINCT CASE WHEN ca."ComplianceStatus" = 'Compliant' THEN ca."Id" END) as compliant_count,
    COUNT(DISTINCT CASE WHEN ca."ComplianceStatus" = 'NonCompliant' THEN ca."Id" END) as non_compliant_count,
    COUNT(DISTINCT CASE WHEN ca."ComplianceStatus" = 'PartiallyCompliant' THEN ca."Id" END) as partial_count,
    ROUND(
        COALESCE(
            COUNT(DISTINCT CASE WHEN ca."ComplianceStatus" = 'Compliant' THEN ca."Id" END)::numeric / 
            NULLIF(COUNT(DISTINCT ca."Id"), 0) * 100, 0
        ), 2
    ) as compliance_rate
FROM "Tenants" t
LEFT JOIN "ControlAssessments" ca ON ca."TenantId" = t."Id"
GROUP BY t."Id", t."Name";

-- Risk Summary View
CREATE OR REPLACE VIEW vw_risk_summary AS
SELECT 
    t."Id" as tenant_id,
    t."Name" as tenant_name,
    COUNT(DISTINCT r."Id") as total_risks,
    COUNT(DISTINCT CASE WHEN r."Status" = 'Open' THEN r."Id" END) as open_risks,
    COUNT(DISTINCT CASE WHEN r."RiskLevel" = 'Critical' THEN r."Id" END) as critical_risks,
    COUNT(DISTINCT CASE WHEN r."RiskLevel" = 'High' THEN r."Id" END) as high_risks,
    COUNT(DISTINCT CASE WHEN r."RiskLevel" = 'Medium' THEN r."Id" END) as medium_risks,
    COUNT(DISTINCT CASE WHEN r."RiskLevel" = 'Low' THEN r."Id" END) as low_risks
FROM "Tenants" t
LEFT JOIN "Risks" r ON r."TenantId" = t."Id"
GROUP BY t."Id", t."Name";

-- Audit Summary View
CREATE OR REPLACE VIEW vw_audit_summary AS
SELECT 
    t."Id" as tenant_id,
    t."Name" as tenant_name,
    COUNT(DISTINCT a."Id") as total_audits,
    COUNT(DISTINCT CASE WHEN a."Status" = 'Completed' THEN a."Id" END) as completed_audits,
    COUNT(DISTINCT CASE WHEN a."Status" = 'InProgress' THEN a."Id" END) as in_progress_audits,
    COUNT(DISTINCT CASE WHEN a."Status" = 'Planned' THEN a."Id" END) as planned_audits
FROM "Tenants" t
LEFT JOIN "Audits" a ON a."TenantId" = t."Id"
GROUP BY t."Id", t."Name";

-- Daily Activity View (for time-series)
CREATE OR REPLACE VIEW vw_daily_activity AS
SELECT 
    DATE_TRUNC('day', al."Timestamp") as activity_date,
    al."TenantId" as tenant_id,
    al."ActionType" as action_type,
    COUNT(*) as action_count
FROM "AuditLogs" al
WHERE al."Timestamp" >= NOW() - INTERVAL '90 days'
GROUP BY DATE_TRUNC('day', al."Timestamp"), al."TenantId", al."ActionType"
ORDER BY activity_date DESC;

-- Framework Coverage View
CREATE OR REPLACE VIEW vw_framework_coverage AS
SELECT 
    tb."TenantId" as tenant_id,
    b."Name" as baseline_name,
    b."FrameworkCode" as framework_code,
    COUNT(DISTINCT c."Id") as total_controls,
    COUNT(DISTINCT CASE WHEN ca."ComplianceStatus" = 'Compliant' THEN c."Id" END) as compliant_controls,
    ROUND(
        COALESCE(
            COUNT(DISTINCT CASE WHEN ca."ComplianceStatus" = 'Compliant' THEN c."Id" END)::numeric / 
            NULLIF(COUNT(DISTINCT c."Id"), 0) * 100, 0
        ), 2
    ) as coverage_percent
FROM "TenantBaselines" tb
JOIN "Baselines" b ON b."Id" = tb."BaselineId"
LEFT JOIN "Controls" c ON c."BaselineId" = b."Id"
LEFT JOIN "ControlAssessments" ca ON ca."ControlId" = c."Id" AND ca."TenantId" = tb."TenantId"
GROUP BY tb."TenantId", b."Name", b."FrameworkCode";

-- Grant access to views
GRANT SELECT ON vw_compliance_summary TO grc_readonly;
GRANT SELECT ON vw_risk_summary TO grc_readonly;
GRANT SELECT ON vw_audit_summary TO grc_readonly;
GRANT SELECT ON vw_daily_activity TO grc_readonly;
GRANT SELECT ON vw_framework_coverage TO grc_readonly;
