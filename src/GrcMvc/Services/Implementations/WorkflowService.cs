using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrcMvc.Data;
using GrcMvc.Exceptions;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Policy;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Strongly-typed workflow step model for safe deserialization
    /// </summary>
    internal class WorkflowStep
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public int Order { get; set; }
    }

    /// <summary>
    /// Strongly-typed execution history entry
    /// </summary>
    internal class ExecutionHistoryEntry
    {
        public string? Step { get; set; }
        public string? Result { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class WorkflowService : IWorkflowService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<WorkflowService> _logger;
        private readonly PolicyEnforcementHelper _policyHelper;
        private readonly ITenantContextService _tenantContext;
        
        // SECURITY: Safe JSON settings - disable TypeNameHandling
        private static readonly JsonSerializerSettings SafeJsonSettings = new()
        {
            TypeNameHandling = TypeNameHandling.None,
            MaxDepth = 32
        };

        public WorkflowService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<WorkflowService> logger,
            PolicyEnforcementHelper policyHelper,
            ITenantContextService tenantContext)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _policyHelper = policyHelper ?? throw new ArgumentNullException(nameof(policyHelper));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));

            // Validate tenant context
            if (!_tenantContext.HasTenantContext())
            {
                throw new InvalidOperationException("Tenant context is required for WorkflowService operations");
            }
        }

        public async Task<IEnumerable<WorkflowDto>> GetAllAsync()
        {
            try
            {
                var workflows = await _unitOfWork.Workflows.GetAllAsync();
                return _mapper.Map<IEnumerable<WorkflowDto>>(workflows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all workflows");
                throw;
            }
        }

        public async Task<WorkflowDto?> GetByIdAsync(Guid id)
        {
            try
            {
                var workflow = await _unitOfWork.Workflows.GetByIdAsync(id);
                if (workflow == null)
                {
                    _logger.LogWarning("Workflow with ID {WorkflowId} not found", id);
                    return null;
                }
                return workflow == null ? null : _mapper.Map<WorkflowDto>(workflow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow with ID {WorkflowId}", id);
                throw;
            }
        }

        public async Task<WorkflowDto> CreateAsync(CreateWorkflowDto createWorkflowDto)
        {
            try
            {
                var workflow = _mapper.Map<Workflow>(createWorkflowDto);
                workflow.Id = Guid.NewGuid();
                workflow.CreatedDate = DateTime.UtcNow;
                workflow.IsActive = true;
                workflow.Version = 1;

                // Validate workflow steps
                if (!string.IsNullOrEmpty(createWorkflowDto.Steps))
                {
                    workflow.Steps = ValidateAndFormatSteps(createWorkflowDto.Steps);
                }

                // Enforce policy before creating workflow
                await _policyHelper.EnforceCreateAsync(
                    resourceType: "Workflow",
                    resource: workflow,
                    dataClassification: "internal",
                    owner: "System");

                await _unitOfWork.Workflows.AddAsync(workflow);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Workflow created with ID {WorkflowId}", workflow.Id);
                return _mapper.Map<WorkflowDto>(workflow);
            }
            catch (PolicyViolationException pve)
            {
                _logger.LogWarning("Policy violation prevented workflow creation: {Message}. Rule: {RuleId}",
                    pve.Message, pve.RuleId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workflow");
                throw;
            }
        }

        public async Task<WorkflowDto?> UpdateAsync(Guid id, UpdateWorkflowDto updateWorkflowDto)
        {
            try
            {
                var workflow = await _unitOfWork.Workflows.GetByIdAsync(id);
                if (workflow == null)
                {
                    _logger.LogWarning("Workflow with ID {WorkflowId} not found for update", id);
                    return null;
                }

                // Check if steps changed to increment version
                bool stepsChanged = workflow.Steps != updateWorkflowDto.Steps;

                _mapper.Map(updateWorkflowDto, workflow);
                workflow.ModifiedDate = DateTime.UtcNow;

                if (stepsChanged)
                {
                    workflow.Version++;
                    workflow.Steps = ValidateAndFormatSteps(updateWorkflowDto.Steps);
                    _logger.LogInformation("Workflow {WorkflowId} version updated to {Version}",
                        id, workflow.Version);
                }

                await _unitOfWork.Workflows.UpdateAsync(workflow);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Workflow with ID {WorkflowId} updated", id);
                return workflow == null ? null : _mapper.Map<WorkflowDto>(workflow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workflow with ID {WorkflowId}", id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var workflow = await _unitOfWork.Workflows.GetByIdAsync(id);
                if (workflow == null)
                {
                    _logger.LogWarning("Workflow with ID {WorkflowId} not found for deletion", id);
                    return;
                }

                // Check if workflow has active executions
                var activeExecutions = await _unitOfWork.WorkflowExecutions.FindAsync(
                    e => e.WorkflowId == id && e.Status == "In Progress");

                if (activeExecutions.Any())
                {
                    throw new InvalidOperationException(
                        "Cannot delete workflow with active executions");
                }

                await _unitOfWork.Workflows.DeleteAsync(workflow);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Workflow with ID {WorkflowId} deleted", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workflow with ID {WorkflowId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<WorkflowDto>> GetByEntityTypeAsync(string entityType)
        {
            try
            {
                var workflows = await _unitOfWork.Workflows.FindAsync(
                    w => w.EntityType == entityType && w.IsActive);
                return _mapper.Map<IEnumerable<WorkflowDto>>(workflows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflows for entity type {EntityType}", entityType);
                throw;
            }
        }

        public async Task<WorkflowExecutionDto> StartWorkflowAsync(Guid workflowId, string triggeredBy)
        {
            try
            {
                var workflow = await _unitOfWork.Workflows.GetByIdAsync(workflowId);
                if (workflow == null)
                {
                    _logger.LogWarning("Workflow with ID {WorkflowId} not found", workflowId);
                    return null;
                }

                if (!workflow.IsActive)
                {
                    throw new GrcException("Cannot start inactive workflow", GrcErrorCodes.InvalidState);
                }

                var execution = new WorkflowExecution
                {
                    Id = Guid.NewGuid(),
                    WorkflowId = workflowId,
                    StartTime = DateTime.UtcNow,
                    Status = "In Progress",
                    CurrentStep = GetFirstStep(workflow.Steps),
                    TriggeredBy = triggeredBy,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.WorkflowExecutions.AddAsync(execution);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Workflow execution {ExecutionId} started for workflow {WorkflowId}",
                    execution.Id, workflowId);

                return _mapper.Map<WorkflowExecutionDto>(execution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting workflow {WorkflowId}", workflowId);
                throw;
            }
        }

        public async Task<WorkflowExecutionDto> CompleteStepAsync(Guid executionId, string stepName, string result)
        {
            try
            {
                var execution = await _unitOfWork.WorkflowExecutions.GetByIdAsync(executionId);
                if (execution == null)
                {
                    _logger.LogWarning("Workflow execution with ID {ExecutionId} not found", executionId);
                    return null;
                }

                if (execution.Status != "In Progress")
                {
                    throw new GrcException("Cannot complete step for non-active execution", GrcErrorCodes.InvalidState);
                }

                var workflow = await _unitOfWork.Workflows.GetByIdAsync(execution.WorkflowId);
                var nextStep = GetNextStep(workflow.Steps, stepName);

                // Update execution history - SECURITY: Use strongly-typed deserialization
                var history = string.IsNullOrEmpty(execution.ExecutionHistory)
                    ? new List<ExecutionHistoryEntry>()
                    : JsonConvert.DeserializeObject<List<ExecutionHistoryEntry>>(execution.ExecutionHistory, SafeJsonSettings) 
                      ?? new List<ExecutionHistoryEntry>();

                history.Add(new ExecutionHistoryEntry
                {
                    Step = stepName,
                    Result = result,
                    CompletedAt = DateTime.UtcNow
                });

                execution.ExecutionHistory = JsonConvert.SerializeObject(history, SafeJsonSettings);
                execution.CurrentStep = nextStep;
                execution.ModifiedDate = DateTime.UtcNow;

                if (string.IsNullOrEmpty(nextStep))
                {
                    execution.Status = "Completed";
                    execution.EndTime = DateTime.UtcNow;
                    execution.Result = result;
                }

                await _unitOfWork.WorkflowExecutions.UpdateAsync(execution);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Step {StepName} completed for execution {ExecutionId}",
                    stepName, executionId);

                return _mapper.Map<WorkflowExecutionDto>(execution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing step for execution {ExecutionId}", executionId);
                throw;
            }
        }

        public async Task<IEnumerable<WorkflowExecutionDto>> GetExecutionsByWorkflowIdAsync(Guid workflowId)
        {
            try
            {
                var executions = await _unitOfWork.WorkflowExecutions.FindAsync(
                    e => e.WorkflowId == workflowId);
                return _mapper.Map<IEnumerable<WorkflowExecutionDto>>(
                    executions.OrderByDescending(e => e.StartTime));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting executions for workflow {WorkflowId}", workflowId);
                throw;
            }
        }

        public async Task<WorkflowStatisticsDto> GetStatisticsAsync()
        {
            try
            {
                var workflows = await _unitOfWork.Workflows.GetAllAsync();
                var workflowsList = workflows.ToList();
                var executions = await _unitOfWork.WorkflowExecutions.GetAllAsync();
                var executionsList = executions.ToList();

                return new WorkflowStatisticsDto
                {
                    TotalWorkflows = workflowsList.Count,
                    ActiveWorkflows = workflowsList.Count(w => w.IsActive),
                    TotalExecutions = executionsList.Count,
                    InProgressExecutions = executionsList.Count(e => e.Status == "In Progress"),
                    CompletedExecutions = executionsList.Count(e => e.Status == "Completed"),
                    FailedExecutions = executionsList.Count(e => e.Status == "Failed"),
                    AverageExecutionTime = CalculateAverageExecutionTime(executionsList),
                    WorkflowsByType = workflowsList.GroupBy(w => w.EntityType)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    SuccessRate = CalculateSuccessRate(executionsList)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow statistics");
                throw;
            }
        }

        public async Task<bool> ValidateWorkflowAsync(Guid workflowId)
        {
            try
            {
                var workflow = await _unitOfWork.Workflows.GetByIdAsync(workflowId);
                if (workflow == null)
                {
                    _logger.LogWarning("Workflow with ID {WorkflowId} not found for validation", workflowId);
                    return false;
                }

                // Validate workflow has steps
                if (string.IsNullOrEmpty(workflow.Steps))
                {
                    _logger.LogWarning("Workflow {WorkflowId} has no steps defined", workflowId);
                    return false;
                }

                // Try to parse steps as JSON - SECURITY: Use strongly-typed deserialization
                try
                {
                    var steps = JsonConvert.DeserializeObject<List<WorkflowStep>>(workflow.Steps, SafeJsonSettings);
                    if (steps == null || !steps.Any())
                    {
                        _logger.LogWarning("Workflow {WorkflowId} has invalid steps", workflowId);
                        return false;
                    }
                }
                catch
                {
                    _logger.LogWarning("Workflow {WorkflowId} has malformed steps JSON", workflowId);
                    return false;
                }

                return workflow.IsActive;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating workflow {WorkflowId}", workflowId);
                throw;
            }
        }

        private string ValidateAndFormatSteps(string steps)
        {
            if (string.IsNullOrEmpty(steps))
                return "[]";

            try
            {
                // SECURITY: Use strongly-typed deserialization
                var parsed = JsonConvert.DeserializeObject<List<WorkflowStep>>(steps, SafeJsonSettings);
                return JsonConvert.SerializeObject(parsed, SafeJsonSettings);
            }
            catch
            {
                // If not valid JSON, create a simple step list
                var stepsList = steps.Split(',').Select(s => new WorkflowStep { Name = s.Trim() });
                return JsonConvert.SerializeObject(stepsList, SafeJsonSettings);
            }
        }

        private string GetFirstStep(string steps)
        {
            if (string.IsNullOrEmpty(steps))
                return null;

            try
            {
                // SECURITY: Use strongly-typed deserialization
                var stepsList = JsonConvert.DeserializeObject<List<WorkflowStep>>(steps, SafeJsonSettings);
                return stepsList?.FirstOrDefault()?.Name;
            }
            catch
            {
                return null;
            }
        }

        private string GetNextStep(string steps, string currentStep)
        {
            if (string.IsNullOrEmpty(steps))
                return null;

            try
            {
                // SECURITY: Use strongly-typed deserialization
                var stepsList = JsonConvert.DeserializeObject<List<WorkflowStep>>(steps, SafeJsonSettings);
                if (stepsList == null) return null;
                
                var currentIndex = -1;

                for (int i = 0; i < stepsList.Count; i++)
                {
                    if (stepsList[i]?.Name == currentStep)
                    {
                        currentIndex = i;
                        break;
                    }
                }

                if (currentIndex >= 0 && currentIndex < stepsList.Count - 1)
                {
                    return stepsList[currentIndex + 1]?.Name;
                }
            }
            catch
            {
                _logger.LogWarning("Error parsing workflow steps");
            }

            return null;
        }

        private double CalculateAverageExecutionTime(List<WorkflowExecution> executions)
        {
            var completedExecutions = executions
                .Where(e => e.Status == "Completed" && e.EndTime.HasValue)
                .ToList();

            if (!completedExecutions.Any())
                return 0;

            var totalMinutes = completedExecutions
                .Sum(e => (e.EndTime.Value - e.StartTime).TotalMinutes);

            return totalMinutes / completedExecutions.Count;
        }

        public async Task<IEnumerable<WorkflowDto>> GetByCategoryAsync(string category)
        {
            try
            {
                var workflows = await _unitOfWork.Workflows.FindAsync(w => w.Category.Equals(category));
                return _mapper.Map<IEnumerable<WorkflowDto>>(workflows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflows by category {Category}", category);
                throw;
            }
        }

        public async Task<IEnumerable<WorkflowDto>> GetByStatusAsync(string status)
        {
            try
            {
                var workflows = await _unitOfWork.Workflows.FindAsync(w => w.Status.Equals(status));
                return _mapper.Map<IEnumerable<WorkflowDto>>(workflows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflows by status {Status}", status);
                throw;
            }
        }

        public async Task<IEnumerable<WorkflowDto>> GetOverdueWorkflowsAsync()
        {
            try
            {
                // Since Workflow doesn't have DueDate, return active workflows that need attention
                var workflows = await _unitOfWork.Workflows.FindAsync(w => w.Status == "Active" && !w.IsActive);
                return _mapper.Map<IEnumerable<WorkflowDto>>(workflows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overdue workflows");
                throw;
            }
        }

        public async Task<IEnumerable<WorkflowExecutionDto>> GetWorkflowExecutionsAsync(Guid id)
        {
            try
            {
                var executions = await _unitOfWork.WorkflowExecutions.FindAsync(e => e.WorkflowId == id);
                return _mapper.Map<IEnumerable<WorkflowExecutionDto>>(executions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting executions for workflow ID {WorkflowId}", id);
                throw;
            }
        }

        public async Task<WorkflowExecutionDto?> ExecuteWorkflowAsync(Guid id)
        {
            try
            {
                var workflow = await _unitOfWork.Workflows.GetByIdAsync(id);
                if (workflow == null)
                {
                    _logger.LogWarning("Workflow with ID {WorkflowId} not found", id);
                    return null;
                }

                var execution = new WorkflowExecution
                {
                    Id = Guid.NewGuid(),
                    WorkflowId = id,
                    ExecutionNumber = GenerateExecutionNumber(),
                    Status = "Started",
                    StartTime = DateTime.UtcNow,
                    InitiatedBy = "System"
                };

                await _unitOfWork.WorkflowExecutions.AddAsync(execution);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<WorkflowExecutionDto>(execution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing workflow with ID {WorkflowId}", id);
                throw;
            }
        }

        private string GenerateExecutionNumber()
        {
            return $"EXEC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
        }

        private double CalculateSuccessRate(List<WorkflowExecution> executions)
        {
            var finishedExecutions = executions
                .Where(e => e.Status == "Completed" || e.Status == "Failed")
                .ToList();

            if (!finishedExecutions.Any())
                return 100.0;

            var successfulExecutions = finishedExecutions.Count(e => e.Status == "Completed");
            return (double)successfulExecutions / finishedExecutions.Count * 100;
        }
    }
}