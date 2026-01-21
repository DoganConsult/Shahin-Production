using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Services.Interfaces;
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
    public class CertificationController : Controller
    {
        private readonly ICertificationService _certificationService;
        private readonly ILogger<CertificationController> _logger;
        private readonly IWorkspaceContextService? _workspaceContext;
        private readonly PolicyEnforcementHelper _policyHelper;

        public CertificationController(
            ICertificationService certificationService,
            ILogger<CertificationController> logger,
            PolicyEnforcementHelper policyHelper,
            IWorkspaceContextService? workspaceContext = null)
        {
            _certificationService = certificationService;
            _logger = logger;
            _policyHelper = policyHelper;
            _workspaceContext = workspaceContext;
        }

        // GET: Certification/Index
        [Authorize(GrcPermissions.Certification.View)]
        public async Task<IActionResult> Index()
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var certifications = await _certificationService.GetAllAsync(tenantId);
                return View(certifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading certifications");
                TempData["ErrorMessage"] = "Failed to load certifications.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Certification/Readiness
        [Authorize(GrcPermissions.Certification.View)]
        public async Task<IActionResult> Readiness()
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var readiness = await _certificationService.GetReadinessAsync(tenantId);
                return View(readiness);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading certification readiness");
                TempData["ErrorMessage"] = "Failed to load certification readiness assessment.";
                return RedirectToAction("Index");
            }
        }

        // GET: Certification/Preparation
        [Authorize(GrcPermissions.Certification.Manage)]
        public async Task<IActionResult> Preparation(Guid? certificationId)
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var preparation = certificationId.HasValue
                    ? await _certificationService.GetPreparationPlanAsync(tenantId, certificationId.Value)
                    : await _certificationService.GetDefaultPreparationPlanAsync(tenantId);

                return View(preparation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading certification preparation");
                TempData["ErrorMessage"] = "Failed to load certification preparation plan.";
                return RedirectToAction("Index");
            }
        }

        // GET: Certification/Audit
        [Authorize(GrcPermissions.Certification.Manage)]
        public async Task<IActionResult> Audit(Guid? certificationId)
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var audits = certificationId.HasValue
                    ? await _certificationService.GetAuditsForCertificationAsync(tenantId, certificationId.Value)
                    : await _certificationService.GetAllAuditsAsync(tenantId);

                return View(audits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading certification audits");
                TempData["ErrorMessage"] = "Failed to load certification audits.";
                return RedirectToAction("Index");
            }
        }

        // GET: Certification/Portfolio
        [Authorize(GrcPermissions.Certification.View)]
        public async Task<IActionResult> Portfolio()
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var portfolio = await _certificationService.GetPortfolioAsync(tenantId);
                return View(portfolio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading certification portfolio");
                TempData["ErrorMessage"] = "Failed to load certification portfolio.";
                return RedirectToAction("Index");
            }
        }
    }
}
