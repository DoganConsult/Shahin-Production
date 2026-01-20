using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace GrcMvc.Authentication
{
    /// <summary>
    /// AM-02: API Key authentication handler.
    /// Authenticates requests using API key in header or query string.
    /// </summary>
    public class ApiKeyAuthHandler : AuthenticationHandler<ApiKeyAuthOptions>
    {
        private readonly IApiKeyService _apiKeyService;

        public ApiKeyAuthHandler(
            IOptionsMonitor<ApiKeyAuthOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IApiKeyService apiKeyService)
            : base(options, logger, encoder)
        {
            _apiKeyService = apiKeyService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Try to get API key from various sources
            var apiKey = GetApiKeyFromRequest();

            if (string.IsNullOrEmpty(apiKey))
            {
                return AuthenticateResult.NoResult();
            }

            // Get client IP
            var ipAddress = Context.Connection.RemoteIpAddress?.ToString();

            // Validate the key
            var result = await _apiKeyService.ValidateKeyAsync(apiKey, ipAddress);

            if (!result.IsValid)
            {
                Logger.LogWarning("API key authentication failed: {ErrorCode} - {ErrorMessage}",
                    result.ErrorCode, result.ErrorMessage);

                return AuthenticateResult.Fail(result.ErrorMessage ?? "Invalid API key");
            }

            // Create claims principal
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.KeyId.ToString()!),
                new Claim("api_key_id", result.KeyId.ToString()!),
                new Claim("api_key_name", result.KeyName ?? "Unknown"),
                new Claim(ClaimTypes.AuthenticationMethod, "ApiKey")
            };

            // Add tenant claim if present
            if (result.TenantId.HasValue)
            {
                claims.Add(new Claim("tenant_id", result.TenantId.Value.ToString()));
            }

            // Add scope claims
            if (result.Scopes != null)
            {
                foreach (var scope in result.Scopes)
                {
                    claims.Add(new Claim("scope", scope));
                }
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            Logger.LogDebug("API key authenticated successfully: {KeyId}", result.KeyId);

            return AuthenticateResult.Success(ticket);
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            Response.Headers["WWW-Authenticate"] = $"ApiKey realm=\"{Options.Realm}\"";

            return Task.CompletedTask;
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        private string? GetApiKeyFromRequest()
        {
            // Try Authorization header with ApiKey scheme
            if (Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var authValue = authHeader.ToString();
                if (authValue.StartsWith("ApiKey ", StringComparison.OrdinalIgnoreCase))
                {
                    return authValue.Substring(7).Trim();
                }
                if (authValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) &&
                    authValue.Substring(7).StartsWith("sk_"))
                {
                    // Also accept API keys in Bearer format if they look like our keys
                    return authValue.Substring(7).Trim();
                }
            }

            // Try custom header
            if (Request.Headers.TryGetValue(Options.HeaderName, out var headerValue) &&
                !string.IsNullOrEmpty(headerValue))
            {
                return headerValue.ToString();
            }

            // Try query parameter (less secure, but sometimes needed)
            if (Options.AllowQueryParameter &&
                Request.Query.TryGetValue(Options.QueryParameterName, out var queryValue) &&
                !string.IsNullOrEmpty(queryValue))
            {
                Logger.LogWarning("API key provided in query string - this is less secure than using headers");
                return queryValue.ToString();
            }

            return null;
        }
    }

    /// <summary>
    /// Options for API key authentication.
    /// </summary>
    public class ApiKeyAuthOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// Scheme name for API key authentication.
        /// </summary>
        public const string DefaultScheme = "ApiKey";

        /// <summary>
        /// Header name to look for API key.
        /// </summary>
        public string HeaderName { get; set; } = "X-API-Key";

        /// <summary>
        /// Query parameter name for API key (if allowed).
        /// </summary>
        public string QueryParameterName { get; set; } = "api_key";

        /// <summary>
        /// Whether to allow API key in query string.
        /// </summary>
        public bool AllowQueryParameter { get; set; } = false;

        /// <summary>
        /// Realm for WWW-Authenticate header.
        /// </summary>
        public string Realm { get; set; } = "GrcMvc API";
    }

    /// <summary>
    /// Extension methods for configuring API key authentication.
    /// </summary>
    public static class ApiKeyAuthExtensions
    {
        /// <summary>
        /// Add API key authentication to the authentication builder.
        /// </summary>
        public static AuthenticationBuilder AddApiKeyAuth(
            this AuthenticationBuilder builder,
            Action<ApiKeyAuthOptions>? configureOptions = null)
        {
            return builder.AddScheme<ApiKeyAuthOptions, ApiKeyAuthHandler>(
                ApiKeyAuthOptions.DefaultScheme,
                configureOptions ?? (options => { }));
        }

        /// <summary>
        /// Add API key authentication with a specific scheme name.
        /// </summary>
        public static AuthenticationBuilder AddApiKeyAuth(
            this AuthenticationBuilder builder,
            string scheme,
            Action<ApiKeyAuthOptions>? configureOptions = null)
        {
            return builder.AddScheme<ApiKeyAuthOptions, ApiKeyAuthHandler>(
                scheme,
                configureOptions ?? (options => { }));
        }
    }

    /// <summary>
    /// Attribute to require API key authentication on controllers or actions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireApiKeyAttribute : Microsoft.AspNetCore.Authorization.AuthorizeAttribute
    {
        /// <summary>
        /// Required scope for this endpoint.
        /// </summary>
        public string? Scope { get; set; }

        public RequireApiKeyAttribute() : base()
        {
            AuthenticationSchemes = ApiKeyAuthOptions.DefaultScheme;
        }

        public RequireApiKeyAttribute(string scope) : this()
        {
            Scope = scope;
            Policy = $"ApiKeyScope:{scope}";
        }
    }
}
