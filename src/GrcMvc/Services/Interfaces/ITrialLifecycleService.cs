using GrcMvc.Models.DTOs;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Trial Lifecycle Service Interface
    /// Manages the complete free trial visitor journey from signup to conversion
    /// </summary>
    public interface ITrialLifecycleService
    {
        // ===== SIGNUP & PROVISIONING =====

        /// <summary>Capture initial trial signup interest</summary>
        Task<TrialSignupResult> SignupAsync(TrialSignupRequest request);

        /// <summary>Provision tenant with trial subscription and create admin user</summary>
        /// <param name="signupId">The trial signup ID</param>
        /// <param name="password">Password for the admin user account</param>
        Task<TrialProvisionResult> ProvisionTrialAsync(Guid signupId, string password);

        /// <summary>Activate trial after email verification</summary>
        Task<bool> ActivateTrialAsync(Guid tenantId, string activationToken);

        // ===== TRIAL STATUS & USAGE =====

        /// <summary>Get current trial status for a tenant</summary>
        Task<TrialStatusDto> GetTrialStatusAsync(Guid tenantId);

        /// <summary>Get trial usage/engagement metrics</summary>
        Task<TrialUsageDto> GetTrialUsageAsync(Guid tenantId);

        /// <summary>Check if trial is active and valid</summary>
        Task<bool> IsTrialActiveAsync(Guid tenantId);

        /// <summary>Get days remaining in trial</summary>
        Task<int> GetDaysRemainingAsync(Guid tenantId);

        // ===== EXTENSION & CONVERSION =====

        /// <summary>Request trial extension (one-time, +7 days)</summary>
        Task<TrialExtensionResult> RequestExtensionAsync(Guid tenantId, string reason);

        /// <summary>Start conversion to paid subscription</summary>
        Task<TrialConversionResult> StartConversionAsync(Guid tenantId, string planCode);

        /// <summary>Complete conversion after payment</summary>
        Task<bool> CompleteConversionAsync(Guid tenantId, string paymentReference);

        // ===== EMAIL NURTURE AUTOMATION =====

        /// <summary>Send nurture email based on trial day</summary>
        Task<bool> SendNurtureEmailAsync(Guid tenantId, TrialEmailType emailType);

        /// <summary>Get pending nurture emails to send</summary>
        Task<List<PendingNurtureEmail>> GetPendingNurtureEmailsAsync();

        /// <summary>Process all pending nurture emails (background job)</summary>
        Task<int> ProcessNurtureEmailsAsync();

        // ===== ADMIN FUNCTIONS =====

        /// <summary>Admin: Get trial analytics dashboard</summary>
        Task<TrialAnalyticsDto> GetTrialAnalyticsAsync();

        /// <summary>Admin: Extend trial manually</summary>
        Task<bool> AdminExtendTrialAsync(Guid tenantId, int days, string adminUserId);

        /// <summary>Admin: Expire trial immediately</summary>
        Task<bool> AdminExpireTrialAsync(Guid tenantId, string adminUserId, string reason);

        /// <summary>Admin: List all active trials</summary>
        Task<List<TrialSummaryDto>> GetActiveTrialsAsync();

        // ===== TEAM COLLABORATION =====

        /// <summary>Invite team member to trial</summary>
        Task<TeamInviteResult> InviteTeamMemberAsync(Guid tenantId, TeamInviteRequest request);

        /// <summary>Get team members for a trial</summary>
        Task<List<TrialTeamMemberDto>> GetTrialTeamAsync(Guid tenantId);

        /// <summary>Track team collaboration activity</summary>
        Task TrackTeamActivityAsync(Guid tenantId, Guid userId, string activityType, string details);

        // ===== ECOSYSTEM COLLABORATION =====

        /// <summary>Request ecosystem partner connection</summary>
        Task<EcosystemConnectionResult> RequestPartnerConnectionAsync(Guid tenantId, EcosystemPartnerRequest request);

        /// <summary>Get available ecosystem partners</summary>
        Task<List<EcosystemPartnerDto>> GetAvailablePartnersAsync(string sector);

        /// <summary>Get tenant's ecosystem connections</summary>
        Task<List<EcosystemConnectionDto>> GetEcosystemConnectionsAsync(Guid tenantId);

        // ===== WORLD-CLASS TEAM ENGAGEMENT =====

        /// <summary>Get team onboarding progress with checklist</summary>
        Task<TeamOnboardingProgress> GetTeamOnboardingProgressAsync(Guid tenantId);

        /// <summary>Get team engagement dashboard with metrics and leaderboard</summary>
        Task<TeamEngagementDashboard> GetTeamEngagementAsync(Guid tenantId);

        /// <summary>Bulk invite team members</summary>
        Task<BulkInviteResult> InviteTeamMembersBulkAsync(Guid tenantId, List<TeamInviteRequest> requests);

        // ===== WORLD-CLASS ECOSYSTEM =====

        /// <summary>Approve a partner connection request</summary>
        Task<bool> ApprovePartnerConnectionAsync(Guid connectionId, string approvedBy);

        /// <summary>Reject a partner connection request</summary>
        Task<bool> RejectPartnerConnectionAsync(Guid connectionId, string reason, string rejectedBy);

        /// <summary>Track interaction with a partner</summary>
        Task TrackPartnerInteractionAsync(Guid connectionId, string interactionType, string details);

        /// <summary>Get available integrations from marketplace</summary>
        Task<List<IntegrationDto>> GetAvailableIntegrationsAsync(string category);

        /// <summary>Connect an integration</summary>
        Task<IntegrationResult> ConnectIntegrationAsync(Guid tenantId, string integrationCode, Dictionary<string, string> config);
    }

    // ===== DTOs =====

    public class TrialSignupRequest
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty;
        public string Source { get; set; } = "website";
        public string? ReferralCode { get; set; }

        /// <summary>
        /// Terms of Service acceptance - REQUIRED for trial signup
        /// </summary>
        public bool TermsAccepted { get; set; } = false;
    }

    public class TrialSignupResult
    {
        public bool Success { get; set; }
        public Guid? SignupId { get; set; }
        public string? Message { get; set; }
        public string? ActivationToken { get; set; }
    }

    public class TrialProvisionResult
    {
        public bool Success { get; set; }
        public Guid? TenantId { get; set; }
        public Guid? UserId { get; set; }
        public string? TenantSlug { get; set; }
        public DateTime? TrialEndsAt { get; set; }
        public string? Message { get; set; }
    }

    public class TrialStatusDto
    {
        public Guid TenantId { get; set; }
        public string Status { get; set; } = "active"; // active, extended, expired, converted
        public DateTime StartedAt { get; set; }
        public DateTime EndsAt { get; set; }
        public int DaysRemaining { get; set; }
        public bool CanExtend { get; set; }
        public bool HasExtended { get; set; }
        public int ExtensionDaysUsed { get; set; }
        public string CurrentPlan { get; set; } = "trial";
        public List<string> AvailableFeatures { get; set; } = new();
        public List<string> LockedFeatures { get; set; } = new();
    }

    public class TrialUsageDto
    {
        public Guid TenantId { get; set; }
        public int TotalLogins { get; set; }
        public int UniqueUsers { get; set; }
        public int FeaturesUsed { get; set; }
        public int DocumentsUploaded { get; set; }
        public int ControlsReviewed { get; set; }
        public int ReportsGenerated { get; set; }
        public double EngagementScore { get; set; } // 0-100
        public DateTime LastActiveAt { get; set; }
        public Dictionary<string, int> FeatureUsage { get; set; } = new();
        public List<ActivitySummary> RecentActivity { get; set; } = new();
    }

    public class ActivitySummary
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    public class TrialExtensionResult
    {
        public bool Success { get; set; }
        public DateTime? NewEndDate { get; set; }
        public int? DaysAdded { get; set; }
        public string? Message { get; set; }
    }

    public class TrialConversionResult
    {
        public bool Success { get; set; }
        public string? CheckoutUrl { get; set; }
        public string? SessionId { get; set; }
        public string? PlanCode { get; set; }
        public decimal? MonthlyPrice { get; set; }
        public string? Message { get; set; }
    }

    public enum TrialEmailType
    {
        Welcome = 0,
        Nudge24h = 1,
        ValuePush72h = 3,
        Midpoint = 5,
        Escalation = 6,
        Expired = 7,
        Winback = 14
    }

    public class PendingNurtureEmail
    {
        public Guid TenantId { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public TrialEmailType EmailType { get; set; }
        public int TrialDay { get; set; }
        public DateTime DueAt { get; set; }
    }

    public class TrialAnalyticsDto
    {
        public int TotalActiveTrials { get; set; }
        public int NewTrialsToday { get; set; }
        public int NewTrialsThisWeek { get; set; }
        public int TrialsExpiringSoon { get; set; }
        public double AvgEngagementScore { get; set; }
        public double ConversionRate { get; set; }
        public int ConvertedThisMonth { get; set; }
        public List<TrialFunnelStep> Funnel { get; set; } = new();
        public Dictionary<string, int> TrialsBySector { get; set; } = new();
    }

    public class TrialFunnelStep
    {
        public string Step { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class TrialSummaryDto
    {
        public Guid TenantId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime EndsAt { get; set; }
        public int DaysRemaining { get; set; }
        public double EngagementScore { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // ===== TEAM COLLABORATION DTOs =====

    public class TeamInviteRequest
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string RoleCode { get; set; } = "viewer";
        public List<string>? Permissions { get; set; }
        public string? WelcomeMessage { get; set; }
    }

    public class TeamInviteResult
    {
        public bool Success { get; set; }
        public Guid? InviteId { get; set; }
        public string? InviteLink { get; set; }
        public string? Message { get; set; }
    }

    public class TrialTeamMemberDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // invited, active, inactive
        public DateTime JoinedAt { get; set; }
        public DateTime? LastActiveAt { get; set; }
        public int ActionsCompleted { get; set; }
        public double ContributionScore { get; set; }
    }

    // ===== ECOSYSTEM COLLABORATION DTOs =====

    public class EcosystemPartnerRequest
    {
        public string PartnerType { get; set; } = string.Empty; // consultant, auditor, vendor, regulator
        public Guid? PartnerId { get; set; }
        public string? PartnerEmail { get; set; }
        public string ConnectionPurpose { get; set; } = string.Empty;
        public List<string>? SharedDataTypes { get; set; }
        public DateTime? ConnectionExpiry { get; set; }
    }

    public class EcosystemConnectionResult
    {
        public bool Success { get; set; }
        public Guid? ConnectionId { get; set; }
        public string? Status { get; set; } // pending, approved, rejected
        public string? Message { get; set; }
    }

    public class EcosystemPartnerDto
    {
        public Guid PartnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Services { get; set; } = new();
        public List<string> Certifications { get; set; } = new();
        public double Rating { get; set; }
        public int Connections { get; set; }
    }

    public class EcosystemConnectionDto
    {
        public Guid ConnectionId { get; set; }
        public Guid PartnerId { get; set; }
        public string PartnerName { get; set; } = string.Empty;
        public string PartnerType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public DateTime ConnectedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public List<string> SharedDataTypes { get; set; } = new();
        public int InteractionsCount { get; set; }
    }

    // ===== ADDITIONAL DTOs =====

    public class TrialProgressDto
    {
        public Guid TenantId { get; set; }
        public bool OnboardingComplete { get; set; }
        public int FrameworksConfigured { get; set; }
        public int ControlsAssessed { get; set; }
        public int TeamMembersInvited { get; set; }
        public int ReportsGenerated { get; set; }
        public int OverallProgress { get; set; }
        public string NextRecommendedAction { get; set; } = string.Empty;
        public List<TrialMilestone> Milestones { get; set; } = new();
    }

    public class TrialMilestone
    {
        public string Name { get; set; } = string.Empty;
        public bool IsComplete { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class TrialRecommendationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
        public string Priority { get; set; } = "medium";
        public int EstimatedMinutes { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    public class TrialConversionRequest
    {
        public string PlanCode { get; set; } = string.Empty;
        public string PaymentMethodId { get; set; } = string.Empty;
        public string BillingCycle { get; set; } = "monthly";
    }

    public class TrialFeatureDto
    {
        public string Name { get; set; } = string.Empty;
        public string Limit { get; set; } = string.Empty;
        public bool IsUnlimited { get; set; }
    }

    // ===== WORLD-CLASS TEAM ENGAGEMENT DTOs =====

    public class TeamOnboardingProgress
    {
        public Guid TenantId { get; set; }
        public int CompletedSteps { get; set; }
        public int TotalSteps { get; set; }
        public double ProgressPercent { get; set; }
        public List<OnboardingStep> Steps { get; set; } = new();
    }

    public class OnboardingStep
    {
        public string StepId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? CompletedBy { get; set; }
    }

    public class TeamEngagementDashboard
    {
        public Guid TenantId { get; set; }
        public double OverallScore { get; set; }
        public int ActiveMembers { get; set; }
        public int TotalActions { get; set; }
        public List<MemberContribution> Leaderboard { get; set; } = new();
        public Dictionary<string, int> ActivityByDay { get; set; } = new();
        public Dictionary<string, int> ActivityByType { get; set; } = new();
    }

    public class MemberContribution
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int ActionsCompleted { get; set; }
        public double ContributionScore { get; set; }
        public int Rank { get; set; }
    }

    public class BulkInviteResult
    {
        public int TotalRequested { get; set; }
        public int Successful { get; set; }
        public int Failed { get; set; }
        public List<InviteStatus> Results { get; set; } = new();
    }

    public class InviteStatus
    {
        public string Email { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? InviteId { get; set; }
    }

    // ===== WORLD-CLASS ECOSYSTEM DTOs =====

    public class IntegrationDto
    {
        public string IntegrationCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public bool IsAvailableInTrial { get; set; }
        public List<string> RequiredScopes { get; set; } = new();
    }

    public class IntegrationResult
    {
        public bool Success { get; set; }
        public string IntegrationCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
        public Dictionary<string, string> ConnectionDetails { get; set; } = new();
    }
}
