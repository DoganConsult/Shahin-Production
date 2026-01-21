using GrcMvc.Models.Entities;
using GrcMvc.Models.Entities.Catalogs;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// PHASE 10: Admin Catalog Management API Controller
    /// Provides CRUD operations for global catalogs (Platform Governance)
    /// </summary>
    [Route("api/admin/catalog")]
    [ApiController]
    [IgnoreAntiforgeryToken]
    [Authorize(Roles = "Admin,PlatformAdmin")]
    public class AdminCatalogController : ControllerBase
    {
        private readonly IAdminCatalogService _catalogService;
        private readonly ILogger<AdminCatalogController> _logger;

        public AdminCatalogController(
            IAdminCatalogService catalogService,
            ILogger<AdminCatalogController> logger)
        {
            _catalogService = catalogService;
            _logger = logger;
        }

        private string CurrentUser => User.Identity?.Name ?? "System";

        #region Regulators

        [HttpPost("regulators")]
        public async Task<IActionResult> CreateRegulator([FromBody] RegulatorCatalog regulator)
        {
            try
            {
                var result = await _catalogService.CreateRegulatorAsync(regulator, CurrentUser);
                return Ok(new { success = true, id = result.Id, code = result.Code });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating regulator");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        [HttpPut("regulators/{id}")]
        public async Task<IActionResult> UpdateRegulator(Guid id, [FromBody] RegulatorCatalog regulator)
        {
            try
            {
                var result = await _catalogService.UpdateRegulatorAsync(id, regulator, CurrentUser);
                return Ok(new { success = true, id = result.Id });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = "The requested resource was not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating regulator");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        [HttpDelete("regulators/{id}")]
        public async Task<IActionResult> DeleteRegulator(Guid id)
        {
            var success = await _catalogService.DeleteRegulatorAsync(id, CurrentUser);
            return success ? Ok(new { success = true }) : NotFound(new { error = "Regulator not found" });
        }

        [HttpPost("regulators/{id}/activate")]
        public async Task<IActionResult> ActivateRegulator(Guid id)
        {
            var success = await _catalogService.ActivateRegulatorAsync(id, CurrentUser);
            return success ? Ok(new { success = true }) : NotFound(new { error = "Regulator not found" });
        }

        [HttpPost("regulators/{id}/deactivate")]
        public async Task<IActionResult> DeactivateRegulator(Guid id)
        {
            var success = await _catalogService.DeactivateRegulatorAsync(id, CurrentUser);
            return success ? Ok(new { success = true }) : NotFound(new { error = "Regulator not found" });
        }

        #endregion

        #region Frameworks

        [HttpPost("frameworks")]
        public async Task<IActionResult> CreateFramework([FromBody] FrameworkCatalog framework)
        {
            try
            {
                var result = await _catalogService.CreateFrameworkAsync(framework, CurrentUser);
                return Ok(new { success = true, id = result.Id, code = result.Code, version = result.Version });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating framework");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        [HttpPut("frameworks/{id}")]
        public async Task<IActionResult> UpdateFramework(Guid id, [FromBody] FrameworkCatalog framework)
        {
            try
            {
                var result = await _catalogService.UpdateFrameworkAsync(id, framework, CurrentUser);
                return Ok(new { success = true, id = result.Id });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = "The requested resource was not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating framework");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        [HttpDelete("frameworks/{id}")]
        public async Task<IActionResult> DeleteFramework(Guid id)
        {
            var success = await _catalogService.DeleteFrameworkAsync(id, CurrentUser);
            return success ? Ok(new { success = true }) : NotFound(new { error = "Framework not found" });
        }

        [HttpPost("frameworks/{id}/publish-version")]
        public async Task<IActionResult> PublishNewVersion(Guid id, [FromBody] PublishVersionRequest request)
        {
            try
            {
                var result = await _catalogService.PublishNewVersionAsync(id, request.NewVersion, CurrentUser);
                return Ok(new { success = true, id = result.Id, version = result.Version });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = "The requested resource was not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing new version");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        [HttpPost("frameworks/{id}/retire")]
        public async Task<IActionResult> RetireFramework(Guid id)
        {
            var success = await _catalogService.RetireFrameworkAsync(id, CurrentUser);
            return success ? Ok(new { success = true }) : NotFound(new { error = "Framework not found" });
        }

        #endregion

        #region Controls

        [HttpPost("controls")]
        public async Task<IActionResult> CreateControl([FromBody] ControlCatalog control)
        {
            try
            {
                var result = await _catalogService.CreateControlAsync(control, CurrentUser);
                return Ok(new { success = true, id = result.Id, controlId = result.ControlId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating control");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        [HttpPut("controls/{id}")]
        public async Task<IActionResult> UpdateControl(Guid id, [FromBody] ControlCatalog control)
        {
            try
            {
                var result = await _catalogService.UpdateControlAsync(id, control, CurrentUser);
                return Ok(new { success = true, id = result.Id });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = "The requested resource was not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating control");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        [HttpDelete("controls/{id}")]
        public async Task<IActionResult> DeleteControl(Guid id)
        {
            var success = await _catalogService.DeleteControlAsync(id, CurrentUser);
            return success ? Ok(new { success = true }) : NotFound(new { error = "Control not found" });
        }

        [HttpPost("controls/bulk-import")]
        public async Task<IActionResult> BulkImportControls([FromBody] List<ControlCatalog> controls)
        {
            try
            {
                var count = await _catalogService.BulkImportControlsAsync(controls, CurrentUser);
                return Ok(new { success = true, imported = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk importing controls");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        #endregion

        #region Roles

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _catalogService.GetAllRolesAsync();
            return Ok(new { total = roles.Count, roles });
        }

        [HttpPost("roles")]
        public async Task<IActionResult> CreateRole([FromBody] RoleCatalog role)
        {
            try
            {
                var result = await _catalogService.CreateRoleAsync(role, CurrentUser);
                return Ok(new { success = true, id = result.Id, roleCode = result.RoleCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        [HttpPut("roles/{id}")]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] RoleCatalog role)
        {
            try
            {
                var result = await _catalogService.UpdateRoleAsync(id, role, CurrentUser);
                return Ok(new { success = true, id = result.Id });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = "The requested resource was not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        [HttpDelete("roles/{id}")]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            var success = await _catalogService.DeleteRoleAsync(id, CurrentUser);
            return success ? Ok(new { success = true }) : NotFound(new { error = "Role not found" });
        }

        #endregion

        #region Titles

        [HttpGet("roles/{roleId}/titles")]
        public async Task<IActionResult> GetTitlesByRole(Guid roleId)
        {
            var titles = await _catalogService.GetTitlesByRoleAsync(roleId);
            return Ok(new { total = titles.Count, titles });
        }

        [HttpPost("titles")]
        public async Task<IActionResult> CreateTitle([FromBody] TitleCatalog title)
        {
            try
            {
                var result = await _catalogService.CreateTitleAsync(title, CurrentUser);
                return Ok(new { success = true, id = result.Id, titleCode = result.TitleCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating title");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        [HttpPut("titles/{id}")]
        public async Task<IActionResult> UpdateTitle(Guid id, [FromBody] TitleCatalog title)
        {
            try
            {
                var result = await _catalogService.UpdateTitleAsync(id, title, CurrentUser);
                return Ok(new { success = true, id = result.Id });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = "The requested resource was not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating title");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        [HttpDelete("titles/{id}")]
        public async Task<IActionResult> DeleteTitle(Guid id)
        {
            var success = await _catalogService.DeleteTitleAsync(id, CurrentUser);
            return success ? Ok(new { success = true }) : NotFound(new { error = "Title not found" });
        }

        #endregion

        #region Baselines

        [HttpGet("baselines")]
        public async Task<IActionResult> GetBaselines()
        {
            var baselines = await _catalogService.GetAllBaselinesAsync();
            return Ok(new { total = baselines.Count, baselines });
        }

        [HttpPost("baselines")]
        public async Task<IActionResult> CreateBaseline([FromBody] BaselineCatalog baseline)
        {
            try
            {
                var result = await _catalogService.CreateBaselineAsync(baseline, CurrentUser);
                return Ok(new { success = true, id = result.Id, baselineCode = result.BaselineCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating baseline");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        [HttpPut("baselines/{id}")]
        public async Task<IActionResult> UpdateBaseline(Guid id, [FromBody] BaselineCatalog baseline)
        {
            try
            {
                var result = await _catalogService.UpdateBaselineAsync(id, baseline, CurrentUser);
                return Ok(new { success = true, id = result.Id });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = "The requested resource was not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating baseline");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        [HttpDelete("baselines/{id}")]
        public async Task<IActionResult> DeleteBaseline(Guid id)
        {
            var success = await _catalogService.DeleteBaselineAsync(id, CurrentUser);
            return success ? Ok(new { success = true }) : NotFound(new { error = "Baseline not found" });
        }

        #endregion

        #region Packages

        [HttpGet("packages")]
        public async Task<IActionResult> GetPackages()
        {
            var packages = await _catalogService.GetAllPackagesAsync();
            return Ok(new { total = packages.Count, packages });
        }

        [HttpPost("packages")]
        public async Task<IActionResult> CreatePackage([FromBody] PackageCatalog package)
        {
            try
            {
                var result = await _catalogService.CreatePackageAsync(package, CurrentUser);
                return Ok(new { success = true, id = result.Id, packageCode = result.PackageCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating package");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        [HttpPut("packages/{id}")]
        public async Task<IActionResult> UpdatePackage(Guid id, [FromBody] PackageCatalog package)
        {
            try
            {
                var result = await _catalogService.UpdatePackageAsync(id, package, CurrentUser);
                return Ok(new { success = true, id = result.Id });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = "The requested resource was not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating package");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        [HttpDelete("packages/{id}")]
        public async Task<IActionResult> DeletePackage(Guid id)
        {
            var success = await _catalogService.DeletePackageAsync(id, CurrentUser);
            return success ? Ok(new { success = true }) : NotFound(new { error = "Package not found" });
        }

        #endregion

        #region Templates

        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates()
        {
            var templates = await _catalogService.GetAllTemplatesAsync();
            return Ok(new { total = templates.Count, templates });
        }

        [HttpPost("templates")]
        public async Task<IActionResult> CreateTemplate([FromBody] TemplateCatalog template)
        {
            try
            {
                var result = await _catalogService.CreateTemplateAsync(template, CurrentUser);
                return Ok(new { success = true, id = result.Id, templateCode = result.TemplateCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating template");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        [HttpPut("templates/{id}")]
        public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] TemplateCatalog template)
        {
            try
            {
                var result = await _catalogService.UpdateTemplateAsync(id, template, CurrentUser);
                return Ok(new { success = true, id = result.Id });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = "The requested resource was not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating template");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        [HttpDelete("templates/{id}")]
        public async Task<IActionResult> DeleteTemplate(Guid id)
        {
            var success = await _catalogService.DeleteTemplateAsync(id, CurrentUser);
            return success ? Ok(new { success = true }) : NotFound(new { error = "Template not found" });
        }

        #endregion

        #region Evidence Types

        [HttpGet("evidence-types")]
        public async Task<IActionResult> GetEvidenceTypes()
        {
            var types = await _catalogService.GetAllEvidenceTypesAsync();
            return Ok(new { total = types.Count, evidenceTypes = types });
        }

        [HttpPost("evidence-types")]
        public async Task<IActionResult> CreateEvidenceType([FromBody] EvidenceTypeCatalog evidenceType)
        {
            try
            {
                var result = await _catalogService.CreateEvidenceTypeAsync(evidenceType, CurrentUser);
                return Ok(new { success = true, id = result.Id, code = result.EvidenceTypeCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating evidence type");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        [HttpPut("evidence-types/{id}")]
        public async Task<IActionResult> UpdateEvidenceType(Guid id, [FromBody] EvidenceTypeCatalog evidenceType)
        {
            try
            {
                var result = await _catalogService.UpdateEvidenceTypeAsync(id, evidenceType, CurrentUser);
                return Ok(new { success = true, id = result.Id });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = "The requested resource was not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating evidence type");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        [HttpDelete("evidence-types/{id}")]
        public async Task<IActionResult> DeleteEvidenceType(Guid id)
        {
            var success = await _catalogService.DeleteEvidenceTypeAsync(id, CurrentUser);
            return success ? Ok(new { success = true }) : NotFound(new { error = "Evidence type not found" });
        }

        #endregion

        #region Rulesets

        [HttpGet("rulesets")]
        public async Task<IActionResult> GetRulesets()
        {
            var rulesets = await _catalogService.GetAllRulesetsAsync();
            return Ok(new { total = rulesets.Count, rulesets });
        }

        [HttpGet("rulesets/{id}")]
        public async Task<IActionResult> GetRuleset(Guid id)
        {
            try
            {
                var ruleset = await _catalogService.GetRulesetAsync(id);
                return Ok(ruleset);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = "The requested resource was not found." });
            }
        }

        [HttpPost("rulesets")]
        public async Task<IActionResult> CreateRuleset([FromBody] Ruleset ruleset)
        {
            try
            {
                var result = await _catalogService.CreateRulesetAsync(ruleset, CurrentUser);
                return Ok(new { success = true, id = result.Id, code = result.RulesetCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ruleset");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        [HttpPut("rulesets/{id}")]
        public async Task<IActionResult> UpdateRuleset(Guid id, [FromBody] Ruleset ruleset)
        {
            try
            {
                var result = await _catalogService.UpdateRulesetAsync(id, ruleset, CurrentUser);
                return Ok(new { success = true, id = result.Id });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = "The requested resource was not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ruleset");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        [HttpPost("rulesets/{id}/activate")]
        public async Task<IActionResult> ActivateRuleset(Guid id)
        {
            var success = await _catalogService.ActivateRulesetAsync(id, CurrentUser);
            return success ? Ok(new { success = true }) : NotFound(new { error = "Ruleset not found" });
        }

        [HttpPost("rulesets/{id}/retire")]
        public async Task<IActionResult> RetireRuleset(Guid id)
        {
            var success = await _catalogService.RetireRulesetAsync(id, CurrentUser);
            return success ? Ok(new { success = true }) : NotFound(new { error = "Ruleset not found" });
        }

        [HttpPost("rulesets/{id}/clone")]
        public async Task<IActionResult> CloneRuleset(Guid id, [FromBody] CloneRulesetRequest request)
        {
            try
            {
                var result = await _catalogService.CloneRulesetAsync(id, request.NewName, CurrentUser);
                return Ok(new { success = true, id = result.Id, code = result.RulesetCode });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = "The requested resource was not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cloning ruleset");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        #endregion

        #region Rules

        [HttpGet("rulesets/{rulesetId}/rules")]
        public async Task<IActionResult> GetRules(Guid rulesetId)
        {
            var rules = await _catalogService.GetRulesByRulesetAsync(rulesetId);
            return Ok(new { total = rules.Count, rules });
        }

        [HttpPost("rules")]
        public async Task<IActionResult> CreateRule([FromBody] Rule rule)
        {
            try
            {
                var result = await _catalogService.CreateRuleAsync(rule, CurrentUser);
                return Ok(new { success = true, id = result.Id, code = result.RuleCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rule");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        [HttpPut("rules/{id}")]
        public async Task<IActionResult> UpdateRule(Guid id, [FromBody] Rule rule)
        {
            try
            {
                var result = await _catalogService.UpdateRuleAsync(id, rule, CurrentUser);
                return Ok(new { success = true, id = result.Id });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = "The requested resource was not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rule");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        [HttpDelete("rules/{id}")]
        public async Task<IActionResult> DeleteRule(Guid id)
        {
            var success = await _catalogService.DeleteRuleAsync(id, CurrentUser);
            return success ? Ok(new { success = true }) : NotFound(new { error = "Rule not found" });
        }

        [HttpPost("rulesets/{rulesetId}/rules/reorder")]
        public async Task<IActionResult> ReorderRules(Guid rulesetId, [FromBody] ReorderRulesRequest request)
        {
            var success = await _catalogService.ReorderRulesAsync(rulesetId, request.RuleIds, CurrentUser);
            return success ? Ok(new { success = true }) : BadRequest(new { error = "Failed to reorder rules" });
        }

        #endregion

        #region Change Log

        [HttpGet("changelog")]
        public async Task<IActionResult> GetChangeLog(
            [FromQuery] string entityType,
            [FromQuery] Guid? entityId = null,
            [FromQuery] int limit = 100)
        {
            var log = await _catalogService.GetChangeLogAsync(entityType, entityId, limit);
            return Ok(new { total = log.Count, changes = log });
        }

        #endregion
    }

    #region Request DTOs

    public class PublishVersionRequest
    {
        public string NewVersion { get; set; } = string.Empty;
    }

    public class CloneRulesetRequest
    {
        public string NewName { get; set; } = string.Empty;
    }

    public class ReorderRulesRequest
    {
        public List<Guid> RuleIds { get; set; } = new();
    }

    #endregion
}
