# 📊 Onboarding & Trial Features Report
## Why We Built It & How It Works

---

## 🎯 WHY: Business Purpose

### The Visitor Journey Strategy

```
┌─────────────────────────────────────────────────────────────────┐
│                     VISITOR JOURNEY                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   Landing Page → Trial Signup → Onboarding → Trial Use → Pay   │
│                                                                 │
│   1. Attract    2. Capture    3. Configure  4. Engage  5. Convert
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Business Goals

| Goal | Implementation |
|------|----------------|
| **Lead Capture** | Trial signup form captures email, company, sector |
| **Low Friction** | 7-day free trial, no credit card required |
| **Self-Service** | 12-step wizard auto-configures compliance scope |
| **Value Demonstration** | Full features during trial (with limits) |
| **Conversion Path** | Clear upgrade CTA when trial expires |

---

## 🔄 HOW: Complete Flow

### Step 1: Trial Signup
```
Location: TrialLifecycleService.SignupAsync()

Captures:
├── Email (required)
├── First Name
├── Last Name
├── Company Name
├── Sector
├── Source (website, referral, etc.)
└── Referral Code (optional)

Creates:
├── TrialSignup record (status: "pending")
└── Activation token
```

### Step 2: Trial Provisioning
```
Location: TrialLifecycleService.ProvisionTrialAsync()

Creates:
├── Tenant (IsTrial: true, TrialEndsAt: +7 days)
├── ApplicationUser (admin user)
├── TenantUser (links user to tenant)
└── Sends welcome email

Sets:
├── OnboardingStatus: "NOT_STARTED"
├── Status: "trial"
└── TrialStartsAt: now
```

### Step 3: 12-Step Onboarding Wizard
```
Location: OnboardingWizardService + OnboardingControlPlaneService

Steps:
1. Organization Identity (name, type, sector)
2. Assurance Objective (what they want to achieve)
3. Regulatory Applicability (country, mandatory frameworks)
4. Scope Definition (assets, systems, locations)
5. Data Risk Profile (data types, sensitivity)
6. Technology Landscape (cloud, on-prem, hybrid)
7. Control Ownership (who owns what)
8. Teams & Roles (users, responsibilities)
9. Workflow Cadence (review cycles)
10. Evidence Standards (what evidence needed)
11. Baseline Overlays (industry-specific requirements)
12. Go-Live Metrics (success criteria)

43-Layer Architecture Integration:
├── Layer 14: Answer Snapshots (immutable audit trail)
├── Layer 15: Derived Outputs (frameworks, controls)
├── Layer 16: Rules Evaluation (auto-selection logic)
├── Layer 17: Explainability (human-readable decisions)
├── Layer 31: Framework Selection (which frameworks apply)
├── Layer 32: Overlays (sector/jurisdiction modifiers)
├── Layer 33: Control Sets (resolved controls)
├── Layer 34: Scope Boundaries (what's in scope)
└── Layer 35: Risk Profile (risk characteristics)
```

### Step 4: Trial Features Access
```
Location: TrialLifecycleService.GetTrialStatusAsync()

Available Features (Trial):
├── Compliance Frameworks: 2 (limited)
├── Team Members: 5 (limited)
├── AI Analysis: 10/day (limited)
└── Storage: 500 MB (limited)

Locked Features (Paid):
├── Unlimited Frameworks
├── Advanced Reporting
├── API Access
└── Priority Support
```

### Step 5: Feature Gating
```
Location: FeatureCheckService + SubscriptionService

How it works:
1. User accesses a feature
2. FeatureCheckService checks:
   ├── Tenant's current subscription plan
   ├── Plan's allowed features
   └── Usage limits (users, storage, AI calls)
3. If allowed → show feature
4. If blocked → show upgrade prompt
```

---

## 📊 Current Implementation Status

### Services Implemented

| Service | Status | Purpose |
|---------|--------|---------|
| `ITrialLifecycleService` | ✅ Full | Signup, provision, status, extension |
| `IOnboardingWizardService` | ✅ Full | 12-step wizard management |
| `IOnboardingControlPlaneService` | ✅ Full | 43-layer orchestration |
| `ISubscriptionService` | ✅ Full | Plans, billing, feature limits |
| `IFeatureCheckService` | ✅ Full | Feature gating per plan |
| `IRulesEngineService` | ✅ Full | Auto-derivation rules |

### Entities Implemented

| Entity | Table | Purpose |
|--------|-------|---------|
| `TrialSignup` | TrialSignups | Lead capture |
| `Tenant` | Tenants | Organization (IsTrial flag) |
| `TrialExtension` | TrialExtensions | Trial extensions |
| `OnboardingWizard` | OnboardingWizards | Wizard state |
| `OnboardingAnswerSnapshot` | OnboardingAnswerSnapshots | Layer 14 |
| `TenantFrameworkSelection` | TenantFrameworkSelections | Layer 31 |
| `TenantControlSet` | TenantControlSets | Layer 33 |
| `Subscription` | Subscriptions | Paid plans |
| `SubscriptionPlan` | SubscriptionPlans | Plan definitions |

### Background Jobs

| Job | Schedule | Purpose |
|-----|----------|---------|
| `TrialNurtureJob` | Hourly | Send nurture emails |
| `CheckExpiringTrials` | Daily 9 AM | Warn about expiring trials |
| `SendWinbackEmails` | Weekly Monday | Re-engage expired trials |

---

## 📈 Trial Limits vs Paid Plans

### Feature Matrix

| Feature | Trial | Starter | Professional | Enterprise |
|---------|-------|---------|--------------|------------|
| Frameworks | 2 | 5 | Unlimited | Unlimited |
| Team Members | 5 | 10 | 50 | Unlimited |
| AI Analysis/day | 10 | 50 | 200 | Unlimited |
| Storage | 500 MB | 5 GB | 50 GB | Unlimited |
| Advanced Reporting | ❌ | ❌ | ✅ | ✅ |
| API Access | ❌ | ❌ | ✅ | ✅ |
| Priority Support | ❌ | ❌ | ❌ | ✅ |
| SSO | ❌ | ❌ | ✅ | ✅ |
| Custom Branding | ❌ | ❌ | ❌ | ✅ |

### Pricing (SAR)

| Plan | Monthly | Annual |
|------|---------|--------|
| **Trial** | Free | - |
| **Starter** | SAR 99 | SAR 990 |
| **Professional** | SAR 299 | SAR 2,990 |
| **Enterprise** | Custom | Custom |

---

## 🔍 Gap Analysis

### ✅ Working Well

| Feature | Status |
|---------|--------|
| Trial signup flow | ✅ Complete |
| 12-step onboarding | ✅ Complete |
| Auto framework derivation | ✅ Complete |
| 43-layer data flow | ✅ Complete |
| Feature gating service | ✅ Complete |
| Subscription plans | ✅ Complete |
| Nurture email jobs | ✅ Complete |

### ⚠️ Needs Attention

| Feature | Issue | Priority |
|---------|-------|----------|
| Usage Tracking | AI call counts not tracked in real-time | Medium |
| Storage Limits | File size limits not enforced at upload | Medium |
| Trial Conversion UI | No in-app payment flow | High |
| Email Templates | Generic, not personalized | Low |
| Analytics | No trial funnel analytics | Medium |

### ❌ Missing

| Feature | Description | Priority |
|---------|-------------|----------|
| In-App Payment | Stripe integration incomplete | High |
| Usage Dashboard | Show users their limits/usage | Medium |
| Plan Comparison | Side-by-side in UI | Low |
| Trial Expiry Banner | Persistent warning banner | Medium |

---

## 🎯 Recommendations

### 1. Complete Payment Integration
```
Priority: HIGH
Effort: 8 hours

Tasks:
- Complete Stripe checkout flow
- Add subscription upgrade API
- Handle payment webhooks
- Create billing history view
```

### 2. Add Usage Tracking
```
Priority: MEDIUM
Effort: 4 hours

Tasks:
- Track AI API calls per tenant
- Track storage usage
- Add usage meters to dashboard
- Alert at 80% usage
```

### 3. Trial Expiry Handling
```
Priority: MEDIUM
Effort: 4 hours

Tasks:
- Add expiry banner component
- Show countdown timer
- Restrict features after expiry
- Grace period (3 days read-only)
```

### 4. Conversion Analytics
```
Priority: LOW
Effort: 2 hours

Tasks:
- Track signup → provision → activate → convert
- Add funnel visualization
- A/B test onboarding steps
```

---

## ✅ Summary

| Question | Answer |
|----------|--------|
| **Why onboarding?** | Auto-configure compliance scope from user answers |
| **Why trial?** | Low-friction entry, demonstrate value |
| **Full features in trial?** | Yes, with limits (2 frameworks, 5 users, etc.) |
| **Feature gating?** | ✅ Implemented via FeatureCheckService |
| **43-Layer integration?** | ✅ Full implementation |
| **Conversion path?** | ⚠️ Needs payment integration |

---

*Generated: 2026-01-16*
*Source: Codebase analysis*
