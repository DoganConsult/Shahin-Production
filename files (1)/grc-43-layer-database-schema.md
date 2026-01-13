# SHAHIN GRC PLATFORM - 43-LAYER DATABASE SCHEMA
## ABP Framework Multi-Tenant GRC with Autonomous Onboarding
### Author: Ahmet (PhD IoT Healthcare Cybersecurity)
### Version: 1.0

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Layer Definitions](#layer-definitions)
3. [Entity Relationships](#entity-relationships)
4. [Implementation Guidelines](#implementation-guidelines)
5. [Security & Performance](#security--performance)
6. [Migration Strategy](#migration-strategy)

---

## Architecture Overview

The 43-layer database architecture provides logical separation of concerns while maintaining physical optimization. Each layer serves a specific purpose in the GRC ecosystem.

```
┌─────────────────────────────────────────────────────────┐
│                 ABP PLATFORM FOUNDATION                 │
│                    Layers 1-12                         │
├─────────────────────────────────────────────────────────┤
│                ONBOARDING CONTROL PLANE                │
│                    Layers 13-20                        │
├─────────────────────────────────────────────────────────┤
│                 COMPLIANCE CATALOG                     │
│                    Layers 21-30                        │
├─────────────────────────────────────────────────────────┤
│              TENANT COMPLIANCE RESOLUTION              │
│                    Layers 31-36                        │
├─────────────────────────────────────────────────────────┤
│                  EXECUTION LAYERS                      │
│                    Layers 37-43                        │
└─────────────────────────────────────────────────────────┘
```

---

## Layer Definitions

### **LAYERS 1-12: ABP PLATFORM FOUNDATION**
*ABP Framework Core Tables - Multi-tenancy, Identity, Permissions*

#### **Layer 1: Tenants**
```sql
CREATE TABLE AbpTenants (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(64) NOT NULL,
    NormalizedName NVARCHAR(64) NOT NULL,
    ExtraProperties NVARCHAR(MAX) NULL,
    ConcurrencyStamp NVARCHAR(40) NOT NULL,
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatorId UNIQUEIDENTIFIER NULL,
    LastModificationTime DATETIME2 NULL,
    LastModifierId UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeleterId UNIQUEIDENTIFIER NULL,
    DeletionTime DATETIME2 NULL,
    
    -- GRC-specific extensions
    TrialExpiryDate DATETIME2 NULL,
    IsQuarantined BIT NOT NULL DEFAULT 0,
    QuarantineReason NVARCHAR(500) NULL,
    TierLevel INT NOT NULL DEFAULT 1, -- Trial/Basic/Premium/Enterprise
    
    CONSTRAINT IX_AbpTenants_Name UNIQUE (Name),
    CONSTRAINT IX_AbpTenants_NormalizedName UNIQUE (NormalizedName)
);

CREATE INDEX IX_AbpTenants_TrialExpiry ON AbpTenants (TrialExpiryDate) 
WHERE TrialExpiryDate IS NOT NULL;
```

#### **Layer 2: Tenant Connection Strings**
```sql
CREATE TABLE AbpTenantConnectionStrings (
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(64) NOT NULL,
    Value NVARCHAR(1024) NOT NULL,
    
    PRIMARY KEY (TenantId, Name),
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE
);
```

#### **Layer 3: Users**
```sql
CREATE TABLE AbpUsers (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NULL,
    UserName NVARCHAR(256) NOT NULL,
    NormalizedUserName NVARCHAR(256) NOT NULL,
    Email NVARCHAR(256) NULL,
    NormalizedEmail NVARCHAR(256) NULL,
    EmailConfirmed BIT NOT NULL DEFAULT 0,
    PasswordHash NVARCHAR(256) NULL,
    SecurityStamp NVARCHAR(256) NOT NULL,
    PhoneNumber NVARCHAR(16) NULL,
    PhoneNumberConfirmed BIT NOT NULL DEFAULT 0,
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    LockoutEnd DATETIME2 NULL,
    LockoutEnabled BIT NOT NULL DEFAULT 1,
    AccessFailedCount INT NOT NULL DEFAULT 0,
    ExtraProperties NVARCHAR(MAX) NULL,
    ConcurrencyStamp NVARCHAR(40) NOT NULL,
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatorId UNIQUEIDENTIFIER NULL,
    LastModificationTime DATETIME2 NULL,
    LastModifierId UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeleterId UNIQUEIDENTIFIER NULL,
    DeletionTime DATETIME2 NULL,
    
    -- GRC-specific extensions
    FirstName NVARCHAR(50) NULL,
    LastName NVARCHAR(50) NULL,
    JobTitle NVARCHAR(100) NULL,
    Department NVARCHAR(100) NULL,
    LastLoginTime DATETIME2 NULL,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id),
    CONSTRAINT IX_AbpUsers_TenantId_NormalizedUserName UNIQUE (TenantId, NormalizedUserName),
    CONSTRAINT IX_AbpUsers_TenantId_NormalizedEmail UNIQUE (TenantId, NormalizedEmail)
);

CREATE INDEX IX_AbpUsers_TenantId ON AbpUsers (TenantId);
CREATE INDEX IX_AbpUsers_Email ON AbpUsers (NormalizedEmail);
```

#### **Layer 4: Roles**
```sql
CREATE TABLE AbpRoles (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NULL,
    Name NVARCHAR(256) NOT NULL,
    NormalizedName NVARCHAR(256) NOT NULL,
    IsDefault BIT NOT NULL DEFAULT 0,
    IsStatic BIT NOT NULL DEFAULT 0,
    IsPublic BIT NOT NULL DEFAULT 1,
    ExtraProperties NVARCHAR(MAX) NULL,
    ConcurrencyStamp NVARCHAR(40) NOT NULL,
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatorId UNIQUEIDENTIFIER NULL,
    LastModificationTime DATETIME2 NULL,
    LastModifierId UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeleterId UNIQUEIDENTIFIER NULL,
    DeletionTime DATETIME2 NULL,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id),
    CONSTRAINT IX_AbpRoles_TenantId_NormalizedName UNIQUE (TenantId, NormalizedName)
);
```

#### **Layer 5: User Roles**
```sql
CREATE TABLE AbpUserRoles (
    UserId UNIQUEIDENTIFIER NOT NULL,
    RoleId UNIQUEIDENTIFIER NOT NULL,
    TenantId UNIQUEIDENTIFIER NULL,
    
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES AbpUsers(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES AbpRoles(Id) ON DELETE CASCADE,
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id)
);
```

#### **Layer 6: Permissions**
```sql
CREATE TABLE AbpPermissionGrants (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NULL,
    Name NVARCHAR(128) NOT NULL,
    ProviderName NVARCHAR(64) NOT NULL,
    ProviderKey NVARCHAR(64) NOT NULL,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id),
    CONSTRAINT IX_AbpPermissionGrants_TenantId_Name_ProviderName_ProviderKey 
        UNIQUE (TenantId, Name, ProviderName, ProviderKey)
);
```

#### **Layer 7: Features**
```sql
CREATE TABLE AbpFeatures (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(128) NOT NULL,
    Value NVARCHAR(256) NOT NULL,
    ProviderName NVARCHAR(64) NULL,
    ProviderKey NVARCHAR(64) NULL,
    
    CONSTRAINT IX_AbpFeatures_Name_ProviderName_ProviderKey 
        UNIQUE (Name, ProviderName, ProviderKey)
);
```

#### **Layer 8: Settings**
```sql
CREATE TABLE AbpSettings (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(128) NOT NULL,
    Value NVARCHAR(2048) NULL,
    ProviderName NVARCHAR(64) NULL,
    ProviderKey NVARCHAR(64) NULL,
    
    CONSTRAINT IX_AbpSettings_Name_ProviderName_ProviderKey 
        UNIQUE (Name, ProviderName, ProviderKey)
);
```

#### **Layer 9: Security Logs**
```sql
CREATE TABLE AbpSecurityLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NULL,
    ApplicationName NVARCHAR(96) NULL,
    Identity NVARCHAR(96) NULL,
    Action NVARCHAR(96) NULL,
    UserId UNIQUEIDENTIFIER NULL,
    UserName NVARCHAR(256) NULL,
    TenantName NVARCHAR(64) NULL,
    ClientId NVARCHAR(64) NULL,
    CorrelationId NVARCHAR(64) NULL,
    ClientIpAddress NVARCHAR(64) NULL,
    BrowserInfo NVARCHAR(512) NULL,
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExtraProperties NVARCHAR(MAX) NULL,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id),
    FOREIGN KEY (UserId) REFERENCES AbpUsers(Id)
);

CREATE INDEX IX_AbpSecurityLogs_TenantId ON AbpSecurityLogs (TenantId);
CREATE INDEX IX_AbpSecurityLogs_CreationTime ON AbpSecurityLogs (CreationTime);
CREATE INDEX IX_AbpSecurityLogs_UserId ON AbpSecurityLogs (UserId);
```

#### **Layer 10: Audit Logs**
```sql
CREATE TABLE AbpAuditLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NULL,
    UserId UNIQUEIDENTIFIER NULL,
    UserName NVARCHAR(256) NULL,
    ExecutionTime DATETIME2 NOT NULL,
    ExecutionDuration INT NOT NULL,
    ClientIpAddress NVARCHAR(64) NULL,
    ClientName NVARCHAR(128) NULL,
    BrowserInfo NVARCHAR(512) NULL,
    HttpMethod NVARCHAR(16) NULL,
    Url NVARCHAR(256) NULL,
    HttpStatusCode INT NULL,
    Comments NVARCHAR(256) NULL,
    ExtraProperties NVARCHAR(MAX) NULL,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id),
    FOREIGN KEY (UserId) REFERENCES AbpUsers(Id)
);

CREATE TABLE AbpAuditLogActions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NULL,
    AuditLogId UNIQUEIDENTIFIER NOT NULL,
    ServiceName NVARCHAR(256) NULL,
    MethodName NVARCHAR(128) NULL,
    Parameters NVARCHAR(2048) NULL,
    ExecutionTime DATETIME2 NOT NULL,
    ExecutionDuration INT NOT NULL,
    ExtraProperties NVARCHAR(MAX) NULL,
    
    FOREIGN KEY (AuditLogId) REFERENCES AbpAuditLogs(Id) ON DELETE CASCADE
);

CREATE TABLE AbpEntityChanges (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AuditLogId UNIQUEIDENTIFIER NOT NULL,
    TenantId UNIQUEIDENTIFIER NULL,
    ChangeTime DATETIME2 NOT NULL,
    ChangeType TINYINT NOT NULL,
    EntityTenantId UNIQUEIDENTIFIER NULL,
    EntityId NVARCHAR(128) NOT NULL,
    EntityTypeFullName NVARCHAR(128) NOT NULL,
    ExtraProperties NVARCHAR(MAX) NULL,
    
    FOREIGN KEY (AuditLogId) REFERENCES AbpAuditLogs(Id) ON DELETE CASCADE
);

CREATE TABLE AbpEntityPropertyChanges (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NULL,
    EntityChangeId UNIQUEIDENTIFIER NOT NULL,
    NewValue NVARCHAR(512) NULL,
    OriginalValue NVARCHAR(512) NULL,
    PropertyName NVARCHAR(128) NOT NULL,
    PropertyTypeFullName NVARCHAR(64) NOT NULL,
    
    FOREIGN KEY (EntityChangeId) REFERENCES AbpEntityChanges(Id) ON DELETE CASCADE
);
```

#### **Layer 11: Organization Units**
```sql
CREATE TABLE AbpOrganizationUnits (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NULL,
    ParentId UNIQUEIDENTIFIER NULL,
    Code NVARCHAR(95) NOT NULL,
    DisplayName NVARCHAR(128) NOT NULL,
    ExtraProperties NVARCHAR(MAX) NULL,
    ConcurrencyStamp NVARCHAR(40) NOT NULL,
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatorId UNIQUEIDENTIFIER NULL,
    LastModificationTime DATETIME2 NULL,
    LastModifierId UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeleterId UNIQUEIDENTIFIER NULL,
    DeletionTime DATETIME2 NULL,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id),
    FOREIGN KEY (ParentId) REFERENCES AbpOrganizationUnits(Id),
    CONSTRAINT IX_AbpOrganizationUnits_TenantId_Code UNIQUE (TenantId, Code)
);

CREATE TABLE AbpUserOrganizationUnits (
    UserId UNIQUEIDENTIFIER NOT NULL,
    OrganizationUnitId UNIQUEIDENTIFIER NOT NULL,
    TenantId UNIQUEIDENTIFIER NULL,
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatorId UNIQUEIDENTIFIER NULL,
    
    PRIMARY KEY (UserId, OrganizationUnitId),
    FOREIGN KEY (UserId) REFERENCES AbpUsers(Id) ON DELETE CASCADE,
    FOREIGN KEY (OrganizationUnitId) REFERENCES AbpOrganizationUnits(Id) ON DELETE CASCADE
);
```

#### **Layer 12: Background Jobs**
```sql
CREATE TABLE AbpBackgroundJobs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    JobName NVARCHAR(128) NOT NULL,
    JobArgs NVARCHAR(MAX) NOT NULL,
    TryCount SMALLINT NOT NULL DEFAULT 1,
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    NextTryTime DATETIME2 NOT NULL,
    LastTryTime DATETIME2 NULL,
    IsAbandoned BIT NOT NULL DEFAULT 0,
    Priority TINYINT NOT NULL DEFAULT 15,
    ExtraProperties NVARCHAR(MAX) NULL
);

CREATE INDEX IX_AbpBackgroundJobs_NextTryTime ON AbpBackgroundJobs (NextTryTime);
CREATE INDEX IX_AbpBackgroundJobs_IsAbandoned_NextTryTime ON AbpBackgroundJobs (IsAbandoned, NextTryTime);
```

---

### **LAYERS 13-20: ONBOARDING CONTROL PLANE**
*Wizard State, Rules Evaluation, Explainability*

#### **Layer 13: Onboarding Wizard State**
```sql
CREATE TABLE OnboardingWizards (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Status INT NOT NULL DEFAULT 0, -- Pending=0, InProgress=1, Completed=2
    CurrentStepId NVARCHAR(50) NULL,
    NextStepId NVARCHAR(50) NULL,
    ResumeUrl NVARCHAR(500) NULL,
    LastActivityAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CompletionPercentage FLOAT NOT NULL DEFAULT 0.0,
    StartedAt DATETIME2 NULL,
    CompletedAt DATETIME2 NULL,
    MetaData NVARCHAR(MAX) NULL, -- JSON
    ExtraProperties NVARCHAR(MAX) NULL,
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatorId UNIQUEIDENTIFIER NULL,
    LastModificationTime DATETIME2 NULL,
    LastModifierId UNIQUEIDENTIFIER NULL,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE,
    CONSTRAINT IX_OnboardingWizards_TenantId UNIQUE (TenantId)
);

CREATE INDEX IX_OnboardingWizards_Status ON OnboardingWizards (Status);
CREATE INDEX IX_OnboardingWizards_LastActivity ON OnboardingWizards (LastActivityAt);
```

#### **Layer 14: Onboarding Answer Snapshots**
```sql
CREATE TABLE OnboardingAnswerSnapshots (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    StepId NVARCHAR(50) NOT NULL,
    SnapshotVersion INT NOT NULL,
    AnswersJson NVARCHAR(MAX) NOT NULL, -- Complete answers as JSON
    AnswersHash NVARCHAR(64) NOT NULL,  -- SHA256 hash for integrity
    IsActive BIT NOT NULL DEFAULT 1,
    ValidatedAt DATETIME2 NULL,
    ValidationResults NVARCHAR(MAX) NULL, -- JSON
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatorId UNIQUEIDENTIFIER NULL,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE,
    CONSTRAINT IX_OnboardingSnapshots_Tenant_Step_Version 
        UNIQUE (TenantId, StepId, SnapshotVersion)
);

CREATE INDEX IX_OnboardingSnapshots_TenantId ON OnboardingAnswerSnapshots (TenantId);
CREATE INDEX IX_OnboardingSnapshots_Hash ON OnboardingAnswerSnapshots (AnswersHash);
```

#### **Layer 15: Onboarding Derived Outputs**
```sql
CREATE TABLE OnboardingDerivedOutputs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    StepId NVARCHAR(50) NOT NULL,
    OutputType NVARCHAR(100) NOT NULL, -- baseline, overlays, scope, templates
    OutputData NVARCHAR(MAX) NOT NULL, -- JSON
    DependsOnSteps NVARCHAR(500) NULL, -- CSV of step IDs
    ComputedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ComputedBy NVARCHAR(100) NOT NULL, -- Agent/User/System
    IsValid BIT NOT NULL DEFAULT 1,
    ExpiresAt DATETIME2 NULL,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE,
    CONSTRAINT IX_OnboardingOutputs_Tenant_Step_Type 
        UNIQUE (TenantId, StepId, OutputType)
);

CREATE INDEX IX_OnboardingOutputs_TenantId ON OnboardingDerivedOutputs (TenantId);
CREATE INDEX IX_OnboardingOutputs_Type ON OnboardingDerivedOutputs (OutputType);
```

#### **Layer 16: Rules Evaluation Log**
```sql
CREATE TABLE RulesEvaluationLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    RuleSetId NVARCHAR(100) NOT NULL,
    RuleId NVARCHAR(100) NOT NULL,
    RuleVersion NVARCHAR(20) NOT NULL,
    InputsHash NVARCHAR(64) NOT NULL,
    InputsJson NVARCHAR(MAX) NOT NULL,
    OutputsJson NVARCHAR(MAX) NULL,
    MatchedConditions NVARCHAR(MAX) NULL, -- JSON array
    ConfidenceScore FLOAT NOT NULL DEFAULT 1.0,
    EvaluationTimeMs INT NOT NULL,
    EvaluatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    EvaluatedBy UNIQUEIDENTIFIER NULL,
    ContextInfo NVARCHAR(500) NULL,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE
);

CREATE INDEX IX_RulesEvaluationLogs_TenantId ON RulesEvaluationLogs (TenantId);
CREATE INDEX IX_RulesEvaluationLogs_RuleSet ON RulesEvaluationLogs (RuleSetId);
CREATE INDEX IX_RulesEvaluationLogs_EvaluatedAt ON RulesEvaluationLogs (EvaluatedAt);
```

#### **Layer 17: Explainability Payload**
```sql
CREATE TABLE ExplainabilityPayloads (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    DecisionId NVARCHAR(100) NOT NULL,
    DecisionType NVARCHAR(100) NOT NULL,
    HumanReadableExplanation NVARCHAR(MAX) NOT NULL,
    TechnicalExplanation NVARCHAR(MAX) NULL, -- JSON
    AlternativeOptions NVARCHAR(MAX) NULL, -- JSON array
    RecommendationRationale NVARCHAR(MAX) NULL,
    ConfidenceLevel FLOAT NOT NULL,
    RulesEvaluationLogId UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE,
    FOREIGN KEY (RulesEvaluationLogId) REFERENCES RulesEvaluationLogs(Id)
);

CREATE INDEX IX_ExplainabilityPayloads_TenantId ON ExplainabilityPayloads (TenantId);
CREATE INDEX IX_ExplainabilityPayloads_DecisionType ON ExplainabilityPayloads (DecisionType);
```

#### **Layer 18: Next Best Action State**
```sql
CREATE TABLE NextBestActionStates (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    CurrentContext NVARCHAR(100) NOT NULL, -- onboarding, assessment, remediation
    RecommendedActions NVARCHAR(MAX) NOT NULL, -- JSON array
    ActionPriorities NVARCHAR(MAX) NOT NULL, -- JSON
    ConfidenceScores NVARCHAR(MAX) NOT NULL, -- JSON
    DismissedActions NVARCHAR(MAX) NULL, -- JSON array
    LastUpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME2 NULL,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES AbpUsers(Id),
    CONSTRAINT IX_NextBestActions_Tenant_User UNIQUE (TenantId, UserId)
);
```

#### **Layer 19: Trial Runtime State**
```sql
CREATE TABLE TrialRuntimeStates (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    TrialStartDate DATETIME2 NOT NULL,
    TrialEndDate DATETIME2 NOT NULL,
    TierLevel INT NOT NULL DEFAULT 1,
    FeatureRestrictions NVARCHAR(MAX) NULL, -- JSON
    UsageLimits NVARCHAR(MAX) NOT NULL, -- JSON
    CurrentUsage NVARCHAR(MAX) NULL, -- JSON
    IsQuarantined BIT NOT NULL DEFAULT 0,
    QuarantineReason NVARCHAR(500) NULL,
    QuarantinedAt DATETIME2 NULL,
    LastActivityCheck DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE,
    CONSTRAINT IX_TrialRuntimeStates_TenantId UNIQUE (TenantId)
);

CREATE INDEX IX_TrialRuntimeStates_TrialEnd ON TrialRuntimeStates (TrialEndDate);
CREATE INDEX IX_TrialRuntimeStates_Quarantine ON TrialRuntimeStates (IsQuarantined);
```

#### **Layer 20: Redirect State**
```sql
CREATE TABLE RedirectStates (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    LastSafePath NVARCHAR(500) NULL,
    IntendedDestination NVARCHAR(500) NULL,
    RedirectReason NVARCHAR(100) NULL,
    RedirectCount INT NOT NULL DEFAULT 0,
    LastRedirectAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ResolvedAt DATETIME2 NULL,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES AbpUsers(Id),
    CONSTRAINT IX_RedirectStates_Tenant_User UNIQUE (TenantId, UserId)
);
```

---

### **LAYERS 21-30: COMPLIANCE CATALOG**
*Global Reference Data - Regulators, Frameworks, Controls*

#### **Layer 21: Regulators**
```sql
CREATE TABLE Regulators (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Code NVARCHAR(20) NOT NULL, -- NCA, SAMA, CBB, SFDA
    Name NVARCHAR(200) NOT NULL,
    FullName NVARCHAR(500) NOT NULL,
    Jurisdiction NVARCHAR(100) NOT NULL, -- Saudi Arabia, UAE, etc.
    Website NVARCHAR(300) NULL,
    ContactEmail NVARCHAR(256) NULL,
    ContactPhone NVARCHAR(50) NULL,
    ContactAddress NVARCHAR(MAX) NULL, -- JSON
    MandatorySectors NVARCHAR(MAX) NULL, -- JSON array
    IsActive BIT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 100,
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatorId UNIQUEIDENTIFIER NULL,
    LastModificationTime DATETIME2 NULL,
    LastModifierId UNIQUEIDENTIFIER NULL,
    
    CONSTRAINT IX_Regulators_Code UNIQUE (Code)
);

CREATE INDEX IX_Regulators_Jurisdiction ON Regulators (Jurisdiction);
CREATE INDEX IX_Regulators_IsActive ON Regulators (IsActive);
```

#### **Layer 22: Frameworks**
```sql
CREATE TABLE Frameworks (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RegulatorId UNIQUEIDENTIFIER NOT NULL,
    Code NVARCHAR(50) NOT NULL, -- NCA-ECC, SAMA-CSF, PDPL
    Name NVARCHAR(200) NOT NULL,
    FullName NVARCHAR(500) NOT NULL,
    Version NVARCHAR(20) NOT NULL,
    EffectiveDate DATE NOT NULL,
    ExpiryDate DATE NULL,
    MandatorySectors NVARCHAR(MAX) NULL, -- JSON array
    ApplicableCountries NVARCHAR(MAX) NULL, -- JSON array
    FrameworkType INT NOT NULL DEFAULT 1, -- Regulatory=1, Standard=2, Best Practice=3
    Description NVARCHAR(MAX) NULL,
    ImplementationGuidance NVARCHAR(MAX) NULL,
    DocumentUrl NVARCHAR(300) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 100,
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatorId UNIQUEIDENTIFIER NULL,
    LastModificationTime DATETIME2 NULL,
    LastModifierId UNIQUEIDENTIFIER NULL,
    
    FOREIGN KEY (RegulatorId) REFERENCES Regulators(Id),
    CONSTRAINT IX_Frameworks_Code_Version UNIQUE (Code, Version)
);

CREATE INDEX IX_Frameworks_RegulatorId ON Frameworks (RegulatorId);
CREATE INDEX IX_Frameworks_EffectiveDate ON Frameworks (EffectiveDate);
CREATE INDEX IX_Frameworks_IsActive ON Frameworks (IsActive);
```

#### **Layer 23: Framework Versions**
```sql
CREATE TABLE FrameworkVersions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    FrameworkId UNIQUEIDENTIFIER NOT NULL,
    Version NVARCHAR(20) NOT NULL,
    ReleaseDate DATE NOT NULL,
    EffectiveDate DATE NOT NULL,
    ExpiryDate DATE NULL,
    ChangeDescription NVARCHAR(MAX) NULL,
    MigrationGuidance NVARCHAR(MAX) NULL,
    IsCurrent BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (FrameworkId) REFERENCES Frameworks(Id) ON DELETE CASCADE,
    CONSTRAINT IX_FrameworkVersions_Framework_Version UNIQUE (FrameworkId, Version)
);

CREATE INDEX IX_FrameworkVersions_IsCurrent ON FrameworkVersions (IsCurrent);
```

#### **Layer 24: Controls**
```sql
CREATE TABLE Controls (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    FrameworkId UNIQUEIDENTIFIER NOT NULL,
    ControlId NVARCHAR(50) NOT NULL, -- AC-1, SC-7, etc.
    Title NVARCHAR(300) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    ImplementationGuidance NVARCHAR(MAX) NULL,
    Domains NVARCHAR(500) NULL, -- JSON array: Access, Security, Risk
    Criticality INT NOT NULL DEFAULT 2, -- Low=1, Medium=2, High=3, Critical=4
    ControlFamily NVARCHAR(100) NULL,
    ControlType INT NOT NULL DEFAULT 1, -- Preventive=1, Detective=2, Corrective=3
    IsOptional BIT NOT NULL DEFAULT 0,
    EstimatedEffortHours INT NULL,
    Tags NVARCHAR(MAX) NULL, -- JSON array
    References NVARCHAR(MAX) NULL, -- JSON array of related standards
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatorId UNIQUEIDENTIFIER NULL,
    LastModificationTime DATETIME2 NULL,
    LastModifierId UNIQUEIDENTIFIER NULL,
    
    FOREIGN KEY (FrameworkId) REFERENCES Frameworks(Id) ON DELETE CASCADE,
    CONSTRAINT IX_Controls_Framework_ControlId UNIQUE (FrameworkId, ControlId)
);

CREATE INDEX IX_Controls_FrameworkId ON Controls (FrameworkId);
CREATE INDEX IX_Controls_Criticality ON Controls (Criticality);
CREATE INDEX IX_Controls_ControlFamily ON Controls (ControlFamily);
```

#### **Layer 25: Control Domains**
```sql
CREATE TABLE ControlDomains (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Category NVARCHAR(100) NULL, -- Technical, Administrative, Physical
    DisplayOrder INT NOT NULL DEFAULT 100,
    IsActive BIT NOT NULL DEFAULT 1,
    
    CONSTRAINT IX_ControlDomains_Name UNIQUE (Name)
);

-- Pre-populate common domains
INSERT INTO ControlDomains (Name, Description, Category, DisplayOrder) VALUES
('Access Control', 'User access management and authentication', 'Technical', 10),
('Security Assessment', 'Security testing and vulnerability management', 'Technical', 20),
('Risk Management', 'Risk identification and mitigation', 'Administrative', 30),
('Incident Response', 'Security incident handling and recovery', 'Administrative', 40),
('Physical Security', 'Physical protection of assets', 'Physical', 50),
('Data Protection', 'Data classification and protection', 'Technical', 60),
('Business Continuity', 'Continuity and disaster recovery planning', 'Administrative', 70),
('Vendor Management', 'Third-party risk management', 'Administrative', 80),
('Training & Awareness', 'Security education and awareness', 'Administrative', 90),
('Compliance Monitoring', 'Compliance tracking and reporting', 'Administrative', 100);
```

#### **Layer 26: Control Tags**
```sql
CREATE TABLE ControlTags (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Category NVARCHAR(50) NULL, -- Technology, Process, Risk Level
    Description NVARCHAR(300) NULL,
    Color NVARCHAR(7) NULL, -- Hex color code
    IsActive BIT NOT NULL DEFAULT 1,
    
    CONSTRAINT IX_ControlTags_Name UNIQUE (Name)
);

CREATE TABLE ControlTagMappings (
    ControlId UNIQUEIDENTIFIER NOT NULL,
    TagId UNIQUEIDENTIFIER NOT NULL,
    
    PRIMARY KEY (ControlId, TagId),
    FOREIGN KEY (ControlId) REFERENCES Controls(Id) ON DELETE CASCADE,
    FOREIGN KEY (TagId) REFERENCES ControlTags(Id) ON DELETE CASCADE
);
```

#### **Layer 27: Crosswalk Mappings**
```sql
CREATE TABLE ControlCrosswalks (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SourceControlId UNIQUEIDENTIFIER NOT NULL,
    TargetControlId UNIQUEIDENTIFIER NOT NULL,
    MappingType INT NOT NULL DEFAULT 1, -- Equivalent=1, Subset=2, Overlap=3, Related=4
    ConfidenceScore FLOAT NOT NULL DEFAULT 1.0,
    MappingRationale NVARCHAR(MAX) NULL,
    CreatedBySystem BIT NOT NULL DEFAULT 0,
    ValidatedBy UNIQUEIDENTIFIER NULL,
    ValidatedAt DATETIME2 NULL,
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatorId UNIQUEIDENTIFIER NULL,
    
    FOREIGN KEY (SourceControlId) REFERENCES Controls(Id),
    FOREIGN KEY (TargetControlId) REFERENCES Controls(Id),
    FOREIGN KEY (ValidatedBy) REFERENCES AbpUsers(Id),
    CONSTRAINT IX_ControlCrosswalks_Source_Target UNIQUE (SourceControlId, TargetControlId)
);

CREATE INDEX IX_ControlCrosswalks_MappingType ON ControlCrosswalks (MappingType);
CREATE INDEX IX_ControlCrosswalks_Confidence ON ControlCrosswalks (ConfidenceScore);
```

#### **Layer 28: Overlay Definitions**
```sql
CREATE TABLE OverlayDefinitions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    OverlayId NVARCHAR(50) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    TriggerConditions NVARCHAR(MAX) NOT NULL, -- JSON array of conditions
    AdditionalFrameworks NVARCHAR(MAX) NULL, -- JSON array of framework IDs
    ModificationRules NVARCHAR(MAX) NULL, -- JSON rules for control modifications
    Priority INT NOT NULL DEFAULT 100,
    IsActive BIT NOT NULL DEFAULT 1,
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT IX_OverlayDefinitions_OverlayId UNIQUE (OverlayId)
);

-- Example overlays
INSERT INTO OverlayDefinitions (OverlayId, Name, Description, TriggerConditions) VALUES
('HEALTHCARE_OVERLAY', 'Healthcare Data Overlay', 'Additional controls for healthcare data', 
 '[{"field": "dataTypes", "operator": "contains", "value": "health"}]'),
('INTERNATIONAL_OVERLAY', 'International Data Transfer Overlay', 'Controls for cross-border data transfers',
 '[{"field": "dataTransfers", "operator": "contains", "value": "international"}]'),
('HIGH_RISK_OVERLAY', 'High Risk Organization Overlay', 'Enhanced controls for high-risk organizations',
 '[{"field": "riskLevel", "operator": "equals", "value": "high"}]');
```

#### **Layer 29: Applicability Rule Definitions**
```sql
CREATE TABLE ApplicabilityRules (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RuleId NVARCHAR(100) NOT NULL,
    RuleName NVARCHAR(200) NOT NULL,
    FrameworkId UNIQUEIDENTIFIER NULL, -- NULL = applies to multiple frameworks
    RuleType INT NOT NULL DEFAULT 1, -- Framework=1, Overlay=2, Exclusion=3
    Conditions NVARCHAR(MAX) NOT NULL, -- JSON rule conditions
    Actions NVARCHAR(MAX) NOT NULL, -- JSON actions to take if conditions match
    Priority INT NOT NULL DEFAULT 100,
    IsActive BIT NOT NULL DEFAULT 1,
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatorId UNIQUEIDENTIFIER NULL,
    
    FOREIGN KEY (FrameworkId) REFERENCES Frameworks(Id),
    CONSTRAINT IX_ApplicabilityRules_RuleId UNIQUE (RuleId)
);

CREATE INDEX IX_ApplicabilityRules_FrameworkId ON ApplicabilityRules (FrameworkId);
CREATE INDEX IX_ApplicabilityRules_RuleType ON ApplicabilityRules (RuleType);
```

#### **Layer 30: Evidence Requirement Definitions**
```sql
CREATE TABLE EvidenceRequirements (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ControlId UNIQUEIDENTIFIER NULL, -- NULL = applies to multiple controls
    EvidenceType INT NOT NULL, -- Document=1, Screenshot=2, Configuration=3, Report=4
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    IsMandatory BIT NOT NULL DEFAULT 1,
    Frequency NVARCHAR(50) NOT NULL DEFAULT 'Annual', -- Annual, Quarterly, Monthly, Continuous
    AcceptableFormats NVARCHAR(300) NULL, -- JSON array: ["pdf", "docx", "xlsx"]
    MaxFileSize INT NOT NULL DEFAULT 10485760, -- 10MB in bytes
    RetentionPeriodYears INT NOT NULL DEFAULT 7,
    ValidationRules NVARCHAR(MAX) NULL, -- JSON validation criteria
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (ControlId) REFERENCES Controls(Id)
);

CREATE INDEX IX_EvidenceRequirements_ControlId ON EvidenceRequirements (ControlId);
CREATE INDEX IX_EvidenceRequirements_EvidenceType ON EvidenceRequirements (EvidenceType);
```

---

### **LAYERS 31-36: TENANT COMPLIANCE RESOLUTION**
*Tenant-specific Applied Compliance State*

#### **Layer 31: Tenant Framework Selection**
```sql
CREATE TABLE TenantFrameworkSelections (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    FrameworkId UNIQUEIDENTIFIER NOT NULL,
    SelectionReason NVARCHAR(MAX) NOT NULL, -- JSON explaining why selected
    IsMandatory BIT NOT NULL DEFAULT 0,
    IsSelected BIT NOT NULL DEFAULT 1,
    AppliedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    AppliedBy UNIQUEIDENTIFIER NOT NULL,
    RulesEvaluationLogId UNIQUEIDENTIFIER NULL,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE,
    FOREIGN KEY (FrameworkId) REFERENCES Frameworks(Id),
    FOREIGN KEY (AppliedBy) REFERENCES AbpUsers(Id),
    FOREIGN KEY (RulesEvaluationLogId) REFERENCES RulesEvaluationLogs(Id),
    CONSTRAINT IX_TenantFrameworks_Tenant_Framework UNIQUE (TenantId, FrameworkId)
);

CREATE INDEX IX_TenantFrameworkSelections_TenantId ON TenantFrameworkSelections (TenantId);
```

#### **Layer 32: Tenant Baseline**
```sql
CREATE TABLE TenantBaselines (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    BaselineVersion NVARCHAR(20) NOT NULL DEFAULT '1.0',
    FrameworkSelections NVARCHAR(MAX) NOT NULL, -- JSON array of selected frameworks
    BaselineControls NVARCHAR(MAX) NOT NULL, -- JSON array of baseline control IDs
    ControlCount INT NOT NULL DEFAULT 0,
    EstimatedEffortHours INT NULL,
    ComplexityLevel INT NOT NULL DEFAULT 2, -- Low=1, Medium=2, High=3, Critical=4
    GeneratedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    GeneratedBy UNIQUEIDENTIFIER NOT NULL,
    ApprovedAt DATETIME2 NULL,
    ApprovedBy UNIQUEIDENTIFIER NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE,
    FOREIGN KEY (GeneratedBy) REFERENCES AbpUsers(Id),
    FOREIGN KEY (ApprovedBy) REFERENCES AbpUsers(Id),
    CONSTRAINT IX_TenantBaselines_Tenant_Version UNIQUE (TenantId, BaselineVersion)
);

CREATE INDEX IX_TenantBaselines_TenantId ON TenantBaselines (TenantId);
```

#### **Layer 33: Tenant Overlays**
```sql
CREATE TABLE TenantOverlays (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    OverlayId NVARCHAR(50) NOT NULL,
    OverlayName NVARCHAR(200) NOT NULL,
    TriggerReason NVARCHAR(MAX) NOT NULL, -- Why this overlay was applied
    AdditionalControls NVARCHAR(MAX) NULL, -- JSON array of additional control IDs
    ModifiedControls NVARCHAR(MAX) NULL, -- JSON of control modifications
    AppliedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    AppliedBy UNIQUEIDENTIFIER NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE,
    FOREIGN KEY (AppliedBy) REFERENCES AbpUsers(Id),
    CONSTRAINT IX_TenantOverlays_Tenant_Overlay UNIQUE (TenantId, OverlayId)
);

CREATE INDEX IX_TenantOverlays_TenantId ON TenantOverlays (TenantId);
```

#### **Layer 34: Tenant Control Set (Final Resolved)**
```sql
CREATE TABLE TenantControlSets (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    ControlId UNIQUEIDENTIFIER NOT NULL,
    FrameworkId UNIQUEIDENTIFIER NOT NULL,
    ControlCode NVARCHAR(50) NOT NULL,
    IsRequired BIT NOT NULL DEFAULT 1,
    IsOptional BIT NOT NULL DEFAULT 0,
    Criticality INT NOT NULL DEFAULT 2,
    EstimatedEffortHours INT NULL,
    InclusionReason NVARCHAR(MAX) NOT NULL, -- Why this control is included
    AppliedOverlays NVARCHAR(MAX) NULL, -- JSON array of overlays that affected this control
    CustomizationRules NVARCHAR(MAX) NULL, -- JSON tenant-specific customizations
    ResolvedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ResolvedBy UNIQUEIDENTIFIER NOT NULL,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE,
    FOREIGN KEY (ControlId) REFERENCES Controls(Id),
    FOREIGN KEY (FrameworkId) REFERENCES Frameworks(Id),
    FOREIGN KEY (ResolvedBy) REFERENCES AbpUsers(Id),
    CONSTRAINT IX_TenantControlSets_Tenant_Control UNIQUE (TenantId, ControlId)
);

CREATE INDEX IX_TenantControlSets_TenantId ON TenantControlSets (TenantId);
CREATE INDEX IX_TenantControlSets_FrameworkId ON TenantControlSets (FrameworkId);
CREATE INDEX IX_TenantControlSets_Criticality ON TenantControlSets (Criticality);
```

#### **Layer 35: Tenant Scope Boundary**
```sql
CREATE TABLE TenantScopeBoundaries (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    EntityType INT NOT NULL, -- Organization=1, System=2, Process=3, Location=4, Asset=5
    EntityId NVARCHAR(100) NOT NULL,
    EntityName NVARCHAR(200) NOT NULL,
    IsInScope BIT NOT NULL DEFAULT 1,
    CriticalityLevel INT NOT NULL DEFAULT 2, -- Low=1, Medium=2, High=3, Critical=4
    ScopeRationale NVARCHAR(MAX) NULL,
    Tags NVARCHAR(MAX) NULL, -- JSON array
    Properties NVARCHAR(MAX) NULL, -- JSON object with entity-specific properties
    DefinedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    DefinedBy UNIQUEIDENTIFIER NOT NULL,
    LastReviewedAt DATETIME2 NULL,
    LastReviewedBy UNIQUEIDENTIFIER NULL,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE,
    FOREIGN KEY (DefinedBy) REFERENCES AbpUsers(Id),
    FOREIGN KEY (LastReviewedBy) REFERENCES AbpUsers(Id),
    CONSTRAINT IX_TenantScope_Tenant_Entity UNIQUE (TenantId, EntityType, EntityId)
);

CREATE INDEX IX_TenantScopeBoundaries_TenantId ON TenantScopeBoundaries (TenantId);
CREATE INDEX IX_TenantScopeBoundaries_EntityType ON TenantScopeBoundaries (EntityType);
CREATE INDEX IX_TenantScopeBoundaries_IsInScope ON TenantScopeBoundaries (IsInScope);
```

#### **Layer 36: Tenant Risk Profile**
```sql
CREATE TABLE TenantRiskProfiles (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    DataTypes NVARCHAR(MAX) NOT NULL, -- JSON array: PII, Financial, Health, etc.
    DataTransfers NVARCHAR(MAX) NULL, -- JSON array: domestic, international
    ProcessingActivities NVARCHAR(MAX) NULL, -- JSON array: collection, storage, processing
    RegulatoryEnvironment NVARCHAR(MAX) NOT NULL, -- JSON: applicable regulations
    ThreatLandscape NVARCHAR(MAX) NULL, -- JSON: identified threats and risks
    BusinessCriticality INT NOT NULL DEFAULT 2, -- Low=1, Medium=2, High=3, Critical=4
    ComplianceMaturity INT NOT NULL DEFAULT 1, -- Initial=1, Developing=2, Defined=3, Managed=4, Optimized=5
    TotalRiskScore FLOAT NOT NULL DEFAULT 50.0, -- 0-100 scale
    LastAssessmentDate DATETIME2 NULL,
    NextAssessmentDue DATETIME2 NULL,
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatorId UNIQUEIDENTIFIER NULL,
    LastModificationTime DATETIME2 NULL,
    LastModifierId UNIQUEIDENTIFIER NULL,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE,
    CONSTRAINT IX_TenantRiskProfiles_TenantId UNIQUE (TenantId)
);

CREATE INDEX IX_TenantRiskProfiles_BusinessCriticality ON TenantRiskProfiles (BusinessCriticality);
CREATE INDEX IX_TenantRiskProfiles_RiskScore ON TenantRiskProfiles (TotalRiskScore);
```

---

### **LAYERS 37-43: EXECUTION LAYERS**
*Operational GRC - Plans, Assessments, Evidence, Workflows*

#### **Layer 37: GRC Plans**
```sql
CREATE TABLE GrcPlans (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    PlanName NVARCHAR(200) NOT NULL,
    PlanType NVARCHAR(50) NOT NULL, -- QuickScan, Comprehensive, Continuous, Custom
    Description NVARCHAR(MAX) NULL,
    FrameworkIds NVARCHAR(MAX) NOT NULL, -- JSON array of framework IDs
    StartDate DATE NOT NULL,
    TargetCompletionDate DATE NOT NULL,
    ActualCompletionDate DATE NULL,
    Status INT NOT NULL DEFAULT 1, -- Draft=1, Active=2, OnHold=3, Completed=4, Cancelled=5
    Progress FLOAT NOT NULL DEFAULT 0.0, -- 0-100 percentage
    EstimatedEffortHours INT NULL,
    ActualEffortHours INT NULL,
    Budget DECIMAL(18,2) NULL,
    ActualCost DECIMAL(18,2) NULL,
    PlanMetadata NVARCHAR(MAX) NULL, -- JSON: milestones, phases, etc.
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatorId UNIQUEIDENTIFIER NULL,
    LastModificationTime DATETIME2 NULL,
    LastModifierId UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeleterId UNIQUEIDENTIFIER NULL,
    DeletionTime DATETIME2 NULL,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE,
    FOREIGN KEY (CreatorId) REFERENCES AbpUsers(Id),
    FOREIGN KEY (LastModifierId) REFERENCES AbpUsers(Id),
    FOREIGN KEY (DeleterId) REFERENCES AbpUsers(Id)
);

CREATE INDEX IX_GrcPlans_TenantId ON GrcPlans (TenantId);
CREATE INDEX IX_GrcPlans_Status ON GrcPlans (Status);
CREATE INDEX IX_GrcPlans_TargetCompletion ON GrcPlans (TargetCompletionDate);
```

#### **Layer 38: Assessment Instances**
```sql
CREATE TABLE Assessments (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    PlanId UNIQUEIDENTIFIER NULL,
    FrameworkId UNIQUEIDENTIFIER NOT NULL,
    AssessmentName NVARCHAR(200) NOT NULL,
    AssessmentType NVARCHAR(50) NOT NULL, -- SelfAssessment, ExternalAudit, Automated, Continuous
    Description NVARCHAR(MAX) NULL,
    StartDate DATE NOT NULL,
    DueDate DATE NOT NULL,
    CompletionDate DATE NULL,
    Status INT NOT NULL DEFAULT 1, -- NotStarted=1, InProgress=2, UnderReview=3, Completed=4, Overdue=5
    OverallScore FLOAT NULL, -- 0-100 compliance score
    TotalControls INT NOT NULL DEFAULT 0,
    CompliantControls INT NOT NULL DEFAULT 0,
    NonCompliantControls INT NOT NULL DEFAULT 0,
    NotAssessedControls INT NOT NULL DEFAULT 0,
    AssessorUserId UNIQUEIDENTIFIER NULL,
    ReviewerUserId UNIQUEIDENTIFIER NULL,
    AssessmentMetadata NVARCHAR(MAX) NULL, -- JSON: methodology, criteria, etc.
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatorId UNIQUEIDENTIFIER NULL,
    LastModificationTime DATETIME2 NULL,
    LastModifierId UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeleterId UNIQUEIDENTIFIER NULL,
    DeletionTime DATETIME2 NULL,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE,
    FOREIGN KEY (PlanId) REFERENCES GrcPlans(Id),
    FOREIGN KEY (FrameworkId) REFERENCES Frameworks(Id),
    FOREIGN KEY (AssessorUserId) REFERENCES AbpUsers(Id),
    FOREIGN KEY (ReviewerUserId) REFERENCES AbpUsers(Id)
);

CREATE INDEX IX_Assessments_TenantId ON Assessments (TenantId);
CREATE INDEX IX_Assessments_PlanId ON Assessments (PlanId);
CREATE INDEX IX_Assessments_Status ON Assessments (Status);
CREATE INDEX IX_Assessments_DueDate ON Assessments (DueDate);
```

#### **Layer 39: Evidence Management**
```sql
CREATE TABLE EvidenceRequests (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    AssessmentId UNIQUEIDENTIFIER NOT NULL,
    ControlId UNIQUEIDENTIFIER NOT NULL,
    EvidenceType INT NOT NULL, -- Document=1, Screenshot=2, Configuration=3, Report=4
    Title NVARCHAR(300) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    RequesterUserId UNIQUEIDENTIFIER NOT NULL,
    AssignedToUserId UNIQUEIDENTIFIER NULL,
    RequestedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    DueDate DATETIME2 NOT NULL,
    Status INT NOT NULL DEFAULT 1, -- Requested=1, InProgress=2, Submitted=3, UnderReview=4, Approved=5, Rejected=6
    Priority INT NOT NULL DEFAULT 2, -- Low=1, Medium=2, High=3, Critical=4
    ValidationCriteria NVARCHAR(MAX) NULL, -- JSON validation rules
    SubmissionInstructions NVARCHAR(MAX) NULL,
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatorId UNIQUEIDENTIFIER NULL,
    LastModificationTime DATETIME2 NULL,
    LastModifierId UNIQUEIDENTIFIER NULL,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE,
    FOREIGN KEY (AssessmentId) REFERENCES Assessments(Id),
    FOREIGN KEY (ControlId) REFERENCES Controls(Id),
    FOREIGN KEY (RequesterUserId) REFERENCES AbpUsers(Id),
    FOREIGN KEY (AssignedToUserId) REFERENCES AbpUsers(Id)
);

CREATE TABLE EvidenceSubmissions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    EvidenceRequestId UNIQUEIDENTIFIER NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    FileUrl NVARCHAR(500) NOT NULL,
    FileSize BIGINT NOT NULL,
    FileHash NVARCHAR(128) NOT NULL, -- SHA256
    MimeType NVARCHAR(100) NULL,
    SubmittedBy UNIQUEIDENTIFIER NOT NULL,
    SubmittedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Comments NVARCHAR(MAX) NULL,
    ValidationStatus INT NOT NULL DEFAULT 1, -- Pending=1, Valid=2, Invalid=3, RequiresReview=4
    ValidationComments NVARCHAR(MAX) NULL,
    ValidatedBy UNIQUEIDENTIFIER NULL,
    ValidatedAt DATETIME2 NULL,
    RetentionPolicyId UNIQUEIDENTIFIER NULL,
    ExpiryDate DATETIME2 NULL,
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE,
    FOREIGN KEY (EvidenceRequestId) REFERENCES EvidenceRequests(Id),
    FOREIGN KEY (SubmittedBy) REFERENCES AbpUsers(Id),
    FOREIGN KEY (ValidatedBy) REFERENCES AbpUsers(Id)
);

CREATE INDEX IX_EvidenceRequests_TenantId ON EvidenceRequests (TenantId);
CREATE INDEX IX_EvidenceRequests_AssessmentId ON EvidenceRequests (AssessmentId);
CREATE INDEX IX_EvidenceRequests_Status ON EvidenceRequests (Status);
CREATE INDEX IX_EvidenceRequests_DueDate ON EvidenceRequests (DueDate);

CREATE INDEX IX_EvidenceSubmissions_TenantId ON EvidenceSubmissions (TenantId);
CREATE INDEX IX_EvidenceSubmissions_RequestId ON EvidenceSubmissions (EvidenceRequestId);
CREATE INDEX IX_EvidenceSubmissions_FileHash ON EvidenceSubmissions (FileHash);
```

#### **Layer 40: Findings & Issues**
```sql
CREATE TABLE Findings (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    AssessmentId UNIQUEIDENTIFIER NOT NULL,
    ControlId UNIQUEIDENTIFIER NOT NULL,
    FindingNumber NVARCHAR(50) NULL, -- Auto-generated: FIND-2024-001
    Title NVARCHAR(300) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    FindingType INT NOT NULL DEFAULT 1, -- Gap=1, Weakness=2, Deficiency=3, Observation=4
    Severity INT NOT NULL DEFAULT 2, -- Low=1, Medium=2, High=3, Critical=4
    RiskRating INT NOT NULL DEFAULT 2, -- Low=1, Medium=2, High=3, Critical=4
    RiskStatement NVARCHAR(MAX) NULL,
    BusinessImpact NVARCHAR(MAX) NULL,
    TechnicalImpact NVARCHAR(MAX) NULL,
    RecommendedActions NVARCHAR(MAX) NULL,
    IdentifiedBy UNIQUEIDENTIFIER NOT NULL,
    IdentifiedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Status INT NOT NULL DEFAULT 1, -- Open=1, InProgress=2, Resolved=3, Accepted=4, Closed=5
    TargetResolutionDate DATE NULL,
    ActualResolutionDate DATE NULL,
    ResolutionNotes NVARCHAR(MAX) NULL,
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatorId UNIQUEIDENTIFIER NULL,
    LastModificationTime DATETIME2 NULL,
    LastModifierId UNIQUEIDENTIFIER NULL,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE,
    FOREIGN KEY (AssessmentId) REFERENCES Assessments(Id),
    FOREIGN KEY (ControlId) REFERENCES Controls(Id),
    FOREIGN KEY (IdentifiedBy) REFERENCES AbpUsers(Id)
);

CREATE INDEX IX_Findings_TenantId ON Findings (TenantId);
CREATE INDEX IX_Findings_AssessmentId ON Findings (AssessmentId);
CREATE INDEX IX_Findings_Status ON Findings (Status);
CREATE INDEX IX_Findings_Severity ON Findings (Severity);
CREATE INDEX IX_Findings_TargetResolution ON Findings (TargetResolutionDate);
```

#### **Layer 41: Remediation & Tasks**
```sql
CREATE TABLE RemediationActions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    FindingId UNIQUEIDENTIFIER NULL, -- Can be independent of findings
    ActionTitle NVARCHAR(300) NOT NULL,
    ActionDescription NVARCHAR(MAX) NOT NULL,
    ActionType INT NOT NULL DEFAULT 1, -- Corrective=1, Preventive=2, Detective=3
    Priority INT NOT NULL DEFAULT 2, -- Low=1, Medium=2, High=3, Critical=4
    AssignedToUserId UNIQUEIDENTIFIER NOT NULL,
    DueDate DATE NOT NULL,
    EstimatedEffortHours INT NULL,
    EstimatedCost DECIMAL(18,2) NULL,
    ActualEffortHours INT NULL,
    ActualCost DECIMAL(18,2) NULL,
    Status INT NOT NULL DEFAULT 1, -- NotStarted=1, InProgress=2, Blocked=3, Completed=4, Cancelled=5
    ProgressPercentage FLOAT NOT NULL DEFAULT 0.0,
    Dependencies NVARCHAR(MAX) NULL, -- JSON array of dependent action IDs
    ProgressNotes NVARCHAR(MAX) NULL,
    CompletedAt DATETIME2 NULL,
    CompletedBy UNIQUEIDENTIFIER NULL,
    ApprovalRequired BIT NOT NULL DEFAULT 0,
    ApprovedBy UNIQUEIDENTIFIER NULL,
    ApprovedAt DATETIME2 NULL,
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatorId UNIQUEIDENTIFIER NULL,
    LastModificationTime DATETIME2 NULL,
    LastModifierId UNIQUEIDENTIFIER NULL,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE,
    FOREIGN KEY (FindingId) REFERENCES Findings(Id),
    FOREIGN KEY (AssignedToUserId) REFERENCES AbpUsers(Id),
    FOREIGN KEY (CompletedBy) REFERENCES AbpUsers(Id),
    FOREIGN KEY (ApprovedBy) REFERENCES AbpUsers(Id)
);

CREATE INDEX IX_RemediationActions_TenantId ON RemediationActions (TenantId);
CREATE INDEX IX_RemediationActions_FindingId ON RemediationActions (FindingId);
CREATE INDEX IX_RemediationActions_AssignedTo ON RemediationActions (AssignedToUserId);
CREATE INDEX IX_RemediationActions_Status ON RemediationActions (Status);
CREATE INDEX IX_RemediationActions_DueDate ON RemediationActions (DueDate);
```

#### **Layer 42: Workflow Runtime**
```sql
CREATE TABLE WorkflowInstances (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    WorkflowDefinitionId NVARCHAR(100) NOT NULL,
    WorkflowName NVARCHAR(200) NOT NULL,
    EntityType NVARCHAR(100) NOT NULL, -- Assessment, Finding, Remediation
    EntityId UNIQUEIDENTIFIER NOT NULL,
    Status INT NOT NULL DEFAULT 1, -- Running=1, Completed=2, Failed=3, Cancelled=4, Suspended=5
    CurrentStepId NVARCHAR(100) NULL,
    StartedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CompletedAt DATETIME2 NULL,
    Variables NVARCHAR(MAX) NULL, -- JSON workflow variables
    ErrorMessage NVARCHAR(MAX) NULL,
    StartedBy UNIQUEIDENTIFIER NULL,
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE,
    FOREIGN KEY (StartedBy) REFERENCES AbpUsers(Id)
);

CREATE TABLE WorkflowSteps (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    WorkflowInstanceId UNIQUEIDENTIFIER NOT NULL,
    StepId NVARCHAR(100) NOT NULL,
    StepName NVARCHAR(200) NOT NULL,
    StepType NVARCHAR(50) NOT NULL, -- Task, Approval, Notification, System
    Status INT NOT NULL DEFAULT 1, -- Pending=1, InProgress=2, Completed=3, Failed=4, Skipped=5
    AssignedToUserId UNIQUEIDENTIFIER NULL,
    StartedAt DATETIME2 NULL,
    CompletedAt DATETIME2 NULL,
    DueDate DATETIME2 NULL,
    StepData NVARCHAR(MAX) NULL, -- JSON step-specific data
    OutputData NVARCHAR(MAX) NULL, -- JSON step outputs
    ErrorMessage NVARCHAR(MAX) NULL,
    
    FOREIGN KEY (WorkflowInstanceId) REFERENCES WorkflowInstances(Id) ON DELETE CASCADE,
    FOREIGN KEY (AssignedToUserId) REFERENCES AbpUsers(Id)
);

CREATE INDEX IX_WorkflowInstances_TenantId ON WorkflowInstances (TenantId);
CREATE INDEX IX_WorkflowInstances_Status ON WorkflowInstances (Status);
CREATE INDEX IX_WorkflowInstances_EntityType ON WorkflowInstances (EntityType);

CREATE INDEX IX_WorkflowSteps_WorkflowInstanceId ON WorkflowSteps (WorkflowInstanceId);
CREATE INDEX IX_WorkflowSteps_AssignedTo ON WorkflowSteps (AssignedToUserId);
CREATE INDEX IX_WorkflowSteps_Status ON WorkflowSteps (Status);
```

#### **Layer 43: Analytics & KPI Projections**
```sql
CREATE TABLE AnalyticsSnapshots (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    SnapshotDate DATE NOT NULL,
    SnapshotType NVARCHAR(50) NOT NULL, -- Daily, Weekly, Monthly, Quarterly
    MetricsData NVARCHAR(MAX) NOT NULL, -- JSON of all metrics
    ComplianceScores NVARCHAR(MAX) NULL, -- JSON of framework scores
    TrendData NVARCHAR(MAX) NULL, -- JSON of trend calculations
    BenchmarkData NVARCHAR(MAX) NULL, -- JSON of benchmark comparisons
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE,
    CONSTRAINT IX_AnalyticsSnapshots_Tenant_Date_Type UNIQUE (TenantId, SnapshotDate, SnapshotType)
);

CREATE TABLE KpiDefinitions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    KpiCode NVARCHAR(50) NOT NULL,
    KpiName NVARCHAR(200) NOT NULL,
    Category NVARCHAR(50) NOT NULL, -- Compliance, Risk, Efficiency, Quality
    Description NVARCHAR(MAX) NULL,
    CalculationFormula NVARCHAR(MAX) NOT NULL, -- JSON calculation definition
    TargetValue DECIMAL(18,4) NULL,
    ThresholdGreen DECIMAL(18,4) NULL,
    ThresholdYellow DECIMAL(18,4) NULL,
    ThresholdRed DECIMAL(18,4) NULL,
    Unit NVARCHAR(20) NULL, -- %, count, hours, $
    DataSources NVARCHAR(MAX) NULL, -- JSON array of data sources
    CalculationFrequency NVARCHAR(20) NOT NULL DEFAULT 'Daily',
    IsActive BIT NOT NULL DEFAULT 1,
    CreationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE,
    CONSTRAINT IX_KpiDefinitions_Tenant_Code UNIQUE (TenantId, KpiCode)
);

CREATE TABLE KpiValues (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    KpiDefinitionId UNIQUEIDENTIFIER NOT NULL,
    ValueDate DATE NOT NULL,
    Value DECIMAL(18,4) NOT NULL,
    PreviousValue DECIMAL(18,4) NULL,
    PercentageChange DECIMAL(18,4) NULL,
    Status INT NOT NULL DEFAULT 1, -- Green=1, Yellow=2, Red=3
    CalculatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CalculationInputs NVARCHAR(MAX) NULL, -- JSON of inputs used
    
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE,
    FOREIGN KEY (KpiDefinitionId) REFERENCES KpiDefinitions(Id) ON DELETE CASCADE,
    CONSTRAINT IX_KpiValues_Tenant_Kpi_Date UNIQUE (TenantId, KpiDefinitionId, ValueDate)
);

CREATE INDEX IX_AnalyticsSnapshots_TenantId ON AnalyticsSnapshots (TenantId);
CREATE INDEX IX_AnalyticsSnapshots_SnapshotDate ON AnalyticsSnapshots (SnapshotDate);

CREATE INDEX IX_KpiDefinitions_TenantId ON KpiDefinitions (TenantId);
CREATE INDEX IX_KpiDefinitions_Category ON KpiDefinitions (Category);

CREATE INDEX IX_KpiValues_TenantId ON KpiValues (TenantId);
CREATE INDEX IX_KpiValues_ValueDate ON KpiValues (ValueDate);
CREATE INDEX IX_KpiValues_Status ON KpiValues (Status);
```

---

## Entity Relationships

### **Core Relationship Patterns**

1. **Tenant-Centric Design**: All tenant data tables have `TenantId` FK to `AbpTenants`
2. **User Auditing**: All business tables include `CreatorId`, `LastModifierId` for audit trail
3. **Soft Delete**: Business entities support soft delete with `IsDeleted`, `DeleterId`, `DeletionTime`
4. **Framework-Control Hierarchy**: `Frameworks` → `Controls` → `TenantControlSets` (resolved)
5. **Assessment-Evidence Chain**: `GrcPlans` → `Assessments` → `EvidenceRequests` → `EvidenceSubmissions`
6. **Finding-Remediation Flow**: `Findings` → `RemediationActions` → `WorkflowInstances`

### **Critical Foreign Key Constraints**

```sql
-- Ensure all tenant data is properly isolated
ALTER TABLE [TenantTable] ADD CONSTRAINT FK_TenantTable_Tenant 
    FOREIGN KEY (TenantId) REFERENCES AbpTenants(Id) ON DELETE CASCADE;

-- Ensure referential integrity for compliance data
ALTER TABLE TenantControlSets ADD CONSTRAINT FK_TenantControlSets_Control
    FOREIGN KEY (ControlId) REFERENCES Controls(Id) ON DELETE RESTRICT;

-- Ensure workflow data integrity
ALTER TABLE WorkflowSteps ADD CONSTRAINT FK_WorkflowSteps_Instance
    FOREIGN KEY (WorkflowInstanceId) REFERENCES WorkflowInstances(Id) ON DELETE CASCADE;
```

---

## Implementation Guidelines

### **1. Database Creation Strategy**

```sql
-- 1. Create ABP Foundation (Layers 1-12)
-- Use ABP CLI to generate these tables
-- dotnet add package Volo.Abp.EntityFrameworkCore

-- 2. Create GRC Extensions in phases:
-- Phase 1: Onboarding & Control Plane (Layers 13-20)
-- Phase 2: Compliance Catalog (Layers 21-30)  
-- Phase 3: Tenant Resolution (Layers 31-36)
-- Phase 4: Execution Layers (Layers 37-43)
```

### **2. Indexing Strategy**

```sql
-- Primary indexes on all FKs
CREATE INDEX IX_[Table]_TenantId ON [Table] (TenantId);

-- Composite indexes for common queries
CREATE INDEX IX_Controls_Framework_Criticality ON Controls (FrameworkId, Criticality);
CREATE INDEX IX_Assessments_Tenant_Status_Due ON Assessments (TenantId, Status, DueDate);
CREATE INDEX IX_Findings_Tenant_Severity_Status ON Findings (TenantId, Severity, Status);

-- Performance indexes for analytics
CREATE INDEX IX_KpiValues_Tenant_Date ON KpiValues (TenantId, ValueDate) INCLUDE (Value, Status);
CREATE INDEX IX_Analytics_Tenant_Type_Date ON AnalyticsSnapshots (TenantId, SnapshotType, SnapshotDate);
```

### **3. Partitioning Strategy**

```sql
-- Partition large audit/analytics tables by date
-- Consider monthly partitions for:
-- - AbpAuditLogs
-- - RulesEvaluationLogs  
-- - AnalyticsSnapshots
-- - KpiValues

CREATE PARTITION FUNCTION PF_Monthly (DATETIME2)
AS RANGE RIGHT FOR VALUES 
('2024-01-01', '2024-02-01', '2024-03-01', /* ... */);

CREATE PARTITION SCHEME PS_Monthly
AS PARTITION PF_Monthly ALL TO ([PRIMARY]);

-- Apply to audit tables
CREATE TABLE AbpAuditLogs_Partitioned (
    -- columns same as above
) ON PS_Monthly (CreationTime);
```

### **4. Data Seeding Strategy**

```sql
-- 1. Seed Reference Data First (Layers 21-30)
INSERT INTO Regulators (Code, Name, FullName, Jurisdiction) VALUES
('NCA', 'NCA', 'National Cybersecurity Authority', 'Saudi Arabia'),
('SAMA', 'SAMA', 'Saudi Arabian Monetary Authority', 'Saudi Arabia'),
('CITC', 'CITC', 'Communications and Information Technology Commission', 'Saudi Arabia');

INSERT INTO Frameworks (RegulatorId, Code, Name, Version, EffectiveDate) VALUES
-- NCA-ECC, SAMA-CSF, PDPL, ISO27001, PCI-DSS, etc.

-- 2. Seed Control Library
INSERT INTO Controls (FrameworkId, ControlId, Title, Description, Criticality) VALUES
-- Complete control sets for each framework

-- 3. Seed Rules & Overlays  
INSERT INTO ApplicabilityRules (RuleId, RuleName, Conditions, Actions) VALUES
-- Framework selection rules, overlay triggers, etc.
```

---

## Security & Performance

### **1. Security Measures**

```sql
-- Row-Level Security (if using SQL Server 2016+)
CREATE SECURITY POLICY TenantSecurityPolicy
ADD FILTER PREDICATE security.fn_securitypredicate(TenantId) ON [Table],
ADD BLOCK PREDICATE security.fn_securitypredicate(TenantId) ON [Table];

-- Encryption for sensitive data
ALTER TABLE EvidenceSubmissions
ALTER COLUMN FileUrl NVARCHAR(500) 
ENCRYPTED WITH (ENCRYPTION_TYPE = DETERMINISTIC, 
               ALGORITHM = 'AES_256', 
               COLUMN_ENCRYPTION_KEY = GRC_CEK);

-- Audit sensitive operations
CREATE TRIGGER TR_ControlSets_Audit ON TenantControlSets
FOR INSERT, UPDATE, DELETE AS
-- Log all control set changes for compliance audit
```

### **2. Performance Optimization**

```sql
-- Columnstore indexes for analytics workloads
CREATE NONCLUSTERED COLUMNSTORE INDEX IX_Analytics_Columnstore
ON AnalyticsSnapshots (TenantId, SnapshotDate, MetricsData);

-- Memory-optimized tables for high-frequency operations
CREATE TABLE OnboardingState_Memory (
    TenantId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,
    Status INT NOT NULL,
    LastActivity DATETIME2 NOT NULL,
    -- other frequently accessed columns
) WITH (MEMORY_OPTIMIZED = ON, DURABILITY = SCHEMA_AND_DATA);

-- Computed columns for common calculations
ALTER TABLE Assessments ADD CompliancePercentage 
AS (CASE WHEN TotalControls > 0 THEN 
    (CompliantControls * 100.0 / TotalControls) 
    ELSE 0 END) PERSISTED;
```

### **3. Backup & Recovery Strategy**

```sql
-- Differential backup strategy for large databases
-- Full backup: Weekly
-- Differential backup: Daily  
-- Transaction log backup: Every 15 minutes

BACKUP DATABASE ShahinGrc 
TO DISK = 'C:\Backups\ShahinGrc_Full.bak' 
WITH COMPRESSION, CHECKSUM, INIT;

-- Point-in-time recovery setup
ALTER DATABASE ShahinGrc SET RECOVERY FULL;
```

---

## Migration Strategy

### **Phase 1: Foundation (Week 1)**
- Deploy ABP Platform tables (Layers 1-12)
- Basic tenant creation working
- User authentication functional

### **Phase 2: Onboarding Core (Week 2)**
- Deploy Onboarding Control Plane (Layers 13-20)
- Trial registration → tenant creation flow
- Basic wizard state management

### **Phase 3: Compliance Engine (Week 3-4)**
- Deploy Compliance Catalog (Layers 21-30)
- Seed reference data (regulators, frameworks, controls)
- Deploy Tenant Resolution (Layers 31-36)
- Rules engine working end-to-end

### **Phase 4: Execution Platform (Week 5-6)**
- Deploy Execution Layers (Layers 37-43)
- Complete GRC workflow operational
- Analytics and reporting functional

### **Phase 5: Optimization (Week 7-8)**
- Performance tuning
- Security hardening
- Monitoring and alerting setup
- Load testing and optimization

---

**🎯 IMPLEMENTATION SUCCESS CRITERIA:**

✅ **Tenant Isolation**: Cross-tenant data access impossible  
✅ **Audit Compliance**: Every decision explainable and traceable  
✅ **Performance**: Sub-2s response times for all operations  
✅ **Scalability**: Support 1000+ concurrent tenants  
✅ **Data Integrity**: Zero data corruption in 99.99% uptime  
✅ **Compliance Ready**: Full audit trail for regulatory review  

---

*This 43-layer database schema provides the foundation for enterprise-grade GRC platform with autonomous onboarding, explainable AI decisions, and regulatory-ready audit trails.*
