using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Models;

namespace GrcMvc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Reports()
    {
        return View();
    }

    public IActionResult Admin()
    {
        return View();
    }

    public IActionResult ManageTenants()
    {
        // Return empty list - will be populated by JavaScript/API
        return View(new List<GrcMvc.Models.DTOs.TenantDto>());
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
