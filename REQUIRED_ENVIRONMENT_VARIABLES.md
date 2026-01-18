# Required Environment Variables for Production

**Date**: January 15, 2026  
**Purpose**: Complete list of environment variables needed for production deployment

---

## 🔴 CRITICAL (Required for Application to Start)

### 1. JWT Authentication
```powershell
$env:JWT_SECRET="your-64-character-secret-here"
```
**Required**: YES  
**Generate**: `openssl rand -base64 64` or use a secure random 64+ character string  
**Purpose**: JWT token signing key

### 2. Database Connections
```powershell
$env:ConnectionStrings__DefaultConnection="Host=localhost;Database=GrcMvcDb;Username=shahin_admin;Password=Shahin@GRC2026!;Port=5432;SSL Mode=Disable"

$env:ConnectionStrings__GrcAuthDb="Host=localhost;Database=GrcAuthDb;Username=shahin_admin;Password=Shahin@GRC2026!;Port=5432;SSL Mode=Disable"
```
**Required**: YES  
**Purpose**: Database connection strings for main app and auth database

### 3. Application Environment
```powershell
$env:ASPNETCORE_ENVIRONMENT="Production"
```
**Required**: YES  
**Purpose**: Sets the application environment

---

## 🟡 IMPORTANT (Required if Features Enabled)

### 4. Claude AI (Required if ClaudeAgents:Enabled=true)
```powershell
$env:CLAUDE_API_KEY="sk-ant-api03-your-claude-api-key-here"
```
**Required**: YES if Claude agents are enabled  
**Alternative**: Set `$env:CLAUDE_ENABLED="false"` to disable Claude agents  
**Purpose**: Claude AI API key for AI features

### 5. Claude Agents Enable/Disable
```powershell
$env:CLAUDE_ENABLED="false"  # Set to "false" to disable if no API key
```
**Required**: NO (defaults to "true")  
**Purpose**: Enable/disable Claude AI agents

---

## 🟢 OPTIONAL (Recommended for Full Functionality)

### 6. Application URLs
```powershell
$env:ASPNETCORE_URLS="http://0.0.0.0:5000"
```
**Required**: NO (defaults to http://localhost:5000)  
**Purpose**: Configure which URLs the application listens on

### 7. SMTP Configuration (For Email Features)
```powershell
$env:SMTP_HOST="smtp.example.com"
$env:SMTP_PORT="587"
$env:SMTP_USERNAME="your-email@example.com"
$env:SMTP_PASSWORD="your-email-password"
$env:SMTP_FROM_EMAIL="noreply@example.com"
$env:SMTP_FROM_NAME="Shahin GRC"
```
**Required**: NO (email features will be disabled)  
**Purpose**: Email sending configuration

### 8. Microsoft Graph (For Email Operations)
```powershell
$env:AZURE_TENANT_ID="your-tenant-id"
$env:MSGRAPH_CLIENT_ID="your-client-id"
$env:MSGRAPH_CLIENT_SECRET="your-client-secret"
$env:MSGRAPH_APP_ID_URI="api://your-app-id"
```
**Required**: NO (Microsoft Graph features will be disabled)  
**Purpose**: Microsoft Graph API for email operations

### 9. Copilot Agent
```powershell
$env:COPILOT_CLIENT_ID="your-copilot-client-id"
$env:COPILOT_CLIENT_SECRET="your-copilot-secret"
$env:COPILOT_APP_ID_URI="api://your-copilot-app-id"
```
**Required**: NO (Copilot features will be disabled)  
**Purpose**: Microsoft Copilot Studio integration

---

## 📋 Minimal Production Setup (Just to Start)

For **basic testing** (Identity schema verification), you only need:

```powershell
# Required
$env:JWT_SECRET="your-64-character-secret"
$env:ConnectionStrings__DefaultConnection="Host=localhost;Database=GrcMvcDb;Username=shahin_admin;Password=Shahin@GRC2026!;Port=5432;SSL Mode=Disable"
$env:ConnectionStrings__GrcAuthDb="Host=localhost;Database=GrcAuthDb;Username=shahin_admin;Password=Shahin@GRC2026!;Port=5432;SSL Mode=Disable"
$env:ASPNETCORE_ENVIRONMENT="Production"

# Disable Claude to avoid API key requirement
$env:CLAUDE_ENABLED="false"
```

---

## 📋 Complete Production Setup

For **full production** with all features:

```powershell
# Critical
$env:JWT_SECRET="your-64-character-secret"
$env:ConnectionStrings__DefaultConnection="Host=localhost;Database=GrcMvcDb;Username=shahin_admin;Password=Shahin@GRC2026!;Port=5432;SSL Mode=Disable"
$env:ConnectionStrings__GrcAuthDb="Host=localhost;Database=GrcAuthDb;Username=shahin_admin;Password=Shahin@GRC2026!;Port=5432;SSL Mode=Disable"
$env:ASPNETCORE_ENVIRONMENT="Production"
$env:ASPNETCORE_URLS="http://0.0.0.0:5000"

# Claude AI (if using)
$env:CLAUDE_API_KEY="sk-ant-api03-your-key"
$env:CLAUDE_ENABLED="true"
$env:CLAUDE_MODEL="claude-sonnet-4-20250514"
$env:CLAUDE_MAX_TOKENS="4096"

# SMTP (if using email)
$env:SMTP_HOST="smtp.example.com"
$env:SMTP_PORT="587"
$env:SMTP_USERNAME="your-email@example.com"
$env:SMTP_PASSWORD="your-password"
$env:SMTP_FROM_EMAIL="noreply@example.com"

# Microsoft Graph (if using)
$env:AZURE_TENANT_ID="your-tenant-id"
$env:MSGRAPH_CLIENT_ID="your-client-id"
$env:MSGRAPH_CLIENT_SECRET="your-secret"
$env:MSGRAPH_APP_ID_URI="api://your-app-id"

# Copilot (if using)
$env:COPILOT_CLIENT_ID="your-copilot-client-id"
$env:COPILOT_CLIENT_SECRET="your-copilot-secret"
```

---

## 🔧 Quick Setup Script

Save this as `set-production-env.ps1`:

```powershell
# Minimal Production Environment Variables
$env:JWT_SECRET="YOUR_JWT_SECRET_HERE"
$env:ConnectionStrings__DefaultConnection="Host=localhost;Database=GrcMvcDb;Username=shahin_admin;Password=Shahin@GRC2026!;Port=5432;SSL Mode=Disable"
$env:ConnectionStrings__GrcAuthDb="Host=localhost;Database=GrcAuthDb;Username=shahin_admin;Password=Shahin@GRC2026!;Port=5432;SSL Mode=Disable"
$env:ASPNETCORE_ENVIRONMENT="Production"
$env:ASPNETCORE_URLS="http://0.0.0.0:5000"
$env:CLAUDE_ENABLED="false"

Write-Host "Production environment variables set!" -ForegroundColor Green
```

Then run:
```powershell
. .\set-production-env.ps1
```

---

## 📝 Environment Variable Format

### PowerShell (Windows)
```powershell
$env:VARIABLE_NAME="value"
```

### Bash (Linux/macOS)
```bash
export VARIABLE_NAME="value"
```

### Docker
```yaml
environment:
  - VARIABLE_NAME=value
```

### Kubernetes
```yaml
env:
  - name: VARIABLE_NAME
    value: "value"
  # Or from secret:
  - name: VARIABLE_NAME
    valueFrom:
      secretKeyRef:
        name: secret-name
        key: secret-key
```

---

## ✅ Verification

After setting variables, verify:

```powershell
# Check critical variables
Write-Host "JWT_SECRET: $($env:JWT_SECRET -ne $null)"
Write-Host "DefaultConnection: $($env:ConnectionStrings__DefaultConnection -ne $null)"
Write-Host "GrcAuthDb: $($env:ConnectionStrings__GrcAuthDb -ne $null)"
Write-Host "Environment: $env:ASPNETCORE_ENVIRONMENT"
Write-Host "Claude Enabled: $env:CLAUDE_ENABLED"
```

---

## 🎯 For Your Current Deployment

**Minimum required** (to test Identity schema):

1. `JWT_SECRET` - Your 64-character secret
2. `ConnectionStrings__DefaultConnection` - Your main database connection
3. `ConnectionStrings__GrcAuthDb` - Your auth database connection  
4. `ASPNETCORE_ENVIRONMENT` - Set to "Production"
5. `CLAUDE_ENABLED` - Set to "false" (to avoid API key requirement)

**Share these 5 values and I'll help you set them up!**
