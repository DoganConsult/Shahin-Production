# Production Readiness Evidence Pack

**Document ID:** PROD-READY-2026-001
**Version:** 1.0
**Date:** January 19, 2026
**Classification:** Internal - Audit/Operations

---

## Purpose

This document defines the **required evidence** for production deployment approval.
All items are **non-negotiable** and must be collected before Go/No-Go decision.

---

## Evidence Folder Structure

```
Production_Readiness_Evidence/
├── Gate_A_Build_Release/
│   ├── ci_build_logs.txt
│   ├── release_artifact_hash.md
│   ├── migration_execution_log.txt
│   ├── schema_version.sql
│   └── release_notes.md
├── Gate_B_Golden_Flows/
│   ├── B1_Self_Registration/
│   │   ├── api_request_response.json
│   │   ├── user_record.png
│   │   └── audit_events.png
│   ├── B2_Trial_Signup/
│   ├── B3_Trial_Provision/
│   ├── B4_User_Invite/
│   ├── B5_Accept_Invite/
│   └── B6_Role_Change/
├── Gate_C_Audit_Security/
│   ├── audit_event_extracts.csv
│   ├── auth_audit_log_extracts.csv
│   ├── mfa_policy_config.png
│   ├── rate_limit_config.png
│   └── rate_limit_test_results.png
├── Gate_D_Operations/
│   ├── ssl_certificate.png
│   ├── secret_management_config.png
│   ├── backup_schedule.png
│   ├── restore_test_report.md
│   ├── monitoring_dashboards/
│   └── alerting_rules.png
└── sign_off.pdf
```

---

## Gate A: Build & Release Quality

### A1. CI Build Logs
- [ ] **File:** `ci_build_logs.txt`
- [ ] **Content:** Full build output showing `dotnet restore`, `dotnet build -c Release`
- [ ] **Pass Criteria:** Zero errors, zero warnings (or documented exceptions)

### A2. Release Artifact Hash
- [ ] **File:** `release_artifact_hash.md`
- [ ] **Content:**
  ```markdown
  Version: 1.0.0
  Build Date: 2026-01-19
  Commit SHA: abc123...
  Artifact Hash (SHA256): def456...
  ```

### A3. Migration Execution Log
- [ ] **File:** `migration_execution_log.txt`
- [ ] **Content:** Output of `dotnet ef database update` on:
  - Empty database (fresh install)
  - Existing database (upgrade path)
- [ ] **Pass Criteria:** All migrations applied successfully

### A4. Schema Version
- [ ] **File:** `schema_version.sql`
- [ ] **Content:** Query result: `SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId DESC`

### A5. Release Notes
- [ ] **File:** `release_notes.md`
- [ ] **Content:** Features, bug fixes, breaking changes, known issues

---

## Gate B: Golden Flow Evidence

### For Each Flow (B1-B6)

#### API Evidence
- [ ] **File:** `api_request_response.json`
- [ ] **Content:** Sanitized request/response pairs
- [ ] **Note:** Remove passwords, tokens in requests

#### Database Evidence
- [ ] **File:** `{entity}_record.png` or `.csv`
- [ ] **Content:** Screenshot or export of created/modified records
- [ ] **Tables:** Users, Tenants, TenantUsers, UserInvitations, Trials

#### Audit Evidence
- [ ] **File:** `audit_events.png` or `.csv`
- [ ] **Content:** AuditEvent records with:
  - EventType (must match expected)
  - TenantId
  - ActorId (UserId)
  - CorrelationId
  - Timestamp

### Required Audit Events Per Flow

| Flow | Required Events |
|------|-----------------|
| B1: Self Registration | AM01_USER_CREATED, AM01_USER_REGISTERED |
| B2: Trial Signup | AM01_TRIAL_SIGNUP_INITIATED |
| B3: Trial Provision | AM01_TENANT_CREATED, AM01_USER_CREATED, AM03_ROLE_ASSIGNED |
| B4: User Invite | AM01_USER_INVITED |
| B5: Accept Invite | AM01_USER_CREATED, AM03_ROLE_ASSIGNED |
| B6: Role Change | AM03_ROLE_ASSIGNED (or AM03_ROLE_CHANGED) |

---

## Gate C: Audit & Security

### C1. Audit Event Extracts
- [ ] **File:** `audit_event_extracts.csv`
- [ ] **Content:** Export of AuditEvents showing all event types from Gate B
- [ ] **Query:**
  ```sql
  SELECT EventType, TenantId, ActorId, EntityType, EntityId,
         CorrelationId, CreatedAt, Details
  FROM AuditEvents
  WHERE CreatedAt >= '{test_start_date}'
  ORDER BY CreatedAt
  ```

### C2. Authentication Audit Log Extracts
- [ ] **File:** `auth_audit_log_extracts.csv`
- [ ] **Content:** AuthenticationAuditLog entries
- [ ] **Required Events:** Login, FailedLogin, RoleChanged
- [ ] **Query:**
  ```sql
  SELECT EventType, UserId, TenantId, IpAddress, UserAgent,
         Success, FailureReason, CreatedAt
  FROM AuthenticationAuditLog
  WHERE CreatedAt >= '{test_start_date}'
  ORDER BY CreatedAt
  ```

### C3. MFA Policy Configuration
- [ ] **File:** `mfa_policy_config.png`
- [ ] **Content:** Screenshot showing MFA requirement for privileged roles
- [ ] **OR:** `mfa_compensating_control.md` documenting alternative

### C4. Rate Limit Configuration & Test
- [ ] **File:** `rate_limit_config.png`
- [ ] **Content:** appsettings showing rate limit rules
- [ ] **File:** `rate_limit_test_results.png`
- [ ] **Content:** Screenshot showing 429 response at threshold

---

## Gate D: Operational Readiness

### D1. TLS/SSL Certificate
- [ ] **File:** `ssl_certificate.png`
- [ ] **Content:** Certificate details showing:
  - Valid dates
  - Domain coverage
  - Issuer

### D2. Secret Management
- [ ] **File:** `secret_management_config.png`
- [ ] **Content:** Evidence that secrets are NOT in appsettings.json
- [ ] **Show:** Azure Key Vault reference OR environment variable config

### D3. Backup Configuration
- [ ] **File:** `backup_schedule.png`
- [ ] **Content:** PostgreSQL backup schedule/policy

### D4. Restore Test Report
- [ ] **File:** `restore_test_report.md`
- [ ] **Content:**
  ```markdown
  ## Restore Test Report

  **Date:** 2026-01-XX
  **Backup Used:** backup_2026XXXX.sql
  **Target:** Test database instance

  ### Steps Executed
  1. Dropped test database
  2. Restored from backup
  3. Verified data integrity
  4. Tested application connectivity

  ### Result
  - [ ] Restore completed successfully
  - [ ] All tables present
  - [ ] Sample queries returned expected data
  - [ ] Application connected and functioned

  **Performed By:** [Name]
  **Witnessed By:** [Name]
  ```

### D5. Monitoring Dashboards
- [ ] **Folder:** `monitoring_dashboards/`
- [ ] **Files:**
  - `error_rate_dashboard.png`
  - `latency_dashboard.png`
  - `job_failures_dashboard.png`
  - `email_failures_dashboard.png`

### D6. Alerting Rules
- [ ] **File:** `alerting_rules.png`
- [ ] **Content:** Screenshot of configured alerts:
  - Login failure spike (>X per minute)
  - Hangfire job failure
  - Database connectivity
  - High error rate (5xx > threshold)
  - High latency (p95 > threshold)

---

## Sign-Off Sheet

### Go/No-Go Decision

| Gate | Status | Blocking Issues | Signed Off By | Date |
|------|--------|-----------------|---------------|------|
| Gate A: Build & Release | | | | |
| Gate B: Golden Flows | | | | |
| Gate C: Audit & Security | | | | |
| Gate D: Operations | | | | |

### Final Approval

| Role | Name | Decision | Signature | Date |
|------|------|----------|-----------|------|
| Engineering Lead | | GO / NO-GO | | |
| Security Lead | | GO / NO-GO | | |
| Operations Lead | | GO / NO-GO | | |
| Product Owner | | GO / NO-GO | | |

### Blocking Issues (if NO-GO)

| Issue ID | Description | Owner | Target Resolution |
|----------|-------------|-------|-------------------|
| | | | |

---

## Recommended Execution Order

1. **Day 1:** Run clean Release build → Capture Gate A evidence
2. **Day 1-2:** Execute Golden Flows in staging → Capture Gate B evidence
3. **Day 2:** Verify audit events match traceability matrix → Capture Gate C evidence
4. **Day 2-3:** Verify operational readiness → Capture Gate D evidence
5. **Day 3:** Assemble evidence pack → Schedule Go/No-Go meeting

---

## Post-Deployment Validation

After production deployment, re-run abbreviated Golden Flows:

- [ ] B1: Register a test user → Verify login works
- [ ] B3: Provision a test tenant (if applicable)
- [ ] Verify audit events are being written
- [ ] Verify monitoring dashboards show data
- [ ] Delete test data after validation

---

*Document generated: January 19, 2026*
*Next review date: Before each major release*
