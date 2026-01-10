namespace GrcMvc.Configuration;

/// <summary>
/// Stripe Configuration Settings
/// ASP.NET Core Best Practice: IOptions pattern with validation
/// </summary>
public class StripeSettings
{
    public const string SectionName = "Stripe";

    /// <summary>
    /// Stripe Secret Key (starts with sk_live_ or sk_test_)
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Stripe Publishable Key (for frontend, starts with pk_live_ or pk_test_)
    /// </summary>
    public string PublishableKey { get; set; } = string.Empty;

    /// <summary>
    /// Stripe Webhook Signing Secret (starts with whsec_)
    /// </summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>
    /// Price IDs for each plan and billing cycle
    /// </summary>
    public StripePriceIds? PriceIds { get; set; }

    /// <summary>
    /// Whether to use test mode
    /// </summary>
    public bool IsTestMode => SecretKey?.StartsWith("sk_test_") ?? true;

    /// <summary>
    /// Validate configuration
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(SecretKey) &&
               (SecretKey.StartsWith("sk_live_") || SecretKey.StartsWith("sk_test_"));
    }
}

/// <summary>
/// Stripe Price IDs for subscription plans
/// </summary>
public class StripePriceIds
{
    // Starter Plan
    public string? StarterMonthly { get; set; }
    public string? StarterAnnual { get; set; }

    // Professional Plan
    public string? ProfessionalMonthly { get; set; }
    public string? ProfessionalAnnual { get; set; }

    // Enterprise Plan
    public string? EnterpriseMonthly { get; set; }
    public string? EnterpriseAnnual { get; set; }
}
