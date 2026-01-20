# Runtime Configuration Checklist
**Generated:** 2026-01-12  
**Purpose:** Identify all missing runtime configurations required for production

---

## 🔴 CRITICAL: Required for Application Startup

### 1. Database Connection String
**Status:** ⚠️ **MISSING** - Application will fail to start without this

**Required Environment Variables:**
```bash
# Option 1: Standard format
ConnectionStrings__DefaultConnection="Host=localhost;Database=grc_main;Username=grc_user;Password=YOUR_PASSWORD;Port=5432"

# Option 2: Railway format (auto-converted)
DATABASE_URL="postgresql://user:password@host:port/dbname"

# Option 3: Individual components
DB_HOST=localhost
DB_PORT=5432
DB_NAME=grc_main
DB_USER=grc_user
DB_PASSWORD=YOUR_PASSWORD
```

**Validation:** `Program.cs:201-209` throws `InvalidOperationException` if missing in Production

**Fix Required:** Set one of the above before starting application

---

### 2. JWT Secret
**Status:** ⚠️ **MISSING** - AuthenticationService will fail without this

**Required Environment Variable:**
```bash
JWT_SECRET="YOUR_64_CHARACTER_SECRET_HERE"
```

**Generate Secret:**
```bash
# PowerShell
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object {[char]$_})

# Linux/Mac
openssl rand -base64 64
```

**Validation:** `AuthenticationService.cs:550` throws `InvalidOperationException` if missing

**Fix Required:** Generate and set JWT_SECRET before starting application

---

### 3. ASP.NET Core Environment
**Status:** ⚠️ **MISSING** - Should be set for production

**Required Environment Variable:**
```bash
ASPNETCORE_ENVIRONMENT="Production"
```

**Fix Required:** Set to "Production" for production deployment

---

## 🟡 IMPORTANT: Required for Full Functionality

### 4. Email Configuration (SMTP)
**Status:** ⚠️ **MISSING** - Notifications won't send emails without this

**Required Environment Variables:**
```bash
SMTP_FROM_EMAIL="noreply@shahin-ai.com"
SMTP_USERNAME="your-email@shahin-ai.com"
SMTP_PASSWORD="your-password"
SMTP_HOST="smtp.office365.com"
SMTP_PORT="587"

# Azure OAuth2 (if using)
AZURE_TENANT_ID="your-tenant-id"
SMTP_CLIENT_ID="your-client-id"
SMTP_CLIENT_SECRET="your-client-secret"
```

**Impact:** Notification Service will fail to send emails

**Fix Required:** Configure SMTP settings for email notifications

---

### 5. Auth Database Connection
**Status:** ⚠️ **MISSING** - Authentication won't work without this

**Required Environment Variable:**
```bash
ConnectionStrings__GrcAuthDb="Host=localhost;Database=grc_auth;Username=grc_user;Password=YOUR_PASSWORD;Port=5432"
```

**Note:** Can use same database as main, but separate database recommended for security

**Fix Required:** Set GrcAuthDb connection string

---

### 6. Redis Connection (Optional but Recommended)
**Status:** ⚠️ **MISSING** - Caching won't work without this

**Required Environment Variable:**
```bash
ConnectionStrings__Redis="localhost:6379"
# Or with password
ConnectionStrings__Redis="localhost:6379,password=YOUR_REDIS_PASSWORD"
```

**Impact:** Caching disabled, performance may be slower

**Fix Required:** Set Redis connection if using caching

---

### 7. Hangfire Connection (Optional but Recommended)
**Status:** ⚠️ **MISSING** - Background jobs won't work without this

**Required Environment Variable:**
```bash
ConnectionStrings__HangfireConnection="Host=localhost;Database=grc_main;Username=grc_user;Password=YOUR_PASSWORD;Port=5432"
```

**Note:** Can use same database as main database

**Impact:** Background jobs (notifications, escalations) won't run

**Fix Required:** Set Hangfire connection for background jobs

---

## 🟢 OPTIONAL: Feature-Specific Configuration

### 8. Claude AI (Optional)
**Status:** ✅ **OPTIONAL** - Has graceful fallback

**Environment Variables:**
```bash
CLAUDE_API_KEY="sk-ant-api03-YOUR_KEY"
CLAUDE_MODEL="claude-sonnet-4-20250514"
CLAUDE_MAX_TOKENS="4096"
CLAUDE_ENABLED="true"
```

**Impact:** AI features disabled, but application works without it

**Fix Required:** Only if using AI features

---

### 9. Microsoft Graph (Optional)
**Status:** ✅ **OPTIONAL** - Only for email operations

**Environment Variables:**
```bash
MSGRAPH_CLIENT_ID="your-client-id"
MSGRAPH_CLIENT_SECRET="your-client-secret"
MSGRAPH_APP_ID_URI="api://your-app-id"
```

**Impact:** Microsoft Graph email operations disabled

**Fix Required:** Only if using Microsoft Graph integration

---

### 10. Application URLs
**Status:** ⚠️ **RECOMMENDED**

**Environment Variable:**
```bash
ASPNETCORE_URLS="http://0.0.0.0:5000;https://0.0.0.0:5001"
```

**Fix Required:** Set for production deployment

---

## 📋 Quick Setup Script

Create a `.env.production` file with all required variables:

```bash
# CRITICAL - Required for startup
ConnectionStrings__DefaultConnection="Host=localhost;Database=grc_main;Username=grc_user;Password=YOUR_PASSWORD;Port=5432"
ConnectionStrings__GrcAuthDb="Host=localhost;Database=grc_auth;Username=grc_user;Password=YOUR_PASSWORD;Port=5432"
JWT_SECRET="GENERATE_64_CHAR_SECRET"
ASPNETCORE_ENVIRONMENT="Production"
ASPNETCORE_URLS="http://0.0.0.0:5000"

# IMPORTANT - Required for full functionality
SMTP_FROM_EMAIL="noreply@shahin-ai.com"
SMTP_USERNAME="your-email@shahin-ai.com"
SMTP_PASSWORD="your-password"
SMTP_HOST="smtp.office365.com"
SMTP_PORT="587"

# OPTIONAL - For background jobs
ConnectionStrings__HangfireConnection="Host=localhost;Database=grc_main;Username=grc_user;Password=YOUR_PASSWORD;Port=5432"

# OPTIONAL - For caching
ConnectionStrings__Redis="localhost:6379"

# OPTIONAL - For AI features
CLAUDE_API_KEY="sk-ant-api03-YOUR_KEY"
CLAUDE_ENABLED="true"
```

---

## 🔍 Validation Commands

### Check if variables are set:
```bash
# PowerShell
Get-ChildItem Env: | Where-Object { $_.Name -like "*ConnectionStrings*" -or $_.Name -like "*JWT*" -or $_.Name -like "*SMTP*" }

# Linux/Mac
env | grep -E "ConnectionStrings|JWT|SMTP|ASPNETCORE"
```

### Test database connection:
```bash
psql -h localhost -U grc_user -d grc_main
```

### Test Redis connection:
```bash
redis-cli ping
```

---

## 🚨 Startup Failure Scenarios

### Scenario 1: Missing Database Connection
**Error:**
```
InvalidOperationException: The following critical environment variables are missing in Production: CONNECTION_STRING or DB_PASSWORD
```

**Fix:** Set `ConnectionStrings__DefaultConnection` or `DATABASE_URL`

---

### Scenario 2: Missing JWT Secret
**Error:**
```
InvalidOperationException: JWT_SECRET environment variable is required
```

**Fix:** Generate and set `JWT_SECRET` environment variable

---

### Scenario 3: Missing SMTP Configuration
**Warning:**
```
SMTP_HOST not set - Email features may not work in Production
```

**Impact:** Notification Service will fail to send emails

**Fix:** Set SMTP environment variables

---

## ✅ Verification Checklist

Before starting the application, verify:

- [ ] `ConnectionStrings__DefaultConnection` is set
- [ ] `ConnectionStrings__GrcAuthDb` is set
- [ ] `JWT_SECRET` is set (64+ characters)
- [ ] `ASPNETCORE_ENVIRONMENT` is set to "Production"
- [ ] `ASPNETCORE_URLS` is set
- [ ] `SMTP_FROM_EMAIL` is set (for notifications)
- [ ] `SMTP_USERNAME` is set (for notifications)
- [ ] `SMTP_PASSWORD` is set (for notifications)
- [ ] Database is accessible
- [ ] Redis is accessible (if using caching)
- [ ] Hangfire connection is set (if using background jobs)

---

## 📊 Missing Configuration Summary

| Configuration | Status | Impact | Priority |
|---------------|--------|--------|----------|
| Database Connection | ⚠️ Missing | 🔴 App won't start | P0 |
| JWT Secret | ⚠️ Missing | 🔴 Auth won't work | P0 |
| ASP.NET Environment | ⚠️ Missing | 🟡 May use wrong config | P1 |
| SMTP Configuration | ⚠️ Missing | 🟡 Emails won't send | P1 |
| Auth Database | ⚠️ Missing | 🔴 Auth won't work | P0 |
| Redis Connection | ⚠️ Missing | 🟡 Caching disabled | P2 |
| Hangfire Connection | ⚠️ Missing | 🟡 Background jobs disabled | P2 |
| Claude AI Key | ✅ Optional | 🟢 AI disabled (graceful) | P3 |

---

**Next Steps:**
1. Set all P0 (Critical) variables
2. Set all P1 (Important) variables for full functionality
3. Set P2/P3 (Optional) variables as needed
4. Test application startup
5. Verify all features work
