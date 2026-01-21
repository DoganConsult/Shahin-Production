using System.Net.Sockets;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.Controllers.Api;

/// <summary>
/// Service Status Controller - Provides internal/external health checks for all platform services.
/// Internal checks use Docker network hostnames (db, redis, grcmvc).
/// External checks use host-mapped ports (localhost:5010, localhost:9092).
/// </summary>
[ApiController]
[Route("status")]
[AllowAnonymous]
public class StatusController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<StatusController> _logger;

    public StatusController(
        IHttpClientFactory httpClientFactory,
        IWebHostEnvironment env,
        ILogger<StatusController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// Returns the endpoints configuration JSON
    /// </summary>
    [HttpGet("endpoints")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Endpoints()
    {
        var jsonPath = GetEndpointsJsonPath();
        if (!System.IO.File.Exists(jsonPath))
        {
            _logger.LogWarning("endpoints.json not found at: {Path}", jsonPath);
            return NotFound(new { error = "endpoints.json not found", path = jsonPath });
        }

        return Content(System.IO.File.ReadAllText(jsonPath), "application/json");
    }

    /// <summary>
    /// Check all services health status
    /// </summary>
    /// <param name="mode">internal (Docker network) or external (host ports)</param>
    [HttpGet("check")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Check([FromQuery] string mode = "internal")
    {
        var jsonPath = GetEndpointsJsonPath();
        if (!System.IO.File.Exists(jsonPath))
        {
            _logger.LogWarning("endpoints.json not found at: {Path}", jsonPath);
            return NotFound(new { error = "endpoints.json not found", path = jsonPath });
        }

        try
        {
            var doc = JsonDocument.Parse(await System.IO.File.ReadAllTextAsync(jsonPath));
            var services = doc.RootElement.GetProperty("services").EnumerateArray();

            var results = new List<object>();
            foreach (var svc in services)
            {
                results.Add(await CheckOneAsync(svc, mode));
            }

            return Ok(new
            {
                mode,
                timestampUtc = DateTime.UtcNow,
                totalServices = results.Count,
                healthy = results.Count(r => GetStatusFromResult(r) == "GREEN"),
                unhealthy = results.Count(r => GetStatusFromResult(r) == "RED"),
                results
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse endpoints.json");
            return StatusCode(500, new { error = "Failed to parse endpoints configuration", details = ex.Message });
        }
    }

    private string GetEndpointsJsonPath()
    {
        // Path relative to src/GrcMvc: ../../infra/endpoints.json
        return Path.Combine(_env.ContentRootPath, "..", "..", "infra", "endpoints.json");
    }

    private static string GetStatusFromResult(object result)
    {
        var json = JsonSerializer.Serialize(result);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("status", out var status) ? status.GetString() ?? "RED" : "RED";
    }

    private async Task<object> CheckOneAsync(JsonElement svc, string mode)
    {
        var id = svc.GetProperty("id").GetString() ?? "";
        var name = svc.GetProperty("name").GetString() ?? id;
        var group = svc.GetProperty("group").GetString() ?? "";
        var checkType = svc.GetProperty("checkType").GetString() ?? "http";
        var timeoutMs = svc.TryGetProperty("timeoutMs", out var t) ? t.GetInt32() : 4000;

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            if (checkType == "http")
            {
                return await CheckHttpAsync(svc, mode, id, name, group, timeoutMs, sw);
            }
            else
            {
                return await CheckTcpAsync(svc, mode, id, name, group, timeoutMs, sw);
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "Health check failed for {ServiceId} ({Mode})", id, mode);
            return new
            {
                id,
                name,
                group,
                checkType,
                status = "RED",
                latencyMs = sw.ElapsedMilliseconds,
                error = ex.Message
            };
        }
    }

    private async Task<object> CheckHttpAsync(
        JsonElement svc, string mode, string id, string name,
        string group, int timeoutMs, System.Diagnostics.Stopwatch sw)
    {
        var baseUrl = mode == "external"
            ? svc.GetProperty("externalBaseUrl").GetString()
            : svc.GetProperty("internalBaseUrl").GetString();

        var healthPath = svc.TryGetProperty("healthPath", out var hp)
            ? hp.GetString() : "/";
        var url = (baseUrl ?? "").TrimEnd('/') + (healthPath ?? "/");

        // In container, external localhost must be rewritten to host.docker.internal
        url = RewriteLocalhostForContainer(url, mode);

        var client = _httpClientFactory.CreateClient("status-check");
        client.Timeout = TimeSpan.FromMilliseconds(timeoutMs);

        using var resp = await client.GetAsync(url);
        sw.Stop();

        var code = (int)resp.StatusCode;
        var status = code >= 200 && code < 400 ? "GREEN"
                   : (code == 401 || code == 403) ? "YELLOW"
                   : "RED";

        return new
        {
            id,
            name,
            group,
            checkType = "http",
            url,
            status,
            httpStatus = code,
            latencyMs = sw.ElapsedMilliseconds
        };
    }

    private async Task<object> CheckTcpAsync(
        JsonElement svc, string mode, string id, string name,
        string group, int timeoutMs, System.Diagnostics.Stopwatch sw)
    {
        var host = mode == "external"
            ? svc.GetProperty("externalHost").GetString()
            : svc.GetProperty("internalHost").GetString();

        var port = mode == "external"
            ? svc.GetProperty("externalPort").GetInt32()
            : svc.GetProperty("internalPort").GetInt32();

        // In container, external localhost must be rewritten to host.docker.internal
        if (mode == "external" && string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
            host = "host.docker.internal";

        using var tcp = new TcpClient();
        var connectTask = tcp.ConnectAsync(host!, port);
        var done = await Task.WhenAny(connectTask, Task.Delay(timeoutMs));
        sw.Stop();

        if (done != connectTask || !tcp.Connected)
        {
            return new
            {
                id,
                name,
                group,
                checkType = "tcp",
                host,
                port,
                status = "RED",
                latencyMs = sw.ElapsedMilliseconds,
                error = "Connection timeout or refused"
            };
        }

        return new
        {
            id,
            name,
            group,
            checkType = "tcp",
            host,
            port,
            status = "GREEN",
            latencyMs = sw.ElapsedMilliseconds
        };
    }

    private static string RewriteLocalhostForContainer(string url, string mode)
    {
        if (mode != "external") return url;
        return url.Replace("http://localhost", "http://host.docker.internal")
                  .Replace("https://localhost", "https://host.docker.internal");
    }
}
