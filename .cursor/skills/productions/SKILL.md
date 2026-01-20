add as roles not negionatbale Below is a practical checklist you can run as a “Go/No-Go” process. If you follow this, you will know exactly what is missing and why.

1) Define a Production Readiness Gate (Go/No-Go)

Create a single document (or Jira epic) with these sections and require objective pass criteria:

Gate A — Build & Release Quality (non-negotiable)

Pass criteria

Clean build in CI: dotnet restore, dotnet build -c Release succeeds

No compiler errors/warnings treated as errors for Release

DB migrations apply cleanly to an empty database and to an upgraded database

Versioning and release notes exist for the build

Evidence

CI logs + release artifact hash

Migration execution log + schema version

Gate B — End-to-End “Golden Flows” Work

Run these with a test tenant in staging:

Self registration: /api/auth/register → verification/password set → login works

Trial signup: /api/trial/signup creates trial record

Trial provision: /api/trial/provision creates tenant + TenantAdmin; immediate login works

Invite: /api/tenants/{tenantId}/users/invite → email sent

Accept invite: /api/invitation/accept → user created + role assigned → login works

Role change: assign/remove roles and verify permissions enforce correctly

Audit evidence exists for each step (see Gate C)

Evidence

API request/response logs (sanitized)

DB record screenshots/exports (tenant, user, tenantuser)

Email delivery evidence (message id)

Gate C — Audit & Security Controls Are Real

Pass criteria

Every access action emits the correct audit events:

Register: AM01_USER_CREATED, AM01_USER_REGISTERED

Trial signup: AM01_TRIAL_SIGNUP_INITIATED

Trial provision: AM01_TENANT_CREATED, AM01_USER_CREATED, AM03_ROLE_ASSIGNED

Invite: AM01_USER_INVITED

Accept: AM01_USER_CREATED, AM03_ROLE_ASSIGNED

Authentication events are logged (AuthenticationAuditLog): Login, FailedLogin, RoleChanged, 2FAEnabled, etc.

Privileged roles (PlatformAdmin/TenantAdmin) require MFA (or a documented compensating control if not ready yet)

Rate limiting is active on auth endpoints (you already have [EnableRateLimiting("auth")])

Evidence

AuditEvent and AuthenticationAuditLog extracts with correlationId and actor/tenant IDs

MFA policy config + test proof

Rate limit config and test proof

Gate D — Operational Readiness (do not skip)

Pass criteria

TLS/SSL enabled in production

Secrets stored securely (not in appsettings committed to git)

Backups configured and tested restore (PostgreSQL)

Monitoring dashboards exist: error rate, latency, job failures, email failures

Alerting configured for critical events (login failures spike, job failures, DB down)

Evidence

Backup/restore test report

Monitoring dashboard links + alert rules

2) Convert “Planned” to “Implemented” with a Minimum Production Scope

To go live, you do not need every planned integration/AI feature. You need the minimum viable production scope:

Must be implemented before production

Build is green and deployable

Golden flows (A/B above) pass

Audit events are emitted consistently (your traceability matrix is the blueprint)

Role enforcement works (RoleConstants are canonical)

Hangfire jobs stable (since ABP workers disabled)

Can be postponed safely (if disabled and controlled)

Kafka/Camunda/ClickHouse/Redis (remain disabled, with governance)

AI modules (remain disabled until policy + provider + opt-in)

3) Professional Validation: Testing Package (what teams do before go-live)
Automated tests (minimum)

Unit tests for: role assignment validation, status transitions, audit emission

Integration tests for:

register → audit events created

provision → tenant/user created + role assigned + audit events

invite/accept → user created + role assigned + audit events

Manual test scripts (audit-friendly)

A 1–2 page “UAT script” showing steps, expected results, and evidence captured.

Run it on staging with production-like config.

4) Hardening Checklist (common production blockers)

Consistent status codes: use your canonical Pending/Active/Suspended etc. everywhere

Consistent role codes: ensure APIs accept only RoleConstants values or map legacy values

Idempotency: provision endpoints and reminder jobs should be safe to retry

Error handling: no raw exceptions returned; consistent error envelope + correlationId

Data retention: define retention for audit logs and authentication logs

5) Your “final proof” pack (what you show auditors/customers)

Create a folder called Production Readiness Evidence containing:

Release build logs + version

DB migration proof

Golden flow test evidence (requests/responses + DB snapshots)

AuditEvent extracts for each flow step

AuthenticationAuditLog extracts (login/failed/role/mfa)

Backup/restore test evidence

Monitoring/alerting screenshots

If you can produce this pack, you are production-ready in a professional sense.

What I recommend you do next (most efficient order)

Run a clean Release build and fix all compile errors (this is always first).

Run the Golden flows in staging and confirm audit events exist per your traceability matrix.

Only after that: implement Access Reviews (AM04) (migration + service + Hangfire reminders), because it’s the next maturity step for audits.---
name: productions
description: This is a new rule
---

# Overview

Insert overview text here. The agent will only see this should they choose to apply the rule.
