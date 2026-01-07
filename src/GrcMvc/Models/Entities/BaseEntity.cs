using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using GrcMvc.Models.Interfaces;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Base entity class with common properties and governance metadata support
    /// </summary>
    public abstract class BaseEntity : IGovernedResource
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? TenantId { get; set; } // Multi-tenant support
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Alias properties for backward compatibility
        [NotMapped]
        public DateTime CreatedAt
        {
            get => CreatedDate;
            set => CreatedDate = value;
        }

        [NotMapped]
        public DateTime? UpdatedAt
        {
            get => ModifiedDate;
            set => ModifiedDate = value;
        }

        /// <summary>
        /// Row version for optimistic concurrency control
        /// Prevents lost updates when multiple users edit the same record
        /// </summary>
        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // =====================================================================
        // GOVERNANCE METADATA (IGovernedResource Implementation)
        // =====================================================================

        /// <summary>
        /// Resource type identifier for policy matching
        /// Override in derived classes to specify the resource type
        /// </summary>
        [NotMapped]
        public virtual string ResourceType => GetType().Name;

        /// <summary>
        /// Owner of the resource (team or individual identifier)
        /// Required by REQUIRE_OWNER policy rule
        /// </summary>
        public string? Owner { get; set; }

        /// <summary>
        /// Data classification level: public, internal, confidential, restricted
        /// Required by REQUIRE_DATA_CLASSIFICATION policy rule
        /// </summary>
        public string? DataClassification { get; set; }

        /// <summary>
        /// JSON-serialized metadata labels for policy evaluation
        /// Stored as JSON in database, exposed as Dictionary in code
        /// </summary>
        public string? LabelsJson { get; set; }

        /// <summary>
        /// Additional metadata labels for policy evaluation
        /// Automatically serialized to/from LabelsJson
        /// </summary>
        [NotMapped]
        public Dictionary<string, string> Labels
        {
            get
            {
                if (string.IsNullOrWhiteSpace(LabelsJson))
                    return new Dictionary<string, string>();
                
                try
                {
                    return JsonSerializer.Deserialize<Dictionary<string, string>>(LabelsJson) 
                           ?? new Dictionary<string, string>();
                }
                catch
                {
                    return new Dictionary<string, string>();
                }
            }
            set
            {
                LabelsJson = value == null || value.Count == 0 
                    ? null 
                    : JsonSerializer.Serialize(value);
            }
        }
    }
}