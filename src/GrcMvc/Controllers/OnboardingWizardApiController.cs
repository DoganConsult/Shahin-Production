using System;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// API Controller for comprehensive 12-step onboarding wizard.
    /// Provides RESTful endpoints for all sections A-L (96 questions total).
    /// Supports both full wizard and minimal onboarding flows.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/onboarding/wizard")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [IgnoreAntiforgeryToken] // API endpoints don't require CSRF tokens
    public class OnboardingWizardApiController : ControllerBase
    {
        private readonly IOnboardingWizardService _wizardService;
        private readonly ILogger<OnboardingWizardApiController> _logger;

        public OnboardingWizardApiController(
            IOnboardingWizardService wizardService,
            ILogger<OnboardingWizardApiController> logger)
        {
            _wizardService = wizardService;
            _logger = logger;
        }

        /// <summary>
        /// Start a new onboarding wizard for a tenant.
        /// </summary>
        [HttpPost("tenants/{tenantId:guid}/start")]
        public async Task<IActionResult> StartWizard(Guid tenantId)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var wizard = await _wizardService.StartWizardAsync(tenantId, userId);

                return Ok(new
                {
                    wizard.Id,
                    wizard.TenantId,
                    wizard.WizardStatus,
                    wizard.CurrentStep,
                    Message = "Wizard started successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting wizard for tenant {TenantId}", tenantId);
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        /// <summary>
        /// Get current wizard state for a tenant.
        /// </summary>
        [HttpGet("tenants/{tenantId:guid}")]
        public async Task<IActionResult> GetWizardState(Guid tenantId)
        {
            try
            {
                var state = await _wizardService.GetWizardStateAsync(tenantId);
                if (state == null)
                {
                    return NotFound(new { error = "Wizard not found. Start a new wizard first." });
                }

                return Ok(state);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wizard state for tenant {TenantId}", tenantId);
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        /// <summary>
        /// Get wizard progress summary.
        /// </summary>
        [HttpGet("tenants/{tenantId:guid}/progress")]
        public async Task<IActionResult> GetProgress(Guid tenantId)
        {
            try
            {
                var progress = await _wizardService.GetProgressAsync(tenantId);
                return Ok(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wizard progress for tenant {TenantId}", tenantId);
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        #region Section Endpoints

        /// <summary>
        /// Save Section A: Organization Identity and Tenancy (Q1-13).
        /// Required section for minimal onboarding.
        /// </summary>
        [HttpPut("tenants/{tenantId:guid}/sections/a")]
        public async Task<IActionResult> SaveSectionA(Guid tenantId, [FromBody] SectionA_OrganizationIdentity section)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var result = await _wizardService.SaveSectionAAsync(tenantId, section, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Section A for tenant {TenantId}", tenantId);
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        /// <summary>
        /// Save Section B: Assurance Objective (Q14-18).
        /// Optional section.
        /// </summary>
        [HttpPut("tenants/{tenantId:guid}/sections/b")]
        public async Task<IActionResult> SaveSectionB(Guid tenantId, [FromBody] SectionB_AssuranceObjective section)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var result = await _wizardService.SaveSectionBAsync(tenantId, section, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Section B for tenant {TenantId}", tenantId);
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        /// <summary>
        /// Save Section C: Regulatory and Framework Applicability (Q19-25).
        /// Optional section.
        /// </summary>
        [HttpPut("tenants/{tenantId:guid}/sections/c")]
        public async Task<IActionResult> SaveSectionC(Guid tenantId, [FromBody] SectionC_RegulatoryApplicability section)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var result = await _wizardService.SaveSectionCAsync(tenantId, section, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Section C for tenant {TenantId}", tenantId);
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        /// <summary>
        /// Save Section D: Scope Definition (Q26-34).
        /// Required section for minimal onboarding.
        /// </summary>
        [HttpPut("tenants/{tenantId:guid}/sections/d")]
        public async Task<IActionResult> SaveSectionD(Guid tenantId, [FromBody] SectionD_ScopeDefinition section)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var result = await _wizardService.SaveSectionDAsync(tenantId, section, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Section D for tenant {TenantId}", tenantId);
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        /// <summary>
        /// Save Section E: Data and Risk Profile (Q35-40).
        /// Required section for minimal onboarding.
        /// </summary>
        [HttpPut("tenants/{tenantId:guid}/sections/e")]
        public async Task<IActionResult> SaveSectionE(Guid tenantId, [FromBody] SectionE_DataRiskProfile section)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var result = await _wizardService.SaveSectionEAsync(tenantId, section, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Section E for tenant {TenantId}", tenantId);
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        /// <summary>
        /// Save Section F: Technology Landscape (Q41-53).
        /// Required section for minimal onboarding.
        /// </summary>
        [HttpPut("tenants/{tenantId:guid}/sections/f")]
        public async Task<IActionResult> SaveSectionF(Guid tenantId, [FromBody] SectionF_TechnologyLandscape section)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var result = await _wizardService.SaveSectionFAsync(tenantId, section, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Section F for tenant {TenantId}", tenantId);
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        /// <summary>
        /// Save Section G: Control Ownership Model (Q54-60).
        /// Optional section.
        /// </summary>
        [HttpPut("tenants/{tenantId:guid}/sections/g")]
        public async Task<IActionResult> SaveSectionG(Guid tenantId, [FromBody] SectionG_ControlOwnership section)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var result = await _wizardService.SaveSectionGAsync(tenantId, section, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Section G for tenant {TenantId}", tenantId);
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        /// <summary>
        /// Save Section H: Teams, Roles, and Access (Q61-70).
        /// Required section for minimal onboarding.
        /// </summary>
        [HttpPut("tenants/{tenantId:guid}/sections/h")]
        public async Task<IActionResult> SaveSectionH(Guid tenantId, [FromBody] SectionH_TeamsRolesAccess section)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var result = await _wizardService.SaveSectionHAsync(tenantId, section, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Section H for tenant {TenantId}", tenantId);
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        /// <summary>
        /// Save Section I: Workflow and Cadence (Q71-80).
        /// Required section for minimal onboarding.
        /// </summary>
        [HttpPut("tenants/{tenantId:guid}/sections/i")]
        public async Task<IActionResult> SaveSectionI(Guid tenantId, [FromBody] SectionI_WorkflowCadence section)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var result = await _wizardService.SaveSectionIAsync(tenantId, section, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Section I for tenant {TenantId}", tenantId);
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        /// <summary>
        /// Save Section J: Evidence Standards (Q81-87).
        /// Optional section.
        /// </summary>
        [HttpPut("tenants/{tenantId:guid}/sections/j")]
        public async Task<IActionResult> SaveSectionJ(Guid tenantId, [FromBody] SectionJ_EvidenceStandards section)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var result = await _wizardService.SaveSectionJAsync(tenantId, section, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Section J for tenant {TenantId}", tenantId);
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        /// <summary>
        /// Save Section K: Baseline and Overlays Selection (Q88-90).
        /// Optional section.
        /// </summary>
        [HttpPut("tenants/{tenantId:guid}/sections/k")]
        public async Task<IActionResult> SaveSectionK(Guid tenantId, [FromBody] SectionK_BaselineOverlays section)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var result = await _wizardService.SaveSectionKAsync(tenantId, section, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Section K for tenant {TenantId}", tenantId);
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        /// <summary>
        /// Save Section L: Go-Live and Success Metrics (Q91-96).
        /// Optional section.
        /// </summary>
        [HttpPut("tenants/{tenantId:guid}/sections/l")]
        public async Task<IActionResult> SaveSectionL(Guid tenantId, [FromBody] SectionL_GoLiveMetrics section)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var result = await _wizardService.SaveSectionLAsync(tenantId, section, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Section L for tenant {TenantId}", tenantId);
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        #endregion

        #region Minimal Onboarding

        /// <summary>
        /// Save minimal onboarding data (short form).
        /// Covers required sections: A1-A9, D26-D33, E35-E38, F41-F50, H61-H66, I71-I79.
        /// </summary>
        [HttpPut("tenants/{tenantId:guid}/minimal")]
        public async Task<IActionResult> SaveMinimalOnboarding(Guid tenantId, [FromBody] MinimalOnboardingDto data)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var result = await _wizardService.SaveMinimalOnboardingAsync(tenantId, data, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving minimal onboarding for tenant {TenantId}", tenantId);
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        #endregion

        #region Validation and Completion

        /// <summary>
        /// Validate wizard completeness.
        /// </summary>
        [HttpGet("tenants/{tenantId:guid}/validate")]
        public async Task<IActionResult> ValidateWizard(Guid tenantId, [FromQuery] bool minimalOnly = false)
        {
            try
            {
                var result = await _wizardService.ValidateWizardAsync(tenantId, minimalOnly);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating wizard for tenant {TenantId}", tenantId);
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        /// <summary>
        /// Complete wizard and trigger scope derivation.
        /// Requires all mandatory sections (A, D, E, F, H, I) to be complete.
        /// </summary>
        [HttpPost("tenants/{tenantId:guid}/complete")]
        public async Task<IActionResult> CompleteWizard(Guid tenantId)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var result = await _wizardService.CompleteWizardAsync(tenantId, userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing wizard for tenant {TenantId}", tenantId);
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        /// <summary>
        /// Get coverage validation for a specific section
        /// </summary>
        [HttpGet("tenants/{tenantId:guid}/coverage/{sectionId}")]
        public async Task<IActionResult> GetSectionCoverage(Guid tenantId, string sectionId)
        {
            try
            {
                var coverageResult = await _wizardService.ValidateSectionCoverageAsync(tenantId, sectionId);
                if (coverageResult == null)
                {
                    return NotFound(new { error = $"Coverage validation not found for section {sectionId}" });
                }
                return Ok(coverageResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting section coverage for tenant {TenantId}, section {SectionId}", tenantId, sectionId);
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        /// <summary>
        /// Get coverage validation for all sections
        /// </summary>
        [HttpGet("tenants/{tenantId:guid}/coverage")]
        public async Task<IActionResult> GetAllSectionsCoverage(Guid tenantId)
        {
            try
            {
                var allCoverage = await _wizardService.GetAllSectionsCoverageAsync(tenantId);
                return Ok(allCoverage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all sections coverage for tenant {TenantId}", tenantId);
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        /// <summary>
        /// Get derived scope after wizard completion.
        /// </summary>
        [HttpGet("tenants/{tenantId:guid}/scope")]
        public async Task<IActionResult> GetDerivedScope(Guid tenantId)
        {
            try
            {
                var scope = await _wizardService.GetDerivedScopeAsync(tenantId);
                return Ok(scope);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting derived scope for tenant {TenantId}", tenantId);
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        #endregion
    }
}
