# Diagnostic Agent Guide - Using AI to Diagnose Errors & Issues

This guide explains how to use the **Diagnostic Agent Service** to automatically analyze errors, conditions, problems, and issues in your GRC application using AI-powered diagnostics.

---

## 🎯 Overview

The Diagnostic Agent Service uses **Claude AI** to:
- ✅ Analyze error logs and patterns
- ✅ Diagnose specific errors with root cause analysis
- ✅ Monitor application health conditions
- ✅ Detect recurring patterns and trends
- ✅ Provide proactive recommendations
- ✅ Generate alerts for critical conditions

---

## 📋 Prerequisites

1. **Claude API Key** configured in `appsettings.json`:
```json
{
  "ClaudeAgents": {
    "ApiKey": "sk-ant-api03-xxxxx"
  }
}
```

2. **Service Registration** (already done in `Program.cs`):
```csharp
builder.Services.AddScoped<IDiagnosticAgentService, DiagnosticAgentService>();
```

3. **Database** with `AuditEvents` table for error logging

---

## 🚀 API Endpoints

### 1. Analyze Recent Errors

**GET** `/api/diagnostic/errors`

Analyze errors from the last 24 hours (or custom period) and get insights.

**Query Parameters:**
- `hoursBack` (optional): Number of hours to look back (default: 24)
- `severity` (optional): Filter by severity (Critical, High, Medium, Low)
- `tenantId` (optional): Filter by tenant

**Example Request:**
```bash
GET /api/diagnostic/errors?hoursBack=48&severity=Critical
```

**Example Response:**
```json
{
  "generatedAt": "2025-01-06T14:30:00Z",
  "analysisPeriod": "24:00:00",
  "totalErrors": 45,
  "criticalErrors": 2,
  "highErrors": 8,
  "mediumErrors": 25,
  "lowErrors": 10,
  "errorSummaries": [
    {
      "errorType": "DatabaseConnectionException",
      "exceptionType": "System.Data.SqlClient.SqlException",
      "occurrenceCount": 12,
      "firstOccurrence": "2025-01-05T10:00:00Z",
      "lastOccurrence": "2025-01-06T14:25:00Z",
      "severity": "High",
      "affectedTenants": ["tenant-1", "tenant-2"]
    }
  ],
  "patterns": [
    {
      "patternType": "recurring",
      "description": "Database connection errors spike every 2 hours",
      "frequency": 6,
      "trend": "increasing",
      "rootCause": "Connection pool exhaustion",
      "suggestedFix": "Increase connection pool size or implement connection retry logic"
    }
  ],
  "insights": [
    {
      "category": "reliability",
      "title": "Database Connection Issues",
      "description": "Recurring database connection failures indicate potential connection pool exhaustion",
      "severity": "High",
      "impact": "Users experiencing intermittent failures"
    }
  ],
  "recommendations": [
    {
      "category": "database",
      "title": "Optimize Connection Pool",
      "description": "Increase connection pool size and implement retry logic",
      "priority": "High",
      "estimatedEffort": 60,
      "steps": [
        "Update connection string with Max Pool Size=200",
        "Add Polly retry policy for database operations",
        "Monitor connection pool metrics"
      ]
    }
  ],
  "overallStatus": "Degraded",
  "healthScore": 65
}
```

---

### 2. Diagnose Specific Error

**POST** `/api/diagnostic/errors/diagnose`

Get detailed diagnosis for a specific error with root cause analysis and fix suggestions.

**Request Body:**
```json
{
  "errorId": "error-12345",
  "exceptionType": "System.NullReferenceException",
  "stackTrace": "at GrcMvc.Services.RiskService.GetRiskAsync(...)",
  "context": "User was viewing risk details page"
}
```

**Example Response:**
```json
{
  "errorId": "error-12345",
  "diagnosedAt": "2025-01-06T14:30:00Z",
  "severity": "High",
  "errorType": "System.NullReferenceException",
  "rootCause": "Risk entity not found in database but code assumes it exists",
  "explanation": "The GetRiskAsync method attempts to access properties of a null Risk entity, indicating the risk was deleted or never existed",
  "contributingFactors": [
    "Missing null check before property access",
    "No validation that risk exists before returning",
    "Race condition: risk deleted between query and access"
  ],
  "fixSuggestions": [
    {
      "title": "Add Null Check",
      "description": "Check if risk is null before accessing properties",
      "priority": "Critical",
      "codeExample": "if (risk == null) throw new NotFoundException(\"Risk not found\");",
      "estimatedEffort": 5
    }
  ],
  "preventionSteps": [
    "Add null checks in all service methods",
    "Use null-conditional operators (?.)",
    "Add integration tests for edge cases"
  ],
  "relatedCodeLocation": "GrcMvc/Services/RiskService.cs:45"
}
```

---

### 3. Analyze Application Health

**GET** `/api/diagnostic/health`

Get comprehensive health analysis including database, performance, and system metrics.

**Query Parameters:**
- `tenantId` (optional): Filter by tenant

**Example Request:**
```bash
GET /api/diagnostic/health
```

**Example Response:**
```json
{
  "diagnosedAt": "2025-01-06T14:30:00Z",
  "overallStatus": "Healthy",
  "healthScore": 85,
  "checkResults": [
    {
      "checkName": "Database",
      "status": "Healthy",
      "description": "Database connection successful",
      "duration": "00:00:00.150"
    }
  ],
  "issues": [
    {
      "category": "performance",
      "title": "High Response Time",
      "description": "Average response time increased by 30%",
      "severity": "Medium",
      "impact": "User experience degradation",
      "affectedComponent": "API"
    }
  ],
  "recommendations": [
    {
      "category": "performance",
      "title": "Optimize Database Queries",
      "description": "Add indexes to frequently queried columns",
      "priority": "Medium",
      "estimatedEffort": 30
    }
  ]
}
```

---

### 4. Detect Patterns

**GET** `/api/diagnostic/patterns`

Detect recurring patterns, trends, and correlations in errors over time.

**Query Parameters:**
- `daysBack` (optional): Number of days to analyze (default: 7)
- `tenantId` (optional): Filter by tenant

**Example Request:**
```bash
GET /api/diagnostic/patterns?daysBack=14
```

**Example Response:**
```json
{
  "analyzedAt": "2025-01-06T14:30:00Z",
  "analysisPeriod": "7.00:00:00",
  "patterns": [
    {
      "patternType": "time-based",
      "description": "Errors spike during business hours (9 AM - 5 PM)",
      "frequency": 45,
      "trend": "stable",
      "rootCause": "Higher user load during business hours",
      "suggestedFix": "Scale resources during peak hours"
    }
  ],
  "trends": [
    {
      "metric": "Error Rate",
      "trend": "decreasing",
      "changePercentage": -15.5,
      "interpretation": "Error rate improving over time"
    }
  ],
  "correlations": [
    {
      "event1": "DatabaseConnectionException",
      "event2": "HighUserLoad",
      "correlationStrength": 0.82,
      "relationship": "Strong positive correlation - connection errors increase with user load"
    }
  ],
  "summary": "Overall system health improving. Main concern is database connection pool during peak hours."
}
```

---

### 5. Root Cause Analysis

**POST** `/api/diagnostic/root-cause`

Perform deep root cause analysis for a specific problem.

**Request Body:**
```json
{
  "problemDescription": "Users cannot submit assessments - form hangs and times out",
  "context": {
    "affectedTenants": ["tenant-1", "tenant-2"],
    "timeOfDay": "afternoon",
    "userCount": 150
  }
}
```

**Example Response:**
```json
{
  "analyzedAt": "2025-01-06T14:30:00Z",
  "problemDescription": "Users cannot submit assessments",
  "rootCause": "Database timeout due to long-running query in AssessmentService.SaveAsync",
  "contributingFactors": [
    "Missing database index on Assessment.TenantId",
    "N+1 query problem loading related entities",
    "Synchronous database operations blocking thread"
  ],
  "symptoms": [
    "Form submission hangs",
    "Request timeout after 30 seconds",
    "High database CPU usage"
  ],
  "fixes": [
    {
      "title": "Add Database Index",
      "description": "Create index on Assessment.TenantId to speed up queries",
      "priority": "Critical",
      "codeExample": "CREATE INDEX IX_Assessment_TenantId ON Assessments(TenantId);",
      "estimatedEffort": 5
    },
    {
      "title": "Fix N+1 Query",
      "description": "Use Include() to eager load related entities",
      "priority": "High",
      "codeExample": "_dbContext.Assessments.Include(a => a.Requirements).Where(...)",
      "estimatedEffort": 15
    }
  ],
  "confidence": "High",
  "evidence": {
    "queryDuration": "28.5 seconds",
    "databaseCpu": "95%",
    "missingIndexes": ["IX_Assessment_TenantId"]
  }
}
```

---

### 6. Get Recommendations

**GET** `/api/diagnostic/recommendations`

Get proactive recommendations to prevent future issues.

**Query Parameters:**
- `tenantId` (optional): Filter by tenant

**Example Request:**
```bash
GET /api/diagnostic/recommendations
```

**Example Response:**
```json
[
  {
    "category": "security",
    "title": "Add Input Validation",
    "description": "Several endpoints lack input validation, increasing XSS risk",
    "priority": "High",
    "impact": "Prevent security vulnerabilities",
    "estimatedEffort": 60,
    "steps": [
      "Add [Required] and [StringLength] attributes to DTOs",
      "Implement FluentValidation for complex validation",
      "Add input sanitization middleware"
    ]
  },
  {
    "category": "performance",
    "title": "Implement Caching",
    "description": "Frequently accessed data should be cached",
    "priority": "Medium",
    "estimatedEffort": 120,
    "steps": [
      "Add Redis cache for framework data",
      "Cache user permissions",
      "Implement cache invalidation strategy"
    ]
  }
]
```

---

### 7. Monitor Conditions & Alerts

**GET** `/api/diagnostic/alerts`

Get real-time alerts for critical conditions.

**Query Parameters:**
- `tenantId` (optional): Filter by tenant

**Example Request:**
```bash
GET /api/diagnostic/alerts
```

**Example Response:**
```json
[
  {
    "id": "alert-123",
    "createdAt": "2025-01-06T14:30:00Z",
    "severity": "Critical",
    "category": "Errors",
    "title": "Critical Errors Detected",
    "message": "3 critical error(s) occurred in the last hour",
    "affectedComponent": "API",
    "isAcknowledged": false
  },
  {
    "id": "alert-124",
    "createdAt": "2025-01-06T14:25:00Z",
    "severity": "High",
    "category": "Database",
    "title": "Database Health Issue",
    "message": "Database connection pool at 95% capacity",
    "affectedComponent": "Database",
    "isAcknowledged": false
  }
]
```

---

## 💻 Usage Examples

### C# Service Usage

```csharp
public class MyService
{
    private readonly IDiagnosticAgentService _diagnosticAgent;

    public MyService(IDiagnosticAgentService diagnosticAgent)
    {
        _diagnosticAgent = diagnosticAgent;
    }

    public async Task HandleErrorAsync(string errorId, Exception ex)
    {
        // Get detailed diagnosis
        var diagnosis = await _diagnosticAgent.DiagnoseErrorAsync(
            errorId,
            ex.GetType().Name,
            ex.StackTrace,
            $"User: {User.Identity?.Name}, Action: {ActionName}");

        // Log the diagnosis
        _logger.LogWarning("Error diagnosed: {RootCause}", diagnosis.RootCause);

        // Apply fix suggestions
        foreach (var fix in diagnosis.FixSuggestions.Where(f => f.Priority == "Critical"))
        {
            _logger.LogInformation("Applying fix: {Title}", fix.Title);
            // Apply fix...
        }
    }
}
```

### JavaScript/TypeScript Usage

```typescript
// Analyze errors
const response = await fetch('/api/diagnostic/errors?hoursBack=24');
const report = await response.json();

console.log(`Total Errors: ${report.totalErrors}`);
console.log(`Health Score: ${report.healthScore}`);

// Get recommendations
const recommendations = await fetch('/api/diagnostic/recommendations');
const recs = await recommendations.json();

recs.forEach(rec => {
    console.log(`${rec.priority}: ${rec.title}`);
});
```

### Background Job Usage

```csharp
public class DiagnosticMonitoringJob
{
    private readonly IDiagnosticAgentService _diagnosticAgent;

    public async Task ExecuteAsync()
    {
        // Monitor conditions
        var alerts = await _diagnosticAgent.MonitorConditionsAsync();

        foreach (var alert in alerts.Where(a => a.Severity == "Critical"))
        {
            // Send notification
            await _notificationService.SendAlertAsync(alert);
        }

        // Get recommendations
        var recommendations = await _diagnosticAgent.GetRecommendationsAsync();
        
        // Log high-priority recommendations
        foreach (var rec in recommendations.Where(r => r.Priority == "High"))
        {
            _logger.LogInformation("Recommendation: {Title}", rec.Title);
        }
    }
}
```

---

## 🔧 Configuration

### appsettings.json

```json
{
  "ClaudeAgents": {
    "ApiKey": "sk-ant-api03-xxxxx",
    "Model": "claude-3-5-sonnet-20241022",
    "MaxTokens": 4096
  },
  "DiagnosticAgent": {
    "DefaultHoursBack": 24,
    "DefaultDaysBack": 7,
    "AlertThresholds": {
      "CriticalErrorsPerHour": 1,
      "HighErrorsPerHour": 5,
      "HealthScoreWarning": 70,
      "HealthScoreCritical": 50
    }
  }
}
```

---

## 📊 Best Practices

1. **Regular Monitoring**: Set up a background job to call `/api/diagnostic/alerts` every 5-10 minutes
2. **Error Analysis**: After critical errors, immediately call `/api/diagnostic/errors/diagnose`
3. **Pattern Detection**: Run `/api/diagnostic/patterns` weekly to identify trends
4. **Health Checks**: Monitor `/api/diagnostic/health` as part of your health check endpoint
5. **Recommendations**: Review `/api/diagnostic/recommendations` during sprint planning

---

## 🎯 Use Cases

### Use Case 1: Production Error Investigation

**Scenario**: Critical error occurs in production

**Steps**:
1. Get error ID from logs
2. Call `POST /api/diagnostic/errors/diagnose` with error details
3. Review root cause and fix suggestions
4. Apply critical fixes immediately
5. Monitor with `GET /api/diagnostic/alerts`

### Use Case 2: Performance Degradation

**Scenario**: Application slowing down

**Steps**:
1. Call `GET /api/diagnostic/health` to get health analysis
2. Review performance issues and recommendations
3. Call `GET /api/diagnostic/patterns?daysBack=7` to see trends
4. Implement recommended optimizations

### Use Case 3: Proactive Maintenance

**Scenario**: Prevent issues before they occur

**Steps**:
1. Daily: Check `GET /api/diagnostic/recommendations`
2. Weekly: Review `GET /api/diagnostic/patterns?daysBack=7`
3. Monthly: Full analysis with `GET /api/diagnostic/errors?hoursBack=720`

---

## 🚨 Troubleshooting

### Issue: "Claude API key not configured"

**Solution**: Add `ClaudeAgents:ApiKey` to `appsettings.json`

### Issue: "No errors found"

**Solution**: Ensure errors are being logged to `AuditEvents` table with `EventType = "Error"`

### Issue: "AI response is empty"

**Solution**: Check Claude API key is valid and has sufficient credits

---

## 📝 Next Steps

1. ✅ Configure Claude API key
2. ✅ Test endpoints with Postman/curl
3. ✅ Set up background job for monitoring
4. ✅ Integrate alerts into notification system
5. ✅ Review recommendations regularly

---

**The Diagnostic Agent is now ready to help you diagnose and fix issues automatically!** 🎉
