using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Trial Lifecycle Service Implementation
    /// Manages free trial from signup through conversion with team and ecosystem collaboration
    /// </summary>
    public class TrialLifecycleService : ITrialLifecycleService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<TrialLifecycleService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGrcEmailService _emailService;
        private readonly IConfiguration _configuration;

        private const int DEFAULT_TRIAL_DAYS = 7;
        private const int EXTENSION_DAYS = 7;
        private const int TRIAL_TEAM_LIMIT = 5;

        public TrialLifecycleService(
            GrcDbContext context,
            ILogger<TrialLifecycleService> logger,
            UserManager<ApplicationUser> userManager,
            IGrcEmailService emailService,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
        }

        #region Signup & Provisioning

        public async Task<TrialSignupResult> SignupAsync(TrialSignupRequest request)
        {
            try
            {
                _logger.LogInformation("Processing trial signup for {Email}", request.Email);

                // Validate terms acceptance
                if (!request.TermsAccepted)
                {
                    return new TrialSignupResult
                    {
                        Success = false,
                        Message = "You must accept the Terms of Service and Privacy Policy to continue."
                    };
                }

                // Validate password strength
                if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 8)
                {
                    return new TrialSignupResult
                    {
                        Success = false,
                        Message = "Password must be at least 8 characters long."
                    };
                }

                // Check for existing tenant with same email
                var existingTenant = await _context.Tenants
                    .FirstOrDefaultAsync(t => t.Email == request.Email);

                if (existingTenant != null)
                {
                    return new TrialSignupResult
                    {
                        Success = false,
                        Message = "An account with this email already exists. Please login instead."
                    };
                }

                // Check for existing user with same email
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return new TrialSignupResult
                    {
                        Success = false,
                        Message = "An account with this email already exists. Please login instead."
                    };
                }

                var signup = new TrialSignup
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    CompanyName = request.CompanyName,
                    Phone = request.Phone,
                    Sector = request.Sector,
                    Source = request.Source,
                    ReferralCode = request.ReferralCode,
                    ActivationToken = GenerateActivationToken(),
                    Status = "pending",
                    CreatedDate = DateTime.UtcNow
                };

                _context.TrialSignups.Add(signup);
                await _context.SaveChangesAsync();

                // Send activation email
                try
                {
                    await _emailService.SendTrialActivationEmailAsync(
                        signup.Email,
                        signup.FirstName ?? "User",
                        signup.ActivationToken ?? "",
                        isArabic: false);
                    _logger.LogInformation("Trial activation email sent to {Email}", signup.Email);
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Failed to send activation email for signup {SignupId}", signup.Id);
                    // Don't fail the signup if email fails - user can request resend
                }

                _logger.LogInformation("Trial signup created: {SignupId} for {Email}", signup.Id, signup.Email);

                return new TrialSignupResult
                {
                    Success = true,
                    SignupId = signup.Id,
                    ActivationToken = signup.ActivationToken,
                    Message = "Signup successful. Please check your email to activate your trial."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing trial signup for {Email}", request.Email);
                return new TrialSignupResult { Success = false, Message = "Signup failed. Please try again." };
            }
        }

        public async Task<TrialProvisionResult> ProvisionTrialAsync(Guid signupId, string password)
        {
            try
            {
                _logger.LogInformation("Provisioning trial for signup {SignupId}", signupId);

                var signup = await _context.TrialSignups.FindAsync(signupId);
                if (signup == null)
                {
                    return new TrialProvisionResult { Success = false, Message = "Signup not found" };
                }

                if (signup.Status == "provisioned")
                {
                    return new TrialProvisionResult { Success = false, Message = "Trial already provisioned" };
                }

                // Validate password
                if (string.IsNullOrEmpty(password) || password.Length < 8)
                {
                    return new TrialProvisionResult { Success = false, Message = "Password must be at least 8 characters long." };
                }

                // Generate unique tenant slug
                var tenantSlug = GenerateSlug(signup.CompanyName ?? "company");

                // Create tenant
                var tenant = new Tenant
                {
                    // Id is auto-generated by ABP - don't set manually
                    OrganizationName = signup.CompanyName ?? "Company",
                    TenantSlug = tenantSlug,
                    Email = signup.Email,
                    AdminEmail = signup.Email,
                    Status = "trial",
                    IsTrial = true,
                    TrialStartsAt = DateTime.UtcNow,
                    TrialEndsAt = DateTime.UtcNow.AddDays(DEFAULT_TRIAL_DAYS),
                    OnboardingStatus = "NOT_STARTED",
                    OnboardingStartedAt = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "system"
                };

                _context.Tenants.Add(tenant);

                // Create ApplicationUser for ASP.NET Identity authentication
                var applicationUser = new ApplicationUser
                {
                    // Id is auto-generated
                    FirstName = signup.FirstName ?? "",
                    LastName = signup.LastName ?? "",
                    IsActive = true,
                    MustChangePassword = false,
                    CreatedDate = DateTime.UtcNow,
                    LastPasswordChangedAt = DateTime.UtcNow
                };

                var createUserResult = await _userManager.CreateAsync(applicationUser, password);
                if (!createUserResult.Succeeded)
                {
                    // Rollback tenant creation
                    _context.Tenants.Remove(tenant);
                    var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create ApplicationUser for trial signup {SignupId}: {Errors}", signupId, errors);
                    return new TrialProvisionResult { Success = false, Message = $"Failed to create user account: {errors}" };
                }

                // Set UserName, Email, and confirm email using UserManager methods
                await _userManager.SetUserNameAsync(applicationUser, signup.Email);
                await _userManager.SetEmailAsync(applicationUser, signup.Email);
                var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
                await _userManager.ConfirmEmailAsync(applicationUser, emailToken);

                // Set FirstAdminUserId before adding role
                tenant.FirstAdminUserId = applicationUser.Id.ToString();

                // Add TenantAdmin role to the user (tenant administrator for their organization)
                var roleResult = await _userManager.AddToRoleAsync(applicationUser, "TenantAdmin");
                if (!roleResult.Succeeded)
                {
                    _logger.LogWarning("Failed to add Admin role to user {Email}: {Errors}",
                        signup.Email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    // Continue - role can be added later
                }

                // Create TenantUser linking ApplicationUser to Tenant
                var tenantUser = new TenantUser
                {
                    // Id is auto-generated by ABP - don't set manually
                    TenantId = tenant.Id,
                    UserId = applicationUser.Id.ToString(), // Use ApplicationUser.Id, NOT email
                    RoleCode = "TenantAdmin",
                    TitleCode = "TENANT_ADMIN",
                    Status = "Active",
                    InvitedAt = DateTime.UtcNow,
                    ActivatedAt = DateTime.UtcNow,
                    InvitedBy = "system",
                    CreatedDate = DateTime.UtcNow
                };

                _context.TenantUsers.Add(tenantUser);

                // Update signup record
                signup.Status = "provisioned";
                signup.TenantId = tenant.Id;
                signup.ProvisionedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Send welcome email
                try
                {
                    await SendNurtureEmailAsync(tenant.Id, TrialEmailType.Welcome);
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Failed to send welcome email for tenant {TenantId}", tenant.Id);
                }

                _logger.LogInformation("Trial provisioned: Tenant={TenantId}, User={UserId}, Email={Email}",
                    tenant.Id, applicationUser.Id, signup.Email);

                return new TrialProvisionResult
                {
                    Success = true,
                    TenantId = tenant.Id,
                    UserId = applicationUser.Id,
                    TenantSlug = tenantSlug,
                    TrialEndsAt = tenant.TrialEndsAt,
                    Message = "Trial provisioned successfully. You can now log in."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error provisioning trial for signup {SignupId}", signupId);
                return new TrialProvisionResult { Success = false, Message = "Provisioning failed. Please try again." };
            }
        }

        public async Task<bool> ActivateTrialAsync(Guid tenantId, string activationToken)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null) return false;

            tenant.ActivatedAt = DateTime.UtcNow;
            tenant.Status = "active";
            await _context.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Trial Status & Usage

        public async Task<TrialStatusDto> GetTrialStatusAsync(Guid tenantId)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
            {
                return new TrialStatusDto { Status = "not_found" };
            }

            var daysRemaining = tenant.TrialEndsAt.HasValue
                ? Math.Max(0, (tenant.TrialEndsAt.Value - DateTime.UtcNow).Days)
                : 0;

            var originalEndDate = (tenant.TrialStartsAt ?? tenant.CreatedDate).AddDays(DEFAULT_TRIAL_DAYS);
            var hasExtended = tenant.TrialEndsAt > originalEndDate;

            return new TrialStatusDto
            {
                TenantId = tenantId,
                Status = DetermineTrialStatus(tenant, daysRemaining),
                StartedAt = tenant.TrialStartsAt ?? tenant.CreatedDate,
                EndsAt = tenant.TrialEndsAt ?? DateTime.UtcNow,
                DaysRemaining = daysRemaining,
                CanExtend = !hasExtended && daysRemaining <= 2 && tenant.Status == "active",
                HasExtended = hasExtended,
                ExtensionDaysUsed = hasExtended ? EXTENSION_DAYS : 0,
                CurrentPlan = "trial",
                AvailableFeatures = new List<string>
                {
                    "Compliance Frameworks (2)",
                    "Team Members (5)",
                    "AI Analysis (10/day)",
                    "Storage (500 MB)"
                },
                LockedFeatures = new List<string>
                {
                    "Unlimited Frameworks",
                    "Advanced Reporting",
                    "API Access",
                    "Priority Support"
                }
            };
        }

        public async Task<TrialUsageDto> GetTrialUsageAsync(Guid tenantId)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
            {
                return new TrialUsageDto { TenantId = tenantId };
            }

            return new TrialUsageDto
            {
                TenantId = tenantId,
                TotalLogins = 0,
                UniqueUsers = 1,
                FeaturesUsed = 0,
                DocumentsUploaded = 0,
                ControlsReviewed = 0,
                ReportsGenerated = 0,
                EngagementScore = 25.0,
                LastActiveAt = tenant.ActivatedAt ?? tenant.CreatedDate,
                FeatureUsage = new Dictionary<string, int>(),
                RecentActivity = new List<ActivitySummary>()
            };
        }

        public async Task<bool> IsTrialActiveAsync(Guid tenantId)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null) return false;

            return tenant.IsTrial &&
                   tenant.TrialEndsAt.HasValue &&
                   tenant.TrialEndsAt > DateTime.UtcNow;
        }

        public async Task<bool> IsTrialExpiredAsync(Guid tenantId)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null) return false;

            // Not a trial tenant - not expired
            if (!tenant.IsTrial) return false;

            // No end date set - not expired
            if (!tenant.TrialEndsAt.HasValue) return false;

            // Check if trial has ended
            return tenant.TrialEndsAt.Value <= DateTime.UtcNow;
        }

        public async Task<int> GetDaysRemainingAsync(Guid tenantId)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null || !tenant.TrialEndsAt.HasValue) return 0;

            return Math.Max(0, (tenant.TrialEndsAt.Value - DateTime.UtcNow).Days);
        }

        #endregion

        #region Extension & Conversion

        public async Task<TrialExtensionResult> RequestExtensionAsync(Guid tenantId, string reason)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(tenantId);
                if (tenant == null)
                {
                    return new TrialExtensionResult { Success = false, Message = "Tenant not found" };
                }

                var originalEndDate = (tenant.TrialStartsAt ?? tenant.CreatedDate).AddDays(DEFAULT_TRIAL_DAYS);
                if (tenant.TrialEndsAt > originalEndDate)
                {
                    return new TrialExtensionResult
                    {
                        Success = false,
                        Message = "Trial has already been extended once"
                    };
                }

                var newEndDate = (tenant.TrialEndsAt ?? DateTime.UtcNow).AddDays(EXTENSION_DAYS);
                tenant.TrialEndsAt = newEndDate;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Trial extended for tenant {TenantId} by {Days} days. Reason: {Reason}",
                    tenantId, EXTENSION_DAYS, reason);

                return new TrialExtensionResult
                {
                    Success = true,
                    NewEndDate = newEndDate,
                    DaysAdded = EXTENSION_DAYS,
                    Message = $"Trial extended by {EXTENSION_DAYS} days"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending trial for tenant {TenantId}", tenantId);
                return new TrialExtensionResult { Success = false, Message = "Extension failed" };
            }
        }

        public async Task<TrialConversionResult> StartConversionAsync(Guid tenantId, string planCode)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(tenantId);
                if (tenant == null)
                {
                    return new TrialConversionResult { Success = false, Message = "Tenant not found" };
                }

                // TODO: Integrate with Stripe for checkout session creation
                _logger.LogInformation("Starting conversion for tenant {TenantId} to plan {PlanCode}", tenantId, planCode);

                return new TrialConversionResult
                {
                    Success = true,
                    CheckoutUrl = "/billing/checkout?plan=" + planCode,
                    PlanCode = planCode,
                    MonthlyPrice = GetPlanPrice(planCode),
                    Message = "Checkout session created"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting conversion for tenant {TenantId}", tenantId);
                return new TrialConversionResult { Success = false, Message = "Conversion start failed" };
            }
        }

        public async Task<bool> CompleteConversionAsync(Guid tenantId, string paymentReference)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(tenantId);
                if (tenant == null) return false;

                tenant.Status = "active";
                tenant.IsTrial = false;
                tenant.TrialEndsAt = null;
                tenant.SubscriptionStartDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Trial converted for tenant {TenantId}, payment ref: {PaymentRef}",
                    tenantId, paymentReference);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing conversion for tenant {TenantId}", tenantId);
                return false;
            }
        }

        #endregion

        #region Email Nurture

        public async Task<bool> SendNurtureEmailAsync(Guid tenantId, TrialEmailType emailType)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(tenantId);
                if (tenant == null) return false;

                // TODO: Integrate with email service
                _logger.LogInformation("Nurture email {Type} would be sent to tenant {TenantId}", emailType, tenantId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending nurture email to tenant {TenantId}", tenantId);
                return false;
            }
        }

        public async Task<List<PendingNurtureEmail>> GetPendingNurtureEmailsAsync()
        {
            var now = DateTime.UtcNow;
            var pendingEmails = new List<PendingNurtureEmail>();

            var trials = await _context.Tenants
                .Where(t => t.IsTrial && t.TrialEndsAt > now)
                .ToListAsync();

            foreach (var tenant in trials)
            {
                var trialDay = (now - (tenant.TrialStartsAt ?? tenant.CreatedDate)).Days;

                TrialEmailType? emailType = trialDay switch
                {
                    1 => TrialEmailType.Nudge24h,
                    3 => TrialEmailType.ValuePush72h,
                    5 => TrialEmailType.Midpoint,
                    6 => TrialEmailType.Escalation,
                    _ => null
                };

                if (emailType.HasValue)
                {
                    pendingEmails.Add(new PendingNurtureEmail
                    {
                        TenantId = tenant.Id,
                        UserId = Guid.Empty,
                        Email = tenant.Email ?? tenant.AdminEmail ?? "",
                        Name = tenant.OrganizationName ?? "Customer",
                        EmailType = emailType.Value,
                        TrialDay = trialDay,
                        DueAt = now
                    });
                }
            }

            return pendingEmails;
        }

        public async Task<int> ProcessNurtureEmailsAsync()
        {
            var pendingEmails = await GetPendingNurtureEmailsAsync();
            var sentCount = 0;

            foreach (var email in pendingEmails)
            {
                var sent = await SendNurtureEmailAsync(email.TenantId, email.EmailType);
                if (sent) sentCount++;
            }

            _logger.LogInformation("Processed {Count} nurture emails", sentCount);
            return sentCount;
        }

        #endregion

        #region Admin Functions

        public async Task<TrialAnalyticsDto> GetTrialAnalyticsAsync()
        {
            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var weekStart = now.AddDays(-7);

            var activeTrials = await _context.Tenants
                .Where(t => t.IsTrial && t.TrialEndsAt > now)
                .ToListAsync();

            var converted = await _context.Tenants
                .Where(t => !t.IsTrial && t.SubscriptionStartDate != default)
                .Where(t => t.SubscriptionStartDate >= now.AddMonths(-1))
                .CountAsync();

            return new TrialAnalyticsDto
            {
                TotalActiveTrials = activeTrials.Count,
                NewTrialsToday = activeTrials.Count(t => t.TrialStartsAt >= todayStart),
                NewTrialsThisWeek = activeTrials.Count(t => t.TrialStartsAt >= weekStart),
                TrialsExpiringSoon = activeTrials.Count(t => t.TrialEndsAt <= now.AddDays(2)),
                AvgEngagementScore = 65.5,
                ConversionRate = activeTrials.Count > 0 ? (double)converted / activeTrials.Count * 100 : 0,
                ConvertedThisMonth = converted,
                Funnel = new List<TrialFunnelStep>
                {
                    new() { Step = "Signup", Count = activeTrials.Count + converted, Percentage = 100 },
                    new() { Step = "Activated", Count = activeTrials.Count(t => t.ActivatedAt != null), Percentage = 85 },
                    new() { Step = "Engaged", Count = activeTrials.Count / 2, Percentage = 50 },
                    new() { Step = "Converted", Count = converted, Percentage = 15 }
                },
                TrialsBySector = new Dictionary<string, int>()
            };
        }

        public async Task<bool> AdminExtendTrialAsync(Guid tenantId, int days, string adminUserId)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null) return false;

            tenant.TrialEndsAt = (tenant.TrialEndsAt ?? DateTime.UtcNow).AddDays(days);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin {AdminId} extended trial for {TenantId} by {Days} days",
                adminUserId, tenantId, days);
            return true;
        }

        public async Task<bool> AdminExpireTrialAsync(Guid tenantId, string adminUserId, string reason)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null) return false;

            tenant.Status = "expired";
            tenant.IsTrial = false;
            tenant.TrialEndsAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin {AdminId} expired trial for {TenantId}: {Reason}",
                adminUserId, tenantId, reason);
            return true;
        }

        public async Task<List<TrialSummaryDto>> GetActiveTrialsAsync()
        {
            var trials = await _context.Tenants
                .Where(t => t.IsTrial && t.TrialEndsAt > DateTime.UtcNow)
                .OrderByDescending(t => t.TrialStartsAt)
                .ToListAsync();

            return trials.Select(t => new TrialSummaryDto
            {
                TenantId = t.Id,
                CompanyName = t.OrganizationName ?? "Unknown",
                ContactEmail = t.Email ?? t.AdminEmail ?? "",
                Sector = "General",
                StartedAt = t.TrialStartsAt ?? t.CreatedDate,
                EndsAt = t.TrialEndsAt ?? DateTime.UtcNow,
                DaysRemaining = t.TrialEndsAt.HasValue
                    ? Math.Max(0, (t.TrialEndsAt.Value - DateTime.UtcNow).Days)
                    : 0,
                EngagementScore = 50,
                Status = t.Status ?? "unknown"
            }).ToList();
        }

        #endregion

        #region Team Collaboration

        public async Task<TeamInviteResult> InviteTeamMemberAsync(Guid tenantId, TeamInviteRequest request)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(tenantId);
                if (tenant == null)
                {
                    return new TeamInviteResult { Success = false, Message = "Tenant not found" };
                }

                // Check team size limit for trials
                var currentTeamSize = await _context.TenantUsers
                    .CountAsync(tu => tu.TenantId == tenantId && tu.Status != "removed");

                if (tenant.IsTrial && currentTeamSize >= TRIAL_TEAM_LIMIT)
                {
                    return new TeamInviteResult
                    {
                        Success = false,
                        Message = $"Trial is limited to {TRIAL_TEAM_LIMIT} team members. Upgrade to add more."
                    };
                }

                // Check if user already exists in team
                var existingMember = await _context.TenantUsers
                    .FirstOrDefaultAsync(tu => tu.TenantId == tenantId && tu.UserId == request.Email);

                if (existingMember != null)
                {
                    return new TeamInviteResult
                    {
                        Success = false,
                        Message = "This user is already a member of your team"
                    };
                }

                // Generate secure invite token
                var inviteToken = GenerateActivationToken();

                // Create tenant user record
                var tenantUser = new TenantUser
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    UserId = request.Email,
                    RoleCode = request.RoleCode ?? "viewer",
                    Status = "invited",
                    InvitationToken = inviteToken,
                    InvitedAt = DateTime.UtcNow,
                    InvitedBy = "system",
                    CreatedDate = DateTime.UtcNow
                };

                _context.TenantUsers.Add(tenantUser);
                await _context.SaveChangesAsync();

                // Generate invite link
                var inviteLink = $"https://portal.shahin-ai.com/invite/{inviteToken}";

                _logger.LogInformation("Team invite sent: {Email} to tenant {TenantId} with role {Role}",
                    request.Email, tenantId, request.RoleCode);

                return new TeamInviteResult
                {
                    Success = true,
                    InviteId = tenantUser.Id,
                    InviteLink = inviteLink,
                    Message = $"Invitation sent to {request.Email}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inviting team member {Email} to tenant {TenantId}",
                    request.Email, tenantId);
                return new TeamInviteResult { Success = false, Message = "Failed to send invitation" };
            }
        }

        public async Task<List<TrialTeamMemberDto>> GetTrialTeamAsync(Guid tenantId)
        {
            var teamMembers = await _context.TenantUsers
                .Where(tu => tu.TenantId == tenantId && tu.Status != "removed")
                .OrderByDescending(tu => tu.RoleCode == "admin")
                .ThenByDescending(tu => tu.ActivatedAt)
                .ToListAsync();

            return teamMembers.Select(tu => new TrialTeamMemberDto
            {
                UserId = Guid.TryParse(tu.UserId, out var uid) ? uid : Guid.Empty,
                Email = tu.UserId ?? "",
                FullName = tu.UserId?.Split('@').FirstOrDefault() ?? "User",
                Role = tu.RoleCode ?? "viewer",
                Status = tu.Status ?? "unknown",
                JoinedAt = tu.ActivatedAt ?? tu.InvitedAt ?? DateTime.UtcNow,
                LastActiveAt = tu.LastActiveAt,
                ActionsCompleted = tu.ActionsCompleted,
                ContributionScore = tu.ContributionScore
            }).ToList();
        }

        public async Task TrackTeamActivityAsync(Guid tenantId, Guid userId, string activityType, string details)
        {
            try
            {
                // Create activity record
                var activity = new TeamActivity
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    UserId = userId,
                    ActivityType = activityType,
                    Description = details,
                    CreatedDate = DateTime.UtcNow
                };

                _context.TeamActivities.Add(activity);

                // Update tenant user stats
                var userIdString = userId.ToString();
                var tenantUser = await _context.TenantUsers
                    .FirstOrDefaultAsync(tu => tu.TenantId == tenantId && tu.UserId == userIdString);

                if (tenantUser != null)
                {
                    tenantUser.LastActiveAt = DateTime.UtcNow;
                    tenantUser.ActionsCompleted++;
                    tenantUser.ContributionScore += GetActivityWeight(activityType);
                }

                await _context.SaveChangesAsync();

                _logger.LogDebug("Activity tracked: {Type} for user {UserId} in tenant {TenantId}",
                    activityType, userId, tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking activity {Type} for user {UserId}", activityType, userId);
            }
        }

        private double GetActivityWeight(string activityType)
        {
            return activityType.ToLower() switch
            {
                "login" => 1.0,
                "document_upload" => 5.0,
                "control_review" => 10.0,
                "report_generated" => 15.0,
                "comment" => 3.0,
                "mention" => 2.0,
                "framework_added" => 20.0,
                "evidence_submitted" => 10.0,
                _ => 1.0
            };
        }

        public async Task<TeamOnboardingProgress> GetTeamOnboardingProgressAsync(Guid tenantId)
        {
            var steps = new List<OnboardingStep>
            {
                new() { StepId = "profile_complete", Title = "Complete Your Profile", Description = "Add your name and role", IsCompleted = false },
                new() { StepId = "first_framework", Title = "Add First Framework", Description = "Select a compliance framework to start", IsCompleted = false },
                new() { StepId = "first_control", Title = "Review First Control", Description = "Review and assess your first control", IsCompleted = false },
                new() { StepId = "invite_team", Title = "Invite Team Member", Description = "Invite a colleague to collaborate", IsCompleted = false },
                new() { StepId = "first_report", Title = "Generate First Report", Description = "Create your first compliance report", IsCompleted = false }
            };

            // Check completion from activities
            var activities = await _context.TeamActivities
                .Where(a => a.TenantId == tenantId)
                .Select(a => a.ActivityType)
                .Distinct()
                .ToListAsync();

            var teamSize = await _context.TenantUsers
                .CountAsync(tu => tu.TenantId == tenantId && tu.Status == "active");

            // Update completion status based on actual data
            if (activities.Contains("profile_updated")) steps[0].IsCompleted = true;
            if (activities.Contains("framework_added")) steps[1].IsCompleted = true;
            if (activities.Contains("control_review")) steps[2].IsCompleted = true;
            if (teamSize > 1) steps[3].IsCompleted = true;
            if (activities.Contains("report_generated")) steps[4].IsCompleted = true;

            var completedCount = steps.Count(s => s.IsCompleted);

            return new TeamOnboardingProgress
            {
                TenantId = tenantId,
                CompletedSteps = completedCount,
                TotalSteps = steps.Count,
                ProgressPercent = (double)completedCount / steps.Count * 100,
                Steps = steps
            };
        }

        public async Task<TeamEngagementDashboard> GetTeamEngagementAsync(Guid tenantId)
        {
            var teamMembers = await _context.TenantUsers
                .Where(tu => tu.TenantId == tenantId && tu.Status == "active")
                .OrderByDescending(tu => tu.ContributionScore)
                .ToListAsync();

            var activities = await _context.TeamActivities
                .Where(a => a.TenantId == tenantId && a.CreatedDate >= DateTime.UtcNow.AddDays(-30))
                .ToListAsync();

            var leaderboard = teamMembers.Select((tu, index) => new MemberContribution
            {
                UserId = Guid.TryParse(tu.UserId, out var uid) ? uid : Guid.Empty,
                Name = tu.UserId?.Split('@').FirstOrDefault() ?? "User",
                Role = tu.RoleCode ?? "member",
                ActionsCompleted = tu.ActionsCompleted,
                ContributionScore = tu.ContributionScore,
                Rank = index + 1
            }).ToList();

            var activityByDay = activities
                .GroupBy(a => a.CreatedDate.ToString("yyyy-MM-dd"))
                .ToDictionary(g => g.Key, g => g.Count());

            var activityByType = activities
                .GroupBy(a => a.ActivityType ?? "other")
                .ToDictionary(g => g.Key, g => g.Count());

            var overallScore = teamMembers.Any()
                ? teamMembers.Average(m => m.ContributionScore)
                : 0;

            return new TeamEngagementDashboard
            {
                TenantId = tenantId,
                OverallScore = overallScore,
                ActiveMembers = teamMembers.Count(m => m.LastActiveAt >= DateTime.UtcNow.AddDays(-7)),
                TotalActions = activities.Count,
                Leaderboard = leaderboard,
                ActivityByDay = activityByDay,
                ActivityByType = activityByType
            };
        }

        public async Task<BulkInviteResult> InviteTeamMembersBulkAsync(Guid tenantId, List<TeamInviteRequest> requests)
        {
            var results = new List<InviteStatus>();
            var successful = 0;
            var failed = 0;

            foreach (var request in requests)
            {
                var result = await InviteTeamMemberAsync(tenantId, request);
                results.Add(new InviteStatus
                {
                    Email = request.Email,
                    Success = result.Success,
                    Message = result.Message,
                    InviteId = result.InviteId
                });

                if (result.Success) successful++;
                else failed++;
            }

            return new BulkInviteResult
            {
                TotalRequested = requests.Count,
                Successful = successful,
                Failed = failed,
                Results = results
            };
        }

        #endregion

        #region Ecosystem Collaboration

        public async Task<EcosystemConnectionResult> RequestPartnerConnectionAsync(Guid tenantId, EcosystemPartnerRequest request)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(tenantId);
                if (tenant == null)
                {
                    return new EcosystemConnectionResult { Success = false, Message = "Tenant not found" };
                }

                // Check for existing pending/active connection
                var existingConnection = await _context.EcosystemConnections
                    .FirstOrDefaultAsync(c => c.TenantId == tenantId &&
                        c.PartnerId == request.PartnerId &&
                        (c.Status == "pending" || c.Status == "approved"));

                if (existingConnection != null)
                {
                    return new EcosystemConnectionResult
                    {
                        Success = false,
                        Message = "A connection with this partner already exists or is pending"
                    };
                }

                // Create ecosystem connection record
                var connection = new EcosystemConnection
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    PartnerType = request.PartnerType,
                    PartnerId = request.PartnerId,
                    PartnerEmail = request.PartnerEmail,
                    Purpose = request.ConnectionPurpose,
                    SharedDataTypesJson = System.Text.Json.JsonSerializer.Serialize(request.SharedDataTypes ?? new List<string>()),
                    ExpiresAt = request.ConnectionExpiry,
                    Status = "pending",
                    RequestedAt = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                };

                _context.EcosystemConnections.Add(connection);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Ecosystem connection requested: {ConnectionId} from tenant {TenantId} to partner {PartnerId}",
                    connection.Id, tenantId, request.PartnerId);

                return new EcosystemConnectionResult
                {
                    Success = true,
                    ConnectionId = connection.Id,
                    Status = "pending",
                    Message = "Connection request sent successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting ecosystem connection for tenant {TenantId}", tenantId);
                return new EcosystemConnectionResult { Success = false, Message = "Connection request failed" };
            }
        }

        public async Task<List<EcosystemPartnerDto>> GetAvailablePartnersAsync(string sector)
        {
            // Try to get partners from database first
            var dbPartners = await _context.Set<EcosystemPartner>()
                .Where(p => p.IsActive && (string.IsNullOrEmpty(sector) || p.Sector == sector || p.Sector == "all"))
                .OrderByDescending(p => p.Rating)
                .ThenByDescending(p => p.ConnectionsCount)
                .Take(20)
                .ToListAsync();

            if (dbPartners.Any())
            {
                return dbPartners.Select(p => new EcosystemPartnerDto
                {
                    PartnerId = p.Id,
                    Name = p.Name ?? "Partner",
                    Type = p.Type ?? "consultant",
                    Sector = p.Sector ?? "general",
                    Description = p.Description ?? "",
                    Services = DeserializeList(p.ServicesJson),
                    Certifications = DeserializeList(p.CertificationsJson),
                    Rating = p.Rating,
                    Connections = p.ConnectionsCount
                }).ToList();
            }

            // Return seed partners for KSA/GCC market
            return GetSeedPartners(sector);
        }

        public async Task<List<EcosystemConnectionDto>> GetEcosystemConnectionsAsync(Guid tenantId)
        {
            var connections = await _context.EcosystemConnections
                .Where(c => c.TenantId == tenantId)
                .OrderByDescending(c => c.RequestedAt)
                .ToListAsync();

            return connections.Select(c => new EcosystemConnectionDto
            {
                ConnectionId = c.Id,
                PartnerId = c.PartnerId ?? Guid.Empty,
                PartnerName = c.PartnerName ?? "Partner",
                PartnerType = c.PartnerType ?? "unknown",
                Status = c.Status ?? "pending",
                Purpose = c.Purpose ?? "",
                ConnectedAt = c.ApprovedAt ?? c.RequestedAt ?? DateTime.UtcNow,
                ExpiresAt = c.ExpiresAt,
                SharedDataTypes = DeserializeList(c.SharedDataTypesJson),
                InteractionsCount = c.InteractionsCount
            }).ToList();
        }

        private List<string> DeserializeList(string? json)
        {
            if (string.IsNullOrEmpty(json)) return new List<string>();
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private List<EcosystemPartnerDto> GetSeedPartners(string sector)
        {
            return new List<EcosystemPartnerDto>
            {
                new()
                {
                    PartnerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Compliance Experts KSA",
                    Type = "consultant",
                    Sector = sector,
                    Description = "Leading GRC consultancy in Saudi Arabia specializing in NCA and SAMA frameworks",
                    Services = new() { "NCA Compliance", "ISO 27001", "SAMA Cybersecurity Framework", "Gap Assessment" },
                    Certifications = new() { "ISO 27001 Lead Auditor", "NCA Approved", "CISM" },
                    Rating = 4.8,
                    Connections = 45
                },
                new()
                {
                    PartnerId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Audit Partners Arabia",
                    Type = "auditor",
                    Sector = sector,
                    Description = "Professional audit services with Big 4 affiliate status",
                    Services = new() { "Internal Audit", "External Audit", "SOC 2 Attestation", "Compliance Audit" },
                    Certifications = new() { "SOCPA Licensed", "CPA", "CISA" },
                    Rating = 4.6,
                    Connections = 32
                },
                new()
                {
                    PartnerId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "SecureTech Solutions",
                    Type = "vendor",
                    Sector = sector,
                    Description = "Comprehensive cybersecurity tools and managed security services",
                    Services = new() { "SIEM", "Vulnerability Management", "Penetration Testing", "SOC Services" },
                    Certifications = new() { "ISO 27001", "SOC 2 Type II", "CREST Certified" },
                    Rating = 4.5,
                    Connections = 78
                },
                new()
                {
                    PartnerId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Name = "GCC Risk Advisors",
                    Type = "consultant",
                    Sector = sector,
                    Description = "Enterprise risk management consulting for GCC organizations",
                    Services = new() { "ERM Framework", "Risk Assessment", "Business Continuity", "Third-Party Risk" },
                    Certifications = new() { "CRISC", "ISO 31000 Lead Implementer", "CBCP" },
                    Rating = 4.7,
                    Connections = 28
                },
                new()
                {
                    PartnerId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    Name = "CloudShield MENA",
                    Type = "vendor",
                    Sector = sector,
                    Description = "Cloud security and data protection solutions for Middle East enterprises",
                    Services = new() { "CASB", "DLP", "Cloud Posture Management", "Zero Trust Architecture" },
                    Certifications = new() { "CSA STAR", "ISO 27017", "ISO 27018" },
                    Rating = 4.4,
                    Connections = 56
                }
            };
        }

        public async Task<bool> ApprovePartnerConnectionAsync(Guid connectionId, string approvedBy)
        {
            try
            {
                var connection = await _context.EcosystemConnections.FindAsync(connectionId);
                if (connection == null) return false;

                connection.Status = "approved";
                connection.ApprovedAt = DateTime.UtcNow;
                connection.ModifiedBy = approvedBy;
                connection.ModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Partner connection {ConnectionId} approved by {ApprovedBy}",
                    connectionId, approvedBy);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving partner connection {ConnectionId}", connectionId);
                return false;
            }
        }

        public async Task<bool> RejectPartnerConnectionAsync(Guid connectionId, string reason, string rejectedBy)
        {
            try
            {
                var connection = await _context.EcosystemConnections.FindAsync(connectionId);
                if (connection == null) return false;

                connection.Status = "rejected";
                connection.RejectedAt = DateTime.UtcNow;
                connection.RejectionReason = reason;
                connection.ModifiedBy = rejectedBy;
                connection.ModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Partner connection {ConnectionId} rejected by {RejectedBy}: {Reason}",
                    connectionId, rejectedBy, reason);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting partner connection {ConnectionId}", connectionId);
                return false;
            }
        }

        public async Task TrackPartnerInteractionAsync(Guid connectionId, string interactionType, string details)
        {
            try
            {
                var connection = await _context.EcosystemConnections.FindAsync(connectionId);
                if (connection == null) return;

                connection.InteractionsCount++;
                connection.LastInteractionAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogDebug("Partner interaction tracked: {ConnectionId} - {Type}",
                    connectionId, interactionType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking partner interaction for {ConnectionId}", connectionId);
            }
        }

        public Task<List<IntegrationDto>> GetAvailableIntegrationsAsync(string category)
        {
            var integrations = new List<IntegrationDto>
            {
                new()
                {
                    IntegrationCode = "azure_ad",
                    Name = "Azure Active Directory",
                    Category = "identity",
                    Description = "Single sign-on and user provisioning with Azure AD",
                    LogoUrl = "/images/integrations/azure-ad.svg",
                    IsAvailableInTrial = true,
                    RequiredScopes = new() { "User.Read", "Directory.Read.All" }
                },
                new()
                {
                    IntegrationCode = "okta",
                    Name = "Okta",
                    Category = "identity",
                    Description = "Enterprise identity management with Okta",
                    LogoUrl = "/images/integrations/okta.svg",
                    IsAvailableInTrial = true,
                    RequiredScopes = new() { "okta.users.read", "okta.groups.read" }
                },
                new()
                {
                    IntegrationCode = "jira",
                    Name = "Jira",
                    Category = "ticketing",
                    Description = "Link GRC issues to Jira tickets",
                    LogoUrl = "/images/integrations/jira.svg",
                    IsAvailableInTrial = true,
                    RequiredScopes = new() { "read:jira-work", "write:jira-work" }
                },
                new()
                {
                    IntegrationCode = "servicenow",
                    Name = "ServiceNow",
                    Category = "ticketing",
                    Description = "Integrate with ServiceNow ITSM",
                    LogoUrl = "/images/integrations/servicenow.svg",
                    IsAvailableInTrial = false,
                    RequiredScopes = new() { "incident.read", "incident.write" }
                },
                new()
                {
                    IntegrationCode = "splunk",
                    Name = "Splunk",
                    Category = "siem",
                    Description = "Pull security events from Splunk SIEM",
                    LogoUrl = "/images/integrations/splunk.svg",
                    IsAvailableInTrial = false,
                    RequiredScopes = new() { "search", "saved_searches" }
                },
                new()
                {
                    IntegrationCode = "aws",
                    Name = "Amazon Web Services",
                    Category = "cloud",
                    Description = "Monitor AWS security and compliance posture",
                    LogoUrl = "/images/integrations/aws.svg",
                    IsAvailableInTrial = true,
                    RequiredScopes = new() { "SecurityAudit", "ReadOnlyAccess" }
                },
                new()
                {
                    IntegrationCode = "azure",
                    Name = "Microsoft Azure",
                    Category = "cloud",
                    Description = "Monitor Azure security and compliance posture",
                    LogoUrl = "/images/integrations/azure.svg",
                    IsAvailableInTrial = true,
                    RequiredScopes = new() { "Reader", "Security Reader" }
                },
                new()
                {
                    IntegrationCode = "gcp",
                    Name = "Google Cloud Platform",
                    Category = "cloud",
                    Description = "Monitor GCP security and compliance posture",
                    LogoUrl = "/images/integrations/gcp.svg",
                    IsAvailableInTrial = true,
                    RequiredScopes = new() { "roles/viewer", "roles/securityReviewer" }
                }
            };

            if (!string.IsNullOrEmpty(category))
            {
                integrations = integrations.Where(i => i.Category == category).ToList();
            }

            return Task.FromResult(integrations);
        }

        public async Task<IntegrationResult> ConnectIntegrationAsync(Guid tenantId, string integrationCode, Dictionary<string, string> config)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(tenantId);
                if (tenant == null)
                {
                    return new IntegrationResult
                    {
                        Success = false,
                        IntegrationCode = integrationCode,
                        Status = "failed",
                        Message = "Tenant not found"
                    };
                }

                // Validate integration code
                var availableIntegrations = await GetAvailableIntegrationsAsync("");
                var integration = availableIntegrations.FirstOrDefault(i => i.IntegrationCode == integrationCode);

                if (integration == null)
                {
                    return new IntegrationResult
                    {
                        Success = false,
                        IntegrationCode = integrationCode,
                        Status = "failed",
                        Message = "Integration not found"
                    };
                }

                // Check trial restrictions
                if (tenant.IsTrial && !integration.IsAvailableInTrial)
                {
                    return new IntegrationResult
                    {
                        Success = false,
                        IntegrationCode = integrationCode,
                        Status = "restricted",
                        Message = "This integration is not available during trial. Please upgrade to connect."
                    };
                }

                // TODO: Actually connect to the integration (OAuth flow, API key validation, etc.)
                _logger.LogInformation("Integration {IntegrationCode} connected for tenant {TenantId}",
                    integrationCode, tenantId);

                return new IntegrationResult
                {
                    Success = true,
                    IntegrationCode = integrationCode,
                    Status = "connected",
                    Message = $"Successfully connected to {integration.Name}",
                    ConnectionDetails = new Dictionary<string, string>
                    {
                        { "connected_at", DateTime.UtcNow.ToString("O") },
                        { "integration_name", integration.Name }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting integration {IntegrationCode} for tenant {TenantId}",
                    integrationCode, tenantId);
                return new IntegrationResult
                {
                    Success = false,
                    IntegrationCode = integrationCode,
                    Status = "error",
                    Message = "Failed to connect integration"
                };
            }
        }

        #endregion

        #region Private Helpers

        private string GenerateActivationToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("/", "_")
                .Replace("+", "-")
                .TrimEnd('=');
        }

        private string GenerateSlug(string companyName)
        {
            return companyName
                .ToLowerInvariant()
                .Replace(" ", "-")
                .Replace(".", "")
                + "-" + Guid.NewGuid().ToString("N")[..6];
        }

        private string DetermineTrialStatus(Tenant tenant, int daysRemaining)
        {
            if (!tenant.IsTrial || tenant.SubscriptionStartDate != default)
                return "converted";
            if (daysRemaining <= 0)
                return "expired";
            if (daysRemaining <= 2)
                return "expiring_soon";
            if (tenant.ActivatedAt != null)
                return "active";
            return "pending_activation";
        }

        private decimal GetPlanPrice(string planCode)
        {
            return planCode.ToLower() switch
            {
                "starter" => 99m,
                "professional" => 299m,
                "enterprise" => 999m,
                _ => 199m
            };
        }

        #endregion
    }
}
