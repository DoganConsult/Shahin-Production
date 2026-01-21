using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// LLM Configuration per tenant
    /// Supports OpenAI, Azure OpenAI, Local LLMs, etc.
    /// </summary>
    public class LlmConfiguration : BaseEntity
    {
        public Guid? TenantId { get; set; }

        /// <summary>
        /// Provider: "OpenAI", "AzureOpenAI", "Local", "Custom"
        /// </summary>
        public string Provider { get; set; } = "OpenAI";

        /// <summary>
        /// API endpoint URL
        /// </summary>
        public string ApiEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// API key (encrypted)
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Model name (gpt-4, gpt-3.5-turbo, local-model, etc.)
        /// </summary>
        public string ModelName { get; set; } = "gpt-3.5-turbo";

        /// <summary>
        /// Is this configuration active?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Max tokens per request
        /// </summary>
        public int MaxTokens { get; set; } = 2000;

        /// <summary>
        /// Temperature (0-1, higher = more creative)
        /// </summary>
        public decimal Temperature { get; set; } = 0.7m;

        /// <summary>
        /// Enable LLM for this tenant?
        /// </summary>
        public bool EnabledForTenant { get; set; } = true;

        /// <summary>
        /// Usage limit per month (0 = unlimited)
        /// </summary>
        public int MonthlyUsageLimit { get; set; } = 0;

        /// <summary>
        /// Current month usage count
        /// </summary>
        public int CurrentMonthUsage { get; set; } = 0;

        /// <summary>
        /// Last reset date
        /// </summary>
        public DateTime? LastUsageResetDate { get; set; }

        public DateTime ConfiguredDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual Tenant? Tenant { get; set; }
    }
}
