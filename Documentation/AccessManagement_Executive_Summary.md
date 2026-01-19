# Access Management Executive Summary

**Document ID:** AM-EXEC-2026-001
**Version:** 1.0
**Date:** January 19, 2026
**Classification:** Internal - Management

---

## 1. Executive Overview

This document summarizes the Access Management control framework implemented in the GRC Platform, covering 12 controls (AM-01 through AM-12) that govern user identity, authentication, authorization, and access lifecycle management.

### Key Metrics

| Metric | Value |
|--------|-------|
| Total Controls | 12 |
| Audit Events Defined | 72 |
| Roles Defined | 21 (2 Platform + 19 Tenant) |
| User Creation Flows | 3 |
| MFA Support | Enabled |

---

## 2. Control Summary

### AM-01: User Identity Management
**Status:** Implemented
**Purpose:** Ensure every user has a unique, traceable identity linked to a real person or service account.

- Unique email enforcement
- Full name required
- 3 creation flows: Self-Registration, Trial Provisioning, Admin Invite
- All creations logged with audit trail

### AM-02: Authentication Security
**Status:** Implemented
**Purpose:** Ensure authentication mechanisms meet security standards.

- Password policy: 12+ chars, complexity required
- MFA support (TOTP)
- Account lockout after 5 failed attempts
- Session timeout: 8 hours

### AM-03: Role-Based Access Control (RBAC)
**Status:** Implemented
**Purpose:** Ensure users are granted access based on defined roles.

- 21 roles across platform and tenant scopes
- Permission-based authorization
- Role assignment audit logging
- Default role: TenantUser

### AM-04: Access Reviews
**Status:** In Development
**Purpose:** Periodic review of user access to ensure appropriateness.

- Quarterly review cycle
- Manager-based review workflow
- Automated reminder system
- Decision tracking: Approve/Revoke/Modify

### AM-05: Privileged Access Management
**Status:** Implemented
**Purpose:** Additional controls for privileged accounts.

- PlatformAdmin requires approval
- MFA mandatory for privileged roles
- Session recording for admin actions
- Just-in-time access support

### AM-06: Service Account Management
**Status:** Implemented
**Purpose:** Governance for non-human identities.

- Dedicated service account type
- Owner assignment required
- Credential rotation policy
- Usage monitoring

### AM-07: Session Management
**Status:** Implemented
**Purpose:** Secure session handling.

- JWT-based sessions
- 8-hour default timeout
- Concurrent session limits
- Secure cookie handling

### AM-08: Password Policy
**Status:** Implemented
**Purpose:** Enforce strong password standards.

- Minimum 12 characters
- Complexity requirements
- 90-day expiry (configurable)
- History: last 5 passwords blocked

### AM-09: Trial Lifecycle
**Status:** Implemented
**Purpose:** Manage trial tenant access lifecycle.

- 14-day default trial
- Expiry warnings at 7, 3, 1 days
- Grace period handling
- Data retention on conversion

### AM-10: Audit Logging
**Status:** Implemented
**Purpose:** Comprehensive audit trail.

- All access events logged
- 7-year retention
- Export capability
- Tamper-evident storage

### AM-11: Periodic Access Reviews
**Status:** In Development
**Purpose:** Scheduled review campaigns.

- Automated campaign creation
- Manager-based workflows
- Overdue escalation
- Completion tracking

### AM-12: Segregation of Duties (SoD)
**Status:** Planned
**Purpose:** Prevent conflicting access combinations.

- Rule-based conflict detection
- Pre-assignment validation
- Exception workflow
- Violation reporting

---

## 3. User Creation Flows

### Flow 1: Self-Registration (`/api/auth/register`)
```
User → AccountApiController → AuthenticationService.RegisterAsync
     → AM01_USER_CREATED, AM01_USER_REGISTERED
```

### Flow 2: Trial Provisioning (`/api/trial/provision`)
```
User → TrialApiController → TrialLifecycleService.ProvisionTrialAsync
     → AM01_TENANT_CREATED, AM01_USER_CREATED, AM03_ROLE_ASSIGNED
```

### Flow 3: Admin Invite (`/api/tenants/{id}/users/invite`)
```
Admin → UserInvitationController → UserInvitationService.InviteUserAsync
      → AM01_USER_INVITED

Invitee → InvitationAcceptController → AcceptInvitationAsync
        → AM01_USER_CREATED, AM03_ROLE_ASSIGNED
```

---

## 4. Role Hierarchy

### Platform Roles (2)
| Role | Scope | Purpose |
|------|-------|---------|
| PlatformAdmin | Global | Full platform administration |
| PlatformSupport | Global | Support operations, read-only admin |

### Tenant Roles (19)
| Role | Level | Key Permissions |
|------|-------|-----------------|
| TenantAdmin | Admin | Full tenant administration |
| SecurityAdmin | Admin | Security settings, user management |
| ComplianceOfficer | Manager | Compliance oversight |
| RiskManager | Manager | Risk management |
| AuditManager | Manager | Audit management |
| PolicyAdmin | Manager | Policy administration |
| VendorManager | Manager | Vendor management |
| EvidenceManager | Specialist | Evidence handling |
| ControlOwner | Specialist | Control ownership |
| RiskAnalyst | Analyst | Risk analysis |
| ComplianceAnalyst | Analyst | Compliance analysis |
| Auditor | Analyst | Audit execution |
| InternalAuditor | Analyst | Internal audit |
| ExternalAuditor | Analyst | External audit (limited) |
| Reviewer | Reviewer | Review and approve |
| Approver | Reviewer | Approval authority |
| ReadOnlyUser | Basic | Read-only access |
| TenantUser | Basic | Standard user |
| Guest | Basic | Limited guest access |

---

## 5. Compliance Mapping

| Framework | Relevant Controls |
|-----------|-------------------|
| SOC 2 | CC6.1, CC6.2, CC6.3, CC6.6 |
| ISO 27001 | A.9.1, A.9.2, A.9.3, A.9.4 |
| NIST CSF | PR.AC-1, PR.AC-4, PR.AC-6 |
| GDPR | Art. 5(1)(f), Art. 32 |

---

## 6. Implementation Status

| Control | Status | Evidence Available |
|---------|--------|-------------------|
| AM-01 | Implemented | User audit logs, creation flows |
| AM-02 | Implemented | Auth logs, lockout records |
| AM-03 | Implemented | Role assignments, permission matrix |
| AM-04 | In Development | Planned Q1 2026 |
| AM-05 | Implemented | Admin action logs |
| AM-06 | Implemented | Service account registry |
| AM-07 | Implemented | Session logs |
| AM-08 | Implemented | Password policy config |
| AM-09 | Implemented | Trial lifecycle logs |
| AM-10 | Implemented | Audit export capability |
| AM-11 | In Development | Planned Q1 2026 |
| AM-12 | Planned | Q2 2026 roadmap |

---

## 7. Key Risks and Mitigations

| Risk | Mitigation | Status |
|------|------------|--------|
| Stale access permissions | AM-04/AM-11 access reviews | In Development |
| Privileged access abuse | AM-05 PAM controls + logging | Implemented |
| Weak passwords | AM-08 password policy | Implemented |
| Session hijacking | AM-07 secure session mgmt | Implemented |
| SoD violations | AM-12 conflict detection | Planned |

---

## 8. Recommendations

1. **Complete AM-04 Implementation** - Priority: High
   - Create AccessReview data model
   - Implement review workflow API
   - Deploy Hangfire reminder jobs

2. **Enable MFA Enforcement** - Priority: High
   - Enforce MFA for all admin roles
   - Implement MFA enrollment flow

3. **Implement SoD Rules** - Priority: Medium
   - Define conflicting role combinations
   - Implement pre-assignment validation

4. **Access Review Automation** - Priority: Medium
   - Automated quarterly campaigns
   - Manager notification workflows

---

## 9. Audit Readiness Checklist

- [x] User identity uniqueness enforced
- [x] Authentication logs available
- [x] Role assignment audit trail
- [ ] Access review records (AM-04 in development)
- [x] Privileged access monitoring
- [x] Session management logs
- [x] Password policy enforcement
- [x] Trial lifecycle tracking
- [x] Audit log export capability
- [ ] SoD violation reports (AM-12 planned)

---

## 10. Contact Information

| Role | Contact |
|------|---------|
| Control Owner | Security Engineering |
| Technical Lead | Platform Engineering |
| Audit Liaison | Compliance Team |

---

*Document generated: January 19, 2026*
*Next review date: April 19, 2026*
