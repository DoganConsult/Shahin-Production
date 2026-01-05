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
    public class PolicyController : Controller
    {
        private readonly IPolicyService _policyService;
        private readonly ILogger<PolicyController> _logger;

        public PolicyController(IPolicyService policyService, ILogger<PolicyController> logger)
        {
            _policyService = policyService;
            _logger = logger;
        }

        // GET: Policy
        public async Task<IActionResult> Index()
        {
            var policies = await _policyService.GetAllAsync();
            return View(policies);
        }

        // GET: Policy/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var policy = await _policyService.GetByIdAsync(id);
            if (policy == null)
            {
                return NotFound();
            }
            return View(policy);
        }

        // GET: Policy/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Policy/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePolicyDto createPolicyDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var policy = await _policyService.CreateAsync(createPolicyDto);
                    TempData["Success"] = "Policy created successfully";
                    return RedirectToAction(nameof(Details), new { id = policy.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating policy");
                    ModelState.AddModelError("", "Error creating policy. Please try again.");
                }
            }
            return View(createPolicyDto);
        }

        // GET: Policy/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var policy = await _policyService.GetByIdAsync(id);
            if (policy == null)
            {
                return NotFound();
            }

            var updateDto = new UpdatePolicyDto
            {
                PolicyNumber = policy.PolicyNumber,
                Title = policy.Title,
                Description = policy.Description,
                Category = policy.Category,
                Type = policy.Type,
                Status = policy.Status,
                EffectiveDate = policy.EffectiveDate,
                ExpirationDate = policy.ExpirationDate,
                ReviewDate = policy.ReviewDate,
                Owner = policy.Owner,
                Approver = policy.Approver,
                Scope = policy.Scope,
                Requirements = policy.Requirements,
                Procedures = policy.Procedures,
                References = policy.References
            };

            return View(updateDto);
        }

        // POST: Policy/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdatePolicyDto updatePolicyDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var policy = await _policyService.UpdateAsync(id, updatePolicyDto);
                    if (policy == null)
                    {
                        return NotFound();
                    }
                    TempData["Success"] = "Policy updated successfully";
                    return RedirectToAction(nameof(Details), new { id = policy.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating policy with ID {PolicyId}", id);
                    ModelState.AddModelError("", "Error updating policy. Please try again.");
                }
            }
            return View(updatePolicyDto);
        }

        // GET: Policy/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var policy = await _policyService.GetByIdAsync(id);
            if (policy == null)
            {
                return NotFound();
            }
            return View(policy);
        }

        // POST: Policy/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _policyService.DeleteAsync(id);
                TempData["Success"] = "Policy deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting policy with ID {PolicyId}", id);
                TempData["Error"] = "Error deleting policy. Please try again.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // GET: Policy/Statistics
        public async Task<IActionResult> Statistics()
        {
            try
            {
                var statistics = await _policyService.GetStatisticsAsync();
                return View(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policy statistics");
                TempData["Error"] = "Error loading statistics. Please try again.";
                return View(new PolicyStatisticsDto());
            }
        }

        // GET: Policy/ByCategory/5
        public async Task<IActionResult> ByCategory(string category)
        {
            try
            {
                var policies = await _policyService.GetByCategoryAsync(category);
                ViewBag.Category = category;
                return View(policies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policies by category {Category}", category);
                TempData["Error"] = "Error loading policies. Please try again.";
                return View(new List<PolicyDto>());
            }
        }

        // GET: Policy/ByStatus/5
        public async Task<IActionResult> ByStatus(string status)
        {
            try
            {
                var policies = await _policyService.GetByStatusAsync(status);
                ViewBag.Status = status;
                return View(policies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policies by status {Status}", status);
                TempData["Error"] = "Error loading policies. Please try again.";
                return View(new List<PolicyDto>());
            }
        }

        // GET: Policy/Expiring
        public async Task<IActionResult> Expiring()
        {
            try
            {
                var policies = await _policyService.GetExpiringPoliciesAsync(30);
                return View(policies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expiring policies");
                TempData["Error"] = "Error loading expiring policies. Please try again.";
                return View(new List<PolicyDto>());
            }
        }

        // GET: Policy/Violations/5
        public async Task<IActionResult> Violations(Guid id)
        {
            try
            {
                var violations = await _policyService.GetPolicyViolationsAsync(id);
                ViewBag.PolicyId = id;
                return View(violations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting violations for policy ID {PolicyId}", id);
                TempData["Error"] = "Error loading violations. Please try again.";
                return View(new List<PolicyViolationDto>());
            }
        }
    }
}
