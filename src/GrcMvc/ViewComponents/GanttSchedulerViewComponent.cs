using Microsoft.AspNetCore.Mvc;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.ViewComponents
{
    /// <summary>
    /// Gantt Chart Scheduler for Audit Planning and Task Management
    /// </summary>
    public class GanttSchedulerViewComponent : ViewComponent
    {
        private readonly IAuditService _auditService;
        private readonly ILogger<GanttSchedulerViewComponent> _logger;

        public GanttSchedulerViewComponent(IAuditService auditService, ILogger<GanttSchedulerViewComponent> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(string scheduleType = "audit", Guid? projectId = null)
        {
            var model = new GanttSchedulerModel
            {
                ScheduleType = scheduleType,
                ProjectId = projectId,
                Tasks = new List<GanttTaskItem>()
            };

            try
            {
                if (scheduleType == "audit")
                {
                    var audits = await _auditService.GetAllAsync();
                    model.Tasks = audits.Select(a => new GanttTaskItem
                    {
                        Id = a.Id.ToString(),
                        Name = a.Name,
                        StartDate = a.PlannedStartDate != default ? a.PlannedStartDate : a.StartDate,
                        EndDate = a.PlannedEndDate != default ? a.PlannedEndDate : a.EndDate,
                        Progress = CalculateProgress(a.Status),
                        Status = a.Status,
                        Assignee = a.LeadAuditor
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Gantt schedule data");
            }

            return View(model);
        }

        private int CalculateProgress(string status)
        {
            return status?.ToLower() switch
            {
                "completed" => 100,
                "closed" => 100,
                "in progress" => 50,
                "ongoing" => 50,
                "planning" => 10,
                "scheduled" => 0,
                "draft" => 0,
                _ => 0
            };
        }
    }

    public class GanttSchedulerModel
    {
        public string ScheduleType { get; set; } = "audit";
        public Guid? ProjectId { get; set; }
        public List<GanttTaskItem> Tasks { get; set; } = new();
        public DateTime ViewStartDate { get; set; } = DateTime.Today.AddMonths(-1);
        public DateTime ViewEndDate { get; set; } = DateTime.Today.AddMonths(3);
    }

    public class GanttTaskItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Progress { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Assignee { get; set; } = string.Empty;
        public string ParentId { get; set; } = string.Empty;
        public List<string> Dependencies { get; set; } = new();
    }
}
