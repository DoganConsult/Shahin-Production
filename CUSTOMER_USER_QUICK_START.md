# 🚀 Customer User Quick Start Guide

## ✅ Complete Setup for Customer User

This guide will help you set up everything needed for a customer user to start using the GRC system.

---

## 📋 Prerequisites Checklist

- [x] .NET 8.0 SDK installed
- [x] PostgreSQL database available
- [x] Application built and ready
- [ ] Database connection configured
- [ ] Database migrations applied
- [ ] Application started (seed data runs automatically)

---

## 🔧 Step-by-Step Setup

### Step 1: Configure Database Connection

Edit `src/GrcMvc/appsettings.json` or `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=GrcMvcDb;Username=postgres;Password=yourpassword;Port=5432"
  }
}
```

### Step 2: Apply Database Migrations

```bash
cd /root/.cursor/worktrees/GRC__Workspace___SSH__doganconsult_/bsk/src/GrcMvc
dotnet ef database update --project GrcMvc.csproj --context GrcDbContext
```

This will:
- Create all database tables
- Set up the schema for all modules
- Prepare the database for seed data

### Step 3: Start the Application

The application will automatically seed data on first startup:

```bash
# Option 1: Development mode
cd src/GrcMvc
dotnet run

# Option 2: Production mode (using published build)
cd publish
dotnet GrcMvc.dll --environment Production

# Option 3: Docker
docker-compose up -d
```

### Step 4: Wait for Initialization

On first startup, the application will automatically:
1. ✅ Create default tenant
2. ✅ Seed role profiles (15 roles)
3. ✅ Seed workflow definitions (10 workflow types)
4. ✅ Seed RBAC system:
   - 40+ permissions
   - 19 features
   - 8 default roles
   - Role-permission mappings
   - Role-feature mappings
5. ✅ Create default users:
   - **Admin User** (SuperAdmin role)
   - **Manager User** (ComplianceManager role)
6. ✅ Link users to default tenant

**Note:** Initialization runs in the background and may take 1-2 minutes.

---

## 🔐 Default Login Credentials

### Admin User
- **Email:** `admin@grcsystem.com`
- **Password:** `Admin@123456`
- **Role:** SuperAdmin (full system access)

### Manager User
- **Email:** `manager@grcsystem.com`
- **Password:** `Manager@123456`
- **Role:** ComplianceManager (compliance oversight)

---

## 🎯 First Login Steps

1. **Access the application:**
   - Development: `https://localhost:5001` or `http://localhost:5000`
   - Production: Configure your domain/port

2. **Login with admin credentials:**
   - Use `admin@grcsystem.com` / `Admin@123456`

3. **Change default password:**
   - Go to user profile settings
   - Change password to a secure one

4. **Verify system:**
   - Check dashboard loads
   - Verify menu items are visible
   - Test creating a test record

---

## 📊 What Gets Created Automatically

### Roles Created
- SuperAdmin
- TenantAdmin
- ComplianceManager
- RiskManager
- Auditor
- EvidenceOfficer
- VendorManager
- Viewer

### Permissions Created
- 40+ granular permissions for all modules
- View, Create, Update, Delete, Approve permissions
- Module-specific permissions

### Features Created
- Home, Dashboard, Subscriptions
- Admin (Users, Roles, Tenants)
- Frameworks, Regulators
- Assessments, Control Assessments
- Evidence, Risks, Audits
- Action Plans, Policies
- Compliance Calendar, Workflow
- Notifications, Vendors
- Reports, Integrations

### Workflow Definitions
- 10 complete workflow types
- Multi-level approval workflows
- Task management workflows

---

## 🔍 Verification Checklist

After first startup, verify:

- [ ] Application starts without errors
- [ ] Database tables created
- [ ] Can login with admin credentials
- [ ] Dashboard loads
- [ ] Menu items visible
- [ ] Can create a test record
- [ ] Permissions working
- [ ] Roles assigned correctly

---

## 🚨 Troubleshooting

### Database Connection Issues
```bash
# Test PostgreSQL connection
psql -h localhost -U postgres -d GrcMvcDb

# Check if database exists
psql -U postgres -l | grep GrcMvcDb
```

### Migration Issues
```bash
# List pending migrations
dotnet ef migrations list --project GrcMvc.csproj

# Check migration status
dotnet ef database update --project GrcMvc.csproj --context GrcDbContext --verbose
```

### Seed Data Not Running
- Check application logs: `/app/logs/grcmvc-.log`
- Look for "Starting application initialization" message
- Verify database connection is working
- Check that default tenant exists

### User Cannot Login
- Verify user was created: Check `AspNetUsers` table
- Verify role assignment: Check `AspNetUserRoles` table
- Verify tenant link: Check `TenantUsers` table
- Check password hasn't been changed

---

## 📝 Post-Setup Configuration

### 1. Email Settings
Configure in `appsettings.Production.json`:
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

### 2. File Storage
Configure file storage for document uploads:
```json
{
  "FileStorage": {
    "Provider": "Local",
    "Path": "/app/uploads"
  }
}
```

### 3. JWT Settings
Set a strong secret key (minimum 32 characters):
```json
{
  "JwtSettings": {
    "Secret": "your-very-long-secret-key-minimum-32-characters-required",
    "Issuer": "your-domain.com",
    "Audience": "your-domain.com"
  }
}
```

---

## 🎉 Success Indicators

You'll know setup is complete when:

✅ Application starts without errors  
✅ Can login with admin credentials  
✅ Dashboard shows data  
✅ All menu items are visible  
✅ Can create/edit records  
✅ Permissions are enforced  
✅ Background jobs are running  

---

## 📞 Quick Reference

**Application Location:** `/root/.cursor/worktrees/GRC__Workspace___SSH__doganconsult_/bsk`  
**Published Build:** `publish/` directory  
**Logs:** `/app/logs/grcmvc-.log`  
**Health Check:** `http://localhost/health`  
**Hangfire Dashboard:** `http://localhost/hangfire`

---

**Status:** ✅ Ready for Customer User  
**Last Updated:** January 5, 2026
