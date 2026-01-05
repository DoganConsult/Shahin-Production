using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.Controllers;

[Authorize]
public class FrameworksController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "مكتبة الأطر التنظيمية";
        return View();
    }
}
