//=============================================================================
// SHAHIN GRC PLATFORM - AGENT GUARDRAIL CODE
// Runtime Enforcement for ABP Multi-Tenant GRC Platform
// Author: Ahmet (PhD IoT Healthcare Cybersecurity)
// Version: 1.0
//=============================================================================

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Volo.Abp.Users;
using Volo.Abp.AspNetCore.Mvc.ApplicationConfigurations;
using Volo.Abp.Authorization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using System.Net;

namespace Shahin.Grc.Guardrails
{
    //=============================================================================
    // 1. ONBOARDING REDIRECT MIDDLEWARE (CORE ENFORCEMENT)
    //=============================================================================
    
    public class OnboardingRedirectMiddleware : ITransientDependency
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<OnboardingRedirectMiddleware> _logger;
        private readonly OnboardingRedirectOptions _options;

        public OnboardingRedirectMiddleware(
            RequestDelegate next,
            ILogger<OnboardingRedirectMiddleware> logger,
            IOptions<OnboardingRedirectOptions> options)
        {
            _next = next;
            _logger = logger;
            _options = options.Value;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ICurrentTenant currentTenant,
            ICurrentUser currentUser,
            IOnboardingStateService onboardingService)
        {
            // Skip if not authenticated or in allowed paths
            if (!currentUser.IsAuthenticated || IsAllowedPath(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Require tenant context for authenticated requests
            if (!currentTenant.IsAvailable)
            {
                await HandleNoTenantContext(context);
                return;
            }

            try
            {
                // Check onboarding status
                var onboardingState = await onboardingService.GetCurrentStateAsync();
                
                if (onboardingState.Status != OnboardingStatus.Completed)
                {
                    // Determine redirect URL based on current step
                    var redirectUrl = GetRedirectUrl(onboardingState, context.Request.Path);
                    
                    if (!string.IsNullOrEmpty(redirectUrl))
                    {
                        _logger.LogInformation(
                            "Redirecting user {UserId} in tenant {TenantId} from {CurrentPath} to {RedirectUrl} due to incomplete onboarding",
                            currentUser.Id, currentTenant.Id, context.Request.Path, redirectUrl);

                        context.Response.Redirect(redirectUrl);
                        return;
                    }
                }

                // Continue pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in onboarding redirect middleware");
                throw;
            }
        }

        private bool IsAllowedPath(string path)
        {
            var allowedPaths = new[]
            {
                "/api/health",
                "/Account/Login",
                "/Account/Logout", 
                "/Account/Register",
                "/onboarding/",
                "/api/public/",
                "/trial/",
                "/assets/",
                "/favicon.ico"
            };

            return allowedPaths.Any(allowed => 
                path.StartsWith(allowed, StringComparison.OrdinalIgnoreCase));
        }

        private string GetRedirectUrl(OnboardingStateDto state, string currentPath)
        {
            // Avoid infinite loops
            if (currentPath.StartsWith("/onboarding/", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return state.Status switch
            {
                OnboardingStatus.Pending => "/onboarding/fast-start",
                OnboardingStatus.InProgress => state.ResumeUrl ?? "/onboarding/continue",
                OnboardingStatus.Quarantined => "/account/quarantine",
                OnboardingStatus.Expired => "/account/trial-expired",
                _ => null
            };
        }

        private async Task HandleNoTenantContext(HttpContext context)
        {
            _logger.LogWarning("Authenticated request without tenant context: {Path}", context.Request.Path);
            
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            context.Response.ContentType = "application/json";
            
            var error = new
            {
                error = "tenant_context_required",
                message = "This resource requires a valid tenant context"
            };
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(error));
        }
    }

    public class OnboardingRedirectOptions
    {
        public List<string> AllowedPaths { get; set; } = new List<string>();
        public string DefaultRedirectUrl { get; set; } = "/onboarding/fast-start";
        public bool EnableStrictMode { get; set; } = true;
    }

    //=============================================================================
    // 2. TENANT ISOLATION GUARD
    //=============================================================================
    
    public class TenantIsolationGuard : ITransientDependency
    {
        private readonly ICurrentTenant _currentTenant;
        private readonly ICurrentUser _currentUser;
        private readonly ILogger<TenantIsolationGuard> _logger;

        public TenantIsolationGuard(
            ICurrentTenant currentTenant,
            ICurrentUser currentUser,
            ILogger<TenantIsolationGuard> logger)
        {
            _currentTenant = currentTenant;
            _currentUser = currentUser;
            _logger = logger;
        }

        public void EnsureTenantContext(string operation = null)
        {
            if (!_currentTenant.IsAvailable)
            {
                var exception = new AbpAuthorizationException(
                    "TENANT_CONTEXT_REQUIRED",
                    $"Operation '{operation}' requires tenant context but none is available.");
                
                _logger.LogWarning(exception, 
                    "Tenant context violation - User: {UserId}, Operation: {Operation}",
                    _currentUser.Id, operation);
                
                throw exception;
            }
        }

        public void EnsureUserBelongsToTenant(Guid? userTenantId = null, string operation = null)
        {
            EnsureTenantContext(operation);
            
            var tenantId = userTenantId ?? _currentUser.TenantId;
            
            if (tenantId != _currentTenant.Id)
            {
                var exception = new AbpAuthorizationException(
                    "TENANT_ISOLATION_VIOLATION",
                    $"User belongs to tenant '{tenantId}' but current context is '{_currentTenant.Id}'");
                
                _logger.LogError(exception,
                    "SECURITY VIOLATION - Cross-tenant access attempt - User: {UserId}, UserTenant: {UserTenant}, CurrentTenant: {CurrentTenant}, Operation: {Operation}",
                    _currentUser.Id, tenantId, _currentTenant.Id, operation);
                
                throw exception;
            }
        }

        public async Task EnsureDataBelongsToTenantAsync<T>(T entity, string operation = null) 
            where T : class, IMultiTenant
        {
            EnsureTenantContext(operation);
            
            if (entity.TenantId != _currentTenant.Id)
            {
                var exception = new AbpAuthorizationException(
                    "TENANT_DATA_ISOLATION_VIOLATION",
                    $"Attempted to access data belonging to tenant '{entity.TenantId}' from context '{_currentTenant.Id}'");
                
                _logger.LogError(exception,
                    "SECURITY VIOLATION - Cross-tenant data access - Entity: {EntityType}, EntityTenant: {EntityTenant}, CurrentTenant: {CurrentTenant}, Operation: {Operation}",
                    typeof(T).Name, entity.TenantId, _currentTenant.Id, operation);
                
                throw exception;
            }
        }
    }

    //=============================================================================
    // 3. TRIAL & SECURITY ENFORCEMENT
    //=============================================================================
    
    public class TrialSecurityGuard : ITransientDependency
    {
        private readonly ITrialService _trialService;
        private readonly ITenantSecurityProfileService _securityProfileService;
        private readonly ILogger<TrialSecurityGuard> _logger;
        private readonly ICurrentTenant _currentTenant;

        public TrialSecurityGuard(
            ITrialService trialService,
            ITenantSecurityProfileService securityProfileService,
            ILogger<TrialSecurityGuard> logger,
            ICurrentTenant currentTenant)
        {
            _trialService = trialService;
            _securityProfileService = securityProfileService;
            _logger = logger;
            _currentTenant = currentTenant;
        }

        public async Task EnforceTrialLimitsAsync(string operation, object parameters = null)
        {
            if (!_currentTenant.IsAvailable) return;

            var trialInfo = await _trialService.GetTrialInfoAsync(_currentTenant.Id.Value);
            var securityProfile = await _securityProfileService.GetProfileAsync(_currentTenant.Id.Value);

            // Check trial expiry
            if (trialInfo.IsExpired)
            {
                throw new TrialExpiredException(
                    $"Trial expired on {trialInfo.ExpiryDate}. Operation '{operation}' is not allowed.");
            }

            // Check if tenant is quarantined
            if (trialInfo.IsQuarantined)
            {
                if (!IsQuarantineAllowedOperation(operation))
                {
                    throw new TenantQuarantinedException(
                        $"Tenant is quarantined. Operation '{operation}' is restricted. Reason: {trialInfo.QuarantineReason}");
                }
            }

            // Enforce feature restrictions
            await EnforceFeatureRestrictionsAsync(operation, securityProfile);
            
            // Enforce rate limits
            await EnforceRateLimitsAsync(operation, trialInfo);
        }

        private bool IsQuarantineAllowedOperation(string operation)
        {
            var allowedOperations = new[]
            {
                "read_basic_data",
                "update_profile", 
                "verify_email",
                "contact_support"
            };

            return allowedOperations.Contains(operation.ToLowerInvariant());
        }

        private async Task EnforceFeatureRestrictionsAsync(string operation, SecurityProfile profile)
        {
            var restrictedFeatures = profile.RestrictedFeatures ?? new List<string>();
            
            if (restrictedFeatures.Contains(operation))
            {
                _logger.LogWarning(
                    "Feature restriction violation - Tenant: {TenantId}, Operation: {Operation}",
                    _currentTenant.Id, operation);
                
                throw new FeatureRestrictedException(
                    $"Operation '{operation}' is restricted for this tenant tier.");
            }

            // Enforce quantitative limits
            switch (operation.ToLowerInvariant())
            {
                case "create_user":
                    await EnforceUserLimitAsync(profile.MaxUsers);
                    break;
                case "create_assessment":
                    await EnforceAssessmentLimitAsync(profile.MaxAssessments);
                    break;
                case "add_framework":
                    await EnforceFrameworkLimitAsync(profile.MaxFrameworks);
                    break;
            }
        }

        private async Task EnforceRateLimitsAsync(string operation, TrialInfo trialInfo)
        {
            // Implementation depends on your rate limiting strategy
            // Could use distributed cache, database, or external service
            
            var rateLimitKey = $"rate_limit:{_currentTenant.Id}:{operation}";
            // Check and increment rate limit counters
            // Throw RateLimitExceededException if limits exceeded
        }

        private async Task EnforceUserLimitAsync(int maxUsers)
        {
            // Implementation to check current user count vs limit
        }

        private async Task EnforceAssessmentLimitAsync(int maxAssessments)
        {
            // Implementation to check current assessment count vs limit
        }

        private async Task EnforceFrameworkLimitAsync(int maxFrameworks)
        {
            // Implementation to check current framework count vs limit
        }
    }

    //=============================================================================
    // 4. AUDIT TRAIL ENFORCEMENT
    //=============================================================================
    
    public class AuditTrailEnforcer : ITransientDependency
    {
        private readonly IAuditEventService _auditEventService;
        private readonly ICurrentTenant _currentTenant;
        private readonly ICurrentUser _currentUser;
        private readonly ILogger<AuditTrailEnforcer> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditTrailEnforcer(
            IAuditEventService auditEventService,
            ICurrentTenant currentTenant,
            ICurrentUser currentUser,
            ILogger<AuditTrailEnforcer> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _auditEventService = auditEventService;
            _currentTenant = currentTenant;
            _currentUser = currentUser;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogDecisionAsync(string decisionType, object inputs, object outputs, string rationale)
        {
            try
            {
                var auditEvent = new AuditEventDto
                {
                    TenantId = _currentTenant.Id ?? Guid.Empty,
                    EventType = "AUTOMATED_DECISION",
                    EntityType = decisionType,
                    Action = "EVALUATE",
                    UserId = _currentUser.Id,
                    UserEmail = _currentUser.Email,
                    Timestamp = DateTime.UtcNow,
                    BeforeValues = new Dictionary<string, object> { ["inputs"] = inputs },
                    AfterValues = new Dictionary<string, object> { ["outputs"] = outputs },
                    Reason = rationale,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = GetUserAgent()
                };

                await _auditEventService.CreateAsync(auditEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log audit event for decision type: {DecisionType}", decisionType);
                // Don't throw - audit failure shouldn't break business logic
            }
        }

        public async Task LogOperationAsync(string operationType, string entityType, Guid? entityId, 
            object beforeValues, object afterValues, string reason = null)
        {
            try
            {
                var auditEvent = new AuditEventDto
                {
                    TenantId = _currentTenant.Id ?? Guid.Empty,
                    EventType = operationType,
                    EntityType = entityType,
                    EntityId = entityId,
                    Action = operationType,
                    UserId = _currentUser.Id,
                    UserEmail = _currentUser.Email,
                    Timestamp = DateTime.UtcNow,
                    BeforeValues = beforeValues as Dictionary<string, object> ?? new Dictionary<string, object>(),
                    AfterValues = afterValues as Dictionary<string, object> ?? new Dictionary<string, object>(),
                    Reason = reason,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = GetUserAgent()
                };

                await _auditEventService.CreateAsync(auditEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log audit event for operation: {OperationType}", operationType);
                // Don't throw - audit failure shouldn't break business logic
            }
        }

        private string GetClientIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "Unknown";

            var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                           context.Request.Headers["X-Real-IP"].FirstOrDefault() ??
                           context.Connection.RemoteIpAddress?.ToString();

            return ipAddress ?? "Unknown";
        }

        private string GetUserAgent()
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
        }
    }

    //=============================================================================
    // 5. RULES ENGINE ENFORCEMENT
    //=============================================================================
    
    public class RulesEngineGuard : ITransientDependency
    {
        private readonly IRulesEngineService _rulesEngine;
        private readonly AuditTrailEnforcer _auditTrailEnforcer;
        private readonly ILogger<RulesEngineGuard> _logger;

        public RulesEngineGuard(
            IRulesEngineService rulesEngine,
            AuditTrailEnforcer auditTrailEnforcer,
            ILogger<RulesEngineGuard> logger)
        {
            _rulesEngine = rulesEngine;
            _auditTrailEnforcer = auditTrailEnforcer;
            _logger = logger;
        }

        public async Task<T> EvaluateWithAuditAsync<T>(string ruleSetId, object inputs, string operationContext = null)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                // Execute rules evaluation
                var result = await _rulesEngine.EvaluateAsync<T>(ruleSetId, inputs);
                
                // Extract explainability
                var explainability = await _rulesEngine.GetExplainabilityAsync(ruleSetId, inputs);
                
                // Log decision with full audit trail
                await _auditTrailEnforcer.LogDecisionAsync(
                    $"RULES_EVALUATION_{ruleSetId}",
                    inputs,
                    result,
                    explainability.HumanReadableExplanation);

                _logger.LogInformation(
                    "Rules evaluation completed - RuleSet: {RuleSetId}, Duration: {Duration}ms, Context: {Context}",
                    ruleSetId, (DateTime.UtcNow - startTime).TotalMilliseconds, operationContext);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Rules evaluation failed - RuleSet: {RuleSetId}, Context: {Context}", 
                    ruleSetId, operationContext);
                
                // Still log the attempt for audit purposes
                await _auditTrailEnforcer.LogDecisionAsync(
                    $"RULES_EVALUATION_ERROR_{ruleSetId}",
                    inputs,
                    null,
                    $"Error: {ex.Message}");
                
                throw;
            }
        }

        public async Task ValidateOutputsAsync(string ruleSetId, object inputs, object outputs)
        {
            var validation = await _rulesEngine.ValidateOutputsAsync(ruleSetId, inputs, outputs);
            
            if (!validation.IsValid)
            {
                var errors = string.Join("; ", validation.Errors.Select(e => $"{e.Field}: {e.Message}"));
                
                _logger.LogError(
                    "Rules output validation failed - RuleSet: {RuleSetId}, Errors: {Errors}",
                    ruleSetId, errors);
                
                throw new RulesValidationException($"Rules output validation failed: {errors}");
            }
        }
    }

    //=============================================================================
    // 6. FRAUD DETECTION MIDDLEWARE
    //=============================================================================
    
    public class FraudDetectionMiddleware : ITransientDependency
    {
        private readonly RequestDelegate _next;
        private readonly IFraudDetectionService _fraudService;
        private readonly ILogger<FraudDetectionMiddleware> _logger;
        private readonly FraudDetectionOptions _options;

        public FraudDetectionMiddleware(
            RequestDelegate next,
            IFraudDetectionService fraudService,
            ILogger<FraudDetectionMiddleware> logger,
            IOptions<FraudDetectionOptions> options)
        {
            _next = next;
            _fraudService = fraudService;
            _logger = logger;
            _options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only check high-risk endpoints
            if (IsMonitoredEndpoint(context.Request.Path))
            {
                var riskAssessment = await _fraudService.AssessRiskAsync(context);
                
                if (riskAssessment.RiskLevel >= FraudRiskLevel.High)
                {
                    _logger.LogWarning(
                        "High fraud risk detected - IP: {IP}, Path: {Path}, Risk: {Risk}, Reasons: {Reasons}",
                        riskAssessment.IpAddress, context.Request.Path, 
                        riskAssessment.RiskLevel, string.Join(", ", riskAssessment.RiskFactors));
                    
                    if (riskAssessment.ShouldBlock)
                    {
                        await HandleFraudBlock(context, riskAssessment);
                        return;
                    }
                    
                    if (riskAssessment.ShouldQuarantine)
                    {
                        // Set quarantine flag for downstream processing
                        context.Items["RequireQuarantine"] = true;
                        context.Items["QuarantineReason"] = riskAssessment.PrimaryReason;
                    }
                }
            }

            await _next(context);
        }

        private bool IsMonitoredEndpoint(string path)
        {
            var monitoredPaths = new[]
            {
                "/trial/register",
                "/api/agent/tenant/create",
                "/account/register",
                "/api/public/trial"
            };

            return monitoredPaths.Any(monitored => 
                path.StartsWith(monitored, StringComparison.OrdinalIgnoreCase));
        }

        private async Task HandleFraudBlock(HttpContext context, FraudRiskAssessment assessment)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                error = "fraud_protection",
                message = "Request blocked due to security policy",
                retryAfter = 3600, // 1 hour
                supportContact = "security@shahin-ai.com"
            };
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }

    //=============================================================================
    // 7. RATE LIMITING GUARD
    //=============================================================================
    
    public class RateLimitGuard : ITransientDependency
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<RateLimitGuard> _logger;
        private readonly RateLimitOptions _options;

        public RateLimitGuard(
            IDistributedCache cache,
            ILogger<RateLimitGuard> logger,
            IOptions<RateLimitOptions> options)
        {
            _cache = cache;
            _logger = logger;
            _options = options.Value;
        }

        public async Task EnforceRateLimitAsync(string operation, string identifier, RateLimitRule rule = null)
        {
            rule ??= _options.GetRuleForOperation(operation);
            if (rule == null) return; // No rate limiting configured

            var key = $"rate_limit:{operation}:{identifier}";
            var window = TimeSpan.FromSeconds(rule.WindowSeconds);
            
            var currentCount = await GetCurrentCountAsync(key, window);
            
            if (currentCount >= rule.Limit)
            {
                _logger.LogWarning(
                    "Rate limit exceeded - Operation: {Operation}, Identifier: {Identifier}, Count: {Count}, Limit: {Limit}",
                    operation, identifier, currentCount, rule.Limit);
                
                throw new RateLimitExceededException(
                    $"Rate limit exceeded for operation '{operation}'. Try again in {rule.WindowSeconds} seconds.");
            }

            await IncrementCountAsync(key, window);
        }

        private async Task<int> GetCurrentCountAsync(string key, TimeSpan window)
        {
            var countStr = await _cache.GetStringAsync(key);
            return int.TryParse(countStr, out var count) ? count : 0;
        }

        private async Task IncrementCountAsync(string key, TimeSpan window)
        {
            var currentCount = await GetCurrentCountAsync(key, window);
            var newCount = currentCount + 1;
            
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = window
            };
            
            await _cache.SetStringAsync(key, newCount.ToString(), options);
        }
    }

    //=============================================================================
    // 8. WORKFLOW ARTIFACT ENFORCER
    //=============================================================================
    
    public class WorkflowArtifactEnforcer : ITransientDependency
    {
        private readonly IWorkflowArtifactService _artifactService;
        private readonly AuditTrailEnforcer _auditEnforcer;
        private readonly ILogger<WorkflowArtifactEnforcer> _logger;

        public WorkflowArtifactEnforcer(
            IWorkflowArtifactService artifactService,
            AuditTrailEnforcer auditEnforcer,
            ILogger<WorkflowArtifactEnforcer> logger)
        {
            _artifactService = artifactService;
            _auditEnforcer = auditEnforcer;
            _logger = logger;
        }

        public async Task EnsureArtifactsExistAsync(Guid tenantId, OnboardingStatus onboardingStatus)
        {
            if (onboardingStatus != OnboardingStatus.Completed) return;

            var missingArtifacts = await _artifactService.GetMissingArtifactsAsync(tenantId);
            
            if (missingArtifacts.Any())
            {
                _logger.LogInformation(
                    "Generating missing workflow artifacts for tenant {TenantId}: {Artifacts}",
                    tenantId, string.Join(", ", missingArtifacts));

                foreach (var artifactType in missingArtifacts)
                {
                    await GenerateArtifactAsync(tenantId, artifactType);
                }
            }
        }

        private async Task GenerateArtifactAsync(Guid tenantId, string artifactType)
        {
            try
            {
                switch (artifactType.ToUpperInvariant())
                {
                    case "GRC_PLAN":
                        await _artifactService.GenerateGrcPlanAsync(tenantId);
                        break;
                    case "ASSESSMENT_TEMPLATES":
                        await _artifactService.GenerateAssessmentTemplatesAsync(tenantId);
                        break;
                    case "EVIDENCE_WORKFLOWS":
                        await _artifactService.GenerateEvidenceWorkflowsAsync(tenantId);
                        break;
                    case "DASHBOARDS":
                        await _artifactService.GenerateDashboardsAsync(tenantId);
                        break;
                    default:
                        _logger.LogWarning("Unknown artifact type: {ArtifactType}", artifactType);
                        break;
                }

                await _auditEnforcer.LogOperationAsync(
                    "WORKFLOW_ARTIFACT_GENERATED",
                    artifactType,
                    tenantId,
                    null,
                    new { ArtifactType = artifactType },
                    "Generated missing workflow artifact after onboarding completion");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Failed to generate artifact {ArtifactType} for tenant {TenantId}",
                    artifactType, tenantId);
                throw;
            }
        }
    }

    //=============================================================================
    // 9. CONFIGURATION CLASSES
    //=============================================================================
    
    public class FraudDetectionOptions
    {
        public bool IsEnabled { get; set; } = true;
        public List<string> MonitoredPaths { get; set; } = new List<string>();
        public FraudRiskLevel BlockThreshold { get; set; } = FraudRiskLevel.Critical;
        public FraudRiskLevel QuarantineThreshold { get; set; } = FraudRiskLevel.High;
    }

    public class RateLimitOptions
    {
        public Dictionary<string, RateLimitRule> Rules { get; set; } = new Dictionary<string, RateLimitRule>();
        
        public RateLimitRule GetRuleForOperation(string operation)
        {
            return Rules.TryGetValue(operation, out var rule) ? rule : null;
        }
    }

    public class RateLimitRule
    {
        public int Limit { get; set; }
        public int WindowSeconds { get; set; }
        public int BurstLimit { get; set; }
    }

    //=============================================================================
    // 10. CUSTOM EXCEPTIONS
    //=============================================================================
    
    public class TrialExpiredException : BusinessException
    {
        public TrialExpiredException(string message) : base("TRIAL_EXPIRED", message) { }
    }

    public class TenantQuarantinedException : BusinessException
    {
        public TenantQuarantinedException(string message) : base("TENANT_QUARANTINED", message) { }
    }

    public class FeatureRestrictedException : BusinessException
    {
        public FeatureRestrictedException(string message) : base("FEATURE_RESTRICTED", message) { }
    }

    public class RateLimitExceededException : BusinessException
    {
        public RateLimitExceededException(string message) : base("RATE_LIMIT_EXCEEDED", message) { }
    }

    public class RulesValidationException : BusinessException
    {
        public RulesValidationException(string message) : base("RULES_VALIDATION_FAILED", message) { }
    }

    //=============================================================================
    // 11. SERVICE INTERFACES (for dependency injection)
    //=============================================================================
    
    public interface IOnboardingStateService
    {
        Task<OnboardingStateDto> GetCurrentStateAsync();
        Task UpdateStateAsync(OnboardingStateDto state);
    }

    public interface ITrialService
    {
        Task<TrialInfo> GetTrialInfoAsync(Guid tenantId);
    }

    public interface ITenantSecurityProfileService
    {
        Task<SecurityProfile> GetProfileAsync(Guid tenantId);
    }

    public interface IAuditEventService
    {
        Task CreateAsync(AuditEventDto auditEvent);
    }

    public interface IRulesEngineService
    {
        Task<T> EvaluateAsync<T>(string ruleSetId, object inputs);
        Task<ExplainabilityDto> GetExplainabilityAsync(string ruleSetId, object inputs);
        Task<ValidationResultDto> ValidateOutputsAsync(string ruleSetId, object inputs, object outputs);
    }

    public interface IFraudDetectionService
    {
        Task<FraudRiskAssessment> AssessRiskAsync(HttpContext context);
    }

    public interface IWorkflowArtifactService
    {
        Task<List<string>> GetMissingArtifactsAsync(Guid tenantId);
        Task GenerateGrcPlanAsync(Guid tenantId);
        Task GenerateAssessmentTemplatesAsync(Guid tenantId);
        Task GenerateEvidenceWorkflowsAsync(Guid tenantId);
        Task GenerateDashboardsAsync(Guid tenantId);
    }

    //=============================================================================
    // 12. VALUE OBJECTS FOR GUARDRAILS
    //=============================================================================
    
    public class TrialInfo
    {
        public Guid TenantId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpiryDate;
        public bool IsQuarantined { get; set; }
        public string QuarantineReason { get; set; }
        public TrialTier Tier { get; set; }
    }

    public class FraudRiskAssessment
    {
        public FraudRiskLevel RiskLevel { get; set; }
        public List<string> RiskFactors { get; set; } = new List<string>();
        public string IpAddress { get; set; }
        public string PrimaryReason { get; set; }
        public bool ShouldBlock => RiskLevel >= FraudRiskLevel.Critical;
        public bool ShouldQuarantine => RiskLevel >= FraudRiskLevel.High;
        public float RiskScore { get; set; }
    }

    public enum FraudRiskLevel
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    //=============================================================================
    // 13. EXTENSION METHODS FOR SERVICE REGISTRATION
    //=============================================================================
    
    public static class GuardrailServiceExtensions
    {
        public static IServiceCollection AddGrcGuardrails(this IServiceCollection services, Action<GrcGuardrailOptions> configure = null)
        {
            var options = new GrcGuardrailOptions();
            configure?.Invoke(options);

            // Register all guardrail services
            services.AddTransient<TenantIsolationGuard>();
            services.AddTransient<TrialSecurityGuard>();
            services.AddTransient<AuditTrailEnforcer>();
            services.AddTransient<RulesEngineGuard>();
            services.AddTransient<RateLimitGuard>();
            services.AddTransient<WorkflowArtifactEnforcer>();

            // Register options
            services.Configure<OnboardingRedirectOptions>(opt => 
            {
                opt.AllowedPaths = options.OnboardingAllowedPaths;
                opt.EnableStrictMode = options.EnableStrictOnboardingMode;
            });

            services.Configure<FraudDetectionOptions>(opt =>
            {
                opt.IsEnabled = options.EnableFraudDetection;
                opt.MonitoredPaths = options.FraudMonitoredPaths;
            });

            services.Configure<RateLimitOptions>(opt =>
            {
                opt.Rules = options.RateLimitRules;
            });

            return services;
        }
    }

    public class GrcGuardrailOptions
    {
        public List<string> OnboardingAllowedPaths { get; set; } = new List<string>();
        public bool EnableStrictOnboardingMode { get; set; } = true;
        public bool EnableFraudDetection { get; set; } = true;
        public List<string> FraudMonitoredPaths { get; set; } = new List<string>();
        public Dictionary<string, RateLimitRule> RateLimitRules { get; set; } = new Dictionary<string, RateLimitRule>();
    }
}

//=============================================================================
// END OF AGENT GUARDRAIL CODE
//=============================================================================
