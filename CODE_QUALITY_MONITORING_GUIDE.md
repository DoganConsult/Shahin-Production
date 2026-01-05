# Code Quality Monitoring with Claude Sub-Agents

**Generated**: 2026-01-04
**Application**: GrcMvc
**Status**: Ready for Deployment

---

## Overview

This system uses Claude AI sub-agents to continuously monitor code quality, detect security vulnerabilities, and send alerts. It integrates with CI/CD pipelines via webhooks and provides real-time feedback on code changes.

## Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                     Code Quality Monitoring System                   │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐          │
│  │   GitHub     │    │   GitLab     │    │   Generic    │          │
│  │   Webhook    │    │   Webhook    │    │   Webhook    │          │
│  └──────┬───────┘    └──────┬───────┘    └──────┬───────┘          │
│         │                   │                   │                   │
│         └───────────────────┼───────────────────┘                   │
│                             ▼                                        │
│                   ┌─────────────────┐                               │
│                   │  API Controller │                               │
│                   │ /api/codequality│                               │
│                   └────────┬────────┘                               │
│                            │                                         │
│         ┌──────────────────┼──────────────────┐                     │
│         ▼                  ▼                  ▼                     │
│  ┌─────────────┐   ┌─────────────┐   ┌─────────────┐               │
│  │  Code       │   │  Alert      │   │  Hangfire   │               │
│  │  Quality    │   │  Service    │   │  Background │               │
│  │  Service    │   │             │   │  Jobs       │               │
│  └──────┬──────┘   └──────┬──────┘   └──────┬──────┘               │
│         │                 │                 │                       │
│         ▼                 │                 │                       │
│  ┌─────────────┐          │                 │                       │
│  │  Claude AI  │          │                 │                       │
│  │  Sub-Agents │          │                 │                       │
│  │  ┌────────┐ │          │                 │                       │
│  │  │Security│ │          │                 │                       │
│  │  │Scanner │ │          │                 │                       │
│  │  ├────────┤ │          │                 │                       │
│  │  │Code    │ │          │                 │                       │
│  │  │Reviewer│ │          ▼                 │                       │
│  │  ├────────┤ │   ┌─────────────┐          │                       │
│  │  │Perform.│ │   │   Alert     │          │                       │
│  │  │Analyzer│ │   │  Channels   │          │                       │
│  │  ├────────┤ │   │ ┌─────────┐ │          │                       │
│  │  │Arch.   │ │   │ │  Email  │ │          │                       │
│  │  │Analyzer│ │   │ ├─────────┤ │          │                       │
│  │  └────────┘ │   │ │  Slack  │ │          │                       │
│  └─────────────┘   │ ├─────────┤ │          │                       │
│                    │ │  Teams  │ │          │                       │
│                    │ ├─────────┤ │          │                       │
│                    │ │ Webhook │ │          │                       │
│                    │ └─────────┘ │          │                       │
│                    └─────────────┘          │                       │
│                                             │                       │
└─────────────────────────────────────────────┴───────────────────────┘
```

## Claude Sub-Agents

### Available Agent Types

| Agent Type | Description | Use Case |
|------------|-------------|----------|
| `code-reviewer` | General code quality review | Code smells, best practices, maintainability |
| `security-scanner` | Security vulnerability scanning | OWASP Top 10, SQL injection, XSS, secrets |
| `performance-analyzer` | Performance issue detection | N+1 queries, memory leaks, async issues |
| `dependency-checker` | Dependency vulnerability check | CVEs, outdated packages, license issues |
| `architecture-analyzer` | Architecture and design analysis | SOLID violations, circular dependencies |
| `code-style` | Code style and formatting | Naming conventions, formatting |
| `documentation` | Documentation completeness | Missing docs, outdated comments |
| `test-coverage` | Test coverage analysis | Missing tests, coverage gaps |

### Agent Response Format

All agents return JSON with:

```json
{
  "severity": "critical|high|medium|low|info",
  "score": 0-100,
  "summary": "Brief assessment",
  "issues": [
    {
      "line": 42,
      "type": "security",
      "severity": "high",
      "description": "SQL injection vulnerability",
      "suggestion": "Use parameterized queries",
      "cwe": "CWE-89"
    }
  ],
  "recommendations": ["List of suggestions"]
}
```

## API Endpoints

### Analyze Code

```bash
POST /api/codequality/analyze
Content-Type: application/json

{
  "agentType": "security-scanner",
  "code": "public void DoSomething() { ... }",
  "filePath": "Controllers/UserController.cs",
  "language": "C#",
  "sendAlerts": true
}
```

### Full Scan

```bash
POST /api/codequality/scan
Content-Type: application/json

{
  "code": "...",
  "filePath": "MyFile.cs"
}
```

### Get Metrics

```bash
GET /api/codequality/metrics?from=2026-01-01&to=2026-01-04
Authorization: Bearer <token>
```

### Get Alerts

```bash
GET /api/codequality/alerts?severity=critical&from=2026-01-01
Authorization: Bearer <token>
```

### Trigger Manual Scan

```bash
POST /api/codequality/scan/trigger
Authorization: Bearer <admin-token>
```

### Trigger Security Audit

```bash
POST /api/codequality/audit/trigger
Authorization: Bearer <admin-token>
```

### Test Alert

```bash
POST /api/codequality/alerts/test
Authorization: Bearer <admin-token>

{
  "severity": "info",
  "message": "This is a test alert"
}
```

## Webhook Integration

### GitHub

```bash
POST /api/codequality/webhook/github
X-GitHub-Event: push

# Automatically triggered on push events
# Analyzes changed .cs, .razor, .cshtml files
```

Configure in GitHub repository:
1. Go to Settings → Webhooks → Add webhook
2. Payload URL: `https://your-domain.com/api/codequality/webhook/github`
3. Content type: `application/json`
4. Events: `Push`, `Pull request`

### GitLab

```bash
POST /api/codequality/webhook/gitlab
X-Gitlab-Event: Push Hook
```

Configure in GitLab project:
1. Go to Settings → Webhooks
2. URL: `https://your-domain.com/api/codequality/webhook/gitlab`
3. Trigger: Push events, Merge request events

### Generic Webhook

```bash
POST /api/codequality/webhook
Content-Type: application/json

{
  "repository": "my-repo",
  "branch": "main",
  "commitSha": "abc123",
  "files": ["src/MyFile.cs", "src/Another.cs"]
}
```

## Configuration

### Environment Variables

```bash
# Claude AI Configuration
CLAUDE_API_KEY=sk-ant-api03-xxxxx

# Code Quality Settings
CODE_QUALITY_ENABLED=true
CODE_QUALITY_SCAN_INTERVAL_HOURS=24

# Alert Channels
ENABLE_SLACK_ALERTS=true
ENABLE_TEAMS_ALERTS=false
SLACK_WEBHOOK_URL=https://hooks.slack.com/services/xxx/yyy/zzz
TEAMS_WEBHOOK_URL=
CUSTOM_WEBHOOK_URL=https://your-webhook.com/alerts

# Alert Thresholds
ALERT_MIN_SCORE_THRESHOLD=50
ALERT_EMAIL_RECIPIENTS=admin@example.com,security@example.com
```

### appsettings.json

```json
{
  "Claude": {
    "ApiKey": "",
    "Model": "claude-sonnet-4-20250514",
    "MaxTokens": 4096
  },
  "CodeQuality": {
    "Enabled": true,
    "ProjectPath": "/app",
    "FilePatterns": ["*.cs", "*.razor", "*.cshtml"],
    "ExcludePatterns": ["/obj/", "/bin/", "/Migrations/"]
  },
  "Alerts": {
    "EnableEmail": true,
    "EnableSlack": true,
    "EnableTeams": false,
    "AlertOnSeverities": ["critical", "high"],
    "MinScoreThreshold": 50
  }
}
```

## Background Jobs

### Scheduled Jobs

| Job | Schedule | Description |
|-----|----------|-------------|
| Code Quality Scan | Daily 2 AM | Scans recently modified files |
| Security Audit | Weekly Sunday 3 AM | Full security audit of all files |
| Daily Report | Weekdays 8 AM | Summary report of code quality |

### Cron Schedules

```csharp
// Daily scan at 2 AM UTC
"0 2 * * *"

// Weekly on Sunday at 3 AM UTC
"0 3 * * 0"

// Weekdays at 8 AM UTC
"0 8 * * 1-5"
```

### Manual Triggers

Access Hangfire Dashboard: `https://your-domain.com/hangfire`

Or via API:
```bash
# Trigger scan
POST /api/codequality/scan/trigger

# Trigger audit
POST /api/codequality/audit/trigger
```

## Alert Channels

### Email Alerts

HTML-formatted emails with:
- Severity color-coded header
- File details
- Issue summary table
- Top issues with line numbers
- Suggestions for fixes

### Slack Alerts

Rich message with:
- Color-coded attachment
- File, score, issue count fields
- Timestamp
- Link to details

### Microsoft Teams Alerts

Adaptive card with:
- Theme color based on severity
- Facts table
- Markdown message body

### Custom Webhook

JSON payload with full alert object:
```json
{
  "id": "guid",
  "createdAt": "2026-01-04T12:00:00Z",
  "severity": "high",
  "title": "Security Alert: Issues Found",
  "message": "...",
  "filePath": "src/Controllers/UserController.cs",
  "agentType": "security-scanner",
  "score": 45,
  "issueCount": 5,
  "topIssues": [...],
  "repositoryUrl": "...",
  "commitSha": "abc123",
  "branch": "main"
}
```

## Alert Thresholds

### Security Score

| Score | Severity | Alert |
|-------|----------|-------|
| < 30 | Critical | Immediate |
| 30-50 | High | Within 1 hour |
| 50-70 | Medium | Daily summary |
| > 70 | Low | Weekly report |

### Issue Counts

| Type | Threshold | Alert |
|------|-----------|-------|
| Critical | > 0 | Immediate |
| High | > 3 | Immediate |
| Medium | > 10 | Daily |
| Low | > 20 | Weekly |

## Files Created

```
src/GrcMvc/
├── Agents/
│   └── CodeQualityAgentConfig.cs      # Agent configuration & prompts
├── Services/
│   ├── Interfaces/
│   │   ├── ICodeQualityService.cs     # Code analysis interface
│   │   └── IAlertService.cs           # Alert service interface
│   └── Implementations/
│       ├── CodeQualityService.cs      # Claude API integration
│       └── AlertService.cs            # Multi-channel alerts
├── BackgroundJobs/
│   └── CodeQualityMonitorJob.cs       # Scheduled scans
├── Controllers/Api/
│   └── CodeQualityController.cs       # REST API endpoints
├── Extensions/
│   └── CodeQualityServiceExtensions.cs # DI registration
└── appsettings.CodeQuality.json       # Configuration
```

## Quick Start

### 1. Set Claude API Key

```bash
# In .env.grcmvc.production
CLAUDE_API_KEY=sk-ant-api03-xxxxx
```

### 2. Configure Alerts

```bash
# Email alerts (default: enabled)
ALERT_EMAIL_RECIPIENTS=your-email@example.com

# Slack (optional)
ENABLE_SLACK_ALERTS=true
SLACK_WEBHOOK_URL=https://hooks.slack.com/services/xxx/yyy/zzz
```

### 3. Rebuild & Deploy

```bash
docker-compose -f docker-compose.grcmvc.yml down
docker-compose -f docker-compose.grcmvc.yml build --no-cache
docker-compose -f docker-compose.grcmvc.yml up -d
```

### 4. Configure GitHub Webhook

1. Go to your repository Settings → Webhooks
2. Add webhook: `https://your-domain.com/api/codequality/webhook/github`
3. Select events: Push, Pull request
4. Save

### 5. Test

```bash
# Test API
curl -X POST http://localhost:5137/api/codequality/analyze \
  -H "Content-Type: application/json" \
  -d '{"agentType":"code-reviewer","code":"public class Test { }"}'

# Test alert
curl -X POST http://localhost:5137/api/codequality/alerts/test \
  -H "Authorization: Bearer <token>" \
  -d '{"severity":"info","message":"Test alert"}'
```

## Troubleshooting

### "Claude API key is not configured"

Set the API key:
```bash
CLAUDE_API_KEY=sk-ant-api03-xxxxx
```

### Alerts not sending

1. Check configuration:
   ```bash
   curl http://localhost:5137/api/codequality/alerts/config
   ```

2. Verify webhook URLs are correct

3. Check logs:
   ```bash
   docker logs grcmvc-app | grep -i alert
   ```

### Webhook not triggering

1. Check webhook secret matches
2. Verify URL is accessible externally
3. Check GitHub/GitLab webhook delivery logs

### Analysis returning empty

1. Ensure Claude API key is valid
2. Check rate limits haven't been exceeded
3. Verify code is not empty

## Security Considerations

1. **API Key Protection**: Store Claude API key securely, never commit to git
2. **Webhook Validation**: Implement signature verification for webhooks
3. **Rate Limiting**: API endpoints are rate-limited (30/min)
4. **Authentication**: Admin endpoints require authentication
5. **Alert Content**: Avoid exposing sensitive code in alerts

---

## Support

- **Documentation**: [GRCMVC_COMPLETE_ACTION_PLAN.md](GRCMVC_COMPLETE_ACTION_PLAN.md)
- **Issues**: Create ticket with `[CodeQuality]` prefix
- **Email**: Info@doganconsult.com

**End of Guide**
