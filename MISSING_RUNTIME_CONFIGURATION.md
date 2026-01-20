# Missing Runtime Configuration
**Generated:** 2026-01-12  
**Status:** 🔴 **CRITICAL - Application Cannot Start Without These**

---

## 🔴 CRITICAL: Application Will Fail to Start

### 1. Database Connection String
**Error if Missing:**
```
InvalidOperationException: The following critical environment variables are missing in Production: CONNECTION_STRING or DB_PASSWORD
```

**Location:** `Program.cs:201-209`

**Required:**
```bash
# Set ONE of these:
ConnectionStrings__DefaultConnection="Host=localhost;Database=grc_main;Username=grc_user;Password=YOUR_PASSWORD;Port=5432"

# OR
DATABASE_URL="postgresql://user:password@host:port/dbname"

# OR individual components
DB_HOST=localhost
DB_PORT=5432
DB_NAME=grc_main
DB_USER=grc_user
DB_PASSWORD=YOUR_PASSWORD
```

**Action:** Set before starting application

---

### 2. JWT Secret
**Error if Missing:**
```
InvalidOperationException: JWT_SECRET environment variable is required
```

**Location:** `AuthenticationService.cs:648`

**Required:**
```bash
JWT_SECRET="YOUR_64_CHARACTER_SECRET"
```

**Generate:**
```powershell
# PowerShell
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object {[char]$_})
```

**Action:** Generate and set before starting application

---

### 3. Auth Database Connection
**Error if Missing:** Authentication will fail

**Required:**
```bash
ConnectionStrings__GrcAuthDb="Host=localhost;Database=grc_auth;Username=grc_user;Password=YOUR_PASSWORD;Port=5432"
```

**Action:** Set before starting application

---

## 🟡 IMPORTANT: Features Won't Work

### 4. SMTP Configuration
**Impact:** Notification Service cannot send emails

**Required:**
```bash
SMTP_FROM_EMAIL="noreply@shahin-ai.com"
SMTP_USERNAME="your-email@shahin-ai.com"
SMTP_PASSWORD="your-password"
SMTP_HOST="smtp.office365.com"
SMTP_PORT="587"
```

**Action:** Set for email notifications to work

---

### 5. ASP.NET Core Environment
**Impact:** May use wrong configuration

**Required:**
```bash
ASPNETCORE_ENVIRONMENT="Production"
ASPNETCORE_URLS="http://0.0.0.0:5000"
```

**Action:** Set for production deployment

---

## 📋 Complete Setup Script

Create `.env.production` or set environment variables:

```bash
# ============================================
# CRITICAL - Required for Application Startup
# ============================================
export ConnectionStrings__DefaultConnection="Host=localhost;Database=grc_main;Username=grc_user;Password=YOUR_PASSWORD;Port=5432"
export ConnectionStrings__GrcAuthDb="Host=localhost;Database=grc_auth;Username=grc_user;Password=YOUR_PASSWORD;Port=5432"
export JWT_SECRET="GENERATE_64_CHAR_SECRET_HERE"
export ASPNETCORE_ENVIRONMENT="Production"
export ASPNETCORE_URLS="http://0.0.0.0:5000"

# ============================================
# IMPORTANT - Required for Full Functionality
# ============================================
export SMTP_FROM_EMAIL="noreply@shahin-ai.com"
export SMTP_USERNAME="your-email@shahin-ai.com"
export SMTP_PASSWORD="your-password"
export SMTP_HOST="smtp.office365.com"
export SMTP_PORT="587"

# ============================================
# OPTIONAL - For Background Jobs
# ============================================
export ConnectionStrings__HangfireConnection="Host=localhost;Database=grc_main;Username=grc_user;Password=YOUR_PASSWORD;Port=5432"

# ============================================
# OPTIONAL - For Caching
# ============================================
export ConnectionStrings__Redis="localhost:6379"

# ============================================
# OPTIONAL - For AI Features
# ============================================
export CLAUDE_API_KEY="sk-ant-api03-YOUR_KEY"
export CLAUDE_ENABLED="true"
```

---

## ✅ Quick Fix Commands

### Generate JWT Secret:
```powershell
# PowerShell
$secret = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object {[char]$_})
Write-Host "JWT_SECRET=$secret"
```

### Set All Critical Variables (PowerShell):
```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Database=grc_main;Username=grc_user;Password=YOUR_PASSWORD;Port=5432"
$env:ConnectionStrings__GrcAuthDb = "Host=localhost;Database=grc_auth;Username=grc_user;Password=YOUR_PASSWORD;Port=5432"
$env:JWT_SECRET = "GENERATE_64_CHAR_SECRET"
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ASPNETCORE_URLS = "http://0.0.0.0:5000"
```

---

## 🚨 What Happens If Missing

| Variable | Startup | Authentication | Notifications | Background Jobs |
|----------|---------|----------------|---------------|-----------------|
| Database Connection | ❌ FAILS | ❌ N/A | ❌ N/A | ❌ N/A |
| JWT_SECRET | ✅ Starts | ❌ FAILS | ✅ Works | ✅ Works |
| Auth Database | ✅ Starts | ❌ FAILS | ✅ Works | ✅ Works |
| SMTP Config | ✅ Starts | ✅ Works | ❌ FAILS | ✅ Works |
| Hangfire | ✅ Starts | ✅ Works | ✅ Works | ❌ FAILS |

---

**Priority:**
1. 🔴 **P0:** Database Connection, JWT_SECRET, Auth Database (App won't start/work)
2. 🟡 **P1:** SMTP Config, ASP.NET Environment (Features won't work)
3. 🟢 **P2:** Hangfire, Redis (Optional features)
