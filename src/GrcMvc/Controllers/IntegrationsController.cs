using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.Controllers;

[Authorize]
public class IntegrationsController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "مركز التكامل";
        return View();
    }
}
