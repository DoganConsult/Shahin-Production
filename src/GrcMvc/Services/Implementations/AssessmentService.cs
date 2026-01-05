using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Policy;
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

        public AssessmentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<AssessmentService> logger,
            PolicyEnforcementHelper policyHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _policyHelper = policyHelper ?? throw new ArgumentNullException(nameof(policyHelper));
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
                var assessments = await _unitOfWork.Assessments.GetAllAsync();
                var assessmentsList = assessments.ToList();

                return new AssessmentStatisticsDto
                {
                    TotalAssessments = assessmentsList.Count,
                    CompletedAssessments = assessmentsList.Count(a => a.Status == "Completed"),
                    PendingAssessments = assessmentsList.Count(a => a.Status == "Scheduled" || a.Status == "In Progress"),
                    OverdueAssessments = assessmentsList.Count(a =>
                        a.ScheduledDate < DateTime.UtcNow && a.Status != "Completed"),
                    AverageScore = assessmentsList.Where(a => a.Score > 0).Any()
                        ? assessmentsList.Where(a => a.Score > 0).Average(a => a.Score)
                        : 0,
                    AssessmentsByType = assessmentsList.GroupBy(a => a.Type)
                        .ToDictionary(g => g.Key, g => g.Count())
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
                    throw new InvalidOperationException($"Plan '{planId}' not found.");

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
            // Get controls for this framework from FrameworkControls table
            var controls = await _unitOfWork.FrameworkControls
                .Query()
                .Where(c => c.FrameworkCode == frameworkCode)
                .OrderBy(c => c.ControlNumber)
                .Take(500) // Limit for performance
                .ToListAsync();

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
    }
}