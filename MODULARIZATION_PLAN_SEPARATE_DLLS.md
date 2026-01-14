# Modularization Plan: Separate DLL Architecture
## Complete Analysis & Extraction Strategy

**Version:** 1.0  
**Date:** January 12, 2026  
**Target:** Multi-Modular Pluggable Architecture using Separate DLLs  
**Scope:** All Modules

---

## 📋 Table of Contents

1. [Executive Summary](#executive-summary)
2. [Current Architecture Analysis](#current-architecture-analysis)
3. [Module Identification & Boundaries](#module-identification--boundaries)
4. [Module Extraction Strategy](#module-extraction-strategy)
5. [Module Contracts & Interfaces](#module-contracts--interfaces)
6. [Runtime Loading Mechanism](#runtime-loading-mechanism)
7. [Migration Steps](#migration-steps)
8. [Layer-by-Layer Breakdown](#layer-by-layer-breakdown)
9. [Function-by-Function Mapping](#function-by-function-mapping)
10. [Dependency Management](#dependency-management)
11. [Testing Strategy](#testing-strategy)
12. [Deployment Considerations](#deployment-considerations)

---

## 1. Executive Summary

### Goal
Transform the single monolithic MVC application (`GrcMvc`) into a **pluggable multi-module architecture** where each domain module is:
- **Separate DLL assembly** (.dll)
- **Discoverable at runtime** via module loader
- **Self-contained** (Entities, DTOs, Services, Controllers, Views)
- **ABP-compatible** (maintains ABP integration)
- **Single deployment** (all DLLs bundled, not dynamically downloaded)

### Benefits
✅ **Isolated Development**: Teams can work on modules independently  
✅ **Pluggable Architecture**: Modules can be enabled/disabled per tenant  
✅ **Testability**: Each module can be tested in isolation  
✅ **Maintainability**: Clear boundaries reduce coupling  
✅ **Scalability**: Add new modules without modifying core  

### Approach
- **No breaking changes**: Core functionality remains intact
- **Incremental migration**: Extract one module at a time
- **Backward compatible**: Existing code continues to work
- **ABP preserved**: All ABP features continue to function

---

## 2. Current Architecture Analysis

### 2.1 Current Structure (Single MVC App)

```
GrcMvc/
├── Controllers/          # 90+ controller files
│   ├── RiskController.cs
│   ├── ControlController.cs
│   ├── AssessmentController.cs
│   ├── AuditController.cs
│   ├── EvidenceController.cs
│   ├── PolicyController.cs
│   ├── WorkflowController.cs
│   ├── Admin/
│   └── Api/
├── Services/
│   ├── Interfaces/      # 115 interface files
│   └── Implementations/ # 132 service files
├── Models/
│   ├── Entities/        # 100+ entity models
│   ├── DTOs/           # 38 DTO files
│   └── ViewModels/
├── Data/
│   ├── GrcDbContext.cs
│   ├── Repositories/
│   ├── IUnitOfWork.cs
│   └── Migrations/
├── Views/               # 373 Razor view files
│   ├── Risk/
│   ├── Control/
│   ├── Assessment/
│   └── ...
├── Areas/               # Some areas exist but incomplete
│   ├── Risk/
│   ├── Audit/
│   └── ...
└── Program.cs          # 1,749 lines - massive DI configuration
```

### 2.2 Key Components Analysis

#### A) Dependency Injection (Program.cs)
- **132+ services** registered via `AddScoped<TInterface, TImplementation>`
- **Autofac** as primary DI container (ABP integration)
- **Cross-cutting services**: `ITenantContextService`, `IWorkspaceContextService`, `IUnitOfWork`
- **ABP services**: `IFeatureCheckService`, `IPermissionChecker`, `ICurrentTenant`

#### B) Data Layer (GrcDbContext.cs)
- **230+ DbSets** (one per entity)
- **Global query filters** for `TenantId` and `WorkspaceId`
- **Single database context** (PostgreSQL)
- **96 migrations** already applied

#### C) Entity Layer (BaseEntity)
- **All entities inherit** from `BaseEntity`
- **Common properties**: `Id`, `TenantId`, `CreatedDate`, `ModifiedDate`, `IsDeleted`, `BusinessCode`
- **Governance metadata**: `Owner`, `DataClassification`, `LabelsJson`
- **Implements `IGovernedResource`**

#### D) Service Layer Dependencies
```
Controller → Service → IUnitOfWork → GenericRepository<T> → DbContext
                    → Cross-cutting services (Tenant, Workspace, Auth)
                    → Other domain services (Risk → Control, Evidence → Control)
```

### 2.3 Cross-Cutting Concerns

| Concern | Implementation | Module Dependency |
|---------|---------------|-------------------|
| **Multi-Tenancy** | `ITenantContextService`, `TenantId` in `BaseEntity` | **ALL** modules depend |
| **Workspace Context** | `IWorkspaceContextService`, `WorkspaceId` in entities | **ALL** modules depend |
| **Authentication** | ASP.NET Core Identity + JWT | **ALL** controllers |
| **Authorization** | ABP Permissions + RBAC | **ALL** services |
| **Audit Logging** | ABP Audit Logging | **ALL** write operations |
| **Feature Flags** | ABP Feature Management | **ALL** modules |
| **Serial Codes** | `ISerialCodeService` (6-stage GRC codes) | **ALL** entities |
| **Background Jobs** | Hangfire | Workflow, Notifications, Escalation |
| **Messaging** | MassTransit (Kafka/RabbitMQ) | Events, Integration |
| **AI Integration** | Claude AI, LLM Services | Assessment, Dashboard, Onboarding |

---

## 3. Module Identification & Boundaries

### 3.1 Proposed Module Structure

Based on domain boundaries and current controller/service organization, here are the identified modules:

| Module ID | Module Name | Controllers | Services | Entities | Views |
|-----------|-------------|-------------|----------|----------|-------|
| **Core** | `GrcMvc.Core` | HomeController, AccountController | Core services (Tenant, Workspace, SerialCode) | BaseEntity, Tenant, Workspace | Shared, Home, Account |
| **Risk** | `GrcMvc.Modules.Risk` | RiskController, RiskApiController | IRiskService, RiskService | Risk, RiskControlMapping, RiskAppetiteSetting | Risk/ |
| **Control** | `GrcMvc.Modules.Control` | ControlController, ControlApiController | IControlService, ControlService | Control, ControlOwner | Control/ |
| **Assessment** | `GrcMvc.Modules.Assessment` | AssessmentController, AssessmentApiController, AssessmentExecutionController | IAssessmentService, AssessmentService | Assessment, AssessmentResult | Assessment/ |
| **Audit** | `GrcMvc.Modules.Audit` | AuditController, AuditApiController | IAuditService, AuditService | Audit, AuditFinding | Audit/ |
| **Evidence** | `GrcMvc.Modules.Evidence` | EvidenceController, EvidenceApiController | IEvidenceService, EvidenceService | Evidence | Evidence/ |
| **Policy** | `GrcMvc.Modules.Policy` | PolicyController, PolicyApiController | IPolicyService, PolicyService | Policy, PolicyViolation | Policy/ |
| **Workflow** | `GrcMvc.Modules.Workflow` | WorkflowController, WorkflowUIController, WorkflowsController | IWorkflowService, IEscalationService | Workflow, WorkflowInstance, WorkflowTask | Workflow/ |
| **ActionPlan** | `GrcMvc.Modules.ActionPlan` | ActionPlansController | IActionPlanService, ActionPlanService | ActionPlan | ActionPlan/ |
| **Vendor** | `GrcMvc.Modules.Vendor` | VendorsController | IVendorService, VendorService | Vendor, VendorAssessment | Vendor/ |
| **Compliance** | `GrcMvc.Modules.Compliance` | FrameworksController, RegulatorsController, ComplianceCalendarController | IFrameworkService, IRegulatorService | Framework, FrameworkControl, Regulator, ComplianceEvent | Framework/, Regulator/, Compliance/ |
| **Dashboard** | `GrcMvc.Modules.Dashboard` | DashboardController, AnalyticsController, MonitoringDashboardController | IDashboardService, IAnalyticsService | Dashboard, KPI | Dashboard/, Analytics/ |
| **Onboarding** | `GrcMvc.Modules.Onboarding` | OnboardingController, OnboardingWizardController | IOnboardingService, OnboardingService | OnboardingWizard, OrganizationProfile | Onboarding/, OnboardingWizard/ |
| **Admin** | `GrcMvc.Modules.Admin` | AdminController, AdminPortalController, PlatformAdminControllerV2, TenantAdminController | ITenantService, IAdminCatalogService | TenantUser, TenantBaseline | Admin/, AdminPortal/, TenantAdmin/ |
| **AI** | `GrcMvc.Modules.AI` | ShahinAIController, AgentController | IClaudeAgentService, IDiagnosticAgentService, IShahinAIOrchestrationService | AgentOperatingModel, AiProviderConfiguration | AI/, Agent/ |
| **Integration** | `GrcMvc.Modules.Integration` | IntegrationsController, EmailOperationsController | IIntegrationService, IWebhookService | Integration | Integration/ |
| **Notification** | `GrcMvc.Modules.Notification` | NotificationsController | INotificationService, ISmtpEmailService | Notification | Notification/ |
| **Infrastructure** | `GrcMvc.Infrastructure` | - | Background jobs, Messaging, Caching, Logging | - | - |

### 3.2 Module Dependencies Graph

```
┌─────────────────────────────────────────────────────────────┐
│                    Infrastructure Module                     │
│  (Hangfire, MassTransit, Redis, Serilog, ABP Core)          │
└─────────────────────────────────────────────────────────────┘
                            ▲
                            │
        ┌───────────────────┴───────────────────┐
        │                                       │
┌───────┴────────┐                    ┌────────┴────────┐
│   Core Module  │                    │   AI Module     │
│ (BaseEntity,   │                    │  (Agents, LLM)  │
│  Tenant,       │                    │                 │
│  Workspace)    │                    │                 │
└───────┬────────┘                    └────────┬────────┘
        │                                       │
        │         ┌─────────────────────────────┘
        │         │
        ▼         ▼
┌─────────────────────────────────────────────────────────────┐
│                   Domain Modules                            │
│  Risk → Control → Assessment → Audit → Evidence → Policy   │
│  Workflow → ActionPlan → Vendor → Compliance → Dashboard   │
│  Onboarding → Admin → Integration → Notification            │
└─────────────────────────────────────────────────────────────┘
```

### 3.3 Module Loading Order

1. **Infrastructure** (loads first - no dependencies)
2. **Core** (depends on Infrastructure)
3. **AI** (depends on Infrastructure)
4. **Domain Modules** (can load in parallel, depend on Core)
5. **Admin** (depends on Core + Domain modules)

---

## 4. Module Extraction Strategy

### 4.1 Module Assembly Structure

Each module will be a **separate .NET 8.0 class library** with the following structure:

```
GrcMvc.Modules.{ModuleName}/
├── GrcMvc.Modules.{ModuleName}.csproj
├── Controllers/
│   └── {Module}Controller.cs
├── Services/
│   ├── Interfaces/
│   │   └── I{Module}Service.cs
│   └── Implementations/
│       └── {Module}Service.cs
├── Models/
│   ├── Entities/
│   │   └── {Entity}.cs
│   └── DTOs/
│       ├── {Entity}Dto.cs
│       ├── Create{Entity}Dto.cs
│       └── Update{Entity}Dto.cs
├── Data/
│   ├── {Module}DbContextExtensions.cs
│   └── Migrations/ (optional - can stay in main DbContext)
├── Views/
│   └── {Module}/
│       ├── Index.cshtml
│       ├── Create.cshtml
│       └── ...
├── Mappings/
│   └── {Module}Profile.cs (AutoMapper)
└── GrcMvc.Modules.{ModuleName}Module.cs
```

### 4.2 Module Contract (Interface)

Every module **MUST** implement this contract:

```csharp
namespace GrcMvc.Modules
{
    /// <summary>
    /// Module contract that every GRC module must implement
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// Module identifier (unique)
        /// </summary>
        string ModuleId { get; }

        /// <summary>
        /// Module name (human-readable)
        /// </summary>
        string ModuleName { get; }

        /// <summary>
        /// Module version
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Module dependencies (other module IDs this module requires)
        /// </summary>
        string[] Dependencies { get; }

        /// <summary>
        /// Initialize module - register services with DI container
        /// </summary>
        void ConfigureServices(IServiceCollection services, IConfiguration configuration);

        /// <summary>
        /// Configure module - register routes, middleware, etc.
        /// </summary>
        void Configure(IApplicationBuilder app, IWebHostEnvironment env);

        /// <summary>
        /// Register module's DbSets with main DbContext
        /// </summary>
        void ConfigureDbContext(ModelBuilder modelBuilder);

        /// <summary>
        /// Register module's AutoMapper profiles
        /// </summary>
        void ConfigureAutoMapper(IMapperConfigurationExpression config);

        /// <summary>
        /// Get module's required permissions
        /// </summary>
        PermissionDefinition[] GetPermissions();

        /// <summary>
        /// Get module's menu items (for navigation)
        /// </summary>
        MenuItemDefinition[] GetMenuItems();

        /// <summary>
        /// Module enabled check (can be feature-gated per tenant)
        /// </summary>
        bool IsEnabled(Guid? tenantId);
    }
}
```

### 4.3 Example: Risk Module Implementation

```csharp
namespace GrcMvc.Modules.Risk
{
    public class RiskModule : IModule
    {
        public string ModuleId => "GrcMvc.Modules.Risk";
        public string ModuleName => "Risk Management";
        public string Version => "1.0.0";
        public string[] Dependencies => new[] { "GrcMvc.Core" };

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Register Risk module services
            services.AddScoped<IRiskService, RiskService>();
            services.AddScoped<IRiskAppetiteService, RiskAppetiteService>();
            services.AddScoped<IRiskControlMappingService, RiskControlMappingService>();

            // Register validators
            services.AddScoped<IValidator<CreateRiskDto>, CreateRiskDtoValidator>();
            services.AddScoped<IValidator<UpdateRiskDto>, UpdateRiskDtoValidator>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Register routes
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "Risk",
                    pattern: "Risk/{action=Index}/{id?}",
                    defaults: new { controller = "Risk", area = "Risk" }
                );
            });
        }

        public void ConfigureDbContext(ModelBuilder modelBuilder)
        {
            // Configure Risk entities
            modelBuilder.Entity<Risk>(entity =>
            {
                entity.ToTable("Risks");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => e.WorkspaceId);
                entity.HasIndex(e => e.BusinessCode).IsUnique();
                
                // Relationships
                entity.HasMany(r => r.RiskControlMappings)
                      .WithOne(rc => rc.Risk)
                      .HasForeignKey(rc => rc.RiskId);
            });

            modelBuilder.Entity<RiskControlMapping>(entity =>
            {
                entity.ToTable("RiskControlMappings");
                entity.HasKey(e => new { e.RiskId, e.ControlId });
            });

            modelBuilder.Entity<RiskAppetiteSetting>(entity =>
            {
                entity.ToTable("RiskAppetiteSettings");
                entity.HasKey(e => e.Id);
            });
        }

        public void ConfigureAutoMapper(IMapperConfigurationExpression config)
        {
            config.CreateMap<Risk, RiskDto>();
            config.CreateMap<CreateRiskDto, Risk>();
            config.CreateMap<UpdateRiskDto, Risk>();
            config.CreateMap<RiskAppetiteSetting, RiskAppetiteSettingDto>();
        }

        public PermissionDefinition[] GetPermissions()
        {
            return new[]
            {
                new PermissionDefinition
                {
                    Name = "Grc.Risk.View",
                    DisplayName = "View Risks",
                    Description = "Permission to view risks"
                },
                new PermissionDefinition
                {
                    Name = "Grc.Risk.Create",
                    DisplayName = "Create Risks",
                    Description = "Permission to create risks"
                },
                new PermissionDefinition
                {
                    Name = "Grc.Risk.Update",
                    DisplayName = "Update Risks",
                    Description = "Permission to update risks"
                },
                new PermissionDefinition
                {
                    Name = "Grc.Risk.Delete",
                    DisplayName = "Delete Risks",
                    Description = "Permission to delete risks"
                }
            };
        }

        public MenuItemDefinition[] GetMenuItems()
        {
            return new[]
            {
                new MenuItemDefinition
                {
                    Id = "Grc.Risk",
                    DisplayName = "إدارة المخاطر",
                    Route = "/Risk",
                    Icon = "fas fa-exclamation-triangle",
                    RequiredPermissions = new[] { "Grc.Risk.View" }
                }
            };
        }

        public bool IsEnabled(Guid? tenantId)
        {
            // Can check ABP feature flags here
            // return _featureChecker.IsEnabledAsync("RiskModule", tenantId).Result;
            return true; // Default enabled
        }
    }
}
```

---

## 5. Module Contracts & Interfaces

### 5.1 Core Module Interfaces

#### IModuleLoader
```csharp
namespace GrcMvc.Modules
{
    public interface IModuleLoader
    {
        /// <summary>
        /// Load all modules from assemblies in a directory
        /// </summary>
        Task<IEnumerable<IModule>> LoadModulesAsync(string modulesDirectory);

        /// <summary>
        /// Get loaded module by ID
        /// </summary>
        IModule? GetModule(string moduleId);

        /// <summary>
        /// Get all loaded modules
        /// </summary>
        IEnumerable<IModule> GetAllModules();

        /// <summary>
        /// Initialize modules in dependency order
        /// </summary>
        Task InitializeModulesAsync(IServiceCollection services, IConfiguration configuration);

        /// <summary>
        /// Configure modules in dependency order
        /// </summary>
        Task ConfigureModulesAsync(IApplicationBuilder app, IWebHostEnvironment env);
    }
}
```

#### IModuleRegistry
```csharp
namespace GrcMvc.Modules
{
    public interface IModuleRegistry
    {
        /// <summary>
        /// Register a module
        /// </summary>
        void Register(IModule module);

        /// <summary>
        /// Unregister a module
        /// </summary>
        void Unregister(string moduleId);

        /// <summary>
        /// Get module by ID
        /// </summary>
        IModule? GetModule(string moduleId);

        /// <summary>
        /// Get all registered modules
        /// </summary>
        IEnumerable<IModule> GetAllModules();

        /// <summary>
        /// Get modules in dependency order
        /// </summary>
        IEnumerable<IModule> GetModulesInOrder();
    }
}
```

### 5.2 Cross-Module Communication

#### IModuleEventBus
```csharp
namespace GrcMvc.Modules
{
    /// <summary>
    /// Event bus for cross-module communication
    /// Uses MassTransit under the hood
    /// </summary>
    public interface IModuleEventBus
    {
        /// <summary>
        /// Publish event to all subscribers
        /// </summary>
        Task PublishAsync<T>(T eventData, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Subscribe to event
        /// </summary>
        Task SubscribeAsync<T>(Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class;
    }
}
```

#### Domain Events (Example)
```csharp
// In GrcMvc.Modules.Risk
namespace GrcMvc.Modules.Risk.Events
{
    public class RiskCreatedEvent
    {
        public Guid RiskId { get; set; }
        public Guid TenantId { get; set; }
        public string RiskName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class RiskUpdatedEvent
    {
        public Guid RiskId { get; set; }
        public Guid TenantId { get; set; }
        public string? PreviousStatus { get; set; }
        public string? NewStatus { get; set; }
    }
}

// In GrcMvc.Modules.Control (subscriber)
namespace GrcMvc.Modules.Control.EventHandlers
{
    public class RiskCreatedEventHandler : IConsumer<RiskCreatedEvent>
    {
        private readonly IControlService _controlService;

        public RiskCreatedEventHandler(IControlService controlService)
        {
            _controlService = controlService;
        }

        public async Task Consume(ConsumeContext<RiskCreatedEvent> context)
        {
            var riskCreated = context.Message;
            // Auto-create controls for new risks if configured
            // await _controlService.AutoCreateControlsForRiskAsync(riskCreated.RiskId);
        }
    }
}
```

### 5.3 Shared Contracts

#### IModuleDbContext
```csharp
namespace GrcMvc.Modules
{
    /// <summary>
    /// Interface for module-specific DbContext extensions
    /// Modules can register their entities via this
    /// </summary>
    public interface IModuleDbContext
    {
        /// <summary>
        /// Configure module's entities in the main DbContext
        /// </summary>
        void OnModelCreating(ModelBuilder modelBuilder);
    }
}
```

#### IModuleMenuContributor
```csharp
namespace GrcMvc.Modules
{
    /// <summary>
    /// Interface for modules to contribute menu items
    /// </summary>
    public interface IModuleMenuContributor
    {
        /// <summary>
        /// Get menu items for this module
        /// </summary>
        MenuItemDefinition[] GetMenuItems();
    }
}
```

---

## 6. Runtime Loading Mechanism

### 6.1 Module Discovery

Modules are discovered at **application startup** by scanning the `Modules/` directory for `.dll` files:

```csharp
// In Program.cs
public class ModuleLoader : IModuleLoader
{
    private readonly IModuleRegistry _registry;
    private readonly ILogger<ModuleLoader> _logger;

    public ModuleLoader(IModuleRegistry registry, ILogger<ModuleLoader> logger)
    {
        _registry = registry;
        _logger = logger;
    }

    public async Task<IEnumerable<IModule>> LoadModulesAsync(string modulesDirectory)
    {
        var modules = new List<IModule>();
        var moduleFiles = Directory.GetFiles(modulesDirectory, "GrcMvc.Modules.*.dll", SearchOption.TopDirectoryOnly);

        foreach (var moduleFile in moduleFiles)
        {
            try
            {
                var assembly = Assembly.LoadFrom(moduleFile);
                var moduleTypes = assembly.GetTypes()
                    .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    .ToList();

                foreach (var moduleType in moduleTypes)
                {
                    var module = (IModule)Activator.CreateInstance(moduleType)!;
                    modules.Add(module);
                    _registry.Register(module);
                    _logger.LogInformation("Loaded module: {ModuleId} v{Version}", module.ModuleId, module.Version);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load module from {ModuleFile}", moduleFile);
            }
        }

        return modules;
    }

    public async Task InitializeModulesAsync(IServiceCollection services, IConfiguration configuration)
    {
        var modules = _registry.GetModulesInOrder(); // Dependency order

        foreach (var module in modules)
        {
            try
            {
                module.ConfigureServices(services, configuration);
                _logger.LogInformation("Initialized module: {ModuleId}", module.ModuleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize module: {ModuleId}", module.ModuleId);
                throw;
            }
        }
    }

    public async Task ConfigureModulesAsync(IApplicationBuilder app, IWebHostEnvironment env)
    {
        var modules = _registry.GetModulesInOrder();

        foreach (var module in modules)
        {
            try
            {
                module.Configure(app, env);
                _logger.LogInformation("Configured module: {ModuleId}", module.ModuleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to configure module: {ModuleId}", module.ModuleId);
                throw;
            }
        }
    }
}
```

### 6.2 Dependency Resolution

Modules are initialized in **dependency order** using topological sort:

```csharp
public class ModuleRegistry : IModuleRegistry
{
    private readonly Dictionary<string, IModule> _modules = new();
    private readonly ILogger<ModuleRegistry> _logger;

    public void Register(IModule module)
    {
        // Check dependencies exist
        foreach (var dependencyId in module.Dependencies)
        {
            if (!_modules.ContainsKey(dependencyId))
            {
                throw new InvalidOperationException(
                    $"Module {module.ModuleId} depends on {dependencyId} which is not loaded");
            }
        }

        _modules[module.ModuleId] = module;
    }

    public IEnumerable<IModule> GetModulesInOrder()
    {
        // Topological sort by dependencies
        var sorted = new List<IModule>();
        var visited = new HashSet<string>();
        var visiting = new HashSet<string>();

        void Visit(IModule module)
        {
            if (visiting.Contains(module.ModuleId))
            {
                throw new InvalidOperationException($"Circular dependency detected involving {module.ModuleId}");
            }

            if (visited.Contains(module.ModuleId))
            {
                return;
            }

            visiting.Add(module.ModuleId);

            foreach (var depId in module.Dependencies)
            {
                if (_modules.TryGetValue(depId, out var dep))
                {
                    Visit(dep);
                }
            }

            visiting.Remove(module.ModuleId);
            visited.Add(module.ModuleId);
            sorted.Add(module);
        }

        foreach (var module in _modules.Values)
        {
            if (!visited.Contains(module.ModuleId))
            {
                Visit(module);
            }
        }

        return sorted;
    }
}
```

### 6.3 Integration with Program.cs

```csharp
// In Program.cs
var builder = WebApplication.CreateBuilder(args);

// ... existing configuration ...

// Register module loader
builder.Services.AddSingleton<IModuleRegistry, ModuleRegistry>();
builder.Services.AddSingleton<IModuleLoader, ModuleLoader>();

var app = builder.Build();

// Load modules
var moduleLoader = app.Services.GetRequiredService<IModuleLoader>();
var modulesDirectory = Path.Combine(app.Environment.ContentRootPath, "Modules");
var loadedModules = await moduleLoader.LoadModulesAsync(modulesDirectory);

// Initialize modules (register services)
await moduleLoader.InitializeModulesAsync(builder.Services, builder.Configuration);

// Rebuild service provider after module initialization
var newServiceProvider = builder.Services.BuildServiceProvider();
app.Services = newServiceProvider;

// Configure modules (register routes, middleware)
await moduleLoader.ConfigureModulesAsync(app, app.Environment);

// ... rest of middleware pipeline ...
```

---

## 7. Migration Steps

### Phase 1: Preparation (Week 1)

1. ✅ **Create module infrastructure**
   - Create `GrcMvc.Modules` base project
   - Define `IModule` interface
   - Implement `IModuleLoader` and `IModuleRegistry`
   - Create `Modules/` directory structure

2. ✅ **Extract shared contracts**
   - Move `BaseEntity` to `GrcMvc.Core`
   - Move `IUnitOfWork` to `GrcMvc.Core`
   - Move `IGenericRepository<T>` to `GrcMvc.Core`
   - Create `GrcMvc.Core` project

3. ✅ **Create Infrastructure module**
   - Extract Hangfire configuration
   - Extract MassTransit configuration
   - Extract Serilog configuration
   - Create `GrcMvc.Infrastructure` project

### Phase 2: Core Module Extraction (Week 2)

1. ✅ **Extract Core module**
   - Create `GrcMvc.Modules.Core` project
   - Move `Tenant`, `Workspace`, `Team`, `TeamMember` entities
   - Move `ITenantContextService`, `IWorkspaceContextService`
   - Move `ISerialCodeService`
   - Move `HomeController`, `AccountController`
   - Update `GrcDbContext` to use module extensions

### Phase 3: First Domain Module (Week 3) - Risk

1. ✅ **Create Risk module project**
   ```
   GrcMvc.Modules.Risk/
   ├── Controllers/
   │   ├── RiskController.cs
   │   └── RiskApiController.cs
   ├── Services/
   │   ├── Interfaces/
   │   │   ├── IRiskService.cs
   │   │   ├── IRiskAppetiteService.cs
   │   │   └── IRiskControlMappingService.cs
   │   └── Implementations/
   │       ├── RiskService.cs
   │       ├── RiskAppetiteService.cs
   │       └── RiskControlMappingService.cs
   ├── Models/
   │   ├── Entities/
   │   │   ├── Risk.cs
   │   │   ├── RiskControlMapping.cs
   │   │   └── RiskAppetiteSetting.cs
   │   └── DTOs/
   │       ├── RiskDto.cs
   │       ├── CreateRiskDto.cs
   │       └── UpdateRiskDto.cs
   ├── Views/
   │   └── Risk/
   │       ├── Index.cshtml
   │       ├── Create.cshtml
   │       ├── Edit.cshtml
   │       └── Details.cshtml
   ├── Mappings/
   │   └── RiskProfile.cs
   └── RiskModule.cs
   ```

2. ✅ **Update main project references**
   - Remove Risk-related files from main project
   - Add reference to `GrcMvc.Modules.Risk`
   - Update `Program.cs` to load Risk module

3. ✅ **Test Risk module**
   - Verify controllers work
   - Verify services work
   - Verify views render
   - Verify database operations

### Phase 4: Remaining Domain Modules (Weeks 4-8)

Repeat Phase 3 for each module:

- **Week 4**: Control Module
- **Week 5**: Assessment Module
- **Week 6**: Audit + Evidence Modules
- **Week 7**: Policy + Workflow Modules
- **Week 8**: ActionPlan + Vendor + Compliance Modules

### Phase 5: Supporting Modules (Week 9)

- **Dashboard Module**
- **Onboarding Module**
- **Admin Module**
- **AI Module**
- **Integration Module**
- **Notification Module**

### Phase 6: Testing & Refinement (Week 10)

- Integration testing across modules
- Performance testing
- Documentation
- Deployment preparation

---

## 8. Layer-by-Layer Breakdown

### 8.1 Controllers Layer

#### Current State
- **90+ controller files** in `Controllers/` directory
- **Mixed concerns**: MVC controllers + API controllers
- **Direct service dependencies**: Each controller injects multiple services

#### Target State
- **Controllers stay in module assemblies**
- **Controllers registered via module configuration**
- **Routes registered per module**

#### Extraction Pattern
```csharp
// Before (in main project)
public class RiskController : Controller
{
    private readonly IRiskService _riskService;
    private readonly IControlService _controlService; // Cross-module dependency
    private readonly ITenantContextService _tenantContext; // Core dependency

    public RiskController(
        IRiskService riskService,
        IControlService controlService,
        ITenantContextService tenantContext)
    {
        _riskService = riskService;
        _controlService = controlService;
        _tenantContext = tenantContext;
    }
}

// After (in GrcMvc.Modules.Risk)
[Area("Risk")]
[Authorize(Permissions = "Grc.Risk.View")]
public class RiskController : Controller
{
    private readonly IRiskService _riskService;
    private readonly ITenantContextService _tenantContext; // Core - still available

    public RiskController(
        IRiskService riskService,
        ITenantContextService tenantContext)
    {
        _riskService = riskService;
        _tenantContext = tenantContext;
    }

    // IControlService accessed via events or cross-module service if needed
}
```

### 8.2 Services Layer

#### Current State
- **132 service implementations** in `Services/Implementations/`
- **115 service interfaces** in `Services/Interfaces/`
- **Complex dependencies**: Services depend on multiple other services
- **All registered in Program.cs**

#### Target State
- **Services stay in module assemblies**
- **Services registered via `IModule.ConfigureServices()`**
- **Cross-module dependencies via events or shared contracts**

#### Extraction Pattern
```csharp
// Before (in main project)
public class RiskService : IRiskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IControlService _controlService; // Direct dependency
    private readonly INotificationService _notificationService; // Direct dependency

    public async Task<RiskDto> CreateAsync(CreateRiskDto dto)
    {
        var risk = _mapper.Map<Risk>(dto);
        await _unitOfWork.Risks.AddAsync(risk);
        
        // Direct call to other service
        await _controlService.LinkToRiskAsync(risk.Id, dto.ControlIds);
        
        await _unitOfWork.SaveChangesAsync();
        
        // Direct call to notification service
        await _notificationService.SendAsync(new RiskCreatedNotification { RiskId = risk.Id });
        
        return _mapper.Map<RiskDto>(risk);
    }
}

// After (in GrcMvc.Modules.Risk)
public class RiskService : IRiskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IModuleEventBus _eventBus; // Cross-module communication

    public async Task<RiskDto> CreateAsync(CreateRiskDto dto)
    {
        var risk = _mapper.Map<Risk>(dto);
        await _unitOfWork.Risks.AddAsync(risk);
        await _unitOfWork.SaveChangesAsync();

        // Publish event - other modules can subscribe
        await _eventBus.PublishAsync(new RiskCreatedEvent
        {
            RiskId = risk.Id,
            TenantId = risk.TenantId!.Value,
            RiskName = risk.Name,
            CreatedAt = DateTime.UtcNow
        });

        return _mapper.Map<RiskDto>(risk);
    }
}

// In GrcMvc.Modules.Control (subscriber)
public class RiskCreatedEventHandler : IConsumer<RiskCreatedEvent>
{
    private readonly IControlService _controlService;

    public async Task Consume(ConsumeContext<RiskCreatedEvent> context)
    {
        var @event = context.Message;
        // Auto-link controls if configured
        // await _controlService.LinkToRiskAsync(@event.RiskId, ...);
    }
}
```

### 8.3 Models/Entities Layer

#### Current State
- **100+ entity files** in `Models/Entities/`
- **All inherit from `BaseEntity`**
- **All in single namespace**

#### Target State
- **Entities stay in module assemblies**
- **All still inherit from `BaseEntity`** (in Core)
- **Entities registered via `IModule.ConfigureDbContext()`**

#### Extraction Pattern
```csharp
// Before (in main project)
namespace GrcMvc.Models.Entities
{
    public class Risk : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public RiskSeverity Severity { get; set; }
        // ...
    }
}

// After (in GrcMvc.Modules.Risk)
namespace GrcMvc.Modules.Risk.Models.Entities
{
    using GrcMvc.Core.Models.Entities; // BaseEntity

    public class Risk : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public RiskSeverity Severity { get; set; }
        // ...
    }
}
```

### 8.4 Data Layer

#### Current State
- **Single `GrcDbContext`** with 230+ DbSets
- **Global query filters** for `TenantId` and `WorkspaceId`
- **96 migrations** in single Migrations folder

#### Target State
- **Single `GrcDbContext` remains** (shared database)
- **Modules extend DbContext** via `IModule.ConfigureDbContext()`
- **Migrations can stay centralized** OR modules can have their own

#### Extraction Pattern
```csharp
// Main DbContext (in GrcMvc.Core or main project)
public partial class GrcDbContext : DbContext
{
    // Core DbSets
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<Workspace> Workspaces { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply global query filters
        modelBuilder.Entity<BaseEntity>()
            .HasQueryFilter(e => !e.IsDeleted);

        // Apply tenant filter
        var tenantId = GetCurrentTenantId();
        if (tenantId.HasValue)
        {
            modelBuilder.Entity<BaseEntity>()
                .HasQueryFilter(e => e.TenantId == tenantId.Value || e.TenantId == null);
        }

        // Let modules configure their entities
        var moduleRegistry = ServiceProvider.GetRequiredService<IModuleRegistry>();
        foreach (var module in moduleRegistry.GetAllModules())
        {
            module.ConfigureDbContext(modelBuilder);
        }
    }
}

// In RiskModule.ConfigureDbContext()
public void ConfigureDbContext(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Risk>(entity =>
    {
        entity.ToTable("Risks");
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.TenantId);
        entity.HasIndex(e => e.WorkspaceId);
        entity.HasIndex(e => e.BusinessCode).IsUnique();

        // Relationships
        entity.HasMany(r => r.RiskControlMappings)
              .WithOne(rc => rc.Risk)
              .HasForeignKey(rc => rc.RiskId);
    });

    modelBuilder.Entity<RiskControlMapping>(entity =>
    {
        entity.ToTable("RiskControlMappings");
        entity.HasKey(e => new { e.RiskId, e.ControlId });
    });
}
```

### 8.5 Views Layer

#### Current State
- **373 Razor view files** in `Views/` directory
- **Organized by controller name** (Views/Risk/, Views/Control/, etc.)
- **Shared views** in `Views/Shared/`

#### Target State
- **Views stay in module assemblies** as **embedded resources**
- **Views loaded at runtime** via Razor Runtime Compilation
- **Shared views** in Core module

#### Extraction Pattern

**Option A: Embedded Resources (Recommended)**
```xml
<!-- In GrcMvc.Modules.Risk.csproj -->
<ItemGroup>
  <EmbeddedResource Include="Views\**\*.cshtml" />
</ItemGroup>
```

```csharp
// In RiskModule.Configure()
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // Register embedded views
    var assembly = Assembly.GetExecutingAssembly();
    var fileProvider = new EmbeddedFileProvider(assembly, "GrcMvc.Modules.Risk.Views");
    
    app.ConfigureApplicationPartManager(manager =>
    {
        manager.ApplicationParts.Add(
            new CompiledRazorAssemblyPart(assembly)
        );
    });

    // Or use RazorRuntimeCompilation with embedded views
    var razorEngine = app.ApplicationServices.GetRequiredService<IRazorViewEngine>();
    // Configure embedded view location expander
}
```

**Option B: Physical Files (Alternative)**
Keep views as physical files in `Modules/GrcMvc.Modules.Risk/Views/` and configure Razor to look there:

```csharp
// In Program.cs
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Add("/Modules/{2}/Views/{1}/{0}.cshtml");
    options.ViewLocationFormats.Add("/Modules/{2}/Views/Shared/{0}.cshtml");
});
```

---

## 9. Function-by-Function Mapping

### 9.1 Core Functions (Stay in Core Module)

| Function | Current Location | Target Location | Notes |
|----------|-----------------|-----------------|-------|
| `GetCurrentTenantId()` | `ITenantContextService` | `GrcMvc.Core` | Used by ALL modules |
| `GetCurrentWorkspaceId()` | `IWorkspaceContextService` | `GrcMvc.Core` | Used by ALL modules |
| `GenerateSerialCode()` | `ISerialCodeService` | `GrcMvc.Core` | Used by ALL entities |
| `SaveChangesAsync()` | `IUnitOfWork` | `GrcMvc.Core` | Transaction management |
| `GetRepository<T>()` | `IGenericRepository<T>` | `GrcMvc.Core` | Generic data access |
| `SignInAsync()` | `SignInManager` | `GrcMvc.Core` | Authentication |
| `CheckPermissionAsync()` | `IPermissionChecker` | `GrcMvc.Core` | ABP authorization |
| `IsFeatureEnabledAsync()` | `IFeatureChecker` | `GrcMvc.Core` | ABP feature flags |

### 9.2 Risk Module Functions

| Function | Current Location | Target Location | Cross-Module Dependencies |
|----------|-----------------|-----------------|---------------------------|
| `CreateRiskAsync()` | `RiskService` | `GrcMvc.Modules.Risk` | Publishes `RiskCreatedEvent` |
| `UpdateRiskAsync()` | `RiskService` | `GrcMvc.Modules.Risk` | Publishes `RiskUpdatedEvent` |
| `LinkControlsToRiskAsync()` | `RiskService` | `GrcMvc.Modules.Risk` | Depends on Control module |
| `CalculateRiskScoreAsync()` | `RiskService` | `GrcMvc.Modules.Risk` | Self-contained |
| `GetRisksByTenantAsync()` | `RiskService` | `GrcMvc.Modules.Risk` | Uses `ITenantContextService` |
| `GetRiskMatrixAsync()` | `RiskService` | `GrcMvc.Modules.Risk` | Self-contained |

### 9.3 Control Module Functions

| Function | Current Location | Target Location | Cross-Module Dependencies |
|----------|-----------------|-----------------|---------------------------|
| `CreateControlAsync()` | `ControlService` | `GrcMvc.Modules.Control` | Publishes `ControlCreatedEvent` |
| `AssignOwnerAsync()` | `ControlService` | `GrcMvc.Modules.Control` | Uses `ITenantContextService` |
| `GetControlsByRiskAsync()` | `ControlService` | `GrcMvc.Modules.Control` | Subscribes to `RiskCreatedEvent` |
| `EvaluateControlEffectivenessAsync()` | `ControlService` | `GrcMvc.Modules.Control` | Depends on Assessment module |

### 9.4 Assessment Module Functions

| Function | Current Location | Target Location | Cross-Module Dependencies |
|----------|-----------------|-----------------|---------------------------|
| `CreateAssessmentAsync()` | `AssessmentService` | `GrcMvc.Modules.Assessment` | Depends on Control, Risk modules |
| `ExecuteAssessmentAsync()` | `AssessmentService` | `GrcMvc.Modules.Assessment` | Depends on Evidence module |
| `CalculateComplianceScoreAsync()` | `AssessmentService` | `GrcMvc.Modules.Assessment` | Self-contained |
| `GenerateAssessmentReportAsync()` | `AssessmentService` | `GrcMvc.Modules.Assessment` | Depends on Report module |

### 9.5 Evidence Module Functions

| Function | Current Location | Target Location | Cross-Module Dependencies |
|----------|-----------------|-----------------|---------------------------|
| `UploadEvidenceAsync()` | `EvidenceService` | `GrcMvc.Modules.Evidence` | Uses `ITenantContextService` |
| `ValidateEvidenceAsync()` | `EvidenceService` | `GrcMvc.Modules.Evidence` | Self-contained |
| `LinkEvidenceToControlAsync()` | `EvidenceService` | `GrcMvc.Modules.Evidence` | Depends on Control module |
| `GetEvidenceByControlAsync()` | `EvidenceService` | `GrcMvc.Modules.Evidence` | Self-contained |

### 9.6 Workflow Module Functions

| Function | Current Location | Target Location | Cross-Module Dependencies |
|----------|-----------------|-----------------|---------------------------|
| `CreateWorkflowAsync()` | `WorkflowService` | `GrcMvc.Modules.Workflow` | Self-contained |
| `ExecuteWorkflowAsync()` | `WorkflowService` | `GrcMvc.Modules.Workflow` | Uses Hangfire (Infrastructure) |
| `EscalateTaskAsync()` | `EscalationService` | `GrcMvc.Modules.Workflow` | Publishes `TaskEscalatedEvent` |
| `CompleteWorkflowTaskAsync()` | `WorkflowService` | `GrcMvc.Modules.Workflow` | Depends on Notification module |

---

## 10. Dependency Management

### 10.1 Module Dependency Rules

1. **Core dependencies**: ALL modules depend on `GrcMvc.Core`
2. **Infrastructure dependencies**: Modules that need Hangfire/MassTransit depend on `GrcMvc.Infrastructure`
3. **Cross-module dependencies**: Avoid direct service dependencies; use events instead
4. **ABP dependencies**: All modules can use ABP services (injected via DI)

### 10.2 Shared Projects

Create shared projects for common contracts:

```
GrcMvc.Shared/
├── Contracts/
│   ├── IModule.cs
│   ├── IModuleLoader.cs
│   ├── IModuleRegistry.cs
│   └── IModuleEventBus.cs
├── Models/
│   ├── BaseEntity.cs
│   └── IGovernedResource.cs
└── Data/
    ├── IUnitOfWork.cs
    └── IGenericRepository.cs
```

All modules reference `GrcMvc.Shared`.

### 10.3 Project References

```
GrcMvc (main host)
├── References
│   ├── GrcMvc.Core
│   ├── GrcMvc.Infrastructure
│   ├── GrcMvc.Shared
│   └── GrcMvc.Modules.* (runtime loaded)

GrcMvc.Modules.Risk
├── References
│   ├── GrcMvc.Core
│   ├── GrcMvc.Shared
│   └── Microsoft.AspNetCore.Mvc.Core

GrcMvc.Modules.Control
├── References
│   ├── GrcMvc.Core
│   ├── GrcMvc.Shared
│   └── Microsoft.AspNetCore.Mvc.Core
```

### 10.4 NuGet Package Dependencies

| Package | Used By | Strategy |
|---------|---------|----------|
| **EntityFrameworkCore** | All modules | Reference in Core, modules use via DI |
| **AutoMapper** | All modules | Reference in Shared |
| **FluentValidation** | All modules | Reference in Shared |
| **MassTransit** | Infrastructure + modules that publish events | Reference in Infrastructure |
| **Hangfire** | Infrastructure + Workflow module | Reference in Infrastructure |
| **ABP packages** | All modules | Reference in Core, available via DI |

---

## 11. Testing Strategy

### 11.1 Unit Tests

Each module should have its own test project:

```
GrcMvc.Modules.Risk.Tests/
├── Services/
│   ├── RiskServiceTests.cs
│   └── RiskAppetiteServiceTests.cs
├── Controllers/
│   └── RiskControllerTests.cs
└── Mappings/
    └── RiskProfileTests.cs
```

### 11.2 Integration Tests

Test module interactions:

```csharp
// GrcMvc.Tests.Integration
public class RiskControlIntegrationTests
{
    [Fact]
    public async Task RiskCreatedEvent_ShouldTriggerControlLinking()
    {
        // Arrange
        var riskService = ServiceProvider.GetRequiredService<IRiskService>();
        var controlService = ServiceProvider.GetRequiredService<IControlService>();

        // Act
        var risk = await riskService.CreateAsync(new CreateRiskDto { /* ... */ });

        // Assert
        // Wait for event to be processed
        await Task.Delay(1000);

        var controls = await controlService.GetControlsByRiskAsync(risk.Id);
        Assert.NotEmpty(controls);
    }
}
```

### 11.3 Module Loading Tests

```csharp
[Fact]
public async Task ModuleLoader_ShouldLoadAllModules()
{
    // Arrange
    var loader = new ModuleLoader(registry, logger);
    var modulesDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "Modules");

    // Act
    var modules = await loader.LoadModulesAsync(modulesDir);

    // Assert
    Assert.Contains(modules, m => m.ModuleId == "GrcMvc.Modules.Risk");
    Assert.Contains(modules, m => m.ModuleId == "GrcMvc.Modules.Control");
    // ...
}

[Fact]
public async Task ModuleLoader_ShouldInitializeInDependencyOrder()
{
    // Arrange
    var loader = new ModuleLoader(registry, logger);
    var initOrder = new List<string>();

    // Mock modules that record init order
    // ...

    // Act
    await loader.InitializeModulesAsync(services, configuration);

    // Assert
    // Core should initialize before Risk
    Assert.True(initOrder.IndexOf("GrcMvc.Core") < initOrder.IndexOf("GrcMvc.Modules.Risk"));
}
```

---

## 12. Deployment Considerations

### 12.1 Build Process

Update `.csproj` to copy module DLLs to output:

```xml
<!-- In main GrcMvc.csproj -->
<Target Name="CopyModules" AfterTargets="Build">
  <ItemGroup>
    <ModuleProjects Include="..\Modules\**\*.csproj" />
  </ItemGroup>
  
  <MSBuild Projects="@(ModuleProjects)" Properties="Configuration=$(Configuration)" />
  
  <ItemGroup>
    <ModuleDlls Include="..\Modules\**\bin\$(Configuration)\net8.0\GrcMvc.Modules.*.dll" />
  </ItemGroup>
  
  <Copy SourceFiles="@(ModuleDlls)" 
        DestinationFolder="$(OutputPath)Modules\" 
        SkipUnchangedFiles="true" />
</Target>
```

### 12.2 Publishing

```xml
<Target Name="PublishModules" AfterTargets="Publish">
  <ItemGroup>
    <ModuleProjects Include="..\Modules\**\*.csproj" />
  </ItemGroup>
  
  <MSBuild Projects="@(ModuleProjects)" 
           Targets="Publish" 
           Properties="Configuration=$(Configuration);PublishDir=$(PublishDir)Modules\" />
</Target>
```

### 12.3 Docker Deployment

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution
COPY ["src/GrcMvc/GrcMvc.csproj", "src/GrcMvc/"]
COPY ["src/Modules/*/*.csproj", "src/Modules/"]
RUN dotnet restore "src/GrcMvc/GrcMvc.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/src/GrcMvc"
RUN dotnet build "GrcMvc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GrcMvc.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /app/publish/Modules ./Modules
ENTRYPOINT ["dotnet", "GrcMvc.dll"]
```

### 12.4 Runtime Discovery

Modules are discovered at startup from `Modules/` directory relative to executable:

```csharp
var modulesDirectory = Path.Combine(
    AppContext.BaseDirectory,
    "Modules"
);

if (!Directory.Exists(modulesDirectory))
{
    Directory.CreateDirectory(modulesDirectory);
}

var modules = await moduleLoader.LoadModulesAsync(modulesDirectory);
```

---

## 13. Function Transfer Checklist

### For Each Module:

- [ ] **Entities**
  - [ ] Move entity files to module project
  - [ ] Ensure `BaseEntity` reference works
  - [ ] Update namespaces
  - [ ] Register entities in `ConfigureDbContext()`

- [ ] **DTOs**
  - [ ] Move DTO files to module project
  - [ ] Update namespaces
  - [ ] Create validators

- [ ] **Services**
  - [ ] Move service interfaces to module project
  - [ ] Move service implementations to module project
  - [ ] Replace direct dependencies with events
  - [ ] Register services in `ConfigureServices()`

- [ ] **Controllers**
  - [ ] Move controller files to module project
  - [ ] Add `[Area]` attribute
  - [ ] Update authorization attributes
  - [ ] Register routes in `Configure()`

- [ ] **Views**
  - [ ] Move view files to module project
  - [ ] Configure as embedded resources OR physical files
  - [ ] Update view imports

- [ ] **Mappings**
  - [ ] Move AutoMapper profiles to module project
  - [ ] Register in `ConfigureAutoMapper()`

- [ ] **Permissions**
  - [ ] Define permissions in `GetPermissions()`
  - [ ] Update authorization checks

- [ ] **Menu Items**
  - [ ] Define menu items in `GetMenuItems()`

- [ ] **Events**
  - [ ] Define domain events
  - [ ] Publish events from services
  - [ ] Subscribe to events from other modules (if needed)

- [ ] **Tests**
  - [ ] Create test project for module
  - [ ] Write unit tests
  - [ ] Write integration tests

---

## 14. Example: Complete Risk Module Implementation

See `RISK_MODULE_EXAMPLE.md` for a complete, working example of the Risk module extraction.

---

## 15. Migration Timeline

| Phase | Duration | Modules | Status |
|-------|----------|---------|--------|
| **Phase 1: Infrastructure** | Week 1 | Core, Infrastructure, Shared | ⏳ Pending |
| **Phase 2: Core Module** | Week 2 | Core | ⏳ Pending |
| **Phase 3: Risk Module** | Week 3 | Risk | ⏳ Pending |
| **Phase 4: Control Module** | Week 4 | Control | ⏳ Pending |
| **Phase 5: Assessment Module** | Week 5 | Assessment | ⏳ Pending |
| **Phase 6: Audit + Evidence** | Week 6 | Audit, Evidence | ⏳ Pending |
| **Phase 7: Policy + Workflow** | Week 7 | Policy, Workflow | ⏳ Pending |
| **Phase 8: Remaining Domain** | Week 8 | ActionPlan, Vendor, Compliance | ⏳ Pending |
| **Phase 9: Supporting Modules** | Week 9 | Dashboard, Onboarding, Admin, AI, Integration, Notification | ⏳ Pending |
| **Phase 10: Testing** | Week 10 | All modules | ⏳ Pending |

**Total Duration**: 10 weeks

---

## 16. Risk Mitigation

### 16.1 Potential Issues

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Breaking changes during migration** | High | Incremental migration, keep old code until new works |
| **Circular dependencies** | High | Use events instead of direct dependencies |
| **View loading issues** | Medium | Test embedded resources thoroughly |
| **ABP integration breaks** | High | Test ABP features after each module extraction |
| **Performance degradation** | Low | Load modules at startup, not per-request |
| **Deployment complexity** | Medium | Automate build/publish with MSBuild targets |

### 16.2 Rollback Plan

1. Keep original code in `_backup/` folders
2. Use feature flags to enable/disable modules
3. Can revert to monolithic by not loading modules
4. Database schema unchanged (entities just move between projects)

---

## 17. Success Criteria

✅ **All modules extracted** into separate DLLs  
✅ **All tests pass** (unit + integration)  
✅ **All controllers work** (MVC + API)  
✅ **All views render** correctly  
✅ **ABP features work** (multi-tenancy, permissions, audit)  
✅ **Module loading** works at startup  
✅ **Cross-module events** function correctly  
✅ **Performance** equal or better than monolithic  
✅ **Deployment** successful (all DLLs bundled)  

---

## 18. Next Steps

1. **Review this plan** with team
2. **Create module infrastructure** (IModule, IModuleLoader, etc.)
3. **Extract Core module first** (lowest risk)
4. **Extract one domain module** (Risk) as proof of concept
5. **Iterate and refine** based on learnings
6. **Extract remaining modules** in dependency order

---

**End of Modularization Plan**
