using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Tracks owner-created tenants and credential delivery for audit trail
    /// </summary>
    public class OwnerTenantCreation : BaseEntity
    {
        public string OwnerId { get; set; } = string.Empty; // ApplicationUser.Id of owner (string from Identity)
        public Guid TenantId { get; set; }
        public string AdminUsername { get; set; } = string.Empty;
        public DateTime CredentialsExpiresAt { get; set; }
        public string DeliveryMethod { get; set; } = "Manual"; // Email, Manual, Download
        public bool CredentialsDelivered { get; set; } = false;
        public DateTime? DeliveredAt { get; set; }
        public string? DeliveryNotes { get; set; }
        
        // Navigation properties
        public virtual ApplicationUser Owner { get; set; } = null!;
        public virtual Tenant Tenant { get; set; } = null!;
    }
}
