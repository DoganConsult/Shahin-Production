# Workflow Infrastructure - Quick Reference Guide

## 🚀 Quick Start

### Using the Workflow REST API

```csharp
// Inject the service
IWorkflowEngineService _workflowService

// Create a workflow
var workflowId = await _workflowService.CreateWorkflowAsync(
    tenantId: tenantId,
    definitionId: workflowDefinitionId,
    priority: "High",
    createdBy: userId
);

// Get workflow details
var workflow = await _workflowService.GetWorkflowAsync(tenantId, workflowId);

// Approve workflow
await _workflowService.ApproveWorkflowAsync(
    tenantId: tenantId,
    workflowId: workflowId,
    approvalLevel: 1,
    approvedBy: userId,
    reason: "Looks good"
);

// Get statistics
var stats = await _workflowService.GetStatisticsAsync(tenantId);
```

### REST API Endpoints

```bash
# Create workflow
POST /api/workflows
{
    "workflowDefinitionId": "guid",
    "priority": "High",
    "dueDate": "2025-01-15"
}

# List workflows
GET /api/workflows?page=1&pageSize=10

# Get workflow details
GET /api/workflows/{id}

# Approve workflow
POST /api/workflows/{id}/approve
{
    "reason": "Approved by manager"
}

# Reject workflow
POST /api/workflows/{id}/reject
{
    "reason": "Need more information"
}

# Complete task
POST /api/workflows/{id}/task/{taskId}/complete
{
    "notes": "Task completed successfully"
}

# Get progress
GET /api/workflows/{id}/status

# Get audit trail
GET /api/workflows/{id}/history

# Get available templates
GET /api/workflows/definitions/available

# Get statistics
GET /api/workflows/stats/dashboard
```

---

## 📋 Workflow Definitions

### 7 Built-in Workflows

| Code | Name | Category | Type | SLA |
|------|------|----------|------|-----|
| WF-001 | NCA ECC Assessment | Assessment | Sequential | 48h |
| WF-002 | SAMA Cyber Assessment | Assessment | Sequential | 60h |
| WF-003 | Evidence Evaluation | Review | Sequential | 40h |
| WF-004 | Finding Remediation | Remediation | Sequential | 168h |
| WF-005 | Policy Review & Approval | Approval | Sequential | 144h |
| WF-006 | Risk Approval | Approval | Parallel | 120h |
| WF-007 | Compliance Audit | Assessment | Sequential | 192h |

### Creating Custom Workflows

```sql
INSERT INTO WorkflowDefinitions 
(Id, TenantId, WorkflowNumber, Name, Description, Category, Framework, Type, TriggerType, Status, IsTemplate, Version, BpmnXml, Steps, RequiredPermission)
VALUES 
(NEWID(), @tenantId, 'WF-008', 'My Custom Workflow', '...', 'Assessment', 'Custom', 'Sequential', 'Manual', 'Active', 1, true, '<bpmn>...', '[{...}]', 'Grc.Custom.Execute')
```

---

## 🔏 Approval Workflow

### Submit for Approval

```csharp
var approvalService = serviceProvider.GetRequiredService<IApprovalWorkflowService>();

var approvalId = await approvalService.SubmitForApprovalAsync(
    tenantId: tenantId,
    workflowId: workflowId,
    submittedBy: userId,
    comments: "Please review this workflow"
);
```

### Get Pending Approvals

```csharp
var pendingApprovals = await approvalService.GetPendingApprovalsAsync(tenantId, userId);
// Returns list of ApprovalDto with all details
```

### Approve/Reject

```csharp
// Approve
await approvalService.ApproveAsync(
    tenantId: tenantId,
    approvalId: approvalId,
    approverId: userId,
    comments: "Approved - ready to proceed"
);

// Reject
await approvalService.RejectAsync(
    tenantId: tenantId,
    approvalId: approvalId,
    approverId: userId,
    rejectionReason: "Needs revision"
);
```

### Delegate Approval

```csharp
await approvalService.DelegateAsync(
    tenantId: tenantId,
    approvalId: approvalId,
    fromUserId: currentUserId,
    toUserId: delegateUserId,
    reason: "On vacation"
);
```

### Approval Chain

```csharp
var chain = await approvalService.GetApprovalChainAsync(tenantId, workflowId);
// Returns:
// - Level 1: Department Head (24h SLA)
// - Level 2: Manager (24h SLA)  
// - Level 3: Director (24h SLA)
// - Level 4: Executive (48h SLA) [if Critical]
```

---

## ⏰ Escalation Management

### Check for Overdue Workflows

```csharp
var escalationService = serviceProvider.GetRequiredService<IEscalationService>();

int escalatedCount = await escalationService.ProcessEscalationsAsync(tenantId);
// Runs automatically every hour
// Returns count of workflows escalated
```

### Get Escalations for Approval

```csharp
var escalations = await escalationService.GetEscalationsAsync(tenantId, approvalId);
// Returns list of EscalationDto with hours overdue
```

### Manually Escalate

```csharp
await escalationService.EscalateApprovalAsync(
    tenantId: tenantId,
    approvalId: approvalId,
    escalatedBy: userId,
    reason: "SLA breach - escalating to director"
);
```

### SLA Configuration

```csharp
// Get current SLA config
var config = await escalationService.GetEscalationConfigAsync(tenantId, workflowDefinitionId);

// Update SLA rules
await escalationService.UpdateEscalationRulesAsync(
    tenantId: tenantId,
    workflowDefinitionId: workflowDefinitionId,
    rules: new List<EscalationRuleDto>
    {
        new() { Level = 1, HoursOverdue = 24, EscalateTo = "Manager" },
        new() { Level = 2, HoursOverdue = 48, EscalateTo = "Director" },
        new() { Level = 3, HoursOverdue = 120, EscalateTo = "Executive" }
    }
);
```

---

## 📊 Dashboard Statistics

### Workflow Stats

```csharp
var stats = await workflowService.GetStatisticsAsync(tenantId);
// Returns:
// - TotalWorkflows
// - CompletedCount
// - InProgressCount  
// - RejectedCount
// - AverageCompletionHours
```

### Approval Stats

```csharp
var approvalStats = await approvalService.GetApprovalStatsAsync(tenantId);
// Returns:
// - TotalPending
// - Overdue
// - AverageTurnaroundHours
// - CompletionRate
```

### Escalation Stats

```csharp
var escalationStats = await escalationService.GetEscalationStatsAsync(tenantId);
// Returns:
// - TotalEscalations
// - ActiveEscalations
// - ResolvedEscalations
// - AverageHoursToResolve
```

---

## 🎨 UI Views

### Available Views

| View | URL | Purpose |
|------|-----|---------|
| **Inbox** | `/Workflow/Inbox` | Task dashboard & quick actions |
| **Approvals** | `/Workflow/Approvals` | Pending approvals management |
| **Process Flow** | `/Workflow/ProcessFlow` | Visual workflow execution |
| **Escalations** | `/Workflow/Escalations` | SLA monitoring & alerts |
| **Index** | `/Workflow/Index` | Main workflow list |
| **Details** | `/Workflow/Details/{id}` | Workflow details |
| **Statistics** | `/Workflow/Statistics` | Dashboard analytics |

### View Integration Example

```html
<!-- Approvals Tab -->
<ul class="nav nav-tabs">
    <li class="nav-item">
        <a class="nav-link" href="/Workflow/Approvals">
            <i class="fas fa-check-circle"></i> Approvals
            <span class="badge bg-warning">3</span>
        </a>
    </li>
</ul>
```

---

## 🔐 Security & Authorization

### Multi-Tenant Isolation

All operations are automatically tenant-scoped:

```csharp
// Automatically filters by tenantId
var userWorkflows = await workflowService.GetUserWorkflowsAsync(
    tenantId: currentUserTenantId,  // Required
    userId: currentUserId,
    page: 1,
    pageSize: 10
);
```

### Required Permissions

```csharp
// Workflow execution requires permission from WorkflowDefinition.RequiredPermission
// Example: "Grc.Workflows.Execute"
// Example: "Grc.Assessments.Execute"
// Example: "Grc.Approvals.Execute"

[Authorize(Roles = "Admin,Manager")]
public async Task<IActionResult> ApproveWorkflow(Guid id) { ... }
```

---

## 🐛 Troubleshooting

### Common Issues

**Issue:** Workflow not appearing in list
- **Cause:** Different tenant ID
- **Fix:** Verify `tenantId` matches current user's tenant

**Issue:** Approval not advancing to next level
- **Cause:** Task not completed
- **Fix:** Complete all workflow tasks first

**Issue:** Escalation not triggering
- **Cause:** EscalationRule not configured
- **Fix:** Add escalation rules for workflow type

**Issue:** API returns 401
- **Cause:** Not authenticated
- **Fix:** Add Authorization header with JWT token

---

## 📝 Logging

All operations are logged with structured logging:

```
✅ Approval 550e8400-e29b-41d4-a716-446655440000 created for workflow 123e4567-e89b-12d3-a456-426614174000
✅ Workflow 123e4567-e89b-12d3-a456-426614174000 approved by John Smith
⚠️ Workflow 123e4567-e89b-12d3-a456-426614174000 rejected by Jane Doe
❌ Error approving workflow: Operation timed out
```

### Log Levels

- **Information (ℹ️):** Workflow created, approval submitted, escalation processed
- **Warning (⚠️):** SLA breach, escalation triggered, overdue item
- **Error (❌):** Failed operations, database errors, authorization failures

---

## 🔗 Integration Points

### With Other Systems

```csharp
// Notification Service
var notifications = GetNotificationService();
await notifications.SendAsync(
    userId: approverId,
    title: "Approval Needed",
    message: $"Workflow {workflow.Number} needs your approval",
    link: $"/Workflow/Approvals"
);

// Audit Service
var auditService = GetAuditService();
await auditService.LogAsync(
    tenantId: tenantId,
    userId: currentUserId,
    action: "WorkflowApproved",
    entityId: workflowId,
    details: "Approved by manager"
);

// Reporting Service
var reportService = GetReportService();
var report = await reportService.GenerateAsync(
    template: "WorkflowCompletionReport",
    parameters: new { tenantId, dateRange = "month" }
);
```

---

## 📚 References

- **REST API Documentation:** `/api/swagger/ui`
- **Database Schema:** Database Migrations
- **Service Interfaces:** `Services/Interfaces/I*.cs`
- **Entity Models:** `Models/Entities/Workflow*.cs`
- **Views:** `Views/Workflow/`

---

## 💡 Best Practices

✅ **Always include tenantId** - Multi-tenant isolation is required
✅ **Use async methods** - All service methods are async
✅ **Handle exceptions** - API calls can fail
✅ **Log important actions** - Use ILogger for debugging
✅ **Validate inputs** - Check workflow ID, user ID, etc.
✅ **Use DTOs** - Return data objects, not entities
✅ **Test with real data** - Use workflow definitions from seed data

---

**Last Updated:** January 5, 2025  
**Version:** 1.0  
**Status:** Production Ready ✅
