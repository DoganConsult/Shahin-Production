# Complete Solution Paths - All Routes & Endpoints

## 📋 Overview
This document lists ALL major routes, paths, and endpoints in the Shahin AI GRC system.

**Last Updated:** 2026-01-13
**Branch:** claude/fix-database-duplication-qQvTq

---

## 🏠 PUBLIC LANDING PAGES

| Path | Purpose | Controller | View |
|------|---------|------------|------|
| `/` | Home/Landing page | LandingController | Views/Landing/Index.cshtml |
| `/features` | Features page | LandingController | Views/Landing/Features.cshtml |
| `/pricing` | Pricing page | LandingController | Views/Landing/Pricing.cshtml |
| `/about` | About page | LandingController | Views/Landing/About.cshtml |
| `/contact` | Contact page | LandingController | Views/Landing/Contact.cshtml |
| `/faq` | FAQ page | LandingController | Views/Landing/Faq.cshtml |
| `/privacy` | Privacy policy | LandingController | Views/Landing/Privacy.cshtml |
| `/terms` | Terms & conditions | LandingController | Views/Landing/Terms.cshtml |
| `/status` | System status | LandingController | Views/Landing/Status.cshtml |

---

## 🔵 TRIAL SIGNUP (Creates NEW Organization)

| Path | Method | Purpose | Controller | View |
|------|--------|---------|------------|------|
| `/trial` | GET | Show trial form | TrialController | Views/Landing/FreeTrial.cshtml |
| `/trial/register` | POST | Create new tenant | TrialController | - |
| `/trial/demo-request` | POST | Request demo | TrialController | - |

**✅ ALL LANDING PAGE BUTTONS NOW POINT TO:** `/trial`

**Buttons Updated:**
- Navbar desktop: Line 173 → `/trial` ✅
- Navbar mobile: Line 209 → `/trial` ✅
- Footer product section: Line 259 → `/trial` ✅
- Landing page hero: Line 32 → `/trial` ✅
- Landing page CTA (2 locations): Lines 385, 575 → `/trial` ✅

---

## 🟢 USER AUTHENTICATION & REGISTRATION

| Path | Method | Purpose | Controller | View |
|------|--------|---------|------------|------|
| `/account/login` | GET | Login page | AccountController | Views/Account/Login.cshtml |
| `/account/login` | POST | Authenticate user | AccountController | - |
| `/account/register` | GET | Register form | AccountController | Views/Account/Register.cshtml |
| `/account/register` | POST | Create user | AccountController | - |
| `/account/logout` | POST | Logout | AccountController | - |
| `/account/forgot-password` | GET | Forgot password | AccountController | Views/Account/ForgotPassword.cshtml |
| `/account/reset-password` | GET | Reset password | AccountController | Views/Account/ResetPassword.cshtml |
| `/account/change-password` | GET | Change password | AccountController | Views/Account/ChangePassword.cshtml |
| `/account/profile` | GET | User profile | AccountController | Views/Account/Profile.cshtml |
| `/account/manage` | GET | Manage account | AccountController | Views/Account/Manage.cshtml |
| `/account/verify-mfa` | GET | MFA verification | AccountController | Views/Account/VerifyMfa.cshtml |

**✅ SIGNUP BUTTON ADDED TO:**
- Navbar desktop: Line 169-172 → `/account/register` ✅
- Navbar mobile: Line 205-208 → `/account/register` ✅
- Footer product section: Line 260 → `/account/register` ✅

---

## 🔐 PLATFORM ADMIN (Owner/Deployment Access)

| Path | Method | Purpose | Controller | View |
|------|--------|---------|------------|------|
| `/admin/login` | GET | Admin login | AdminPortalController | Views/AdminPortal/Login.cshtml |
| `/admin/login` | POST | Authenticate admin | AdminPortalController | - |
| `/admin/dashboard` | GET | Admin dashboard | AdminPortalController | Views/AdminPortal/Dashboard.cshtml |
| `/admin/tenants` | GET | Manage tenants | AdminPortalController | Views/AdminPortal/Tenants.cshtml |
| `/admin/tenantdetails/{id}` | GET | Tenant details | AdminPortalController | Views/AdminPortal/TenantDetails.cshtml |
| `/owner` | GET | Owner portal | OwnerController | Views/Owner/Index.cshtml |
| `/owner/create` | POST | Create tenant | OwnerController | - |
| `/owner-setup` | GET | Owner setup wizard | OwnerSetupController | Views/OwnerSetup/Index.cshtml |

**✅ PLATFORM ADMIN LINK ADDED:**
- Footer legal section: Line 296-298 → `/admin/login` (subtle icon 🔒) ✅

---

## 📊 CORE GRC MODULES

### Stage 1: Scope & Context
| Path | Controller | Purpose |
|------|------------|---------|
| `/assessment` | AssessmentController | Compliance assessments |
| `/scope` | ScopeController | Compliance scope management |
| `/onboarding-wizard` | OnboardingWizardController | 12-step onboarding |

### Stage 2: Risk Management
| Path | Controller | Purpose |
|------|------------|---------|
| `/risk` | RiskController | Risk register |
| `/threat` | ThreatController | Threat profiles |
| `/vulnerability` | VulnerabilityController | Vulnerability tracking |
| `/risk-appetite` | RiskAppetiteController | Risk appetite settings |

### Stage 3: Control Implementation
| Path | Controller | Purpose |
|------|------------|---------|
| `/control` | ControlController | Control library (400+) |
| `/control-matrix` | ControlMatrixController | Control mapping |
| `/control-test` | ControlTestController | Control testing |
| `/policy` | PolicyController | Policy management |
| `/evidence` | EvidenceController | Evidence repository |

### Stage 4: Monitoring & Testing
| Path | Controller | Purpose |
|------|------------|---------|
| `/audit` | AuditController | Audit management |
| `/finding` | FindingController | Audit findings |
| `/monitoring` | MonitoringDashboardController | Continuous monitoring |
| `/certification` | CertificationController | Certification tracking |

### Stage 5: Resilience (⚠️ Needs completion)
| Path | Controller | Purpose |
|------|------------|---------|
| `/resilience` | ResilienceController | Business resilience |
| `/sustainability` | SustainabilityController | ⚠️ 4 TODOs |

### Stage 6: Excellence (⚠️ Needs completion)
| Path | Controller | Purpose |
|------|------------|---------|
| `/excellence` | ExcellenceController | ⚠️ 4 TODOs |
| `/kpis` | KPIsController | ⚠️ 2 TODOs |
| `/benchmarking` | BenchmarkingController | ⚠️ 1 TODO |

---

## 🤖 AI AGENTS & CHAT

| Path | Controller | Purpose |
|------|------------|---------|
| `/agent` | AgentController | AI agent orchestration |
| `/agent-chat` | AgentChatController | AI chat interface |
| `/api/agent/*` | API: AgentController | Agent API endpoints |

**Available Agents (12):**
1. SHAHIN_AI - Main orchestrator
2. COMPLIANCE_AGENT - Compliance analysis
3. RISK_AGENT - Risk assessment
4. AUDIT_AGENT - Audit analysis
5. POLICY_AGENT - Policy management
6. ANALYTICS_AGENT - Analytics & insights
7. REPORT_AGENT - Report generation
8. DIAGNOSTIC_AGENT - System diagnostics
9. SUPPORT_AGENT - Customer support
10. WORKFLOW_AGENT - Workflow optimization
11. EVIDENCE_AGENT - Evidence collection
12. EMAIL_AGENT - Email classification

---

## 📈 DASHBOARDS & ANALYTICS

| Path | Controller | Purpose |
|------|------------|---------|
| `/home/index` | HomeController | Main dashboard |
| `/dashboard` | DashboardController | Overview dashboard |
| `/advanced-dashboard` | AdvancedDashboardController | Advanced analytics |
| `/monitoring-dashboard` | MonitoringDashboardController | Real-time monitoring |
| `/owner-dashboard` | OwnerController | Owner multi-tenant view |

---

## 🔄 WORKFLOWS

| Path | Controller | Purpose |
|------|------------|---------|
| `/workflow` | WorkflowController | Workflow management |
| `/workflow-definition` | WorkflowDefinitionController | Define workflows |
| `/workflow-instance` | WorkflowInstanceController | Active workflows |
| `/inbox` | InboxController | Task inbox |
| `/approval` | ApprovalController | Approval workflows |

**10 Workflow Types:**
1. Control Implementation
2. Risk Assessment
3. Approval
4. Evidence Collection
5. Compliance Testing
6. Remediation
7. Policy Review
8. Training Assignment
9. Audit
10. Exception Handling

---

## 🔔 NOTIFICATIONS & MESSAGES

| Path | Controller | Purpose |
|------|------------|---------|
| `/notification` | NotificationController | User notifications |
| `/message` | MessageController | Internal messaging |
| `/email` | EmailController | Email management |

---

## 📚 KNOWLEDGE BASE & HELP

| Path | Controller | Purpose |
|------|------------|---------|
| `/knowledge-base` | KnowledgeBaseController | Help articles |
| `/help` | HelpController | Help center |
| `/docs` | DocsController | Documentation |
| `/blog` | BlogController | Blog articles |
| `/webinars` | WebinarsController | Webinar library |
| `/case-studies` | CaseStudiesController | Case studies |

---

## ⚙️ SYSTEM SETTINGS

| Path | Controller | Purpose |
|------|------------|---------|
| `/settings` | SettingsController | System settings |
| `/tenant-settings` | TenantSettingsController | Tenant config |
| `/workspace` | WorkspaceController | Workspace management |
| `/workspace-switcher` | Component | Switch workspaces |
| `/user-management` | UserManagementController | User admin |
| `/role-management` | RoleManagementController | Role config |

---

## 🌐 LOCALIZATION & LANGUAGE

| Path | Method | Purpose |
|------|--------|---------|
| `/home/set-language` | GET | Change language |

**Supported Languages:**
- 🇸🇦 Arabic (ar) - Default, RTL
- 🇺🇸 English (en)
- 🇹🇷 Turkish (tr) - ✅ NEWLY ADDED

**Language Switcher Location:**
- Main layout navbar: Lines 328-373
- Shows current language with flag
- Dropdown with all 3 languages

---

## 🔒 SECURITY & COMPLIANCE

### Health Checks
| Path | Purpose |
|------|---------|
| `/health` | Application health |
| `/health/ready` | Readiness check |
| `/health/db` | Database connectivity |
| `/health/detailed` | All subsystems |

### Security Headers (Middleware)
- ✅ Nonce-based CSP (removed unsafe-inline/unsafe-eval)
- ✅ X-Frame-Options: DENY
- ✅ X-Content-Type-Options: nosniff
- ✅ Strict-Transport-Security (HSTS)
- ✅ Referrer-Policy
- ✅ Permissions-Policy

### API Security
- JWT Bearer authentication
- Rate limiting: 3 attempts per 15 minutes
- CSRF protection on all forms
- Query filters for tenant isolation (100% coverage)

---

## 📡 API ENDPOINTS

### Base URL: `/api/`

**Core Resources:**
- `/api/assessment/*` - Assessment API
- `/api/risk/*` - Risk API
- `/api/control/*` - Control API
- `/api/audit/*` - Audit API
- `/api/evidence/*` - Evidence API
- `/api/workflow/*` - Workflow API
- `/api/agent/*` - AI Agent API
- `/api/dashboard/*` - Dashboard API
- `/api/platform-admin/*` - Platform Admin API

**Total API Controllers:** 20+
**Total API Endpoints:** 200+

---

## 🔧 DEVELOPMENT & ADMIN TOOLS

| Path | Controller | Purpose |
|------|------------|---------|
| `/api/seed/*` | SeedController | Database seeding |
| `/migration-metrics` | MigrationMetricsController | Track migrations |
| `/swagger` | - | API documentation (Dev only) |

**⚠️ SECURITY NOTE:**
- `/api/seed/fix-all-admins` endpoint was **REMOVED** (Issue #1 - CRITICAL)
- Exposed admin credentials - security vulnerability fixed

---

## 📂 FILE STRUCTURE REFERENCE

### Controllers Location:
```
/home/user/Shahin-Jan-2026/src/GrcMvc/Controllers/
├── AccountController.cs (1,575 lines - god class)
├── TrialController.cs
├── AdminPortalController.cs
├── OwnerController.cs
├── OnboardingWizardController.cs (2,424 lines - god class)
├── LandingController.cs (1,906 lines - god class)
└── ... (100+ controllers total)
```

### Views Location:
```
/home/user/Shahin-Jan-2026/src/GrcMvc/Views/
├── Landing/
│   ├── Index.cshtml (main landing page)
│   ├── FreeTrial.cshtml (trial signup form)
│   └── _LandingLayout.cshtml (landing layout)
├── Account/
│   ├── Register.cshtml (user signup form)
│   ├── Login.cshtml
│   └── ... (21 account views)
├── Shared/
│   └── _Layout.cshtml (main app layout)
└── ... (337 views total)
```

---

## 🎯 RECENT FIXES (2026-01-13)

### ✅ Commit: Latest (uncommitted)
**Landing Page Navigation - COMPLETE FIX**

**Navbar Changes:**
- ✅ Added "إنشاء حساب" (Create Account) button → `/account/register`
- ✅ Fixed trial button → `/trial` (was `/grc-free-trial`)

**Footer Changes:**
- ✅ Fixed trial link → `/trial` (was `/grc-free-trial`)
- ✅ Added signup link → `/account/register`
- ✅ Added subtle platform admin link 🔒 → `/admin/login`

**Mobile Menu:**
- ✅ Added signup button → `/account/register`
- ✅ Fixed trial button → `/trial`

**All Buttons Now Correct:**
1. Login → `/account/login` ✅
2. Signup → `/account/register` ✅ (NEWLY ADDED)
3. Trial → `/trial` ✅ (ALL FIXED)
4. Admin → `/admin/login` ✅ (NEWLY ADDED - subtle)

---

## 🔍 QUICK VERIFICATION COMMANDS

```bash
# Find all trial button references
grep -r "/trial\|grc-free-trial" /home/user/Shahin-Jan-2026/src/GrcMvc/Views/Landing -n

# Find all account/register references
grep -r "/account/register" /home/user/Shahin-Jan-2026/src/GrcMvc/Views/Landing -n

# Find all admin login references
grep -r "/admin/login" /home/user/Shahin-Jan-2026/src/GrcMvc/Views -n

# List all controllers
ls /home/user/Shahin-Jan-2026/src/GrcMvc/Controllers/*.cs | wc -l

# List all views
find /home/user/Shahin-Jan-2026/src/GrcMvc/Views -name "*.cshtml" | wc -l
```

---

## 📊 STATISTICS

| Metric | Count |
|--------|-------|
| **Total Controllers** | 100+ |
| **Total Views** | 337 |
| **Total API Endpoints** | 200+ |
| **Supported Languages** | 3 (ar, en, tr) |
| **AI Agents** | 12 |
| **Workflow Types** | 10 |
| **Query Filters** | 170 (100% coverage) |
| **Security Issues Fixed** | 17/21 (81% complete) |

---

## ⚠️ KNOWN ISSUES

### Database Connection Error (Current):
```
SocketException: Resource temporarily unavailable
```
**Cause:** PostgreSQL not running
**Solution:** Start database with `docker-compose up -d db`

### Remaining Work (4 issues):
1. **Issue #11** - Complete Risk Contextualization view
2. **Issue #5** - Complete Stage 5-6 controllers (11 TODOs)
3. **Issue #2** - Purge secrets from git history
4. **Issue #3** - Refactor god classes (3 controllers)

---

## 📝 NOTES FOR DEPLOYMENT

**Before Deploying:**
1. ✅ All trial links point to `/trial`
2. ✅ Signup buttons added to landing page
3. ✅ Platform admin link accessible (footer)
4. ✅ Turkish language supported
5. ✅ CSP security headers enabled
6. ✅ XSS vulnerabilities fixed
7. ✅ Multi-tenancy isolation complete

**Required Environment Variables:**
- `DB_HOST`, `DB_PORT`, `DB_NAME`, `DB_USER`, `DB_PASSWORD`
- `JWT_SECRET` (32+ characters)
- `CLAUDE_API_KEY` (if AI features enabled)
- `SMTP_*` settings (if email enabled)

**Start Command:**
```bash
cd /home/user/Shahin-Jan-2026/src/GrcMvc
dotnet run
```

---

**Document Version:** 3.0
**Last Updated:** 2026-01-13 15:45 UTC
**Maintained By:** Claude AI Assistant
