namespace GrcMvc.Models.DTOs;

/// <summary>
/// Result of delegation operation
/// </summary>
public class DelegationResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid DelegationId { get; set; }
    public Guid TaskId { get; set; }
    public string FromType { get; set; } = string.Empty; // Human, Agent
    public Guid? FromUserId { get; set; }
    public string? FromAgentType { get; set; }
    public string ToType { get; set; } = string.Empty; // Human, Agent, MultipleAgents
    public Guid? ToUserId { get; set; }
    public List<string>? ToAgentTypes { get; set; }
    public DateTime DelegatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Result of swap operation
/// </summary>
public class SwapResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid Task1Id { get; set; }
    public Guid Task2Id { get; set; }
    public Guid? Task1NewAssigneeId { get; set; }
    public string? Task1NewAgentType { get; set; }
    public Guid? Task2NewAssigneeId { get; set; }
    public string? Task2NewAgentType { get; set; }
    public DateTime SwappedAt { get; set; }
}

/// <summary>
/// Delegation history entry
/// </summary>
public class DelegationHistoryDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public string FromType { get; set; } = string.Empty;
    public Guid? FromUserId { get; set; }
    public string? FromUserName { get; set; }
    public string? FromAgentType { get; set; }
    public string ToType { get; set; } = string.Empty;
    public Guid? ToUserId { get; set; }
    public string? ToUserName { get; set; }
    public string? ToAgentType { get; set; }
    public string Action { get; set; } = string.Empty; // Delegated, Revoked, Completed
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Request to delegate a task
/// </summary>
public class DelegateTaskRequestDto
{
    public Guid TaskId { get; set; }
    public string ToType { get; set; } = "Human"; // Human, Agent, MultipleAgents
    public Guid? ToUserId { get; set; }
    public string? ToAgentType { get; set; }
    public List<string>? ToAgentTypes { get; set; }
    public string? Reason { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string DelegationStrategy { get; set; } = "Parallel"; // For multi-agent: Parallel, Sequential, FirstAvailable
}

/// <summary>
/// Request to swap tasks
/// </summary>
public class SwapTasksRequestDto
{
    public Guid Task1Id { get; set; }
    public Guid Task2Id { get; set; }
    public string? Reason { get; set; }
}
