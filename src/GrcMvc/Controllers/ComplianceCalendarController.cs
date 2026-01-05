using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.Controllers;

[Authorize]
public class ComplianceCalendarController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "تقويم الامتثال";
        return View();
    }
}
