# GRC System Deployment Guide

## Quick Deployment

```bash
# From project root
./deploy.sh
```

## Manual Deployment Steps

### 1. Pre-Deployment Checklist

- [ ] All code changes committed to Git
- [ ] Build passes: `dotnet build -c Release`
- [ ] Database migrations ready
- [ ] Environment variables configured
- [ ] Docker installed and running

### 2. Environment Variables

Create a `.env` file or set these variables:

```bash
# Required
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=db;Database=GrcMvcDb;Username=postgres;Password=<SECURE_PASSWORD>;Port=5432

# JWT Settings
JwtSettings__Secret=<YOUR_SECURE_SECRET_KEY_MIN_32_CHARS>
JwtSettings__Issuer=https://your-domain.com
JwtSettings__Audience=https://your-domain.com

# Admin User (first run only)
AdminUser__Email=admin@your-domain.com
AdminUser__Password=<SECURE_ADMIN_PASSWORD>

# Optional - AI Features
Claude__ApiKey=<YOUR_CLAUDE_API_KEY>

# Optional - Alerts
Alerts__EnableEmail=true
Alerts__SlackWebhookUrl=<SLACK_WEBHOOK>
```

### 3. Deploy with Docker Compose

```bash
# Build and start
docker compose -f docker-compose.grcmvc.yml up -d --build

# View logs
docker compose -f docker-compose.grcmvc.yml logs -f

# Check health
curl http://localhost:5137/health

# Stop
docker compose -f docker-compose.grcmvc.yml down
```

### 4. Database Migrations

Migrations run automatically on startup. To run manually:

```bash
# Inside container
docker exec -it grcmvc-app dotnet ef database update

# Or from host (requires connection string)
cd src/GrcMvc
dotnet ef database update
```

### 5. Verify Deployment

| Check | URL | Expected |
|-------|-----|----------|
| Health | http://localhost:5137/health | "Healthy" |
| Live | http://localhost:5137/health/live | 200 OK |
| Ready | http://localhost:5137/health/ready | 200 OK |
| App | http://localhost:5137 | Login page |

### 6. Production Checklist

- [ ] Use strong passwords (not defaults)
- [ ] Enable HTTPS (use `docker-compose.https.yml`)
- [ ] Configure proper domain in `AllowedHosts`
- [ ] Set up backup for PostgreSQL volume
- [ ] Configure log rotation
- [ ] Set up monitoring/alerting
- [ ] Review security headers

---

## Complete Application Inventory (Jan 2026)

### All View Folders (43 Controllers, 150+ Views)

| Folder | Views | Description |
|--------|-------|-------------|
| **WorkflowUI** | 16 | Workflow management (Approvals, Evidence, Audits, Exceptions, Risks, Testing, Remediation, Policies, Training) |
| **OnboardingWizard** | 16 | 12-step wizard (Steps A-L) + partials |
| **Workflow** | 14 | Workflow definitions, instances, tasks |
| **Account** | 13 | Auth (Login, Register, 2FA, Password reset) |
| **Evidence** | 11 | Evidence management (Upload, Review, Expiring) |
| **Policy** | 10 | Policy lifecycle management |
| **Controls** | 10 | Control management (CRUD, Applicability, Assess) |
| **Audit** | 9 | Audit management (Findings, Statistics) |
| **Control** | 8 | Control operations (Matrix, ByRisk) |
| **Assessment** | 8 | Assessment management (Statistics, Upcoming) |
| **ShahinAI** | 7 | AI platform features |
| **Risk** | 7 | Risk management |
| **Plans** | 7 | Compliance plans (Phases, Teams) |
| **OrgSetup** | 7 | Organization setup |
| **Shared** | 6 | Layout, partials, modals |
| **Onboarding** | 6 | Onboarding flow |
| **Help** | 5 | Help Center, FAQ, Glossary, Contact |
| **EmailTemplates** | 5 | Email notifications |
| **Subscription** | 4 | Billing & subscriptions |
| **ShahinAIIntegration** | 4 | AI integration (OnboardingProfile, Assessments, Workflows) |
| **Home** | 4 | Landing pages |
| **DocumentFlow** | 3 | Document workflow |
| **Admin** | 2 | Admin panel, tenant management |
| **Others** | 20+ | Vendors, Reports, Frameworks, etc. |

### New Features Added (This Session)

#### WorkflowUI Views (12 NEW)
- `ControlImplementation.cshtml` - Control implementation list
- `ControlImplementationDetail.cshtml` - Implementation details
- `ApprovalDetail.cshtml` - Approval request details
- `EvidenceDetail.cshtml` - Evidence review & scoring
- `AuditDetail.cshtml` - Audit finding remediation
- `Exceptions.cshtml` - Exception list
- `ExceptionDetail.cshtml` - Exception request approval
- `Risks.cshtml` - Risk management workflows
- `Testing.cshtml` - Control testing queue
- `Remediation.cshtml` - Remediation tracking
- `Policies.cshtml` - Policy workflow queue
- `Training.cshtml` - Training assignments

#### ShahinAIIntegration Views (3 NEW)
- `OnboardingProfile.cshtml` - AI analysis of org profile
- `Assessments.cshtml` - AI-powered assessments
- `Workflows.cshtml` - AI-managed workflows

#### Controls Views (3 NEW)
- `Details.cshtml` - Control details view
- `Create.cshtml` - Create new control
- `Edit.cshtml` - Edit control

#### Help System (Complete)
- `Index.cshtml` - Help Center hub
- `GettingStarted.cshtml` - Getting started guide
- `FAQ.cshtml` - Searchable FAQ
- `Glossary.cshtml` - GRC terms dictionary
- `Contact.cshtml` - Support contact form

#### Shared Components
- `_GlossaryModal.cshtml` - Glossary popup modal
- `_WelcomeTour.cshtml` - First-login tour
- `_SupportChatWidget.cshtml` - Chat widget

#### JavaScript
- `help-system.js` - Tooltips, glossary, contextual help
- `tour.js` - Interactive welcome tour

#### Data Files
- `glossary.json` - GRC terms (EN/AR bilingual)

### Controllers (43 Total)

| Controller | Actions | Status |
|------------|---------|--------|
| AccountController | 13 | ✅ Complete |
| AdminController | 2 | ✅ Complete |
| AssessmentController | 8 | ✅ Complete |
| AuditController | 9 | ✅ Complete |
| ControlController | 8 | ✅ Complete |
| ControlsController | 10 | ✅ Complete |
| DashboardController | 1 | ✅ Complete |
| EvidenceController | 11 | ✅ Complete |
| HelpController | 5 | ✅ Complete |
| OnboardingController | 6 | ✅ Complete |
| OnboardingWizardController | 16 | ✅ Complete |
| PlansController | 7 | ✅ Complete |
| PolicyController | 10 | ✅ Complete |
| RiskController | 7 | ✅ Complete |
| ShahinAIController | 7 | ✅ Complete |
| ShahinAIIntegrationController | 4 | ✅ Complete |
| SubscriptionController | 4 | ✅ Complete |
| WorkflowController | 14 | ✅ Complete |
| WorkflowUIController | 16 | ✅ Complete |

### Database Migrations (22 Total)

| Date | Migration | Description |
|------|-----------|-------------|
| 2026-01-03 | InitialCreate | Base schema |
| 2026-01-04 | AddMultiTenantOnboarding | Multi-tenant + Rules Engine |
| 2026-01-04 | AddWorkflowInfrastructure | Workflow engine |
| 2026-01-04 | AddRoleProfileAndKsa | KSA compliance roles |
| 2026-01-04 | AddInboxAndTaskComments | Inbox, comments |
| 2026-01-04 | AddLlmConfiguration | AI/LLM config |
| 2026-01-04 | AddSubscriptionTables | Billing |
| 2026-01-05 | AddCatalogTables | Catalogs |
| 2026-01-05 | AddWorkflowDefinitionFields | Workflow fields |
| 2026-01-05 | AddFrameworkControlsTable | Framework controls |
| 2026-01-05 | AddNewGrcEntities | GRC entities |
| 2026-01-05 | AddReportFileFields | Reports |
| 2026-01-05 | AddTaskDelegationEntity | Task delegation |
| 2026-01-05 | ShahinAIPlatform | AI platform |
| 2026-01-06 | AddMissingOnboardingTables | Onboarding tables |
| 2026-01-06 | OnboardingGRCIntegration | GRC integration |
| 2026-01-06 | PolicyDecisionAuditTrail | Policy audit |
| 2026-01-06 | PocSeederSupport | POC seeding |
| 2026-01-06 | FixOnboardingWizardDefaults | Defaults fix |

### Services & Infrastructure

| Component | Status |
|-----------|--------|
| Multi-Tenant Architecture | ✅ |
| JWT Authentication | ✅ |
| Role-Based Access Control | ✅ |
| Workflow Engine | ✅ |
| Rules Engine | ✅ |
| Evidence Service | ✅ |
| Policy Enforcement | ✅ |
| AI Integration (Shahin) | ✅ |
| Email Notifications | ✅ |
| Health Checks (Master + Tenant DB) | ✅ |
| Bilingual Support (EN/AR) | ✅ |
| Help System | ✅ |
| Welcome Tour | ✅ |

### API Endpoints

- `/api/controls` - Control CRUD
- `/api/evidence` - Evidence management
- `/api/workflows` - Workflow operations
- `/api/assessments` - Assessment management
- `/api/plans` - Plan management
- `/api/risks` - Risk management
- `/api/audit` - Audit operations
- `/api/reports` - Report generation
- `/api/shahinai` - AI integration
- `/health` - Health checks

---

## Troubleshooting

### Container won't start
```bash
docker compose -f docker-compose.grcmvc.yml logs grcmvc
```

### Database connection issues
```bash
# Check DB is healthy
docker compose -f docker-compose.grcmvc.yml ps db

# Check connectivity
docker exec -it grcmvc-db pg_isready -U postgres
```

### Reset everything
```bash
docker compose -f docker-compose.grcmvc.yml down -v
docker compose -f docker-compose.grcmvc.yml up -d --build
```
