using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Data;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Controllers;

[Authorize]
public class ControlsController : Controller
{
    private readonly GrcDbContext _db;
    private readonly ILogger<ControlsController> _logger;

    public ControlsController(GrcDbContext db, ILogger<ControlsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: Controls - Control Library (Shahin MAP)
    public IActionResult Index()
    {
        return View();
    }

    // GET: Controls/Requirements - Requirements Manager
    public IActionResult Requirements()
    {
        return View();
    }

    // GET: Controls/Mapping - Control Mapping (Req → Obj → Control)
    public IActionResult Mapping()
    {
        return View();
    }

    // GET: Controls/Applicability - Applicability Manager (Shahin APPLY)
    public IActionResult Applicability()
    {
        return View();
    }

    // GET: Controls/Tests - Test Manager (Shahin PROVE)
    public IActionResult Tests()
    {
        return View();
    }

    // GET: Controls/Remediation - Remediation Manager (Shahin FIX)
    public IActionResult Remediation()
    {
        return View();
    }

    // GET: Controls/Assess - Control Assessment Form
    public IActionResult Assess(Guid? id)
    {
        ViewBag.ControlId = id;
        return View();
    }

    // GET: Controls/Details/{id}
    public async Task<IActionResult> Details(Guid id)
    {
        var control = await _db.Controls.FirstOrDefaultAsync(c => c.Id == id);
        if (control == null)
            return NotFound();
        return View(control);
    }

    // GET: Controls/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Controls/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] Models.Entities.Catalogs.Control control)
    {
        if (ModelState.IsValid)
        {
            control.Id = Guid.NewGuid();
            _db.Controls.Add(control);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(control);
    }

    // GET: Controls/Edit/{id}
    public async Task<IActionResult> Edit(Guid id)
    {
        var control = await _db.Controls.FirstOrDefaultAsync(c => c.Id == id);
        if (control == null)
            return NotFound();
        return View(control);
    }

    // POST: Controls/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [FromForm] Models.Entities.Catalogs.Control control)
    {
        if (id != control.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            _db.Update(control);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(control);
    }
}
