using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace GrcMvc.Controllers.Api;

/// <summary>
/// Localization diagnostics controller
/// </summary>
[ApiController]
[Route("api/localization")]
[AllowAnonymous]
public class LocalizationDiagController : ControllerBase
{
    private readonly IStringLocalizer<GrcMvc.Resources.SharedResource> _localizer;
    private readonly ILogger<LocalizationDiagController> _logger;

    public LocalizationDiagController(
        IStringLocalizer<GrcMvc.Resources.SharedResource> localizer,
        ILogger<LocalizationDiagController> logger)
    {
        _localizer = localizer;
        _logger = logger;
    }

    /// <summary>
    /// Test localization for specific keys
    /// </summary>
    [HttpGet("test")]
    public IActionResult TestLocalization([FromQuery] string? culture = null)
    {
        var testKeys = new[]
        {
            "Nav_RiskManagement",
            "Nav_Compliance",
            "Nav_Dashboard",
            "Error_404_Title",
            "Error_GoHome",
            "Risks",
            "Controls"
        };

        var currentCulture = CultureInfo.CurrentCulture.Name;
        var currentUICulture = CultureInfo.CurrentUICulture.Name;

        if (!string.IsNullOrEmpty(culture))
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            CultureInfo.CurrentUICulture = new CultureInfo(culture);
        }

        var results = new Dictionary<string, object>();
        foreach (var key in testKeys)
        {
            var value = _localizer[key];
            results[key] = new
            {
                value = value.Value,
                resourceNotFound = value.ResourceNotFound,
                searchedLocation = value.SearchedLocation
            };
        }

        return Ok(new
        {
            requestCulture = culture,
            originalCulture = currentCulture,
            originalUICulture = currentUICulture,
            effectiveCulture = CultureInfo.CurrentCulture.Name,
            effectiveUICulture = CultureInfo.CurrentUICulture.Name,
            testResults = results,
            resourceAssembly = typeof(GrcMvc.Resources.SharedResource).Assembly.FullName,
            resourceType = typeof(GrcMvc.Resources.SharedResource).FullName
        });
    }

    /// <summary>
    /// List all embedded resources
    /// </summary>
    [HttpGet("resources")]
    public IActionResult ListResources()
    {
        var assembly = typeof(GrcMvc.Resources.SharedResource).Assembly;
        var resourceNames = assembly.GetManifestResourceNames();

        // Check satellite assemblies
        var arAssembly = TryLoadSatellite(assembly, "ar");
        var enAssembly = TryLoadSatellite(assembly, "en");

        return Ok(new
        {
            mainAssembly = assembly.FullName,
            mainAssemblyLocation = assembly.Location,
            mainResources = resourceNames,
            arSatellite = arAssembly != null ? new
            {
                name = arAssembly.FullName,
                location = arAssembly.Location,
                resources = arAssembly.GetManifestResourceNames()
            } : null,
            enSatellite = enAssembly != null ? new
            {
                name = enAssembly.FullName,
                location = enAssembly.Location,
                resources = enAssembly.GetManifestResourceNames()
            } : null
        });
    }

    private Assembly? TryLoadSatellite(Assembly mainAssembly, string culture)
    {
        try
        {
            var satelliteName = new AssemblyName(mainAssembly.FullName!)
            {
                Name = mainAssembly.GetName().Name + ".resources"
            };
            satelliteName.CultureInfo = new CultureInfo(culture);
            
            return Assembly.Load(satelliteName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Could not load satellite assembly for {Culture}: {Error}", culture, ex.Message);
            return null;
        }
    }
}
