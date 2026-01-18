# Development & Production Setup Report

**Date:** 2026-01-12  
**Purpose:** Comprehensive review of development and production environment configurations

---

## Executive Summary

### Configuration Files Found

| File | Status | Purpose |
|------|--------|---------|
| `appsettings.json` | ✅ Base | Default configuration (empty connection strings) |
| `appsettings.Development.json` | ✅ Active | Development environment settings |
| `appsettings.Production.json` | ✅ Active | Production environment settings (uses env vars) |
| `appsettings.Local.json` | ✅ Active | Local development settings |
| `appsettings.clean.json` | ✅ Template | Clean template file |
| `appsettings.CodeQuality.json` | ✅ Active | Code quality testing settings |

---

## Development Setup

### Configuration: `appsettings.Development.json`

**Key Settings:**

#### Connection Strings
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "",  // ⚠️ Empty - must be set via environment
    "GrcAuthDb": "",
    "HangfireConnection": "",
    "Redis": ""
  }
}
```

#### Security Settings
- ✅ `AllowPublicRegistration: true` - Public registration enabled
- ⚠️ Demo login enabled (`DisableDemoLogin: false`)

#### Logging
- **Default:** Information
- **Microsoft.AspNetCore:** Warning
- **GrcMvc.Services.Implementations:** Debug (detailed logging)
- **MetricsService:** Information

#### CORS & Allowed Origins
- ✅ Localhost ports: `http://localhost:3000`, `http://localhost:5137`
- ✅ HTTPS localhost: `https://localhost:5001`, `https://localhost:7001`
- ✅ Production domains: `portal.shahin-ai.com`, `shahin-ai.com`

#### Feature Flags
- ✅ `UseSecurePasswordGeneration: true`
- ✅ `UseSessionBasedClaims: true`
- ✅ `UseEnhancedAuditLogging: true`
- ✅ `UseDeterministicTenantResolution: true`
- ⚠️ `DisableDemoLogin: false` - Demo login allowed
- ✅ `CanaryPercentage: 0` - No canary deployments
- ✅ `VerifyConsistency: true`
- ✅ `LogFeatureFlagDecisions: true`

#### Background Jobs
- ⚠️ `Hangfire.Enabled: false` - Background jobs disabled in development

#### Demo Account
- Email: `support@shahin-ai.com`
- Password: `CHANGE_ME_IN_USER_SECRETS` (must be changed)

---

## Production Setup

### Configuration: `appsettings.Production.json`

**Key Settings:**

#### Connection Strings (Environment Variables)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "${ConnectionStrings__DefaultConnection}",
    "GrcAuthDb": "${ConnectionStrings__GrcAuthDb}",
    "Redis": "${ConnectionStrings__Redis}",
    "HangfireConnection": "${ConnectionStrings__HangfireConnection}"
  }
}
```
**✅ Uses environment variable placeholders** - Secure, no hardcoded secrets

#### Kestrel Server Configuration
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:8080"
      }
    }
  }
}
```
- ✅ Listens on all interfaces (`0.0.0.0`)
- ✅ Port 8080 (standard for containers)
- ✅ HTTPS handled by reverse proxy (nginx/traefik)

#### JWT Settings (Environment Variables)
```json
{
  "JwtSettings": {
    "Secret": "${JWT_SECRET}",
    "Issuer": "ShahinAI",
    "Audience": "ShahinAIUsers",
    "ExpiryInMinutes": 60
  }
}
```
**✅ Uses environment variable for secret**

#### Logging
- **Default:** Warning (less verbose)
- **Microsoft.AspNetCore:** Warning
- **EntityFrameworkCore:** Warning
- ✅ Production-appropriate logging levels

#### Allowed Hosts
```
shahin-ai.com;www.shahin-ai.com;portal.shahin-ai.com;app.shahin-ai.com;api.shahin-ai.com
```
✅ Production domains only

#### CORS Origins
- ✅ Production domains only:
  - `https://shahin-ai.com`
  - `https://www.shahin-ai.com`
  - `https://portal.shahin-ai.com`
- ❌ No localhost origins (secure)

#### Feature Flags
- ✅ `DisableDemoLogin: true` - Demo login disabled
- ✅ `CanaryPercentage: 10` - 10% canary deployment
- ✅ `RequirePaymentVerificationForTrial: true` - Payment required
- ✅ `ShowTrialEditionBanner: false` - No trial banner
- ✅ `AllowDemoLoginInProduction: false` - No demo login

#### File Storage
```json
{
  "FileStorage": {
    "Provider": "LocalFileSystem",
    "BasePath": "/var/www/shahin-ai/storage",
    "MaxFileSizeMB": 50
  }
}
```
✅ Production file storage path

#### SMTP Settings (Environment Variables)
```json
{
  "SmtpSettings": {
    "Host": "smtp.office365.com",
    "Port": 587,
    "EnableSsl": true,
    "FromEmail": "${SMTP_FROM_EMAIL}",
    "Username": "${SMTP_USERNAME}",
    "Password": "${SMTP_PASSWORD}",
    "UseOAuth2": true,
    "TenantId": "${AZURE_TENANT_ID}",
    "ClientId": "${SMTP_CLIENT_ID}",
    "ClientSecret": "${SMTP_CLIENT_SECRET}"
  }
}
```
✅ Uses OAuth2 with Azure AD
✅ All secrets from environment variables

#### Microsoft Graph Integration
```json
{
  "MicrosoftGraph": {
    "TenantId": "${AZURE_TENANT_ID}",
    "ClientId": "${MSGRAPH_CLIENT_ID}",
    "ClientSecret": "${MSGRAPH_CLIENT_SECRET}",
    "ApplicationIdUri": "${MSGRAPH_APP_ID_URI}"
  }
}
```
✅ Enterprise email integration

#### Copilot Agent
```json
{
  "CopilotAgent": {
    "Enabled": true,
    "TenantId": "${AZURE_TENANT_ID}",
    "ClientId": "${COPILOT_CLIENT_ID}",
    "ClientSecret": "${COPILOT_CLIENT_SECRET}",
    "ApplicationIdUri": "${COPILOT_APP_ID_URI}"
  }
}
```
✅ Microsoft Copilot integration enabled

#### Claude AI Agents
```json
{
  "ClaudeAgents": {
    "Enabled": true,
    "ApiKey": "${CLAUDE_API_KEY}",
    "Model": "claude-sonnet-4-20250514"
  }
}
```
✅ AI agent integration

#### Kafka & Camunda
- ⚠️ `Kafka.Enabled: false` - Disabled until configured
- ⚠️ `Camunda.Enabled: false` - Disabled until configured

---

## Environment Variable Requirements

### Production Environment Variables

#### Database
- `ConnectionStrings__DefaultConnection` - Main database
- `ConnectionStrings__GrcAuthDb` - Auth database (optional)
- `ConnectionStrings__Redis` - Redis cache (optional)
- `ConnectionStrings__HangfireConnection` - Background jobs (optional)

#### Security
- `JWT_SECRET` - JWT signing key (minimum 32 characters)

#### Email (SMTP)
- `SMTP_FROM_EMAIL` - Sender email address
- `SMTP_USERNAME` - SMTP username
- `SMTP_PASSWORD` - SMTP password
- `SMTP_CLIENT_ID` - OAuth2 client ID
- `SMTP_CLIENT_SECRET` - OAuth2 client secret

#### Azure AD
- `AZURE_TENANT_ID` - Azure tenant ID
- `MSGRAPH_CLIENT_ID` - Microsoft Graph client ID
- `MSGRAPH_CLIENT_SECRET` - Microsoft Graph secret
- `MSGRAPH_APP_ID_URI` - Microsoft Graph app ID URI
- `COPILOT_CLIENT_ID` - Copilot client ID
- `COPILOT_CLIENT_SECRET` - Copilot secret
- `COPILOT_APP_ID_URI` - Copilot app ID URI

#### AI Services
- `CLAUDE_API_KEY` - Claude AI API key

#### Optional Services
- `KAFKA_BOOTSTRAP_SERVERS` - Kafka servers (if enabled)
- `CAMUNDA_BASE_URL` - Camunda BPM URL (if enabled)
- `CAMUNDA_USERNAME` - Camunda username
- `CAMUNDA_PASSWORD` - Camunda password

---

## Development vs Production Comparison

| Feature | Development | Production |
|---------|-------------|------------|
| **Connection Strings** | Empty (set via env) | Environment variables |
| **Logging Level** | Information/Debug | Warning |
| **Demo Login** | ✅ Enabled | ❌ Disabled |
| **Public Registration** | ✅ Enabled | ⚠️ Not specified |
| **CORS Origins** | Localhost + Production | Production only |
| **Hangfire** | ❌ Disabled | ⚠️ Not specified |
| **Canary Deployment** | 0% | 10% |
| **Payment Verification** | ❌ Not required | ✅ Required |
| **HTTPS** | Self-signed certs | Reverse proxy |
| **File Storage** | Not specified | `/var/www/shahin-ai/storage` |
| **Secrets** | Can be in config | Environment variables only |

---

## Launch Settings

### Development Profiles

**Location:** `Properties/launchSettings.json`

Expected profiles:
- **Development** - Local development with hot reload
- **Production** - Production-like local testing
- **Docker** - Container-based development

---

## Deployment Architecture

### Production Deployment

**Server Configuration:**
- ✅ Kestrel on port 8080 (internal)
- ✅ Reverse proxy (nginx/traefik) for HTTPS
- ✅ Environment variables for all secrets
- ✅ File storage at `/var/www/shahin-ai/storage`

**Security:**
- ✅ No hardcoded secrets
- ✅ OAuth2 for email
- ✅ JWT authentication
- ✅ CORS restricted to production domains
- ✅ Demo login disabled

**Services:**
- ✅ Microsoft Graph integration
- ✅ Copilot Agent enabled
- ✅ Claude AI enabled
- ⚠️ Kafka disabled
- ⚠️ Camunda disabled

---

## Setup Checklist

### Development Setup

- [ ] Set `ConnectionStrings__DefaultConnection` environment variable
- [ ] Configure `appsettings.Development.json` if needed
- [ ] Update demo account password in user secrets
- [ ] Enable Hangfire if needed for background jobs
- [ ] Configure local PostgreSQL database
- [ ] Set up Redis (optional)

### Production Setup

- [ ] Set all required environment variables
- [ ] Configure reverse proxy (nginx/traefik)
- [ ] Set up SSL certificates
- [ ] Configure file storage path
- [ ] Set up Azure AD applications
- [ ] Configure SMTP/OAuth2
- [ ] Set up Claude API key
- [ ] Configure Microsoft Graph
- [ ] Set up Copilot Agent
- [ ] Test all integrations
- [ ] Configure monitoring/logging
- [ ] Set up backup strategy

---

## Recommendations

### Development
1. ✅ Keep demo login enabled for testing
2. ✅ Use detailed logging (Debug level)
3. ✅ Allow localhost CORS
4. ⚠️ Consider enabling Hangfire for background job testing

### Production
1. ✅ All secrets in environment variables (✅ Already configured)
2. ✅ Restrict CORS to production domains (✅ Already configured)
3. ✅ Disable demo login (✅ Already configured)
4. ✅ Use production logging levels (✅ Already configured)
5. ⚠️ Consider enabling Kafka for event-driven architecture
6. ⚠️ Consider enabling Camunda for workflow orchestration

---

## Missing Configurations

### Development
- ⚠️ No Docker Compose file found
- ⚠️ No local development database setup script
- ⚠️ No development environment setup guide

### Production
- ⚠️ No Dockerfile found
- ⚠️ No Kubernetes manifests
- ⚠️ No deployment scripts
- ⚠️ No CI/CD pipeline configuration

---

## Summary

### ✅ What's Good
- Clear separation between development and production configs
- Production uses environment variables for all secrets
- Appropriate security settings for production
- Good feature flag management

### ⚠️ What Needs Attention
- Connection strings not configured in any environment
- No Docker/containerization setup
- No deployment automation
- Some services disabled (Kafka, Camunda)

### 📝 Next Steps
1. Configure connection strings for development
2. Create Docker setup for local development
3. Create production deployment documentation
4. Set up CI/CD pipeline
5. Enable and configure optional services (Kafka, Camunda)

---

**Report Generated:** 2026-01-12  
**Configuration Files Reviewed:** 6  
**Environment Variables Required:** 15+
