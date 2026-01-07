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
    public class WorkflowController : Controller
    {
        private readonly IWorkflowService _workflowService;
        private readonly ILogger<WorkflowController> _logger;
        private readonly PolicyEnforcementHelper _policyHelper;

        public WorkflowController(
            IWorkflowService workflowService, 
            ILogger<WorkflowController> logger,
            PolicyEnforcementHelper policyHelper)
        {
            _workflowService = workflowService;
            _logger = logger;
            _policyHelper = policyHelper;
        }

        // GET: Workflow
        [Authorize(GrcPermissions.Workflow.View)]
        public async Task<IActionResult> Index()
        {
            var workflows = await _workflowService.GetAllAsync();
            return View(workflows);
        }

        // GET: Workflow/Details/5
        [Authorize(GrcPermissions.Workflow.View)]
        public async Task<IActionResult> Details(Guid id)
        {
            var workflow = await _workflowService.GetByIdAsync(id);
            if (workflow == null)
            {
                return NotFound();
            }
            return View(workflow);
        }

        // GET: Workflow/Create
        [Authorize(GrcPermissions.Workflow.Manage)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Workflow/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(GrcPermissions.Workflow.Manage)]
        public async Task<IActionResult> Create(CreateWorkflowDto createWorkflowDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // POLICY ENFORCEMENT: Validate governance metadata before creation
                    await _policyHelper.EnforceCreateAsync(
                        "Workflow",
                        createWorkflowDto,
                        dataClassification: createWorkflowDto.DataClassification,
                        owner: createWorkflowDto.Owner
                    );

                    var workflow = await _workflowService.CreateAsync(createWorkflowDto);
                    TempData["Success"] = "Workflow created successfully";
                    return RedirectToAction(nameof(Details), new { id = workflow.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation creating workflow");
                    ModelState.AddModelError("", $"Policy Violation: {pex.Message}");
                    if (!string.IsNullOrEmpty(pex.RemediationHint))
                    {
                        ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating workflow");
                    ModelState.AddModelError("", "Error creating workflow. Please try again.");
                }
            }
            return View(createWorkflowDto);
        }

        // GET: Workflow/Edit/5
        [Authorize(GrcPermissions.Workflow.Manage)]
        public async Task<IActionResult> Edit(Guid id)
        {
            var workflow = await _workflowService.GetByIdAsync(id);
            if (workflow == null)
            {
                return NotFound();
            }

            var updateDto = new UpdateWorkflowDto
            {
                WorkflowNumber = workflow.WorkflowNumber,
                Name = workflow.Name,
                Description = workflow.Description,
                Category = workflow.Category,
                Type = workflow.Type,
                Status = workflow.Status,
                Priority = workflow.Priority,
                AssignedTo = workflow.AssignedTo,
                InitiatedBy = workflow.InitiatedBy,
                DueDate = workflow.DueDate,
                Steps = workflow.Steps,
                Conditions = workflow.Conditions,
                Notifications = workflow.Notifications,
                DataClassification = workflow.DataClassification,
                Owner = workflow.Owner
            };

            return View(updateDto);
        }

        // POST: Workflow/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(GrcPermissions.Workflow.Manage)]
        public async Task<IActionResult> Edit(Guid id, UpdateWorkflowDto updateWorkflowDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // POLICY ENFORCEMENT: Validate governance metadata before update
                    await _policyHelper.EnforceUpdateAsync(
                        "Workflow",
                        updateWorkflowDto,
                        dataClassification: updateWorkflowDto.DataClassification,
                        owner: updateWorkflowDto.Owner
                    );

                    var workflow = await _workflowService.UpdateAsync(id, updateWorkflowDto);
                    if (workflow == null)
                    {
                        return NotFound();
                    }
                    TempData["Success"] = "Workflow updated successfully";
                    return RedirectToAction(nameof(Details), new { id = workflow.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation updating workflow {WorkflowId}", id);
                    ModelState.AddModelError("", $"Policy Violation: {pex.Message}");
                    if (!string.IsNullOrEmpty(pex.RemediationHint))
                    {
                        ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating workflow with ID {WorkflowId}", id);
                    ModelState.AddModelError("", "Error updating workflow. Please try again.");
                }
            }
            return View(updateWorkflowDto);
        }

        // GET: Workflow/Delete/5
        [Authorize(GrcPermissions.Workflow.Manage)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var workflow = await _workflowService.GetByIdAsync(id);
            if (workflow == null)
            {
                return NotFound();
            }
            return View(workflow);
        }

        // POST: Workflow/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(GrcPermissions.Workflow.Manage)]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                // POLICY ENFORCEMENT: Check if deletion is allowed
                var workflow = await _workflowService.GetByIdAsync(id);
                if (workflow != null)
                {
                    await _policyHelper.EnforceAsync(
                        "delete",
                        "Workflow",
                        workflow,
                        dataClassification: workflow.DataClassification,
                        owner: workflow.Owner
                    );
                }

                await _workflowService.DeleteAsync(id);
                TempData["Success"] = "Workflow deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (PolicyViolationException pex)
            {
                _logger.LogWarning(pex, "Policy violation deleting workflow {WorkflowId}", id);
                TempData["Error"] = $"Policy Violation: {pex.Message}. {pex.RemediationHint}";
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workflow with ID {WorkflowId}", id);
                TempData["Error"] = "Error deleting workflow. Please try again.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // GET: Workflow/Statistics
        [Authorize(GrcPermissions.Workflow.View)]
        public async Task<IActionResult> Statistics()
        {
            try
            {
                var statistics = await _workflowService.GetStatisticsAsync();
                return View(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow statistics");
                TempData["Error"] = "Error loading statistics. Please try again.";
                return View(new WorkflowStatisticsDto());
            }
        }

        // GET: Workflow/ByCategory/5
        [Authorize(GrcPermissions.Workflow.View)]
        public async Task<IActionResult> ByCategory(string category)
        {
            try
            {
                var workflows = await _workflowService.GetByCategoryAsync(category);
                ViewBag.Category = category;
                return View(workflows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflows by category {Category}", category);
                TempData["Error"] = "Error loading workflows. Please try again.";
                return View(new List<WorkflowDto>());
            }
        }

        // GET: Workflow/ByStatus/5
        [Authorize(GrcPermissions.Workflow.View)]
        public async Task<IActionResult> ByStatus(string status)
        {
            try
            {
                var workflows = await _workflowService.GetByStatusAsync(status);
                ViewBag.Status = status;
                return View(workflows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflows by status {Status}", status);
                TempData["Error"] = "Error loading workflows. Please try again.";
                return View(new List<WorkflowDto>());
            }
        }

        // GET: Workflow/Overdue
        [Authorize(GrcPermissions.Workflow.View)]
        public async Task<IActionResult> Overdue()
        {
            try
            {
                var workflows = await _workflowService.GetOverdueWorkflowsAsync();
                return View(workflows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overdue workflows");
                TempData["Error"] = "Error loading overdue workflows. Please try again.";
                return View(new List<WorkflowDto>());
            }
        }

        // GET: Workflow/Executions/5
        [Authorize(GrcPermissions.Workflow.View)]
        public async Task<IActionResult> Executions(Guid id)
        {
            try
            {
                var executions = await _workflowService.GetWorkflowExecutionsAsync(id);
                ViewBag.WorkflowId = id;
                return View(executions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting executions for workflow ID {WorkflowId}", id);
                TempData["Error"] = "Error loading executions. Please try again.";
                return View(new List<WorkflowExecutionDto>());
            }
        }

        // POST: Workflow/Execute/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(GrcPermissions.Workflow.Manage)]
        public async Task<IActionResult> Execute(Guid id)
        {
            try
            {
                // POLICY ENFORCEMENT: Check if execution is allowed
                var workflow = await _workflowService.GetByIdAsync(id);
                if (workflow != null)
                {
                    await _policyHelper.EnforceAsync(
                        "execute",
                        "Workflow",
                        workflow,
                        dataClassification: workflow.DataClassification,
                        owner: workflow.Owner
                    );
                }

                var execution = await _workflowService.ExecuteWorkflowAsync(id);
                TempData["Success"] = "Workflow execution started successfully";
                return RedirectToAction(nameof(Executions), new { id });
            }
            catch (PolicyViolationException pex)
            {
                _logger.LogWarning(pex, "Policy violation executing workflow {WorkflowId}", id);
                TempData["Error"] = $"Policy Violation: {pex.Message}. {pex.RemediationHint}";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing workflow with ID {WorkflowId}", id);
                TempData["Error"] = "Error executing workflow. Please try again.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}
