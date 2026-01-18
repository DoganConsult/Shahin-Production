# Database Health Check Report
**Date:** 2026-01-15  
**Issue:** Users cannot login or register new accounts

## 🔴 CRITICAL ISSUE IDENTIFIED

### Root Cause
The application **cannot connect to the database** because the connection strings are not configured.

### Evidence
1. **appsettings.json** - Connection strings are empty:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "",
     "GrcAuthDb": "",
     "Redis": "",
     "HangfireConnection": ""
   }
   ```

2. **appsettings.Production.json** - Uses environment variables that are likely not set:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "${ConnectionStrings__DefaultConnection}",
     "GrcAuthDb": "${ConnectionStrings__GrcAuthDb}",
     "Redis": "${ConnectionStrings__Redis}",
     "HangfireConnection": "${ConnectionStrings__HangfireConnection}"
   }
   ```

### Impact
- ❌ Users cannot login (database authentication fails)
- ❌ Users cannot register (cannot create user records)
- ❌ All database-dependent features are non-functional
- ❌ Application may crash or show errors when accessing any authenticated pages

---

## 🔧 SOLUTION

### Option 1: Set Environment Variables (RECOMMENDED for Production)

Set the following environment variables on your server:

```bash
# Main Application Database (PostgreSQL)
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=YOUR_PASSWORD;Include Error Detail=true"

# Authentication Database (PostgreSQL)
export ConnectionStrings__GrcAuthDb="Host=localhost;Port=5432;Database=GrcAuthDb;Username=postgres;Password=YOUR_PASSWORD;Include Error Detail=true"

# Redis Cache (Optional but recommended)
export ConnectionStrings__Redis="localhost:6379,abortConnect=false"

# Hangfire Background Jobs Database
export ConnectionStrings__HangfireConnection="Host=localhost;Port=5432;Database=HangfireDb;Username=postgres;Password=YOUR_PASSWORD;Include Error Detail=true"

# JWT Secret (CRITICAL for authentication)
export JWT_SECRET="YOUR_SECURE_RANDOM_STRING_AT_LEAST_32_CHARACTERS"

# SMTP Settings (for email verification)
export SMTP_FROM_EMAIL="info@shahin-ai.com"
export SMTP_USERNAME="info@shahin-ai.com"
export SMTP_PASSWORD="YOUR_SMTP_PASSWORD"
```

**For Windows (PowerShell):**
```powershell
$env:ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=YOUR_PASSWORD;Include Error Detail=true"
$env:ConnectionStrings__GrcAuthDb="Host=localhost;Port=5432;Database=GrcAuthDb;Username=postgres;Password=YOUR_PASSWORD;Include Error Detail=true"
$env:JWT_SECRET="YOUR_SECURE_RANDOM_STRING_AT_LEAST_32_CHARACTERS"
```

### Option 2: Update appsettings.Local.json (For Development/Testing)

Create or update `appsettings.Local.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=GrcMvcDb;Username=postgres;Password=YOUR_PASSWORD;Include Error Detail=true",
    "GrcAuthDb": "Host=localhost;Port=5432;Database=GrcAuthDb;Username=postgres;Password=YOUR_PASSWORD;Include Error Detail=true",
    "Redis": "localhost:6379,abortConnect=false",
    "HangfireConnection": "Host=localhost;Port=5432;Database=HangfireDb;Username=postgres;Password=YOUR_PASSWORD;Include Error Detail=true"
  },
  "JwtSettings": {
    "Secret": "YOUR_SECURE_RANDOM_STRING_AT_LEAST_32_CHARACTERS_LONG"
  },
  "SmtpSettings": {
    "Host": "smtp.office365.com",
    "Port": 587,
    "EnableSsl": true,
    "FromEmail": "info@shahin-ai.com",
    "FromName": "Shahin AI",
    "Username": "info@shahin-ai.com",
    "Password": "YOUR_SMTP_PASSWORD"
  }
}
```

---

## 📋 VERIFICATION STEPS

### Step 1: Verify Database Server is Running

**PostgreSQL:**
```bash
# Check if PostgreSQL is running
sudo systemctl status postgresql

# Or on Windows
Get-Service postgresql*

# Test connection
psql -h localhost -U postgres -d GrcMvcDb
```

### Step 2: Verify Databases Exist

```sql
-- Connect to PostgreSQL
psql -U postgres

-- List all databases
\l

-- You should see:
-- GrcMvcDb (main application database)
-- GrcAuthDb (authentication database)
-- HangfireDb (background jobs database)
```

### Step 3: Create Missing Databases (if needed)

```sql
-- Create databases if they don't exist
CREATE DATABASE "GrcMvcDb";
CREATE DATABASE "GrcAuthDb";
CREATE DATABASE "HangfireDb";

-- Grant permissions
GRANT ALL PRIVILEGES ON DATABASE "GrcMvcDb" TO postgres;
GRANT ALL PRIVILEGES ON DATABASE "GrcAuthDb" TO postgres;
GRANT ALL PRIVILEGES ON DATABASE "HangfireDb" TO postgres;
```

### Step 4: Run Database Migrations

```bash
# Navigate to the project directory
cd Shahin-Jan-2026/src/GrcMvc

# Run migrations for main database
dotnet ef database update --context GrcDbContext

# Run migrations for auth database
dotnet ef database update --context GrcAuthDbContext
```

### Step 5: Restart the Application

```bash
# Stop the application
# Then start it again to pick up the new configuration

# For development
dotnet run

# For production (if using systemd)
sudo systemctl restart shahin-ai
```

### Step 6: Test Login/Registration

1. Navigate to: `https://portal.shahin-ai.com/account/login`
2. Try to register a new user
3. Check application logs for any database connection errors

---

## 🔍 DIAGNOSTIC COMMANDS

### Check Application Logs
```bash
# View recent logs
tail -f Shahin-Jan-2026/src/GrcMvc/logs/grc-system-*.log

# Search for database errors
grep -i "database\|connection" Shahin-Jan-2026/src/GrcMvc/logs/grc-system-*.log
```

### Test Database Connection from Application
```bash
# Run a simple database health check
cd Shahin-Jan-2026/src/GrcMvc
dotnet run --urls="http://localhost:5000"

# Then visit: http://localhost:5000/health
```

---

## 📊 EXPECTED DATABASE SCHEMA

### GrcAuthDb (Authentication Database)
- AspNetUsers
- AspNetRoles
- AspNetUserRoles
- AspNetUserClaims
- AspNetUserLogins
- AspNetUserTokens
- PasswordHistory
- AuthenticationAuditEvents
- UserSessions

### GrcMvcDb (Main Application Database)
- Tenants
- TenantUsers
- Organizations
- Risks
- Controls
- Assessments
- AuditEvents
- WorkflowTasks
- (and many more GRC-related tables)

---

## ⚠️ SECURITY NOTES

1. **Never commit database passwords** to version control
2. **Use strong passwords** for database users (minimum 16 characters)
3. **Use environment variables** in production
4. **Rotate JWT secrets** regularly
5. **Enable SSL/TLS** for database connections in production
6. **Restrict database access** to application server IP only

---

## 🚀 QUICK FIX FOR IMMEDIATE TESTING

If you need to get the system working immediately for testing:

1. **Install PostgreSQL** (if not already installed)
2. **Create a simple connection string** in `appsettings.Local.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=GrcMvcDb;Username=postgres;Password=postgres",
       "GrcAuthDb": "Host=localhost;Database=GrcAuthDb;Username=postgres;Password=postgres"
     },
     "JwtSettings": {
       "Secret": "ThisIsATemporarySecretKeyForTestingOnly123456789"
     }
   }
   ```
3. **Run migrations**: `dotnet ef database update`
4. **Start the app**: `dotnet run`

---

## 📞 NEXT STEPS

1. ✅ Set up database connection strings (choose Option 1 or 2 above)
2. ✅ Verify databases exist and are accessible
3. ✅ Run database migrations
4. ✅ Configure SMTP settings for email verification
5. ✅ Restart the application
6. ✅ Test user registration and login
7. ✅ Monitor application logs for any errors

---

## 📝 ADDITIONAL NOTES

### Why Login/Registration Fails Without Database:

1. **Registration Flow:**
   - User submits registration form
   - Application tries to create user in `AspNetUsers` table
   - **FAILS** - Cannot connect to GrcAuthDb
   - User sees error or timeout

2. **Login Flow:**
   - User submits credentials
   - Application tries to query `AspNetUsers` table
   - **FAILS** - Cannot connect to GrcAuthDb
   - User sees "Invalid login attempt" or connection error

3. **Email Verification:**
   - After registration, verification email should be sent
   - Verification token stored in database
   - **FAILS** - Cannot store token without database connection

### Current Authentication Configuration:
- Uses **ASP.NET Core Identity** for user management
- Requires **two databases**: GrcAuthDb (auth) and GrcMvcDb (application data)
- Email verification is **REQUIRED** before login (see AccountController.cs line 109-130)
- Password requirements: minimum 8 characters, complexity rules enforced

---

**Report Generated:** 2026-01-15  
**Status:** 🔴 CRITICAL - Database connection required for login/registration  
**Priority:** P0 - Immediate action required
