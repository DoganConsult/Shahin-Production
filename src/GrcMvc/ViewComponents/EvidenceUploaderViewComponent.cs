using Microsoft.AspNetCore.Mvc;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.ViewComponents
{
    /// <summary>
    /// Evidence Uploader with AI Auto-Tagging - Uses Claude to classify uploaded documents
    /// </summary>
    public class EvidenceUploaderViewComponent : ViewComponent
    {
        private readonly IClaudeAgentService _claudeService;

        public EvidenceUploaderViewComponent(IClaudeAgentService claudeService)
        {
            _claudeService = claudeService;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid? controlId = null, Guid? assessmentId = null, bool showAiTagging = true)
        {
            var model = new EvidenceUploaderModel
            {
                ControlId = controlId,
                AssessmentId = assessmentId,
                ShowAiTagging = showAiTagging,
                AiAvailable = await _claudeService.IsAvailableAsync()
            };

            return View(model);
        }
    }

    public class EvidenceUploaderModel
    {
        public Guid? ControlId { get; set; }
        public Guid? AssessmentId { get; set; }
        public bool ShowAiTagging { get; set; } = true;
        public bool AiAvailable { get; set; }
        public List<string> AcceptedFileTypes { get; set; } = new() { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg" };
        public int MaxFileSizeMb { get; set; } = 25;
    }
}
