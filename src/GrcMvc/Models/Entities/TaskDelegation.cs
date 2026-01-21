using System;

namespace GrcMvc.Models.Entities;

/// <summary>
/// Records task delegation between humans and agents
/// Supports: Human↔Human, Human↔Agent, Agent↔Agent, Multi-Agent
/// </summary>
public class TaskDelegation : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid TaskId { get; set; }
    public Guid WorkflowInstanceId { get; set; }

    // From (Original Assignee)
    public string FromType { get; set; } = "Human"; // Human, Agent
    public Guid? FromUserId { get; set; }
    public string? FromUserName { get; set; }
    public string? FromAgentType { get; set; } // ComplianceAgent, RiskAgent, etc.

    // To (New Assignee)
    public string ToType { get; set; } = "Human"; // Human, Agent, MultipleAgents
    public Guid? ToUserId { get; set; }
    public string? ToUserName { get; set; }
    public string? ToAgentType { get; set; } // For single agent
    public string? ToAgentTypesJson { get; set; } // For multiple agents: ["Agent1", "Agent2"]

    // Delegation details
    public string Action { get; set; } = "Delegated"; // Delegated, Swapped, Revoked, Completed
    public string? Reason { get; set; }
    public DateTime DelegatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }
    public Guid? RevokedByUserId { get; set; }

    // Multi-agent delegation strategy
    public string DelegationStrategy { get; set; } = "Parallel"; // Parallel, Sequential, FirstAvailable
    public string? SelectedAgentType { get; set; } // Which agent was selected (for FirstAvailable)

    // Navigation
    public virtual WorkflowTask Task { get; set; } = null!;
    public virtual WorkflowInstance WorkflowInstance { get; set; } = null!;
}
