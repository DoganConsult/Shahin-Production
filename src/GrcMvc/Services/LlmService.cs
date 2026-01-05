using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GrcMvc.Models.Entities;
using GrcMvc.Data;

namespace GrcMvc.Services
{
    /// <summary>
    /// Enterprise LLM Service for multi-tenant AI-powered insights
    /// Supports OpenAI, Azure OpenAI, and local LLMs
    /// Auto-feeds app with analysis, summaries, recommendations
    /// </summary>
    public interface ILlmService
    {
        Task<string> GenerateWorkflowInsightAsync(Guid workflowInstanceId, string context);
        Task<string> GenerateRiskAnalysisAsync(Guid riskId, string riskDescription);
        Task<string> GenerateComplianceRecommendationAsync(Guid assessmentId, string findings);
        Task<string> GenerateTaskSummaryAsync(Guid taskId, string taskDescription);
        Task<string> GenerateAuditFindingRemedyAsync(Guid findingId, string description);
        Task<LlmResponse> CallLlmAsync(Guid tenantId, string prompt, string context = "");
        Task<bool> IsLlmEnabledAsync(Guid tenantId);
        Task<LlmConfiguration> GetTenantLlmConfigAsync(Guid tenantId);
    }

    public class LlmService : ILlmService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<LlmService> _logger;
        private readonly HttpClient _httpClient;

        public LlmService(GrcDbContext context, ILogger<LlmService> logger, HttpClient httpClient)
        {
            _context = context;
            _logger = logger;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Generate workflow execution insights
        /// </summary>
        public async Task<string> GenerateWorkflowInsightAsync(Guid workflowInstanceId, string context)
        {
            try
            {
                var workflow = await _context.WorkflowInstances
                    .Include(w => w.WorkflowDefinition)
                    .Include(w => w.Tasks)
                    .FirstOrDefaultAsync(w => w.Id == workflowInstanceId);

                if (workflow == null)
                    return "Workflow not found";

                var prompt = $@"
Analyze this workflow execution and provide insights:

Workflow: {workflow.WorkflowDefinition?.Name}
Status: {workflow.Status}
Started: {workflow.StartedAt}
Tasks Completed: {workflow.Tasks.Count(t => t.Status == "Completed")} of {workflow.Tasks.Count}
Context: {context}

Provide:
1. Execution status summary
2. Potential bottlenecks
3. Recommended next steps
4. Any risks or delays";

                var tenantId = workflow.TenantId;
                var response = await CallLlmAsync(tenantId, prompt, "workflow-analysis");
                return response.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error generating workflow insight: {ex.Message}");
                return "Unable to generate insight";
            }
        }

        /// <summary>
        /// Generate risk analysis insights
        /// </summary>
        public async Task<string> GenerateRiskAnalysisAsync(Guid riskId, string riskDescription)
        {
            try
            {
                var risk = await _context.Risks.FirstOrDefaultAsync(r => r.Id == riskId);
                if (risk == null)
                    return "Risk not found";

                var prompt = $@"
Analyze this risk and provide insights:

Risk: {risk.Name}
Category: {risk.Category}
Owner: {risk.Owner}
Description: {riskDescription}

Provide:
1. Risk assessment summary
2. Potential impact on organization
3. Recommended mitigation strategies
4. Control recommendations
5. Priority level (High/Medium/Low)";

                var tenantId = risk.TenantId ?? Guid.Empty;
                var response = await CallLlmAsync(tenantId, prompt, "risk-analysis");
                return response.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error generating risk analysis: {ex.Message}");
                return "Unable to generate analysis";
            }
        }

        /// <summary>
        /// Generate compliance recommendations
        /// </summary>
        public async Task<string> GenerateComplianceRecommendationAsync(Guid assessmentId, string findings)
        {
            try
            {
                var assessment = await _context.Assessments.FirstOrDefaultAsync(a => a.Id == assessmentId);
                if (assessment == null)
                    return "Assessment not found";

                var prompt = $@"
Based on this compliance assessment, provide recommendations:

Assessment: {assessment.Name}
Type: {assessment.Type}
Status: {assessment.Status}
Findings: {findings}

Provide:
1. Compliance gaps identified
2. Regulatory requirements not met
3. Recommended remediation steps
4. Implementation timeline
5. Resource requirements";

                var tenantId = assessment.TenantId ?? Guid.Empty;
                var response = await CallLlmAsync(tenantId, prompt, "compliance-analysis");
                return response.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error generating compliance recommendation: {ex.Message}");
                return "Unable to generate recommendation";
            }
        }

        /// <summary>
        /// Generate task execution summary
        /// </summary>
        public async Task<string> GenerateTaskSummaryAsync(Guid taskId, string taskDescription)
        {
            try
            {
                var task = await _context.WorkflowTasks
                    .Include(t => t.WorkflowInstance)
                    .FirstOrDefaultAsync(t => t.Id == taskId);

                if (task == null)
                    return "Task not found";

                var prompt = $@"
Summarize this task execution:

Task: {task.TaskName}
Status: {task.Status}
Priority: {task.Priority}
Assigned: {task.AssignedToUserName}
Description: {taskDescription}
Started: {task.StartedAt}

Provide:
1. Task completion status
2. Progress summary
3. Outstanding items
4. Recommended actions
5. SLA status";

                var tenantId = task.WorkflowInstance?.TenantId ?? Guid.Empty;
                var response = await CallLlmAsync(tenantId, prompt, "task-analysis");
                return response.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error generating task summary: {ex.Message}");
                return "Unable to generate summary";
            }
        }

        /// <summary>
        /// Generate remediation recommendations for audit findings
        /// </summary>
        public async Task<string> GenerateAuditFindingRemedyAsync(Guid findingId, string description)
        {
            try
            {
                var finding = await _context.AuditFindings.FirstOrDefaultAsync(f => f.Id == findingId);
                if (finding == null)
                    return "Finding not found";

                var prompt = $@"
Provide remediation strategy for this audit finding:

Finding: {finding.FindingNumber}
Category: {finding.Category}
Severity: {finding.Severity}
Description: {description}

Provide:
1. Root cause analysis
2. Remediation steps
3. Prevention measures
4. Expected timeline
5. Success metrics
6. Resource needs";

                var tenantId = finding.TenantId ?? Guid.Empty;
                var response = await CallLlmAsync(tenantId, prompt, "audit-remediation");
                return response.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error generating audit finding remedy: {ex.Message}");
                return "Unable to generate remedy";
            }
        }

        /// <summary>
        /// Generic LLM call with multi-tenant support
        /// </summary>
        public async Task<LlmResponse> CallLlmAsync(Guid tenantId, string prompt, string context = "")
        {
            try
            {
                // Get tenant LLM configuration
                var config = await GetTenantLlmConfigAsync(tenantId);
                if (config == null || !config.IsActive || !config.EnabledForTenant)
                {
                    _logger.LogWarning($"⚠️ LLM not enabled for tenant {tenantId}");
                    return new LlmResponse { Success = false, Content = "LLM not configured for this tenant" };
                }

                // Check usage limit
                if (config.MonthlyUsageLimit > 0 && config.CurrentMonthUsage >= config.MonthlyUsageLimit)
                {
                    _logger.LogWarning($"⚠️ Monthly LLM usage limit reached for tenant {tenantId}");
                    return new LlmResponse { Success = false, Content = "Monthly usage limit reached" };
                }

                // Call LLM based on provider
                string response = config.Provider.ToLower() switch
                {
                    "openai" => await CallOpenAiAsync(config, prompt),
                    "azureopenai" => await CallAzureOpenAiAsync(config, prompt),
                    "local" => await CallLocalLlmAsync(config, prompt),
                    _ => throw new InvalidOperationException($"Unknown LLM provider: {config.Provider}")
                };

                // Increment usage counter
                config.CurrentMonthUsage++;
                _context.LlmConfigurations.Update(config);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ LLM call successful for tenant {tenantId} (context: {context})");

                return new LlmResponse
                {
                    Success = true,
                    Content = response,
                    Provider = config.Provider,
                    Model = config.ModelName,
                    Context = context,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error calling LLM: {ex.Message}");
                return new LlmResponse
                {
                    Success = false,
                    Content = $"Error: {ex.Message}",
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// Check if LLM is enabled for tenant
        /// </summary>
        public async Task<bool> IsLlmEnabledAsync(Guid tenantId)
        {
            var config = await GetTenantLlmConfigAsync(tenantId);
            return config?.IsActive == true && config.EnabledForTenant == true;
        }

        /// <summary>
        /// Get LLM configuration for tenant
        /// </summary>
        public async Task<LlmConfiguration> GetTenantLlmConfigAsync(Guid tenantId)
        {
            try
            {
                var config = await _context.LlmConfigurations
                    .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.IsActive);

                // Reset monthly usage if month changed
                if (config != null && config.LastUsageResetDate.HasValue)
                {
                    if (config.LastUsageResetDate.Value.Month != DateTime.UtcNow.Month)
                    {
                        config.CurrentMonthUsage = 0;
                        config.LastUsageResetDate = DateTime.UtcNow;
                        _context.LlmConfigurations.Update(config);
                        await _context.SaveChangesAsync();
                    }
                }

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting LLM config: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Call OpenAI API
        /// </summary>
        private async Task<string> CallOpenAiAsync(LlmConfiguration config, string prompt)
        {
            try
            {
                var request = new
                {
                    model = config.ModelName,
                    messages = new[]
                    {
                        new { role = "system", content = "You are an enterprise governance, risk, and compliance assistant." },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = config.MaxTokens,
                    temperature = (double)config.Temperature
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}");

                var response = await _httpClient.PostAsync(
                    "https://api.openai.com/v1/chat/completions",
                    content);

                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException($"OpenAI API error: {response.StatusCode}");

                var responseContent = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;

                return root
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "No response from OpenAI";
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ OpenAI API error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Call Azure OpenAI API
        /// </summary>
        private async Task<string> CallAzureOpenAiAsync(LlmConfiguration config, string prompt)
        {
            try
            {
                var request = new
                {
                    messages = new[]
                    {
                        new { role = "system", content = "You are an enterprise governance, risk, and compliance assistant." },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = config.MaxTokens,
                    temperature = (double)config.Temperature
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("api-key", config.ApiKey);

                var response = await _httpClient.PostAsync(
                    config.ApiEndpoint,
                    content);

                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException($"Azure OpenAI API error: {response.StatusCode}");

                var responseContent = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;

                return root
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "No response from Azure OpenAI";
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Azure OpenAI API error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Call local LLM (for on-premises deployments)
        /// </summary>
        private async Task<string> CallLocalLlmAsync(LlmConfiguration config, string prompt)
        {
            try
            {
                var request = new
                {
                    prompt = prompt,
                    model = config.ModelName,
                    max_tokens = config.MaxTokens,
                    temperature = (double)config.Temperature
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                _httpClient.DefaultRequestHeaders.Clear();

                var response = await _httpClient.PostAsync(
                    config.ApiEndpoint,
                    content);

                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException($"Local LLM error: {response.StatusCode}");

                var responseContent = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;

                return root.GetProperty("result").GetString() ?? "No response from local LLM";
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Local LLM error: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>
    /// LLM Response model
    /// </summary>
    public class LlmResponse
    {
        public bool Success { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Error { get; set; }
    }
}
