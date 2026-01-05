using System;
using System.Collections.Generic;
using GrcMvc.Models.Entities;

namespace GrcMvc.Models.DTOs
{
    /// <summary>
    /// DTO for tenant onboarding scope (applicable baselines, packages, templates).
    /// </summary>
    public class OnboardingScopeDto
    {
        public Guid TenantId { get; set; }
        public List<BaselineDto> ApplicableBaselines { get; set; } = new List<BaselineDto>();
        public List<PackageDto> ApplicablePackages { get; set; } = new List<PackageDto>();
        public List<TemplateDto> ApplicableTemplates { get; set; } = new List<TemplateDto>();
        public List<string> ApplicableFrameworks { get; set; } = new List<string>();
        public List<string> RecommendedPackages { get; set; } = new List<string>();
        public List<string> ApplicableTemplatesList { get; set; } = new List<string>();
        public List<string> AuditPackages { get; set; } = new List<string>();
        public DateTime RetrievedAt { get; set; }
    }

    /// <summary>
    /// DTO for baseline in scope.
    /// </summary>
    public class BaselineDto
    {
        public string BaselineCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ReasonJson { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for package in scope.
    /// </summary>
    public class PackageDto
    {
        public string PackageCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ReasonJson { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for template in scope.
    /// </summary>
    public class TemplateDto
    {
        public string TemplateCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ReasonJson { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for creating a new tenant during signup.
    /// </summary>
    public class CreateTenantDto
    {
        public string OrganizationName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string TenantSlug { get; set; } = string.Empty;
        public string SubscriptionTier { get; set; } = "Professional";
        public string Country { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for activating a tenant.
    /// </summary>
    public class ActivateTenantDto
    {
        public string TenantSlug { get; set; } = string.Empty;
        public string ActivationToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for organization profile setup during onboarding.
    /// </summary>
    public class OrganizationProfileDto
    {
        public Guid TenantId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public string OrgType { get; set; } = string.Empty;
        public string OrganizationType { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string DataTypes { get; set; } = string.Empty;
        public string HostingModel { get; set; } = string.Empty;
        public string OrganizationSize { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string ComplianceMaturity { get; set; } = string.Empty;
        public string Maturity { get; set; } = string.Empty;
        public bool IsCriticalInfrastructure { get; set; }
        public string Vendors { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for plan creation during scope delivery.
    /// </summary>
    public class CreatePlanDto
    {
        public Guid TenantId { get; set; }
        public string PlanCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PlanType { get; set; } = string.Empty; // QuickScan, Full, Remediation
        public DateTime StartDate { get; set; }
        public DateTime TargetEndDate { get; set; }
        public Guid? RulesetVersionId { get; set; }
    }
}
