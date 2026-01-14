# Module Function Mapping - Detailed Breakdown
## Complete Function-by-Function Analysis for Module Extraction

**Version:** 1.0  
**Date:** January 12, 2026

---

## 📋 Purpose

This document provides a **complete function-by-function mapping** showing:
- Current location of each function
- Target location after modularization
- Dependencies and relationships
- Transfer strategy for each function

---

## 🔍 How to Use This Document

1. **Find your function** in the table below
2. **See current location** (service/controller/entity)
3. **See target location** (which module DLL)
4. **Understand dependencies** (what it needs from other modules)
5. **Follow transfer strategy** (how to move it)

---

## 1. Core Module Functions

### 1.1 Tenant Context Functions

| Function | Current Location | Target Location | Dependencies | Transfer Strategy |
|----------|-----------------|-----------------|--------------|-------------------|
| `GetCurrentTenantId()` | `ITenantContextService.GetCurrentTenantId()` | `GrcMvc.Modules.Core` | None | Move entire `ITenantContextService` interface and implementation |
| `SetTenantContext(Guid tenantId)` | `ITenantContextService.SetTenantContext()` | `GrcMvc.Modules.Core` | None | Move to Core |
| `GetCurrentTenant()` | `ITenantContextService.GetCurrentTenant()` | `GrcMvc.Modules.Core` | `ITenantRepository` | Move to Core, inject `ITenantRepository` |
| `IsTenantActive()` | `ITenantContextService.IsTenantActive()` | `GrcMvc.Modules.Core` | `ITenantRepository` | Move to Core |

**Files to Transfer:**
- `Services/Interfaces/ITenantContextService.cs` → `GrcMvc.Modules.Core/Services/Interfaces/`
- `Services/Implementations/TenantContextService.cs` → `GrcMvc.Modules.Core/Services/Implementations/`

**Registration in Program.cs:**
```csharp
// Before
builder.Services.AddScoped<ITenantContextService, TenantContextService>();

// After (in CoreModule.ConfigureServices)
services.AddScoped<ITenantContextService, TenantContextService>();
```

### 1.2 Workspace Context Functions

| Function | Current Location | Target Location | Dependencies | Transfer Strategy |
|----------|-----------------|-----------------|--------------|-------------------|
| `GetCurrentWorkspaceId()` | `IWorkspaceContextService.GetCurrentWorkspaceId()` | `GrcMvc.Modules.Core` | None | Move entire service |
| `SetWorkspaceContext(Guid workspaceId)` | `IWorkspaceContextService.SetWorkspaceContext()` | `GrcMvc.Modules.Core` | None | Move to Core |
| `GetCurrentWorkspace()` | `IWorkspaceContextService.GetCurrentWorkspace()` | `GrcMvc.Modules.Core` | `IWorkspaceRepository` | Move to Core |
| `GetWorkspacesByTenant()` | `IWorkspaceContextService.GetWorkspacesByTenant()` | `GrcMvc.Modules.Core` | `IWorkspaceRepository` | Move to Core |

**Files to Transfer:**
- `Services/Interfaces/IWorkspaceContextService.cs` → `GrcMvc.Modules.Core/Services/Interfaces/`
- `Services/Implementations/WorkspaceContextService.cs` → `GrcMvc.Modules.Core/Services/Implementations/`

### 1.3 Serial Code Functions

| Function | Current Location | Target Location | Dependencies | Transfer Strategy |
|----------|-----------------|-----------------|--------------|-------------------|
| `GenerateSerialCode(string entityType, Guid? tenantId)` | `ISerialCodeService.GenerateSerialCode()` | `GrcMvc.Modules.Core` | `IUnitOfWork`, `ITenantContextService` | Move entire service |
| `GetNextCode(string prefix, int stage)` | `ISerialCodeService.GetNextCode()` | `GrcMvc.Modules.Core` | `IUnitOfWork` | Move to Core |
| `ValidateBusinessCode(string code)` | `ISerialCodeService.ValidateBusinessCode()` | `GrcMvc.Modules.Core` | None | Move to Core |

**Files to Transfer:**
- `Services/Interfaces/ISerialCodeService.cs` → `GrcMvc.Modules.Core/Services/Interfaces/`
- `Services/Implementations/SerialCodeService.cs` → `GrcMvc.Modules.Core/Services/Implementations/`

### 1.4 Unit of Work Functions

| Function | Current Location | Target Location | Dependencies | Transfer Strategy |
|----------|-----------------|-----------------|--------------|-------------------|
| `SaveChangesAsync()` | `IUnitOfWork.SaveChangesAsync()` | `GrcMvc.Modules.Core` | `GrcDbContext` | Move entire interface and implementation |
| `BeginTransactionAsync()` | `IUnitOfWork.BeginTransactionAsync()` | `GrcMvc.Modules.Core` | `GrcDbContext` | Move to Core |
| `CommitTransactionAsync()` | `IUnitOfWork.CommitTransactionAsync()` | `GrcMvc.Modules.Core` | `GrcDbContext` | Move to Core |
| `RollbackTransactionAsync()` | `IUnitOfWork.RollbackTransactionAsync()` | `GrcMvc.Modules.Core` | `GrcDbContext` | Move to Core |
| `Risks { get; }` | `IUnitOfWork.Risks` | **Risk Module** | `IGenericRepository<Risk>` | Keep interface in Core, but repository per module |

**Files to Transfer:**
- `Data/IUnitOfWork.cs` → `GrcMvc.Modules.Core/Data/`
- `Data/UnitOfWork.cs` → `GrcMvc.Modules.Core/Data/`

**Note:** `IUnitOfWork` will become a **composite** that aggregates repositories from all modules. See section 8.4.

### 1.5 Generic Repository Functions

| Function | Current Location | Target Location | Dependencies | Transfer Strategy |
|----------|-----------------|-----------------|--------------|-------------------|
| `GetByIdAsync(Guid id)` | `IGenericRepository<T>.GetByIdAsync()` | `GrcMvc.Modules.Core` | `GrcDbContext` | Move entire generic repository |
| `GetAllAsync()` | `IGenericRepository<T>.GetAllAsync()` | `GrcMvc.Modules.Core` | `GrcDbContext` | Move to Core |
| `AddAsync(T entity)` | `IGenericRepository<T>.AddAsync()` | `GrcMvc.Modules.Core` | `GrcDbContext` | Move to Core |
| `UpdateAsync(T entity)` | `IGenericRepository<T>.UpdateAsync()` | `GrcMvc.Modules.Core` | `GrcDbContext` | Move to Core |
| `DeleteAsync(Guid id)` | `IGenericRepository<T>.DeleteAsync()` | `GrcMvc.Modules.Core` | `GrcDbContext` | Move to Core |
| `FindAsync(Expression<Func<T, bool>> predicate)` | `IGenericRepository<T>.FindAsync()` | `GrcMvc.Modules.Core` | `GrcDbContext` | Move to Core |

**Files to Transfer:**
- `Data/Repositories/IGenericRepository.cs` → `GrcMvc.Modules.Core/Data/Repositories/`
- `Data/Repositories/GenericRepository.cs` → `GrcMvc.Modules.Core/Data/Repositories/`

---

## 2. Risk Module Functions

### 2.1 Risk Service Functions

| Function | Current Location | Target Location | Dependencies | Cross-Module Events | Transfer Strategy |
|----------|-----------------|-----------------|--------------|---------------------|-------------------|
| `CreateRiskAsync(CreateRiskDto dto)` | `IRiskService.CreateRiskAsync()` | `GrcMvc.Modules.Risk` | `IUnitOfWork`, `ISerialCodeService`, `ITenantContextService` | Publishes `RiskCreatedEvent` | Move service, replace direct calls with events |
| `UpdateRiskAsync(Guid id, UpdateRiskDto dto)` | `IRiskService.UpdateRiskAsync()` | `GrcMvc.Modules.Risk` | `IUnitOfWork` | Publishes `RiskUpdatedEvent` | Move to Risk module |
| `DeleteRiskAsync(Guid id)` | `IRiskService.DeleteRiskAsync()` | `GrcMvc.Modules.Risk` | `IUnitOfWork` | Publishes `RiskDeletedEvent` | Move to Risk module |
| `GetRiskByIdAsync(Guid id)` | `IRiskService.GetRiskByIdAsync()` | `GrcMvc.Modules.Risk` | `IUnitOfWork` | None | Move to Risk module |
| `GetRisksByTenantAsync(Guid tenantId)` | `IRiskService.GetRisksByTenantAsync()` | `GrcMvc.Modules.Risk` | `IUnitOfWork`, `ITenantContextService` | None | Move to Risk module |
| `GetRisksByWorkspaceAsync(Guid workspaceId)` | `IRiskService.GetRisksByWorkspaceAsync()` | `GrcMvc.Modules.Risk` | `IUnitOfWork`, `IWorkspaceContextService` | None | Move to Risk module |
| `CalculateRiskScoreAsync(Guid riskId)` | `IRiskService.CalculateRiskScoreAsync()` | `GrcMvc.Modules.Risk` | `IUnitOfWork` | None | Move to Risk module |
| `GetRiskMatrixAsync(Guid? tenantId)` | `IRiskService.GetRiskMatrixAsync()` | `GrcMvc.Modules.Risk` | `IUnitOfWork` | None | Move to Risk module |
| `LinkControlsToRiskAsync(Guid riskId, List<Guid> controlIds)` | `IRiskService.LinkControlsToRiskAsync()` | `GrcMvc.Modules.Risk` | `IUnitOfWork` | **REMOVE** - Control module subscribes to `RiskCreatedEvent` | Remove direct dependency, use events |

**Files to Transfer:**
- `Services/Interfaces/IRiskService.cs` → `GrcMvc.Modules.Risk/Services/Interfaces/`
- `Services/Implementations/RiskService.cs` → `GrcMvc.Modules.Risk/Services/Implementations/`
- `Models/Entities/Risk.cs` → `GrcMvc.Modules.Risk/Models/Entities/`
- `Models/DTOs/RiskDto.cs` → `GrcMvc.Modules.Risk/Models/DTOs/`
- `Models/DTOs/CreateRiskDto.cs` → `GrcMvc.Modules.Risk/Models/DTOs/`
- `Models/DTOs/UpdateRiskDto.cs` → `GrcMvc.Modules.Risk/Models/DTOs/`

**Before (with direct dependency):**
```csharp
public async Task<RiskDto> CreateRiskAsync(CreateRiskDto dto)
{
    var risk = _mapper.Map<Risk>(dto);
    risk.BusinessCode = await _serialCodeService.GenerateSerialCodeAsync("RISK", risk.TenantId);
    
    await _unitOfWork.Risks.AddAsync(risk);
    await _unitOfWork.SaveChangesAsync();
    
    // Direct dependency on ControlService - REMOVE
    if (dto.ControlIds?.Any() == true)
    {
        await _controlService.LinkToRiskAsync(risk.Id, dto.ControlIds);
    }
    
    // Direct dependency on NotificationService - REMOVE
    await _notificationService.SendAsync(new RiskCreatedNotification { RiskId = risk.Id });
    
    return _mapper.Map<RiskDto>(risk);
}
```

**After (using events):**
```csharp
public async Task<RiskDto> CreateRiskAsync(CreateRiskDto dto)
{
    var risk = _mapper.Map<Risk>(dto);
    risk.BusinessCode = await _serialCodeService.GenerateSerialCodeAsync("RISK", risk.TenantId);
    
    await _unitOfWork.Risks.AddAsync(risk);
    await _unitOfWork.SaveChangesAsync();
    
    // Publish event - other modules can subscribe
    await _eventBus.PublishAsync(new RiskCreatedEvent
    {
        RiskId = risk.Id,
        TenantId = risk.TenantId!.Value,
        WorkspaceId = risk.WorkspaceId,
        RiskName = risk.Name,
        ControlIds = dto.ControlIds ?? new List<Guid>(),
        CreatedAt = DateTime.UtcNow
    });
    
    return _mapper.Map<RiskDto>(risk);
}
```

**In Control Module (subscriber):**
```csharp
public class RiskCreatedEventHandler : IConsumer<RiskCreatedEvent>
{
    private readonly IControlService _controlService;
    
    public async Task Consume(ConsumeContext<RiskCreatedEvent> context)
    {
        var @event = context.Message;
        if (@event.ControlIds.Any())
        {
            await _controlService.LinkToRiskAsync(@event.RiskId, @event.ControlIds);
        }
    }
}
```

### 2.2 Risk Appetite Service Functions

| Function | Current Location | Target Location | Dependencies | Transfer Strategy |
|----------|-----------------|-----------------|--------------|-------------------|
| `GetRiskAppetiteSettingsAsync(Guid tenantId)` | `IRiskAppetiteService.GetRiskAppetiteSettingsAsync()` | `GrcMvc.Modules.Risk` | `IUnitOfWork` | Move to Risk module |
| `UpdateRiskAppetiteSettingsAsync(Guid tenantId, RiskAppetiteSettingDto dto)` | `IRiskAppetiteService.UpdateRiskAppetiteSettingsAsync()` | `GrcMvc.Modules.Risk` | `IUnitOfWork` | Move to Risk module |
| `CalculateRiskLevelAsync(RiskSeverity severity, RiskLikelihood likelihood)` | `IRiskAppetiteService.CalculateRiskLevelAsync()` | `GrcMvc.Modules.Risk` | `IUnitOfWork` | Move to Risk module |

**Files to Transfer:**
- `Services/Interfaces/IRiskAppetiteService.cs` → `GrcMvc.Modules.Risk/Services/Interfaces/`
- `Services/Implementations/RiskAppetiteService.cs` → `GrcMvc.Modules.Risk/Services/Implementations/`
- `Models/Entities/RiskAppetiteSetting.cs` → `GrcMvc.Modules.Risk/Models/Entities/`

### 2.3 Risk Controller Functions

| Function | Current Location | Target Location | Dependencies | Transfer Strategy |
|----------|-----------------|-----------------|--------------|-------------------|
| `Index()` | `RiskController.Index()` | `GrcMvc.Modules.Risk` | `IRiskService`, `ITenantContextService` | Move controller, add `[Area("Risk")]` |
| `Details(Guid id)` | `RiskController.Details()` | `GrcMvc.Modules.Risk` | `IRiskService` | Move to Risk module |
| `Create()` | `RiskController.Create()` | `GrcMvc.Modules.Risk` | `IRiskService` | Move to Risk module |
| `Create(CreateRiskDto dto)` | `RiskController.Create()` | `GrcMvc.Modules.Risk` | `IRiskService` | Move to Risk module |
| `Edit(Guid id)` | `RiskController.Edit()` | `GrcMvc.Modules.Risk` | `IRiskService` | Move to Risk module |
| `Edit(Guid id, UpdateRiskDto dto)` | `RiskController.Edit()` | `GrcMvc.Modules.Risk` | `IRiskService` | Move to Risk module |
| `Delete(Guid id)` | `RiskController.Delete()` | `GrcMvc.Modules.Risk` | `IRiskService` | Move to Risk module |
| `GetRiskMatrix()` | `RiskController.GetRiskMatrix()` | `GrcMvc.Modules.Risk` | `IRiskService` | Move to Risk module |

**Files to Transfer:**
- `Controllers/RiskController.cs` → `GrcMvc.Modules.Risk/Controllers/`
- `Controllers/RiskApiController.cs` → `GrcMvc.Modules.Risk/Controllers/`
- `Views/Risk/Index.cshtml` → `GrcMvc.Modules.Risk/Views/Risk/`
- `Views/Risk/Create.cshtml` → `GrcMvc.Modules.Risk/Views/Risk/`
- `Views/Risk/Edit.cshtml` → `GrcMvc.Modules.Risk/Views/Risk/`
- `Views/Risk/Details.cshtml` → `GrcMvc.Modules.Risk/Views/Risk/`
- `Views/Risk/Delete.cshtml` → `GrcMvc.Modules.Risk/Views/Risk/`

**Controller Changes:**
```csharp
// Before
[Authorize]
public class RiskController : Controller
{
    // ...
}

// After
[Area("Risk")]
[Authorize(Permissions = "Grc.Risk.View")]
public class RiskController : Controller
{
    // ...
}
```

---

## 3. Control Module Functions

### 3.1 Control Service Functions

| Function | Current Location | Target Location | Dependencies | Cross-Module Events | Transfer Strategy |
|----------|-----------------|-----------------|--------------|---------------------|-------------------|
| `CreateControlAsync(CreateControlDto dto)` | `IControlService.CreateControlAsync()` | `GrcMvc.Modules.Control` | `IUnitOfWork`, `ISerialCodeService` | Publishes `ControlCreatedEvent` | Move to Control module |
| `UpdateControlAsync(Guid id, UpdateControlDto dto)` | `IControlService.UpdateControlAsync()` | `GrcMvc.Modules.Control` | `IUnitOfWork` | Publishes `ControlUpdatedEvent` | Move to Control module |
| `DeleteControlAsync(Guid id)` | `IControlService.DeleteControlAsync()` | `GrcMvc.Modules.Control` | `IUnitOfWork` | Publishes `ControlDeletedEvent` | Move to Control module |
| `GetControlByIdAsync(Guid id)` | `IControlService.GetControlByIdAsync()` | `GrcMvc.Modules.Control` | `IUnitOfWork` | None | Move to Control module |
| `GetControlsByRiskAsync(Guid riskId)` | `IControlService.GetControlsByRiskAsync()` | `GrcMvc.Modules.Control` | `IUnitOfWork` | **SUBSCRIBER** to `RiskCreatedEvent` | Move to Control, subscribe to Risk events |
| `AssignOwnerAsync(Guid controlId, Guid ownerId)` | `IControlService.AssignOwnerAsync()` | `GrcMvc.Modules.Control` | `IUnitOfWork` | Publishes `ControlOwnerAssignedEvent` | Move to Control module |
| `EvaluateControlEffectivenessAsync(Guid controlId)` | `IControlService.EvaluateControlEffectivenessAsync()` | `GrcMvc.Modules.Control` | `IUnitOfWork`, `IAssessmentService` (via events) | Publishes `ControlEffectivenessEvaluatedEvent` | Move to Control, subscribe to Assessment events |

**Files to Transfer:**
- `Services/Interfaces/IControlService.cs` → `GrcMvc.Modules.Control/Services/Interfaces/`
- `Services/Implementations/ControlService.cs` → `GrcMvc.Modules.Control/Services/Implementations/`
- `Models/Entities/Control.cs` → `GrcMvc.Modules.Control/Models/Entities/`
- `Models/DTOs/ControlDto.cs` → `GrcMvc.Modules.Control/Models/DTOs/`

**Event Subscriber Example:**
```csharp
// In GrcMvc.Modules.Control
public class RiskCreatedEventHandler : IConsumer<RiskCreatedEvent>
{
    private readonly IControlService _controlService;
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task Consume(ConsumeContext<RiskCreatedEvent> context)
    {
        var @event = context.Message;
        
        // Auto-link controls to risk if configured
        if (@event.ControlIds?.Any() == true)
        {
            foreach (var controlId in @event.ControlIds)
            {
                await _controlService.LinkToRiskAsync(@event.RiskId, controlId);
            }
        }
    }
}
```

---

## 4. Assessment Module Functions

### 4.1 Assessment Service Functions

| Function | Current Location | Target Location | Dependencies | Cross-Module Events | Transfer Strategy |
|----------|-----------------|-----------------|--------------|---------------------|-------------------|
| `CreateAssessmentAsync(CreateAssessmentDto dto)` | `IAssessmentService.CreateAssessmentAsync()` | `GrcMvc.Modules.Assessment` | `IUnitOfWork`, `IControlService` (via events) | Publishes `AssessmentCreatedEvent` | Move to Assessment module |
| `ExecuteAssessmentAsync(Guid assessmentId)` | `IAssessmentService.ExecuteAssessmentAsync()` | `GrcMvc.Modules.Assessment` | `IUnitOfWork`, `IEvidenceService` (via events) | Publishes `AssessmentExecutedEvent` | Move to Assessment module |
| `CalculateComplianceScoreAsync(Guid assessmentId)` | `IAssessmentService.CalculateComplianceScoreAsync()` | `GrcMvc.Modules.Assessment` | `IUnitOfWork` | None | Move to Assessment module |
| `GetAssessmentResultsAsync(Guid assessmentId)` | `IAssessmentService.GetAssessmentResultsAsync()` | `GrcMvc.Modules.Assessment` | `IUnitOfWork` | None | Move to Assessment module |
| `GetControlsByAssessmentAsync(Guid assessmentId)` | `IAssessmentService.GetControlsByAssessmentAsync()` | `GrcMvc.Modules.Assessment` | `IUnitOfWork` | **SUBSCRIBER** to `ControlCreatedEvent` | Move to Assessment, subscribe to Control events |

**Files to Transfer:**
- `Services/Interfaces/IAssessmentService.cs` → `GrcMvc.Modules.Assessment/Services/Interfaces/`
- `Services/Implementations/AssessmentService.cs` → `GrcMvc.Modules.Assessment/Services/Implementations/`
- `Models/Entities/Assessment.cs` → `GrcMvc.Modules.Assessment/Models/Entities/`
- `Models/Entities/AssessmentResult.cs` → `GrcMvc.Modules.Assessment/Models/Entities/`
- `Controllers/AssessmentController.cs` → `GrcMvc.Modules.Assessment/Controllers/`
- `Views/Assessment/*.cshtml` → `GrcMvc.Modules.Assessment/Views/Assessment/`

---

## 5. Evidence Module Functions

### 5.1 Evidence Service Functions

| Function | Current Location | Target Location | Dependencies | Cross-Module Events | Transfer Strategy |
|----------|-----------------|-----------------|--------------|---------------------|-------------------|
| `UploadEvidenceAsync(CreateEvidenceDto dto, IFormFile file)` | `IEvidenceService.UploadEvidenceAsync()` | `GrcMvc.Modules.Evidence` | `IUnitOfWork`, `ITenantContextService` | Publishes `EvidenceUploadedEvent` | Move to Evidence module |
| `UpdateEvidenceAsync(Guid id, UpdateEvidenceDto dto)` | `IEvidenceService.UpdateEvidenceAsync()` | `GrcMvc.Modules.Evidence` | `IUnitOfWork` | Publishes `EvidenceUpdatedEvent` | Move to Evidence module |
| `DeleteEvidenceAsync(Guid id)` | `IEvidenceService.DeleteEvidenceAsync()` | `GrcMvc.Modules.Evidence` | `IUnitOfWork` | Publishes `EvidenceDeletedEvent` | Move to Evidence module |
| `ValidateEvidenceAsync(Guid id)` | `IEvidenceService.ValidateEvidenceAsync()` | `GrcMvc.Modules.Evidence` | `IUnitOfWork` | None | Move to Evidence module |
| `LinkEvidenceToControlAsync(Guid evidenceId, Guid controlId)` | `IEvidenceService.LinkEvidenceToControlAsync()` | `GrcMvc.Modules.Evidence` | `IUnitOfWork` | **SUBSCRIBER** to `ControlCreatedEvent` | Move to Evidence, subscribe to Control events |
| `GetEvidenceByControlAsync(Guid controlId)` | `IEvidenceService.GetEvidenceByControlAsync()` | `GrcMvc.Modules.Evidence` | `IUnitOfWork` | None | Move to Evidence module |

**Files to Transfer:**
- `Services/Interfaces/IEvidenceService.cs` → `GrcMvc.Modules.Evidence/Services/Interfaces/`
- `Services/Implementations/EvidenceService.cs` → `GrcMvc.Modules.Evidence/Services/Implementations/`
- `Models/Entities/Evidence.cs` → `GrcMvc.Modules.Evidence/Models/Entities/`
- `Controllers/EvidenceController.cs` → `GrcMvc.Modules.Evidence/Controllers/`
- `Views/Evidence/*.cshtml` → `GrcMvc.Modules.Evidence/Views/Evidence/`

---

## 6. Workflow Module Functions

### 6.1 Workflow Service Functions

| Function | Current Location | Target Location | Dependencies | Cross-Module Events | Transfer Strategy |
|----------|-----------------|-----------------|--------------|---------------------|-------------------|
| `CreateWorkflowAsync(CreateWorkflowDto dto)` | `IWorkflowService.CreateWorkflowAsync()` | `GrcMvc.Modules.Workflow` | `IUnitOfWork` | Publishes `WorkflowCreatedEvent` | Move to Workflow module |
| `ExecuteWorkflowAsync(Guid workflowId)` | `IWorkflowService.ExecuteWorkflowAsync()` | `GrcMvc.Modules.Workflow` | `IUnitOfWork`, Hangfire (Infrastructure) | Publishes `WorkflowExecutedEvent` | Move to Workflow module |
| `CompleteWorkflowTaskAsync(Guid taskId)` | `IWorkflowService.CompleteWorkflowTaskAsync()` | `GrcMvc.Modules.Workflow` | `IUnitOfWork` | Publishes `WorkflowTaskCompletedEvent` | Move to Workflow module |
| `EscalateTaskAsync(Guid taskId)` | `IEscalationService.EscalateTaskAsync()` | `GrcMvc.Modules.Workflow` | `IUnitOfWork`, `INotificationService` (via events) | Publishes `TaskEscalatedEvent` | Move to Workflow module |

**Files to Transfer:**
- `Services/Interfaces/IWorkflowService.cs` → `GrcMvc.Modules.Workflow/Services/Interfaces/`
- `Services/Implementations/WorkflowService.cs` → `GrcMvc.Modules.Workflow/Services/Implementations/`
- `Services/Interfaces/IEscalationService.cs` → `GrcMvc.Modules.Workflow/Services/Interfaces/`
- `Services/Implementations/EscalationService.cs` → `GrcMvc.Modules.Workflow/Services/Implementations/`
- `Models/Entities/Workflow.cs` → `GrcMvc.Modules.Workflow/Models/Entities/`
- `Models/Entities/WorkflowInstance.cs` → `GrcMvc.Modules.Workflow/Models/Entities/`
- `Models/Entities/WorkflowTask.cs` → `GrcMvc.Modules.Workflow/Models/Entities/`
- `Controllers/WorkflowController.cs` → `GrcMvc.Modules.Workflow/Controllers/`
- `Controllers/WorkflowUIController.cs` → `GrcMvc.Modules.Workflow/Controllers/`
- `Views/Workflow/*.cshtml` → `GrcMvc.Modules.Workflow/Views/Workflow/`

---

## 7. Cross-Module Event Flow

### 7.1 Risk → Control Event Flow

```
1. User creates Risk via RiskController
2. RiskController calls IRiskService.CreateRiskAsync()
3. RiskService creates Risk entity, saves to DB
4. RiskService publishes RiskCreatedEvent via IModuleEventBus
5. ControlModule subscribes to RiskCreatedEvent
6. ControlModule's RiskCreatedEventHandler receives event
7. ControlModule calls IControlService.LinkToRiskAsync()
8. ControlService links controls to risk
```

### 7.2 Control → Evidence Event Flow

```
1. User creates Control via ControlController
2. ControlController calls IControlService.CreateControlAsync()
3. ControlService creates Control entity, saves to DB
4. ControlService publishes ControlCreatedEvent
5. EvidenceModule subscribes to ControlCreatedEvent
6. EvidenceModule's ControlCreatedEventHandler receives event
7. EvidenceModule can auto-create evidence requirements
```

### 7.3 Assessment → Evidence Event Flow

```
1. User executes Assessment via AssessmentController
2. AssessmentController calls IAssessmentService.ExecuteAssessmentAsync()
3. AssessmentService determines which controls need evidence
4. AssessmentService publishes AssessmentExecutedEvent with ControlIds
5. EvidenceModule subscribes to AssessmentExecutedEvent
6. EvidenceModule can trigger evidence collection requests
```

---

## 8. Special Cases

### 8.1 Functions Used by Multiple Modules

| Function | Current Location | Target Location | Strategy |
|----------|-----------------|-----------------|----------|
| `GenerateSerialCode(string entityType)` | `ISerialCodeService` | `GrcMvc.Modules.Core` | Keep in Core, all modules reference Core |
| `GetCurrentTenantId()` | `ITenantContextService` | `GrcMvc.Modules.Core` | Keep in Core |
| `SaveChangesAsync()` | `IUnitOfWork` | `GrcMvc.Modules.Core` | Keep in Core, modules inject IUnitOfWork |

### 8.2 Functions with Circular Dependencies

**Problem:** RiskService depends on ControlService, ControlService depends on RiskService

**Solution:** Use events instead of direct dependencies

```csharp
// Before (circular dependency)
public class RiskService
{
    private readonly IControlService _controlService; // ❌ Circular
}

public class ControlService
{
    private readonly IRiskService _riskService; // ❌ Circular
}

// After (event-based)
public class RiskService
{
    private readonly IModuleEventBus _eventBus; // ✅ No circular dependency
}

public class ControlService
{
    private readonly IModuleEventBus _eventBus; // ✅ No circular dependency
    
    // Subscribe to RiskCreatedEvent in module initialization
}
```

### 8.3 Functions Requiring Background Jobs

| Function | Current Location | Target Location | Strategy |
|----------|-----------------|-----------------|----------|
| `ExecuteWorkflowAsync()` | `IWorkflowService` | `GrcMvc.Modules.Workflow` | Use Hangfire (Infrastructure module) |
| `SendNotificationAsync()` | `INotificationService` | `GrcMvc.Modules.Notification` | Use Hangfire for batch sending |
| `ProcessEvidenceAsync()` | `IEvidenceService` | `GrcMvc.Modules.Evidence` | Use Hangfire for async processing |

**Strategy:** Modules that need background jobs depend on `GrcMvc.Infrastructure`, which provides Hangfire integration.

### 8.4 UnitOfWork Repository Properties

**Problem:** `IUnitOfWork` has properties like `Risks`, `Controls`, etc. - one per entity type.

**Solution:** Make `IUnitOfWork` a **composite** that aggregates repositories from all modules:

```csharp
// In GrcMvc.Modules.Core
public interface IUnitOfWork : IDisposable
{
    // Generic repository accessor
    IGenericRepository<T> GetRepository<T>() where T : BaseEntity;
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

// In modules, use GetRepository<T>() instead of specific properties
// Before: await _unitOfWork.Risks.AddAsync(risk);
// After:  await _unitOfWork.GetRepository<Risk>().AddAsync(risk);
```

---

## 9. Function Transfer Checklist

### For Each Function:

- [ ] **Identify dependencies** (what it needs from other modules)
- [ ] **Replace direct dependencies** with events (if cross-module)
- [ ] **Move function** to appropriate module
- [ ] **Update namespace** references
- [ ] **Update DI registration** (move to module's ConfigureServices)
- [ ] **Update tests** (move to module's test project)
- [ ] **Verify event subscriptions** (if applicable)
- [ ] **Test after transfer**

---

## 10. Complete Function Mapping Table

See `MODULE_FUNCTION_MAPPING_TABLE.md` for a complete table of all 500+ functions with their current location, target location, dependencies, and transfer strategy.

---

**End of Function Mapping Document**
