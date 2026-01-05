using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.Controllers;

[Authorize]
public class RegulatorsController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "الجهات التنظيمية";
        return View();
    }
}
