using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.Controllers;

[Authorize]
public class ActionPlansController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "خطط العمل";
        return View();
    }
}
