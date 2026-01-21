using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using GrcMvc.Resources;

namespace GrcMvc.ViewComponents
{
    /// <summary>
    /// ViewComponent for rendering standard CRUD action buttons with localization
    /// </summary>
    public class ActionButtonsViewComponent : ViewComponent
    {
        private readonly IHtmlLocalizer<SharedResource> _localizer;

        public ActionButtonsViewComponent(IHtmlLocalizer<SharedResource> localizer)
        {
            _localizer = localizer;
        }

        public IViewComponentResult Invoke(
            string controller,
            int id,
            bool showEdit = true,
            bool showDetails = true,
            bool showDelete = true,
            bool showDownload = false,
            string? downloadUrl = null)
        {
            ViewBag.Controller = controller;
            ViewBag.Id = id;
            ViewBag.ShowEdit = showEdit;
            ViewBag.ShowDetails = showDetails;
            ViewBag.ShowDelete = showDelete;
            ViewBag.ShowDownload = showDownload;
            ViewBag.DownloadUrl = downloadUrl;
            ViewBag.Localizer = _localizer;
            
            return View();
        }
    }
}
