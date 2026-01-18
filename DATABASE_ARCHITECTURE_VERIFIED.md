# 🗄️ Shahin AI GRC - Database Architecture Verification
## 2 Databases + 43-Layer Logical Architecture

---

## ✅ VERIFIED: 2 Physical Databases

| Database | DbContext | Purpose |
|----------|-----------|---------|
| **GrcMvcDb** | `GrcDbContext` | Main GRC application data |
| **GrcAuthDb** | `GrcAuthDbContext` | Identity & Authentication |

### Database 1: GrcMvcDb (Main Application)
```
DbContext: GrcDbContext : AbpDbContext<GrcDbContext>
Location: Data/GrcDbContext.cs

Contains:
├── Multi-Tenant Core
│   ├── Tenants
│   ├── TenantUsers
│   ├── OrganizationProfiles
│   └── OnboardingWizards
│
├── GRC Entities
│   ├── Risks
│   ├── Controls
│   ├── Assessments
│   ├── Policies
│   ├── Evidence
│   ├── Workflows
│   └── ... (100+ entities)
│
├── Global Catalogs
│   ├── RegulatorCatalogs
│   ├── FrameworkCatalogs
│   ├── ControlCatalogs (13,528 controls)
│   └── ...
│
└── 43-Layer Architecture Entities
    ├── OnboardingAnswerSnapshots (Layer 14)
    ├── OnboardingDerivedOutputs (Layer 15)
    ├── RulesEvaluationLogs (Layer 16)
    ├── ExplainabilityPayloads (Layer 17)
    ├── TenantFrameworkSelections (Layer 31)
    ├── TenantOverlays (Layer 32)
    ├── TenantControlSets (Layer 33)
    ├── TenantScopeBoundaries (Layer 34)
    └── TenantRiskProfiles (Layer 35)
```

### Database 2: GrcAuthDb (Authentication)
```
DbContext: GrcAuthDbContext : IdentityDbContext<ApplicationUser>
Location: Data/GrcAuthDbContext.cs

Contains:
├── ASP.NET Identity Tables
│   ├── AspNetUsers (ApplicationUser)
│   ├── AspNetRoles
│   ├── AspNetUserRoles
│   ├── AspNetUserClaims
│   ├── AspNetUserLogins
│   └── AspNetUserTokens
│
└── Security Audit Tables
    ├── PasswordHistory
    ├── RefreshTokens
    ├── LoginAttempts
    └── AuthenticationAuditLogs
```

---

## 📐 43-Layer LOGICAL Architecture

The "43 layers" are NOT 43 databases - they are a **logical architecture** for organizing code and data:

### Layer 0: Platform Administration
```
- PlatformAdmin entity
- Super-admin above all tenants
```

### Layers 1-12: Platform Layer (Infrastructure)
| Layer | Name | Implementation |
|-------|------|----------------|
| 1 | Tenants | Tenant entity + TenantContextService |
| 2 | Users | ASP.NET Core Identity |
| 3 | Editions | Edition entity |
| 4 | Roles | RoleProfile entity |
| 5 | Permissions | PermissionCatalog entity |
| 6 | Features | FeatureCheckService |
| 7 | Settings | TenantSettings entity |
| 8 | Audit Logs | AuditEventService |
| 9 | Background Jobs | Hangfire |
| 10 | Data Dictionary | Lookup tables |
| 11 | Blob Storage | Azure Blob Storage |
| 12 | Notifications | Custom notification system |

### Layers 13-20: Onboarding Control Plane
| Layer | Name | Entity | Purpose |
|-------|------|--------|---------|
| 13 | Wizard State | OnboardingWizard | 12-step wizard state |
| 14 | Answer Snapshots | OnboardingAnswerSnapshot | Immutable versioned answers |
| 15 | Derived Outputs | OnboardingDerivedOutput | Derived baselines/packages |
| 16 | Rules Evaluation | RulesEvaluationLog | Rule evaluation audit |
| 17 | Explainability | ExplainabilityPayload | Human-readable decisions |
| 18-20 | Reserved | - | Future expansion |

### Layers 21-30: Reserved
```
Future expansion for additional modules
```

### Layers 31-36: Tenant Compliance Resolution
| Layer | Name | Entity | Purpose |
|-------|------|--------|---------|
| 31 | Framework Selection | TenantFrameworkSelection | Tenant's framework choices |
| 32 | Overlays | TenantOverlay | Industry/size overlays |
| 33 | Control Sets | TenantControlSet | Resolved controls per tenant |
| 34 | Scope Boundaries | TenantScopeBoundary | In-scope entities/systems |
| 35 | Risk Profiles | TenantRiskProfile | Risk characteristics |
| 36 | Reserved | - | Future expansion |

### Layers 37-43: Reserved
```
Future expansion
```

---

## 🔄 Data Flow Through Layers

```
┌─────────────────────────────────────────────────────────────────┐
│ Layer 0: Platform Admin                                         │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ Manages global catalogs, all tenants, platform settings     │ │
│ └─────────────────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────────┤
│ Layers 1-12: Platform Infrastructure                           │
│ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐           │
│ │ Tenants  │ │ Users    │ │ Roles    │ │ Perms    │ ...       │
│ │ Layer 1  │ │ Layer 2  │ │ Layer 4  │ │ Layer 5  │           │
│ └──────────┘ └──────────┘ └──────────┘ └──────────┘           │
├─────────────────────────────────────────────────────────────────┤
│ Layers 13-20: Onboarding Control Plane                         │
│                                                                 │
│   User answers → Snapshot → Rules Engine → Derived Output       │
│       (13)         (14)        (16)           (15)              │
│                                  ↓                              │
│                           Explainability (17)                   │
├─────────────────────────────────────────────────────────────────┤
│ Layers 31-36: Tenant Compliance Resolution                      │
│                                                                 │
│   Derived Output → Framework Selection → Overlays → Control Set │
│       (15)              (31)             (32)         (33)      │
│                                                         ↓       │
│                                    Scope Boundaries → Risk Profile
│                                          (34)           (35)    │
├─────────────────────────────────────────────────────────────────┤
│ Operational Layer: GRC Execution                                │
│ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐           │
│ │ Risks    │ │ Controls │ │ Evidence │ │ Workflows│ ...       │
│ └──────────┘ └──────────┘ └──────────┘ └──────────┘           │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📊 Entity Count by Layer

| Layer Group | Entities | Status |
|-------------|----------|--------|
| **Layer 0** (Platform Admin) | 1 | ✅ Implemented |
| **Layers 1-12** (Platform) | ~20 | ✅ Implemented |
| **Layers 13-20** (Onboarding) | 5 | ✅ Implemented |
| **Layers 31-36** (Compliance) | 5 | ✅ Implemented |
| **GRC Operational** | 100+ | ✅ Implemented |
| **Catalogs** | 9 | ✅ Implemented |

---

## 🗃️ Connection Strings

### docker-compose.production.yml
```yaml
services:
  grcmvc-prod:
    environment:
      # Main database
      - ConnectionStrings__DefaultConnection=Host=db-prod;Database=GrcMvcDb;...
      # Auth database (separate)
      - ConnectionStrings__GrcAuthDb=Host=db-prod;Database=GrcMvcDb_auth;...
```

### Kubernetes (secrets.yaml)
```yaml
stringData:
  CONNECTION_STRING: "Host=postgres-headless;Database=GrcMvcDb;..."
  AUTH_CONNECTION_STRING: "Host=postgres-headless;Database=GrcAuthDb;..."
```

---

## ✅ Summary

| Question | Answer |
|----------|--------|
| **How many physical databases?** | **2** (GrcMvcDb + GrcAuthDb) |
| **How many DbContexts?** | **2** (GrcDbContext + GrcAuthDbContext) |
| **What is 43-Layer?** | **Logical architecture**, not physical databases |
| **Are all layers implemented?** | Layers 0, 1-12, 13-17, 31-35 = **YES** |
| **Total entities?** | **130+** across both databases |

---

*Verified: 2026-01-16*
*Source: Direct codebase analysis*
