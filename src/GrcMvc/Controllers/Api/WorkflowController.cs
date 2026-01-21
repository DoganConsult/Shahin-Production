using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Services.Camunda;
using GrcMvc.Services.Kafka;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api;

/// <summary>
/// Workflow orchestration API controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkflowController : ControllerBase
{
    private readonly ICamundaService _camundaService;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly ILogger<WorkflowController> _logger;

    public WorkflowController(
        ICamundaService camundaService,
        IKafkaProducer kafkaProducer,
        ILogger<WorkflowController> logger)
    {
        _camundaService = camundaService;
        _kafkaProducer = kafkaProducer;
        _logger = logger;
    }

    /// <summary>
    /// Get workflow engine status
    /// </summary>
    [HttpGet("status")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStatus()
    {
        var camundaAvailable = await _camundaService.IsAvailableAsync();
        
        return Ok(new
        {
            camunda = new
            {
                available = camundaAvailable,
                description = "Camunda BPM for workflow orchestration"
            },
            kafka = new
            {
                available = true, // Assume Kafka is available if app started
                description = "Kafka for event-driven architecture"
            },
            processes = new[]
            {
                new { key = GrcProcessKeys.AssessmentApproval, name = "Assessment Approval Workflow" },
                new { key = GrcProcessKeys.RiskAssessment, name = "Risk Assessment Workflow" },
                new { key = GrcProcessKeys.EvidenceReview, name = "Evidence Review Workflow" },
                new { key = GrcProcessKeys.PolicyApproval, name = "Policy Approval Workflow" },
                new { key = GrcProcessKeys.AuditExecution, name = "Audit Execution Workflow" },
                new { key = GrcProcessKeys.VendorOnboarding, name = "Vendor Onboarding Workflow" }
            }
        });
    }

    /// <summary>
    /// Start assessment approval workflow
    /// </summary>
    [HttpPost("assessment/start")]
    public async Task<IActionResult> StartAssessmentWorkflow([FromBody] StartAssessmentWorkflowRequest request)
    {
        var variables = new Dictionary<string, object>
        {
            ["assessmentId"] = request.AssessmentId.ToString(),
            ["frameworkId"] = request.FrameworkId.ToString(),
            ["submitterId"] = request.SubmitterId,
            ["reviewerId"] = request.ReviewerId,
            ["tenantId"] = request.TenantId.ToString()
        };

        var instance = await _camundaService.StartProcessAsync(
            GrcProcessKeys.AssessmentApproval,
            variables,
            request.AssessmentId.ToString()
        );

        // Publish event to Kafka
        await _kafkaProducer.PublishAsync(KafkaTopics.WorkflowStarted, new
        {
            workflowId = instance.Id,
            workflowType = GrcProcessKeys.AssessmentApproval,
            tenantId = request.TenantId,
            startedAt = DateTime.UtcNow,
            variables
        });

        _logger.LogInformation("Started assessment workflow: {WorkflowId}", instance.Id);

        return Ok(new { workflowId = instance.Id, status = "started" });
    }

    /// <summary>
    /// Start risk assessment workflow
    /// </summary>
    [HttpPost("risk/start")]
    public async Task<IActionResult> StartRiskWorkflow([FromBody] StartRiskWorkflowRequest request)
    {
        var variables = new Dictionary<string, object>
        {
            ["riskId"] = request.RiskId.ToString(),
            ["riskTitle"] = request.Title,
            ["riskOwnerId"] = request.OwnerId,
            ["tenantId"] = request.TenantId.ToString()
        };

        var instance = await _camundaService.StartProcessAsync(
            GrcProcessKeys.RiskAssessment,
            variables,
            request.RiskId.ToString()
        );

        await _kafkaProducer.PublishAsync(KafkaTopics.RiskIdentified, new
        {
            riskId = request.RiskId,
            title = request.Title,
            severity = "Pending Assessment",
            tenantId = request.TenantId
        });

        return Ok(new { workflowId = instance.Id, status = "started" });
    }

    /// <summary>
    /// Get tasks for current user
    /// </summary>
    [HttpGet("tasks")]
    public async Task<IActionResult> GetMyTasks()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var tasks = await _camundaService.GetTasksForUserAsync(userId);
        
        return Ok(tasks);
    }

    /// <summary>
    /// Get tasks for a specific workflow
    /// </summary>
    [HttpGet("{workflowId}/tasks")]
    public async Task<IActionResult> GetWorkflowTasks(string workflowId)
    {
        var tasks = await _camundaService.GetTasksForProcessAsync(workflowId);
        return Ok(tasks);
    }

    /// <summary>
    /// Claim a task
    /// </summary>
    [HttpPost("tasks/{taskId}/claim")]
    public async Task<IActionResult> ClaimTask(string taskId)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        await _camundaService.ClaimTaskAsync(taskId, userId);
        
        return Ok(new { message = "Task claimed successfully" });
    }

    /// <summary>
    /// Complete a task
    /// </summary>
    [HttpPost("tasks/{taskId}/complete")]
    public async Task<IActionResult> CompleteTask(string taskId, [FromBody] CompleteTaskRequest request)
    {
        await _camundaService.CompleteTaskAsync(taskId, request.Variables);

        await _kafkaProducer.PublishAsync(KafkaTopics.TaskCompleted, new
        {
            taskId,
            completedBy = User.Identity?.Name,
            completedAt = DateTime.UtcNow
        });

        return Ok(new { message = "Task completed successfully" });
    }

    /// <summary>
    /// Get workflow instance status
    /// </summary>
    [HttpGet("{workflowId}/status")]
    public async Task<IActionResult> GetWorkflowStatus(string workflowId)
    {
        var instance = await _camundaService.GetProcessInstanceAsync(workflowId);
        
        if (instance == null)
            return NotFound(new { error = "Workflow not found" });

        return Ok(instance);
    }

    /// <summary>
    /// Publish custom event to Kafka
    /// </summary>
    [HttpPost("events/publish")]
    public async Task<IActionResult> PublishEvent([FromBody] PublishEventRequest request)
    {
        await _kafkaProducer.PublishAsync(request.Topic, request.Payload, request.Key);
        
        _logger.LogInformation("Published event to {Topic}", request.Topic);
        
        return Ok(new { message = "Event published", topic = request.Topic });
    }
}

// Request DTOs
public class StartAssessmentWorkflowRequest
{
    public Guid AssessmentId { get; set; }
    public Guid FrameworkId { get; set; }
    public string SubmitterId { get; set; } = string.Empty;
    public string ReviewerId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
}

public class StartRiskWorkflowRequest
{
    public Guid RiskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
}

public class CompleteTaskRequest
{
    public Dictionary<string, object>? Variables { get; set; }
}

public class PublishEventRequest
{
    public string Topic { get; set; } = string.Empty;
    public object Payload { get; set; } = new();
    public string? Key { get; set; }
}
