# STAGE 2 - Next Steps Implementation Guide

## Phase Priority Order

### Phase 1: Workflow Controller (REST API) - HIGH PRIORITY
**Why First:** Enables immediate testing and UI integration  
**Expected Time:** 60 minutes  
**Deliverables:** 6 endpoints for full workflow lifecycle

### Phase 2: Workflow Definition Seed Data - HIGH PRIORITY  
**Why Second:** Provides templates for all 7 workflows  
**Expected Time:** 45 minutes  
**Deliverables:** 7 JSON configurations + BPMN diagrams

### Phase 3: ApprovalWorkflowService - MEDIUM PRIORITY
**Why Third:** Enables evidence approval workflows  
**Expected Time:** 90 minutes  
**Deliverables:** Sequential/Parallel/Hybrid approval orchestration

### Phase 4: EscalationService (Background Worker) - MEDIUM PRIORITY
**Why Fourth:** Implements SLA enforcement  
**Expected Time:** 60 minutes  
**Deliverables:** Scheduled background task for escalation

### Phase 5: Workflow UI Views - MEDIUM PRIORITY
**Why Last:** Consumes all previous layers  
**Expected Time:** 120 minutes  
**Deliverables:** Start, MyTasks, Details, Approvals views

---

## Phase 1: WorkflowController Implementation

### File Location
`src/GrcMvc/Controllers/API/WorkflowController.cs`

### 6 Required Endpoints

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowEngineService _workflowEngine;
    private readonly IUnitOfWork _unitOfWork;
    
    // 1. POST /api/workflows/start
    // Start workflow from template definition
    // Input: workflowDefinitionId, variables object
    // Output: WorkflowInstance with created tasks
    // Error: 404 if definition not found, 400 if not active
    
    // 2. GET /api/workflows/{instanceId}
    // Get workflow instance with all tasks
    // Input: instanceId (route param)
    // Output: WorkflowInstance with expanded WorkflowTask array
    // Error: 404 if not found or not in tenant
    
    // 3. POST /api/workflows/tasks/{taskId}/complete
    // Mark task as Approved and check completion
    // Input: taskId, optional comments
    // Output: Updated WorkflowInstance
    // Error: 404 if task not found, 400 if wrong status
    
    // 4. POST /api/workflows/tasks/{taskId}/reject
    // Mark task as Rejected with reason
    // Input: taskId, reason (required)
    // Output: Updated WorkflowInstance with FailureReason
    // Error: 404 if not found, 400 if wrong status
    
    // 5. GET /api/workflows/my-tasks
    // Get all tasks assigned to current user in tenant
    // Input: optional filter (status, priority, dueDate)
    // Output: List<WorkflowTask> with parent instances
    // Query: GET /api/workflows/my-tasks?status=Pending&priority=4
    
    // 6. GET /api/workflows/instances
    // Get all workflow instances in tenant
    // Input: optional pagination (page=1, size=20)
    // Output: PagedResult<WorkflowInstance>
    // Query: GET /api/workflows/instances?page=1&size=20&status=InProgress
}
```

### Pseudo-code Structure

```csharp
[HttpPost("start/{definitionId}")]
[Authorize]
public async Task<IActionResult> StartWorkflow(
    Guid definitionId,
    [FromBody] StartWorkflowRequest request)
{
    try
    {
        var tenantId = User.GetTenantId();
        var userId = User.GetId();
        var userName = User.GetName();
        
        // Validate definition exists
        var definition = await _unitOfWork.WorkflowDefinitions.GetByIdAsync(definitionId);
        if (definition == null || definition.TenantId != tenantId)
            return NotFound("Workflow definition not found");
        
        if (definition.Status != "Active")
            return BadRequest("Workflow definition is not active");
        
        // Start workflow
        var instance = await _workflowEngine.StartWorkflowAsync(
            definitionId,
            tenantId,
            userId,
            userName,
            request.Variables);
        
        return Ok(new { instance, taskCount = instance.Tasks.Count });
    }
    catch (Exception ex)
    {
        return StatusCode(500, ex.Message);
    }
}

[HttpPost("tasks/{taskId}/complete")]
[Authorize]
public async Task<IActionResult> CompleteTask(
    Guid taskId,
    [FromBody] CompleteTaskRequest? request)
{
    try
    {
        var userId = User.GetId();
        var userName = User.GetName();
        
        var instance = await _workflowEngine.CompleteTaskAsync(
            taskId,
            userId,
            userName,
            request?.Comments);
        
        if (instance == null)
            return NotFound("Workflow instance not found");
        
        return Ok(instance);
    }
    catch (UnauthorizedAccessException)
    {
        return Forbid("Not assigned to this task");
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}

[HttpGet("my-tasks")]
[Authorize]
public async Task<IActionResult> GetMyTasks(
    [FromQuery] string? status = null,
    [FromQuery] int? priority = null)
{
    var userId = User.GetId();
    var tenantId = User.GetTenantId();
    
    var tasks = await _workflowEngine.GetUserTasksAsync(userId, tenantId);
    
    if (!string.IsNullOrEmpty(status))
        tasks = tasks.Where(t => t.Status == status).ToList();
    
    if (priority.HasValue)
        tasks = tasks.Where(t => t.Priority == priority).ToList();
    
    return Ok(tasks.OrderBy(t => t.DueDate));
}
```

### DTOs Required

```csharp
public class StartWorkflowRequest
{
    public Dictionary<string, object>? Variables { get; set; }
}

public class CompleteTaskRequest
{
    public string? Comments { get; set; }
}

public class RejectTaskRequest
{
    [Required]
    public string Reason { get; set; }
}

public class WorkflowInstanceDto
{
    public Guid Id { get; set; }
    public string InstanceNumber { get; set; }
    public string Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<WorkflowTaskDto> Tasks { get; set; }
    public string? FailureReason { get; set; }
}

public class WorkflowTaskDto
{
    public Guid Id { get; set; }
    public string TaskName { get; set; }
    public string Status { get; set; }
    public DateTime DueDate { get; set; }
    public int Priority { get; set; }
    public string? AssignedToUserName { get; set; }
}
```

---

## Phase 2: Workflow Definition Seed Data

### File Location
`src/GrcMvc/Data/Seeds/WorkflowSeeds.cs`

### Structure for Each Workflow

```csharp
public static class WorkflowDefinitionSeeds
{
    public static async Task SeedWorkflowDefinitionsAsync(
        GrcDbContext context,
        ILogger<ApplicationInitializer> logger)
    {
        if (await context.WorkflowDefinitions.AnyAsync())
        {
            logger.LogInformation("Workflow definitions already exist");
            return;
        }
        
        var definitions = new List<WorkflowDefinition>
        {
            CreateNcaEccAssessment(),
            CreateSamaCsfAssessment(),
            CreatePdplPia(),
            CreateErm(),
            CreateEvidenceReviewAndApproval(),
            CreateAuditFindingRemediation(),
            CreatePolicyReviewAndPublication()
        };
        
        context.WorkflowDefinitions.AddRange(definitions);
        await context.SaveChangesAsync();
        logger.LogInformation($"Seeded {definitions.Count} workflow definitions");
    }
    
    private static WorkflowDefinition CreateNcaEccAssessment()
    {
        var steps = new List<dynamic>
        {
            new { id = "start", name = "Start Assessment", type = "startEvent" },
            new { id = "scope", name = "Define Scope", type = "userTask", assignee = "RiskManager", daysToComplete = 3 },
            new { id = "controls", name = "Inventory Controls", type = "userTask", assignee = "RiskManager", daysToComplete = 5 },
            new { id = "gaps", name = "Identify Gaps", type = "userTask", assignee = "ComplianceOfficer", daysToComplete = 4 },
            new { id = "risk", name = "Perform Risk Ranking", type = "userTask", assignee = "ComplianceOfficer", daysToComplete = 3 },
            new { id = "remediation", name = "Develop Remediation Plan", type = "userTask", assignee = "RiskManager", daysToComplete = 5 },
            new { id = "report", name = "Document Findings", type = "userTask", assignee = "ComplianceOfficer", daysToComplete = 2 },
            new { id = "end", name = "Complete Assessment", type = "endEvent" }
        };
        
        return new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            WorkflowNumber = "WF-NCA-ECC-001",
            Name = "NCA ECC Assessment",
            Category = "Assessment",
            Framework = "NCA",
            Type = "Assessment",
            Status = "Active",
            TriggerType = "Manual",
            DefaultAssignee = "RiskManager",
            Steps = JsonSerializer.Serialize(steps),
            TenantId = null // Global template
        };
    }
    
    // Similar methods for other 6 workflows...
}
```

### JSON Steps Format Example

```json
[
  {
    "id": "start",
    "name": "Start Assessment",
    "type": "startEvent"
  },
  {
    "id": "scope",
    "name": "Define Scope",
    "type": "userTask",
    "assignee": "RiskManager",
    "daysToComplete": 3,
    "description": "Define assessment scope and boundaries"
  },
  {
    "id": "controls",
    "name": "Inventory Controls",
    "type": "userTask",
    "assignee": "RiskManager",
    "daysToComplete": 5,
    "description": "Document existing security controls"
  },
  {
    "id": "end",
    "name": "Complete Assessment",
    "type": "endEvent"
  }
]
```

### Where to Call Seeding

In `ApplicationInitializer.cs`:

```csharp
public class ApplicationInitializer
{
    public async Task InitializeAsync()
    {
        // ... existing seeds ...
        await WorkflowDefinitionSeeds.SeedWorkflowDefinitionsAsync(
            _context, _logger);
    }
}
```

---

## Phase 3: ApprovalWorkflowService

### File Location
`src/GrcMvc/Services/Implementations/ApprovalWorkflowService.cs`

### Interface

```csharp
public interface IApprovalWorkflowService
{
    // Create approval chain from definition
    Task<ApprovalInstance> StartApprovalAsync(
        Guid approvalChainId,
        Guid entityId,
        string entityType,
        Guid initiatorUserId,
        string initiatorUserName);
    
    // Approve current step
    Task<ApprovalInstance> ApproveStepAsync(
        Guid approvalInstanceId,
        Guid approverId,
        string approverName,
        string? comments = null);
    
    // Reject approval chain
    Task<ApprovalInstance> RejectApprovalAsync(
        Guid approvalInstanceId,
        Guid approverId,
        string approverName,
        string reason);
    
    // Get pending approvals for user/role
    Task<List<ApprovalInstance>> GetPendingApprovalsAsync(
        Guid userId,
        Guid tenantId);
    
    // Get approval chain history
    Task<ApprovalInstance?> GetApprovalAsync(
        Guid approvalInstanceId,
        Guid tenantId);
}
```

### Sequential Execution Logic

```csharp
private async Task EvaluateApprovalCompletionAsync(
    ApprovalInstance instance)
{
    // Get approval chain definition
    var chain = await _unitOfWork.ApprovalChains.GetByIdAsync(instance.ApprovalChainId);
    var steps = JsonSerializer.Deserialize<List<ApprovalStep>>(chain.ApprovalSteps);
    
    // Sequential: Current step must be approved before next
    if (chain.ApprovalMode == "Sequential")
    {
        if (instance.CurrentStepIndex < steps.Count - 1)
        {
            // Move to next step
            instance.CurrentStepIndex++;
            instance.CurrentApproverRole = steps[instance.CurrentStepIndex].Role;
            instance.Status = "InProgress";
        }
        else
        {
            // All steps approved
            instance.Status = "Approved";
        }
    }
    
    // Parallel: All steps must be approved
    else if (chain.ApprovalMode == "Parallel")
    {
        var allApproved = steps.All(s => s.Status == "Approved");
        instance.Status = allApproved ? "Approved" : "InProgress";
    }
    
    await _unitOfWork.ApprovalInstances.UpdateAsync(instance);
}
```

---

## Phase 4: EscalationService

### File Location
`src/GrcMvc/Services/Implementations/EscalationService.cs`

### Hosted Service Registration

```csharp
// In Program.cs
builder.Services.AddHostedService<EscalationBackgroundService>();

public class EscalationBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _escalationService.ProcessOverdueTasksAsync();
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in escalation service");
            }
        }
    }
}
```

### Core Logic

```csharp
public async Task ProcessOverdueTasksAsync()
{
    var now = DateTime.UtcNow;
    
    // Find overdue tasks by day threshold
    var escalationRules = await _unitOfWork.EscalationRules.Query()
        .Where(r => r.IsActive)
        .ToListAsync();
    
    foreach (var rule in escalationRules)
    {
        // Get tasks overdue by this many days
        var daysOverdue = rule.DaysOverdueTrigger;
        var triggerDate = now.AddDays(-daysOverdue);
        
        var overdueTasks = await _unitOfWork.WorkflowTasks.Query()
            .Where(t => t.DueDate <= triggerDate && t.Status == "Pending")
            .ToListAsync();
        
        foreach (var task in overdueTasks)
        {
            // Apply escalation action
            await ApplyEscalationAsync(task, rule);
        }
    }
}

private async Task ApplyEscalationAsync(WorkflowTask task, EscalationRule rule)
{
    var action = rule.Action;
    
    if (action == "SendNotification")
    {
        await _emailService.SendAsync(
            task.AssignedToUserId,
            "Task Overdue",
            $"Task '{task.TaskName}' is {rule.DaysOverdueTrigger} days overdue");
    }
    else if (action == "ReassignTask")
    {
        // Get manager of current assignee
        var assignee = await _unitOfWork.Users.GetByIdAsync(task.AssignedToUserId);
        task.AssignedToUserId = assignee.ReportsToUserId ?? Guid.Empty;
        
        await _unitOfWork.WorkflowTasks.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();
    }
    else if (action == "AlertExecutive")
    {
        // Send to director/executive
        var executives = await _unitOfWork.Users.Query()
            .Where(u => u.Role == "Director" || u.Role == "Executive")
            .ToListAsync();
        
        foreach (var executive in executives)
        {
            await _emailService.SendAsync(
                executive.Id,
                "Escalation Alert",
                $"Task '{task.TaskName}' is {rule.DaysOverdueTrigger} days overdue");
        }
    }
}
```

---

## Phase 5: UI Views Implementation

### 5.1 Start Workflow View
Location: `src/GrcMvc/Views/Workflows/Start.cshtml`

```html
@model StartWorkflowViewModel

@{
    ViewData["Title"] = "Start Assessment Workflow";
}

<div class="container mt-4">
    <h2>Start Assessment Workflow</h2>
    
    <form asp-action="Start" method="post">
        <!-- Workflow Selection -->
        <div class="form-group">
            <label asp-for="WorkflowDefinitionId">Select Assessment Type</label>
            <select asp-for="WorkflowDefinitionId" class="form-control">
                <option>-- Select Workflow --</option>
                @foreach (var def in Model.WorkflowDefinitions)
                {
                    <option value="@def.Id">
                        @def.Name (@def.WorkflowNumber)
                    </option>
                }
            </select>
        </div>
        
        <!-- Scope/Variables -->
        <div class="form-group">
            <label asp-for="Scope">Scope</label>
            <textarea asp-for="Scope" class="form-control" rows="4"></textarea>
        </div>
        
        <button type="submit" class="btn btn-primary">Start Workflow</button>
        <a asp-action="Index" class="btn btn-secondary">Cancel</a>
    </form>
</div>
```

### 5.2 My Tasks View
Location: `src/GrcMvc/Views/Workflows/MyTasks.cshtml`

```html
@model MyTasksViewModel

@{
    ViewData["Title"] = "My Tasks";
}

<div class="container mt-4">
    <h2>My Pending Tasks</h2>
    
    <!-- Filter -->
    <form method="get" class="mb-4">
        <select name="status" class="form-control">
            <option>All</option>
            <option>Pending</option>
            <option>InProgress</option>
        </select>
        <button type="submit" class="btn btn-sm btn-primary">Filter</button>
    </form>
    
    <!-- Task List -->
    <div class="table-responsive">
        <table class="table table-striped">
            <thead>
                <tr>
                    <th>Task Name</th>
                    <th>Workflow</th>
                    <th>Due Date</th>
                    <th>Priority</th>
                    <th>Action</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var task in Model.Tasks)
                {
                    <tr class="@(task.DueDate < DateTime.Now ? "table-danger" : "")">
                        <td>@task.TaskName</td>
                        <td>@task.WorkflowName</td>
                        <td>@task.DueDate:d</td>
                        <td>
                            <span class="badge badge-danger">
                                @task.Priority
                            </span>
                        </td>
                        <td>
                            <a asp-action="Complete" asp-route-id="@task.Id" 
                               class="btn btn-sm btn-success">
                                Approve
                            </a>
                            <a asp-action="Reject" asp-route-id="@task.Id"
                               class="btn btn-sm btn-danger">
                                Reject
                            </a>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    </div>
</div>
```

### 5.3 Workflow Details View
Location: `src/GrcMvc/Views/Workflows/Details.cshtml`

```html
@model WorkflowDetailsViewModel

@{
    ViewData["Title"] = "Workflow Details";
}

<div class="container mt-4">
    <h2>@Model.Instance.WorkflowName</h2>
    
    <!-- Instance Info -->
    <div class="card mb-4">
        <div class="card-header">
            <strong>Status:</strong> @Model.Instance.Status
        </div>
        <div class="card-body">
            <p><strong>Started:</strong> @Model.Instance.StartedAt</p>
            <p><strong>Completed:</strong> @(Model.Instance.CompletedAt?.ToString("g") ?? "In Progress")</p>
        </div>
    </div>
    
    <!-- Task Timeline -->
    <h4>Task Progress</h4>
    <div class="progress mb-4">
        <div class="progress-bar" style="width: @Model.CompletionPercentage%">
            @Model.CompletionPercentage%
        </div>
    </div>
    
    <!-- Task List -->
    @foreach (var task in Model.Instance.Tasks)
    {
        <div class="card mb-3">
            <div class="card-body">
                <h5>@task.TaskName</h5>
                <p><strong>Assigned To:</strong> @task.AssignedToUserName</p>
                <p><strong>Due:</strong> @task.DueDate:d</p>
                <p><strong>Status:</strong> 
                    <span class="badge badge-@(task.Status == "Approved" ? "success" : "warning")">
                        @task.Status
                    </span>
                </p>
            </div>
        </div>
    }
</div>
```

---

## Testing Checklist

- [ ] WorkflowController endpoints all responding (6 endpoints)
- [ ] Start workflow creates tasks with correct DueDate
- [ ] Complete task triggers completion evaluation
- [ ] Reject task marks instance as Rejected
- [ ] GetMyTasks returns only user's assigned tasks
- [ ] Audit trail records all operations
- [ ] Multi-tenancy filter applied automatically
- [ ] Approval chain moves through steps sequentially
- [ ] EscalationService detects overdue tasks
- [ ] UI views load and display data correctly

---

## Database Quick Reference

### Query Examples

```sql
-- Get pending tasks for user
SELECT * FROM WorkflowTasks 
WHERE AssignedToUserId = 'user-id' 
AND Status = 'Pending' 
AND TenantId = 'tenant-id'
ORDER BY DueDate;

-- Get workflow instances by status
SELECT * FROM WorkflowInstances 
WHERE TenantId = 'tenant-id' 
AND Status = 'InProgress'
ORDER BY StartedAt DESC;

-- Get audit trail for instance
SELECT * FROM WorkflowAuditEntries 
WHERE InstanceId = 'instance-id'
ORDER BY EventTime DESC;

-- Count overdue tasks
SELECT COUNT(*) FROM WorkflowTasks 
WHERE Status = 'Pending' 
AND DueDate < NOW()
AND TenantId = 'tenant-id';
```

---

## Performance Tips

1. **Index Usage:**
   - TenantId + Status for filtering
   - AssignedToUserId + Status for task lists
   - DueDate for overdue queries

2. **Query Optimization:**
   - Use `.Query()` and filter in LINQ before `.ToList()`
   - Eager load related entities only when needed
   - Use `.Select()` to project only needed columns

3. **Caching:**
   - Cache workflow definitions (immutable)
   - Cache active escalation rules (updated rarely)
   - Don't cache instances (mutable state)

---

## Implementation Order Recommendation

```
Day 1 Morning:
- WorkflowController (REST API) - 60 min
- Test endpoints with Postman

Day 1 Afternoon:
- Workflow Definition Seed Data - 45 min
- ApprovalWorkflowService - 90 min
- Test workflows end-to-end

Day 2 Morning:
- EscalationService - 60 min
- Configuration in Program.cs
- Test escalation logic

Day 2 Afternoon:
- UI Views (Start, MyTasks, Details) - 120 min
- Integration testing
- Performance validation
```

**Estimated Total: 6-7 hours from planning to production-ready**

---

*STAGE 2 - Workflow Infrastructure Implementation | Phase 1-5 Planning | Build: 0 Errors, 0 Warnings*
