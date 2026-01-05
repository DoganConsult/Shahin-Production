using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Services.Interfaces;
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

        public OnboardingService(
            IUnitOfWork unitOfWork,
            IRulesEngineService rulesEngine,
            IAuditEventService auditService,
            ILogger<OnboardingService> logger)
        {
            _unitOfWork = unitOfWork;
            _rulesEngine = rulesEngine;
            _auditService = auditService;
            _logger = logger;
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
            try
            {
                var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
                if (tenant == null)
                {
                    throw new InvalidOperationException($"Tenant '{tenantId}' not found.");
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

                await _unitOfWork.OrganizationProfiles.AddAsync(profile);
                await _unitOfWork.SaveChangesAsync();

                // Log event
                await _auditService.LogEventAsync(
                    tenantId: tenantId,
                    eventType: "OrganizationProfileCreated",
                    affectedEntityType: "OrganizationProfile",
                    affectedEntityId: profile.Id.ToString(),
                    action: "Create",
                    actor: userId,
                    payloadJson: JsonSerializer.Serialize(profile),
                    correlationId: tenant.CorrelationId
                );

                _logger.LogInformation($"Organization profile created for tenant {tenantId}");
                return profile;
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
            try
            {
                var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
                if (tenant == null)
                {
                    throw new InvalidOperationException($"Tenant '{tenantId}' not found.");
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
    }
}
