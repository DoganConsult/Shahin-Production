# Complete Production Deployment Guide

**Date**: January 15, 2026  
**Status**: Step-by-Step Deployment Instructions

---

## 🚀 Step 1: Start Application and Monitor Logs

### Option A: Use Monitoring Script (Easiest)

```powershell
powershell -ExecutionPolicy Bypass -File scripts\start-and-monitor.ps1
```

This will:
- ✅ Start the application automatically
- ✅ Monitor logs in real-time
- ✅ Show you when migrations are applied
- ✅ Display any errors

### Option B: Manual Start

```powershell
# 1. Navigate to release directory
cd src\GrcMvc\bin\Release\net8.0

# 2. Set environment variables
$env:JWT_SECRET="your-64-character-secret"
$env:ConnectionStrings__GrcAuthDb="Host=localhost;Database=GrcAuthDb;Username=shahin_admin;Password=Shahin@GRC2026!;Port=5432;SSL Mode=Disable"
$env:ASPNETCORE_ENVIRONMENT="Production"

# 3. Start application with log redirection
Start-Process -FilePath "dotnet" -ArgumentList "GrcMvc.dll" -NoNewWindow -RedirectStandardOutput "startup.log" -RedirectStandardError "startup-errors.log"

# 4. Monitor logs in real-time (in same or new terminal)
Get-Content startup.log -Wait -Tail 20
```

### What to Look For in Logs

**✅ Success Message:**
```
🔄 Applying Auth database migrations...
✅ Auth database migrations applied
```

**Or:**
```
Applying Auth database migrations...
Auth database migrations applied
Done.
```

**❌ If you see errors:**
- Check `startup-errors.log` for details
- Verify environment variables are set
- Check database connection

---

## 🔍 Step 2: Verify Database Schema

### Option A: Using SQL Query (if psql available)

```bash
# Connect to database
psql -h localhost -U shahin_admin -d GrcAuthDb

# Run verification query
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'AspNetUsers'
AND column_name IN (
    'FirstName', 'LastName', 'Department', 'JobTitle',
    'Abilities', 'AssignedScope', 'RoleProfileId', 'KsaCompetencyLevel',
    'KnowledgeAreas', 'Skills', 'IsActive', 'CreatedDate',
    'LastLoginDate', 'RefreshToken', 'RefreshTokenExpiry',
    'MustChangePassword', 'LastPasswordChangedAt'
)
ORDER BY column_name;
```

**Expected Result:** 17 rows returned

### Option B: Using EF Core

```powershell
cd src\GrcMvc
dotnet ef migrations list --context GrcAuthDbContext
```

**Expected Result:** Should show `20260115064458_AddApplicationUserCustomColumns` (not "Pending")

### Option C: Using SQL Script

```bash
# If psql available
psql -h localhost -U shahin_admin -d GrcAuthDb -f scripts\verify-database-schema.sql
```

---

## 🧪 Step 3: Test User Forms

### 3.1 Create New User

1. **Open Application:**
   - Navigate to: `http://localhost:5000` (or your configured port)
   - Or: `http://localhost:8080`

2. **Navigate to User Creation:**
   - Go to: `/Users/Create` or `/Account/Register`
   - Or use the navigation menu

3. **Fill in All Fields:**
   - ✅ **First Name**: Test
   - ✅ **Last Name**: User
   - ✅ **Email**: testuser@example.com
   - ✅ **Department**: IT
   - ✅ **Job Title**: Developer
   - ✅ **Abilities**: `["Coding", "Testing", "Debugging"]` (JSON array)
   - ✅ **Assigned Scope**: Global
   - ✅ **KSA Competency Level**: 3
   - ✅ **Knowledge Areas**: `["Software Development", "Quality Assurance"]`
   - ✅ **Skills**: `["C#", "ASP.NET", "PostgreSQL"]`
   - ✅ **Is Active**: Yes
   - ✅ **Password**: (set a secure password)

4. **Save and Verify:**
   - Click "Save" or "Create"
   - Verify success message appears
   - Verify user appears in user list

### 3.2 Edit Existing User

1. **Navigate to User List:**
   - Go to: `/Users` or `/Users/Index`

2. **Select User to Edit:**
   - Click "Edit" on any user
   - Or navigate to: `/Users/Edit/{userId}`

3. **Verify All Fields Load:**
   - ✅ First Name loads correctly
   - ✅ Last Name loads correctly
   - ✅ Department loads correctly
   - ✅ Job Title loads correctly
   - ✅ Abilities loads correctly (should show JSON array)
   - ✅ Assigned Scope loads correctly
   - ✅ All other fields load correctly

4. **Modify Fields:**
   - Change Job Title to "Senior Developer"
   - Update Abilities to `["Coding", "Testing", "Debugging", "Architecture"]`
   - Modify Assigned Scope

5. **Save and Verify:**
   - Click "Save"
   - Verify success message
   - Reload edit page and verify changes persisted

### 3.3 Verify Database Data

```sql
-- Check created/modified user
SELECT 
    "Id",
    "Email",
    "FirstName",
    "LastName",
    "Department",
    "JobTitle",
    "Abilities",
    "AssignedScope",
    "KsaCompetencyLevel",
    "IsActive",
    "CreatedDate"
FROM "AspNetUsers"
WHERE "Email" = 'testuser@example.com';
```

**Expected Result:** User record with all fields populated

---

## ✅ Step 4: Complete Verification Checklist

### Application Status
- [ ] Application starts without errors
- [ ] Logs show "Auth database migrations applied"
- [ ] Application responds to HTTP requests
- [ ] Health check endpoint works: `/health/ready`

### Database Schema
- [ ] `AspNetUsers` table exists
- [ ] All 17 custom columns present (verify with SQL)
- [ ] Indexes created (IX_AspNetUsers_Email, IX_AspNetUsers_IsActive, IX_AspNetUsers_RoleProfileId)
- [ ] Migration history shows `AddApplicationUserCustomColumns`

### User Forms
- [ ] User creation form works
- [ ] All fields save correctly
- [ ] User editing form works
- [ ] All fields load correctly
- [ ] Changes persist after save

### ApplicationUser Properties
- [ ] FirstName saves/loads
- [ ] LastName saves/loads
- [ ] Department saves/loads
- [ ] JobTitle saves/loads
- [ ] Abilities saves/loads (JSON array)
- [ ] AssignedScope saves/loads
- [ ] RoleProfileId saves/loads
- [ ] KsaCompetencyLevel saves/loads
- [ ] KnowledgeAreas saves/loads
- [ ] Skills saves/loads
- [ ] IsActive saves/loads
- [ ] CreatedDate is set automatically
- [ ] All other properties work correctly

---

## 🎯 Quick Test Commands

### Check Application Status
```powershell
# Check if running
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.Path -like "*GrcMvc*" }

# Test HTTP endpoint (if curl available)
curl http://localhost:5000/health/ready
```

### Check Logs
```powershell
# View recent logs
Get-Content startup.log -Tail 30

# Search for migration
Get-Content startup.log | Select-String -Pattern "migration|applied"

# Check for errors
Get-Content startup-errors.log
```

### Verify Database
```powershell
# Check migration status
cd src\GrcMvc
dotnet ef migrations list --context GrcAuthDbContext
```

---

## 🎉 Success Criteria

Your deployment is **complete and successful** when:

1. ✅ Application starts without errors
2. ✅ Logs show "Auth database migrations applied"
3. ✅ Database has all 17 custom columns (verified with SQL)
4. ✅ User creation form works with all fields
5. ✅ User editing form works with all fields
6. ✅ All ApplicationUser properties save/load correctly

---

## 📞 Troubleshooting

### Application Won't Start

1. **Check Environment Variables:**
   ```powershell
   $env:JWT_SECRET
   $env:ConnectionStrings__GrcAuthDb
   $env:ASPNETCORE_ENVIRONMENT
   ```

2. **Check Error Log:**
   ```powershell
   Get-Content startup-errors.log
   ```

3. **Check Database Connection:**
   - Verify database is running
   - Verify connection string is correct
   - Test connection manually

### Migration Not Applied

1. **Check Migration Status:**
   ```powershell
   dotnet ef migrations list --context GrcAuthDbContext
   ```

2. **Apply Migration Manually:**
   ```powershell
   dotnet ef database update --context GrcAuthDbContext
   ```

### Forms Not Working

1. **Check Browser Console:** Look for JavaScript errors
2. **Check Application Logs:** Look for server-side errors
3. **Verify Database:** Ensure columns exist
4. **Check Model Binding:** Verify form field names match entity properties

---

**🎊 Once all checks pass, your application is production-ready!**
