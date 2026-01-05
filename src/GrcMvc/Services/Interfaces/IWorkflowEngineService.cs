using GrcMvc.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Interface for Workflow Engine Service
    /// Provides workflow orchestration, execution, and state management
    /// </summary>
    public interface IWorkflowEngineService
    {
        // Workflow Creation & Initialization
        Task<WorkflowInstance> CreateWorkflowAsync(Guid tenantId, Guid definitionId, string priority = "Medium", string createdBy = "System");
        
        /// <summary>
        /// Start a workflow instance from a definition with BPMN parsing and task creation
        /// </summary>
        Task<WorkflowInstance> StartWorkflowAsync(
            Guid tenantId,
            Guid definitionId,
            Guid? initiatedByUserId,
            Dictionary<string, object>? inputVariables = null);
        
        // Workflow Execution
        Task<WorkflowInstance> GetWorkflowAsync(Guid tenantId, Guid workflowId);
        Task<List<WorkflowInstance>> GetUserWorkflowsAsync(Guid tenantId, int page = 1, int pageSize = 20);
        
        // Workflow State Transitions
        Task<bool> ApproveWorkflowAsync(Guid tenantId, Guid workflowId, string reason = "", string approvedBy = "");
        Task<bool> RejectWorkflowAsync(Guid tenantId, Guid workflowId, string reason = "", string rejectedBy = "");
        Task<bool> CompleteWorkflowAsync(Guid tenantId, Guid workflowId);
        
        // Task Management
        Task<WorkflowTask> GetTaskAsync(Guid tenantId, Guid taskId);
        Task<List<WorkflowTask>> GetWorkflowTasksAsync(Guid tenantId, Guid workflowId);
        Task<bool> CompleteTaskAsync(Guid tenantId, Guid taskId, string notes = "");
        
        /// <summary>
        /// Complete a task and evaluate workflow completion
        /// </summary>
        Task<bool> CompleteTaskAsync(
            Guid tenantId,
            Guid taskId,
            Guid userId,
            Dictionary<string, object>? outputData = null,
            string? notes = null);
        
        // Statistics
        Task<WorkflowStats> GetStatisticsAsync(Guid tenantId);
    }

    /// <summary>
    /// Statistics DTO for workflow dashboard
    /// </summary>
    public class WorkflowStats
    {
        public int TotalWorkflows { get; set; }
        public int ActiveWorkflows { get; set; }
        public int PendingWorkflows { get; set; }
        public int CompletedWorkflows { get; set; }
        public int RejectedWorkflows { get; set; }
        public double AverageCompletionTimeHours { get; set; }
    }
}
