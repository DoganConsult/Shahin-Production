using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.Controllers;

[Authorize]
public class VendorsController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "إدارة الموردين";
        return View();
    }
}
