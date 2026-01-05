# 🚀 Start Customer User - Complete Guide

## ✅ Everything You Need to Start

This document contains **all steps** needed to get a customer user started with the GRC system.

---

## 📋 Quick Start (3 Steps)

### Step 1: Configure Database

Edit `src/GrcMvc/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=GrcMvcDb;Username=postgres;Password=yourpassword;Port=5432"
  }
}
```

### Step 2: Apply Migrations

```bash
cd /root/.cursor/worktrees/GRC__Workspace___SSH__doganconsult_/bsk/src/GrcMvc

# Remove hash.csproj temporarily or specify project explicitly
dotnet ef database update --project GrcMvc.csproj --context GrcDbContext
```

**OR** if that doesn't work:

```bash
# Navigate to parent directory and specify startup project
cd /root/.cursor/worktrees/GRC__Workspace___SSH__doganconsult_/bsk
dotnet ef database update --project src/GrcMvc/GrcMvc.csproj --startup-project src/GrcMvc/GrcMvc.csproj --context GrcDbContext
```

### Step 3: Start Application

```bash
cd /root/.cursor/worktrees/GRC__Workspace___SSH__doganconsult_/bsk/src/GrcMvc
dotnet run
```

**The application will automatically:**
- ✅ Create default tenant
- ✅ Seed all role profiles
- ✅ Seed workflow definitions
- ✅ Set up RBAC system (permissions, features, roles)
- ✅ Create admin and manager users
- ✅ Link users to tenant

**Wait 1-2 minutes** for initialization to complete.

---

## 🔐 Login Credentials

### Admin User (Full Access)
- **Email:** `admin@grcsystem.com`
- **Password:** `Admin@123456`
- **Role:** SuperAdmin

### Manager User (Compliance Access)
- **Email:** `manager@grcsystem.com`
- **Password:** `Manager@123456`
- **Role:** ComplianceManager

---

## ✅ Verification Checklist

After starting the application, verify:

1. **Application Starts:**
   ```bash
   # Check logs for:
   # "🚀 Starting application initialization"
   # "✅ Application initialization completed"
   ```

2. **Can Login:**
   - Go to login page
   - Use admin credentials
   - Should successfully login

3. **Dashboard Works:**
   - Dashboard should load
   - Menu items visible
   - No errors in browser console

4. **Database Has Data:**
   ```sql
   -- Check users
   SELECT * FROM "AspNetUsers";
   
   -- Check roles
   SELECT * FROM "AspNetRoles";
   
   -- Check tenant
   SELECT * FROM "Tenants";
   ```

---

## 🎯 What Gets Created Automatically

### On First Startup:

1. **Default Tenant**
   - Organization: Default Organization
   - Slug: "default"

2. **Roles (8 roles)**
   - SuperAdmin
   - TenantAdmin
   - ComplianceManager
   - RiskManager
   - Auditor
   - EvidenceOfficer
   - VendorManager
   - Viewer

3. **Permissions (40+ permissions)**
   - All module permissions
   - View, Create, Update, Delete, Approve

4. **Features (19 features)**
   - All menu items
   - Home, Dashboard, Admin, etc.

5. **Users (2 users)**
   - Admin user (SuperAdmin)
   - Manager user (ComplianceManager)

6. **Role Profiles (15 profiles)**
   - Executive, Management, Operational, Support layers

7. **Workflow Definitions (10 workflows)**
   - All workflow types configured

---

## 🚨 Troubleshooting

### Database Connection Error
```bash
# Test PostgreSQL
psql -h localhost -U postgres -d GrcMvcDb

# If database doesn't exist:
createdb -U postgres GrcMvcDb
```

### Migration Errors
```bash
# Check migration status
dotnet ef migrations list --project src/GrcMvc/GrcMvc.csproj

# Force update
dotnet ef database update --project src/GrcMvc/GrcMvc.csproj --context GrcDbContext --verbose
```

### Users Not Created
- Check application logs: Look for "Starting application initialization"
- Verify database connection works
- Check that default tenant was created
- Review error logs in `/app/logs/grcmvc-errors-.log`

### Cannot Login
- Verify user exists: `SELECT * FROM "AspNetUsers" WHERE "Email" = 'admin@grcsystem.com';`
- Check password hasn't been changed
- Verify email is confirmed: `EmailConfirmed = true`
- Check user is active: `IsActive = true`

---

## 📝 Post-Setup Configuration

### 1. Change Default Passwords
- Login as admin
- Go to Profile Settings
- Change password

### 2. Configure Email (Optional)
Edit `appsettings.Production.json`:
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "From": "noreply@yourdomain.com"
  }
}
```

### 3. Configure JWT Secret
```json
{
  "JwtSettings": {
    "Secret": "your-very-long-secret-key-minimum-32-characters-required"
  }
}
```

---

## 🎉 Success Indicators

You'll know everything is working when:

✅ Application starts without errors  
✅ Can login with admin credentials  
✅ Dashboard loads with data  
✅ All menu items are visible  
✅ Can create/edit records  
✅ Permissions are enforced  
✅ Background jobs running (Hangfire)  

---

## 📞 Quick Commands

```bash
# Start application
cd src/GrcMvc && dotnet run

# Check health
curl http://localhost:5000/health

# View logs
tail -f /app/logs/grcmvc-.log

# Check database
psql -U postgres -d GrcMvcDb -c "SELECT COUNT(*) FROM \"AspNetUsers\";"
```

---

## 📚 Additional Resources

- **Detailed Guide:** `CUSTOMER_USER_QUICK_START.md`
- **Deployment Guide:** `PRODUCTION_DEPLOYMENT_RELEASE.md`
- **Production Build:** `publish/` directory

---

**Status:** ✅ Ready to Start  
**Last Updated:** January 5, 2026
