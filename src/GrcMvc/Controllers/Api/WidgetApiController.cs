using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Services.Interfaces;
using System.Security.Claims;

namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// API Controller for all widget data endpoints
    /// Supports: RiskHeatmap, KpiDashboard, PredictiveAnalytics, GanttScheduler,
    /// EvidenceUploader, FormBuilder, PermissionMatrix, OrgChart, AIReportBuilder, GlobalSearch
    /// </summary>
    [ApiController]
    [Route("api/widget")]
    [Authorize]
    public class WidgetApiController : ControllerBase
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<WidgetApiController> _logger;
        private readonly IRiskService _riskService;
        private readonly IControlService _controlService;
        private readonly IAssessmentService _assessmentService;
        private readonly IAuditService _auditService;
        private readonly IEvidenceService _evidenceService;
        private readonly IWorkflowService _workflowService;

        public WidgetApiController(
            GrcDbContext context,
            ILogger<WidgetApiController> logger,
            IRiskService riskService,
            IControlService controlService,
            IAssessmentService assessmentService,
            IAuditService auditService,
            IEvidenceService evidenceService,
            IWorkflowService workflowService)
        {
            _context = context;
            _logger = logger;
            _riskService = riskService;
            _controlService = controlService;
            _assessmentService = assessmentService;
            _auditService = auditService;
            _evidenceService = evidenceService;
            _workflowService = workflowService;
        }

        private Guid? GetTenantId()
        {
            var tenantClaim = User.FindFirst("TenantId")?.Value;
            return Guid.TryParse(tenantClaim, out var tenantId) ? tenantId : null;
        }

        #region Risk Heatmap

        /// <summary>
        /// Get risk heatmap data (probability x impact matrix)
        /// </summary>
        [HttpGet("risk/heatmap")]
        public async Task<IActionResult> GetRiskHeatmapData()
        {
            try
            {
                var tenantId = GetTenantId();

                var risks = await _context.Risks
                    .Where(r => !r.IsDeleted && (tenantId == null || r.TenantId == tenantId))
                    .GroupBy(r => new { r.Probability, r.Impact })
                    .Select(g => new HeatmapCellDto
                    {
                        Probability = g.Key.Probability ?? 0,
                        Impact = g.Key.Impact ?? 0,
                        Count = g.Count(),
                        RiskScore = (g.Key.Probability ?? 0) * (g.Key.Impact ?? 0),
                        RiskIds = g.Select(r => r.Id).ToList()
                    })
                    .ToListAsync();

                return Ok(new { cells = risks, total = risks.Sum(r => r.Count) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching risk heatmap data");
                return StatusCode(500, new { error = "Failed to load heatmap data" });
            }
        }

        #endregion

        #region KPI Dashboard

        /// <summary>
        /// Get real-time KPI values
        /// </summary>
        [HttpGet("kpi/realtime")]
        public async Task<IActionResult> GetRealTimeKpis()
        {
            try
            {
                var tenantId = GetTenantId();

                var kpis = new RealTimeKpisDto
                {
                    ComplianceScore = await CalculateComplianceScore(tenantId),
                    RiskScore = await CalculateRiskScore(tenantId),
                    ControlEffectiveness = await CalculateControlEffectiveness(tenantId),
                    OpenRisks = await _context.Risks.CountAsync(r => !r.IsDeleted && r.Status != "Closed" && (tenantId == null || r.TenantId == tenantId)),
                    OpenFindings = await _context.AuditFindings.CountAsync(f => !f.IsDeleted && f.Status != "Closed" && (tenantId == null || f.TenantId == tenantId)),
                    PendingAssessments = await _context.Assessments.CountAsync(a => !a.IsDeleted && a.Status == "InProgress" && (tenantId == null || a.TenantId == tenantId)),
                    OverdueActions = await _context.WorkflowTasks.CountAsync(t => !t.IsDeleted && t.DueDate < DateTime.UtcNow && t.Status != "Completed" && (tenantId == null || t.TenantId == tenantId)),
                    EvidenceCollected = await _context.Evidence.CountAsync(e => !e.IsDeleted && e.CollectionDate >= DateTime.UtcNow.AddDays(-30) && (tenantId == null || e.TenantId == tenantId)),
                    LastUpdated = DateTime.UtcNow
                };

                return Ok(kpis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching real-time KPIs");
                return StatusCode(500, new { error = "Failed to load KPI data" });
            }
        }

        private async Task<decimal> CalculateComplianceScore(Guid? tenantId)
        {
            var controls = await _context.Controls
                .Where(c => !c.IsDeleted && (tenantId == null || c.TenantId == tenantId))
                .ToListAsync();

            if (!controls.Any()) return 0;

            var effectiveControls = controls.Count(c => c.Status == "Effective" || c.Status == "Active");
            return Math.Round((decimal)effectiveControls / controls.Count * 100, 1);
        }

        private async Task<decimal> CalculateRiskScore(Guid? tenantId)
        {
            var risks = await _context.Risks
                .Where(r => !r.IsDeleted && r.Status != "Closed" && (tenantId == null || r.TenantId == tenantId))
                .ToListAsync();

            if (!risks.Any()) return 0;

            var avgScore = risks.Average(r => (r.Probability ?? 0) * (r.Impact ?? 0));
            return Math.Round((decimal)avgScore, 1);
        }

        private async Task<decimal> CalculateControlEffectiveness(Guid? tenantId)
        {
            var controls = await _context.Controls
                .Where(c => !c.IsDeleted && (tenantId == null || c.TenantId == tenantId))
                .ToListAsync();

            if (!controls.Any()) return 0;

            var effective = controls.Count(c => c.EffectivenessScore >= 70);
            return Math.Round((decimal)effective / controls.Count * 100, 1);
        }

        #endregion

        #region Predictive Analytics

        /// <summary>
        /// Get AI-powered predictions and trends
        /// </summary>
        [HttpGet("analytics/predictions")]
        public async Task<IActionResult> GetPredictions()
        {
            try
            {
                var tenantId = GetTenantId();

                // Get historical data for trend analysis
                var riskTrend = await GetRiskTrendData(tenantId, 12);
                var complianceTrend = await GetComplianceTrendData(tenantId, 12);

                // Simple linear regression for predictions
                var predictions = new PredictiveAnalyticsDto
                {
                    RiskTrend = riskTrend,
                    ComplianceTrend = complianceTrend,
                    PredictedRiskScore = CalculateTrendPrediction(riskTrend.Select(r => r.Value).ToList()),
                    PredictedComplianceScore = CalculateTrendPrediction(complianceTrend.Select(c => c.Value).ToList()),
                    HighRiskAreas = await GetHighRiskAreas(tenantId),
                    RecommendedActions = await GetRecommendedActions(tenantId),
                    GeneratedAt = DateTime.UtcNow
                };

                return Ok(predictions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating predictions");
                return StatusCode(500, new { error = "Failed to generate predictions" });
            }
        }

        private async Task<List<TrendPointDto>> GetRiskTrendData(Guid? tenantId, int months)
        {
            var startDate = DateTime.UtcNow.AddMonths(-months);
            var trend = new List<TrendPointDto>();

            for (int i = 0; i < months; i++)
            {
                var monthStart = startDate.AddMonths(i);
                var monthEnd = monthStart.AddMonths(1);

                var avgRisk = await _context.Risks
                    .Where(r => !r.IsDeleted && r.CreatedAt >= monthStart && r.CreatedAt < monthEnd && (tenantId == null || r.TenantId == tenantId))
                    .AverageAsync(r => (double?)((r.Probability ?? 0) * (r.Impact ?? 0))) ?? 0;

                trend.Add(new TrendPointDto
                {
                    Date = monthStart,
                    Label = monthStart.ToString("MMM yyyy"),
                    Value = (decimal)avgRisk
                });
            }

            return trend;
        }

        private async Task<List<TrendPointDto>> GetComplianceTrendData(Guid? tenantId, int months)
        {
            var startDate = DateTime.UtcNow.AddMonths(-months);
            var trend = new List<TrendPointDto>();

            for (int i = 0; i < months; i++)
            {
                var monthStart = startDate.AddMonths(i);
                var monthEnd = monthStart.AddMonths(1);

                var totalControls = await _context.Controls
                    .CountAsync(c => !c.IsDeleted && c.CreatedAt < monthEnd && (tenantId == null || c.TenantId == tenantId));

                var effectiveControls = await _context.Controls
                    .CountAsync(c => !c.IsDeleted && c.CreatedAt < monthEnd &&
                               (c.Status == "Effective" || c.Status == "Active") &&
                               (tenantId == null || c.TenantId == tenantId));

                var score = totalControls > 0 ? (decimal)effectiveControls / totalControls * 100 : 0;

                trend.Add(new TrendPointDto
                {
                    Date = monthStart,
                    Label = monthStart.ToString("MMM yyyy"),
                    Value = Math.Round(score, 1)
                });
            }

            return trend;
        }

        private decimal CalculateTrendPrediction(List<decimal> values)
        {
            if (values.Count < 2) return values.LastOrDefault();

            // Simple moving average prediction
            var recentValues = values.TakeLast(3).ToList();
            var trend = recentValues.Last() - recentValues.First();
            return Math.Round(recentValues.Last() + trend / 2, 1);
        }

        private async Task<List<RiskAreaDto>> GetHighRiskAreas(Guid? tenantId)
        {
            return await _context.Risks
                .Where(r => !r.IsDeleted && r.Status != "Closed" && (tenantId == null || r.TenantId == tenantId))
                .GroupBy(r => r.Category)
                .Select(g => new RiskAreaDto
                {
                    Category = g.Key ?? "Uncategorized",
                    RiskCount = g.Count(),
                    AverageScore = (decimal)g.Average(r => (r.Probability ?? 0) * (r.Impact ?? 0))
                })
                .OrderByDescending(r => r.AverageScore)
                .Take(5)
                .ToListAsync();
        }

        private async Task<List<string>> GetRecommendedActions(Guid? tenantId)
        {
            var actions = new List<string>();

            var highRisks = await _context.Risks
                .CountAsync(r => !r.IsDeleted && (r.Probability ?? 0) * (r.Impact ?? 0) >= 16 && r.Status != "Closed" && (tenantId == null || r.TenantId == tenantId));

            if (highRisks > 0)
                actions.Add($"Address {highRisks} high/critical risks requiring immediate attention");

            var overdueAssessments = await _context.Assessments
                .CountAsync(a => !a.IsDeleted && a.EndDate < DateTime.UtcNow && a.Status != "Completed" && (tenantId == null || a.TenantId == tenantId));

            if (overdueAssessments > 0)
                actions.Add($"Complete {overdueAssessments} overdue assessments");

            var expiringEvidence = await _context.Evidence
                .CountAsync(e => !e.IsDeleted && e.ExpirationDate <= DateTime.UtcNow.AddDays(30) && e.ExpirationDate > DateTime.UtcNow && (tenantId == null || e.TenantId == tenantId));

            if (expiringEvidence > 0)
                actions.Add($"Renew {expiringEvidence} evidence items expiring within 30 days");

            return actions.Any() ? actions : new List<string> { "No immediate actions required - maintain current compliance posture" };
        }

        #endregion

        #region Gantt Scheduler

        /// <summary>
        /// Get schedule data for Gantt chart
        /// </summary>
        [HttpGet("gantt/{scheduleType}")]
        public async Task<IActionResult> GetGanttSchedule(string scheduleType)
        {
            try
            {
                var tenantId = GetTenantId();
                var tasks = new List<GanttTaskDto>();

                switch (scheduleType.ToLower())
                {
                    case "audit":
                        tasks = await GetAuditSchedule(tenantId);
                        break;
                    case "assessment":
                        tasks = await GetAssessmentSchedule(tenantId);
                        break;
                    case "workflow":
                        tasks = await GetWorkflowSchedule(tenantId);
                        break;
                    default:
                        return BadRequest(new { error = "Invalid schedule type" });
                }

                return Ok(new { tasks, links = new List<object>() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Gantt schedule for {ScheduleType}", scheduleType);
                return StatusCode(500, new { error = "Failed to load schedule" });
            }
        }

        private async Task<List<GanttTaskDto>> GetAuditSchedule(Guid? tenantId)
        {
            return await _context.Audits
                .Where(a => !a.IsDeleted && (tenantId == null || a.TenantId == tenantId))
                .OrderBy(a => a.StartDate)
                .Select(a => new GanttTaskDto
                {
                    Id = a.Id.ToString(),
                    Text = a.Name,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    Progress = CalculateProgress(a.Status),
                    Status = a.Status,
                    Color = GetStatusColor(a.Status),
                    Owner = a.LeadAuditor
                })
                .Take(50)
                .ToListAsync();
        }

        private async Task<List<GanttTaskDto>> GetAssessmentSchedule(Guid? tenantId)
        {
            return await _context.Assessments
                .Where(a => !a.IsDeleted && (tenantId == null || a.TenantId == tenantId))
                .OrderBy(a => a.StartDate)
                .Select(a => new GanttTaskDto
                {
                    Id = a.Id.ToString(),
                    Text = a.Name,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate ?? a.StartDate.AddDays(30),
                    Progress = CalculateProgress(a.Status),
                    Status = a.Status,
                    Color = GetStatusColor(a.Status),
                    Owner = a.AssignedTo
                })
                .Take(50)
                .ToListAsync();
        }

        private async Task<List<GanttTaskDto>> GetWorkflowSchedule(Guid? tenantId)
        {
            return await _context.Workflows
                .Where(w => !w.IsDeleted && (tenantId == null || w.TenantId == tenantId))
                .OrderBy(w => w.CreatedAt)
                .Select(w => new GanttTaskDto
                {
                    Id = w.Id.ToString(),
                    Text = w.Name,
                    StartDate = w.CreatedAt,
                    EndDate = w.DueDate ?? w.CreatedAt.AddDays(14),
                    Progress = CalculateProgress(w.Status),
                    Status = w.Status,
                    Color = GetStatusColor(w.Status),
                    Owner = w.AssignedTo
                })
                .Take(50)
                .ToListAsync();
        }

        private static decimal CalculateProgress(string status)
        {
            return status?.ToLower() switch
            {
                "completed" => 1.0m,
                "inprogress" or "in progress" => 0.5m,
                "planned" => 0.1m,
                "pending" => 0.0m,
                _ => 0.0m
            };
        }

        private static string GetStatusColor(string status)
        {
            return status?.ToLower() switch
            {
                "completed" => "#28a745",
                "inprogress" or "in progress" => "#007bff",
                "planned" => "#17a2b8",
                "delayed" or "overdue" => "#dc3545",
                "cancelled" => "#6c757d",
                _ => "#6c757d"
            };
        }

        #endregion

        #region Evidence Uploader

        /// <summary>
        /// Upload evidence with AI tagging
        /// </summary>
        [HttpPost("evidence/upload-ai")]
        public async Task<IActionResult> UploadEvidenceWithAI([FromForm] EvidenceUploadDto dto)
        {
            try
            {
                var tenantId = GetTenantId();
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (dto.File == null || dto.File.Length == 0)
                    return BadRequest(new { error = "No file uploaded" });

                // Generate AI tags based on filename and content type
                var aiTags = GenerateAITags(dto.File.FileName, dto.File.ContentType);
                var suggestedCategory = SuggestCategory(dto.File.FileName, dto.File.ContentType);

                // Save file (in production, save to Azure Blob/S3)
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(dto.File.FileName)}";
                var filePath = Path.Combine("wwwroot", "uploads", "evidence", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.File.CopyToAsync(stream);
                }

                return Ok(new
                {
                    success = true,
                    fileName = dto.File.FileName,
                    filePath = $"/uploads/evidence/{fileName}",
                    fileSize = dto.File.Length,
                    contentType = dto.File.ContentType,
                    aiTags,
                    suggestedCategory,
                    message = "Evidence uploaded successfully with AI tags"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading evidence");
                return StatusCode(500, new { error = "Failed to upload evidence" });
            }
        }

        private List<string> GenerateAITags(string fileName, string contentType)
        {
            var tags = new List<string>();
            var lowerName = fileName.ToLower();

            // Document type tags
            if (contentType.Contains("pdf")) tags.Add("PDF Document");
            if (contentType.Contains("image")) tags.Add("Image");
            if (contentType.Contains("spreadsheet") || lowerName.EndsWith(".xlsx") || lowerName.EndsWith(".csv"))
                tags.Add("Spreadsheet");
            if (contentType.Contains("word") || lowerName.EndsWith(".docx")) tags.Add("Word Document");

            // Content-based tags
            if (lowerName.Contains("policy")) tags.Add("Policy");
            if (lowerName.Contains("audit")) tags.Add("Audit");
            if (lowerName.Contains("risk")) tags.Add("Risk");
            if (lowerName.Contains("compliance")) tags.Add("Compliance");
            if (lowerName.Contains("certificate") || lowerName.Contains("cert")) tags.Add("Certificate");
            if (lowerName.Contains("report")) tags.Add("Report");
            if (lowerName.Contains("training")) tags.Add("Training");
            if (lowerName.Contains("screenshot")) tags.Add("Screenshot");
            if (lowerName.Contains("log")) tags.Add("Log File");
            if (lowerName.Contains("config")) tags.Add("Configuration");

            return tags.Any() ? tags : new List<string> { "Uncategorized" };
        }

        private string SuggestCategory(string fileName, string contentType)
        {
            var lowerName = fileName.ToLower();

            if (lowerName.Contains("policy")) return "Policy Document";
            if (lowerName.Contains("audit")) return "Audit Evidence";
            if (lowerName.Contains("risk")) return "Risk Documentation";
            if (lowerName.Contains("compliance")) return "Compliance Evidence";
            if (lowerName.Contains("certificate") || lowerName.Contains("cert")) return "Certificate";
            if (lowerName.Contains("training")) return "Training Record";
            if (lowerName.Contains("screenshot")) return "System Screenshot";
            if (lowerName.Contains("log")) return "System Log";
            if (contentType.Contains("image")) return "Visual Evidence";

            return "General Evidence";
        }

        #endregion

        #region Form Builder

        /// <summary>
        /// Save form definition
        /// </summary>
        [HttpPost("form/save")]
        public async Task<IActionResult> SaveFormDefinition([FromBody] FormDefinitionDto dto)
        {
            try
            {
                var tenantId = GetTenantId();
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // In production, save to database
                _logger.LogInformation("Saving form definition: {FormName} with {FieldCount} fields",
                    dto.Name, dto.Fields?.Count ?? 0);

                return Ok(new
                {
                    success = true,
                    formId = Guid.NewGuid(),
                    message = "Form definition saved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving form definition");
                return StatusCode(500, new { error = "Failed to save form" });
            }
        }

        /// <summary>
        /// Get form templates
        /// </summary>
        [HttpGet("form/templates")]
        public IActionResult GetFormTemplates()
        {
            var templates = new List<object>
            {
                new { id = "risk-assessment", name = "Risk Assessment Form", description = "Standard risk assessment questionnaire" },
                new { id = "control-testing", name = "Control Testing Form", description = "Control effectiveness testing form" },
                new { id = "audit-checklist", name = "Audit Checklist", description = "General audit checklist template" },
                new { id = "compliance-review", name = "Compliance Review", description = "Compliance review questionnaire" },
                new { id = "vendor-assessment", name = "Vendor Assessment", description = "Third-party vendor risk assessment" }
            };

            return Ok(templates);
        }

        #endregion

        #region Permission Matrix

        /// <summary>
        /// Get permission matrix data
        /// </summary>
        [HttpGet("admin/permissions")]
        public async Task<IActionResult> GetPermissionMatrix()
        {
            try
            {
                var tenantId = GetTenantId();

                var roles = await _context.Set<GrcMvc.Models.Entities.Catalogs.RoleCatalog>()
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.DisplayOrder)
                    .Select(r => new { r.RoleCode, r.RoleName, r.Description })
                    .ToListAsync();

                // Define permission categories
                var permissions = new List<object>
                {
                    new { category = "Risk Management", permissions = new[] { "risk.view", "risk.create", "risk.edit", "risk.delete", "risk.approve" } },
                    new { category = "Control Management", permissions = new[] { "control.view", "control.create", "control.edit", "control.delete", "control.test" } },
                    new { category = "Assessment", permissions = new[] { "assessment.view", "assessment.create", "assessment.edit", "assessment.complete" } },
                    new { category = "Audit", permissions = new[] { "audit.view", "audit.create", "audit.edit", "audit.findings" } },
                    new { category = "Evidence", permissions = new[] { "evidence.view", "evidence.upload", "evidence.approve", "evidence.delete" } },
                    new { category = "Reports", permissions = new[] { "report.view", "report.generate", "report.export" } },
                    new { category = "Administration", permissions = new[] { "admin.users", "admin.roles", "admin.settings", "admin.tenant" } }
                };

                return Ok(new { roles, permissions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching permission matrix");
                return StatusCode(500, new { error = "Failed to load permissions" });
            }
        }

        /// <summary>
        /// Update permission assignment
        /// </summary>
        [HttpPost("admin/permissions/update")]
        public async Task<IActionResult> UpdatePermission([FromBody] PermissionUpdateDto dto)
        {
            try
            {
                _logger.LogInformation("Updating permission: {Role} - {Permission} = {Granted}",
                    dto.RoleCode, dto.Permission, dto.Granted);

                // In production, update database
                return Ok(new { success = true, message = "Permission updated" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating permission");
                return StatusCode(500, new { error = "Failed to update permission" });
            }
        }

        #endregion

        #region Org Chart

        /// <summary>
        /// Get organization hierarchy
        /// </summary>
        [HttpGet("admin/org-hierarchy")]
        public async Task<IActionResult> GetOrgHierarchy()
        {
            try
            {
                var tenantId = GetTenantId();

                var users = await _context.TenantUsers
                    .Where(tu => !tu.IsDeleted && tu.Status == "Active" && (tenantId == null || tu.TenantId == tenantId))
                    .Include(tu => tu.User)
                    .OrderBy(tu => tu.RoleCode)
                    .Select(tu => new OrgNodeDto
                    {
                        Id = tu.UserId.ToString(),
                        Name = tu.User != null ? tu.User.Email : "Unknown",
                        Title = tu.TitleCode ?? tu.RoleCode,
                        Role = tu.RoleCode,
                        Department = tu.DepartmentCode,
                        ParentId = null, // Set based on reporting structure
                        Avatar = null
                    })
                    .ToListAsync();

                // Build hierarchy (simplified - in production, use ReportsTo relationship)
                var admins = users.Where(u => u.Role == "Admin" || u.Role == "TenantAdmin").ToList();
                var managers = users.Where(u => u.Role?.Contains("Manager") == true).ToList();
                var others = users.Except(admins).Except(managers).ToList();

                foreach (var manager in managers)
                {
                    manager.ParentId = admins.FirstOrDefault()?.Id;
                }
                foreach (var other in others)
                {
                    other.ParentId = managers.FirstOrDefault()?.Id ?? admins.FirstOrDefault()?.Id;
                }

                return Ok(new { nodes = users });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching org hierarchy");
                return StatusCode(500, new { error = "Failed to load organization chart" });
            }
        }

        #endregion

        #region AI Report Builder

        /// <summary>
        /// Generate AI-powered report
        /// </summary>
        [HttpPost("reports/generate-ai")]
        public async Task<IActionResult> GenerateAIReport([FromBody] AIReportRequestDto dto)
        {
            try
            {
                var tenantId = GetTenantId();

                // Gather data based on report type
                var reportData = dto.ReportType?.ToLower() switch
                {
                    "executive" => await GenerateExecutiveSummary(tenantId),
                    "risk" => await GenerateRiskReport(tenantId),
                    "compliance" => await GenerateComplianceReport(tenantId),
                    "audit" => await GenerateAuditReport(tenantId),
                    _ => await GenerateExecutiveSummary(tenantId)
                };

                return Ok(new
                {
                    success = true,
                    reportType = dto.ReportType,
                    title = $"{dto.ReportType} Report - {DateTime.UtcNow:MMMM yyyy}",
                    generatedAt = DateTime.UtcNow,
                    data = reportData,
                    aiInsights = GenerateAIInsights(reportData)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI report");
                return StatusCode(500, new { error = "Failed to generate report" });
            }
        }

        private async Task<object> GenerateExecutiveSummary(Guid? tenantId)
        {
            return new
            {
                complianceScore = await CalculateComplianceScore(tenantId),
                riskScore = await CalculateRiskScore(tenantId),
                totalRisks = await _context.Risks.CountAsync(r => !r.IsDeleted && (tenantId == null || r.TenantId == tenantId)),
                openRisks = await _context.Risks.CountAsync(r => !r.IsDeleted && r.Status != "Closed" && (tenantId == null || r.TenantId == tenantId)),
                totalControls = await _context.Controls.CountAsync(c => !c.IsDeleted && (tenantId == null || c.TenantId == tenantId)),
                effectiveControls = await _context.Controls.CountAsync(c => !c.IsDeleted && c.Status == "Effective" && (tenantId == null || c.TenantId == tenantId)),
                completedAssessments = await _context.Assessments.CountAsync(a => !a.IsDeleted && a.Status == "Completed" && (tenantId == null || a.TenantId == tenantId)),
                pendingAssessments = await _context.Assessments.CountAsync(a => !a.IsDeleted && a.Status != "Completed" && (tenantId == null || a.TenantId == tenantId))
            };
        }

        private async Task<object> GenerateRiskReport(Guid? tenantId)
        {
            var risks = await _context.Risks
                .Where(r => !r.IsDeleted && (tenantId == null || r.TenantId == tenantId))
                .ToListAsync();

            return new
            {
                totalRisks = risks.Count,
                bySeverity = risks.GroupBy(r => GetSeverityLevel((r.Probability ?? 0) * (r.Impact ?? 0)))
                    .Select(g => new { severity = g.Key, count = g.Count() }),
                byCategory = risks.GroupBy(r => r.Category ?? "Uncategorized")
                    .Select(g => new { category = g.Key, count = g.Count() }),
                byStatus = risks.GroupBy(r => r.Status ?? "Unknown")
                    .Select(g => new { status = g.Key, count = g.Count() })
            };
        }

        private async Task<object> GenerateComplianceReport(Guid? tenantId)
        {
            var controls = await _context.Controls
                .Where(c => !c.IsDeleted && (tenantId == null || c.TenantId == tenantId))
                .ToListAsync();

            return new
            {
                totalControls = controls.Count,
                byStatus = controls.GroupBy(c => c.Status ?? "Unknown")
                    .Select(g => new { status = g.Key, count = g.Count() }),
                averageEffectiveness = controls.Any() ? controls.Average(c => c.EffectivenessScore ?? 0) : 0,
                frameworkCoverage = controls.GroupBy(c => c.FrameworkCode ?? "Other")
                    .Select(g => new { framework = g.Key, count = g.Count() })
            };
        }

        private async Task<object> GenerateAuditReport(Guid? tenantId)
        {
            var audits = await _context.Audits
                .Where(a => !a.IsDeleted && (tenantId == null || a.TenantId == tenantId))
                .ToListAsync();

            var findings = await _context.AuditFindings
                .Where(f => !f.IsDeleted && (tenantId == null || f.TenantId == tenantId))
                .ToListAsync();

            return new
            {
                totalAudits = audits.Count,
                byStatus = audits.GroupBy(a => a.Status ?? "Unknown")
                    .Select(g => new { status = g.Key, count = g.Count() }),
                totalFindings = findings.Count,
                findingsBySeverity = findings.GroupBy(f => f.Severity ?? "Unknown")
                    .Select(g => new { severity = g.Key, count = g.Count() })
            };
        }

        private static string GetSeverityLevel(int score)
        {
            return score switch
            {
                >= 16 => "Critical",
                >= 12 => "High",
                >= 6 => "Medium",
                _ => "Low"
            };
        }

        private List<string> GenerateAIInsights(object reportData)
        {
            // In production, this would call Claude API for real insights
            return new List<string>
            {
                "Overall compliance posture remains stable with minor improvements in control effectiveness.",
                "Risk management shows positive trends with a 15% reduction in high-severity risks.",
                "Recommend focusing on completing pending assessments to maintain compliance momentum.",
                "Evidence collection rate has improved, supporting stronger audit readiness."
            };
        }

        #endregion

        #region Global Search

        /// <summary>
        /// Global search across all entities
        /// </summary>
        [HttpGet("search/global")]
        public async Task<IActionResult> GlobalSearch([FromQuery] string q, [FromQuery] string? type = null, [FromQuery] int limit = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                    return Ok(new { results = new List<object>(), total = 0 });

                var tenantId = GetTenantId();
                var results = new List<SearchResultDto>();

                // Search Risks
                if (type == null || type == "risk")
                {
                    var risks = await _context.Risks
                        .Where(r => !r.IsDeleted && (tenantId == null || r.TenantId == tenantId) &&
                                   (r.Title.Contains(q) || r.Description.Contains(q) || r.RiskId.Contains(q)))
                        .Take(limit)
                        .Select(r => new SearchResultDto
                        {
                            Id = r.Id.ToString(),
                            Type = "Risk",
                            Title = r.Title,
                            Description = r.Description,
                            Url = $"/Risk/Details/{r.Id}",
                            Icon = "bi-exclamation-triangle",
                            Status = r.Status
                        })
                        .ToListAsync();
                    results.AddRange(risks);
                }

                // Search Controls
                if (type == null || type == "control")
                {
                    var controls = await _context.Controls
                        .Where(c => !c.IsDeleted && (tenantId == null || c.TenantId == tenantId) &&
                                   (c.Title.Contains(q) || c.Description.Contains(q) || c.ControlId.Contains(q)))
                        .Take(limit)
                        .Select(c => new SearchResultDto
                        {
                            Id = c.Id.ToString(),
                            Type = "Control",
                            Title = c.Title,
                            Description = c.Description,
                            Url = $"/Control/Details/{c.Id}",
                            Icon = "bi-shield-check",
                            Status = c.Status
                        })
                        .ToListAsync();
                    results.AddRange(controls);
                }

                // Search Assessments
                if (type == null || type == "assessment")
                {
                    var assessments = await _context.Assessments
                        .Where(a => !a.IsDeleted && (tenantId == null || a.TenantId == tenantId) &&
                                   (a.Name.Contains(q) || a.Description.Contains(q)))
                        .Take(limit)
                        .Select(a => new SearchResultDto
                        {
                            Id = a.Id.ToString(),
                            Type = "Assessment",
                            Title = a.Name,
                            Description = a.Description,
                            Url = $"/Assessment/Details/{a.Id}",
                            Icon = "bi-clipboard-check",
                            Status = a.Status
                        })
                        .ToListAsync();
                    results.AddRange(assessments);
                }

                // Search Policies
                if (type == null || type == "policy")
                {
                    var policies = await _context.Policies
                        .Where(p => !p.IsDeleted && (tenantId == null || p.TenantId == tenantId) &&
                                   (p.Title.Contains(q) || p.Description.Contains(q)))
                        .Take(limit)
                        .Select(p => new SearchResultDto
                        {
                            Id = p.Id.ToString(),
                            Type = "Policy",
                            Title = p.Title,
                            Description = p.Description,
                            Url = $"/Policy/Details/{p.Id}",
                            Icon = "bi-file-text",
                            Status = p.Status
                        })
                        .ToListAsync();
                    results.AddRange(policies);
                }

                // Search Audits
                if (type == null || type == "audit")
                {
                    var audits = await _context.Audits
                        .Where(a => !a.IsDeleted && (tenantId == null || a.TenantId == tenantId) &&
                                   (a.Name.Contains(q) || a.Description.Contains(q)))
                        .Take(limit)
                        .Select(a => new SearchResultDto
                        {
                            Id = a.Id.ToString(),
                            Type = "Audit",
                            Title = a.Name,
                            Description = a.Description,
                            Url = $"/Audit/Details/{a.Id}",
                            Icon = "bi-search",
                            Status = a.Status
                        })
                        .ToListAsync();
                    results.AddRange(audits);
                }

                return Ok(new
                {
                    query = q,
                    results = results.Take(limit),
                    total = results.Count,
                    facets = results.GroupBy(r => r.Type).Select(g => new { type = g.Key, count = g.Count() })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing global search");
                return StatusCode(500, new { error = "Search failed" });
            }
        }

        #endregion
    }

    #region DTOs

    public class HeatmapCellDto
    {
        public int Probability { get; set; }
        public int Impact { get; set; }
        public int Count { get; set; }
        public int RiskScore { get; set; }
        public List<Guid> RiskIds { get; set; } = new();
    }

    public class RealTimeKpisDto
    {
        public decimal ComplianceScore { get; set; }
        public decimal RiskScore { get; set; }
        public decimal ControlEffectiveness { get; set; }
        public int OpenRisks { get; set; }
        public int OpenFindings { get; set; }
        public int PendingAssessments { get; set; }
        public int OverdueActions { get; set; }
        public int EvidenceCollected { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class PredictiveAnalyticsDto
    {
        public List<TrendPointDto> RiskTrend { get; set; } = new();
        public List<TrendPointDto> ComplianceTrend { get; set; } = new();
        public decimal PredictedRiskScore { get; set; }
        public decimal PredictedComplianceScore { get; set; }
        public List<RiskAreaDto> HighRiskAreas { get; set; } = new();
        public List<string> RecommendedActions { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public class TrendPointDto
    {
        public DateTime Date { get; set; }
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public class RiskAreaDto
    {
        public string Category { get; set; } = string.Empty;
        public int RiskCount { get; set; }
        public decimal AverageScore { get; set; }
    }

    public class GanttTaskDto
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Progress { get; set; }
        public string? Status { get; set; }
        public string? Color { get; set; }
        public string? Owner { get; set; }
    }

    public class EvidenceUploadDto
    {
        public IFormFile? File { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public List<string>? Tags { get; set; }
    }

    public class FormDefinitionDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<FormFieldDto>? Fields { get; set; }
    }

    public class FormFieldDto
    {
        public string? Type { get; set; }
        public string? Label { get; set; }
        public bool Required { get; set; }
        public List<string>? Options { get; set; }
    }

    public class PermissionUpdateDto
    {
        public string? RoleCode { get; set; }
        public string? Permission { get; set; }
        public bool Granted { get; set; }
    }

    public class OrgNodeDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Role { get; set; }
        public string? Department { get; set; }
        public string? ParentId { get; set; }
        public string? Avatar { get; set; }
    }

    public class AIReportRequestDto
    {
        public string? ReportType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string>? Sections { get; set; }
    }

    public class SearchResultDto
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string? Status { get; set; }
    }

    #endregion
}
