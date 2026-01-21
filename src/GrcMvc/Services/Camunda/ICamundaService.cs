namespace GrcMvc.Services.Camunda;

/// <summary>
/// Camunda workflow orchestration service interface
/// </summary>
public interface ICamundaService
{
    /// <summary>
    /// Start a new workflow process instance
    /// </summary>
    Task<ProcessInstance> StartProcessAsync(string processKey, Dictionary<string, object>? variables = null, string? businessKey = null, CancellationToken ct = default);
    
    /// <summary>
    /// Complete a user task
    /// </summary>
    Task CompleteTaskAsync(string taskId, Dictionary<string, object>? variables = null, CancellationToken ct = default);
    
    /// <summary>
    /// Get active tasks for a user
    /// </summary>
    Task<IEnumerable<UserTask>> GetTasksForUserAsync(string userId, CancellationToken ct = default);
    
    /// <summary>
    /// Get active tasks for a process instance
    /// </summary>
    Task<IEnumerable<UserTask>> GetTasksForProcessAsync(string processInstanceId, CancellationToken ct = default);
    
    /// <summary>
    /// Claim a task
    /// </summary>
    Task ClaimTaskAsync(string taskId, string userId, CancellationToken ct = default);
    
    /// <summary>
    /// Get process instance status
    /// </summary>
    Task<ProcessInstance?> GetProcessInstanceAsync(string processInstanceId, CancellationToken ct = default);
    
    /// <summary>
    /// Send a message to trigger events in workflows
    /// </summary>
    Task SendMessageAsync(string messageName, Dictionary<string, object>? variables = null, string? businessKey = null, CancellationToken ct = default);
    
    /// <summary>
    /// Deploy a BPMN process definition
    /// </summary>
    Task<string> DeployProcessAsync(string processName, byte[] bpmnContent, CancellationToken ct = default);
    
    /// <summary>
    /// Check if Camunda is available
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
}

/// <summary>
/// Process instance information
/// </summary>
public class ProcessInstance
{
    public string Id { get; set; } = string.Empty;
    public string ProcessDefinitionId { get; set; } = string.Empty;
    public string ProcessDefinitionKey { get; set; } = string.Empty;
    public string BusinessKey { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public bool Ended { get; set; }
    public bool Suspended { get; set; }
}

/// <summary>
/// User task information
/// </summary>
public class UserTask
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string ProcessDefinitionKey { get; set; } = string.Empty;
    public string? Assignee { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Due { get; set; }
    public string? Description { get; set; }
    public int Priority { get; set; }
    public Dictionary<string, object> Variables { get; set; } = new();
}

/// <summary>
/// GRC Workflow Process Keys (BPMN Process IDs)
/// </summary>
public static class GrcProcessKeys
{
    // Assessment Workflows
    public const string AssessmentApproval = "grc-assessment-approval";
    public const string ControlAssessment = "grc-control-assessment";
    
    // Evidence Workflows
    public const string EvidenceReview = "grc-evidence-review";
    public const string EvidenceExpiry = "grc-evidence-expiry";
    
    // Risk Workflows
    public const string RiskAssessment = "grc-risk-assessment";
    public const string RiskMitigation = "grc-risk-mitigation";
    public const string RiskEscalation = "grc-risk-escalation";
    
    // Audit Workflows
    public const string AuditExecution = "grc-audit-execution";
    public const string AuditFindingResolution = "grc-audit-finding-resolution";
    
    // Policy Workflows
    public const string PolicyApproval = "grc-policy-approval";
    public const string PolicyReview = "grc-policy-review";
    
    // Vendor Workflows
    public const string VendorOnboarding = "grc-vendor-onboarding";
    public const string VendorAssessment = "grc-vendor-assessment";
    
    // Incident Workflows
    public const string IncidentResponse = "grc-incident-response";
    
    // Email Workflows
    public const string EmailTriage = "grc-email-triage";
}
