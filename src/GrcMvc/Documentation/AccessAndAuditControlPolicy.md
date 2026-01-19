# Access & Audit Control Policy

**System:** GRC Platform (ABP Framework-based)
**Version:** 2.0
**Last Updated:** 2026-01-19
**Owner:** Security Engineering + GRC Team
**Classification:** Internal

---

## 1. Purpose

This policy establishes mandatory controls to ensure that:

1. **Access Control**: Access to the GRC platform is provisioned, modified, and revoked in a controlled and auditable manner.
2. **Audit Integrity**: All security-relevant activity is logged with sufficient integrity and traceability to support investigations, customer assurance, and audits.
3. **Feature Governance**: Feature/module enablement and background processing are governed to prevent uncontrolled risk exposure.
4. **AI Governance**: AI features are enabled only with explicit consent and all usage is logged transparently.

---

## 2. Scope

This policy applies to:

### 2.1 Users and Tenants
- All tenants (trial and paid)
- All users across all roles

### 2.2 User Creation Flows
| Flow | Endpoint | Default Status |
|------|----------|----------------|
| Self-Registration Trial | `/register`, `/api/trial/signup` | Pending |
| Trial API Provision | `/api/trial/provision` | Active (with auth) |
| Admin Invite | `/api/tenants/{tenantId}/users/invite` | Pending |

### 2.3 System Components
- All predefined roles in `RoleConstants.cs` (21 roles: 2 platform + 19 tenant)
- All audit events (`AuditEvent`) and authentication audit logs (`AuthenticationAuditLog`)
- All platform-level events under `PlatformAuditEventTypes`
- Feature flags/modules managed through ABP FeatureManagement
- All integrations (Kafka, Camunda, ClickHouse, Redis, etc.)
- All AI features (Assistant, Classification, Risk Assessment, Compliance Analysis)

---

## 3. Definitions

### 3.1 Status Values (Authoritative)

| Category | Values |
|----------|--------|
| Entity Status (General) | `Active`, `Inactive`, `Pending`, `Suspended`, `Deleted` |
| Onboarding Status | `NOT_STARTED`, `IN_PROGRESS`, `COMPLETED`, `FAILED` |
| Tenant Billing Status | `Trialing`, `Expired`, `TrialExpired` |
| Audit Event Status | `Success`, `Partial`, `Failed` |
| Severity Levels | `Info`, `Warning`, `Error`, `Critical` |

### 3.2 Roles (Authoritative)

The system uses 21 predefined roles in `RoleConstants.cs`:

**Platform Roles (2):**
- `PlatformAdmin`, `PlatformSupport`

**Tenant Roles (19):**
- Executive: `TenantOwner`, `TenantAdmin`
- Management: `ComplianceOfficer`, `RiskManager`, `AuditManager`, `PolicyManager`
- Operational: `Assessor`, `Analyst`, `Implementer`, `EvidenceCollector`, `Auditor`, `InternalAuditor`, `RiskAnalyst`
- Support: `Reviewer`, `Approver`, `TeamMember`, `ReadOnly`, `Contributor`, `Guest`

**Role Normalization**: All role values must align to `RoleConstants.*`. Legacy role codes must be normalized using `RoleConstants.NormalizeRoleCode()`.

### 3.3 Audit Records (Authoritative)

| Record Type | Purpose | Key Fields |
|-------------|---------|------------|
| `AuditEvent` | Business/Platform audit | EventType, Action, Status, Severity |
| `AuthenticationAuditLog` | Authentication audit | EventType (Login, FailedLogin, RoleChanged, 2FAEnabled, etc.) |
| `PlatformAuditEventTypes` | Platform admin events | Tenant/user/admin/config lifecycle |

### 3.4 Control Families

| Family | Controls | Events | Purpose |
|--------|----------|--------|---------|
| AM (Access Management) | AM-01 to AM-12 | 72 | User lifecycle, roles, MFA, access reviews |
| MG (Module Governance) | MG-01 to MG-03 | 18 | Module inventory, change approval, environment parity |
| AU (Auditability) | AU-01 to AU-03 | 20 | Authentication, business, and platform audit logging |
| FM (Feature Management) | FM-01 | 8 | Feature flag governance |
| BP (Background Processing) | BP-01 to BP-02 | 13 | Hangfire jobs, ABP worker documentation |
| IN (Integration) | IN-01 to IN-02 | 14 | Integration approval, credential management |
| AI (AI Governance) | AI-01 to AI-02 | 15 | AI opt-in, usage logging |
| **Total** | **25 controls** | **160 events** | |

---

## 4. Policy Statements

### 4.1 Identity and Access Lifecycle Control (AM-01, AM-05)

#### 4.1.1 Canonical State Machine

All access-bearing entities (User, TenantUser) must follow this lifecycle:

```
Pending → Active → (Suspended | Inactive) → Deleted (soft delete)
```

**Requirements:**
- A user/TenantUser MUST NOT become `Active` without a permitted activation trigger
- `Suspended` MUST immediately prevent interactive login and API access
- `Deleted` MUST represent a soft-delete state and prevent all access

#### 4.1.2 Permitted Activation Triggers (By Flow)

| Flow | Default Status | Activation Trigger | Billing Status |
|------|----------------|-------------------|----------------|
| Self-Registration Trial | `Pending` | Email verification + password set | `Trialing` |
| Trial API Provision | `Active` | Authenticated & authorized caller | `Trialing` |
| Admin Invite | `Pending` | Invite acceptance + password set | Inherited |

**Mandatory Logging:** Each activation MUST produce:
- `AuditEvent` with `AM01_*` event type
- `AuthenticationAuditLog` entries (PasswordChanged, Login, etc.)

### 4.2 Trial API Provisioning Security Control (AM-02)

`/api/trial/provision` is a privileged provisioning interface.

**Requirements:**
1. Strong caller authentication (client credentials, API keys with rotation, or signed JWT)
2. Authorization policies:
   - Tenant creation quotas/rate limits per caller (max 10/hour, 50/day)
   - Domain or account constraints
   - Restriction on privileged role assignment
3. Traceable logs:
   - Caller identity (client/app identity)
   - Source metadata (request ID, correlation ID, timestamp)
   - Created tenantId and userId

**Audit Events:** `AM02_PROVISION_REQUESTED`, `AM02_PROVISION_AUTHORIZED`, `AM02_PROVISION_COMPLETED`

### 4.3 Role Governance and Least Privilege (AM-03, AM-04)

#### 4.3.1 Role Canonicalization

All role assignments MUST use canonical values from `RoleConstants.cs`. The `NormalizeRoleCode()` function handles:
- SNAKE_CASE: `TENANT_ADMIN` → `TenantAdmin`
- kebab-case: `tenant-admin` → `TenantAdmin`
- Abbreviations: `admin` → `TenantAdmin`

Unmapped values MUST be rejected.

#### 4.3.2 Privileged Roles

The following roles are considered privileged and require enhanced controls:

| Role | Level | MFA Required | Step-Up Auth |
|------|-------|--------------|--------------|
| PlatformAdmin | 0 | Yes | Yes |
| SystemAdministrator | 5 | Yes | Yes |
| TenantOwner | 10 | Yes | Yes |
| TenantAdmin | 15 | Yes | Yes |

**Requirements:**
- Privileged role assignment restricted to authorized administrators only
- Role changes logged as `AM03_ROLE_ASSIGNED`, `AM03_ROLE_CHANGED`, `AM03_ROLE_REVOKED`
- Severity ≥ `Warning` for privileged role changes

#### 4.3.3 MFA and Strong Authentication (AM-04)

- MFA MUST be enabled for all privileged roles
- MFA events logged: `AM04_MFA_ENABLED`, `AM04_MFA_DISABLED`, `AM04_MFA_VERIFIED`, `AM04_MFA_FAILED`
- Failed authentication triggers account lock after 5 attempts (30-minute lockout)
- Lock events logged: `AM08_ACCOUNT_LOCKED`, `AM08_ACCOUNT_UNLOCKED`

### 4.4 Audit Logging Policy (AU-01, AU-02, AU-03)

#### 4.4.1 AuditEvent Requirements (AU-02)

Every `AuditEvent` record MUST include:

| Field | Requirement |
|-------|-------------|
| EventType | Aligned to defined constants (AM01_*, MG01_*, etc.) |
| Action | Create/Update/Delete/Approve/Reject |
| Status | Success/Partial/Failed |
| Severity | Info/Warning/Error/Critical |
| Actor | User ID or "SYSTEM" |
| TenantId | Tenant scope (or null for platform) |
| Timestamp | UTC timestamp |
| CorrelationId | Request tracing identifier (required) |

#### 4.4.2 Authentication Logging (AU-01)

The system MUST record in `AuthenticationAuditLog`:

| Event Type | Description |
|------------|-------------|
| Login | Successful login |
| Logout | User logout |
| FailedLogin | Failed authentication attempt |
| AccountLocked | Account locked due to failed attempts |
| PasswordChanged | Password modification |
| RoleChanged | Role assignment/removal |
| ClaimsModified | Claim changes |
| TokenRefresh | Token refresh |
| 2FAEnabled | MFA enabled |
| 2FADisabled | MFA disabled |

**Mandatory Fields:** Timestamp, UserId, EventType, IpAddress, UserAgent, Result, CorrelationId

#### 4.4.3 Platform Admin Audit Trail (AU-03)

Platform admin actions emit `PlatformAuditEventTypes`:

| Category | Events |
|----------|--------|
| Tenant Lifecycle | CREATED, ACTIVATED, SUSPENDED, DELETED, UPDATED |
| User Management | CREATED, DEACTIVATED, ACTIVATED, PASSWORD_RESET, IMPERSONATED |
| Admin Management | CREATED, UPDATED, SUSPENDED, REACTIVATED, DELETED |
| Authentication | ADMIN_LOGIN, ADMIN_LOGOUT, LOGIN_FAILED |
| Configuration | CONFIG_CHANGED, CATALOG_UPDATED |

### 4.5 Trial Governance (AM-09)

#### 4.5.1 Trial Lifecycle

| Stage | Duration | Status | Events |
|-------|----------|--------|--------|
| Trial Start | Day 0 | `Trialing` | `AM09_TRIAL_STARTED` |
| Expiry Warning | Day 5 | `Trialing` | `AM09_TRIAL_EXPIRY_WARNING` |
| Trial Expired | Day 7 | `TrialExpired` | `AM09_TRIAL_EXPIRED` |
| Data Archived | Day 37 | `TrialExpired` | `AM09_TRIAL_DATA_ARCHIVED` |
| Data Deleted | Day 67 | N/A | `AM09_TRIAL_DATA_DELETED` |

**Configuration:**
- Trial duration: 7 days (configurable)
- Data retention after expiry: 30 days
- Expiry warning: 2 days before

### 4.6 Module Governance (MG-01, MG-02, MG-03)

#### 4.6.1 Module Inventory (MG-01)

A Module Register MUST be maintained listing all 41 modules:
- 32 active modules
- 9 disabled modules

**Required Fields:**
- ModuleName, Category, Default, Environment Status
- Owner, Dependencies, Data Sensitivity
- Enablement Criteria, Monitoring Link, Last Reviewed

**Evidence:** Module Register export, change tickets referencing MG-01

#### 4.6.2 Module Change Approval (MG-02)

Any module enablement/disablement requires:
1. Change ticket with justification
2. Approval by platform owner
3. Security review for high-risk modules
4. Audit event: `MG02_CHANGE_APPROVED`, `MG02_CHANGE_DEPLOYED`

#### 4.6.3 Environment Parity (MG-03)

Module configuration MUST be identical across Dev, Staging, and Prod unless:
- Documented exception exists
- Approved by Platform Engineering
- Parity check automated in CI/CD

**Events:** `MG03_PARITY_CHECK_PASSED`, `MG03_DRIFT_DETECTED`

### 4.7 Feature Flag Governance (FM-01)

#### 4.7.1 Change Control

Feature flag changes require:
1. Audit trail with actor, timestamp, old/new value
2. Production changes require approval ticket
3. Rollback capability

**Events:** `FM01_FEATURE_ENABLED`, `FM01_FEATURE_DISABLED`, `FM01_FEATURE_APPROVED`

#### 4.7.2 Categories Requiring Approval

| Category | Approval Level | Security Review |
|----------|---------------|-----------------|
| Security | Platform Admin | Required |
| Integration | Platform Admin | Required |
| AI | Platform Admin + Legal | Required |
| Billing | Platform Admin | Optional |
| DataProcessing | Platform Admin | Required |

### 4.8 Background Processing Control (BP-01, BP-02)

#### 4.8.1 Hangfire Job Governance (BP-01)

Since ABP background workers are disabled, Hangfire is the compensating control.

**Critical Compliance Jobs:**
| Job | Schedule | Purpose |
|-----|----------|---------|
| TrialExpiryChecker | Every 2 hours | Enforce trial expiration |
| AccessReviewReminder | Daily 9 AM | Send review reminders |
| InactivitySuspensionChecker | Daily 3 AM | Suspend inactive users |
| AuditLogRetention | Weekly Sunday 2 AM | Apply retention policy |

**Requirements:**
- All compliance jobs implemented in Hangfire
- Retry logic with max 3 attempts
- Alert on critical job failure within 15 minutes
- Hangfire dashboard restricted to authorized admins

#### 4.8.2 ABP Worker Disablement Documentation (BP-02)

**Known Issue Record:**
- Issue ID: PLATFORM-001
- Root Cause: OpenIddict initialization fails with ABP BackgroundWorkers enabled
- Compensating Control: Hangfire
- Exit Criteria: ABP/OpenIddict bug fixed upstream
- Next Review: 2026-04-01

### 4.9 Integration Governance (IN-01, IN-02)

#### 4.9.1 Integration Enablement Approval (IN-01)

| Integration | Category | Risk Level | Security Review |
|-------------|----------|------------|-----------------|
| Kafka | Messaging | High | Required |
| Camunda | Workflow | Medium | Required |
| ClickHouse | Analytics | Medium | Required |
| Redis | Caching | Low | Optional |
| Stripe | Payment | High | Required |
| Twilio | Communication | Medium | Required |
| Slack/Teams | Notification | Low | Optional |

**Requirements:**
1. Approval ticket with security sign-off
2. Configuration stored in Azure Key Vault
3. Audit event on enablement: `IN01_INTEGRATION_ENABLED`

#### 4.9.2 Credential Management (IN-02)

| Credential Type | Rotation Period | MFA Required |
|-----------------|-----------------|--------------|
| API Key | 90 days | Yes |
| Client Secret | 90 days | Yes |
| Connection String | 180 days | Yes |
| Certificate | 365 days | Yes |
| Webhook Secret | 90 days | No |
| Encryption Key | 365 days | Yes |

**Events:** `IN02_CREDENTIAL_ROTATED`, `IN02_ROTATION_OVERDUE`

### 4.10 AI Governance (AI-01, AI-02)

#### 4.10.1 AI Feature Enablement (AI-01)

All AI features require explicit tenant opt-in:

| Feature | Data Processed | Default |
|---------|---------------|---------|
| AI Assistant | User queries, GRC context | Disabled |
| Risk Classification | Risk descriptions | Disabled |
| Compliance Analysis | Compliance data | Disabled |
| Document Analysis | Uploaded documents | Disabled |
| Auto Recommendations | Historical decisions | Disabled |

**Requirements:**
1. Tenant opt-in record with approval
2. AI features disabled by default
3. Opt-in logged: `AI01_OPTIN_APPROVED`
4. Opt-out capability: `AI01_OPTOUT_COMPLETED`

#### 4.10.2 AI Usage Logging (AI-02)

Every AI API call MUST log:

| Field | Description |
|-------|-------------|
| Timestamp | UTC timestamp |
| TenantId | Tenant making request |
| UserId | User making request |
| FeatureId | AI feature used |
| ModelId | AI model (claude-opus-4-5, etc.) |
| InputTokens | Token count |
| OutputTokens | Token count |
| LatencyMs | Response time |
| Status | Success/Failed/RateLimited |
| CorrelationId | Request tracing |
| EstimatedCostUSD | Cost tracking |

**Limits:**
- Max tokens per request: 10,000 (default)
- Max requests per minute per tenant: 60
- Max daily cost per tenant: $50 USD

---

## 5. Responsibilities

| Role | Responsibilities |
|------|-----------------|
| Platform Engineering | Authentication, provisioning, background jobs, logging, configuration |
| Security Engineering | MFA enforcement, monitoring, privileged access review, integration security |
| GRC/Compliance | Policy ownership, control testing, evidence collection |
| Tenant Administrators | User invites, role assignments, access reviews |
| Product Engineering | Feature flag management, AI feature governance |
| DevOps | Infrastructure, monitoring dashboards, credential rotation |

---

## 6. Control Testing and Evidence Collection

### 6.1 Minimum Evidence Pack (Per Quarter)

| Evidence Type | Sample Size | Source |
|--------------|-------------|--------|
| Tenant creation events | 10+ | AuditEvent |
| User invitation events | 10+ | AuditEvent |
| Role change events | 10+ | AuthenticationAuditLog |
| Login/FailedLogin events | 10+ | AuthenticationAuditLog |
| MFA enablement events | 10+ | AuthenticationAuditLog |
| Config change events | All | PlatformAuditEvent |
| Feature flag changes | All | AuditEvent |
| Integration credentials | All | Key Vault logs |
| AI usage logs | 10+ | AIUsageRecord |

### 6.2 Testing Procedures

| Control | Test Procedure |
|---------|---------------|
| AM-01 | Verify user cannot activate without email verification |
| AM-02 | Verify provisioning requires authentication |
| AM-03 | Verify role normalization rejects invalid codes |
| AM-04 | Verify MFA required for privileged roles |
| AM-11 | Verify access review generates all items |
| MG-01 | Export Module Register; verify completeness |
| MG-02 | Sample 5 module changes; verify approval tickets |
| AU-01 | Export 10 auth logs; verify mandatory fields |
| AU-02 | Trace correlation ID across events |
| FM-01 | Review feature changes; verify approval |
| BP-01 | Verify critical jobs succeeded in last 7 days |
| IN-02 | Verify no credentials past rotation date |
| AI-01 | Verify AI disabled by default for new tenants |
| AI-02 | Sample 10 AI logs; verify mandatory fields |

---

## 7. Exceptions

Any exception to this policy must be:

1. **Documented** in exception register
2. **Risk-assessed** with impact analysis
3. **Approved** by Security and GRC leadership
4. **Time-bound** with remediation plan
5. **Logged** with `AM12_SOD_EXCEPTION_GRANTED`

---

## 8. Enforcement

Non-compliance may result in:

1. Immediate account suspension (`Suspended` status)
2. Configuration rollback
3. Escalation to platform administrators
4. Security incident report
5. Remediation tracking

---

## 9. Appendix A: Event Mapping Guidance

### 9.1 Actions

| Action | Use Case |
|--------|----------|
| Create | Entity creation, invites, provisioning |
| Update | Status changes, role changes, config changes |
| Delete | Soft-delete operations |
| Approve | Workflow approvals, evidence verification |
| Reject | Workflow rejections, denied requests |

### 9.2 Severity Guidelines

| Severity | Use Case |
|----------|----------|
| Critical | Auth bypass, privilege escalation, suspicious impersonation |
| Error | Repeated failed provisioning, compliance job failures |
| Warning | Role changes, feature/integration enablement |
| Info | Standard successful operations |

---

## 10. Appendix B: Control Reference

### Access Management (AM)

| Control | Name | Objective |
|---------|------|-----------|
| AM-01 | Identity Proofing | Ensure only legitimate users become Active |
| AM-02 | Trial Provisioning | Prevent unauthorized tenant creation |
| AM-03 | RBAC | Enforce least privilege |
| AM-04 | Privileged Access | Protect privileged accounts |
| AM-05 | JML Lifecycle | Manage joiner/mover/leaver |
| AM-06 | Invitation Control | Prevent invite abuse |
| AM-07 | Abuse Prevention | Block bot signups and DoS |
| AM-08 | Password Controls | Secure password handling |
| AM-09 | Trial Governance | Control trial lifecycle |
| AM-10 | Audit Logging | Maintain audit trail |
| AM-11 | Access Reviews | Periodic access certification |
| AM-12 | Separation of Duties | Prevent conflicts |

### Module Governance (MG)

| Control | Name | Objective |
|---------|------|-----------|
| MG-01 | Module Inventory | Track all modules |
| MG-02 | Change Approval | Control module changes |
| MG-03 | Environment Parity | Ensure consistency |

### Auditability (AU)

| Control | Name | Objective |
|---------|------|-----------|
| AU-01 | Access Logging | Log authentication events |
| AU-02 | Business Logging | Log business events |
| AU-03 | Platform Audit | Log admin actions |

### Feature Management (FM)

| Control | Name | Objective |
|---------|------|-----------|
| FM-01 | Feature Flag Governance | Control feature changes |

### Background Processing (BP)

| Control | Name | Objective |
|---------|------|-----------|
| BP-01 | Job Governance | Ensure compliance jobs run |
| BP-02 | Worker Documentation | Document compensating controls |

### Integration (IN)

| Control | Name | Objective |
|---------|------|-----------|
| IN-01 | Integration Approval | Control external integrations |
| IN-02 | Credential Management | Secure credentials |

### AI Governance (AI)

| Control | Name | Objective |
|---------|------|-----------|
| AI-01 | Feature Enablement | Require tenant opt-in |
| AI-02 | Usage Logging | Log all AI usage |

---

## 11. Document Control

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-06-01 | Security Team | Initial release |
| 2.0 | 2026-01-19 | Platform Team | Added MG, AU, FM, BP, IN, AI controls; expanded from 12 to 25 controls |

---

*End of Policy Document*
