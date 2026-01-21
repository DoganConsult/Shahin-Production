using Microsoft.AspNetCore.Mvc;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.ViewComponents
{
    /// <summary>
    /// Workflow Designer - Visual BPMN-style workflow editor using GoJS
    /// </summary>
    public class WorkflowDesignerViewComponent : ViewComponent
    {
        private readonly IWorkflowService _workflowService;

        public WorkflowDesignerViewComponent(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid? workflowId = null, string workflowType = "approval")
        {
            var model = new WorkflowDesignerModel
            {
                WorkflowId = workflowId,
                WorkflowType = workflowType,
                NodeTypes = new List<WorkflowNodeType>
                {
                    new() { Id = "start", Name = "Start", Icon = "bi-play-circle", Color = "#10b981" },
                    new() { Id = "task", Name = "Task", Icon = "bi-square", Color = "#3b82f6" },
                    new() { Id = "approval", Name = "Approval", Icon = "bi-check-square", Color = "#f59e0b" },
                    new() { Id = "decision", Name = "Decision", Icon = "bi-diamond", Color = "#8b5cf6" },
                    new() { Id = "notification", Name = "Notification", Icon = "bi-bell", Color = "#06b6d4" },
                    new() { Id = "end", Name = "End", Icon = "bi-stop-circle", Color = "#ef4444" }
                }
            };

            if (workflowId.HasValue)
            {
                try
                {
                    var workflow = await _workflowService.GetByIdAsync(workflowId.Value);
                    if (workflow != null)
                    {
                        model.WorkflowName = workflow.Name;
                        model.WorkflowJson = workflow.Steps;
                    }
                }
                catch { /* Use empty workflow */ }
            }

            return View(model);
        }
    }

    public class WorkflowDesignerModel
    {
        public Guid? WorkflowId { get; set; }
        public string WorkflowName { get; set; } = "New Workflow";
        public string WorkflowType { get; set; } = "approval";
        public string WorkflowJson { get; set; }
        public List<WorkflowNodeType> NodeTypes { get; set; } = new();
    }

    public class WorkflowNodeType
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
    }
}
