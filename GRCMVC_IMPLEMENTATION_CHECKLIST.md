# GRC MVC - Implementation Checklist

## Phase 0: Critical Prerequisites (MUST DO FIRST)

- [ ] Create database migrations
  ```bash
  cd /home/dogan/grc-system/src/GrcMvc
  dotnet ef migrations add Initial -s .
  ```
- [ ] Apply migrations to database
  ```bash
  dotnet ef database update
  ```
- [ ] Verify database creation successfully

---

## Phase 1: Core Services (Week 1)

### 1.1 Service Implementations

- [ ] Create `ControlService.cs` following RiskService pattern
  - [ ] Add to `/home/dogan/grc-system/src/GrcMvc/Services/Implementations/`
  - [ ] Implement CRUD operations
  - [ ] Implement filtering methods
  - [ ] Register in Program.cs DI container

- [ ] Create `AssessmentService.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Services/Implementations/`
  - [ ] Implement CRUD operations
  - [ ] Implement status filtering
  - [ ] Register in Program.cs DI container

- [ ] Create `AuditService.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Services/Implementations/`
  - [ ] Implement CRUD operations
  - [ ] Implement finding aggregation
  - [ ] Register in Program.cs DI container

- [ ] Create `AuditFindingService.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Services/Implementations/`
  - [ ] Implement CRUD operations
  - [ ] Implement status tracking
  - [ ] Register in Program.cs DI container

- [ ] Create `PolicyService.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Services/Implementations/`
  - [ ] Implement CRUD operations
  - [ ] Implement violation tracking
  - [ ] Register in Program.cs DI container

- [ ] Create `EvidenceService.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Services/Implementations/`
  - [ ] Implement CRUD operations
  - [ ] Implement file association
  - [ ] Register in Program.cs DI container

- [ ] Create `WorkflowService.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Services/Implementations/`
  - [ ] Implement CRUD operations
  - [ ] Implement workflow template management
  - [ ] Register in Program.cs DI container

- [ ] Create `WorkflowExecutionService.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Services/Implementations/`
  - [ ] Implement execution tracking
  - [ ] Implement state management
  - [ ] Register in Program.cs DI container

### 1.2 Validators

- [ ] Create `ControlValidators.cs` (CreateControlDtoValidator, UpdateControlDtoValidator)
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Validators/`
  - [ ] Follow RiskValidators pattern
  - [ ] Register in Program.cs

- [ ] Create `AssessmentValidators.cs` (CreateAssessmentDtoValidator, UpdateAssessmentDtoValidator)
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Validators/`
  - [ ] Register in Program.cs

- [ ] Create `AuditValidators.cs` (CreateAuditDtoValidator, UpdateAuditDtoValidator, CreateAuditFindingDtoValidator)
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Validators/`
  - [ ] Register in Program.cs

- [ ] Create `PolicyValidators.cs` (CreatePolicyDtoValidator, UpdatePolicyDtoValidator)
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Validators/`
  - [ ] Register in Program.cs

- [ ] Create `EvidenceValidators.cs` (CreateEvidenceDtoValidator)
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Validators/`
  - [ ] Register in Program.cs

- [ ] Create `WorkflowValidators.cs` (CreateWorkflowDtoValidator, CreateWorkflowExecutionDtoValidator)
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Validators/`
  - [ ] Register in Program.cs

### 1.3 Infrastructure

- [ ] Create error handling middleware
  - [ ] Create `GlobalExceptionHandlingMiddleware.cs`
    - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Middleware/`
    - [ ] Implement exception handling
    - [ ] Add to Program.cs middleware pipeline

- [ ] Configure Serilog logging
  - [ ] Add NuGet packages:
    - [ ] Serilog.AspNetCore
    - [ ] Serilog.Sinks.File
  - [ ] Update appsettings.json with Serilog configuration
  - [ ] Initialize Serilog in Program.cs

---

## Phase 2: Controllers & Core Views (Week 2)

### 2.1 Controllers

- [ ] Create `AssessmentController.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Areas/Assessment/Controllers/`
  - [ ] Implement: Index, Create, Edit, Delete, Details, Results
  - [ ] Follow RiskController pattern

- [ ] Create `AuditController.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Areas/Audit/Controllers/`
  - [ ] Implement: Index, Create, Edit, Delete, Details, Report
  - [ ] Follow RiskController pattern

- [ ] Create `AuditFindingController.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Areas/Audit/Controllers/`
  - [ ] Implement: Index, Create, Edit, Delete, Details
  - [ ] Follow RiskController pattern

- [ ] Create `PolicyController.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Areas/Policy/Controllers/`
  - [ ] Implement: Index, Create, Edit, Delete, Details, Violations
  - [ ] Follow RiskController pattern

- [ ] Create `ComplianceController.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Areas/Compliance/Controllers/`
  - [ ] Implement: Dashboard, Controls, RiskMatrix, Reports
  - [ ] Special dashboard logic

- [ ] Create `WorkflowController.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Areas/Workflow/Controllers/`
  - [ ] Implement: Index, Create, Edit, Delete, Designer, Executions
  - [ ] Follow RiskController pattern

- [ ] Create `VendorController.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Areas/Vendor/Controllers/`
  - [ ] Implement: Index, Create, Edit, Delete, Details, RiskProfile
  - [ ] Follow RiskController pattern

- [ ] Create `AdminController.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Areas/Admin/Controllers/`
  - [ ] Implement: Dashboard, Users, Roles
  - [ ] Add special admin logic

### 2.2 Core Views (Risk Area - Template)

- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Areas/Risk/Views/Risk/Index.cshtml`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Areas/Risk/Views/Risk/Create.cshtml`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Areas/Risk/Views/Risk/Edit.cshtml`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Areas/Risk/Views/Risk/Details.cshtml`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Areas/Risk/Views/Risk/Delete.cshtml`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Areas/Risk/Views/Risk/Statistics.cshtml`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Areas/Risk/Views/Shared/_RiskForm.cshtml` (Partial)

### 2.3 Assessment Area Views (6 views)

- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Areas/Assessment/Views/Assessment/Index.cshtml`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Areas/Assessment/Views/Assessment/Create.cshtml`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Areas/Assessment/Views/Assessment/Edit.cshtml`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Areas/Assessment/Views/Assessment/Details.cshtml`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Areas/Assessment/Views/Assessment/Delete.cshtml`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Areas/Assessment/Views/Assessment/Results.cshtml`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Areas/Assessment/Views/Shared/_AssessmentForm.cshtml`

---

## Phase 3: Additional Views & Features (Week 3)

### 3.1 Authentication Views

- [ ] Create `AccountController.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Controllers/`
  - [ ] Implement: Login, Register, Logout, ForgotPassword, ResetPassword

- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Views/Account/Login.cshtml`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Views/Account/Register.cshtml`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Views/Account/ForgotPassword.cshtml`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Views/Account/ResetPassword.cshtml`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Views/Account/AccessDenied.cshtml`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Views/Account/Shared/_LoginForm.cshtml`

- [ ] Create `AuthenticationDtos.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Models/DTOs/`
  - [ ] Include: LoginDto, RegisterDto, ChangePasswordDto, ForgotPasswordDto, ResetPasswordDto

### 3.2 Remaining Area Views

- [ ] Audit Area Views (10 views)
  - [ ] AuditController views (6)
  - [ ] AuditFindingController views (4)

- [ ] Policy Area Views (6 views)
  - [ ] Index, Create, Edit, Delete, Details, Violations

- [ ] Workflow Area Views (6 views)
  - [ ] Index, Create, Edit, Delete, Designer, Executions

- [ ] Compliance Area Views (4 views)
  - [ ] Dashboard, Controls, RiskMatrix, Reports

- [ ] Vendor Area Views (6 views)
  - [ ] Index, Create, Edit, Delete, Details, RiskProfile

- [ ] Admin Area Views (11 views)
  - [ ] Admin Dashboard (1)
  - [ ] User Management (5)
  - [ ] Role Management (4)
  - [ ] Settings (1)

### 3.3 Additional Services

- [ ] Create `EmailService.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Services/Implementations/`
  - [ ] Implement email sending via SMTP
  - [ ] Add to Program.cs DI container

- [ ] Create `IEmailService.cs` interface
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Services/Interfaces/`

- [ ] Create `EmailSettings.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Configuration/`

- [ ] Create email templates folder
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/EmailTemplates/`
  - [ ] Create: WelcomeEmail.html, PasswordResetEmail.html, etc.

- [ ] Create `NotificationService.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Services/Implementations/`
  - [ ] Implement in-app notifications

- [ ] Create `Notification.cs` entity
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Models/Entities/`
  - [ ] Add DbSet to GrcDbContext

- [ ] Create `NotificationDtos.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Models/DTOs/`

---

## Phase 4: Dashboard & Navigation (Week 4)

### 4.1 Dashboard

- [ ] Create `DashboardController.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Controllers/`
  - [ ] Implement: Index, Executive, Compliance, Operations

- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Views/Dashboard/Index.cshtml`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Views/Dashboard/Executive.cshtml`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Views/Dashboard/Compliance.cshtml`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Views/Dashboard/Operations.cshtml`

- [ ] Create dashboard widget partials
  - [ ] `/home/dogan/grc-system/src/GrcMvc/Views/Dashboard/Shared/_RiskWidget.cshtml`
  - [ ] `/home/dogan/grc-system/src/GrcMvc/Views/Dashboard/Shared/_ComplianceWidget.cshtml`
  - [ ] `/home/dogan/grc-system/src/GrcMvc/Views/Dashboard/Shared/_AuditWidget.cshtml`
  - [ ] `/home/dogan/grc-system/src/GrcMvc/Views/Dashboard/Shared/_PolicyWidget.cshtml`

- [ ] Create `DashboardDtos.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Models/DTOs/`

### 4.2 Navigation & Layout

- [ ] Update `_Layout.cshtml`
  - [ ] Add header with navigation
  - [ ] Add user menu
  - [ ] Add navigation bar

- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Views/Shared/_Header.cshtml`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Views/Shared/_Sidebar.cshtml`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Views/Shared/_Breadcrumb.cshtml`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/Views/Shared/_Navigation.cshtml`

- [ ] Create `NavigationMenuViewModel.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Models/ViewModels/`

- [ ] Create `INavigationService.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Services/Interfaces/`

- [ ] Create `NavigationService.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Services/Implementations/`

### 4.3 Frontend Assets

- [ ] Create custom CSS files
  - [ ] `/home/dogan/grc-system/src/GrcMvc/wwwroot/css/dashboard.css`
  - [ ] `/home/dogan/grc-system/src/GrcMvc/wwwroot/css/forms.css`
  - [ ] `/home/dogan/grc-system/src/GrcMvc/wwwroot/css/tables.css`
  - [ ] `/home/dogan/grc-system/src/GrcMvc/wwwroot/css/modals.css`
  - [ ] `/home/dogan/grc-system/src/GrcMvc/wwwroot/css/charts.css`

- [ ] Create custom JavaScript files
  - [ ] `/home/dogan/grc-system/src/GrcMvc/wwwroot/js/dashboard.js`
  - [ ] `/home/dogan/grc-system/src/GrcMvc/wwwroot/js/forms.js`
  - [ ] `/home/dogan/grc-system/src/GrcMvc/wwwroot/js/tables.js`
  - [ ] `/home/dogan/grc-system/src/GrcMvc/wwwroot/js/modals.js`
  - [ ] `/home/dogan/grc-system/src/GrcMvc/wwwroot/js/notifications.js`
  - [ ] `/home/dogan/grc-system/src/GrcMvc/wwwroot/js/charts.js`
  - [ ] `/home/dogan/grc-system/src/GrcMvc/wwwroot/js/app.js`

### 4.4 Advanced Services

- [ ] Create `ReportService.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Services/Implementations/`

- [ ] Create `PdfGeneratorService.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Services/Implementations/`
  - [ ] Add iTextSharp NuGet package

- [ ] Create `ExcelExportService.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Services/Implementations/`
  - [ ] Add EPPlus NuGet package

- [ ] Create `ReportController.cs`
  - [ ] Location: `/home/dogan/grc-system/src/GrcMvc/Controllers/`

- [ ] Create health check endpoints
  - [ ] Add health check NuGet package
  - [ ] Configure in Program.cs
  - [ ] Add health check controller

---

## Phase 5: Testing & Documentation (Week 5)

### 5.1 Unit Tests

- [ ] Create test project
  - [ ] Create `/home/dogan/grc-system/test/GrcMvc.Tests/GrcMvc.Tests.csproj`

- [ ] Create service tests
  - [ ] RiskServiceTests.cs
  - [ ] ControlServiceTests.cs
  - [ ] AssessmentServiceTests.cs
  - [ ] AuditServiceTests.cs
  - [ ] PolicyServiceTests.cs

- [ ] Create controller tests
  - [ ] RiskControllerTests.cs
  - [ ] AssessmentControllerTests.cs

- [ ] Create validator tests
  - [ ] RiskValidatorTests.cs

- [ ] Create fixtures
  - [ ] DbContextFixture.cs
  - [ ] RiskFixtures.cs

### 5.2 Integration Tests

- [ ] Create integration test suite
  - [ ] RiskIntegrationTests.cs
  - [ ] AssessmentIntegrationTests.cs
  - [ ] AuthenticationIntegrationTests.cs
  - [ ] DatabaseMigrationTests.cs
  - [ ] EndToEndTests.cs

### 5.3 API Documentation

- [ ] Add Swashbuckle.AspNetCore NuGet package
- [ ] Configure Swagger in Program.cs
- [ ] Add XML documentation comments to all controllers
- [ ] Add XML documentation comments to all DTOs
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/docs/API.md`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/docs/AUTHENTICATION.md`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/docs/ENDPOINTS.md`
- [ ] Create `/home/dogan/grc-system/src/GrcMvc/docs/ERRORS.md`

---

## Phase 6: Optional Enhancements

### 6.1 Caching

- [ ] Create `ICacheService.cs`
- [ ] Create `CacheService.cs`
- [ ] Add caching configuration to appsettings.json
- [ ] Add Microsoft.Extensions.Caching.StackExchangeRedis NuGet package
- [ ] Implement Redis caching for frequently accessed data

### 6.2 Rate Limiting

- [ ] Add Microsoft.AspNetCore.RateLimiting NuGet package
- [ ] Configure rate limiting in Program.cs
- [ ] Apply rate limiting attributes to API controllers

### 6.3 Security Headers

- [ ] Create `SecurityHeadersMiddleware.cs`
- [ ] Add to Program.cs middleware pipeline
- [ ] Configure CORS policies in Program.cs

### 6.4 Advanced Features

- [ ] Background jobs (Hangfire)
- [ ] Search service
- [ ] Bulk import/export
- [ ] Approval workflow
- [ ] Advanced reporting

---

## Phase 7: Deployment Preparation

- [ ] Verify all tests pass
- [ ] Run code quality checks
- [ ] Update appsettings files for production
- [ ] Create deployment documentation
- [ ] Setup CI/CD pipeline
- [ ] Prepare database backup strategy
- [ ] Create disaster recovery plan

---

## Completion Tracking

**Overall Progress:**
- [ ] Phase 0: Prerequisites (0% → 100%)
- [ ] Phase 1: Core Services (0% → 100%)
- [ ] Phase 2: Controllers & Core Views (0% → 100%)
- [ ] Phase 3: Additional Views & Features (0% → 100%)
- [ ] Phase 4: Dashboard & Navigation (0% → 100%)
- [ ] Phase 5: Testing & Documentation (0% → 100%)
- [ ] Phase 6: Optional Enhancements (0% → 100%)
- [ ] Phase 7: Deployment (0% → 100%)

---

## Notes

- Use RiskService as template for other service implementations
- Use RiskController as template for other controllers
- Use Risk area views as template for other area views
- Ensure all services are registered in Program.cs DI container
- Run migrations after schema changes
- Test each component as it's completed
- Update this checklist as progress is made

---

## Critical Dependencies

**Must be completed before next phase:**
- Phase 0 → Phase 1 (Migrations must exist)
- Phase 1 → Phase 2 (Services must be implemented)
- Phase 2 → Phase 3 (Controllers must exist)
- Phase 3 → Phase 4 (Views must exist)
- Phase 4 → Phase 5 (All features must be functional)

