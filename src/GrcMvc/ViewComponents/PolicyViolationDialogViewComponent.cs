using Microsoft.AspNetCore.Mvc;
using GrcMvc.Models.ViewModels;

namespace GrcMvc.ViewComponents;

/// <summary>
/// View Component for Policy Violation Dialog
/// Displays policy violation errors to users
/// </summary>
public class PolicyViolationDialogViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        string? message = null,
        string? ruleId = null,
        string? remediationHint = null,
        List<string>? violations = null,
        string? retryUrl = null,
        bool autoShow = false)
    {
        var model = new PolicyViolationViewModel
        {
            Message = message ?? "A policy violation occurred. Please review the requirements.",
            RuleId = ruleId,
            RemediationHint = remediationHint,
            Violations = violations ?? new List<string>(),
            RetryUrl = retryUrl,
            AutoShow = autoShow
        };

        return View(model);
    }
}

/// <summary>
/// View Model for Policy Violation Dialog
/// </summary>
public class PolicyViolationViewModel
{
    public string Message { get; set; } = string.Empty;
    public string? RuleId { get; set; }
    public string? RemediationHint { get; set; }
    public List<string> Violations { get; set; } = new();
    public string? RetryUrl { get; set; }
    public bool AutoShow { get; set; }
}
