using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// 43-Layer Architecture: Onboarding Control Plane API
    /// Provides endpoints for answer snapshots, rules evaluation, derived outputs, and explainability.
    /// </summary>
    [ApiController]
    [Route("api/onboarding-control-plane")]
    [Authorize]
    public class OnboardingControlPlaneController : ControllerBase
    {
        private readonly IOnboardingControlPlaneService _controlPlaneService;
        private readonly ITenantContextService _tenantService;
        private readonly ILogger<OnboardingControlPlaneController> _logger;

        public OnboardingControlPlaneController(
            IOnboardingControlPlaneService controlPlaneService,
            ITenantContextService tenantService,
            ILogger<OnboardingControlPlaneController> logger)
        {
            _controlPlaneService = controlPlaneService;
            _tenantService = tenantService;
            _logger = logger;
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // LAYER 14: Answer Snapshots
        // ═══════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Get all snapshots for a wizard (audit replay).
        /// </summary>
        [HttpGet("snapshots/{wizardId}")]
        public async Task<IActionResult> GetSnapshots(Guid wizardId)
        {
            var snapshots = await _controlPlaneService.GetSnapshotsAsync(wizardId);
            return Ok(snapshots);
        }

        /// <summary>
        /// Get the latest snapshot.
        /// </summary>
        [HttpGet("snapshots/{wizardId}/latest")]
        public async Task<IActionResult> GetLatestSnapshot(Guid wizardId)
        {
            var snapshot = await _controlPlaneService.GetLatestSnapshotAsync(wizardId);
            if (snapshot == null)
                return NotFound();
            return Ok(snapshot);
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // LAYER 15: Derived Outputs
        // ═══════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Get derived outputs for a wizard.
        /// </summary>
        [HttpGet("derived-outputs/{wizardId}")]
        public async Task<IActionResult> GetDerivedOutputs(Guid wizardId, [FromQuery] string? outputType = null)
        {
            var outputs = await _controlPlaneService.GetDerivedOutputsAsync(wizardId, outputType);
            return Ok(outputs);
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // LAYER 17: Explainability
        // ═══════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Get explainability payloads for the current tenant.
        /// </summary>
        [HttpGet("explainability")]
        public async Task<IActionResult> GetExplainability(
            [FromQuery] string? decisionType = null,
            [FromQuery] string? subjectCode = null)
        {
            if (!_tenantService.HasTenantContext())
                return Unauthorized("No tenant context");

            var tenantId = _tenantService.GetCurrentTenantId();
            var explanations = await _controlPlaneService.GetExplainabilityAsync(
                tenantId, decisionType, subjectCode);

            return Ok(explanations);
        }

        /// <summary>
        /// Override a decision with justification.
        /// </summary>
        [HttpPost("explainability/{payloadId}/override")]
        public async Task<IActionResult> OverrideDecision(
            Guid payloadId,
            [FromBody] OverrideDecisionRequest request)
        {
            var userId = User.Identity?.Name ?? "system";
            var payload = await _controlPlaneService.OverrideDecisionAsync(
                payloadId, request.NewDecision, request.Justification, userId);
            return Ok(payload);
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // LAYER 31-35: Tenant Compliance Resolution
        // ═══════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Get framework selections for current tenant.
        /// </summary>
        [HttpGet("framework-selections")]
        public async Task<IActionResult> GetFrameworkSelections()
        {
            if (!_tenantService.HasTenantContext())
                return Unauthorized("No tenant context");

            var tenantId = _tenantService.GetCurrentTenantId();
            var explanations = await _controlPlaneService.GetExplainabilityAsync(
                tenantId, "FRAMEWORK_SELECTION");

            return Ok(explanations);
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // COMPLETE ONBOARDING
        // ═══════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Complete onboarding and derive all outputs.
        /// </summary>
        [HttpPost("complete/{wizardId}")]
        public async Task<IActionResult> CompleteOnboarding(Guid wizardId)
        {
            if (!_tenantService.HasTenantContext())
                return Unauthorized("No tenant context");

            var tenantId = _tenantService.GetCurrentTenantId();
            var userId = User.Identity?.Name ?? "system";

            _logger.LogInformation(
                "Completing onboarding for wizard {WizardId} tenant {TenantId}",
                wizardId, tenantId);

            var result = await _controlPlaneService.CompleteOnboardingAsync(
                tenantId, wizardId, userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Evaluate rules for a step (manual trigger).
        /// </summary>
        [HttpPost("evaluate-rules/{wizardId}/{step}")]
        public async Task<IActionResult> EvaluateRules(
            Guid wizardId,
            int step,
            [FromBody] Dictionary<string, object> inputContext)
        {
            var result = await _controlPlaneService.EvaluateRulesAsync(wizardId, step, inputContext);
            return Ok(result);
        }
    }

    public class OverrideDecisionRequest
    {
        public string NewDecision { get; set; } = string.Empty;
        public string Justification { get; set; } = string.Empty;
    }
}
