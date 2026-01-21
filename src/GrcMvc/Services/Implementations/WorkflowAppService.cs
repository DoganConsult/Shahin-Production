using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Exceptions;
using GrcMvc.Models.Entities;
using GrcMvc.Models.DTOs;
using GrcMvc.Services.Interfaces;
using GrcMvc.Data;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Application Service for Workflow operations
    /// Provides high-level workflow management API
    /// </summary>
    public class WorkflowAppService
    {
        private readonly IWorkflowEngineService _workflowEngine;
        private readonly GrcDbContext _context;
        private readonly ILogger<WorkflowAppService> _logger;

        public WorkflowAppService(
            IWorkflowEngineService workflowEngine,
            GrcDbContext context,
            ILogger<WorkflowAppService> logger)
        {
            _workflowEngine = workflowEngine;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Start a workflow instance from a definition
        /// Implements the specification: StartWorkflowAsync endpoint
        /// </summary>
        public async Task<WorkflowInstanceDto> StartWorkflowAsync(
            Guid tenantId,
            Guid userId,
            StartWorkflowDto input)
        {
            if (tenantId == Guid.Empty)
                throw new TenantRequiredException();
            if (userId == Guid.Empty)
                throw new ValidationException("UserId", "User ID is required");

            // Validate workflow definition exists
            var definition = await _context.WorkflowDefinitions
                .FirstOrDefaultAsync(d => d.Id == input.WorkflowDefinitionId && (d.TenantId == tenantId || d.TenantId == null));
            
            if (definition == null)
                throw new EntityNotFoundException("WorkflowDefinition", input.WorkflowDefinitionId);

            if (definition.Status != "Active" && definition.IsActive != true)
                throw new GrcException($"Workflow definition {input.WorkflowDefinitionId} is not active", GrcErrorCodes.InvalidState);

            // Start workflow instance
            var instance = await _workflowEngine.StartWorkflowAsync(
                tenantId,
                input.WorkflowDefinitionId,
                userId,
                input.InputVariables);

            // Map to DTO
            return MapToDto(instance);
        }

        /// <summary>
        /// Get workflow instance details
        /// </summary>
        public async Task<WorkflowInstanceDto> GetInstanceAsync(Guid tenantId, Guid instanceId)
        {
            if (tenantId == Guid.Empty)
                throw new TenantRequiredException();
            
            var instance = await _workflowEngine.GetWorkflowAsync(tenantId, instanceId);
            
            if (instance == null)
                throw new EntityNotFoundException("WorkflowInstance", instanceId);

            return MapToDto(instance);
        }

        /// <summary>
        /// Complete a workflow task
        /// </summary>
        public async Task<bool> CompleteTaskAsync(Guid tenantId, Guid userId, CompleteTaskDto input)
        {
            if (tenantId == Guid.Empty)
                throw new TenantRequiredException();
            if (userId == Guid.Empty)
                throw new ValidationException("UserId", "User ID is required");

            return await _workflowEngine.CompleteTaskAsync(
                tenantId,
                input.TaskId,
                userId,
                input.OutputData,
                input.Notes);
        }

        /// <summary>
        /// Get user's pending tasks
        /// </summary>
        public async Task<List<WorkflowTaskDto>> GetUserTasksAsync(Guid tenantId, Guid userId)
        {
            if (tenantId == Guid.Empty)
                throw new TenantRequiredException();
            if (userId == Guid.Empty)
                throw new ValidationException("UserId", "User ID is required");

            var workflows = await _workflowEngine.GetUserWorkflowsAsync(tenantId);
            var allTasks = new List<WorkflowTaskDto>();

            foreach (var workflow in workflows)
            {
                var tasks = await _workflowEngine.GetWorkflowTasksAsync(tenantId, workflow.Id);
                var userTasks = tasks
                    .Where(t => t.AssignedToUserId == userId && (t.Status == "Pending" || t.Status == "InProgress"))
                    .Select(MapTaskToDto)
                    .ToList();

                allTasks.AddRange(userTasks);
            }

            return allTasks;
        }

        private WorkflowInstanceDto MapToDto(WorkflowInstance instance)
        {
            return new WorkflowInstanceDto
            {
                Id = instance.Id,
                WorkflowDefinitionId = instance.WorkflowDefinitionId,
                WorkflowType = instance.WorkflowType,
                EntityType = instance.EntityType,
                EntityId = instance.EntityId,
                CurrentState = instance.CurrentState,
                Status = instance.Status,
                CreatedAt = instance.StartedAt,
                CompletedAt = instance.CompletedAt,
                Tasks = instance.Tasks?.Select(MapTaskToDto).ToList() ?? new List<WorkflowTaskDto>()
            };
        }

        private WorkflowTaskDto MapTaskToDto(WorkflowTask task)
        {
            return new WorkflowTaskDto
            {
                Id = task.Id,
                TaskName = task.TaskName,
                Description = task.Description,
                AssignedToUserId = task.AssignedToUserId,
                AssignedToUserName = task.AssignedToUserName,
                Status = task.Status,
                DueDate = task.DueDate,
                Priority = task.Priority,
                StartedAt = task.StartedAt,
                CompletedAt = task.CompletedAt
            };
        }
    }
}
