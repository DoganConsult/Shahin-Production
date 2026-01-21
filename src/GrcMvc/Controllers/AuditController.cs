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

namespace GrcMvc.Controllers
{
    [Authorize]
    [RequireTenant]
    public class AuditController : Controller
    {
        private readonly IAuditService _auditService;
        private readonly ILogger<AuditController> _logger;
        private readonly IWorkspaceContextService? _workspaceContext;
        private readonly PolicyEnforcementHelper _policyHelper;

        public AuditController(IAuditService auditService, ILogger<AuditController> logger, PolicyEnforcementHelper policyHelper, IWorkspaceContextService? workspaceContext = null)
        {
            _auditService = auditService;
            _logger = logger;
            _policyHelper = policyHelper;
            _workspaceContext = workspaceContext;
        }

        [Authorize(GrcPermissions.Audits.View)]
        public async Task<IActionResult> Index()
        {
            var audits = await _auditService.GetAllAsync();
            return View(audits);
        }

        [Authorize(GrcPermissions.Audits.View)]
        public async Task<IActionResult> Details(Guid id)
        {
            var audit = await _auditService.GetByIdAsync(id);
            if (audit == null) return NotFound();
            return View(audit);
        }

        [Authorize(GrcPermissions.Audits.Manage)]
        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Audits.Manage)]
        public async Task<IActionResult> Create(CreateAuditDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _policyHelper.EnforceCreateAsync("Audit", dto, dataClassification: dto.DataClassification, owner: dto.Owner);
                    var audit = await _auditService.CreateAsync(dto);
                    TempData["Success"] = "Audit created successfully";
                    return RedirectToAction(nameof(Details), new { id = audit.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation creating audit");
                    ModelState.AddModelError("", "A policy violation occurred. Please review the requirements.");
                    if (!string.IsNullOrEmpty(pex.RemediationHint)) ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating audit");
                    ModelState.AddModelError("", "Error creating audit. Please try again.");
                }
            }
            return View(dto);
        }

        [Authorize(GrcPermissions.Audits.Manage)]
        public async Task<IActionResult> Edit(Guid id)
        {
            var audit = await _auditService.GetByIdAsync(id);
            if (audit == null) return NotFound();

            var updateDto = new UpdateAuditDto
            {
                Id = audit.Id,
                AuditNumber = audit.AuditNumber,
                Title = audit.Title,
                Description = audit.Description,
                AuditType = audit.AuditType,
                Status = audit.Status,
                StartDate = audit.StartDate,
                EndDate = audit.EndDate,
                ScheduledDate = audit.ScheduledDate,
                AuditorId = audit.AuditorId,
                Scope = audit.Scope,
                Objectives = audit.Objectives,
                Methodology = audit.Methodology,
                Findings = audit.Findings,
                Recommendations = audit.Recommendations,
                Owner = audit.Owner,
                DataClassification = audit.DataClassification
            };

            return View(updateDto);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Audits.Manage)]
        public async Task<IActionResult> Edit(Guid id, UpdateAuditDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _policyHelper.EnforceUpdateAsync("Audit", dto, dataClassification: dto.DataClassification, owner: dto.Owner);
                    var audit = await _auditService.UpdateAsync(id, dto);
                    if (audit == null) return NotFound();
                    TempData["Success"] = "Audit updated successfully";
                    return RedirectToAction(nameof(Details), new { id = audit.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation updating audit {AuditId}", id);
                    ModelState.AddModelError("", "A policy violation occurred. Please review the requirements.");
                    if (!string.IsNullOrEmpty(pex.RemediationHint)) ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating audit {AuditId}", id);
                    ModelState.AddModelError("", "Error updating audit. Please try again.");
                }
            }
            return View(dto);
        }

        [Authorize(GrcPermissions.Audits.Manage)]
        public async Task<IActionResult> UpdateAudit(Guid id, [FromBody] UpdateAuditDto updateAuditDto)
        {
            var auditDto = await _auditService.UpdateAsync(id, updateAuditDto);
            if (auditDto == null) return NotFound();
            return Ok(auditDto);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Audits.Close)]
        public async Task<IActionResult> Close(Guid id)
        {
            try
            {
                var audit = await _auditService.GetByIdAsync(id);
                if (audit == null) return NotFound();
                await _policyHelper.EnforceAsync("close", "Audit", audit, dataClassification: audit.DataClassification, owner: audit.Owner);
                await _auditService.CloseAsync(id);
                TempData["Success"] = "Audit closed successfully";
            }
            catch (PolicyViolationException pex)
            {
                _logger.LogWarning(pex, "Policy violation closing audit {AuditId}", id);
                TempData["Error"] = "A policy violation occurred. Please review the requirements.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing audit {AuditId}", id);
                TempData["Error"] = "Error closing audit. Please try again.";
            }
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
