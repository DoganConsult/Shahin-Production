# Configuration Setup Guide

## Overview

This guide documents all required configuration values for the Shahin AI GRC Platform. Many configuration values are intentionally left empty in `appsettings.json` for security reasons and must be set via environment variables or production configuration files.

## Configuration Priority

Configuration values are loaded in the following order (highest to lowest priority):

1. **Environment Variables** (`.env` file or system environment)
2. **appsettings.Production.json** (production overrides)
3. **appsettings.Development.json** (development overrides)
4. **appsettings.json** (base configuration)

## Critical Missing Configurations

### ❌ Required for Application Startup

| Configuration Key | Location | Status | Impact | How to Set |
|-------------------|----------|--------|--------|------------|
| `ConnectionStrings:DefaultConnection` | appsettings.json | ❌ EMPTY | **Application will fail to start** | PostgreSQL connection string |
| `ConnectionStrings:GrcAuthDb` | appsettings.json | ❌ EMPTY | **Authentication will fail** | Auth database connection string |
| `ConnectionStrings:HangfireConnection` | appsettings.json | ❌ EMPTY | **Background jobs disabled** | Hangfire storage connection |

### ⚠️ Required for Core Features

| Configuration Key | Location | Status | Impact | How to Set |
|-------------------|----------|--------|--------|------------|
| `ClaudeAgents:ApiKey` | appsettings.json | ❌ EMPTY | **AI features disabled** | Anthropic API key |
| `SmtpSettings:Password` | appsettings.json | ⚠️ `${SMTP_PASSWORD}` | Email sending requires env var | Environment variable |
| `SmtpSettings:ClientSecret` | appsettings.json | ❌ EMPTY | OAuth2 email requires this | Azure AD client secret |
| `EmailOperations:MicrosoftGraph:ClientSecret` | appsettings.json | ❌ EMPTY | Microsoft Graph requires this | Azure AD client secret |

### ⚠️ Optional but Configured

| Configuration Key | Location | Status | Impact | How to Set |
|-------------------|----------|--------|--------|------------|
| `ConnectionStrings:Redis` | appsettings.json | ❌ EMPTY | Caching disabled | Redis connection string |
| `RabbitMq:*` | appsettings.json | ⚠️ NOT FOUND | MassTransit falls back to in-memory | RabbitMQ settings |
| `Stripe:*` | appsettings.json | ❌ EMPTY | Payment processing disabled | Stripe API keys |
| `OAuth2:*:ClientId` | appsettings.json | ❌ EMPTY | External login disabled | OAuth2 provider credentials |
| `Storage:Azure:ConnectionString` | appsettings.json | ❌ EMPTY | Cloud storage disabled | Azure storage connection |
| `Storage:AWS:AccessKeyId` | appsettings.json | ❌ EMPTY | Cloud storage disabled | AWS credentials |

---

## 1. Database Configuration

### PostgreSQL Connection Strings

**Required for:** Application startup, authentication, background jobs

#### DefaultConnection (Main Database)
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=YOUR_PASSWORD;Pooling=true;SSL Mode=Prefer;Trust Server Certificate=true;"
}
```

#### GrcAuthDb (Authentication Database)
```json
"ConnectionStrings": {
  "GrcAuthDb": "Host=localhost;Port=5432;Database=GrcAuthDb;Username=postgres;Password=YOUR_PASSWORD;Pooling=true;SSL Mode=Prefer;Trust Server Certificate=true;"
}
```

#### HangfireConnection (Background Jobs)
```json
"ConnectionStrings": {
  "HangfireConnection": "Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=YOUR_PASSWORD;Pooling=true;SSL Mode=Prefer;Trust Server Certificate=true;"
}
```

**Environment Variable Format:**
```bash
ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=YOUR_PASSWORD;Pooling=true;SSL Mode=Prefer;Trust Server Certificate=true;"
ConnectionStrings__GrcAuthDb="Host=localhost;Port=5432;Database=GrcAuthDb;Username=postgres;Password=YOUR_PASSWORD;Pooling=true;SSL Mode=Prefer;Trust Server Certificate=true;"
ConnectionStrings__HangfireConnection="Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=YOUR_PASSWORD;Pooling=true;SSL Mode=Prefer;Trust Server Certificate=true;"
```

**Docker Compose Format:**
```yaml
environment:
  - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=GrcMvcDb;Username=postgres;Password=${DB_PASSWORD}
  - ConnectionStrings__GrcAuthDb=Host=db;Port=5432;Database=GrcAuthDb;Username=postgres;Password=${DB_PASSWORD}
  - ConnectionStrings__HangfireConnection=Host=db;Port=5432;Database=GrcMvcDb;Username=postgres;Password=${DB_PASSWORD}
```

---

## 2. AI Configuration (Claude)

### ClaudeAgents:ApiKey

**Required for:** All AI agent features (12 specialized agents)

**Location:** `appsettings.json` → `ClaudeAgents:ApiKey`

**How to Obtain:**
1. Go to [Anthropic Console](https://console.anthropic.com/)
2. Sign up / Log in
3. Navigate to **API Keys**
4. Create a new API key
5. Copy the key (starts with `sk-ant-api03-`)

**Configuration:**
```json
"ClaudeAgents": {
  "Enabled": true,
  "ApiKey": "sk-ant-api03-xxxxx",
  "Model": "claude-sonnet-4-20250514",
  "MaxTokens": 4096,
  "Temperature": 0.7
}
```

**Environment Variable:**
```bash
ClaudeAgents__ApiKey="sk-ant-api03-xxxxx"
```

**⚠️ Without this key, all AI agent features will be disabled!**

---

## 3. Email Configuration

### SMTP Settings

**Required for:** Email notifications, password resets, confirmations

#### Basic SMTP (Office 365)
```json
"SmtpSettings": {
  "Host": "smtp.office365.com",
  "Port": 587,
  "EnableSsl": true,
  "FromEmail": "info@doganconsult.com",
  "FromName": "Shahin GRC",
  "Username": "info@doganconsult.com",
  "Password": "${SMTP_PASSWORD}",  // Set via environment variable
  "UseOAuth2": false
}
```

**Environment Variable:**
```bash
SMTP_PASSWORD="your_smtp_password"
```

#### OAuth2 SMTP (Microsoft 365)
```json
"SmtpSettings": {
  "UseOAuth2": true,
  "TenantId": "your-azure-tenant-id",
  "ClientId": "your-azure-client-id",
  "ClientSecret": "your-azure-client-secret"  // ⚠️ REQUIRED
}
```

**Environment Variable:**
```bash
SmtpSettings__ClientSecret="your-azure-client-secret"
```

### Microsoft Graph Configuration

**Required for:** Email operations, mailbox management

```json
"EmailOperations": {
  "MicrosoftGraph": {
    "TenantId": "c8847e8a-33a0-4b6c-8e01-2e0e6b4aaef5",
    "ClientId": "4e2575c6-e269-48eb-b055-ad730a2150a7",
    "ClientSecret": "",  // ⚠️ REQUIRED
    "Authority": "https://login.microsoftonline.com/c8847e8a-33a0-4b6c-8e01-2e0e6b4aaef5",
    "GraphEndpoint": "https://graph.microsoft.com"
  }
}
```

**Environment Variable:**
```bash
EmailOperations__MicrosoftGraph__ClientSecret="your-graph-client-secret"
```

---

## 4. Message Queue Configuration

### RabbitMQ (MassTransit)

**Required for:** Message queue processing, event-driven architecture

```json
"RabbitMq": {
  "Enabled": true,
  "Host": "localhost",
  "Port": 5672,
  "VirtualHost": "/",
  "Username": "guest",
  "Password": "guest",  // ⚠️ Change default password
  "UseSsl": false,
  "PrefetchCount": 16,
  "ConcurrencyLimit": 10,
  "RetryLimit": 3,
  "RetryIntervals": [5, 15, 60]
}
```

**Environment Variables:**
```bash
RabbitMq__Enabled=true
RabbitMq__Host=localhost
RabbitMq__Port=5672
RabbitMq__Username=guest
RabbitMq__Password=your_rabbitmq_password
```

**Docker Compose:**
```yaml
rabbitmq:
  image: rabbitmq:3-management
  environment:
    RABBITMQ_DEFAULT_USER: admin
    RABBITMQ_DEFAULT_PASS: ${RABBITMQ_PASSWORD}
```

**Note:** If RabbitMQ is not configured, MassTransit will fall back to in-memory message queue (messages lost on restart).

---

## 5. Redis Configuration

**Optional but Recommended:** Caching, session storage

```json
"Redis": {
  "Enabled": true,
  "ConnectionString": "localhost:6379",
  "InstanceName": "GrcCache_"
}
```

**Connection String Format:**
```json
"ConnectionStrings": {
  "Redis": "localhost:6379"  // Simple format
  // OR
  "Redis": "localhost:6379,password=YOUR_PASSWORD,ssl=true"  // With auth
}
```

**Environment Variable:**
```bash
ConnectionStrings__Redis="localhost:6379"
```

---

## 6. Payment Gateway (Stripe)

**Required for:** Subscription payments, payment processing

```json
"Stripe": {
  "SecretKey": "sk_test_xxxxx",  // ⚠️ REQUIRED
  "PublishableKey": "pk_test_xxxxx",
  "WebhookSecret": "whsec_xxxxx",  // For webhook verification
  "Enabled": true
}
```

**How to Obtain:**
1. Go to [Stripe Dashboard](https://dashboard.stripe.com/)
2. Navigate to **Developers** → **API keys**
3. Copy **Secret key** and **Publishable key**
4. For webhooks: **Developers** → **Webhooks** → Create endpoint → Copy **Signing secret**

**Environment Variables:**
```bash
Stripe__SecretKey="sk_test_xxxxx"
Stripe__PublishableKey="pk_test_xxxxx"
Stripe__WebhookSecret="whsec_xxxxx"
Stripe__Enabled=true
```

---

## 7. OAuth2 / External Authentication

### Google OAuth2

```json
"OAuth2": {
  "Google": {
    "ClientId": "your-google-client-id.apps.googleusercontent.com",
    "ClientSecret": "your-google-client-secret",
    "Enabled": true
  }
}
```

**How to Obtain:**
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create OAuth 2.0 credentials
3. Add authorized redirect URI: `https://portal.shahin-ai.com/signin-google`

### Microsoft OAuth2 (Azure AD)

```json
"OAuth2": {
  "Microsoft": {
    "ClientId": "your-azure-client-id",
    "ClientSecret": "your-azure-client-secret",
    "Enabled": true
  }
}
```

**How to Obtain:**
1. Go to [Azure Portal](https://portal.azure.com/)
2. Navigate to **Azure Active Directory** → **App registrations**
3. Create new registration
4. Add redirect URI: `https://portal.shahin-ai.com/signin-microsoft`

### GitHub OAuth2

```json
"OAuth2": {
  "GitHub": {
    "ClientId": "your-github-client-id",
    "ClientSecret": "your-github-client-secret",
    "Enabled": true
  }
}
```

**How to Obtain:**
1. Go to [GitHub Developer Settings](https://github.com/settings/developers)
2. Create new OAuth App
3. Add authorization callback URL: `https://portal.shahin-ai.com/signin-github`

---

## 8. Cloud Storage Configuration

### Azure Blob Storage

```json
"Storage": {
  "Provider": "azure",
  "Azure": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...",
    "ContainerName": "grc-files"
  }
}
```

**How to Obtain:**
1. Go to [Azure Portal](https://portal.azure.com/)
2. Create Storage Account
3. Navigate to **Access keys**
4. Copy **Connection string**

### AWS S3

```json
"Storage": {
  "Provider": "aws",
  "AWS": {
    "AccessKeyId": "AKIA...",
    "SecretAccessKey": "...",
    "Region": "us-east-1",
    "BucketName": "grc-files"
  }
}
```

**How to Obtain:**
1. Go to [AWS IAM Console](https://console.aws.amazon.com/iam/)
2. Create IAM user with S3 permissions
3. Generate access keys

### Google Cloud Storage

```json
"Storage": {
  "Provider": "googlecloud",
  "GoogleCloud": {
    "CredentialsPath": "/path/to/service-account-key.json",
    "BucketName": "grc-files"
  }
}
```

---

## 9. SAML 2.0 Configuration

**Required for:** Enterprise SSO

```json
"Saml": {
  "Enabled": true,
  "Issuer": "https://portal.shahin-ai.com",
  "IdpSsoUrl": "https://idp.example.com/sso",
  "IdpSloUrl": "https://idp.example.com/slo",
  "SpCertificatePath": "/path/to/sp-certificate.pfx",
  "SpCertificatePassword": "certificate-password",
  "IdpCertificatePath": "/path/to/idp-certificate.cer",
  "AssertionConsumerServiceUrl": "https://portal.shahin-ai.com/saml/acs"
}
```

---

## 10. LDAP / Active Directory Configuration

**Required for:** Enterprise directory integration

```json
"Ldap": {
  "Enabled": true,
  "Server": "ldap.example.com",
  "Port": 389,
  "UseSsl": false,
  "BaseDn": "DC=example,DC=com",
  "UserDnFormat": "{username}@{domain}",
  "Domain": "example.com",
  "UserAttribute": "sAMAccountName",
  "ServiceAccountDn": "CN=ServiceAccount,OU=ServiceAccounts,DC=example,DC=com",
  "ServiceAccountPassword": "service-account-password"
}
```

---

## 11. Twilio (SMS 2FA)

**Optional:** SMS-based two-factor authentication

```json
"Twilio": {
  "AccountSid": "ACxxxxx",
  "AuthToken": "your-auth-token",
  "FromPhoneNumber": "+1234567890"
}
```

**How to Obtain:**
1. Go to [Twilio Console](https://console.twilio.com/)
2. Copy **Account SID** and **Auth Token**
3. Get phone number from **Phone Numbers**

---

## 12. Application Insights

**Optional:** Application monitoring and telemetry

```json
"ApplicationInsights": {
  "ConnectionString": "InstrumentationKey=xxxxx;IngestionEndpoint=https://...",
  "EnableAdaptiveSampling": true
}
```

**How to Obtain:**
1. Go to [Azure Portal](https://portal.azure.com/)
2. Create Application Insights resource
3. Copy **Connection String**

---

## Environment Variable Mapping

### .env File Format

Create a `.env` file in the project root:

```bash
# ════════════════════════════════════════════════════════════
# Database (PostgreSQL) - REQUIRED
# ════════════════════════════════════════════════════════════
ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=YOUR_PASSWORD;Pooling=true;SSL Mode=Prefer;Trust Server Certificate=true;"
ConnectionStrings__GrcAuthDb="Host=localhost;Port=5432;Database=GrcAuthDb;Username=postgres;Password=YOUR_PASSWORD;Pooling=true;SSL Mode=Prefer;Trust Server Certificate=true;"
ConnectionStrings__HangfireConnection="Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=YOUR_PASSWORD;Pooling=true;SSL Mode=Prefer;Trust Server Certificate=true;"
ConnectionStrings__Redis="localhost:6379"

# ════════════════════════════════════════════════════════════
# Claude AI - REQUIRED for AI features
# ════════════════════════════════════════════════════════════
ClaudeAgents__ApiKey="sk-ant-api03-xxxxx"
ClaudeAgents__Enabled=true

# ════════════════════════════════════════════════════════════
# Email (SMTP) - REQUIRED
# ════════════════════════════════════════════════════════════
SMTP_PASSWORD="your_smtp_password"
SmtpSettings__ClientSecret="your-azure-client-secret"

# ════════════════════════════════════════════════════════════
# Microsoft Graph - REQUIRED for email operations
# ════════════════════════════════════════════════════════════
EmailOperations__MicrosoftGraph__ClientSecret="your-graph-client-secret"

# ════════════════════════════════════════════════════════════
# RabbitMQ - Optional (falls back to in-memory if not set)
# ════════════════════════════════════════════════════════════
RabbitMq__Enabled=true
RabbitMq__Host=localhost
RabbitMq__Port=5672
RabbitMq__Username=guest
RabbitMq__Password=guest

# ════════════════════════════════════════════════════════════
# Stripe - Required for payments
# ════════════════════════════════════════════════════════════
Stripe__SecretKey="sk_test_xxxxx"
Stripe__PublishableKey="pk_test_xxxxx"
Stripe__WebhookSecret="whsec_xxxxx"
Stripe__Enabled=true

# ════════════════════════════════════════════════════════════
# OAuth2 - Optional (external login)
# ════════════════════════════════════════════════════════════
OAuth2__Google__ClientId="your-google-client-id"
OAuth2__Google__ClientSecret="your-google-client-secret"
OAuth2__Google__Enabled=true

OAuth2__Microsoft__ClientId="your-azure-client-id"
OAuth2__Microsoft__ClientSecret="your-azure-client-secret"
OAuth2__Microsoft__Enabled=true

OAuth2__GitHub__ClientId="your-github-client-id"
OAuth2__GitHub__ClientSecret="your-github-client-secret"
OAuth2__GitHub__Enabled=true

# ════════════════════════════════════════════════════════════
# Cloud Storage - Optional
# ════════════════════════════════════════════════════════════
Storage__Provider="local"  # or "azure", "aws", "googlecloud"
Storage__Azure__ConnectionString="DefaultEndpointsProtocol=https;AccountName=...;AccountKey=..."
Storage__AWS__AccessKeyId="AKIA..."
Storage__AWS__SecretAccessKey="..."
Storage__GoogleCloud__CredentialsPath="/path/to/service-account-key.json"

# ════════════════════════════════════════════════════════════
# Twilio (SMS 2FA) - Optional
# ════════════════════════════════════════════════════════════
Twilio__AccountSid="ACxxxxx"
Twilio__AuthToken="your-auth-token"
Twilio__FromPhoneNumber="+1234567890"
```

---

## Configuration Validation

### Startup Validation

The application validates critical configurations on startup:

- ✅ Database connections (DefaultConnection, GrcAuthDb)
- ✅ Claude API key (if AI features enabled)
- ✅ SMTP settings (if email enabled)
- ⚠️ RabbitMQ connection (warns if not configured, falls back to in-memory)

### Health Checks

Health check endpoints verify configuration:

```bash
# Application health
curl http://localhost:5010/health

# Database connectivity
curl http://localhost:5010/health/db

# All subsystems
curl http://localhost:5010/health/detailed
```

---

## Security Best Practices

### ❌ DO NOT:

- ❌ Commit `.env` file to version control
- ❌ Commit `appsettings.Production.json` with real secrets
- ❌ Hardcode secrets in `appsettings.json`
- ❌ Share API keys in chat/email

### ✅ DO:

- ✅ Use environment variables for all secrets
- ✅ Use Azure Key Vault / AWS Secrets Manager in production
- ✅ Rotate API keys regularly
- ✅ Use separate credentials for dev/staging/production
- ✅ Enable SSL/TLS for all external connections
- ✅ Use strong passwords (min 32 characters for JWT secrets)

---

## Production Deployment

### Azure App Service

Use **Application Settings** in Azure Portal:

```
ConnectionStrings__DefaultConnection = "Host=..."
ClaudeAgents__ApiKey = "sk-ant-api03-..."
```

### AWS Elastic Beanstalk

Use **Environment Properties**:

```yaml
aws:elasticbeanstalk:application:environment:
  ConnectionStrings__DefaultConnection: "Host=..."
  ClaudeAgents__ApiKey: "sk-ant-api03-..."
```

### Docker Compose

Use environment variables in `docker-compose.yml`:

```yaml
services:
  grcmvc:
    environment:
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
      - ClaudeAgents__ApiKey=${CLAUDE_API_KEY}
```

---

## Troubleshooting

### "Cannot connect to database"

1. Check PostgreSQL is running: `docker-compose ps db`
2. Verify connection string format
3. Check firewall/network access
4. Verify credentials

### "Claude API key is not configured"

1. Add `ClaudeAgents__ApiKey` to `.env` file
2. Restart application
3. Verify key is valid at [Anthropic Console](https://console.anthropic.com/)

### "Email sending failed"

1. Check SMTP credentials
2. Verify `SMTP_PASSWORD` environment variable is set
3. Check firewall allows SMTP port 587
4. For OAuth2, verify `SmtpSettings__ClientSecret` is set

### "RabbitMQ connection failed"

1. Check RabbitMQ is running: `docker-compose ps rabbitmq`
2. Verify credentials
3. Check port 5672 is accessible
4. Application will fall back to in-memory queue (messages lost on restart)

---

## Quick Reference

### Minimum Required for Startup

```bash
ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=YOUR_PASSWORD;"
ConnectionStrings__GrcAuthDb="Host=localhost;Port=5432;Database=GrcAuthDb;Username=postgres;Password=YOUR_PASSWORD;"
ConnectionStrings__HangfireConnection="Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=YOUR_PASSWORD;"
```

### Recommended for Full Features

```bash
# Database (required)
ConnectionStrings__DefaultConnection="..."
ConnectionStrings__GrcAuthDb="..."
ConnectionStrings__HangfireConnection="..."

# AI Features (required)
ClaudeAgents__ApiKey="sk-ant-api03-..."

# Email (required)
SMTP_PASSWORD="..."
EmailOperations__MicrosoftGraph__ClientSecret="..."

# Optional but Recommended
ConnectionStrings__Redis="localhost:6379"
RabbitMq__Enabled=true
RabbitMq__Host=localhost
RabbitMq__Password=...
Stripe__SecretKey="sk_test_..."
```

---

**Last Updated:** 2026-01-10  
**Version:** 1.0.0
