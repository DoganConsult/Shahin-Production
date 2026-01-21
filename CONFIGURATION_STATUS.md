# Configuration Status Report

## Summary

This document tracks the status of all configuration values in the Shahin AI GRC Platform. Many values are intentionally left empty for security reasons and must be set via environment variables or production configuration files.

**Last Updated:** 2026-01-10

---

## Critical Missing Configurations

### ❌ Required for Application Startup

| Configuration Key | Location | Status | Impact | Priority |
|-------------------|----------|--------|--------|----------|
| `ConnectionStrings:DefaultConnection` | appsettings.json | ❌ EMPTY | **Application will fail to start** | 🔴 CRITICAL |
| `ConnectionStrings:GrcAuthDb` | appsettings.json | ❌ EMPTY | **Authentication will fail** | 🔴 CRITICAL |
| `ConnectionStrings:HangfireConnection` | appsettings.json | ❌ EMPTY | **Background jobs disabled** | 🔴 CRITICAL |

### ⚠️ Required for Core Features

| Configuration Key | Location | Status | Impact | Priority |
|-------------------|----------|--------|--------|----------|
| `ClaudeAgents:ApiKey` | appsettings.json | ❌ EMPTY | **AI features disabled** | 🟡 HIGH |
| `SmtpSettings:Password` | appsettings.json | ⚠️ `${SMTP_PASSWORD}` | Email sending requires env var | 🟡 HIGH |
| `SmtpSettings:ClientSecret` | appsettings.json | ❌ EMPTY | OAuth2 email requires this | 🟡 HIGH |
| `EmailOperations:MicrosoftGraph:ClientSecret` | appsettings.json | ❌ EMPTY | Microsoft Graph requires this | 🟡 HIGH |

### ⚠️ Optional but Configured

| Configuration Key | Location | Status | Impact | Priority |
|-------------------|----------|--------|--------|----------|
| `ConnectionStrings:Redis` | appsettings.json | ❌ EMPTY | Caching disabled | 🟢 LOW |
| `RabbitMq:*` | appsettings.json | ✅ ADDED | MassTransit falls back to in-memory | 🟢 LOW |
| `Stripe:*` | appsettings.json | ❌ EMPTY | Payment processing disabled | 🟡 MEDIUM |
| `OAuth2:*:ClientId` | appsettings.json | ❌ EMPTY | External login disabled | 🟢 LOW |
| `Storage:Azure:ConnectionString` | appsettings.json | ❌ EMPTY | Cloud storage disabled | 🟢 LOW |
| `Storage:AWS:AccessKeyId` | appsettings.json | ❌ EMPTY | Cloud storage disabled | 🟢 LOW |

---

## Configuration Structure

### Connection Strings

```json
"ConnectionStrings": {
  "DefaultConnection": "",          // ❌ MISSING - Required
  "GrcAuthDb": "",                 // ❌ MISSING - Required
  "Redis": "",                      // ❌ MISSING - Optional
  "HangfireConnection": ""         // ❌ MISSING - Required
}
```

### Claude AI

```json
"ClaudeAgents": {
  "Enabled": true,                 // ✅ Set
  "ApiKey": "",                    // ❌ MISSING - Required for AI features
  "Model": "claude-sonnet-4-20250514",
  "MaxTokens": 4096,
  "Temperature": 0.7
}
```

### Email Settings

```json
"SmtpSettings": {
  "Host": "smtp.office365.com",
  "Port": 587,
  "EnableSsl": true,
  "FromEmail": "info@doganconsult.com",
  "FromName": "Shahin GRC",
  "Username": "info@doganconsult.com",
  "Password": "${SMTP_PASSWORD}",  // ⚠️ Needs env var
  "UseOAuth2": false,
  "ClientSecret": ""                // ❌ MISSING (OAuth2)
}
```

### Microsoft Graph

```json
"EmailOperations": {
  "MicrosoftGraph": {
    "TenantId": "c8847e8a-33a0-4b6c-8e01-2e0e6b4aaef5",
    "ClientId": "4e2575c6-e269-48eb-b055-ad730a2150a7",
    "ClientSecret": "",            // ❌ MISSING - Required
    "Authority": "https://login.microsoftonline.com/...",
    "GraphEndpoint": "https://graph.microsoft.com"
  }
}
```

### RabbitMQ (NEW - Added)

```json
"RabbitMq": {
  "Enabled": false,
  "Host": "localhost",
  "Port": 5672,
  "VirtualHost": "/",
  "Username": "guest",
  "Password": "",                  // ❌ MISSING - Change default
  "UseSsl": false,
  "PrefetchCount": 16,
  "ConcurrencyLimit": 10,
  "RetryLimit": 3,
  "RetryIntervals": [5, 15, 60]
}
```

### Stripe

```json
"Stripe": {
  "SecretKey": "",                  // ❌ MISSING
  "PublishableKey": "",            // ❌ MISSING
  "WebhookSecret": "",             // ❌ MISSING
  "Enabled": false
}
```

### OAuth2

```json
"OAuth2": {
  "Google": {
    "ClientId": "",                // ❌ MISSING
    "ClientSecret": "",            // ❌ MISSING
    "Enabled": false
  },
  "Microsoft": {
    "ClientId": "",                // ❌ MISSING
    "ClientSecret": "",            // ❌ MISSING
    "Enabled": false
  },
  "GitHub": {
    "ClientId": "",                // ❌ MISSING
    "ClientSecret": "",            // ❌ MISSING
    "Enabled": false
  }
}
```

### Cloud Storage

```json
"Storage": {
  "Provider": "local",
  "Azure": {
    "ConnectionString": "",        // ❌ MISSING
    "ContainerName": "grc-files"
  },
  "AWS": {
    "AccessKeyId": "",             // ❌ MISSING
    "SecretAccessKey": "",        // ❌ MISSING
    "Region": "us-east-1",
    "BucketName": "grc-files"
  },
  "GoogleCloud": {
    "CredentialsPath": "",         // ❌ MISSING
    "BucketName": "grc-files"
  }
}
```

---

## How to Set Configuration Values

### Option 1: Environment Variables (Recommended)

Create a `.env` file in the project root (see `env.template`):

```bash
ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=YOUR_PASSWORD;"
ClaudeAgents__ApiKey="sk-ant-api03-xxxxx"
SMTP_PASSWORD="your_smtp_password"
```

### Option 2: appsettings.Production.json

Override values in `src/GrcMvc/appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db;Port=5432;Database=GrcMvcDb;..."
  },
  "ClaudeAgents": {
    "ApiKey": "sk-ant-api03-xxxxx"
  }
}
```

### Option 3: Azure Key Vault / AWS Secrets Manager

For production deployments, use cloud secrets management:

- **Azure:** Azure Key Vault
- **AWS:** AWS Secrets Manager
- **Google Cloud:** Secret Manager

---

## Configuration Priority

1. **Environment Variables** (`.env` file or system environment) - **Highest Priority**
2. **appsettings.Production.json** (production overrides)
3. **appsettings.Development.json** (development overrides)
4. **appsettings.json** (base configuration) - **Lowest Priority**

---

## Quick Setup Checklist

### Minimum Required for Startup

- [ ] Set `ConnectionStrings:DefaultConnection`
- [ ] Set `ConnectionStrings:GrcAuthDb`
- [ ] Set `ConnectionStrings:HangfireConnection`

### Recommended for Full Features

- [ ] Set `ClaudeAgents:ApiKey` (for AI features)
- [ ] Set `SMTP_PASSWORD` environment variable (for email)
- [ ] Set `EmailOperations:MicrosoftGraph:ClientSecret` (for email operations)
- [ ] Set `ConnectionStrings:Redis` (for caching)
- [ ] Set `RabbitMq:Password` (for message queue)
- [ ] Set `Stripe:SecretKey` (for payments)

---

## Documentation

See **CONFIGURATION_SETUP_GUIDE.md** for detailed instructions on:
- How to obtain API keys and credentials
- Configuration examples for each service
- Environment variable mapping
- Production deployment guidelines
- Troubleshooting common issues

---

## Status Legend

- ✅ **Set** - Configuration value is present and valid
- ❌ **MISSING** - Configuration value is empty and must be set
- ⚠️ **Placeholder** - Configuration uses placeholder (e.g., `${SMTP_PASSWORD}`) and requires environment variable
- 🔴 **CRITICAL** - Required for application startup
- 🟡 **HIGH** - Required for core features
- 🟢 **LOW** - Optional but recommended

---

**Note:** This document is automatically generated. For the most up-to-date configuration requirements, see `CONFIGURATION_SETUP_GUIDE.md`.
