using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Regulator entity for tracking regulatory bodies and authorities
    /// </summary>
    public class Regulator : BaseEntity
    {
        public override string ResourceType => "Regulator";

        public Guid? WorkspaceId { get; set; }
        public string RegulatorCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Jurisdiction { get; set; } = string.Empty; // Country/Region
        public string Type { get; set; } = string.Empty; // Government, Industry, International
        public string Website { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Status { get; set; } = "Active"; // Active, Inactive
        public bool IsPrimary { get; set; } = false;
        public string Notes { get; set; } = string.Empty;
    }
}
