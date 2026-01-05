# STAGE 2 - REMAINING WORK: UI & API Layer

## 🎯 Current Status

| Component | Backend | UI | APIs | Status |
|-----------|---------|----|----|--------|
| **Workflows** | ✅ Complete | ❌ Missing | ❌ Missing | 50% |
| **Roles & Governance** | ✅ Complete | ❌ Missing | ❌ Missing | 50% |
| **Inbox & Tasks** | ✅ Complete | ❌ Missing | ❌ Missing | 50% |
| **LLM Integration** | ✅ Complete | ❌ Missing | ❌ Missing | 50% |

---

## 📋 MISSING: API ENDPOINTS

### 1. Workflow APIs

```csharp
// Controllers/WorkflowController.cs
[ApiController]
[Route("api/[controller]")]
public class WorkflowController : ControllerBase
{
    // ❌ MISSING ENDPOINTS:
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorkflow(Guid id)
    
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserWorkflows(Guid userId)
    
    [HttpPost]
    public async Task<IActionResult> CreateWorkflow(CreateWorkflowDto dto)
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWorkflow(Guid id, UpdateWorkflowDto dto)
    
    [HttpPost("{id}/execute-task")]
    public async Task<IActionResult> ExecuteTask(Guid id, ExecuteTaskDto dto)
    
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveWorkflow(Guid id)
    
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectWorkflow(Guid id)
    
    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetWorkflowStatus(Guid id)
    
    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetWorkflowHistory(Guid id)
}
```

### 2. Inbox APIs

```csharp
// Controllers/InboxController.cs
[ApiController]
[Route("api/[controller]")]
public class InboxController : ControllerBase
{
    // ❌ MISSING ENDPOINTS:
    
    [HttpGet]
    public async Task<IActionResult> GetInbox()
    
    [HttpGet("process-card/{workflowId}")]
    public async Task<IActionResult> GetProcessCard(Guid workflowId)
    
    [HttpGet("pending-actions")]
    public async Task<IActionResult> GetPendingActions()
    
    [HttpPost("task/{taskId}/approve")]
    public async Task<IActionResult> ApproveTask(Guid taskId)
    
    [HttpPost("task/{taskId}/reject")]
    public async Task<IActionResult> RejectTask(Guid taskId)
    
    [HttpPost("task/{taskId}/escalate")]
    public async Task<IActionResult> EscalateTask(Guid taskId)
    
    [HttpPost("task/{taskId}/comment")]
    public async Task<IActionResult> AddComment(Guid taskId, AddCommentDto dto)
    
    [HttpGet("task/{taskId}/comments")]
    public async Task<IActionResult> GetComments(Guid taskId)
    
    [HttpGet("task/{taskId}/sla")]
    public async Task<IActionResult> GetTaskSla(Guid taskId)
}
```

### 3. Role Management APIs

```csharp
// Controllers/RoleController.cs
[ApiController]
[Route("api/[controller]")]
public class RoleController : ControllerBase
{
    // ❌ MISSING ENDPOINTS:
    
    [HttpGet]
    public async Task<IActionResult> GetAllRoles()
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRole(Guid id)
    
    [HttpPost("user/{userId}/assign")]
    public async Task<IActionResult> AssignRoleToUser(Guid userId, AssignRoleDto dto)
    
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserRole(Guid userId)
    
    [HttpGet("by-layer/{layer}")]
    public async Task<IActionResult> GetRolesByLayer(string layer)
    
    [HttpPost("user/{userId}/ksa")]
    public async Task<IActionResult> UpdateUserKsa(Guid userId, UpdateKsaDto dto)
}
```

### 4. LLM APIs

```csharp
// Controllers/LlmController.cs
[ApiController]
[Route("api/[controller]")]
public class LlmController : ControllerBase
{
    // ❌ MISSING ENDPOINTS:
    
    [HttpGet("config")]
    public async Task<IActionResult> GetLlmConfig()
    
    [HttpPost("config")]
    public async Task<IActionResult> ConfigureLlm(ConfigureLlmDto dto)
    
    [HttpPost("workflow/{workflowId}/insight")]
    public async Task<IActionResult> GenerateWorkflowInsight(Guid workflowId)
    
    [HttpPost("risk/{riskId}/analysis")]
    public async Task<IActionResult> GenerateRiskAnalysis(Guid riskId)
    
    [HttpPost("assessment/{assessmentId}/recommendation")]
    public async Task<IActionResult> GenerateComplianceRec(Guid assessmentId)
    
    [HttpPost("task/{taskId}/summary")]
    public async Task<IActionResult> GenerateTaskSummary(Guid taskId)
    
    [HttpPost("finding/{findingId}/remedy")]
    public async Task<IActionResult> GenerateAuditRemedy(Guid findingId)
    
    [HttpGet("usage")]
    public async Task<IActionResult> GetUsage()
}
```

---

## 📦 MISSING: DTOs (Data Transfer Objects)

### Workflow DTOs
```csharp
// Models/DTOs/WorkflowDtos.cs
public class CreateWorkflowDto
{
    public Guid WorkflowDefinitionId { get; set; }
    public string Priority { get; set; } = "Medium";
    public DateTime? DueDate { get; set; }
}

public class UpdateWorkflowDto
{
    public string Status { get; set; }
    public DateTime? DueDate { get; set; }
}

public class ExecuteTaskDto
{
    public Guid WorkflowTaskId { get; set; }
    public string Status { get; set; }
}

public class WorkflowResponseDto
{
    public Guid Id { get; set; }
    public string WorkflowName { get; set; }
    public string Status { get; set; }
    public int TasksCompleted { get; set; }
    public int TotalTasks { get; set; }
}
```

### Inbox DTOs
```csharp
// Models/DTOs/InboxDtos.cs
public class AddCommentDto
{
    public string Comment { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
}

public class TaskActionDto
{
    public Guid TaskId { get; set; }
    public string Action { get; set; } // "Approve", "Reject", "Escalate"
    public string? Reason { get; set; }
}

public class InboxResponseDto
{
    public int TotalTasks { get; set; }
    public int PendingTasks { get; set; }
    public int OverdueTasks { get; set; }
    public List<TaskSummaryDto> Tasks { get; set; }
}

public class TaskSummaryDto
{
    public Guid TaskId { get; set; }
    public string TaskName { get; set; }
    public string Status { get; set; }
    public string Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public string SlaStatus { get; set; }
}
```

### Role DTOs
```csharp
// Models/DTOs/RoleDtos.cs
public class AssignRoleDto
{
    public Guid RoleProfileId { get; set; }
}

public class UpdateKsaDto
{
    public int CompetencyLevel { get; set; }
    public List<string> KnowledgeAreas { get; set; }
    public List<string> Skills { get; set; }
    public List<string> Abilities { get; set; }
}

public class RoleResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Layer { get; set; }
    public int ApprovalLevel { get; set; }
    public List<string> Permissions { get; set; }
}
```

### LLM DTOs
```csharp
// Models/DTOs/LlmDtos.cs
public class ConfigureLlmDto
{
    public string Provider { get; set; } // "openai", "azureopenai", "local"
    public string ApiKey { get; set; }
    public string ApiEndpoint { get; set; }
    public string ModelName { get; set; }
    public int MaxTokens { get; set; } = 2000;
    public decimal Temperature { get; set; } = 0.7m;
    public int MonthlyUsageLimit { get; set; } = 10000;
}

public class LlmInsightResponseDto
{
    public bool Success { get; set; }
    public string Content { get; set; }
    public string Provider { get; set; }
    public string Model { get; set; }
}

public class LlmUsageDto
{
    public int TotalCalls { get; set; }
    public int CallsThisMonth { get; set; }
    public int MonthlyLimit { get; set; }
    public decimal UsagePercent { get; set; }
}
```

---

## 🎨 MISSING: UI Pages (Blazor/MVC)

### 1. Workflow Pages
```
Pages/Workflows/
├─ WorkflowList.razor                    ❌ List all workflows
├─ WorkflowDetails.razor                 ❌ View workflow progress
├─ WorkflowCreate.razor                  ❌ Create new workflow
├─ WorkflowExecute.razor                 ❌ Execute workflow tasks
└─ WorkflowHistory.razor                 ❌ View workflow history
```

### 2. Inbox Pages
```
Pages/Inbox/
├─ InboxDashboard.razor                  ❌ User inbox overview
├─ ProcessCard.razor                     ❌ Workflow process visualization
├─ TaskDetail.razor                      ❌ Task details & actions
├─ TaskComments.razor                    ❌ Task communication
└─ BulkActions.razor                     ❌ Approve/reject multiple tasks
```

### 3. Role Management Pages
```
Pages/Administration/
├─ RoleManagement.razor                  ❌ Manage role profiles
├─ RoleAssignment.razor                  ❌ Assign roles to users
├─ UserKsa.razor                         ❌ Manage user KSA
└─ ApprovalHierarchy.razor               ❌ View approval chains
```

### 4. LLM Configuration Pages
```
Pages/Administration/
├─ LlmConfiguration.razor                ❌ Configure AI providers
├─ LlmUsage.razor                        ❌ Monitor API usage
└─ LlmTest.razor                         ❌ Test LLM responses
```

---

## 📊 MISSING: UI COMPONENTS

### Workflow Components
```
Components/Workflows/
├─ WorkflowProgressBar.razor             ❌ Visual progress indicator
├─ ApprovalChainVisual.razor             ❌ Show approval flow
├─ TaskStatusBadge.razor                 ❌ Status indicator
├─ DeadlineAlert.razor                   ❌ Deadline warning
└─ WorkflowTable.razor                   ❌ List workflows in table
```

### Inbox Components
```
Components/Inbox/
├─ SlaStatusIndicator.razor              ❌ 5-color SLA status
├─ ProcessCard.razor                     ❌ Dynamics Flow-style card
├─ TaskCard.razor                        ❌ Individual task display
├─ CommentThread.razor                   ❌ Task comments UI
└─ BulkActionBar.razor                   ❌ Multi-select actions
```

### Shared Components
```
Components/Shared/
├─ RoleProfileBadge.razor                ❌ Display user role
├─ ApprovalLevelIcon.razor               ❌ Approval level indicator
├─ UserAvatar.razor                      ❌ User profile picture
├─ SearchFilter.razor                    ❌ Filter workflows/tasks
└─ PaginationControl.razor               ❌ Page navigation
```

---

## 🔄 PRIORITY ORDER FOR IMPLEMENTATION

### PHASE 1: Core Inbox & Workflow UI (Most Critical)
1. **InboxDashboard.razor** - User's main view
2. **TaskDetail.razor** - Complete task information
3. **ProcessCard.razor** - Visual workflow progress
4. **InboxController.cs** - API endpoints for inbox
5. **SlaStatusIndicator.razor** - Status visualization

### PHASE 2: Workflow Management UI
1. **WorkflowList.razor** - View all workflows
2. **WorkflowCreate.razor** - Create new workflow
3. **WorkflowDetails.razor** - View workflow progress
4. **WorkflowController.cs** - API endpoints
5. **ApprovalChainVisual.razor** - Show approvers

### PHASE 3: Administration UI
1. **RoleManagement.razor** - Manage roles
2. **RoleAssignment.razor** - Assign roles to users
3. **LlmConfiguration.razor** - AI configuration
4. **RoleController.cs** - API endpoints
5. **LlmController.cs** - API endpoints

### PHASE 4: Advanced Features
1. **TaskComments.razor** - Full comment system
2. **LlmUsage.razor** - Monitor AI usage
3. **BulkActions.razor** - Multi-task operations
4. **WorkflowHistory.razor** - Audit trail view

---

## 📝 SAMPLE: API CONTROLLER (WorkflowController.cs)

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Services;
using GrcMvc.Models.DTOs;
using System;
using System.Threading.Tasks;

namespace GrcMvc.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowEngineService _workflowService;
        private readonly IUserWorkspaceService _workspaceService;

        public WorkflowController(
            IWorkflowEngineService workflowService,
            IUserWorkspaceService workspaceService)
        {
            _workflowService = workflowService;
            _workspaceService = workspaceService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorkflow(Guid id)
        {
            try
            {
                var workflow = await _workflowService.GetWorkflowAsync(id);
                if (workflow == null)
                    return NotFound();

                return Ok(new WorkflowResponseDto
                {
                    Id = workflow.Id,
                    WorkflowName = workflow.WorkflowDefinition.Name,
                    Status = workflow.Status,
                    TasksCompleted = workflow.Tasks.Count(t => t.Status == "Completed"),
                    TotalTasks = workflow.Tasks.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserWorkflows(Guid userId)
        {
            try
            {
                var workflows = await _workflowService.GetUserWorkflowsAsync(userId);
                return Ok(workflows);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateWorkflow([FromBody] CreateWorkflowDto dto)
        {
            try
            {
                var workflow = await _workflowService.CreateWorkflowAsync(
                    dto.WorkflowDefinitionId,
                    dto.Priority,
                    dto.DueDate);

                return CreatedAtAction(nameof(GetWorkflow), 
                    new { id = workflow.Id }, workflow);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveWorkflow(Guid id)
        {
            try
            {
                await _workflowService.ApproveWorkflowAsync(id);
                return Ok(new { message = "Workflow approved" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectWorkflow(Guid id)
        {
            try
            {
                await _workflowService.RejectWorkflowAsync(id);
                return Ok(new { message = "Workflow rejected" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{id}/status")]
        public async Task<IActionResult> GetWorkflowStatus(Guid id)
        {
            try
            {
                var status = await _workflowService.GetWorkflowStatusAsync(id);
                return Ok(status);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
```

---

## 📱 SAMPLE: Blazor Page (InboxDashboard.razor)

```razor
@page "/inbox"
@using GrcMvc.Services
@using GrcMvc.Models.DTOs
@inject IInboxService InboxService
@inject HttpClient Http

<div class="inbox-container">
    <h1>My Inbox</h1>
    
    <!-- Summary Cards -->
    <div class="summary-row">
        <div class="summary-card">
            <h3>@TotalTasks</h3>
            <p>Total Tasks</p>
        </div>
        <div class="summary-card pending">
            <h3>@PendingTasks</h3>
            <p>Pending</p>
        </div>
        <div class="summary-card overdue">
            <h3>@OverdueTasks</h3>
            <p>Overdue</p>
        </div>
    </div>

    <!-- Task List -->
    <div class="task-list">
        @foreach (var task in Tasks)
        {
            <div class="task-card">
                <div class="task-header">
                    <h3>@task.TaskName</h3>
                    <span class="status-badge">@task.Status</span>
                </div>
                <div class="task-body">
                    <p>Priority: @task.Priority</p>
                    <p>Due: @task.DueDate?.ToString("MMM dd, yyyy")</p>
                    <SlaStatusIndicator Status="@task.SlaStatus" />
                </div>
                <div class="task-actions">
                    <button @onclick="() => ApproveTask(task.TaskId)">Approve</button>
                    <button @onclick="() => RejectTask(task.TaskId)">Reject</button>
                    <button @onclick="() => ViewDetails(task.TaskId)">View</button>
                </div>
            </div>
        }
    </div>
</div>

@code {
    private List<TaskSummaryDto> Tasks = new();
    private int TotalTasks = 0;
    private int PendingTasks = 0;
    private int OverdueTasks = 0;

    protected override async Task OnInitializedAsync()
    {
        await LoadInbox();
    }

    private async Task LoadInbox()
    {
        var response = await Http.GetAsync("api/inbox");
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadAsAsync<InboxResponseDto>();
            Tasks = data.Tasks;
            TotalTasks = data.TotalTasks;
            PendingTasks = data.PendingTasks;
            OverdueTasks = data.OverdueTasks;
        }
    }

    private async Task ApproveTask(Guid taskId)
    {
        var response = await Http.PostAsync(
            $"api/inbox/task/{taskId}/approve", null);
        if (response.IsSuccessStatusCode)
            await LoadInbox();
    }

    private async Task RejectTask(Guid taskId)
    {
        var response = await Http.PostAsync(
            $"api/inbox/task/{taskId}/reject", null);
        if (response.IsSuccessStatusCode)
            await LoadInbox();
    }

    private void ViewDetails(Guid taskId)
    {
        // Navigate to task detail page
    }
}
```

---

## 📈 IMPLEMENTATION EFFORT ESTIMATE

| Component | Effort | Time |
|-----------|--------|------|
| **API Controllers** (4) | 500 lines | 3-4 hours |
| **DTOs** | 300 lines | 2 hours |
| **Blazor Pages** (6) | 800 lines | 4-5 hours |
| **Blazor Components** (10) | 600 lines | 3-4 hours |
| **CSS/Styling** | 400 lines | 2-3 hours |
| **Testing** | - | 2-3 hours |
| **Total** | **2,600 lines** | **16-22 hours** |

---

## 🎯 NEXT STEPS

### Immediate (Next)
1. Create `WorkflowController.cs` with 8+ endpoints
2. Create `InboxController.cs` with 8+ endpoints
3. Create all DTO classes
4. Create `InboxDashboard.razor` (main page)
5. Create `ProcessCard.razor` (visual component)

### Short-term
1. Create remaining pages
2. Create reusable components
3. Add CSS/Bootstrap styling
4. Wire up all CRUD operations

### Medium-term
1. Add search and filtering
2. Add bulk operations
3. Add dashboards and analytics
4. Add export functionality

---

## ✅ CHECKLIST

- [ ] Create WorkflowController.cs
- [ ] Create InboxController.cs
- [ ] Create RoleController.cs
- [ ] Create LlmController.cs
- [ ] Create all DTO classes
- [ ] Create InboxDashboard.razor
- [ ] Create TaskDetail.razor
- [ ] Create ProcessCard.razor
- [ ] Create WorkflowList.razor
- [ ] Create SlaStatusIndicator.razor
- [ ] Create reusable components
- [ ] Add CSS styling
- [ ] Test all endpoints
- [ ] Test all pages
- [ ] Document APIs

---

## 💡 RECOMMENDATION

**Start with the Inbox API + Dashboard** since it's the most critical user-facing feature:

1. `InboxController.cs` (8 endpoints)
2. `InboxDtos.cs` (5 DTOs)
3. `InboxDashboard.razor` (main page)
4. `SlaStatusIndicator.razor` (status component)
5. `ProcessCard.razor` (workflow visualization)

This gives you a **complete, working inbox feature** in ~6-8 hours.

Then expand to workflows, roles, and LLM management.

---

**Ready to start building the UI/API layer?**
