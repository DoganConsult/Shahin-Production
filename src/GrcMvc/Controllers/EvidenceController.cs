using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Services.Interfaces;
using GrcMvc.Models.DTOs;
using GrcMvc.Application.Permissions;
using GrcMvc.Application.Policy;
using GrcMvc.Authorization;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace GrcMvc.Controllers
{
    [Authorize]
    [RequireTenant]
    public class EvidenceController : Controller
    {
        private readonly IEvidenceService _evidenceService;
        private readonly ILogger<EvidenceController> _logger;
        private readonly IWorkspaceContextService? _workspaceContext;
        private readonly PolicyEnforcementHelper _policyHelper;

        public EvidenceController(
            IEvidenceService evidenceService, 
            ILogger<EvidenceController> logger,
            PolicyEnforcementHelper policyHelper,
            IWorkspaceContextService? workspaceContext = null)
        {
            _evidenceService = evidenceService;
            _logger = logger;
            _policyHelper = policyHelper;
            _workspaceContext = workspaceContext;
        }

        // GET: Evidence
        [Authorize(GrcPermissions.Evidence.View)]
        public async Task<IActionResult> Index()
        {
            var evidences = await _evidenceService.GetAllAsync();
            return View(evidences);
        }

        // GET: Evidence/Details/5
        [Authorize(GrcPermissions.Evidence.View)]
        public async Task<IActionResult> Details(Guid id)
        {
            var evidence = await _evidenceService.GetByIdAsync(id);
            if (evidence == null)
            {
                return NotFound();
            }
            return View(evidence);
        }

        // GET: Evidence/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Evidence/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(GrcPermissions.Evidence.Upload)]
        public async Task<IActionResult> Create(CreateEvidenceDto createEvidenceDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // POLICY ENFORCEMENT: Validate governance metadata before creation
                    await _policyHelper.EnforceCreateAsync(
                        "Evidence",
                        createEvidenceDto,
                        dataClassification: createEvidenceDto.DataClassification,
                        owner: createEvidenceDto.Owner
                    );

                    var evidence = await _evidenceService.CreateAsync(createEvidenceDto);
                    TempData["Success"] = "Evidence created successfully";
                    return RedirectToAction(nameof(Details), new { id = evidence.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation creating evidence");
                    ModelState.AddModelError("", $"Policy Violation: {pex.Message}");
                    if (!string.IsNullOrEmpty(pex.RemediationHint))
                    {
                        ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating evidence");
                    ModelState.AddModelError("", "Error creating evidence. Please try again.");
                }
            }
            return View(createEvidenceDto);
        }

        // GET: Evidence/Edit/5
        [Authorize(GrcPermissions.Evidence.Update)]
        public async Task<IActionResult> Edit(Guid id)
        {
            var evidence = await _evidenceService.GetByIdAsync(id);
            if (evidence == null)
            {
                return NotFound();
            }

            var updateDto = new UpdateEvidenceDto
            {
                Name = evidence.Name,
                Description = evidence.Description,
                EvidenceType = evidence.EvidenceType,
                DataClassification = evidence.DataClassification,
                Source = evidence.Source,
                CollectionDate = evidence.CollectionDate,
                ExpirationDate = evidence.ExpirationDate,
                Status = evidence.Status,
                Owner = evidence.Owner,
                Location = evidence.Location,
                Tags = evidence.Tags,
                Notes = evidence.Notes
            };

            return View(updateDto);
        }

        // POST: Evidence/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(GrcPermissions.Evidence.Update)]
        public async Task<IActionResult> Edit(Guid id, UpdateEvidenceDto updateEvidenceDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // POLICY ENFORCEMENT: Validate governance metadata before update
                    await _policyHelper.EnforceUpdateAsync(
                        "Evidence",
                        updateEvidenceDto,
                        dataClassification: updateEvidenceDto.DataClassification,
                        owner: updateEvidenceDto.Owner
                    );

                    var evidence = await _evidenceService.UpdateAsync(id, updateEvidenceDto);
                    if (evidence == null)
                    {
                        return NotFound();
                    }
                    TempData["Success"] = "Evidence updated successfully";
                    return RedirectToAction(nameof(Details), new { id = evidence.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation updating evidence {EvidenceId}", id);
                    ModelState.AddModelError("", $"Policy Violation: {pex.Message}");
                    if (!string.IsNullOrEmpty(pex.RemediationHint))
                    {
                        ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating evidence with ID {EvidenceId}", id);
                    ModelState.AddModelError("", "Error updating evidence. Please try again.");
                }
            }
            return View(updateEvidenceDto);
        }

        // GET: Evidence/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var evidence = await _evidenceService.GetByIdAsync(id);
            if (evidence == null)
            {
                return NotFound();
            }
            return View(evidence);
        }

        // POST: Evidence/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(GrcPermissions.Evidence.Delete)]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                // POLICY ENFORCEMENT: Check if deletion is allowed
                var evidence = await _evidenceService.GetByIdAsync(id);
                if (evidence != null)
                {
                    await _policyHelper.EnforceAsync(
                        "delete",
                        "Evidence",
                        evidence,
                        dataClassification: evidence.DataClassification,
                        owner: evidence.Owner
                    );
                }

                await _evidenceService.DeleteAsync(id);
                TempData["Success"] = "Evidence deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (PolicyViolationException pex)
            {
                _logger.LogWarning(pex, "Policy violation deleting evidence {EvidenceId}", id);
                TempData["Error"] = $"Policy Violation: {pex.Message}. {pex.RemediationHint}";
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting evidence with ID {EvidenceId}", id);
                TempData["Error"] = "Error deleting evidence. Please try again.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // GET: Evidence/Statistics
        [Authorize(GrcPermissions.Evidence.View)]
        public async Task<IActionResult> Statistics()
        {
            try
            {
                var statistics = await _evidenceService.GetStatisticsAsync();
                return View(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting evidence statistics");
                TempData["Error"] = "Error loading statistics. Please try again.";
                return View(new EvidenceStatisticsDto());
            }
        }

        // GET: Evidence/ByType/5
        public async Task<IActionResult> ByType(string type)
        {
            try
            {
                var evidences = await _evidenceService.GetByTypeAsync(type);
                ViewBag.Type = type;
                return View(evidences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting evidences by type {Type}", type);
                TempData["Error"] = "Error loading evidences. Please try again.";
                return View(new List<EvidenceDto>());
            }
        }

        // GET: Evidence/ByClassification/5
        [Authorize(GrcPermissions.Evidence.View)]
        public async Task<IActionResult> ByClassification(string classification)
        {
            try
            {
                var evidences = await _evidenceService.GetByClassificationAsync(classification);
                ViewBag.Classification = classification;
                return View(evidences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting evidences by classification {Classification}", classification);
                TempData["Error"] = "Error loading evidences. Please try again.";
                return View(new List<EvidenceDto>());
            }
        }

        // GET: Evidence/Expiring
        public async Task<IActionResult> Expiring()
        {
            try
            {
                var evidences = await _evidenceService.GetExpiringEvidencesAsync(30);
                return View(evidences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expiring evidences");
                TempData["Error"] = "Error loading expiring evidences. Please try again.";
                return View(new List<EvidenceDto>());
            }
        }

        // GET: Evidence/ByAudit/5
        [Authorize(GrcPermissions.Evidence.View)]
        public async Task<IActionResult> ByAudit(Guid auditId)
        {
            try
            {
                var evidences = await _evidenceService.GetByAuditIdAsync(auditId);
                ViewBag.AuditId = auditId;
                return View(evidences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting evidences for audit ID {AuditId}", auditId);
                TempData["Error"] = "Error loading evidences. Please try again.";
                return View(new List<EvidenceDto>());
            }
        }
    }
}
