using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Regulatory Framework entity for managing compliance frameworks
    /// </summary>
    public class Framework : BaseEntity
    {
        public override string ResourceType => "RegulatoryFramework";

        public Guid? WorkspaceId { get; set; }
        public string FrameworkCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Jurisdiction { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Regulatory, Industry, International
        public string Status { get; set; } = "Active"; // Active, Inactive, Deprecated
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string Website { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public bool IsMandatory { get; set; } = false;
    }
}
