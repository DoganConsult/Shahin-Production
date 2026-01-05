using System;
using System.Collections.Generic;

namespace GrcMvc.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object for Tenant information
    /// </summary>
    public class TenantDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TenantSlug { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string SubscriptionTier { get; set; } = "Professional";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public string? ActivationToken { get; set; }
    }
}
