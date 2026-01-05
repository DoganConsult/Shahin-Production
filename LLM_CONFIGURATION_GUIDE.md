# LLM Configuration & Setup Guide

## Quick Start

### 1. Database Migration
```bash
cd /home/dogan/grc-system
dotnet ef database update --project src/GrcMvc --context GrcDbContext
```

### 2. Configure LLM for Your Tenant

#### Option A: OpenAI
```sql
INSERT INTO "LlmConfigurations" 
(
    "Id", "TenantId", "Provider", "ApiEndpoint", "ApiKey", "ModelName", 
    "MaxTokens", "Temperature", "IsActive", "EnabledForTenant", 
    "MonthlyUsageLimit", "CurrentMonthUsage", "ConfiguredDate", "CreatedDate"
)
VALUES
(
    gen_random_uuid(),
    'YOUR-TENANT-ID',
    'openai',
    'https://api.openai.com/v1/chat/completions',
    'sk-YOUR-API-KEY',
    'gpt-4',
    2000,
    0.7,
    true,
    true,
    10000,
    0,
    NOW(),
    NOW()
);
```

**Get OpenAI API Key:**
1. Go to https://platform.openai.com/api-keys
2. Create new secret key
3. Copy and paste above

#### Option B: Azure OpenAI
```sql
INSERT INTO "LlmConfigurations" 
(
    "Id", "TenantId", "Provider", "ApiEndpoint", "ApiKey", "ModelName", 
    "MaxTokens", "Temperature", "IsActive", "EnabledForTenant", 
    "MonthlyUsageLimit", "CurrentMonthUsage", "ConfiguredDate", "CreatedDate"
)
VALUES
(
    gen_random_uuid(),
    'YOUR-TENANT-ID',
    'azureopenai',
    'https://YOUR-RESOURCE.openai.azure.com/openai/deployments/YOUR-DEPLOYMENT/chat/completions?api-version=2024-02-15-preview',
    'YOUR-API-KEY',
    'gpt-4',
    2000,
    0.7,
    true,
    true,
    10000,
    0,
    NOW(),
    NOW()
);
```

**Get Azure OpenAI Details:**
1. Go to https://portal.azure.com
2. Find your OpenAI resource
3. Copy "Key" and "Endpoint"
4. Create deployment and note its name

#### Option C: Local LLM (Ollama)
```sql
INSERT INTO "LlmConfigurations" 
(
    "Id", "TenantId", "Provider", "ApiEndpoint", "ApiKey", "ModelName", 
    "MaxTokens", "Temperature", "IsActive", "EnabledForTenant", 
    "MonthlyUsageLimit", "CurrentMonthUsage", "ConfiguredDate", "CreatedDate"
)
VALUES
(
    gen_random_uuid(),
    'YOUR-TENANT-ID',
    'local',
    'http://localhost:11434/api/generate',
    'unused',
    'llama2',
    2000,
    0.7,
    true,
    true,
    0,
    0,
    NOW(),
    NOW()
);
```

**Run Ollama Locally:**
```bash
# Download and run Ollama
ollama run llama2

# Or via Docker
docker run -d -p 11434:11434 ollama/ollama
docker exec <container> ollama pull llama2
```

### 3. Verify Configuration
```bash
# Check if LLM is enabled for tenant
SELECT * FROM "LlmConfigurations" WHERE "TenantId" = 'YOUR-TENANT-ID';

# Should see:
# IsActive = true
# EnabledForTenant = true
# Provider = 'openai' | 'azureopenai' | 'local'
```

### 4. Test LLM Integration
Create a test file `TestLlmService.cs`:

```csharp
using GrcMvc.Services;
using GrcMvc.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

// In your test/configuration
var services = new ServiceCollection();
services.AddScoped<ILlmService, LlmService>();
services.AddHttpClient<ILlmService, LlmService>();

var provider = services.BuildServiceProvider();
var llmService = provider.GetRequiredService<ILlmService>();

// Test call
var response = await llmService.CallLlmAsync(
    tenantId: Guid.Parse("YOUR-TENANT-ID"),
    prompt: "What are the key steps for risk mitigation?",
    context: "risk-analysis");

Console.WriteLine($"Success: {response.Success}");
Console.WriteLine($"Content: {response.Content}");
Console.WriteLine($"Provider: {response.Provider}");
Console.WriteLine($"Model: {response.Model}");
```

---

## AI Features in Action

### 1. Workflow Insights
```csharp
var insight = await _llmService.GenerateWorkflowInsightAsync(
    workflowId,
    "Risk assessment process in progress");

// Output:
// "The risk assessment workflow is progressing well. 
//  3 of 5 approval stages complete. 
//  Estimated completion: 2 days. 
//  No blockers detected."
```

### 2. Risk Analysis
```csharp
var analysis = await _llmService.GenerateRiskAnalysisAsync(
    riskId,
    "Cyber attack targeting customer data");

// Output:
// "Critical risk requiring immediate mitigation:
//  - Implement 2FA for all systems
//  - Conduct security audit
//  - Update incident response plan
//  - Timeline: 30 days
//  - Resources: Security team (3), IT (2)"
```

### 3. Compliance Recommendations
```csharp
var recommendations = await _llmService.GenerateComplianceRecommendationAsync(
    assessmentId,
    "PDPL compliance gaps identified");

// Output:
// "PDPL Assessment Findings:
//  Gap 1: Data retention policy missing
//  - Action: Create retention policy
//  - Owner: Legal team
//  - Timeline: 2 weeks
//  
//  Gap 2: User consent not documented
//  - Action: Implement consent tracking
//  - Owner: Product team
//  - Timeline: 1 month"
```

### 4. Task Summary
```csharp
var summary = await _llmService.GenerateTaskSummaryAsync(
    taskId,
    "Review and approve vendor security assessment");

// Output:
// "Task Status: In Progress
//  Progress: 70% complete
//  - Assessment document reviewed ✓
//  - Pending: Legal review
//  - Recommendation: Approve with conditions
//  - Next step: Route to compliance team"
```

---

## Monitoring & Troubleshooting

### Check Monthly Usage
```sql
SELECT 
    "TenantId",
    "Provider",
    "ModelName",
    "CurrentMonthUsage",
    "MonthlyUsageLimit",
    ROUND(("CurrentMonthUsage"::float / "MonthlyUsageLimit" * 100)::numeric, 2) AS "UsagePercent",
    "LastUsageResetDate"
FROM "LlmConfigurations"
WHERE "IsActive" = true
ORDER BY "CurrentMonthUsage" DESC;
```

### Debug LLM Calls
```csharp
// Check logs for LLM activity
tail -f /app/logs/grcmvc-*.log | grep "LLM"

// Expected output:
// ✅ LLM call successful for tenant [...] (context: workflow-analysis)
// ⚠️ LLM not enabled for tenant [...]
// ❌ Error calling LLM: Connection timeout
```

### Common Issues

#### ❌ "LLM not configured for this tenant"
**Solution**: Insert LlmConfiguration record for tenant

```sql
INSERT INTO "LlmConfigurations" (...) VALUES (...)
```

#### ❌ "Monthly usage limit reached"
**Solution**: Increase `MonthlyUsageLimit` or reset `CurrentMonthUsage`

```sql
UPDATE "LlmConfigurations"
SET "MonthlyUsageLimit" = 50000
WHERE "TenantId" = 'YOUR-TENANT-ID';
```

#### ❌ "API authentication failed"
**Solution**: Verify API key and endpoint

```sql
-- Check configuration
SELECT "ApiKey", "ApiEndpoint", "Provider" 
FROM "LlmConfigurations"
WHERE "TenantId" = 'YOUR-TENANT-ID';

-- Test with curl
curl -H "Authorization: Bearer YOUR-API-KEY" \
  https://api.openai.com/v1/models
```

#### ❌ "Local LLM connection refused"
**Solution**: Ensure Ollama is running

```bash
# Check if Ollama is running
curl http://localhost:11434/api/tags

# Start Ollama if not running
ollama serve
```

---

## Cost Estimation

### OpenAI Pricing (as of 2024)
- **gpt-4**: $0.03 per 1K input tokens + $0.06 per 1K output tokens
- **gpt-3.5-turbo**: $0.0005 per 1K input tokens + $0.0015 per 1K output tokens

### Azure OpenAI Pricing
- **Pay-per-token**: Similar to OpenAI
- **Provisioned throughput**: Fixed price for guaranteed capacity

### Monthly Cost Examples

| Model | Monthly Calls | Avg Tokens | Cost |
|-------|---------------|-----------|------|
| gpt-3.5-turbo | 10,000 | 500 in/200 out | ~$5 |
| gpt-4 | 10,000 | 500 in/200 out | ~$200 |
| Local (Ollama) | 10,000 | N/A | $0 |

---

## Best Practices

### 1. Cost Control
```csharp
// Set monthly limits based on budget
config.MonthlyUsageLimit = 10000;  // 10k calls/month

// Use cheaper models for simple tasks
if (isSimpleTask)
    config.ModelName = "gpt-3.5-turbo";
else
    config.ModelName = "gpt-4";
```

### 2. Performance
```csharp
// Use appropriate temperature
Temperature = 0.3m,  // More deterministic for structured tasks
Temperature = 0.9m,  // More creative for ideation
```

### 3. Security
```csharp
// Never log API keys
_logger.LogInformation($"Config: {config.Provider}");  // ✓ Safe

_logger.LogInformation($"Key: {config.ApiKey}");       // ✗ Dangerous
```

### 4. Fallback Strategy
```csharp
try
{
    return await _llmService.GenerateWorkflowInsightAsync(workflowId, context);
}
catch
{
    // Use predefined templates if LLM unavailable
    return "Workflow in progress. Check details for status.";
}
```

---

## Support & Documentation

### API Documentation
- OpenAI: https://platform.openai.com/docs
- Azure OpenAI: https://learn.microsoft.com/en-us/azure/ai-services/openai/
- Ollama: https://ollama.ai

### Troubleshooting
1. Check logs: `/app/logs/grcmvc-*.log`
2. Verify configuration: `SELECT * FROM "LlmConfigurations"`
3. Test connectivity: Use curl or Postman
4. Check usage: Monitor `CurrentMonthUsage`

### Contact
For issues or questions, check:
- Application logs in `/app/logs/`
- GRC system documentation
- LLM provider support channels
