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
    public class AuditController : Controller
    {
        private readonly IAuditService _auditService;
        private readonly ILogger<AuditController> _logger;

        public AuditController(IAuditService auditService, ILogger<AuditController> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        // GET: Audit
        public async Task<IActionResult> Index()
        {
            var audits = await _auditService.GetAllAsync();
            return View(audits);
        }

        // GET: Audit/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var audit = await _auditService.GetByIdAsync(id);
            if (audit == null)
            {
                return NotFound();
            }
            return View(audit);
        }

        // GET: Audit/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Audit/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAuditDto createAuditDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var audit = await _auditService.CreateAsync(createAuditDto);
                    TempData["Success"] = "Audit created successfully";
                    return RedirectToAction(nameof(Details), new { id = audit.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating audit");
                    ModelState.AddModelError("", "Error creating audit. Please try again.");
                }
            }
            return View(createAuditDto);
        }

        // GET: Audit/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var audit = await _auditService.GetByIdAsync(id);
            if (audit == null)
            {
                return NotFound();
            }

            var updateDto = new UpdateAuditDto
            {
                AuditNumber = audit.AuditNumber,
                Type = audit.Type,
                Name = audit.Name,
                Description = audit.Description,
                StartDate = audit.StartDate,
                EndDate = audit.EndDate,
                AssignedTo = audit.AssignedTo,
                Status = audit.Status,
                Scope = audit.Scope,
                Objectives = audit.Objectives,
                Criteria = audit.Criteria,
                Methodology = audit.Methodology,
                ReportSummary = audit.ReportSummary
            };

            return View(updateDto);
        }

        // POST: Audit/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateAuditDto updateAuditDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var audit = await _auditService.UpdateAsync(id, updateAuditDto);
                    if (audit == null)
                    {
                        return NotFound();
                    }
                    TempData["Success"] = "Audit updated successfully";
                    return RedirectToAction(nameof(Details), new { id = audit.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating audit with ID {AuditId}", id);
                    ModelState.AddModelError("", "Error updating audit. Please try again.");
                }
            }
            return View(updateAuditDto);
        }

        // GET: Audit/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var audit = await _auditService.GetByIdAsync(id);
            if (audit == null)
            {
                return NotFound();
            }
            return View(audit);
        }

        // POST: Audit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _auditService.DeleteAsync(id);
                TempData["Success"] = "Audit deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting audit with ID {AuditId}", id);
                TempData["Error"] = "Error deleting audit. Please try again.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // GET: Audit/Statistics
        public async Task<IActionResult> Statistics()
        {
            try
            {
                var statistics = await _auditService.GetStatisticsAsync();
                return View(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit statistics");
                TempData["Error"] = "Error loading statistics. Please try again.";
                return View(new AuditStatisticsDto());
            }
        }

        // GET: Audit/ByType/5
        public async Task<IActionResult> ByType(string type)
        {
            try
            {
                var audits = await _auditService.GetByTypeAsync(type);
                ViewBag.Type = type;
                return View(audits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audits by type {Type}", type);
                TempData["Error"] = "Error loading audits. Please try again.";
                return View(new List<AuditDto>());
            }
        }

        // GET: Audit/Upcoming
        public async Task<IActionResult> Upcoming()
        {
            try
            {
                var audits = await _auditService.GetUpcomingAuditsAsync(30);
                return View(audits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming audits");
                TempData["Error"] = "Error loading upcoming audits. Please try again.";
                return View(new List<AuditDto>());
            }
        }

        // GET: Audit/Findings/5
        public async Task<IActionResult> Findings(Guid id)
        {
            try
            {
                var findings = await _auditService.GetAuditFindingsAsync(id);
                ViewBag.AuditId = id;
                return View(findings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting findings for audit ID {AuditId}", id);
                TempData["Error"] = "Error loading findings. Please try again.";
                return View(new List<AuditFindingDto>());
            }
        }
    }
}
