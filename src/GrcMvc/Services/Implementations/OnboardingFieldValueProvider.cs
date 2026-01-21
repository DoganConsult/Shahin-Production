using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Field value provider implementation for OnboardingWizard entity
    /// Maps canonical field IDs (e.g., "SF.S1.organization_name") to actual entity properties
    /// </summary>
    public class OnboardingFieldValueProvider : IFieldValueProvider
    {
        private readonly OnboardingWizard _wizard;
        private readonly Dictionary<string, Func<object?>> _fieldMappers;

        public OnboardingFieldValueProvider(OnboardingWizard wizard)
        {
            _wizard = wizard ?? throw new ArgumentNullException(nameof(wizard));
            _fieldMappers = BuildFieldMappers();
        }

        public object? GetFieldValue(string fieldId)
        {
            if (_fieldMappers.TryGetValue(fieldId, out var mapper))
            {
                return mapper();
            }

            // Try to extract from JSON fields using dot notation
            return ExtractFromJsonFields(fieldId);
        }

        public bool HasFieldValue(string fieldId)
        {
            var value = GetFieldValue(fieldId);
            
            if (value == null)
                return false;

            if (value is string str)
                return !string.IsNullOrWhiteSpace(str);

            if (value is bool boolVal)
                return boolVal;

            if (value is IEnumerable<object> enumerable)
                return enumerable.Any();

            if (value is ICollection<object> collection)
                return collection.Count > 0;

            return true;
        }

        public HashSet<string> GetCollectedFieldIds()
        {
            var collectedIds = new HashSet<string>();

            foreach (var fieldId in _fieldMappers.Keys)
            {
                if (HasFieldValue(fieldId))
                {
                    collectedIds.Add(fieldId);
                }
            }

            return collectedIds;
        }

        private Dictionary<string, Func<object?>> BuildFieldMappers()
        {
            return new Dictionary<string, Func<object?>>(StringComparer.OrdinalIgnoreCase)
            {
                // ============================================================================
                // SIMPLE FLOW (SF) - Step 1 (SF.S1)
                // Note: Some fields like subscription_tier, terms_accepted may come from Tenant or AllAnswersJson
                // ============================================================================
                ["SF.S1.organization_name"] = () => _wizard.OrganizationLegalNameEn,
                ["SF.S1.admin_email"] = () => ExtractAdminEmail(),
                ["SF.S1.subscription_tier"] = () => ExtractStringFromJson(_wizard.AllAnswersJson, "subscription_tier") ?? ExtractStringFromJson(_wizard.AllAnswersJson, "SF.S1.subscription_tier"),
                ["SF.S1.country_of_operation"] = () => _wizard.CountryOfIncorporation,
                ["SF.S1.tos_accepted"] = () => ExtractBoolFromJson(_wizard.AllAnswersJson, "tos_accepted") ?? ExtractBoolFromJson(_wizard.AllAnswersJson, "SF.S1.tos_accepted") ?? false,
                ["SF.S1.privacy_policy_accepted"] = () => ExtractBoolFromJson(_wizard.AllAnswersJson, "privacy_policy_accepted") ?? ExtractBoolFromJson(_wizard.AllAnswersJson, "SF.S1.privacy_policy_accepted") ?? false,
                ["SF.S1.data_processing_consent"] = () => ExtractBoolFromJson(_wizard.AllAnswersJson, "data_processing_consent") ?? ExtractBoolFromJson(_wizard.AllAnswersJson, "SF.S1.data_processing_consent") ?? false,

                // ============================================================================
                // SIMPLE FLOW (SF) - Step 2 (SF.S2)
                // ============================================================================
                ["SF.S2.organization_type"] = () => _wizard.OrganizationType,
                ["SF.S2.sector"] = () => _wizard.IndustrySector ?? SafeDeserialize<List<string>>(ExtractStringFromJson(_wizard.AllAnswersJson, "industry_sectors") ?? "[]")?.FirstOrDefault() ?? string.Empty,
                ["SF.S2.primary_country"] = () => _wizard.CountryOfIncorporation,
                ["SF.S2.data_hosting_model"] = () => ExtractStringFromJson(_wizard.AllAnswersJson, "data_hosting_model") ?? ExtractStringFromJson(_wizard.AllAnswersJson, "SF.S2.data_hosting_model") ?? string.Empty,
                ["SF.S2.data_types_processed"] = () => SafeDeserialize<List<string>>(_wizard.DataTypesProcessedJson) ?? new List<string>(),
                ["SF.S2.organization_size"] = () => ExtractStringFromJson(_wizard.AllAnswersJson, "organization_size") ?? ExtractStringFromJson(_wizard.AllAnswersJson, "SF.S2.organization_size") ?? string.Empty,
                ["SF.S2.compliance_maturity"] = () => ExtractStringFromJson(_wizard.AllAnswersJson, "compliance_maturity") ?? ExtractStringFromJson(_wizard.AllAnswersJson, "SF.S2.compliance_maturity") ?? string.Empty,
                ["SF.S2.is_critical_infrastructure"] = () => ExtractBoolFromJson(_wizard.AllAnswersJson, "is_critical_infrastructure") ?? ExtractBoolFromJson(_wizard.AllAnswersJson, "SF.S2.is_critical_infrastructure"),
                ["SF.S2.third_party_vendors"] = () => ExtractStringFromJson(_wizard.AllAnswersJson, "third_party_vendors") ?? ExtractStringFromJson(_wizard.AllAnswersJson, "SF.S2.third_party_vendors"),

                // ============================================================================
                // SIMPLE FLOW (SF) - Step 3 (SF.S3) - Derived fields
                // Note: These may be stored in Tenant.ApplicableBaselines or AllAnswersJson
                // ============================================================================
                ["SF.S3.applicable_baselines"] = () => ExtractFromTenantBaselines() ?? SafeDeserialize<List<string>>(ExtractStringFromJson(_wizard.AllAnswersJson, "applicable_baselines") ?? "[]") ?? new List<string>(),
                ["SF.S3.baseline_reason"] = () => ExtractStringFromJson(_wizard.AllAnswersJson, "baseline_reason") ?? ExtractStringFromJson(_wizard.AllAnswersJson, "SF.S3.baseline_reason") ?? "{}",
                ["SF.S3.estimated_controls"] = () => ExtractIntFromJson(_wizard.AllAnswersJson, "estimated_controls") ?? ExtractIntFromJson(_wizard.AllAnswersJson, "SF.S3.estimated_controls"),
                ["SF.S3.recommended_packages"] = () => SafeDeserialize<List<string>>(ExtractStringFromJson(_wizard.AllAnswersJson, "recommended_packages") ?? "[]") ?? new List<string>(),
                ["SF.S3.recommended_templates"] = () => SafeDeserialize<List<string>>(ExtractStringFromJson(_wizard.AllAnswersJson, "recommended_templates") ?? "[]") ?? new List<string>(),
                ["SF.S3.user_confirmation"] = () => _wizard.AdoptDefaultBaseline, // Use AdoptDefaultBaseline as confirmation

                // ============================================================================
                // SIMPLE FLOW (SF) - Step 4 (SF.S4)
                // Note: Plan fields may come from Plan entity or AllAnswersJson
                // ============================================================================
                ["SF.S4.plan_name"] = () => ExtractStringFromJson(_wizard.AllAnswersJson, "plan_name") ?? ExtractStringFromJson(_wizard.AllAnswersJson, "SF.S4.plan_name") ?? string.Empty,
                ["SF.S4.description"] = () => ExtractStringFromJson(_wizard.AllAnswersJson, "plan_description") ?? ExtractStringFromJson(_wizard.AllAnswersJson, "SF.S4.description") ?? string.Empty,
                ["SF.S4.plan_type"] = () => ExtractStringFromJson(_wizard.AllAnswersJson, "plan_type") ?? ExtractStringFromJson(_wizard.AllAnswersJson, "SF.S4.plan_type") ?? string.Empty,
                ["SF.S4.start_date"] = () => ExtractDateTimeFromJson(_wizard.AllAnswersJson, "plan_start_date") ?? ExtractDateTimeFromJson(_wizard.AllAnswersJson, "SF.S4.start_date"),
                ["SF.S4.target_end_date"] = () => ExtractDateTimeFromJson(_wizard.AllAnswersJson, "plan_end_date") ?? ExtractDateTimeFromJson(_wizard.AllAnswersJson, "SF.S4.target_end_date"),

                // ============================================================================
                // WIZARD - Section A (W.A)
                // ============================================================================
                ["W.A.1.legal_name_en"] = () => _wizard.OrganizationLegalNameEn,
                ["W.A.2.legal_name_ar"] = () => _wizard.OrganizationLegalNameAr,
                ["W.A.3.trade_name"] = () => _wizard.TradeName,
                ["W.A.4.country_of_incorporation"] = () => _wizard.CountryOfIncorporation,
                ["W.A.5.operating_countries"] = () => SafeDeserialize<List<string>>(_wizard.OperatingCountriesJson) ?? new List<string>(),
                ["W.A.6.primary_hq_location"] = () => _wizard.PrimaryHqLocation,
                ["W.A.7.timezone"] = () => _wizard.DefaultTimezone,
                ["W.A.8.primary_language"] = () => _wizard.PrimaryLanguage,
                ["W.A.9.corporate_email_domains"] = () => SafeDeserialize<List<string>>(_wizard.CorporateEmailDomainsJson) ?? new List<string>(),
                ["W.A.10.domain_verification_method"] = () => _wizard.DomainVerificationMethod ?? string.Empty,
                ["W.A.11.organization_type"] = () => _wizard.OrganizationType ?? string.Empty,
                ["W.A.12.industry_sector"] = () => _wizard.IndustrySector ?? string.Empty,
                ["W.A.13.data_residency_requirements"] = () => _wizard.HasDataResidencyRequirement ? SafeDeserialize<List<string>>(_wizard.DataResidencyCountriesJson)?.FirstOrDefault() ?? "yes" : "no",

                // ============================================================================
                // WIZARD - Section B (W.B) - Assurance Objective
                // ============================================================================
                ["W.B.1.primary_driver"] = () => _wizard.PrimaryDriver ?? string.Empty,
                ["W.B.2.target_timeline_milestone"] = () => _wizard.TargetTimeline?.ToString("yyyy-MM-dd") ?? string.Empty,
                ["W.B.3.pain_points_rank_1_3"] = () => SafeDeserialize<List<string>>(_wizard.CurrentPainPointsJson) ?? new List<string>(),
                ["W.B.4.desired_maturity_level"] = () => _wizard.DesiredMaturity ?? string.Empty,
                ["W.B.5.reporting_audience"] = () => SafeDeserialize<List<string>>(_wizard.ReportingAudienceJson) ?? new List<string>(),

                // ============================================================================
                // WIZARD - Section C (W.C) - Regulatory Applicability
                // ============================================================================
                ["W.C.1.primary_regulators"] = () => SafeDeserialize<List<string>>(_wizard.PrimaryRegulatorsJson) ?? new List<string>(),
                ["W.C.2.secondary_regulators"] = () => SafeDeserialize<List<string>>(_wizard.SecondaryRegulatorsJson) ?? new List<string>(),
                ["W.C.3.mandatory_frameworks"] = () => SafeDeserialize<List<string>>(_wizard.MandatoryFrameworksJson) ?? new List<string>(),
                ["W.C.4.benchmarking_frameworks"] = () => SafeDeserialize<List<string>>(_wizard.OptionalFrameworksJson) ?? new List<string>(),
                ["W.C.5.internal_policies_standards"] = () => SafeDeserialize<List<string>>(_wizard.InternalPoliciesJson) ?? new List<string>(),
                ["W.C.6.certifications_held"] = () => SafeDeserialize<List<string>>(_wizard.CertificationsHeldJson) ?? new List<string>(),
                ["W.C.7.audit_scope_type"] = () => _wizard.AuditScopeType,

                // ============================================================================
                // WIZARD - Section D (W.D) - Scope Definition
                // ============================================================================
                ["W.D.1.in_scope_legal_entities"] = () => SafeDeserialize<List<object>>(_wizard.InScopeLegalEntitiesJson) ?? new List<object>(),
                ["W.D.2.in_scope_business_units"] = () => SafeDeserialize<List<object>>(_wizard.InScopeBusinessUnitsJson) ?? new List<object>(),
                ["W.D.3.in_scope_systems_apps"] = () => SafeDeserialize<List<object>>(_wizard.InScopeSystemsJson) ?? new List<object>(),
                ["W.D.4.in_scope_processes"] = () => SafeDeserialize<List<string>>(_wizard.InScopeProcessesJson) ?? new List<string>(),
                ["W.D.5.in_scope_environments"] = () => !string.IsNullOrEmpty(_wizard.InScopeEnvironments) ? new List<string> { _wizard.InScopeEnvironments } : SafeDeserialize<List<string>>(ExtractStringFromJson(_wizard.AllAnswersJson, "in_scope_environments") ?? "[]") ?? new List<string>(),
                ["W.D.6.in_scope_locations"] = () => SafeDeserialize<List<object>>(_wizard.InScopeLocationsJson) ?? new List<object>(),
                ["W.D.7.system_criticality_tiers"] = () => SafeDeserialize<object>(_wizard.SystemCriticalityTiersJson) ?? SafeDeserialize<List<object>>(ExtractStringFromJson(_wizard.AllAnswersJson, "system_criticality_tiers") ?? "[]") ?? new List<object>(),
                ["W.D.8.important_business_services"] = () => SafeDeserialize<List<object>>(_wizard.ImportantBusinessServicesJson) ?? new List<object>(),
                ["W.D.9.exclusions_with_rationale"] = () => SafeDeserialize<List<object>>(_wizard.ExclusionsJson) ?? new List<object>(),

                // ============================================================================
                // WIZARD - Section E (W.E) - Data & Risk Profile
                // ============================================================================
                ["W.E.1.data_types_processed"] = () => SafeDeserialize<List<string>>(_wizard.DataTypesProcessedJson) ?? new List<string>(),
                ["W.E.2.payment_card_data"] = () => _wizard.HasPaymentCardData,
                ["W.E.2b.payment_card_details"] = () => SafeDeserialize<object>(_wizard.PaymentCardDataLocationsJson) ?? ExtractObjectFromJson(_wizard.AllAnswersJson, "payment_card_details"),
                ["W.E.3.cross_border_transfers"] = () => _wizard.HasCrossBorderDataTransfers ? SafeDeserialize<List<string>>(_wizard.CrossBorderTransferCountriesJson) ?? new List<string>() : new List<object>(),
                ["W.E.4.customer_volume_tier"] = () => _wizard.CustomerVolumeTier,
                ["W.E.5.transaction_volume_tier"] = () => _wizard.TransactionVolumeTier,
                ["W.E.6.third_party_data_processors"] = () => _wizard.HasThirdPartyDataProcessing ? SafeDeserialize<List<object>>(_wizard.ThirdPartyDataProcessorsJson) ?? new List<object>() : new List<object>(),

                // ============================================================================
                // WIZARD - Section F (W.F) - Technology Landscape
                // ============================================================================
                ["W.F.1.identity_provider"] = () => _wizard.IdentityProvider,
                ["W.F.2.sso_enabled"] = () => _wizard.SsoEnabled,
                ["W.F.2b.sso_protocol"] = () => ExtractStringFromJson(_wizard.AllAnswersJson, "sso_protocol"),
                ["W.F.3.scim_provisioning"] = () => _wizard.ScimProvisioningAvailable,
                ["W.F.4.itsm_ticketing_platform"] = () => _wizard.ItsmPlatform,
                ["W.F.5.evidence_repository"] = () => _wizard.EvidenceRepository,
                ["W.F.6.siem_soc_platform"] = () => _wizard.SiemPlatform,
                ["W.F.7.vulnerability_management"] = () => _wizard.VulnerabilityManagementTool,
                ["W.F.8.edr_platform"] = () => _wizard.EdrPlatform,
                ["W.F.9.cloud_providers"] = () => SafeDeserialize<List<string>>(_wizard.CloudProvidersJson) ?? new List<string>(),
                ["W.F.10.erp_platform"] = () => _wizard.ErpSystem,
                ["W.F.11.cmdb_asset_inventory"] = () => _wizard.CmdbSource,
                ["W.F.12.cicd_tooling"] = () => _wizard.CiCdTooling,
                ["W.F.13.backup_dr_tooling"] = () => _wizard.BackupDrTooling,

                // ============================================================================
                // WIZARD - Section G (W.G) - Control Ownership
                // ============================================================================
                ["W.G.1.ownership_approach"] = () => _wizard.ControlOwnershipApproach,
                ["W.G.2.default_control_owner_team"] = () => _wizard.DefaultControlOwnerTeam,
                ["W.G.3.exception_approver_role"] = () => _wizard.ExceptionApproverRole,
                ["W.G.4.regulatory_interpreter_role"] = () => _wizard.RegulatoryInterpretationApproverRole,
                ["W.G.5.effectiveness_signoff_role"] = () => _wizard.ControlEffectivenessSignoffRole,
                ["W.G.6.internal_audit_contact"] = () => !string.IsNullOrEmpty(_wizard.InternalAuditStakeholder) ? new List<object> { new { Name = _wizard.InternalAuditStakeholder } } : SafeDeserialize<List<object>>(ExtractStringFromJson(_wizard.AllAnswersJson, "internal_audit_contact") ?? "[]") ?? new List<object>(),
                ["W.G.7.risk_committee"] = () => SafeDeserialize<List<object>>(_wizard.RiskCommitteeAttendeesJson) ?? new List<object>(),

                // ============================================================================
                // WIZARD - Section H (W.H) - Teams, Roles & Access
                // ============================================================================
                ["W.H.1.organization_admins"] = () => SafeDeserialize<List<object>>(_wizard.OrgAdminsJson) ?? new List<object>(),
                ["W.H.2.create_teams_now"] = () => _wizard.CreateTeamsNow,
                ["W.H.3.team_definitions"] = () => SafeDeserialize<List<object>>(_wizard.TeamListJson) ?? new List<object>(),
                ["W.H.4.team_members"] = () => SafeDeserialize<List<object>>(_wizard.TeamMembersJson) ?? new List<object>(),
                ["W.H.5.role_catalog"] = () => SafeDeserialize<List<string>>(_wizard.SelectedRoleCatalogJson) ?? new List<string>(),
                ["W.H.6.raci_mapping_needed"] = () => _wizard.RaciMappingNeeded,
                ["W.H.6b.raci_matrix"] = () => SafeDeserialize<object>(_wizard.RaciMappingJson) ?? new object(),
                ["W.H.7.approval_gates"] = () => _wizard.ApprovalGatesNeeded,
                ["W.H.7b.approval_gate_config"] = () => SafeDeserialize<object>(_wizard.ApprovalGatesJson) ?? new object(),
                ["W.H.8.delegation_rules"] = () => SafeDeserialize<List<object>>(_wizard.DelegationRulesJson) ?? new List<object>(),
                ["W.H.9.notification_preferences"] = () => _wizard.NotificationPreference,
                ["W.H.10.escalation_path"] = () => new { DaysOverdue = _wizard.EscalationDaysOverdue, Target = _wizard.EscalationTarget },

                // ============================================================================
                // WIZARD - Section I (W.I) - Workflow & Cadence
                // ============================================================================
                ["W.I.1.evidence_frequency_defaults"] = () => SafeDeserialize<object>(_wizard.EvidenceFrequencyDefaultsJson) ?? new object(),
                ["W.I.2.access_review_frequency"] = () => _wizard.AccessReviewsFrequency,
                ["W.I.3.vulnerability_review_frequency"] = () => _wizard.VulnerabilityPatchReviewFrequency,
                ["W.I.4.backup_review_frequency"] = () => _wizard.BackupReviewFrequency,
                ["W.I.5.restore_test_cadence"] = () => _wizard.RestoreTestCadence,
                ["W.I.6.dr_exercise_cadence"] = () => _wizard.DrExerciseCadence,
                ["W.I.7.incident_tabletop_cadence"] = () => _wizard.IncidentTabletopCadence,
                ["W.I.8.evidence_sla_submit_days"] = () => _wizard.EvidenceSlaSubmitDays,
                ["W.I.9.remediation_sla_by_severity"] = () => SafeDeserialize<object>(_wizard.RemediationSlaJson) ?? new object(),
                ["W.I.10.exception_expiry_days"] = () => _wizard.ExceptionExpiryDays,

                // ============================================================================
                // WIZARD - Section J (W.J) - Evidence Standards
                // ============================================================================
                ["W.J.1.naming_convention_required"] = () => _wizard.EvidenceNamingConventionRequired,
                ["W.J.1b.naming_pattern"] = () => _wizard.EvidenceNamingPattern,
                ["W.J.2.storage_location_by_domain"] = () => SafeDeserialize<object>(_wizard.EvidenceStorageLocationJson) ?? new object(),
                ["W.J.3.retention_period_years"] = () => _wizard.EvidenceRetentionYears,
                ["W.J.4.access_rules"] = () => SafeDeserialize<object>(_wizard.EvidenceAccessRulesJson) ?? new object(),
                ["W.J.5.acceptable_evidence_types"] = () => SafeDeserialize<List<string>>(_wizard.AcceptableEvidenceTypesJson) ?? new List<string>(),
                ["W.J.6.sampling_guidance"] = () => SafeDeserialize<object>(_wizard.SamplingGuidanceJson) ?? new object(),
                ["W.J.7.confidential_evidence_handling"] = () => new { Encryption = _wizard.ConfidentialEvidenceEncryption, Access = SafeDeserialize<List<string>>(_wizard.ConfidentialEvidenceAccessJson) ?? new List<string>() },

                // ============================================================================
                // WIZARD - Section K (W.K) - Baseline + Overlays
                // ============================================================================
                ["W.K.1.adopt_default_baseline"] = () => _wizard.AdoptDefaultBaseline,
                ["W.K.2.select_overlays"] = () => SafeDeserialize<List<string>>(_wizard.SelectedOverlaysJson) ?? new List<string>(),
                ["W.K.3.custom_control_requirements"] = () => SafeDeserialize<List<object>>(_wizard.ClientSpecificControlsJson) ?? new List<object>(),

                // ============================================================================
                // WIZARD - Section L (W.L) - Go-Live & Success Metrics
                // ============================================================================
                ["W.L.1.success_metrics_top3"] = () => SafeDeserialize<List<string>>(_wizard.SuccessMetricsTop3Json) ?? new List<string>(),
                ["W.L.2.current_audit_prep_hours_per_month"] = () => _wizard.BaselineAuditPrepHoursPerMonth,
                ["W.L.3.current_remediation_closure_days"] = () => _wizard.BaselineRemediationClosureDays,
                ["W.L.4.current_overdue_controls_per_month"] = () => _wizard.BaselineOverdueControlsPerMonth,
                ["W.L.5.target_improvement_percent"] = () => SafeDeserialize<object>(_wizard.TargetImprovementJson) ?? new object(),
                ["W.L.6.pilot_scope"] = () => SafeDeserialize<List<string>>(_wizard.PilotScopeJson) ?? new List<string>(),
            };
        }

        private object? ExtractFromJsonFields(string fieldId)
        {
            // Try to find field in AllAnswersJson using dot notation
            if (!string.IsNullOrEmpty(_wizard.AllAnswersJson))
            {
                try
                {
                    var jsonDict = JsonSerializer.Deserialize<Dictionary<string, object>>(_wizard.AllAnswersJson);
                    if (jsonDict != null)
                    {
                        // Try exact field ID first
                        if (jsonDict.TryGetValue(fieldId, out var exactValue))
                        {
                            return exactValue;
                        }

                        // Convert fieldId to camelCase or snake_case key
                        var key = fieldId.Replace(".", "_").ToLowerInvariant();
                        if (jsonDict.TryGetValue(key, out var value))
                        {
                            return value;
                        }

                        // Try last part of field ID
                        var lastPart = fieldId.Split('.').LastOrDefault();
                        if (!string.IsNullOrEmpty(lastPart) && jsonDict.TryGetValue(lastPart, out var lastValue))
                        {
                            return lastValue;
                        }
                    }
                }
                catch
                {
                    // Ignore JSON parse errors
                }
            }

            return null;
        }

        private List<string>? ExtractFromTenantBaselines()
        {
            // Try to extract from AllAnswersJson first, then from tenant baselines if available
            // This would require tenant context, so for now return null and let caller handle
            return null;
        }

        private string? ExtractAdminEmail()
        {
            // Try to extract from OrgAdminsJson (first admin email)
            var orgAdmins = SafeDeserialize<List<Dictionary<string, object>>>(_wizard.OrgAdminsJson);
            if (orgAdmins != null && orgAdmins.Any())
            {
                var firstAdmin = orgAdmins.FirstOrDefault();
                if (firstAdmin != null && firstAdmin.TryGetValue("email", out var emailObj))
                {
                    return emailObj?.ToString();
                }
            }
            return null;
        }

        private static T? SafeDeserialize<T>(string? json) where T : class
        {
            if (string.IsNullOrWhiteSpace(json) || json == "[]")
                return null;

            try
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            catch
            {
                return null;
            }
        }

        private static string? ExtractStringFromJson(string? json, string key)
        {
            if (string.IsNullOrWhiteSpace(json) || json == "{}" || json == "[]")
                return null;

            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                if (dict != null && dict.TryGetValue(key, out var value))
                {
                    if (value is JsonElement jsonElement)
                    {
                        return jsonElement.ValueKind == JsonValueKind.String ? jsonElement.GetString() : jsonElement.GetRawText();
                    }
                    return value?.ToString();
                }
            }
            catch
            {
                // Ignore JSON parse errors
            }

            return null;
        }

        private static bool? ExtractBoolFromJson(string? json, string key)
        {
            if (string.IsNullOrWhiteSpace(json) || json == "{}" || json == "[]")
                return null;

            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                if (dict != null && dict.TryGetValue(key, out var value))
                {
                    if (value is JsonElement jsonElement)
                    {
                        if (jsonElement.ValueKind == JsonValueKind.True || jsonElement.ValueKind == JsonValueKind.False)
                            return jsonElement.GetBoolean();
                    }
                    if (value is bool boolVal)
                        return boolVal;
                    if (bool.TryParse(value?.ToString(), out var parsed))
                        return parsed;
                }
            }
            catch
            {
                // Ignore
            }

            return null;
        }

        private static int? ExtractIntFromJson(string? json, string key)
        {
            if (string.IsNullOrWhiteSpace(json) || json == "{}" || json == "[]")
                return null;

            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                if (dict != null && dict.TryGetValue(key, out var value))
                {
                    if (value is JsonElement jsonElement)
                    {
                        if (jsonElement.ValueKind == JsonValueKind.Number)
                            return jsonElement.GetInt32();
                    }
                    if (value is int intVal)
                        return intVal;
                    if (int.TryParse(value?.ToString(), out var parsed))
                        return parsed;
                }
            }
            catch
            {
                // Ignore
            }

            return null;
        }

        private static DateTime? ExtractDateTimeFromJson(string? json, string key)
        {
            var str = ExtractStringFromJson(json, key);
            if (DateTime.TryParse(str, out var dateValue))
                return dateValue;
            return null;
        }

        private static object? ExtractObjectFromJson(string? json, string key)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                if (dict != null && dict.TryGetValue(key, out var value))
                {
                    return value;
                }
            }
            catch
            {
                // Ignore
            }

            return null;
        }
    }
}
