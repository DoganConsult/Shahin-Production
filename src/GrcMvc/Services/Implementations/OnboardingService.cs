using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Exceptions;
using GrcMvc.Models.Entities;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Policy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for tenant onboarding workflow.
    /// Handles organizational profile setup and rules engine invocation.
    /// </summary>
    public class OnboardingService : IOnboardingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRulesEngineService _rulesEngine;
        private readonly IAuditEventService _auditService;
        private readonly ILogger<OnboardingService> _logger;
        private readonly PolicyEnforcementHelper _policyHelper;

        public OnboardingService(
            IUnitOfWork unitOfWork,
            IRulesEngineService rulesEngine,
            IAuditEventService auditService,
            ILogger<OnboardingService> logger,
            PolicyEnforcementHelper policyHelper)
        {
            _unitOfWork = unitOfWork;
            _rulesEngine = rulesEngine;
            _auditService = auditService;
            _logger = logger;
            _policyHelper = policyHelper;
        }

        /// <summary>
        /// Save organizational profile from onboarding questionnaire.
        /// </summary>
        public async Task<OrganizationProfile> SaveOrganizationProfileAsync(
            Guid tenantId,
            string orgType,
            string sector,
            string country,
            string dataTypes,
            string hostingModel,
            string organizationSize,
            string complianceMaturity,
            string vendors,
            Dictionary<string, string> questionnaire,
            string userId)
        {
            // Input validation
            if (tenantId == Guid.Empty)
                throw new ArgumentException("Tenant ID is required", nameof(tenantId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required", nameof(userId));
            if (string.IsNullOrWhiteSpace(orgType))
                throw new ArgumentException("Organization type is required", nameof(orgType));
            if (string.IsNullOrWhiteSpace(sector))
                throw new ArgumentException("Sector is required", nameof(sector));

            try
            {
                var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
                if (tenant == null)
                {
                    throw new EntityNotFoundException("Tenant", tenantId);
                }

                var profile = new OrganizationProfile
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    OrganizationType = orgType,
                    Sector = sector,
                    Country = country ?? "SA",
                    DataTypes = dataTypes,
                    HostingModel = hostingModel,
                    OrganizationSize = organizationSize,
                    ComplianceMaturity = complianceMaturity,
                    Vendors = vendors,
                    OnboardingQuestionsJson = JsonSerializer.Serialize(questionnaire),
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId
                };

                // Enforce policy before saving organization profile
                await _policyHelper.EnforceCreateAsync(
                    resourceType: "OrganizationProfile",
                    resource: profile,
                    dataClassification: "confidential",
                    owner: userId);

                await _unitOfWork.OrganizationProfiles.AddAsync(profile);
                await _unitOfWork.SaveChangesAsync();

                // Log event (use options to avoid circular reference issues)
                var jsonOptions = new JsonSerializerOptions
                {
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                    WriteIndented = false
                };
                await _auditService.LogEventAsync(
                    tenantId: tenantId,
                    eventType: "OrganizationProfileCreated",
                    affectedEntityType: "OrganizationProfile",
                    affectedEntityId: profile.Id.ToString(),
                    action: "Create",
                    actor: userId,
                    payloadJson: JsonSerializer.Serialize(profile, jsonOptions),
                    correlationId: tenant.CorrelationId
                );

                _logger.LogInformation($"Organization profile created for tenant {tenantId}");
                return profile;
            }
            catch (PolicyViolationException pve)
            {
                _logger.LogWarning($"Policy violation prevented organization profile creation: {pve.Message}. Rule: {pve.RuleId}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving organization profile");
                throw;
            }
        }

        /// <summary>
        /// Complete onboarding and trigger rules engine to derive scope.
        /// </summary>
        public async Task<RuleExecutionLog> CompleteOnboardingAsync(Guid tenantId, string userId)
        {
            // Input validation
            if (tenantId == Guid.Empty)
                throw new ArgumentException("Tenant ID is required", nameof(tenantId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required", nameof(userId));

            try
            {
                var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
                if (tenant == null)
                {
                    throw new EntityNotFoundException("Tenant", tenantId);
                }

                // Verify organization profile exists before completing
                var profile = await _unitOfWork.OrganizationProfiles
                    .Query()
                    .FirstOrDefaultAsync(p => p.TenantId == tenantId);
                if (profile == null)
                {
                    throw new EntityNotFoundException("OrganizationProfile", $"tenant:{tenantId}");
                }

                // Execute rules engine to derive and persist scope
                var executionLog = await _rulesEngine.DeriveAndPersistScopeAsync(tenantId, userId);

                // Log event
                await _auditService.LogEventAsync(
                    tenantId: tenantId,
                    eventType: "OnboardingCompleted",
                    affectedEntityType: "Tenant",
                    affectedEntityId: tenantId.ToString(),
                    action: "Complete",
                    actor: userId,
                    payloadJson: JsonSerializer.Serialize(new {
                        Status = "Completed",
                        ExecutionLogId = executionLog?.Id
                    }),
                    correlationId: tenant.CorrelationId
                );

                _logger.LogInformation($"Onboarding completed for tenant {tenantId}");
                return executionLog;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing onboarding");
                throw;
            }
        }

        /// <summary>
        /// Get derived scope (applicable baselines, packages, templates) for tenant.
        /// </summary>
        public async Task<OnboardingScopeDto> GetDerivedScopeAsync(Guid tenantId)
        {
            if (tenantId == Guid.Empty)
                throw new ArgumentException("Tenant ID is required", nameof(tenantId));

            try
            {
                var baselines = await _unitOfWork.TenantBaselines
                    .Query()
                    .Where(b => b.TenantId == tenantId && !b.IsDeleted)
                    .ToListAsync();

                var packages = await _unitOfWork.TenantPackages
                    .Query()
                    .Where(p => p.TenantId == tenantId && !p.IsDeleted)
                    .ToListAsync();

                var templates = await _unitOfWork.TenantTemplates
                    .Query()
                    .Where(t => t.TenantId == tenantId && !t.IsDeleted)
                    .ToListAsync();

                return new OnboardingScopeDto
                {
                    TenantId = tenantId,
                    ApplicableBaselines = baselines.Select(b => new BaselineDto
                    {
                        BaselineCode = b.BaselineCode,
                        Name = b.BaselineCode,
                        ReasonJson = b.ReasonJson
                    }).ToList(),
                    ApplicablePackages = packages.Select(p => new PackageDto
                    {
                        PackageCode = p.PackageCode,
                        Name = p.PackageCode,
                        ReasonJson = p.ReasonJson
                    }).ToList(),
                    ApplicableTemplates = templates.Select(t => new TemplateDto
                    {
                        TemplateCode = t.TemplateCode,
                        Name = t.TemplateCode,
                        ReasonJson = t.ReasonJson
                    }).ToList(),
                    RetrievedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting derived scope");
                throw;
            }
        }

        /// <summary>
        /// Re-evaluate and refresh scope when profile or assets change.
        /// </summary>
        public async Task<RuleExecutionLog> RefreshScopeAsync(Guid tenantId, string userId, string reason)
        {
            // Input validation
            if (tenantId == Guid.Empty)
                throw new ArgumentException("Tenant ID is required", nameof(tenantId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required", nameof(userId));

            try
            {
                var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
                if (tenant == null)
                {
                    throw new EntityNotFoundException("Tenant", tenantId);
                }

                _logger.LogInformation("Refreshing scope for tenant {TenantId}. Reason: {Reason}", tenantId, reason);

                // Re-execute rules engine to derive updated scope
                var executionLog = await _rulesEngine.DeriveAndPersistScopeAsync(tenantId, userId);

                // Log audit event
                await _auditService.LogEventAsync(
                    tenantId: tenantId,
                    eventType: "ScopeRefreshed",
                    affectedEntityType: "Tenant",
                    affectedEntityId: tenantId.ToString(),
                    action: "RefreshScope",
                    actor: userId,
                    payloadJson: System.Text.Json.JsonSerializer.Serialize(new {
                        Reason = reason,
                        ExecutionLogId = executionLog?.Id,
                        RefreshedAt = DateTime.UtcNow
                    }),
                    correlationId: tenant.CorrelationId
                );

                _logger.LogInformation("Scope refreshed for tenant {TenantId}. ExecutionLog: {LogId}",
                    tenantId, executionLog?.Id);

                return executionLog;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing scope for tenant {TenantId}", tenantId);
                throw;
            }
        }

        // ============================================
        // Enhanced UX Methods for Smooth Onboarding
        // ============================================

        /// <summary>
        /// Get current onboarding status and progress for a tenant.
        /// </summary>
        public async Task<OnboardingStatusDto> GetOnboardingStatusAsync(Guid tenantId)
        {
            if (tenantId == Guid.Empty)
                throw new ArgumentException("Tenant ID is required", nameof(tenantId));

            var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
            if (tenant == null)
                throw new EntityNotFoundException("Tenant", tenantId);

            var hasProfile = await HasOrganizationProfileAsync(tenantId);
            var hasScope = await _unitOfWork.TenantBaselines
                .Query()
                .AnyAsync(b => b.TenantId == tenantId && !b.IsDeleted);

            var completionPct = await GetCompletionPercentageAsync(tenantId);
            var isComplete = tenant.OnboardingStatus == "COMPLETED";

            var status = new OnboardingStatusDto
            {
                TenantId = tenantId,
                CurrentStep = DetermineCurrentStep(hasProfile, hasScope, isComplete),
                CompletionPercentage = completionPct,
                IsProfileComplete = hasProfile,
                IsScopeDerived = hasScope,
                IsOnboardingComplete = isComplete,
                StartedAt = tenant.CreatedDate,
                CompletedAt = isComplete ? tenant.ModifiedDate : null,
                Steps = BuildStepStatuses(hasProfile, hasScope, isComplete)
            };

            // Set next action
            if (!hasProfile)
            {
                status.NextAction = "Complete Organization Profile";
                status.NextActionUrl = "/Onboarding/OrgProfile";
            }
            else if (!hasScope)
            {
                status.NextAction = "Review Compliance Scope";
                status.NextActionUrl = "/Onboarding/ReviewScope";
            }
            else if (!isComplete)
            {
                status.NextAction = "Create Your First Plan";
                status.NextActionUrl = "/Onboarding/CreatePlan";
            }

            return status;
        }

        /// <summary>
        /// Check if tenant can proceed to a specific onboarding step.
        /// </summary>
        public async Task<StepValidationResult> CanProceedToStepAsync(Guid tenantId, string stepName)
        {
            if (tenantId == Guid.Empty)
                throw new ArgumentException("Tenant ID is required", nameof(tenantId));

            var hasProfile = await HasOrganizationProfileAsync(tenantId);
            var hasScope = await _unitOfWork.TenantBaselines
                .Query()
                .AnyAsync(b => b.TenantId == tenantId && !b.IsDeleted);

            return stepName.ToLower() switch
            {
                "orgprofile" or "org-profile" or "profile" => new StepValidationResult { CanProceed = true },
                "reviewscope" or "review-scope" or "scope" => hasProfile
                    ? new StepValidationResult { CanProceed = true }
                    : new StepValidationResult
                    {
                        CanProceed = false,
                        BlockerReason = "Please complete your organization profile first.",
                        BlockerReasonAr = "يرجى إكمال ملف المؤسسة أولاً.",
                        MissingPrerequisites = new List<string> { "Organization Profile" },
                        RedirectToStep = "OrgProfile"
                    },
                "createplan" or "create-plan" or "plan" => hasProfile && hasScope
                    ? new StepValidationResult { CanProceed = true }
                    : new StepValidationResult
                    {
                        CanProceed = false,
                        BlockerReason = hasProfile ? "Please review your compliance scope first." : "Please complete your organization profile first.",
                        BlockerReasonAr = hasProfile ? "يرجى مراجعة نطاق الامتثال أولاً." : "يرجى إكمال ملف المؤسسة أولاً.",
                        MissingPrerequisites = hasProfile ? new List<string> { "Scope Review" } : new List<string> { "Organization Profile", "Scope Review" },
                        RedirectToStep = hasProfile ? "ReviewScope" : "OrgProfile"
                    },
                _ => new StepValidationResult { CanProceed = true }
            };
        }

        /// <summary>
        /// Get recommended next step based on current progress.
        /// </summary>
        public async Task<NextStepRecommendation> GetNextStepAsync(Guid tenantId)
        {
            if (tenantId == Guid.Empty)
                throw new ArgumentException("Tenant ID is required", nameof(tenantId));

            var hasProfile = await HasOrganizationProfileAsync(tenantId);
            var hasScope = await _unitOfWork.TenantBaselines
                .Query()
                .AnyAsync(b => b.TenantId == tenantId && !b.IsDeleted);

            var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
            var isComplete = tenant?.OnboardingStatus == "COMPLETED";

            if (!hasProfile)
            {
                return new NextStepRecommendation
                {
                    StepName = "Organization Profile",
                    StepNameAr = "ملف المؤسسة",
                    StepUrl = "/Onboarding/OrgProfile",
                    Description = "Tell us about your organization to help us recommend the right compliance frameworks.",
                    DescriptionAr = "أخبرنا عن مؤسستك لمساعدتنا في التوصية بأطر الامتثال المناسبة.",
                    EstimatedMinutes = 5,
                    IsOptional = false
                };
            }

            if (!hasScope)
            {
                return new NextStepRecommendation
                {
                    StepName = "Review Compliance Scope",
                    StepNameAr = "مراجعة نطاق الامتثال",
                    StepUrl = "/Onboarding/ReviewScope",
                    Description = "Review the recommended compliance frameworks based on your organization profile.",
                    DescriptionAr = "راجع أطر الامتثال الموصى بها بناءً على ملف مؤسستك.",
                    EstimatedMinutes = 3,
                    IsOptional = false
                };
            }

            if (!isComplete)
            {
                return new NextStepRecommendation
                {
                    StepName = "Create Assessment Plan",
                    StepNameAr = "إنشاء خطة التقييم",
                    StepUrl = "/Onboarding/CreatePlan",
                    Description = "Create your first compliance assessment plan to start your GRC journey.",
                    DescriptionAr = "أنشئ أول خطة تقييم امتثال لبدء رحلتك في الحوكمة والمخاطر والامتثال.",
                    EstimatedMinutes = 2,
                    IsOptional = false
                };
            }

            return new NextStepRecommendation
            {
                StepName = "Dashboard",
                StepNameAr = "لوحة التحكم",
                StepUrl = "/Dashboard",
                Description = "Your onboarding is complete! Start managing your GRC program.",
                DescriptionAr = "اكتمل إعدادك! ابدأ في إدارة برنامج الحوكمة والمخاطر والامتثال.",
                EstimatedMinutes = 0,
                IsOptional = true
            };
        }

        /// <summary>
        /// Resume onboarding from last saved state.
        /// </summary>
        public async Task<ResumeOnboardingResult> ResumeOnboardingAsync(Guid tenantId)
        {
            if (tenantId == Guid.Empty)
                throw new ArgumentException("Tenant ID is required", nameof(tenantId));

            var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
            if (tenant == null)
            {
                return new ResumeOnboardingResult
                {
                    CanResume = false,
                    ResumeStep = "Signup",
                    ResumeUrl = "/Onboarding/Signup"
                };
            }

            var hasProfile = await HasOrganizationProfileAsync(tenantId);
            var hasScope = await _unitOfWork.TenantBaselines
                .Query()
                .AnyAsync(b => b.TenantId == tenantId && !b.IsDeleted);
            var isComplete = tenant.OnboardingStatus == "COMPLETED";

            int completedSteps = 0;
            if (hasProfile) completedSteps++;
            if (hasScope) completedSteps++;
            if (isComplete) completedSteps++;

            string resumeStep, resumeUrl;
            if (!hasProfile)
            {
                resumeStep = "OrgProfile";
                resumeUrl = "/Onboarding/OrgProfile";
            }
            else if (!hasScope)
            {
                resumeStep = "ReviewScope";
                resumeUrl = "/Onboarding/ReviewScope";
            }
            else if (!isComplete)
            {
                resumeStep = "CreatePlan";
                resumeUrl = "/Onboarding/CreatePlan";
            }
            else
            {
                resumeStep = "Dashboard";
                resumeUrl = "/Dashboard";
            }

            return new ResumeOnboardingResult
            {
                CanResume = true,
                ResumeStep = resumeStep,
                ResumeUrl = resumeUrl,
                CompletedSteps = completedSteps,
                TotalSteps = 4,
                LastActivityAt = tenant.ModifiedDate,
                WelcomeBackMessage = $"Welcome back! You've completed {completedSteps} of 4 onboarding steps.",
                WelcomeBackMessageAr = $"مرحباً بعودتك! لقد أكملت {completedSteps} من 4 خطوات الإعداد."
            };
        }

        /// <summary>
        /// Check if organization profile exists for tenant.
        /// </summary>
        public async Task<bool> HasOrganizationProfileAsync(Guid tenantId)
        {
            if (tenantId == Guid.Empty) return false;

            return await _unitOfWork.OrganizationProfiles
                .Query()
                .AnyAsync(p => p.TenantId == tenantId);
        }

        /// <summary>
        /// Get onboarding completion percentage.
        /// </summary>
        public async Task<int> GetCompletionPercentageAsync(Guid tenantId)
        {
            if (tenantId == Guid.Empty) return 0;

            var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
            if (tenant == null) return 0;

            var hasProfile = await HasOrganizationProfileAsync(tenantId);
            var hasScope = await _unitOfWork.TenantBaselines
                .Query()
                .AnyAsync(b => b.TenantId == tenantId && !b.IsDeleted);
            var isComplete = tenant.OnboardingStatus == "COMPLETED";

            // 4 steps: Signup (25%), Profile (50%), Scope (75%), Plan (100%)
            if (isComplete) return 100;
            if (hasScope) return 75;
            if (hasProfile) return 50;
            return 25; // Tenant exists = signup done
        }

        // Helper methods
        private string DetermineCurrentStep(bool hasProfile, bool hasScope, bool isComplete)
        {
            if (isComplete) return "Completed";
            if (hasScope) return "CreatePlan";
            if (hasProfile) return "ReviewScope";
            return "OrgProfile";
        }

        private List<OnboardingStepStatus> BuildStepStatuses(bool hasProfile, bool hasScope, bool isComplete)
        {
            return new List<OnboardingStepStatus>
            {
                new OnboardingStepStatus
                {
                    StepName = "Organization Signup",
                    StepNameAr = "تسجيل المؤسسة",
                    StepNumber = 1,
                    Status = "Completed",
                    IsRequired = true,
                    IsAccessible = true
                },
                new OnboardingStepStatus
                {
                    StepName = "Organization Profile",
                    StepNameAr = "ملف المؤسسة",
                    StepNumber = 2,
                    Status = hasProfile ? "Completed" : "InProgress",
                    IsRequired = true,
                    IsAccessible = true
                },
                new OnboardingStepStatus
                {
                    StepName = "Review Compliance Scope",
                    StepNameAr = "مراجعة نطاق الامتثال",
                    StepNumber = 3,
                    Status = hasScope ? "Completed" : (hasProfile ? "InProgress" : "Pending"),
                    IsRequired = true,
                    IsAccessible = hasProfile
                },
                new OnboardingStepStatus
                {
                    StepName = "Create First Plan",
                    StepNameAr = "إنشاء الخطة الأولى",
                    StepNumber = 4,
                    Status = isComplete ? "Completed" : (hasScope ? "InProgress" : "Pending"),
                    IsRequired = true,
                    IsAccessible = hasScope
                }
            };
        }
    }
}
