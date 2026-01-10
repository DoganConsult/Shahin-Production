using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api;

/// <summary>
/// Simple test webhook controller for debugging
/// </summary>
[ApiController]
[Route("api/test-webhook")]
[AllowAnonymous]
[IgnoreAntiforgeryToken]
public class TestWebhookController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "Test webhook is working", timestamp = DateTime.UtcNow });
    }

    [HttpPost]
    public IActionResult Post([FromQuery] string? validationToken = null)
    {
        if (!string.IsNullOrEmpty(validationToken))
        {
            return Content(validationToken, "text/plain");
        }
        
        return Accepted(new { message = "Notification received", timestamp = DateTime.UtcNow });
    }
}
