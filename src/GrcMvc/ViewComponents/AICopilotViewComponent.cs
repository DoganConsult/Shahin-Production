using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.ViewComponents
{
    /// <summary>
    /// AI Copilot floating chat interface ViewComponent
    /// Provides natural language interaction with GRC data via Claude AI
    /// </summary>
    public class AICopilotViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
