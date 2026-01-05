using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Stub rules engine service for development/testing
    /// </summary>
    public class StubRulesEngineService : IRulesEngineService
    {
        private readonly ILogger<StubRulesEngineService> _logger;

        public StubRulesEngineService(ILogger<StubRulesEngineService> logger)
        {
            _logger = logger;
        }

        public Task<List<Control>> DeriveApplicableControlsAsync(Guid tenantId, List<Guid> frameworkIds)
        {
            _logger.LogInformation("🔧 [STUB] DeriveApplicableControls for tenant {TenantId}", tenantId);
            return Task.FromResult(new List<Control>());
        }

        public Task<bool> EvaluateRuleAsync(string ruleExpression, Dictionary<string, object> context)
        {
            _logger.LogInformation("🔧 [STUB] EvaluateRule: {Expression}", ruleExpression);
            return Task.FromResult(true);
        }

        public Task<RuleExecutionLog> EvaluateRulesAsync(Guid tenantId, OrganizationProfile profile, Ruleset ruleset, string userId)
        {
            _logger.LogInformation("🔧 [STUB] EvaluateRulesAsync for tenant {TenantId}", tenantId);
            return Task.FromResult(new RuleExecutionLog { Id = Guid.NewGuid(), Status = "Completed" });
        }

        public Task<RuleExecutionLog> DeriveAndPersistScopeAsync(Guid tenantId, string userId)
        {
            _logger.LogInformation("🔧 [STUB] DeriveAndPersistScopeAsync for tenant {TenantId}", tenantId);
            return Task.FromResult(new RuleExecutionLog { Id = Guid.NewGuid(), Status = "Completed" });
        }
    }
}
