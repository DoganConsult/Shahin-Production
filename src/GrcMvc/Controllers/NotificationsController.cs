using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "الإشعارات";
        return View();
    }
}
