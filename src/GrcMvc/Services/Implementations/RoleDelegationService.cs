using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Service for role delegation and swapping between humans and agents
/// Supports: Human↔Human, Human↔Agent, Agent↔Agent, Multi-Agent delegation
/// </summary>
public class RoleDelegationService : IRoleDelegationService
{
    private readonly GrcDbContext _context;
    private readonly ILogger<RoleDelegationService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleDelegationService(
        GrcDbContext context,
        ILogger<RoleDelegationService> logger,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
    }

    /// <summary>
    /// Delegate a task from one user to another (Human to Human)
    /// </summary>
    public async Task<DelegationResultDto> DelegateTaskAsync(
        Guid tenantId,
        Guid taskId,
        Guid fromUserId,
        Guid toUserId,
        string? reason = null,
        DateTime? expiresAt = null)
    {
        try
        {
            var task = await _context.WorkflowTasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.TenantId == tenantId);

            if (task == null)
                throw new InvalidOperationException($"Task {taskId} not found");

            if (task.AssignedToUserId != fromUserId)
                throw new InvalidOperationException($"Task is not assigned to user {fromUserId}");

            var fromUser = await _userManager.FindByIdAsync(fromUserId.ToString());
            var toUser = await _userManager.FindByIdAsync(toUserId.ToString());

            if (fromUser == null || toUser == null)
                throw new InvalidOperationException("User not found");

            // Create delegation record
            var delegation = new TaskDelegation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TaskId = taskId,
                WorkflowInstanceId = task.WorkflowInstanceId,
                FromType = "Human",
                FromUserId = fromUserId,
                FromUserName = fromUser.FullName,
                ToType = "Human",
                ToUserId = toUserId,
                ToUserName = toUser.FullName,
                Action = "Delegated",
                Reason = reason,
                DelegatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                IsActive = true
            };

            // Update task assignment
            task.AssignedToUserId = toUserId;
            task.AssignedToUserName = toUser.FullName;

            _context.TaskDelegations.Add(delegation);
            _context.WorkflowTasks.Update(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} delegated from {FromUser} to {ToUser}", 
                taskId, fromUser.FullName, toUser.FullName);

            return new DelegationResultDto
            {
                Success = true,
                Message = $"Task delegated from {fromUser.FullName} to {toUser.FullName}",
                DelegationId = delegation.Id,
                TaskId = taskId,
                FromType = "Human",
                FromUserId = fromUserId,
                ToType = "Human",
                ToUserId = toUserId,
                DelegatedAt = delegation.DelegatedAt,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error delegating task {TaskId}", taskId);
            throw;
        }
    }

    /// <summary>
    /// Delegate a task from human to agent (Human to Agent)
    /// </summary>
    public async Task<DelegationResultDto> DelegateToAgentAsync(
        Guid tenantId,
        Guid taskId,
        Guid fromUserId,
        string agentType,
        string? reason = null)
    {
        try
        {
            var task = await _context.WorkflowTasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.TenantId == tenantId);

            if (task == null)
                throw new InvalidOperationException($"Task {taskId} not found");

            var fromUser = await _userManager.FindByIdAsync(fromUserId.ToString());
            if (fromUser == null)
                throw new InvalidOperationException($"User {fromUserId} not found");

            // Validate agent type
            var validAgentTypes = new[] { "ComplianceAgent", "RiskAgent", "AuditAgent", "PolicyAgent", 
                "WorkflowAgent", "AnalyticsAgent", "IntegrationAgent", "SecurityAgent", "ReportingAgent" };
            
            if (!validAgentTypes.Contains(agentType))
                throw new InvalidOperationException($"Invalid agent type: {agentType}");

            // Create delegation record
            var delegation = new TaskDelegation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TaskId = taskId,
                WorkflowInstanceId = task.WorkflowInstanceId,
                FromType = "Human",
                FromUserId = fromUserId,
                FromUserName = fromUser.FullName,
                ToType = "Agent",
                ToAgentType = agentType,
                Action = "Delegated",
                Reason = reason ?? $"Delegated to {agentType} for automated processing",
                DelegatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Update task - agent assignment (store in metadata or separate field)
            // For now, we'll use AssignedToUserName to store agent type
            task.AssignedToUserId = null; // No user assigned
            task.AssignedToUserName = $"[AGENT]{agentType}";
            task.Metadata = JsonSerializer.Serialize(new { AgentType = agentType, DelegatedFrom = fromUserId });

            _context.TaskDelegations.Add(delegation);
            _context.WorkflowTasks.Update(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} delegated from {FromUser} to agent {AgentType}", 
                taskId, fromUser.FullName, agentType);

            return new DelegationResultDto
            {
                Success = true,
                Message = $"Task delegated from {fromUser.FullName} to {agentType}",
                DelegationId = delegation.Id,
                TaskId = taskId,
                FromType = "Human",
                FromUserId = fromUserId,
                ToType = "Agent",
                ToAgentTypes = new List<string> { agentType },
                DelegatedAt = delegation.DelegatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error delegating task {TaskId} to agent {AgentType}", taskId, agentType);
            throw;
        }
    }

    /// <summary>
    /// Delegate a task from agent to human (Agent to Human)
    /// </summary>
    public async Task<DelegationResultDto> DelegateToHumanAsync(
        Guid tenantId,
        Guid taskId,
        string fromAgentType,
        Guid toUserId,
        string? reason = null)
    {
        try
        {
            var task = await _context.WorkflowTasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.TenantId == tenantId);

            if (task == null)
                throw new InvalidOperationException($"Task {taskId} not found");

            // Verify task is currently assigned to agent
            if (!task.AssignedToUserName?.StartsWith("[AGENT]") == true)
                throw new InvalidOperationException($"Task is not assigned to an agent");

            var toUser = await _userManager.FindByIdAsync(toUserId.ToString());
            if (toUser == null)
                throw new InvalidOperationException($"User {toUserId} not found");

            // Create delegation record
            var delegation = new TaskDelegation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TaskId = taskId,
                WorkflowInstanceId = task.WorkflowInstanceId,
                FromType = "Agent",
                FromAgentType = fromAgentType,
                ToType = "Human",
                ToUserId = toUserId,
                ToUserName = toUser.FullName,
                Action = "Delegated",
                Reason = reason ?? $"Delegated from {fromAgentType} to human for review",
                DelegatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Update task assignment
            task.AssignedToUserId = toUserId;
            task.AssignedToUserName = toUser.FullName;
            task.Metadata = null; // Clear agent metadata

            _context.TaskDelegations.Add(delegation);
            _context.WorkflowTasks.Update(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} delegated from agent {AgentType} to {ToUser}", 
                taskId, fromAgentType, toUser.FullName);

            return new DelegationResultDto
            {
                Success = true,
                Message = $"Task delegated from {fromAgentType} to {toUser.FullName}",
                DelegationId = delegation.Id,
                TaskId = taskId,
                FromType = "Agent",
                FromAgentType = fromAgentType,
                ToType = "Human",
                ToUserId = toUserId,
                DelegatedAt = delegation.DelegatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error delegating task {TaskId} from agent {AgentType} to human", taskId, fromAgentType);
            throw;
        }
    }

    /// <summary>
    /// Delegate a task from one agent to another (Agent to Agent)
    /// </summary>
    public async Task<DelegationResultDto> DelegateBetweenAgentsAsync(
        Guid tenantId,
        Guid taskId,
        string fromAgentType,
        string toAgentType,
        string? reason = null)
    {
        try
        {
            var task = await _context.WorkflowTasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.TenantId == tenantId);

            if (task == null)
                throw new InvalidOperationException($"Task {taskId} not found");

            // Validate agent types
            var validAgentTypes = new[] { "ComplianceAgent", "RiskAgent", "AuditAgent", "PolicyAgent", 
                "WorkflowAgent", "AnalyticsAgent", "IntegrationAgent", "SecurityAgent", "ReportingAgent" };
            
            if (!validAgentTypes.Contains(fromAgentType) || !validAgentTypes.Contains(toAgentType))
                throw new InvalidOperationException("Invalid agent type");

            // Create delegation record
            var delegation = new TaskDelegation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TaskId = taskId,
                WorkflowInstanceId = task.WorkflowInstanceId,
                FromType = "Agent",
                FromAgentType = fromAgentType,
                ToType = "Agent",
                ToAgentType = toAgentType,
                Action = "Delegated",
                Reason = reason ?? $"Delegated from {fromAgentType} to {toAgentType}",
                DelegatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Update task assignment
            task.AssignedToUserId = null;
            task.AssignedToUserName = $"[AGENT]{toAgentType}";
            task.Metadata = JsonSerializer.Serialize(new { AgentType = toAgentType, DelegatedFrom = fromAgentType });

            _context.TaskDelegations.Add(delegation);
            _context.WorkflowTasks.Update(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} delegated from agent {FromAgent} to agent {ToAgent}", 
                taskId, fromAgentType, toAgentType);

            return new DelegationResultDto
            {
                Success = true,
                Message = $"Task delegated from {fromAgentType} to {toAgentType}",
                DelegationId = delegation.Id,
                TaskId = taskId,
                FromType = "Agent",
                FromAgentType = fromAgentType,
                ToType = "Agent",
                ToAgentTypes = new List<string> { toAgentType },
                DelegatedAt = delegation.DelegatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error delegating task {TaskId} between agents {FromAgent} and {ToAgent}", 
                taskId, fromAgentType, toAgentType);
            throw;
        }
    }

    /// <summary>
    /// Delegate a task to multiple agents (Multi-Agent)
    /// </summary>
    public async Task<DelegationResultDto> DelegateToMultipleAgentsAsync(
        Guid tenantId,
        Guid taskId,
        Guid fromUserId,
        List<string> agentTypes,
        string delegationStrategy = "Parallel",
        string? reason = null)
    {
        try
        {
            var task = await _context.WorkflowTasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.TenantId == tenantId);

            if (task == null)
                throw new InvalidOperationException($"Task {taskId} not found");

            var fromUser = await _userManager.FindByIdAsync(fromUserId.ToString());
            if (fromUser == null)
                throw new InvalidOperationException($"User {fromUserId} not found");

            // Validate agent types
            var validAgentTypes = new[] { "ComplianceAgent", "RiskAgent", "AuditAgent", "PolicyAgent", 
                "WorkflowAgent", "AnalyticsAgent", "IntegrationAgent", "SecurityAgent", "ReportingAgent" };
            
            foreach (var agentType in agentTypes)
            {
                if (!validAgentTypes.Contains(agentType))
                    throw new InvalidOperationException($"Invalid agent type: {agentType}");
            }

            // Determine which agent to assign based on strategy
            string? selectedAgent = null;
            if (delegationStrategy == "FirstAvailable")
            {
                selectedAgent = agentTypes.FirstOrDefault(); // For now, use first
            }
            else if (delegationStrategy == "Parallel")
            {
                // All agents work in parallel - assign to first, but track all
                selectedAgent = agentTypes.FirstOrDefault();
            }
            else // Sequential
            {
                selectedAgent = agentTypes.FirstOrDefault();
            }

            // Create delegation record
            var delegation = new TaskDelegation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TaskId = taskId,
                WorkflowInstanceId = task.WorkflowInstanceId,
                FromType = "Human",
                FromUserId = fromUserId,
                FromUserName = fromUser.FullName,
                ToType = "MultipleAgents",
                ToAgentTypesJson = JsonSerializer.Serialize(agentTypes),
                DelegationStrategy = delegationStrategy,
                SelectedAgentType = selectedAgent,
                Action = "Delegated",
                Reason = reason ?? $"Delegated to {agentTypes.Count} agents using {delegationStrategy} strategy",
                DelegatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Update task assignment
            task.AssignedToUserId = null;
            task.AssignedToUserName = $"[MULTI-AGENT]{string.Join(",", agentTypes)}";
            task.Metadata = JsonSerializer.Serialize(new 
            { 
                AgentTypes = agentTypes, 
                Strategy = delegationStrategy,
                SelectedAgent = selectedAgent,
                DelegatedFrom = fromUserId 
            });

            _context.TaskDelegations.Add(delegation);
            _context.WorkflowTasks.Update(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} delegated from {FromUser} to {Count} agents using {Strategy} strategy", 
                taskId, fromUser.FullName, agentTypes.Count, delegationStrategy);

            return new DelegationResultDto
            {
                Success = true,
                Message = $"Task delegated to {agentTypes.Count} agents using {delegationStrategy} strategy",
                DelegationId = delegation.Id,
                TaskId = taskId,
                FromType = "Human",
                FromUserId = fromUserId,
                ToType = "MultipleAgents",
                ToAgentTypes = agentTypes,
                DelegatedAt = delegation.DelegatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error delegating task {TaskId} to multiple agents", taskId);
            throw;
        }
    }

    /// <summary>
    /// Swap roles/tasks between two users (Human to Human swap)
    /// </summary>
    public async Task<SwapResultDto> SwapTasksAsync(
        Guid tenantId,
        Guid task1Id,
        Guid task2Id,
        Guid user1Id,
        Guid user2Id,
        string? reason = null)
    {
        try
        {
            var task1 = await _context.WorkflowTasks
                .FirstOrDefaultAsync(t => t.Id == task1Id && t.TenantId == tenantId);
            var task2 = await _context.WorkflowTasks
                .FirstOrDefaultAsync(t => t.Id == task2Id && t.TenantId == tenantId);

            if (task1 == null || task2 == null)
                throw new InvalidOperationException("One or both tasks not found");

            if (task1.AssignedToUserId != user1Id || task2.AssignedToUserId != user2Id)
                throw new InvalidOperationException("Tasks are not assigned to the specified users");

            var user1 = await _userManager.FindByIdAsync(user1Id.ToString());
            var user2 = await _userManager.FindByIdAsync(user2Id.ToString());

            if (user1 == null || user2 == null)
                throw new InvalidOperationException("User not found");

            // Create delegation records for both directions
            var delegation1 = new TaskDelegation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TaskId = task1Id,
                WorkflowInstanceId = task1.WorkflowInstanceId,
                FromType = "Human",
                FromUserId = user1Id,
                FromUserName = user1.FullName,
                ToType = "Human",
                ToUserId = user2Id,
                ToUserName = user2.FullName,
                Action = "Swapped",
                Reason = reason ?? $"Swapped with task {task2Id}",
                DelegatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var delegation2 = new TaskDelegation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TaskId = task2Id,
                WorkflowInstanceId = task2.WorkflowInstanceId,
                FromType = "Human",
                FromUserId = user2Id,
                FromUserName = user2.FullName,
                ToType = "Human",
                ToUserId = user1Id,
                ToUserName = user1.FullName,
                Action = "Swapped",
                Reason = reason ?? $"Swapped with task {task1Id}",
                DelegatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Swap assignments
            var tempUserId = task1.AssignedToUserId;
            var tempUserName = task1.AssignedToUserName;

            task1.AssignedToUserId = task2.AssignedToUserId;
            task1.AssignedToUserName = task2.AssignedToUserName;

            task2.AssignedToUserId = tempUserId;
            task2.AssignedToUserName = tempUserName;

            _context.TaskDelegations.AddRange(delegation1, delegation2);
            _context.WorkflowTasks.UpdateRange(task1, task2);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tasks {Task1Id} and {Task2Id} swapped between {User1} and {User2}", 
                task1Id, task2Id, user1.FullName, user2.FullName);

            return new SwapResultDto
            {
                Success = true,
                Message = $"Tasks swapped between {user1.FullName} and {user2.FullName}",
                Task1Id = task1Id,
                Task2Id = task2Id,
                Task1NewAssigneeId = task1.AssignedToUserId,
                Task2NewAssigneeId = task2.AssignedToUserId,
                SwappedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error swapping tasks {Task1Id} and {Task2Id}", task1Id, task2Id);
            throw;
        }
    }

    /// <summary>
    /// Swap role assignment between human and agent
    /// </summary>
    public async Task<SwapResultDto> SwapHumanAgentAsync(
        Guid tenantId,
        Guid taskId,
        Guid userId,
        string agentType,
        string? reason = null)
    {
        try
        {
            var task = await _context.WorkflowTasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.TenantId == tenantId);

            if (task == null)
                throw new InvalidOperationException($"Task {taskId} not found");

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new InvalidOperationException($"User {userId} not found");

            bool isCurrentlyHuman = task.AssignedToUserId.HasValue;
            bool isCurrentlyAgent = task.AssignedToUserName?.StartsWith("[AGENT]") == true;

            if (!isCurrentlyHuman && !isCurrentlyAgent)
                throw new InvalidOperationException("Task is not assigned to a human or agent");

            // Create delegation record
            var delegation = new TaskDelegation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TaskId = taskId,
                WorkflowInstanceId = task.WorkflowInstanceId,
                Action = "Swapped",
                Reason = reason,
                DelegatedAt = DateTime.UtcNow,
                IsActive = true
            };

            if (isCurrentlyHuman)
            {
                // Swap from Human to Agent
                delegation.FromType = "Human";
                delegation.FromUserId = task.AssignedToUserId;
                delegation.FromUserName = task.AssignedToUserName;
                delegation.ToType = "Agent";
                delegation.ToAgentType = agentType;

                task.AssignedToUserId = null;
                task.AssignedToUserName = $"[AGENT]{agentType}";
                task.Metadata = JsonSerializer.Serialize(new { AgentType = agentType, SwappedFrom = userId });
            }
            else
            {
                // Swap from Agent to Human
                var currentAgentType = task.AssignedToUserName?.Replace("[AGENT]", "") ?? "Unknown";
                delegation.FromType = "Agent";
                delegation.FromAgentType = currentAgentType;
                delegation.ToType = "Human";
                delegation.ToUserId = userId;
                delegation.ToUserName = user.FullName;

                task.AssignedToUserId = userId;
                task.AssignedToUserName = user.FullName;
                task.Metadata = null;
            }

            _context.TaskDelegations.Add(delegation);
            _context.WorkflowTasks.Update(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} swapped between {FromType} and {ToType}", 
                taskId, isCurrentlyHuman ? "Human" : "Agent", isCurrentlyHuman ? "Agent" : "Human");

            return new SwapResultDto
            {
                Success = true,
                Message = $"Task swapped from {(isCurrentlyHuman ? "Human" : "Agent")} to {(isCurrentlyHuman ? "Agent" : "Human")}",
                Task1Id = taskId,
                Task1NewAssigneeId = isCurrentlyHuman ? null : userId,
                Task1NewAgentType = isCurrentlyHuman ? agentType : null,
                SwappedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error swapping task {TaskId} between human and agent", taskId);
            throw;
        }
    }

    /// <summary>
    /// Get delegation history for a task
    /// </summary>
    public async Task<List<DelegationHistoryDto>> GetDelegationHistoryAsync(Guid tenantId, Guid taskId)
    {
        var delegations = await _context.TaskDelegations
            .Where(d => d.TenantId == tenantId && d.TaskId == taskId)
            .OrderByDescending(d => d.DelegatedAt)
            .ToListAsync();

        return delegations.Select(d => new DelegationHistoryDto
        {
            Id = d.Id,
            TaskId = d.TaskId,
            FromType = d.FromType,
            FromUserId = d.FromUserId,
            FromUserName = d.FromUserName,
            FromAgentType = d.FromAgentType,
            ToType = d.ToType,
            ToUserId = d.ToUserId,
            ToUserName = d.ToUserName,
            ToAgentType = d.ToAgentType,
            Action = d.Action,
            Reason = d.Reason,
            CreatedAt = d.DelegatedAt,
            ExpiresAt = d.ExpiresAt,
            IsActive = d.IsActive && !d.IsRevoked
        }).ToList();
    }

    /// <summary>
    /// Revoke a delegation
    /// </summary>
    public async Task<bool> RevokeDelegationAsync(Guid tenantId, Guid delegationId, Guid revokedByUserId)
    {
        try
        {
            var delegation = await _context.TaskDelegations
                .FirstOrDefaultAsync(d => d.Id == delegationId && d.TenantId == tenantId);

            if (delegation == null)
                return false;

            if (!delegation.IsActive || delegation.IsRevoked)
                return false;

            // Get original assignee
            var task = await _context.WorkflowTasks
                .FirstOrDefaultAsync(t => t.Id == delegation.TaskId && t.TenantId == tenantId);

            if (task != null)
            {
                // Revert to original assignee
                if (delegation.FromType == "Human" && delegation.FromUserId.HasValue)
                {
                    var originalUser = await _userManager.FindByIdAsync(delegation.FromUserId.Value.ToString());
                    if (originalUser != null)
                    {
                        task.AssignedToUserId = delegation.FromUserId;
                        task.AssignedToUserName = originalUser.FullName;
                        task.Metadata = null;
                    }
                }
                else if (delegation.FromType == "Agent" && !string.IsNullOrEmpty(delegation.FromAgentType))
                {
                    task.AssignedToUserId = null;
                    task.AssignedToUserName = $"[AGENT]{delegation.FromAgentType}";
                    task.Metadata = JsonSerializer.Serialize(new { AgentType = delegation.FromAgentType });
                }

                _context.WorkflowTasks.Update(task);
            }

            delegation.IsRevoked = true;
            delegation.RevokedAt = DateTime.UtcNow;
            delegation.RevokedByUserId = revokedByUserId;
            delegation.IsActive = false;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Delegation {DelegationId} revoked by user {UserId}", delegationId, revokedByUserId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking delegation {DelegationId}", delegationId);
            return false;
        }
    }
}
