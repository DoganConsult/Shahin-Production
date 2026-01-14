using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.Tests.Fixtures;

/// <summary>
/// Factory for creating mock services used in controller tests
/// </summary>
public static class MockServiceFactory
{
    /// <summary>
    /// Creates a mock logger for the specified type
    /// </summary>
    public static Mock<ILogger<T>> CreateLogger<T>()
    {
        return new Mock<ILogger<T>>();
    }

    /// <summary>
    /// Creates a mock ITenantContextService
    /// </summary>
    public static Mock<ITenantContextService> CreateTenantContextService(Guid? tenantId = null, bool isAuthenticated = true)
    {
        var mock = new Mock<ITenantContextService>();
        var actualTenantId = tenantId ?? Guid.NewGuid();

        mock.Setup(x => x.GetCurrentTenantId()).Returns(actualTenantId);
        mock.Setup(x => x.IsAuthenticated()).Returns(isAuthenticated);
        mock.Setup(x => x.HasTenantContext()).Returns(true);
        mock.Setup(x => x.GetCurrentUserId()).Returns(Guid.NewGuid().ToString());
        mock.Setup(x => x.GetCurrentUserName()).Returns("testuser");

        return mock;
    }

    /// <summary>
    /// Creates a mock HttpContext with optional user claims
    /// </summary>
    public static Mock<HttpContext> CreateHttpContext(
        bool isAuthenticated = false,
        string? userId = null,
        string? userName = null,
        string[]? roles = null,
        string host = "localhost")
    {
        var claims = new List<Claim>();

        if (isAuthenticated)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId ?? Guid.NewGuid().ToString()));
            claims.Add(new Claim(ClaimTypes.Name, userName ?? "testuser"));
            claims.Add(new Claim(ClaimTypes.Email, $"{userName ?? "testuser"}@test.com"));

            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }
        }

        var identity = new ClaimsIdentity(claims, isAuthenticated ? "Test" : null);
        var principal = new ClaimsPrincipal(identity);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.User).Returns(principal);

        // Setup Request
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(x => x.Host).Returns(new HostString(host));
        mockRequest.Setup(x => x.Path).Returns("/");
        mockRequest.Setup(x => x.Headers).Returns(new HeaderDictionary());
        mockHttpContext.Setup(x => x.Request).Returns(mockRequest.Object);

        // Setup Response
        var mockResponse = new Mock<HttpResponse>();
        mockResponse.Setup(x => x.Headers).Returns(new HeaderDictionary());
        mockHttpContext.Setup(x => x.Response).Returns(mockResponse.Object);

        // Setup Connection
        var mockConnection = new Mock<ConnectionInfo>();
        mockConnection.Setup(x => x.RemoteIpAddress).Returns(System.Net.IPAddress.Parse("127.0.0.1"));
        mockHttpContext.Setup(x => x.Connection).Returns(mockConnection.Object);

        // Setup TraceIdentifier
        mockHttpContext.Setup(x => x.TraceIdentifier).Returns(Guid.NewGuid().ToString());

        return mockHttpContext;
    }

    /// <summary>
    /// Creates a ControllerContext with mock HttpContext
    /// </summary>
    public static ControllerContext CreateControllerContext(
        bool isAuthenticated = false,
        string? userId = null,
        string? userName = null,
        string host = "localhost")
    {
        var httpContext = CreateHttpContext(isAuthenticated, userId, userName, host: host);

        return new ControllerContext
        {
            HttpContext = httpContext.Object
        };
    }

    /// <summary>
    /// Sets up a controller with the necessary context
    /// </summary>
    public static T SetupController<T>(T controller, bool isAuthenticated = false, string host = "localhost") where T : Controller
    {
        var controllerContext = CreateControllerContext(isAuthenticated, host: host);
        controller.ControllerContext = controllerContext;

        // Setup TempData
        var tempData = new TempDataDictionary(controllerContext.HttpContext, Mock.Of<ITempDataProvider>());
        controller.TempData = tempData;

        return controller;
    }

    /// <summary>
    /// Creates a mock IEmailService
    /// </summary>
    public static Mock<IEmailService> CreateEmailService()
    {
        var mock = new Mock<IEmailService>();
        mock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    /// <summary>
    /// Creates a mock INotificationService
    /// </summary>
    public static Mock<INotificationService> CreateNotificationService()
    {
        var mock = new Mock<INotificationService>();
        mock.Setup(x => x.SendNotificationAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Guid>(), It.IsAny<Dictionary<string, object>?>()))
            .ReturnsAsync(new NotificationResult { IsSuccess = true });
        return mock;
    }
}

/// <summary>
/// Extension methods for setting up test controllers
/// </summary>
public static class ControllerTestExtensions
{
    /// <summary>
    /// Sets up the controller as an unauthenticated user
    /// </summary>
    public static T AsAnonymous<T>(this T controller) where T : Controller
    {
        return MockServiceFactory.SetupController(controller, isAuthenticated: false);
    }

    /// <summary>
    /// Sets up the controller as an authenticated user
    /// </summary>
    public static T AsAuthenticated<T>(this T controller, string host = "localhost") where T : Controller
    {
        return MockServiceFactory.SetupController(controller, isAuthenticated: true, host: host);
    }
}
