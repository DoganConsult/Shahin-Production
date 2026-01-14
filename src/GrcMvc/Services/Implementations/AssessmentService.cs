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
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    public class AssessmentService : IAssessmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AssessmentService> _logger;
        private readonly PolicyEnforcementHelper _policyHelper;
        private readonly IWorkspaceContextService? _workspaceContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AssessmentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<AssessmentService> logger,
            PolicyEnforcementHelper policyHelper,
            IHttpContextAccessor httpContextAccessor,
            IWorkspaceContextService? workspaceContext = null)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _policyHelper = policyHelper ?? throw new ArgumentNullException(nameof(policyHelper));
            _httpContextAccessor = httpContextAccessor;
            _workspaceContext = workspaceContext;
        }

        public async Task<IEnumerable<AssessmentDto>> GetAllAsync()
        {
            try
            {
                var assessments = await _unitOfWork.Assessments.GetAllAsync();
                return _mapper.Map<IEnumerable<AssessmentDto>>(assessments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all assessments");
                throw;
            }
        }

        public async Task<AssessmentDto?> GetByIdAsync(Guid id)
        {
            try
            {
                var assessment = await _unitOfWork.Assessments.GetByIdAsync(id);
                if (assessment == null)
                {
                    _logger.LogWarning("Assessment with ID {AssessmentId} not found", id);
                    return null;
                }
                return _mapper.Map<AssessmentDto>(assessment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assessment with ID {AssessmentId}", id);
                throw;
            }
        }

        public async Task<AssessmentDto> CreateAsync(CreateAssessmentDto createAssessmentDto)
        {
            try
            {
                var assessment = _mapper.Map<Assessment>(createAssessmentDto);
                assessment.Id = Guid.NewGuid();
                assessment.ModifiedDate = DateTime.UtcNow;
                assessment.AssessmentCode = GenerateAssessmentCode();

                // Set workspace context if available
                if (_workspaceContext != null && _workspaceContext.HasWorkspaceContext())
                {
                    assessment.WorkspaceId = _workspaceContext.GetCurrentWorkspaceId();
                }

                // Enforce policies before saving
                await _policyHelper.EnforceCreateAsync(
                    resourceType: "Assessment",
                    resource: assessment,
                    dataClassification: null, // Will be set to "internal" by helper if null
                    owner: null); // Will be set to current user by helper if null

                await _unitOfWork.Assessments.AddAsync(assessment);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<AssessmentDto>(assessment);
            }
            catch (PolicyViolationException pve)
            {
                _logger.LogWarning("Policy violation prevented assessment creation: {Message}. Rule: {RuleId}",
                    pve.Message, pve.RuleId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating assessment");
                throw;
            }
        }

        public async Task<AssessmentDto?> UpdateAsync(Guid id, UpdateAssessmentDto updateAssessmentDto)
        {
            try
            {
                var assessment = await _unitOfWork.Assessments.GetByIdAsync(id);
                if (assessment == null)
                {
                    _logger.LogWarning("Assessment with ID {AssessmentId} not found for update", id);
                    return null;
                }

                _mapper.Map(updateAssessmentDto, assessment);
                assessment.ModifiedDate = DateTime.UtcNow;

                // Calculate score based on results
                if (!string.IsNullOrEmpty(updateAssessmentDto.Results))
                {
                    assessment.Score = CalculateScore(updateAssessmentDto.Results);
                }

                await _unitOfWork.Assessments.UpdateAsync(assessment);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<AssessmentDto>(assessment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating assessment with ID {AssessmentId}", id);
                throw;
            }
        }


        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var assessment = await _unitOfWork.Assessments.GetByIdAsync(id);
                if (assessment == null)
                {
                    _logger.LogWarning("Assessment with ID {AssessmentId} not found for deletion", id);
                    return;
                }

                await _unitOfWork.Assessments.DeleteAsync(assessment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Assessment with ID {AssessmentId} deleted", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting assessment with ID {AssessmentId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<AssessmentDto>> GetByControlIdAsync(Guid controlId)
        {
            try
            {
                var assessments = await _unitOfWork.Assessments.FindAsync(a => a.ControlId == controlId);
                return _mapper.Map<IEnumerable<AssessmentDto>>(assessments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assessments for control ID {ControlId}", controlId);
                throw;
            }
        }

        public async Task<IEnumerable<AssessmentDto>> GetUpcomingAssessmentsAsync(int days = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(days);
                var assessments = await _unitOfWork.Assessments.FindAsync(
                    a => a.ScheduledDate <= cutoffDate && a.Status != "Completed");

                return _mapper.Map<IEnumerable<AssessmentDto>>(
                    assessments.OrderBy(a => a.ScheduledDate));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming assessments");
                throw;
            }
        }

        public async Task<AssessmentStatisticsDto> GetStatisticsAsync()
        {
            try
            {
                // Use database-level aggregation instead of loading all records into memory
                var query = _unitOfWork.Assessments.Query();
                var now = DateTime.UtcNow;

                // Execute all counts in parallel for better performance
                var totalTask = query.CountAsync();
                var completedTask = query.CountAsync(a => a.Status == "Completed");
                var pendingTask = query.CountAsync(a => a.Status == "Scheduled" || a.Status == "InProgress" || a.Status == "In Progress" || a.Status == "Draft");
                var overdueTask = query.CountAsync(a => a.ScheduledDate < now && a.Status != "Completed");
                var avgScoreTask = query.Where(a => a.Score > 0).AverageAsync(a => (double?)a.Score);
                var byTypeTask = query.GroupBy(a => a.Type)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(g => g.Type, g => g.Count);

                await Task.WhenAll(totalTask, completedTask, pendingTask, overdueTask);
                var avgScore = await avgScoreTask;
                var byType = await byTypeTask;

                return new AssessmentStatisticsDto
                {
                    TotalAssessments = await totalTask,
                    CompletedAssessments = await completedTask,
                    PendingAssessments = await pendingTask,
                    OverdueAssessments = await overdueTask,
                    AverageScore = avgScore ?? 0,
                    AssessmentsByType = byType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assessment statistics");
                throw;
            }
        }

        private string GenerateAssessmentCode()
        {
            return $"ASMT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
        }

        private int CalculateScore(string results)
        {
            // Simple scoring logic - can be enhanced based on actual business rules
            if (string.IsNullOrEmpty(results))
                return 0;

            var resultsLower = results.ToLower();
            if (resultsLower.Contains("excellent") || resultsLower.Contains("outstanding"))
                return 95;
            if (resultsLower.Contains("good") || resultsLower.Contains("satisfactory"))
                return 75;
            if (resultsLower.Contains("needs improvement") || resultsLower.Contains("fair"))
                return 50;
            if (resultsLower.Contains("poor") || resultsLower.Contains("fail"))
                return 25;

            return 60; // Default score
        }

        /// <summary>
        /// Generate assessments from a Plan based on derived templates.
        /// Creates Assessment instances with AssessmentRequirements linked to framework controls.
        /// </summary>
        public async Task<List<Assessment>> GenerateAssessmentsFromPlanAsync(Guid planId, string createdBy)
        {
            try
            {
                var plan = await _unitOfWork.Plans.GetByIdAsync(planId);
                if (plan == null)
                    throw new EntityNotFoundException("Plan", planId);

                // Get tenant's derived templates
                var tenantTemplates = await _unitOfWork.TenantTemplates
                    .Query()
                    .Where(t => t.TenantId == plan.TenantId && !t.IsDeleted)
                    .ToListAsync();

                // Get template catalog definitions
                var templateCodes = tenantTemplates.Select(t => t.TemplateCode).ToList();
                var templateCatalogs = await _unitOfWork.TemplateCatalogs
                    .Query()
                    .Where(t => templateCodes.Contains(t.TemplateCode))
                    .ToListAsync();

                var assessments = new List<Assessment>();

                foreach (var template in templateCatalogs)
                {
                    // Create assessment from template
                    var assessment = new Assessment
                    {
                        Id = Guid.NewGuid(),
                        TenantId = plan.TenantId,
                        PlanId = planId,
                        AssessmentCode = $"ASMT-{template.TemplateCode}-{DateTime.UtcNow:yyyyMMdd}",
                        Name = $"{template.TemplateName} Assessment",
                        Type = template.TemplateType ?? "Compliance",
                        Status = "Draft",
                        ScheduledDate = plan.StartDate,
                        DueDate = plan.TargetEndDate,
                        TemplateCode = template.TemplateCode,
                        FrameworkCode = template.TemplateCode.Split('_').FirstOrDefault() ?? "",
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = createdBy
                    };

                    await _unitOfWork.Assessments.AddAsync(assessment);
                    assessments.Add(assessment);

                    // Generate assessment requirements from framework controls based on template code
                    await GenerateAssessmentRequirementsAsync(assessment, assessment.FrameworkCode, createdBy);

                    _logger.LogInformation($"Assessment created: {assessment.AssessmentCode} from template {template.TemplateCode}");
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Generated {assessments.Count} assessments from plan {planId}");
                return assessments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating assessments from plan {PlanId}", planId);
                throw;
            }
        }

        /// <summary>
        /// Generate assessment requirements from framework controls.
        /// </summary>
        private async Task GenerateAssessmentRequirementsAsync(Assessment assessment, string frameworkCode, string createdBy)
        {
            // Get all controls for this framework from FrameworkControls table
            // No artificial limit - frameworks may have varying control counts
            var controls = await _unitOfWork.FrameworkControls
                .Query()
                .Where(c => c.FrameworkCode == frameworkCode)
                .OrderBy(c => c.ControlNumber)
                .ToListAsync();

            _logger.LogInformation("Generating {Count} requirements for framework {Framework}",
                controls.Count, frameworkCode);

            foreach (var control in controls)
            {
                var requirement = new AssessmentRequirement
                {
                    Id = Guid.NewGuid(),
                    AssessmentId = assessment.Id,
                    TenantId = assessment.TenantId,
                    ControlNumber = control.ControlNumber,
                    ControlTitle = control.TitleEn,
                    ControlTitleAr = control.TitleAr,
                    RequirementText = control.RequirementEn,
                    RequirementTextAr = control.RequirementAr,
                    Domain = control.Domain,
                    ControlType = control.ControlType ?? "",
                    MaturityLevel = control.MaturityLevel.ToString(),
                    Status = "NotStarted",
                    EvidenceStatus = "Pending",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = createdBy
                };

                await _unitOfWork.AssessmentRequirements.AddAsync(requirement);
            }

            _logger.LogInformation($"Generated {controls.Count} requirements for assessment {assessment.AssessmentCode}");
        }

        /// <summary>
        /// Get assessments for a specific plan.
        /// </summary>
        public async Task<IEnumerable<Assessment>> GetAssessmentsByPlanAsync(Guid planId)
        {
            return await _unitOfWork.Assessments
                .Query()
                .Where(a => a.PlanId == planId && !a.IsDeleted)
                .OrderBy(a => a.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Submit an assessment for review/approval
        /// </summary>
        public async Task<AssessmentDto> SubmitAsync(Guid id)
        {
            try
            {
                var assessment = await _unitOfWork.Assessments.GetByIdAsync(id);
                if (assessment == null)
                {
                    throw new AssessmentException(id, "Assessment not found");
                }

                // Validate assessment can be submitted
                // MEDIUM FIX: Status string should be "InProgress" (no space) to match entity convention
                if (assessment.Status != "Draft" && assessment.Status != "InProgress")
                {
                    throw new AssessmentException(id, $"Assessment in status '{assessment.Status}' cannot be submitted. Only Draft or InProgress assessments can be submitted.");
                }

                // Update status
                assessment.Status = "Submitted";
                assessment.ModifiedDate = DateTime.UtcNow;
                assessment.ModifiedBy = GetCurrentUser();

                await _unitOfWork.Assessments.UpdateAsync(assessment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Assessment {AssessmentId} submitted successfully", id);
                return _mapper.Map<AssessmentDto>(assessment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting assessment {AssessmentId}", id);
                throw;
            }
        }

        /// <summary>
        /// Approve an assessment
        /// </summary>
        public async Task<AssessmentDto> ApproveAsync(Guid id)
        {
            try
            {
                var assessment = await _unitOfWork.Assessments.GetByIdAsync(id);
                if (assessment == null)
                {
                    throw new AssessmentException(id, "Assessment not found");
                }

                // Validate assessment can be approved
                if (assessment.Status != "Submitted")
                {
                    throw new AssessmentException(id, $"Assessment in status '{assessment.Status}' cannot be approved. Only Submitted assessments can be approved.");
                }

                // Update status
                assessment.Status = "Approved";
                assessment.ModifiedDate = DateTime.UtcNow;
                assessment.ModifiedBy = GetCurrentUser();

                await _unitOfWork.Assessments.UpdateAsync(assessment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Assessment {AssessmentId} approved successfully", id);
                return _mapper.Map<AssessmentDto>(assessment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving assessment {AssessmentId}", id);
                throw;
            }
        }

        /// <summary>
        /// Reject an assessment (return to draft)
        /// </summary>
        public async Task<AssessmentDto> RejectAsync(Guid id, string reason)
        {
            try
            {
                var assessment = await _unitOfWork.Assessments.GetByIdAsync(id);
                if (assessment == null)
                {
                    throw new AssessmentException(id, "Assessment not found");
                }

                // Validate assessment can be rejected
                if (assessment.Status != "Submitted" && assessment.Status != "UnderReview")
                {
                    throw new AssessmentException(id, $"Assessment in status '{assessment.Status}' cannot be rejected.");
                }

                assessment.Status = "Draft";
                assessment.Findings = string.IsNullOrEmpty(assessment.Findings) 
                    ? $"Rejected: {reason}" 
                    : $"{assessment.Findings}\nRejected: {reason}";
                assessment.ModifiedDate = DateTime.UtcNow;
                assessment.ModifiedBy = GetCurrentUser();

                await _unitOfWork.Assessments.UpdateAsync(assessment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Assessment {AssessmentId} rejected: {Reason}", id, reason);
                return _mapper.Map<AssessmentDto>(assessment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting assessment {AssessmentId}", id);
                throw;
            }
        }

        /// <summary>
        /// Cancel an assessment
        /// </summary>
        public async Task<AssessmentDto> CancelAsync(Guid id, string reason)
        {
            try
            {
                var assessment = await _unitOfWork.Assessments.GetByIdAsync(id);
                if (assessment == null)
                {
                    throw new AssessmentException(id, "Assessment not found");
                }

                // Cannot cancel completed or archived assessments
                if (assessment.Status == "Completed" || assessment.Status == "Archived")
                {
                    throw new AssessmentException(id, $"Assessment in status '{assessment.Status}' cannot be cancelled.");
                }

                assessment.Status = "Cancelled";
                assessment.Findings = string.IsNullOrEmpty(assessment.Findings) 
                    ? $"Cancelled: {reason}" 
                    : $"{assessment.Findings}\nCancelled: {reason}";
                assessment.ModifiedDate = DateTime.UtcNow;
                assessment.ModifiedBy = GetCurrentUser();

                await _unitOfWork.Assessments.UpdateAsync(assessment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Assessment {AssessmentId} cancelled: {Reason}", id, reason);
                return _mapper.Map<AssessmentDto>(assessment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling assessment {AssessmentId}", id);
                throw;
            }
        }

        /// <summary>
        /// Start review of a submitted assessment
        /// </summary>
        public async Task<AssessmentDto> StartReviewAsync(Guid id)
        {
            try
            {
                var assessment = await _unitOfWork.Assessments.GetByIdAsync(id);
                if (assessment == null)
                {
                    throw new AssessmentException(id, "Assessment not found");
                }

                if (assessment.Status != "Submitted")
                {
                    throw new AssessmentException(id, $"Assessment in status '{assessment.Status}' cannot start review. Only Submitted assessments can be reviewed.");
                }

                assessment.Status = "UnderReview";
                assessment.ModifiedDate = DateTime.UtcNow;
                assessment.ModifiedBy = GetCurrentUser();

                await _unitOfWork.Assessments.UpdateAsync(assessment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Assessment {AssessmentId} started review", id);
                return _mapper.Map<AssessmentDto>(assessment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting review for assessment {AssessmentId}", id);
                throw;
            }
        }

        /// <summary>
        /// Complete an approved assessment
        /// </summary>
        public async Task<AssessmentDto> CompleteAsync(Guid id)
        {
            try
            {
                var assessment = await _unitOfWork.Assessments.GetByIdAsync(id);
                if (assessment == null)
                {
                    throw new AssessmentException(id, "Assessment not found");
                }

                if (assessment.Status != "Approved")
                {
                    throw new AssessmentException(id, $"Assessment in status '{assessment.Status}' cannot be completed. Only Approved assessments can be completed.");
                }

                assessment.Status = "Completed";
                assessment.EndDate = DateTime.UtcNow;
                assessment.ModifiedDate = DateTime.UtcNow;
                assessment.ModifiedBy = GetCurrentUser();

                await _unitOfWork.Assessments.UpdateAsync(assessment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Assessment {AssessmentId} completed", id);
                return _mapper.Map<AssessmentDto>(assessment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing assessment {AssessmentId}", id);
                throw;
            }
        }

        /// <summary>
        /// Archive a completed assessment
        /// </summary>
        public async Task<AssessmentDto> ArchiveAsync(Guid id)
        {
            try
            {
                var assessment = await _unitOfWork.Assessments.GetByIdAsync(id);
                if (assessment == null)
                {
                    throw new AssessmentException(id, "Assessment not found");
                }

                if (assessment.Status != "Completed")
                {
                    throw new AssessmentException(id, $"Assessment in status '{assessment.Status}' cannot be archived. Only Completed assessments can be archived.");
                }

                assessment.Status = "Archived";
                assessment.ModifiedDate = DateTime.UtcNow;
                assessment.ModifiedBy = GetCurrentUser();

                await _unitOfWork.Assessments.UpdateAsync(assessment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Assessment {AssessmentId} archived", id);
                return _mapper.Map<AssessmentDto>(assessment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving assessment {AssessmentId}", id);
                throw;
            }
        }

        private string? GetCurrentUser()
        {
            // Use injected IHttpContextAccessor for proper user identification
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
        }
    }
}