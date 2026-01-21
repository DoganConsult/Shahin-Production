using GrcMvc.Models.DTOs;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Service for role delegation and swapping between humans and agents
/// Supports: Human↔Human, Human↔Agent, Agent↔Agent, Multi-Agent delegation
/// </summary>
public interface IRoleDelegationService
{
    /// <summary>
    /// Delegate a task from one user to another (Human to Human)
    /// </summary>
    Task<DelegationResultDto> DelegateTaskAsync(
        Guid tenantId,
        Guid taskId,
        Guid fromUserId,
        Guid toUserId,
        string? reason = null,
        DateTime? expiresAt = null);

    /// <summary>
    /// Delegate a task from human to agent (Human to Agent)
    /// </summary>
    Task<DelegationResultDto> DelegateToAgentAsync(
        Guid tenantId,
        Guid taskId,
        Guid fromUserId,
        string agentType,
        string? reason = null);

    /// <summary>
    /// Delegate a task from agent to human (Agent to Human)
    /// </summary>
    Task<DelegationResultDto> DelegateToHumanAsync(
        Guid tenantId,
        Guid taskId,
        string fromAgentType,
        Guid toUserId,
        string? reason = null);

    /// <summary>
    /// Delegate a task from one agent to another (Agent to Agent)
    /// </summary>
    Task<DelegationResultDto> DelegateBetweenAgentsAsync(
        Guid tenantId,
        Guid taskId,
        string fromAgentType,
        string toAgentType,
        string? reason = null);

    /// <summary>
    /// Delegate a task to multiple agents (Multi-Agent)
    /// </summary>
    Task<DelegationResultDto> DelegateToMultipleAgentsAsync(
        Guid tenantId,
        Guid taskId,
        Guid fromUserId,
        List<string> agentTypes,
        string delegationStrategy = "Parallel", // Parallel, Sequential, FirstAvailable
        string? reason = null);

    /// <summary>
    /// Swap roles/tasks between two users (Human to Human swap)
    /// </summary>
    Task<SwapResultDto> SwapTasksAsync(
        Guid tenantId,
        Guid task1Id,
        Guid task2Id,
        Guid user1Id,
        Guid user2Id,
        string? reason = null);

    /// <summary>
    /// Swap role assignment between human and agent
    /// </summary>
    Task<SwapResultDto> SwapHumanAgentAsync(
        Guid tenantId,
        Guid taskId,
        Guid userId,
        string agentType,
        string? reason = null);

    /// <summary>
    /// Get delegation history for a task
    /// </summary>
    Task<List<DelegationHistoryDto>> GetDelegationHistoryAsync(Guid tenantId, Guid taskId);

    /// <summary>
    /// Revoke a delegation
    /// </summary>
    Task<bool> RevokeDelegationAsync(Guid tenantId, Guid delegationId, Guid revokedByUserId);
}
