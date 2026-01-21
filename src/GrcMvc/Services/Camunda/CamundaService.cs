using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace GrcMvc.Services.Camunda;

/// <summary>
/// Camunda REST API client implementation
/// </summary>
public class CamundaService : ICamundaService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CamundaService> _logger;
    private readonly CamundaSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;

    public CamundaService(
        HttpClient httpClient,
        IOptions<CamundaSettings> settings,
        ILogger<CamundaService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
        
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        if (!_settings.Enabled) return false;
        
        try
        {
            var response = await _httpClient.GetAsync("/engine-rest/engine", ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ProcessInstance> StartProcessAsync(
        string processKey, 
        Dictionary<string, object>? variables = null, 
        string? businessKey = null,
        CancellationToken ct = default)
    {
        var url = $"/engine-rest/process-definition/key/{processKey}/start";
        
        var request = new
        {
            businessKey = businessKey ?? Guid.NewGuid().ToString(),
            variables = ConvertToVariables(variables)
        };

        var response = await _httpClient.PostAsJsonAsync(url, request, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CamundaProcessInstance>(_jsonOptions, ct);
        
        _logger.LogInformation("Started process {ProcessKey}: {InstanceId}", processKey, result?.Id);
        
        return MapToProcessInstance(result!);
    }

    public async Task CompleteTaskAsync(string taskId, Dictionary<string, object>? variables = null, CancellationToken ct = default)
    {
        var url = $"/engine-rest/task/{taskId}/complete";
        
        var request = new
        {
            variables = ConvertToVariables(variables)
        };

        var response = await _httpClient.PostAsJsonAsync(url, request, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();
        
        _logger.LogInformation("Completed task: {TaskId}", taskId);
    }

    public async Task<IEnumerable<UserTask>> GetTasksForUserAsync(string userId, CancellationToken ct = default)
    {
        var url = $"/engine-rest/task?assignee={userId}";
        
        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var tasks = await response.Content.ReadFromJsonAsync<List<CamundaTask>>(_jsonOptions, ct);
        return tasks?.Select(MapToUserTask) ?? Enumerable.Empty<UserTask>();
    }

    public async Task<IEnumerable<UserTask>> GetTasksForProcessAsync(string processInstanceId, CancellationToken ct = default)
    {
        var url = $"/engine-rest/task?processInstanceId={processInstanceId}";
        
        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var tasks = await response.Content.ReadFromJsonAsync<List<CamundaTask>>(_jsonOptions, ct);
        return tasks?.Select(MapToUserTask) ?? Enumerable.Empty<UserTask>();
    }

    public async Task ClaimTaskAsync(string taskId, string userId, CancellationToken ct = default)
    {
        var url = $"/engine-rest/task/{taskId}/claim";
        
        var request = new { userId };
        var response = await _httpClient.PostAsJsonAsync(url, request, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();
        
        _logger.LogInformation("Task {TaskId} claimed by {UserId}", taskId, userId);
    }

    public async Task<ProcessInstance?> GetProcessInstanceAsync(string processInstanceId, CancellationToken ct = default)
    {
        var url = $"/engine-rest/process-instance/{processInstanceId}";
        
        try
        {
            var response = await _httpClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<CamundaProcessInstance>(_jsonOptions, ct);
            return result != null ? MapToProcessInstance(result) : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task SendMessageAsync(
        string messageName, 
        Dictionary<string, object>? variables = null, 
        string? businessKey = null,
        CancellationToken ct = default)
    {
        var url = "/engine-rest/message";
        
        var request = new
        {
            messageName,
            businessKey,
            processVariables = ConvertToVariables(variables)
        };

        var response = await _httpClient.PostAsJsonAsync(url, request, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();
        
        _logger.LogInformation("Sent message: {MessageName}", messageName);
    }

    public async Task<string> DeployProcessAsync(string processName, byte[] bpmnContent, CancellationToken ct = default)
    {
        var url = "/engine-rest/deployment/create";
        
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(processName), "deployment-name");
        content.Add(new StringContent("true"), "enable-duplicate-filtering");
        content.Add(new ByteArrayContent(bpmnContent), "data", $"{processName}.bpmn");

        var response = await _httpClient.PostAsync(url, content, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions, ct);
        var deploymentId = result.GetProperty("id").GetString() ?? "";
        
        _logger.LogInformation("Deployed process {ProcessName}: {DeploymentId}", processName, deploymentId);
        
        return deploymentId;
    }

    private static Dictionary<string, object>? ConvertToVariables(Dictionary<string, object>? variables)
    {
        if (variables == null || variables.Count == 0) return null;
        
        return variables.ToDictionary(
            kv => kv.Key,
            kv => (object)new { value = kv.Value, type = GetVariableType(kv.Value) }
        );
    }

    private static string GetVariableType(object value) => value switch
    {
        string => "String",
        int or long => "Long",
        double or float or decimal => "Double",
        bool => "Boolean",
        DateTime => "Date",
        _ => "Object"
    };

    private static ProcessInstance MapToProcessInstance(CamundaProcessInstance p) => new()
    {
        Id = p.Id,
        ProcessDefinitionId = p.DefinitionId,
        ProcessDefinitionKey = p.DefinitionId?.Split(':')[0] ?? "",
        BusinessKey = p.BusinessKey ?? "",
        TenantId = p.TenantId ?? "",
        Ended = p.Ended,
        Suspended = p.Suspended
    };

    private static UserTask MapToUserTask(CamundaTask t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        ProcessInstanceId = t.ProcessInstanceId,
        ProcessDefinitionKey = t.ProcessDefinitionId?.Split(':')[0] ?? "",
        Assignee = t.Assignee,
        Created = t.Created,
        Due = t.Due,
        Description = t.Description,
        Priority = t.Priority
    };
}

// Camunda REST API DTOs
internal class CamundaProcessInstance
{
    public string Id { get; set; } = "";
    public string DefinitionId { get; set; } = "";
    public string? BusinessKey { get; set; }
    public string? TenantId { get; set; }
    public bool Ended { get; set; }
    public bool Suspended { get; set; }
}

internal class CamundaTask
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string ProcessInstanceId { get; set; } = "";
    public string? ProcessDefinitionId { get; set; }
    public string? Assignee { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Due { get; set; }
    public string? Description { get; set; }
    public int Priority { get; set; }
}

/// <summary>
/// Camunda configuration settings
/// </summary>
public class CamundaSettings
{
    public const string SectionName = "Camunda";
    
    public bool Enabled { get; set; } = true;
    public string BaseUrl { get; set; } = "http://localhost:8080/camunda";
    public string Username { get; set; } = "demo";
    public string Password { get; set; } = "demo";
}
