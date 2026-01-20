using System;
using Volo.Abp.Data;
using Volo.Abp.TenantManagement;

namespace GrcMvc.Abp;

/// <summary>
/// Extension methods for ABP Tenant to store/retrieve custom GRC business fields
/// Uses ABP's ExtraProperties for seamless integration
/// </summary>
public static class TenantExtensions
{
    // Property keys for ExtraProperties
    private const string TenantSlugKey = "TenantSlug";
    private const string OrganizationNameKey = "OrganizationName";
    private const string AdminEmailKey = "AdminEmail";
    private const string EmailKey = "Email";
    private const string StripeCustomerIdKey = "StripeCustomerId";
    private const string TenantCodeKey = "TenantCode";
    private const string StatusKey = "Status";
    private const string IsActiveKey = "IsActive";
    private const string ActivationTokenKey = "ActivationToken";
    private const string ActivatedAtKey = "ActivatedAt";
    private const string ActivatedByKey = "ActivatedBy";
    private const string TrialEndDateKey = "TrialEndDate";
    private const string TrialExpiresAtKey = "TrialExpiresAt";
    private const string TrialExtendedAtKey = "TrialExtendedAt";
    private const string SubscriptionStartedAtKey = "SubscriptionStartedAt";
    private const string SubscriptionStartDateKey = "SubscriptionStartDate";
    private const string SubscriptionEndDateKey = "SubscriptionEndDate";
    private const string SubscriptionTierKey = "SubscriptionTier";
    private const string CorrelationIdKey = "CorrelationId";
    private const string CreatedByOwnerIdKey = "CreatedByOwnerId";
    private const string IsOwnerCreatedKey = "IsOwnerCreated";
    private const string BypassPaymentKey = "BypassPayment";
    private const string CredentialExpiresAtKey = "CredentialExpiresAt";
    private const string AdminAccountGeneratedKey = "AdminAccountGenerated";
    private const string AdminAccountGeneratedAtKey = "AdminAccountGeneratedAt";
    private const string IsTrialKey = "IsTrial";
    private const string TrialStartsAtKey = "TrialStartsAt";
    private const string TrialEndsAtKey = "TrialEndsAt";
    private const string BillingStatusKey = "BillingStatus";
    private const string DefaultWorkspaceIdKey = "DefaultWorkspaceId";
    private const string AssessmentTemplateIdKey = "AssessmentTemplateId";
    private const string GrcPlanIdKey = "GrcPlanId";
    private const string OnboardingStatusKey = "OnboardingStatus";
    private const string OnboardingCompletedAtKey = "OnboardingCompletedAt";
    private const string BusinessCodeKey = "BusinessCode";

    // Basic tenant info
    public static string GetTenantSlug(this Tenant tenant) => tenant.GetProperty<string>(TenantSlugKey) ?? tenant.Name;
    public static void SetTenantSlug(this Tenant tenant, string value) => tenant.SetProperty(TenantSlugKey, value);

    public static string GetOrganizationName(this Tenant tenant) => tenant.GetProperty<string>(OrganizationNameKey) ?? tenant.Name;
    public static void SetOrganizationName(this Tenant tenant, string value) => tenant.SetProperty(OrganizationNameKey, value);

    public static string GetAdminEmail(this Tenant tenant) => tenant.GetProperty<string>(AdminEmailKey) ?? string.Empty;
    public static void SetAdminEmail(this Tenant tenant, string value) => tenant.SetProperty(AdminEmailKey, value);

    public static string? GetEmail(this Tenant tenant) => tenant.GetProperty<string>(EmailKey);
    public static void SetEmail(this Tenant tenant, string? value) => tenant.SetProperty(EmailKey, value);

    public static string? GetStripeCustomerId(this Tenant tenant) => tenant.GetProperty<string>(StripeCustomerIdKey);
    public static void SetStripeCustomerId(this Tenant tenant, string? value) => tenant.SetProperty(StripeCustomerIdKey, value);

    public static string GetTenantCode(this Tenant tenant) => tenant.GetProperty<string>(TenantCodeKey) ?? string.Empty;
    public static void SetTenantCode(this Tenant tenant, string value) => tenant.SetProperty(TenantCodeKey, value);

    public static string GetBusinessCode(this Tenant tenant) => tenant.GetProperty<string>(BusinessCodeKey) ?? string.Empty;
    public static void SetBusinessCode(this Tenant tenant, string value) => tenant.SetProperty(BusinessCodeKey, value);

    // Status
    public static string GetStatus(this Tenant tenant) => tenant.GetProperty<string>(StatusKey) ?? "Pending";
    public static void SetStatus(this Tenant tenant, string value) => tenant.SetProperty(StatusKey, value);

    public static bool GetIsActive(this Tenant tenant) => tenant.GetProperty<bool>(IsActiveKey);
    public static void SetIsActive(this Tenant tenant, bool value) => tenant.SetProperty(IsActiveKey, value);

    // Activation
    public static string GetActivationToken(this Tenant tenant) => tenant.GetProperty<string>(ActivationTokenKey) ?? string.Empty;
    public static void SetActivationToken(this Tenant tenant, string value) => tenant.SetProperty(ActivationTokenKey, value);

    public static DateTime? GetActivatedAt(this Tenant tenant) => tenant.GetProperty<DateTime?>(ActivatedAtKey);
    public static void SetActivatedAt(this Tenant tenant, DateTime? value) => tenant.SetProperty(ActivatedAtKey, value);

    public static string GetActivatedBy(this Tenant tenant) => tenant.GetProperty<string>(ActivatedByKey) ?? string.Empty;
    public static void SetActivatedBy(this Tenant tenant, string value) => tenant.SetProperty(ActivatedByKey, value);

    // Trial tracking
    public static DateTime? GetTrialEndDate(this Tenant tenant) => tenant.GetProperty<DateTime?>(TrialEndDateKey);
    public static void SetTrialEndDate(this Tenant tenant, DateTime? value) => tenant.SetProperty(TrialEndDateKey, value);

    public static DateTime? GetTrialExpiresAt(this Tenant tenant) => tenant.GetProperty<DateTime?>(TrialExpiresAtKey);
    public static void SetTrialExpiresAt(this Tenant tenant, DateTime? value) => tenant.SetProperty(TrialExpiresAtKey, value);

    public static DateTime? GetTrialExtendedAt(this Tenant tenant) => tenant.GetProperty<DateTime?>(TrialExtendedAtKey);
    public static void SetTrialExtendedAt(this Tenant tenant, DateTime? value) => tenant.SetProperty(TrialExtendedAtKey, value);

    public static DateTime? GetSubscriptionStartedAt(this Tenant tenant) => tenant.GetProperty<DateTime?>(SubscriptionStartedAtKey);
    public static void SetSubscriptionStartedAt(this Tenant tenant, DateTime? value) => tenant.SetProperty(SubscriptionStartedAtKey, value);

    public static DateTime GetSubscriptionStartDate(this Tenant tenant) => tenant.GetProperty<DateTime>(SubscriptionStartDateKey);
    public static void SetSubscriptionStartDate(this Tenant tenant, DateTime value) => tenant.SetProperty(SubscriptionStartDateKey, value);

    public static DateTime? GetSubscriptionEndDate(this Tenant tenant) => tenant.GetProperty<DateTime?>(SubscriptionEndDateKey);
    public static void SetSubscriptionEndDate(this Tenant tenant, DateTime? value) => tenant.SetProperty(SubscriptionEndDateKey, value);

    public static string GetSubscriptionTier(this Tenant tenant) => tenant.GetProperty<string>(SubscriptionTierKey) ?? "MVP";
    public static void SetSubscriptionTier(this Tenant tenant, string value) => tenant.SetProperty(SubscriptionTierKey, value);

    // Correlation
    public static string GetCorrelationId(this Tenant tenant) => tenant.GetProperty<string>(CorrelationIdKey) ?? string.Empty;
    public static void SetCorrelationId(this Tenant tenant, string value) => tenant.SetProperty(CorrelationIdKey, value);

    // Owner tracking
    public static string? GetCreatedByOwnerId(this Tenant tenant) => tenant.GetProperty<string>(CreatedByOwnerIdKey);
    public static void SetCreatedByOwnerId(this Tenant tenant, string? value) => tenant.SetProperty(CreatedByOwnerIdKey, value);

    public static bool GetIsOwnerCreated(this Tenant tenant) => tenant.GetProperty<bool>(IsOwnerCreatedKey);
    public static void SetIsOwnerCreated(this Tenant tenant, bool value) => tenant.SetProperty(IsOwnerCreatedKey, value);

    public static bool GetBypassPayment(this Tenant tenant) => tenant.GetProperty<bool>(BypassPaymentKey);
    public static void SetBypassPayment(this Tenant tenant, bool value) => tenant.SetProperty(BypassPaymentKey, value);

    public static DateTime? GetCredentialExpiresAt(this Tenant tenant) => tenant.GetProperty<DateTime?>(CredentialExpiresAtKey);
    public static void SetCredentialExpiresAt(this Tenant tenant, DateTime? value) => tenant.SetProperty(CredentialExpiresAtKey, value);

    public static bool GetAdminAccountGenerated(this Tenant tenant) => tenant.GetProperty<bool>(AdminAccountGeneratedKey);
    public static void SetAdminAccountGenerated(this Tenant tenant, bool value) => tenant.SetProperty(AdminAccountGeneratedKey, value);

    public static DateTime? GetAdminAccountGeneratedAt(this Tenant tenant) => tenant.GetProperty<DateTime?>(AdminAccountGeneratedAtKey);
    public static void SetAdminAccountGeneratedAt(this Tenant tenant, DateTime? value) => tenant.SetProperty(AdminAccountGeneratedAtKey, value);

    // Trial edition
    public static bool GetIsTrial(this Tenant tenant) => tenant.GetProperty<bool>(IsTrialKey);
    public static void SetIsTrial(this Tenant tenant, bool value) => tenant.SetProperty(IsTrialKey, value);

    public static DateTime? GetTrialStartsAt(this Tenant tenant) => tenant.GetProperty<DateTime?>(TrialStartsAtKey);
    public static void SetTrialStartsAt(this Tenant tenant, DateTime? value) => tenant.SetProperty(TrialStartsAtKey, value);

    public static DateTime? GetTrialEndsAt(this Tenant tenant) => tenant.GetProperty<DateTime?>(TrialEndsAtKey);
    public static void SetTrialEndsAt(this Tenant tenant, DateTime? value) => tenant.SetProperty(TrialEndsAtKey, value);

    public static string GetBillingStatus(this Tenant tenant) => tenant.GetProperty<string>(BillingStatusKey) ?? "Active";
    public static void SetBillingStatus(this Tenant tenant, string value) => tenant.SetProperty(BillingStatusKey, value);

    // Onboarding
    public static Guid? GetDefaultWorkspaceId(this Tenant tenant) => tenant.GetProperty<Guid?>(DefaultWorkspaceIdKey);
    public static void SetDefaultWorkspaceId(this Tenant tenant, Guid? value) => tenant.SetProperty(DefaultWorkspaceIdKey, value);

    public static Guid? GetAssessmentTemplateId(this Tenant tenant) => tenant.GetProperty<Guid?>(AssessmentTemplateIdKey);
    public static void SetAssessmentTemplateId(this Tenant tenant, Guid? value) => tenant.SetProperty(AssessmentTemplateIdKey, value);

    public static Guid? GetGrcPlanId(this Tenant tenant) => tenant.GetProperty<Guid?>(GrcPlanIdKey);
    public static void SetGrcPlanId(this Tenant tenant, Guid? value) => tenant.SetProperty(GrcPlanIdKey, value);

    public static string GetOnboardingStatus(this Tenant tenant) => tenant.GetProperty<string>(OnboardingStatusKey) ?? "NOT_STARTED";
    public static void SetOnboardingStatus(this Tenant tenant, string value) => tenant.SetProperty(OnboardingStatusKey, value);

    public static DateTime? GetOnboardingCompletedAt(this Tenant tenant) => tenant.GetProperty<DateTime?>(OnboardingCompletedAtKey);
    public static void SetOnboardingCompletedAt(this Tenant tenant, DateTime? value) => tenant.SetProperty(OnboardingCompletedAtKey, value);
}
