# Complete List of All Services and Endpoints

**Date**: January 15, 2026  
**Application**: Shahin GRC Platform  
**Base URL**: http://localhost:5000

---

## 📊 Summary

- **Total Controllers**: 100+ controllers
- **Total API Endpoints**: 94+ endpoints
- **Total Services**: 50+ services
- **Status**: ✅ All configured and ready for testing

---

## 🔐 Authentication & Authorization APIs

### AuthController (`/api/auth`)
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/auth/validate` - Validate token
- `POST /api/auth/user` - Get current user
- `POST /api/auth/logout` - User logout
- `POST /api/auth/refresh` - Refresh token

### Authorization APIs
- `GET /api/auth/users/{userId}/roles` - Get user roles
- `POST /api/auth/users/{userId}/roles` - Assign role
- `DELETE /api/auth/users/{userId}/roles/{role}` - Revoke role
- `POST /api/auth/users/{userId}/permissions/check` - Check permission
- `GET /api/auth/users/{userId}/permissions` - Get permissions

---

## 📋 Core GRC APIs

### ControlApiController (`/api/controls`)
- `GET /api/controls` - List all controls (pagination, filtering)
- `GET /api/controls/{id}` - Get control by ID
- `POST /api/controls` - Create control
- `PUT /api/controls/{id}` - Update control
- `DELETE /api/controls/{id}` - Delete control
- `GET /api/controls/risk/{riskId}` - Get controls by risk
- `GET /api/controls/statistics` - Get statistics
- `PATCH /api/controls/{id}` - Partial update
- `POST /api/controls/bulk` - Bulk create

### EvidenceApiController (`/api/evidence`)
- `GET /api/evidence` - List all evidence
- `GET /api/evidence/{id}` - Get evidence by ID
- `POST /api/evidence` - Create evidence
- `PUT /api/evidence/{id}` - Update evidence
- `DELETE /api/evidence/{id}` - Delete evidence
- `GET /api/evidence/control/{controlId}` - Get by control
- `GET /api/evidence/assessment/{assessmentId}` - Get by assessment
- `POST /api/evidence/upload` - Upload files
- `PATCH /api/evidence/{id}` - Partial update

### RiskApiController (`/api/risks`)
- `GET /api/risks` - List all risks
- `GET /api/risks/{id}` - Get risk by ID
- `POST /api/risks` - Create risk
- `PUT /api/risks/{id}` - Update risk
- `DELETE /api/risks/{id}` - Delete risk
- `GET /api/risks/assessment/{assessmentId}` - Get by assessment
- `GET /api/risks/statistics` - Get statistics
- `POST /api/risks/{id}/mitigate` - Mitigate risk
- `PATCH /api/risks/{id}` - Partial update

### PlansApiController (`/api/plans`)
- `GET /api/plans` - List all plans
- `GET /api/plans/{id}` - Get plan by ID
- `POST /api/plans` - Create plan
- `PUT /api/plans/{id}` - Update plan
- `DELETE /api/plans/{id}` - Delete plan
- `GET /api/plans/{id}/phases` - Get plan phases
- `POST /api/plans/{id}/phases` - Add phase
- `GET /api/plans/{id}/progress` - Get progress

### DashboardApiController (`/api/dashboard`)
- `GET /api/dashboard/metrics` - Get metrics
- `GET /api/dashboard/charts` - Get chart data
- `GET /api/dashboard/reports` - Get reports
- `GET /api/dashboard/analytics` - Get analytics
- `GET /api/dashboard/compliance-status` - Get compliance status
- `GET /api/dashboard/risk-summary` - Get risk summary

---

## 🤖 AI Agent APIs

### AgentController (`/api/agents`)
- `POST /api/agents/invoke` - Invoke AI agent
- `GET /api/agents/{agentCode}` - Get agent info
- `GET /api/agents` - List all agents
- `POST /api/agents/{agentCode}/chat` - Chat with agent
- `GET /api/agents/{agentCode}/history` - Get agent history
- `POST /api/agents/{agentCode}/approve` - Approve agent action
- `POST /api/agents/{agentCode}/reject` - Reject agent action

**Available Agents:**
- SHAHIN_AI - Main assistant
- COMPLIANCE_AGENT - Compliance analysis
- RISK_AGENT - Risk assessment
- AUDIT_AGENT - Audit analysis
- POLICY_AGENT - Policy management
- ANALYTICS_AGENT - Analytics & insights
- REPORT_AGENT - Report generation
- DIAGNOSTIC_AGENT - System diagnostics
- SUPPORT_AGENT - Customer support
- WORKFLOW_AGENT - Workflow optimization
- EVIDENCE_AGENT - Evidence collection
- EMAIL_AGENT - Email classification

---

## 📧 Email Operations APIs

### EmailOperationsController (`/api/email-operations`)
- `GET /api/email-operations` - List email operations
- `GET /api/email-operations/{id}` - Get email operation
- `POST /api/email-operations` - Create email operation
- `PUT /api/email-operations/{id}` - Update email operation
- `POST /api/email-operations/{id}/assign` - Assign email
- `POST /api/email-operations/{id}/resolve` - Resolve email
- `GET /api/email-operations/stats` - Get statistics

---

## 🏢 Tenant & Workspace APIs

### WorkspaceController (`/api/workspaces`)
- `GET /api/workspaces` - List workspaces
- `GET /api/workspaces/{id}` - Get workspace
- `POST /api/workspaces` - Create workspace
- `PUT /api/workspaces/{id}` - Update workspace
- `DELETE /api/workspaces/{id}` - Delete workspace

### PlatformTenantsController (`/api/tenants`)
- `GET /api/tenants` - List tenants
- `GET /api/tenants/{id}` - Get tenant
- `POST /api/tenants` - Create tenant
- `PUT /api/tenants/{id}` - Update tenant

---

## 🔍 Assessment & Audit APIs

### Assessment APIs
- `GET /api/assessments` - List assessments
- `GET /api/assessments/{id}` - Get assessment
- `POST /api/assessments` - Create assessment
- `PUT /api/assessments/{id}` - Update assessment
- `DELETE /api/assessments/{id}` - Delete assessment
- `GET /api/assessments/stats/summary` - Get statistics

### Audit APIs
- `GET /api/audits` - List audits
- `GET /api/audits/{id}` - Get audit
- `POST /api/audits` - Create audit
- `PUT /api/audits/{id}` - Update audit
- `DELETE /api/audits/{id}` - Delete audit
- `GET /api/audits/{id}/findings` - Get findings
- `POST /api/audits/{id}/findings` - Create finding
- `GET /api/audits/stats/summary` - Get statistics

---

## 📊 Policy & Framework APIs

### PolicyApiController (`/api/policies`)
- `GET /api/policies` - List policies
- `GET /api/policies/{id}` - Get policy
- `POST /api/policies` - Create policy
- `PUT /api/policies/{id}` - Update policy
- `DELETE /api/policies/{id}` - Delete policy

### FrameworksController (`/api/frameworks`)
- `GET /api/frameworks` - List frameworks
- `GET /api/frameworks/{id}` - Get framework
- `POST /api/frameworks` - Create framework

---

## 🔄 Workflow APIs

### WorkflowController (`/api/workflows`)
- `GET /api/workflows` - List workflows
- `GET /api/workflows/{id}` - Get workflow
- `POST /api/workflows` - Create workflow
- `PUT /api/workflows/{id}` - Update workflow
- `POST /api/workflows/{id}/start` - Start workflow
- `POST /api/workflows/{id}/approve` - Approve step
- `POST /api/workflows/{id}/reject` - Reject step

---

## 👥 User Management APIs

### UsersController (`/api/users`)
- `GET /api/users` - List users
- `GET /api/users/{id}` - Get user
- `POST /api/users` - Create user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user
- `GET /api/users/{id}/profile` - Get user profile

---

## 🧪 Testing & Diagnostics APIs

### SystemTestController (`/api/system-test`)
- `GET /api/system-test/health` - Health check
- `GET /api/system-test/database` - Database test
- `GET /api/system-test/services` - Services test
- `POST /api/system-test/email` - Email test

### IntegrationHealthController (`/api/integration-health`)
- `GET /api/integration-health` - Check all integrations
- `GET /api/integration-health/claude` - Check Claude
- `GET /api/integration-health/copilot` - Check Copilot
- `GET /api/integration-health/email` - Check email

### SchemaTestController (`/api/schema-test`)
- `GET /api/schema-test/tables` - List tables
- `GET /api/schema-test/columns/{tableName}` - Get columns
- `GET /api/schema-test/foreign-keys` - Get foreign keys

---

## 📈 Analytics & Reporting APIs

### DashboardController (`/api/dashboard`)
- `GET /api/dashboard/metrics` - Get metrics
- `GET /api/dashboard/charts` - Get charts
- `GET /api/dashboard/reports` - Get reports

### KPIsController (`/api/kpis`)
- `GET /api/kpis` - List KPIs
- `GET /api/kpis/{id}` - Get KPI
- `POST /api/kpis` - Create KPI

---

## 🔗 Integration APIs

### IntegrationsController (`/api/integrations`)
- `GET /api/integrations` - List integrations
- `GET /api/integrations/{id}` - Get integration
- `POST /api/integrations` - Create integration
- `PUT /api/integrations/{id}` - Update integration

### GraphSubscriptionsController (`/api/graph-subscriptions`)
- `GET /api/graph-subscriptions` - List subscriptions
- `POST /api/graph-subscriptions` - Create subscription
- `DELETE /api/graph-subscriptions/{id}` - Delete subscription

---

## 🎯 Onboarding APIs

### OnboardingWizardApiController (`/api/onboarding`)
- `GET /api/onboarding/status` - Get onboarding status
- `POST /api/onboarding/start` - Start onboarding
- `POST /api/onboarding/complete-step` - Complete step
- `GET /api/onboarding/progress` - Get progress

---

## 🏥 Health & Monitoring

### Health Endpoints
- `GET /health` - Basic health check
- `GET /health/ready` - Readiness check
- `GET /health/live` - Liveness check

### Monitoring
- `GET /api/monitoring/metrics` - Application metrics
- `GET /api/monitoring/logs` - Application logs

---

## 📚 Documentation

### Swagger/OpenAPI
- `GET /swagger` - Swagger UI
- `GET /swagger/v1/swagger.json` - OpenAPI JSON
- `GET /api-docs` - API documentation

---

## 🔧 Services Layer

### Core Services
- `IControlService` - Control management
- `IEvidenceService` - Evidence management
- `IRiskService` - Risk management
- `IPlanService` - Plan management
- `IReportService` - Reporting
- `IAssessmentService` - Assessment management
- `IAuditService` - Audit management
- `IPolicyService` - Policy management

### AI Services
- `IClaudeAgentService` - Claude AI integration
- `ISupportAgentService` - Support agent
- `IDiagnosticAgentService` - Diagnostic agent
- `IEmailAiService` - Email AI classification

### Integration Services
- `IEmailService` - Email sending
- `IMicrosoftGraphService` - Microsoft Graph integration
- `ICopilotService` - Copilot Studio integration
- `IWorkflowService` - Workflow management

### User Services
- `IUserService` - User management
- `IRoleService` - Role management
- `IPermissionService` - Permission management
- `IAuthenticationService` - Authentication

---

## 🧪 Testing Endpoints

### Quick Test URLs
- Health: `http://localhost:5000/health/ready`
- Swagger: `http://localhost:5000/api-docs`
- API Test: `http://localhost:5000/api/system-test/health`

---

## 📝 Notes

- All API endpoints require authentication except:
  - Health checks
  - Public GET endpoints (marked with [AllowAnonymous])
  - Authentication endpoints (login/register)

- All modification endpoints (POST/PUT/PATCH/DELETE) require authorization

- Base URL: `http://localhost:5000` (or your configured port)

---

**Total**: 94+ API endpoints across 100+ controllers
