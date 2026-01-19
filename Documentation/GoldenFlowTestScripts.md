# Golden Flow Test Scripts (UAT)

**Document ID:** GATE-B-UAT-2026-001
**Version:** 1.0
**Date:** January 19, 2026
**Classification:** Internal - QA/Operations

---

## Overview

These test scripts validate the **non-negotiable Golden Flows** required for production readiness.
Execute these in **staging environment** with production-like configuration.

---

## Pre-requisites

- [ ] Staging environment deployed with latest build
- [ ] Database migrations applied
- [ ] Email service configured (for invite flows)
- [ ] Test tenant created OR using provision flow to create one
- [ ] API testing tool ready (Postman, curl, or similar)

---

## Flow B1: Self Registration

### Endpoint
`POST /api/auth/register`

### Steps

| Step | Action | Expected Result | Pass/Fail |
|------|--------|-----------------|-----------|
| 1 | POST `/api/auth/register` with body: `{"email": "test-user-{timestamp}@test.com", "password": "Test@12345678", "fullName": "Test User"}` | 201 Created, user object returned | |
| 2 | Query `Users` table for email | Record exists with correct FullName | |
| 3 | Query `AuditEvents` for user ID | `AM01_USER_CREATED` event exists | |
| 4 | Query `AuditEvents` for user ID | `AM01_USER_REGISTERED` event exists | |
| 5 | POST `/api/auth/login` with same credentials | 200 OK, JWT token returned | |
| 6 | Decode JWT token | Contains sub, email, name claims | |

### Evidence to Capture
- [ ] API request/response (sanitize password)
- [ ] User record screenshot
- [ ] AuditEvent records screenshot
- [ ] JWT token decode (payload only)

### Expected Audit Events
```
AM01_USER_CREATED
AM01_USER_REGISTERED
```

---

## Flow B2: Trial Signup

### Endpoint
`POST /api/trial/signup`

### Steps

| Step | Action | Expected Result | Pass/Fail |
|------|--------|-----------------|-----------|
| 1 | POST `/api/trial/signup` with body: `{"companyName": "Test Company", "email": "admin-{timestamp}@testco.com", "fullName": "Admin User", "industry": "Technology"}` | 200/201, trial record returned | |
| 2 | Query `Trials` table for company name | Record exists with Status = "Pending" | |
| 3 | Query `AuditEvents` for trial ID | `AM01_TRIAL_SIGNUP_INITIATED` event exists | |

### Evidence to Capture
- [ ] API request/response
- [ ] Trial record screenshot
- [ ] AuditEvent record screenshot

### Expected Audit Events
```
AM01_TRIAL_SIGNUP_INITIATED
```

---

## Flow B3: Trial Provision

### Endpoint
`POST /api/trial/provision`

### Pre-requisite
- Trial ID from Flow B2

### Steps

| Step | Action | Expected Result | Pass/Fail |
|------|--------|-----------------|-----------|
| 1 | POST `/api/trial/provision` with trial ID | 200 OK, tenant + user info returned | |
| 2 | Query `Tenants` table | New tenant created with trial.CompanyName | |
| 3 | Query `Users` table | User created with trial email | |
| 4 | Query `TenantUsers` table | Junction record exists with RoleCode = "TenantAdmin" | |
| 5 | Query `AuditEvents` | `AM01_TENANT_CREATED` event exists | |
| 6 | Query `AuditEvents` | `AM01_USER_CREATED` event exists | |
| 7 | Query `AuditEvents` | `AM03_ROLE_ASSIGNED` event exists | |
| 8 | POST `/api/auth/login` with provisioned user | Login succeeds immediately | |

### Evidence to Capture
- [ ] API request/response
- [ ] Tenant record screenshot
- [ ] User record screenshot
- [ ] TenantUser record screenshot (showing RoleCode)
- [ ] AuditEvent records (3 events)
- [ ] Successful login response

### Expected Audit Events
```
AM01_TENANT_CREATED
AM01_USER_CREATED
AM03_ROLE_ASSIGNED
```

---

## Flow B4: User Invite

### Endpoint
`POST /api/tenants/{tenantId}/users/invite`

### Pre-requisite
- Valid tenant ID
- Authenticated as TenantAdmin

### Steps

| Step | Action | Expected Result | Pass/Fail |
|------|--------|-----------------|-----------|
| 1 | POST `/api/tenants/{tenantId}/users/invite` with body: `{"email": "invited-{timestamp}@test.com", "roleCode": "TenantUser", "message": "Welcome to the team"}` | 200/201, invitation object returned | |
| 2 | Query `UserInvitations` table | Record exists with Status = "Pending" | |
| 3 | Query `AuditEvents` | `AM01_USER_INVITED` event exists | |
| 4 | Check email delivery logs | Email sent with invitation link | |

### Evidence to Capture
- [ ] API request/response
- [ ] UserInvitation record screenshot
- [ ] AuditEvent record screenshot
- [ ] Email delivery evidence (message ID, timestamp)

### Expected Audit Events
```
AM01_USER_INVITED
```

---

## Flow B5: Accept Invite

### Endpoint
`POST /api/invitation/accept`

### Pre-requisite
- Invitation token from Flow B4

### Steps

| Step | Action | Expected Result | Pass/Fail |
|------|--------|-----------------|-----------|
| 1 | POST `/api/invitation/accept` with body: `{"token": "{invitationToken}", "password": "NewUser@12345", "fullName": "Invited User"}` | 200/201, user created | |
| 2 | Query `Users` table | User exists with invited email | |
| 3 | Query `TenantUsers` table | Junction record with assigned RoleCode | |
| 4 | Query `UserInvitations` table | Status changed to "Accepted" | |
| 5 | Query `AuditEvents` | `AM01_USER_CREATED` event exists | |
| 6 | Query `AuditEvents` | `AM03_ROLE_ASSIGNED` event exists | |
| 7 | POST `/api/auth/login` with new user | Login succeeds | |

### Evidence to Capture
- [ ] API request/response (sanitize password)
- [ ] User record screenshot
- [ ] TenantUser record screenshot
- [ ] UserInvitation record showing Accepted
- [ ] AuditEvent records (2 events)
- [ ] Successful login response

### Expected Audit Events
```
AM01_USER_CREATED
AM03_ROLE_ASSIGNED
```

---

## Flow B6: Role Change

### Endpoint
`PUT /api/tenants/{tenantId}/users/{userId}/roles`

### Pre-requisite
- Valid tenant ID and user ID
- Authenticated as TenantAdmin

### Steps

| Step | Action | Expected Result | Pass/Fail |
|------|--------|-----------------|-----------|
| 1 | GET current user's role | Note current RoleCode | |
| 2 | PUT `/api/tenants/{tenantId}/users/{userId}/roles` with body: `{"roleCode": "ComplianceOfficer"}` | 200 OK, updated record | |
| 3 | Query `TenantUsers` table | RoleCode updated to "ComplianceOfficer" | |
| 4 | Query `AuditEvents` | `AM03_ROLE_ASSIGNED` or `AM03_ROLE_CHANGED` exists | |
| 5 | Test permission enforcement: Access endpoint requiring ComplianceOfficer role | Access granted | |
| 6 | Revert to original role | Role changed back successfully | |

### Evidence to Capture
- [ ] Before/after TenantUser records
- [ ] API request/response
- [ ] AuditEvent record
- [ ] Permission enforcement test result

### Expected Audit Events
```
AM03_ROLE_ASSIGNED
AM03_ROLE_CHANGED (if applicable)
```

---

## Authentication Logging Verification (Gate C)

### AuthenticationAuditLog Events to Verify

| Event Type | Trigger | Verification Query |
|------------|---------|-------------------|
| Login | Successful login | `SELECT * FROM AuthenticationAuditLog WHERE EventType = 'Login' AND UserId = '{userId}'` |
| FailedLogin | Failed login attempt | `SELECT * FROM AuthenticationAuditLog WHERE EventType = 'FailedLogin'` |
| RoleChanged | Role assignment | `SELECT * FROM AuthenticationAuditLog WHERE EventType = 'RoleChanged'` |
| 2FAEnabled | MFA setup | `SELECT * FROM AuthenticationAuditLog WHERE EventType = '2FAEnabled'` |

---

## Rate Limiting Verification (Gate C)

### Test Procedure

| Step | Action | Expected Result | Pass/Fail |
|------|--------|-----------------|-----------|
| 1 | Note rate limit config (e.g., 10/minute) | Document threshold | |
| 2 | Send requests up to threshold | All succeed with 200 | |
| 3 | Send one more request | 429 Too Many Requests | |
| 4 | Wait for window reset | Requests succeed again | |

### Evidence
- [ ] Rate limit configuration screenshot
- [ ] Successful requests before threshold
- [ ] 429 response at threshold
- [ ] Headers showing rate limit info (if exposed)

---

## Summary Checklist

| Flow | Status | Audit Events Verified | Evidence Captured |
|------|--------|----------------------|-------------------|
| B1: Self Registration | | | |
| B2: Trial Signup | | | |
| B3: Trial Provision | | | |
| B4: User Invite | | | |
| B5: Accept Invite | | | |
| B6: Role Change | | | |
| Auth Logging | | | |
| Rate Limiting | | | |

---

## Sign-off

| Role | Name | Date | Signature |
|------|------|------|-----------|
| QA Lead | | | |
| Security | | | |
| Operations | | | |

---

*Document generated: January 19, 2026*
