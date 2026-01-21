using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Asset - Organization's assets for recognition and scoping
    /// Ingested from: ERP, IAM, Cloud, CMDB, Security tools
    /// Used for: Auto-deriving applicable controls based on asset characteristics
    /// </summary>
    public class Asset : BaseEntity
    {
        public Guid TenantId { get; set; }
        public string AssetCode { get; set; } = string.Empty; // ASSET-001, SRV-WEB-01
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Asset type: Application, Server, Database, Network, Vendor, Process, Data, Person, Location
        /// </summary>
        public string AssetType { get; set; } = string.Empty;

        /// <summary>
        /// Sub-type for more granular classification
        /// </summary>
        public string SubType { get; set; } = string.Empty; // WebApp, API, PostgreSQL, Firewall, etc.

        /// <summary>
        /// External system ID (from source system)
        /// </summary>
        public string SystemId { get; set; } = string.Empty;

        /// <summary>
        /// Source system: ERP, Azure_AD, AWS, GCP, ServiceNow, Manual
        /// </summary>
        public string SourceSystem { get; set; } = "Manual";

        // ===== CLASSIFICATION =====
        /// <summary>
        /// Criticality tier: T1 (Critical), T2 (High), T3 (Medium), T4 (Low)
        /// T1 assets get stricter controls and higher test frequency
        /// </summary>
        public string Criticality { get; set; } = "T3";

        /// <summary>
        /// Data classification: Public, Internal, Confidential, Restricted, PII, PCI, PHI
        /// If any asset has data_class=PCI → apply PCI overlay
        /// </summary>
        public string DataClassification { get; set; } = "Internal";

        /// <summary>
        /// Additional data types processed (comma-separated)
        /// </summary>
        public string DataTypes { get; set; } = string.Empty; // PII, Financial, Health, Biometric

        // ===== OWNERSHIP =====
        /// <summary>
        /// Owner user ID
        /// </summary>
        public Guid? OwnerUserId { get; set; }

        /// <summary>
        /// Owner team ID
        /// </summary>
        public Guid? OwnerTeamId { get; set; }

        /// <summary>
        /// Business owner (department/unit)
        /// </summary>
        public string BusinessOwner { get; set; } = string.Empty;

        /// <summary>
        /// Technical owner
        /// </summary>
        public string TechnicalOwner { get; set; } = string.Empty;

        // ===== LOCATION & ENVIRONMENT =====
        /// <summary>
        /// Hosting model: OnPremise, Cloud, Hybrid, SaaS
        /// </summary>
        public string HostingModel { get; set; } = string.Empty;

        /// <summary>
        /// Cloud provider if applicable: AWS, Azure, GCP, Alibaba, Other
        /// </summary>
        public string CloudProvider { get; set; } = string.Empty;

        /// <summary>
        /// Environment: Production, Staging, Development, DR, Test
        /// </summary>
        public string Environment { get; set; } = "Production";

        /// <summary>
        /// Geographic location/region
        /// </summary>
        public string Location { get; set; } = string.Empty;

        // ===== TAGS & METADATA =====
        /// <summary>
        /// Tags for flexible filtering (JSON array)
        /// e.g., ["market:retail", "BU:finance", "project:alpha"]
        /// </summary>
        public string TagsJson { get; set; } = "[]";

        /// <summary>
        /// Custom attributes (JSON object)
        /// </summary>
        public string AttributesJson { get; set; } = "{}";

        // ===== COMPLIANCE RELEVANCE =====
        /// <summary>
        /// Is this asset in scope for compliance assessments?
        /// </summary>
        public bool IsInScope { get; set; } = true;

        /// <summary>
        /// Applicable frameworks derived from asset characteristics
        /// </summary>
        public string ApplicableFrameworks { get; set; } = string.Empty; // NCA-ECC, PCI-DSS, PDPL

        /// <summary>
        /// Last risk assessment date
        /// </summary>
        public DateTime? LastRiskAssessmentDate { get; set; }

        /// <summary>
        /// Risk score (0-100)
        /// </summary>
        public int? RiskScore { get; set; }

        // ===== LIFECYCLE =====
        public DateTime? CommissionedDate { get; set; }
        public DateTime? DecommissionedDate { get; set; }
        public string Status { get; set; } = "Active"; // Active, Inactive, Decommissioned, Pending

        // ===== SYNC TRACKING =====
        public DateTime? LastSyncDate { get; set; }
        public string LastSyncStatus { get; set; } = string.Empty;

        // Navigation
        public virtual Tenant Tenant { get; set; } = null!;
        public virtual Team? OwnerTeam { get; set; }
    }
}
