using Microsoft.AspNetCore.Mvc;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.ViewComponents
{
    /// <summary>
    /// Approval Flow Visualizer - Shows workflow approval chain status
    /// </summary>
    public class ApprovalFlowViewComponent : ViewComponent
    {
        private readonly IWorkflowService _workflowService;

        public ApprovalFlowViewComponent(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid? workflowId = null, string entityType = "", Guid? entityId = null)
        {
            var model = new ApprovalFlowModel
            {
                WorkflowId = workflowId,
                EntityType = entityType,
                EntityId = entityId,
                Steps = new List<ApprovalStepModel>()
            };

            if (workflowId.HasValue)
            {
                try
                {
                    var workflow = await _workflowService.GetByIdAsync(workflowId.Value);
                    if (workflow != null)
                    {
                        model.Status = workflow.Status;
                        model.WorkflowName = workflow.Name;
                    }

                    var executions = await _workflowService.GetWorkflowExecutionsAsync(workflowId.Value);
                    if (executions != null)
                    {
                        var executionList = executions.ToList();
                        model.CurrentStepIndex = executionList.Count;
                        model.Steps = executionList.Select((e, i) => new ApprovalStepModel
                        {
                            StepIndex = i,
                            StepName = e.ExecutionNumber,
                            ApproverName = e.InitiatedBy,
                            Status = e.Status,
                            CompletedAt = e.EndTime,
                            Comments = e.Duration.HasValue ? $"Duration: {e.Duration.Value:F1}s" : string.Empty
                        }).ToList();
                    }
                }
                catch { /* Silent fail */ }
            }

            return View(model);
        }
    }

    public class ApprovalFlowModel
    {
        public Guid? WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public Guid? EntityId { get; set; }
        public int CurrentStepIndex { get; set; }
        public string Status { get; set; } = "Pending";
        public List<ApprovalStepModel> Steps { get; set; } = new();
    }

    public class ApprovalStepModel
    {
        public int StepIndex { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string ApproverName { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime? CompletedAt { get; set; }
        public string Comments { get; set; } = string.Empty;
    }
}
