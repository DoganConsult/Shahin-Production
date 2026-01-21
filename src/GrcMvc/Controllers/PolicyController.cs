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
    public class PolicyController : Controller
    {
        private readonly IPolicyService _policyService;
        private readonly ILogger<PolicyController> _logger;
        private readonly IWorkspaceContextService? _workspaceContext;
        private readonly PolicyEnforcementHelper _policyHelper;

        public PolicyController(IPolicyService policyService, ILogger<PolicyController> logger, PolicyEnforcementHelper policyHelper, IWorkspaceContextService? workspaceContext = null)
        {
            _policyService = policyService;
            _logger = logger;
            _policyHelper = policyHelper;
            _workspaceContext = workspaceContext;
        }

        [Authorize(GrcPermissions.Policies.View)]
        public async Task<IActionResult> Index()
        {
            var policies = await _policyService.GetAllAsync();
            return View(policies);
        }

        [Authorize(GrcPermissions.Policies.View)]
        public async Task<IActionResult> Details(Guid id)
        {
            var policy = await _policyService.GetByIdAsync(id);
            if (policy == null) return NotFound();
            return View(policy);
        }

        [Authorize(GrcPermissions.Policies.Manage)]
        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Policies.Manage)]
        public async Task<IActionResult> Create(CreatePolicyDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _policyHelper.EnforceCreateAsync("PolicyDocument", dto, dataClassification: dto.DataClassification, owner: dto.Owner);
                    var policy = await _policyService.CreateAsync(dto);
                    TempData["Success"] = "Policy created successfully";
                    return RedirectToAction(nameof(Details), new { id = policy.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation creating policy");
                    ModelState.AddModelError("", "A policy violation occurred. Please review the requirements.");
                    if (!string.IsNullOrEmpty(pex.RemediationHint)) ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating policy");
                    ModelState.AddModelError("", "Error creating policy. Please try again.");
                }
            }
            return View(dto);
        }

        [Authorize(GrcPermissions.Policies.Manage)]
        public async Task<IActionResult> Edit(Guid id)
        {
            var policy = await _policyService.GetByIdAsync(id);
            if (policy == null) return NotFound();

            var updateDto = new UpdatePolicyDto
            {
                Id = policy.Id,
                PolicyNumber = policy.PolicyNumber,
                Title = policy.Title,
                Description = policy.Description,
                Category = policy.Category,
                Version = policy.Version,
                EffectiveDate = policy.EffectiveDate,
                ExpirationDate = policy.ExpirationDate,
                Status = policy.Status,
                Owner = policy.Owner,
                DataClassification = policy.DataClassification,
                Content = policy.Content,
                ApprovalRequired = policy.ApprovalRequired,
                ReviewFrequency = policy.ReviewFrequency
            };

            return View(updateDto);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Policies.Manage)]
        public async Task<IActionResult> Edit(Guid id, UpdatePolicyDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _policyHelper.EnforceUpdateAsync("PolicyDocument", dto, dataClassification: dto.DataClassification, owner: dto.Owner);
                    var policy = await _policyService.UpdateAsync(id, dto);
                    if (policy == null) return NotFound();
                    TempData["Success"] = "Policy updated successfully";
                    return RedirectToAction(nameof(Details), new { id = policy.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation updating policy {PolicyId}", id);
                    ModelState.AddModelError("", "A policy violation occurred. Please review the requirements.");
                    if (!string.IsNullOrEmpty(pex.RemediationHint)) ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating policy {PolicyId}", id);
                    ModelState.AddModelError("", "Error updating policy. Please try again.");
                }
            }
            return View(dto);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Policies.Approve)]
        public async Task<IActionResult> Approve(Guid id)
        {
            try
            {
                var policy = await _policyService.GetByIdAsync(id);
                if (policy == null) return NotFound();
                await _policyHelper.EnforceApproveAsync("PolicyDocument", policy, dataClassification: policy.DataClassification, owner: policy.Owner);
                await _policyService.ApproveAsync(id);
                TempData["Success"] = "Policy approved successfully";
            }
            catch (PolicyViolationException pex)
            {
                _logger.LogWarning(pex, "Policy violation approving policy {PolicyId}", id);
                TempData["Error"] = "A policy violation occurred. Please review the requirements.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving policy {PolicyId}", id);
                TempData["Error"] = "Error approving policy. Please try again.";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Policies.Publish)]
        public async Task<IActionResult> Publish(Guid id)
        {
            try
            {
                var policy = await _policyService.GetByIdAsync(id);
                if (policy == null) return NotFound();
                await _policyHelper.EnforcePublishAsync("PolicyDocument", policy, dataClassification: policy.DataClassification, owner: policy.Owner);
                await _policyService.PublishAsync(id);
                TempData["Success"] = "Policy published successfully";
            }
            catch (PolicyViolationException pex)
            {
                _logger.LogWarning(pex, "Policy violation publishing policy {PolicyId}", id);
                TempData["Error"] = "A policy violation occurred. Please review the requirements.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing policy {PolicyId}", id);
                TempData["Error"] = "Error publishing policy. Please try again.";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Policies.Manage)]
        public async Task<IActionResult> UpdatePolicy(Guid id, [FromBody] UpdatePolicyDto updatePolicyDto)
        {
            var policyDto = await _policyService.UpdateAsync(id, updatePolicyDto);
            if (policyDto == null) return NotFound();
            return Ok(policyDto);
        }
    }
}
