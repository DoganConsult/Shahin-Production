using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.Schemas;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.X509Certificates;
using System.Security.Claims;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// SAML 2.0 Service Implementation
/// Handles SAML authentication for enterprise SSO
/// </summary>
public class SamlService : ISamlService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SamlService> _logger;
    private readonly Saml2Configuration _samlConfig;

    public SamlService(
        IConfiguration configuration,
        ILogger<SamlService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _samlConfig = LoadSamlConfiguration();
    }

    public bool IsEnabled => _configuration.GetValue<bool>("Saml:Enabled", false);

    public Saml2Configuration GetConfiguration()
    {
        return _samlConfig;
    }

    private Saml2Configuration LoadSamlConfiguration()
    {
        var config = new Saml2Configuration
        {
            Issuer = _configuration["Saml:Issuer"] ?? "https://portal.shahin-ai.com",
            SingleSignOnDestination = new Uri(_configuration["Saml:IdpSsoUrl"] ?? "https://idp.example.com/sso"),
            SingleLogoutDestination = new Uri(_configuration["Saml:IdpSloUrl"] ?? "https://idp.example.com/slo"),
            SignatureAlgorithm = _configuration["Saml:SignatureAlgorithm"] ?? "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256",
            CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.ChainTrust,
            RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.Online
        };

        // Load SP (Service Provider) certificate
        var spCertPath = _configuration["Saml:SpCertificatePath"];
        var spCertPassword = _configuration["Saml:SpCertificatePassword"];
        if (!string.IsNullOrEmpty(spCertPath) && File.Exists(spCertPath))
        {
            config.SigningCertificate = new X509Certificate2(spCertPath, spCertPassword);
        }

        // Load IdP (Identity Provider) certificate
        var idpCertPath = _configuration["Saml:IdpCertificatePath"];
        if (!string.IsNullOrEmpty(idpCertPath) && File.Exists(idpCertPath))
        {
            config.AllowedAudienceUris.Add(config.Issuer);
            var idpCert = new X509Certificate2(idpCertPath);
            config.SignatureValidationCertificates.Add(idpCert);
        }

        // Configure name identifier format
        config.NameIdFormats.Add(Saml2NameIdentifierFormats.Unspecified);

        // Configure requested attributes
        config.RequestedAuthnContext = new RequestedAuthnContext
        {
            Comparison = AuthnContextComparisonTypes.Exact,
            AuthnContextClassRef = new[] { AuthnContextClassTypes.PasswordProtectedTransport.OriginalString }
        };

        return config;
    }

    public async Task<Saml2AuthnRequest> CreateAuthnRequestAsync(string returnUrl)
    {
        try
        {
            var saml2AuthnRequest = new Saml2AuthnRequest(_samlConfig)
            {
                Destination = _samlConfig.SingleSignOnDestination,
                AssertionConsumerServiceUrl = new Uri(_configuration["Saml:AssertionConsumerServiceUrl"] ?? "https://portal.shahin-ai.com/saml/acs"),
                Issuer = _samlConfig.Issuer
            };

            return await Task.FromResult(saml2AuthnRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SAML authentication request");
            throw;
        }
    }

    public async Task<Saml2ClaimsPrincipal?> ProcessResponseAsync(string samlResponse)
    {
        try
        {
            var binding = new Saml2PostBinding();
            var saml2AuthnResponse = new Saml2AuthnResponse(_samlConfig);

            binding.ReadSamlResponse(samlResponse, saml2AuthnResponse);

            if (saml2AuthnResponse.Status == Saml2StatusCodes.Success)
            {
                binding.Unbind(samlResponse, saml2AuthnResponse);
                var claimsPrincipal = new Saml2ClaimsPrincipal(saml2AuthnResponse.ClaimsIdentity);

                _logger.LogInformation("SAML authentication successful for user: {NameId}", 
                    saml2AuthnResponse.ClaimsIdentity.Name);

                return await Task.FromResult(claimsPrincipal);
            }
            else
            {
                _logger.LogWarning("SAML authentication failed with status: {Status}", saml2AuthnResponse.Status);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SAML response");
            return null;
        }
    }
}

/// <summary>
/// SAML Service Interface
/// </summary>
public interface ISamlService
{
    bool IsEnabled { get; }
    Saml2Configuration GetConfiguration();
    Task<Saml2AuthnRequest> CreateAuthnRequestAsync(string returnUrl);
    Task<Saml2ClaimsPrincipal?> ProcessResponseAsync(string samlResponse);
}

/// <summary>
/// SAML Claims Principal wrapper
/// </summary>
public class Saml2ClaimsPrincipal
{
    public ClaimsIdentity ClaimsIdentity { get; }
    public string? NameId => ClaimsIdentity.Name;
    public Dictionary<string, string> Attributes { get; }

    public Saml2ClaimsPrincipal(ClaimsIdentity claimsIdentity)
    {
        ClaimsIdentity = claimsIdentity;
        Attributes = new Dictionary<string, string>();
        
        foreach (var claim in claimsIdentity.Claims)
        {
            Attributes[claim.Type] = claim.Value;
        }
    }
}
