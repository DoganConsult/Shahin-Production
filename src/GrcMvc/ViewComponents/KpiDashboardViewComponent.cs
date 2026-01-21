using Microsoft.AspNetCore.Mvc;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.ViewComponents
{
    /// <summary>
    /// KPI Dashboard with Real-Time Updates via SignalR
    /// </summary>
    public class KpiDashboardViewComponent : ViewComponent
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<KpiDashboardViewComponent> _logger;

        public KpiDashboardViewComponent(IDashboardService dashboardService, ILogger<KpiDashboardViewComponent> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(string dashboardType = "executive")
        {
            var model = new KpiDashboardModel
            {
                DashboardType = dashboardType,
                Kpis = new List<KpiItemModel>()
            };

            try
            {
                var tenantIdClaim = HttpContext.User.FindFirst("TenantId")?.Value;
                if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
                {
                    _logger.LogWarning("No tenant ID found in claims for KPI dashboard");
                    return View(model);
                }

                var executiveDashboard = await _dashboardService.GetExecutiveDashboardAsync(tenantId);
                var riskDashboard = await _dashboardService.GetRiskDashboardAsync(tenantId);
                var taskDashboard = await _dashboardService.GetTaskDashboardAsync(tenantId);

                model.Kpis = new List<KpiItemModel>
                {
                    new()
                    {
                        Id = "compliance_score",
                        Name = "Compliance Score",
                        Value = (double)executiveDashboard.OverallComplianceScore,
                        Unit = "%",
                        Icon = "bi-check-circle",
                        Color = "green"
                    },
                    new()
                    {
                        Id = "total_risks",
                        Name = "Total Risks",
                        Value = riskDashboard.TotalRisks,
                        Icon = "bi-exclamation-triangle",
                        Color = "red"
                    },
                    new()
                    {
                        Id = "high_risks",
                        Name = "High Risks",
                        Value = riskDashboard.HighRisks,
                        Icon = "bi-shield-exclamation",
                        Color = "red"
                    },
                    new()
                    {
                        Id = "open_tasks",
                        Name = "Open Tasks",
                        Value = taskDashboard.OpenTasks,
                        Icon = "bi-list-task",
                        Color = "blue"
                    },
                    new()
                    {
                        Id = "overdue_tasks",
                        Name = "Overdue Tasks",
                        Value = taskDashboard.OverdueTasks,
                        Icon = "bi-clock-history",
                        Color = "yellow"
                    },
                    new()
                    {
                        Id = "compliant_requirements",
                        Name = "Compliant",
                        Value = executiveDashboard.CompliantRequirements,
                        Icon = "bi-check-square",
                        Color = "green"
                    }
                };

                model.LastUpdated = executiveDashboard.GeneratedAt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading KPI dashboard data");
            }

            return View(model);
        }
    }

    public class KpiDashboardModel
    {
        public string DashboardType { get; set; } = "executive";
        public List<KpiItemModel> Kpis { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class KpiItemModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public double? PreviousValue { get; set; }
        public double? ChangePercent => PreviousValue.HasValue && PreviousValue != 0
            ? Math.Round((Value - PreviousValue.Value) / PreviousValue.Value * 100, 1)
            : null;
    }
}
