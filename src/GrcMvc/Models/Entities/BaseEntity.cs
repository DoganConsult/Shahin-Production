using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Base entity class with common properties
    /// </summary>
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? TenantId { get; set; } // Multi-tenant support
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}