using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Services.Interfaces;
using GrcMvc.Models.DTOs;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace GrcMvc.Controllers
{
    [Authorize]
    public class EvidenceController : Controller
    {
        private readonly IEvidenceService _evidenceService;
        private readonly ILogger<EvidenceController> _logger;

        public EvidenceController(IEvidenceService evidenceService, ILogger<EvidenceController> logger)
        {
            _evidenceService = evidenceService;
            _logger = logger;
        }

        // GET: Evidence
        public async Task<IActionResult> Index()
        {
            var evidences = await _evidenceService.GetAllAsync();
            return View(evidences);
        }

        // GET: Evidence/Details/5
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
        public async Task<IActionResult> Create(CreateEvidenceDto createEvidenceDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var evidence = await _evidenceService.CreateAsync(createEvidenceDto);
                    TempData["Success"] = "Evidence created successfully";
                    return RedirectToAction(nameof(Details), new { id = evidence.Id });
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
        public async Task<IActionResult> Edit(Guid id, UpdateEvidenceDto updateEvidenceDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var evidence = await _evidenceService.UpdateAsync(id, updateEvidenceDto);
                    if (evidence == null)
                    {
                        return NotFound();
                    }
                    TempData["Success"] = "Evidence updated successfully";
                    return RedirectToAction(nameof(Details), new { id = evidence.Id });
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
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _evidenceService.DeleteAsync(id);
                TempData["Success"] = "Evidence deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting evidence with ID {EvidenceId}", id);
                TempData["Error"] = "Error deleting evidence. Please try again.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // GET: Evidence/Statistics
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
