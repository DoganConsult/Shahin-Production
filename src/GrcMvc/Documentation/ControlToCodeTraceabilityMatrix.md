# Control-to-Code Traceability Matrix

**System:** GRC Platform (ABP Framework-based)
**Version:** 1.0
**Last Updated:** 2026-01-19
**Purpose:** Map each policy control to implementation code and audit evidence

---

## 1. Access Management Controls (AM-01 to AM-12)

### AM-01: Identity Proofing and Account Activation

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/AccessManagementControls.cs` → `AM01_IdentityProofing` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `AccessManagementAuditEvents.AM01_*` |
| **Implementation** | |
| - Self Registration | `Controllers/AccountApiController.cs` → `Register()` |
| - Email Verification | `Controllers/AuthActivateController.cs` → `VerifyEmail()` |
| - Status Update | `Services/Implementations/UserService.cs` → `ActivateUserAsync()` |
| **Evidence** | AuditEvent records with AM01_* EventType |

### AM-02: Secure Trial Provisioning Authorization

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/AccessManagementControls.cs` → `AM02_TrialProvisioning` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `AccessManagementAuditEvents.AM02_*` |
| **Implementation** | |
| - Provision Endpoint | `Controllers/Api/TenantsApiController.cs` → `ProvisionTrial()` |
| - Rate Limiting | `Middleware/RateLimitingMiddleware.cs` |
| - Auth Validation | `Services/Implementations/ProvisioningService.cs` |
| **Evidence** | AuditEvent records with AM02_* EventType, rate limit logs |

### AM-03: Role-Based Access Control and Least Privilege

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/AccessManagementControls.cs` → `AM03_RBAC` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `AccessManagementAuditEvents.AM03_*` |
| **Implementation** | |
| - Role Constants | `Constants/RoleConstants.cs` → All roles + normalization |
| - Role Assignment | `Services/Implementations/UserService.cs` → `AssignRoleAsync()` |
| - Permission Check | `Services/Implementations/PermissionService.cs` |
| **Evidence** | AuditEvent records with AM03_* EventType |

### AM-04: Privileged Access Safeguards

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/AccessManagementControls.cs` → `AM04_PrivilegedAccess` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `AccessManagementAuditEvents.AM04_*` |
| **Implementation** | |
| - MFA Service | `Services/Implementations/MfaService.cs` |
| - Step-Up Auth | `Services/Implementations/AuthenticationService.cs` → `RequireStepUp()` |
| - Privileged Roles | `Constants/RoleConstants.cs` → `PrivilegedRoles` array |
| **Evidence** | AuditEvent AM04_*, AuthenticationAuditLog (2FAEnabled, etc.) |

### AM-05: Joiner/Mover/Leaver Lifecycle Management

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/AccessManagementControls.cs` → `AM05_JMLLifecycle` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `AccessManagementAuditEvents.AM05_*` |
| **Implementation** | |
| - User Lifecycle | `Services/Implementations/UserService.cs` → `SuspendAsync()`, `ReactivateAsync()` |
| - Inactivity Check | `Jobs/InactivitySuspensionJob.cs` |
| **Evidence** | AuditEvent records with AM05_* EventType |

### AM-06: Invitation Control and Integrity

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/AccessManagementControls.cs` → `AM06_InvitationControl` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `AccessManagementAuditEvents.AM06_*` |
| **Implementation** | |
| - Invite Creation | `Controllers/Api/UserInvitationController.cs` → `InviteUser()` |
| - Invite Service | `Services/Implementations/InvitationService.cs` |
| - Rate Limiting | `Services/Implementations/InvitationService.cs` → `CheckRateLimit()` |
| **Evidence** | AuditEvent records with AM06_* EventType |

### AM-07: Registration and Provisioning Abuse Prevention

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/AccessManagementControls.cs` → `AM07_AbusePrevention` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `AccessManagementAuditEvents.AM07_*` |
| **Implementation** | |
| - CAPTCHA | `Services/Implementations/CaptchaService.cs` |
| - IP Blocking | `Services/Implementations/AbusePreventionService.cs` |
| - Rate Limiting | `Middleware/RateLimitingMiddleware.cs` |
| **Evidence** | AuditEvent records with AM07_* EventType |

### AM-08: Password and Recovery Controls

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/AccessManagementControls.cs` → `AM08_PasswordControls` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `AccessManagementAuditEvents.AM08_*` |
| **Implementation** | |
| - Password Set | `Services/Implementations/AuthenticationService.cs` → `SetPasswordAsync()` |
| - Password Reset | `Controllers/AccountApiController.cs` → `ResetPassword()` |
| - Account Lock | `Services/Implementations/AuthenticationService.cs` → `LockAccount()` |
| **Evidence** | AuditEvent AM08_*, AuthenticationAuditLog (PasswordChanged, AccountLocked) |

### AM-09: Trial Tenant Governance

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/AccessManagementControls.cs` → `AM09_TrialGovernance` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `AccessManagementAuditEvents.AM09_*` |
| **Implementation** | |
| - Trial Start | `Services/Implementations/TenantService.cs` → `CreateTrialAsync()` |
| - Trial Expiry | `Jobs/TrialExpiryCheckerJob.cs` |
| - Data Retention | `Jobs/TrialDataRetentionJob.cs` |
| **Evidence** | AuditEvent records with AM09_* EventType, Hangfire job logs |

### AM-10: Audit Logging and Traceability

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/AccessManagementControls.cs` → `AM10_AuditLogging` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `AccessManagementAuditEvents.AM10_*` |
| **Implementation** | |
| - Audit Service | `Services/Implementations/AuditEventService.cs` |
| - Export | `Services/Implementations/AuditEventService.cs` → `ExportEventsAsync()` |
| - Retention | `Jobs/AuditLogRetentionJob.cs` |
| **Evidence** | AuditEvent records, export files, retention job logs |

### AM-11: Periodic Access Reviews

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/AccessManagementControls.cs` → `AM11_AccessReviews` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `AccessManagementAuditEvents.AM11_*` |
| **Implementation** | |
| - Entity | `Models/Entities/AccessReview.cs` |
| - Service Interface | `Services/Interfaces/IAccessReviewService.cs` |
| - Service Impl | `Services/Implementations/AccessReviewService.cs` |
| - Reminder Job | `Jobs/AccessReviewReminderJob.cs` |
| **Evidence** | AccessReview records, AuditEvent AM11_* |

### AM-12: Separation of Duties for GRC Actions

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/AccessManagementControls.cs` → `AM12_SeparationOfDuties` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `AccessManagementAuditEvents.AM12_*` |
| **Implementation** | |
| - SoD Rules | `Services/Implementations/SoDService.cs` |
| - Violation Check | `Services/Implementations/SoDService.cs` → `CheckViolation()` |
| **Evidence** | AuditEvent records with AM12_* EventType |

---

## 2. Module Governance Controls (MG-01 to MG-03)

### MG-01: Module Inventory and Traceability

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/ModuleGovernanceControls.cs` → `MG01_ModuleInventory` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `ModuleGovernanceAuditEvents.MG01_*` |
| **Implementation** | |
| - Module Register | `Configuration/ModuleGovernanceControls.cs` → `AbpModuleRegister` |
| - Module Definition | `Configuration/ModuleGovernanceControls.cs` → `ModuleRegisterEntry` |
| **Evidence** | Module Register export (ModuleRegister.md), change tickets |

### MG-02: Module Change Approval

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/ModuleGovernanceControls.cs` → `MG02_ChangeApproval` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `ModuleGovernanceAuditEvents.MG02_*` |
| **Implementation** | |
| - Change Process | Azure DevOps tickets + Git commits |
| - ABP Module Config | `Abp/GrcMvcAbpModule.cs` |
| **Evidence** | Change tickets with approval, deployment logs |

### MG-03: Environment Parity

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/ModuleGovernanceControls.cs` → `MG03_EnvironmentParity` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `ModuleGovernanceAuditEvents.MG03_*` |
| **Implementation** | |
| - Config Files | `appsettings.json`, `appsettings.{env}.json` |
| - CI/CD Check | Azure DevOps pipeline |
| **Evidence** | Parity check CI/CD output, exception documentation |

---

## 3. Auditability Controls (AU-01 to AU-03)

### AU-01: Access and Authentication Logging

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/AuditabilityControls.cs` → `AU01_AccessLogging` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `AuditabilityAuditEvents.AU01_*` |
| **Implementation** | |
| - Auth Events | `Models/Entities/AuthenticationAuditLog.cs` |
| - Logging Service | `Services/Implementations/AuthenticationAuditService.cs` |
| **Evidence** | AuthenticationAuditLog query export |

### AU-02: Business Event Logging

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/AuditabilityControls.cs` → `AU02_BusinessEventLogging` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `AuditabilityAuditEvents.AU02_*` |
| **Implementation** | |
| - Audit Event Entity | `Models/Entities/AuditEvent.cs` |
| - Audit Service | `Services/Implementations/AuditEventService.cs` |
| **Evidence** | AuditEvent query export, correlation traces |

### AU-03: Platform Admin Audit Trail

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/AuditabilityControls.cs` → `AU03_PlatformAdminAudit` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `PlatformAuditEventTypes` |
| **Implementation** | |
| - Platform Events | `Services/Interfaces/IAuditEventService.cs` → `PlatformAuditEventTypes` |
| - Logging | `Services/Implementations/AuditEventService.cs` → `LogPlatformAdminActionAsync()` |
| **Evidence** | Platform audit event export |

---

## 4. Feature Management Control (FM-01)

### FM-01: Feature Flag Governance

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/FeatureManagementControls.cs` → `FM01_FeatureFlagGovernance` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `FeatureManagementAuditEvents.FM01_*` |
| **Implementation** | |
| - Feature Definitions | `Configuration/FeatureManagementControls.cs` → `GrcFeatures` |
| - ABP Features | ABP FeatureManagement module |
| - Feature Options | `Configuration/GrcFeatureOptions.cs` |
| **Evidence** | Feature change audit log, approval tickets |

---

## 5. Background Processing Controls (BP-01 to BP-02)

### BP-01: Background Job Governance (Hangfire)

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/BackgroundProcessingControls.cs` → `BP01_JobGovernance` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `BackgroundProcessingAuditEvents.BP01_*` |
| **Implementation** | |
| - Job Registry | `Configuration/BackgroundProcessingControls.cs` → `HangfireJobRegistry` |
| - Critical Jobs | `Jobs/TrialExpiryCheckerJob.cs`, `Jobs/AccessReviewReminderJob.cs`, etc. |
| - Hangfire Config | `Program.cs` → Hangfire configuration |
| **Evidence** | Hangfire dashboard, job success/failure metrics |

### BP-02: ABP Worker Disablement Documentation

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/BackgroundProcessingControls.cs` → `BP02_WorkerDisablement` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `BackgroundProcessingAuditEvents.BP02_*` |
| **Implementation** | |
| - Known Issue | `Configuration/BackgroundProcessingControls.cs` → `KnownIssue` class |
| - Disabled Modules | `Abp/GrcMvcAbpModule.cs` → disabled worker modules |
| **Evidence** | PLATFORM-001 ticket, compensating control documentation |

---

## 6. Integration Controls (IN-01 to IN-02)

### IN-01: Integration Enablement Approval

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/IntegrationControls.cs` → `IN01_IntegrationApproval` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `IntegrationAuditEvents.IN01_*` |
| **Implementation** | |
| - Integration Defs | `Configuration/IntegrationControls.cs` → `IntegrationDefinition` |
| - Status Tracker | `Configuration/IntegrationControls.cs` → `IntegrationStatus` |
| - Config Files | `Configuration/*.Settings.cs` (RabbitMqSettings, SlackSettings, etc.) |
| **Evidence** | Integration approval tickets, configuration audit log |

### IN-02: Integration Credential Management

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/IntegrationControls.cs` → `IN02_CredentialManagement` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `IntegrationAuditEvents.IN02_*` |
| **Implementation** | |
| - Credential Policy | `Configuration/IntegrationControls.cs` → `CredentialPolicy` |
| - Key Vault | Azure Key Vault configuration |
| **Evidence** | Key Vault secret inventory, rotation logs |

---

## 7. AI Governance Controls (AI-01 to AI-02)

### AI-01: AI Feature Enablement Governance

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/AIGovernanceControls.cs` → `AI01_FeatureEnablement` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `AIGovernanceAuditEvents.AI01_*` |
| **Implementation** | |
| - AI Features | `Configuration/AIGovernanceControls.cs` → `AIFeatureDefinition` |
| - Tenant Settings | `Configuration/AIGovernanceControls.cs` → `TenantAISettings` |
| - Feature Flags | `Configuration/FeatureManagementControls.cs` → `GrcFeatures.AI` |
| **Evidence** | Tenant AI opt-in records, feature enablement audit |

### AI-02: AI Usage Logging and Transparency

| Aspect | Reference |
|--------|-----------|
| **Control File** | `Configuration/AIGovernanceControls.cs` → `AI02_UsageLogging` |
| **Event Constants** | `Services/Interfaces/IAuditEventService.cs` → `AIGovernanceAuditEvents.AI02_*` |
| **Implementation** | |
| - Usage Record | `Configuration/AIGovernanceControls.cs` → `AIUsageRecord` |
| - Model Defs | `Configuration/AIGovernanceControls.cs` → `AIModelDefinition` |
| - Claude Config | `Configuration/ClaudeApiSettings.cs` |
| - AI Service | `Services/Implementations/ShahinModuleServices.cs` |
| **Evidence** | AI usage audit log export, token/cost reports |

---

## 8. Endpoint-to-Control Mapping

### User Creation Endpoints

| Endpoint | File | Controls |
|----------|------|----------|
| `POST /api/account/register` | `Controllers/AccountApiController.cs` | AM-01, AM-07, AM-08 |
| `POST /api/trial/signup` | `Controllers/Api/TenantsApiController.cs` | AM-01, AM-02, AM-09 |
| `POST /api/trial/provision` | `Controllers/Api/TenantsApiController.cs` | AM-02, AM-03, AM-09 |
| `POST /api/tenants/{id}/users/invite` | `Controllers/Api/UserInvitationController.cs` | AM-01, AM-03, AM-06 |
| `POST /api/account/verify-email` | `Controllers/AuthActivateController.cs` | AM-01 |
| `POST /api/account/accept-invite` | `Controllers/Api/UserInvitationController.cs` | AM-01, AM-06 |

### Authentication Endpoints

| Endpoint | File | Controls |
|----------|------|----------|
| `POST /api/account/login` | `Controllers/AccountApiController.cs` | AU-01, AM-04, AM-08 |
| `POST /api/account/logout` | `Controllers/AccountApiController.cs` | AU-01 |
| `POST /api/account/reset-password` | `Controllers/AccountApiController.cs` | AM-08, AU-01 |
| `POST /api/account/change-password` | `Controllers/AccountApiController.cs` | AM-08, AU-01 |
| `POST /api/account/enable-mfa` | `Controllers/AccountApiController.cs` | AM-04, AU-01 |

### Role Management Endpoints

| Endpoint | File | Controls |
|----------|------|----------|
| `POST /api/users/{id}/roles` | `Controllers/Api/UserProfileController.cs` | AM-03, AM-04 |
| `DELETE /api/users/{id}/roles/{role}` | `Controllers/Api/UserProfileController.cs` | AM-03, AM-05 |
| `GET /api/roles` | `Controllers/RoleProfileController.cs` | AM-03 |

### Access Review Endpoints

| Endpoint | File | Controls |
|----------|------|----------|
| `POST /api/access-reviews` | `Controllers/Api/AccessReviewController.cs` | AM-11 |
| `POST /api/access-reviews/{id}/start` | `Controllers/Api/AccessReviewController.cs` | AM-11 |
| `POST /api/access-reviews/{id}/items/{itemId}/decision` | `Controllers/Api/AccessReviewController.cs` | AM-11 |
| `POST /api/access-reviews/{id}/complete` | `Controllers/Api/AccessReviewController.cs` | AM-11 |

---

## 9. Authoritative Code References

### Constants Files

| File | Purpose |
|------|---------|
| `Constants/RoleConstants.cs` | All 21 roles, normalization, hierarchy |
| `Constants/ClaimConstants.cs` | Custom claim types |
| `Constants/StatusConstants.cs` | Entity status values |

### Configuration Files

| File | Purpose |
|------|---------|
| `Configuration/AccessManagementControls.cs` | AM-01 to AM-12 control definitions |
| `Configuration/ModuleGovernanceControls.cs` | MG-01 to MG-03 + Module Register |
| `Configuration/AuditabilityControls.cs` | AU-01 to AU-03 control definitions |
| `Configuration/FeatureManagementControls.cs` | FM-01 + GrcFeatures |
| `Configuration/BackgroundProcessingControls.cs` | BP-01 to BP-02 + Hangfire registry |
| `Configuration/IntegrationControls.cs` | IN-01 to IN-02 + integration inventory |
| `Configuration/AIGovernanceControls.cs` | AI-01 to AI-02 + AI feature definitions |

### Service Interfaces

| File | Purpose |
|------|---------|
| `Services/Interfaces/IAuditEventService.cs` | All 160 audit event constants |
| `Services/Interfaces/IAccessReviewService.cs` | AM-11 service contract |

### Entity Files

| File | Purpose |
|------|---------|
| `Models/Entities/AuditEvent.cs` | Business audit record |
| `Models/Entities/AuthenticationAuditLog.cs` | Auth audit record |
| `Models/Entities/AccessReview.cs` | AM-11 entities |

---

## 10. Evidence Collection Summary

| Control | Primary Evidence | Secondary Evidence |
|---------|-----------------|-------------------|
| AM-01 to AM-12 | AuditEvent (AM*_*) | AuthenticationAuditLog |
| MG-01 | Module Register export | Change tickets |
| MG-02 | Change tickets | Git commits |
| MG-03 | CI/CD parity check | Exception docs |
| AU-01 | AuthenticationAuditLog | Serilog |
| AU-02 | AuditEvent | Correlation traces |
| AU-03 | PlatformAuditEvent | Admin action logs |
| FM-01 | Feature change audit | Approval tickets |
| BP-01 | Hangfire dashboard | Job metrics |
| BP-02 | PLATFORM-001 ticket | Exit plan doc |
| IN-01 | Integration tickets | Config audit |
| IN-02 | Key Vault logs | Rotation schedule |
| AI-01 | Tenant opt-in records | Feature audit |
| AI-02 | AIUsageRecord | Cost reports |

---

*End of Traceability Matrix*
