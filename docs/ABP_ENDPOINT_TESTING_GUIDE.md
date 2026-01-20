# 🧪 ABP Endpoint Testing Guide

**Date:** 2026-01-19  
**Purpose:** Test the new ABP-based tenant and admin creation endpoints

---

## 🎯 Testing Checklist

### **Prerequisites**

1. ✅ **Database Migration Applied**
   ```bash
   cd Shahin-Jan-2026/src/GrcMvc
   dotnet ef database update --context GrcDbContext
   ```

2. ✅ **Application Running**
   ```bash
   cd Shahin-Jan-2026/src/GrcMvc
   dotnet run
   ```
   Application should be running on `http://localhost:5010`

3. ✅ **ABP Tables Created**
   - Check that `AbpTenants` table exists
   - Check that `AbpUsers` table exists
   - Check that `AbpUserRoles` table exists

---

## 🧪 Test Scenarios

### **Test 1: Agent Tenant Creation API (ABP Way)**

**Endpoint:** `POST /api/agent/tenant/create`

**Request:**
```json
{
  "organizationName": "Acme Corp",
  "adminEmail": "admin@acme.com",
  "adminPassword": "SecurePass123!"
}
```

**Expected Response:**
```json
{
  "success": true,
  "status": "created",
  "tenantId": "550e8400-e29b-41d4-a716-446655440000",
  "adminUserId": "660e8400-e29b-41d4-a716-446655440000",
  "organizationName": "Acme Corp",
  "redirectUrl": "/onboarding/wizard/fast-start?tenantId=...",
  "loginUrl": "/Account/Login"
}
```

**Verification:**
- ✅ Check `AbpTenants` table for new tenant
- ✅ Check `AbpUsers` table for new admin user (with `TenantId` set)
- ✅ Check `AbpUserRoles` table for admin role assignment
- ✅ Verify `TenantId` in `AbpUsers` matches `Id` in `AbpTenants`

---

### **Test 2: Get Tenant Details**

**Endpoint:** `GET /api/agent/tenant/{tenantId}`

**Expected Response:**
```json
{
  "success": true,
  "tenantId": "550e8400-e29b-41d4-a716-446655440000",
  "organizationName": "Acme Corp",
  "adminUserId": "660e8400-e29b-41d4-a716-446655440000"
}
```

---

### **Test 3: Tenants API with Password (ABP Way)**

**Endpoint:** `POST /api/tenants`

**Request:**
```json
{
  "organizationName": "Test Corp",
  "adminEmail": "admin@test.com",
  "adminPassword": "SecurePass123!"
}
```

**Expected Response:**
```json
{
  "tenantId": "...",
  "organizationName": "Test Corp",
  "adminUserId": "...",
  "message": "Tenant and admin user created successfully"
}
```

---

### **Test 4: Validation Test**

**Endpoint:** `POST /api/agent/tenant/create`

**Request (Invalid Password):**
```json
{
  "organizationName": "Test Corp",
  "adminEmail": "admin@test.com",
  "adminPassword": "short"
}
```

**Expected Response:**
```json
{
  "success": false,
  "error": "Password must be at least 8 characters long"
}
```

**Status Code:** `400 Bad Request`

---

## 🚀 Quick Test Script

Use the provided PowerShell script:

```powershell
cd Shahin-Jan-2026
.\test-abp-endpoints.ps1
```

Or test manually with curl:

```bash
# Test 1: Create tenant + admin
curl -X POST http://localhost:5010/api/agent/tenant/create \
  -H "Content-Type: application/json" \
  -d '{
    "organizationName": "Acme Corp",
    "adminEmail": "admin@acme.com",
    "adminPassword": "SecurePass123!"
  }'

# Test 2: Get tenant
curl http://localhost:5010/api/agent/tenant/{tenantId}
```

---

## 📊 Database Verification

### **Check ABP Tables**

```sql
-- Check tenant was created
SELECT * FROM "AbpTenants" WHERE "Name" = 'Acme Corp';

-- Check admin user was created (in tenant context)
SELECT * FROM "AbpUsers" 
WHERE "Email" = 'admin@acme.com' 
  AND "TenantId" = '<tenant-id>';

-- Check admin role was assigned
SELECT ur.*, r."Name" as RoleName
FROM "AbpUserRoles" ur
JOIN "AbpRoles" r ON ur."RoleId" = r."Id"
WHERE ur."UserId" = '<admin-user-id>'
  AND ur."TenantId" = '<tenant-id>';
```

### **Verify Tenant Context Isolation**

```sql
-- Users should be tenant-scoped
SELECT 
    u."Id",
    u."UserName",
    u."Email",
    u."TenantId",
    t."Name" as TenantName
FROM "AbpUsers" u
JOIN "AbpTenants" t ON u."TenantId" = t."Id"
ORDER BY t."Name", u."Email";
```

---

## ✅ Success Criteria

1. ✅ **Endpoint Returns Success**
   - Status code: `200 OK`
   - Response contains `tenantId` and `adminUserId`

2. ✅ **Database Records Created**
   - Tenant exists in `AbpTenants`
   - Admin user exists in `AbpUsers` with correct `TenantId`
   - Admin role assigned in `AbpUserRoles`

3. ✅ **Tenant Context Isolation**
   - User's `TenantId` matches tenant's `Id`
   - User cannot access other tenants' data

4. ✅ **Validation Works**
   - Short passwords are rejected
   - Missing fields return `400 Bad Request`

---

## 🐛 Troubleshooting

### **Error: "ITenantAppService not found"**
- **Solution:** Ensure ABP modules are properly registered in `GrcMvcAbpModule.cs`

### **Error: "Table 'AbpTenants' does not exist"**
- **Solution:** Run database migration:
  ```bash
  dotnet ef database update --context GrcDbContext
  ```

### **Error: "User already exists"**
- **Solution:** Use unique email addresses for each test

### **Error: "Tenant context not set"**
- **Solution:** Verify `TenantResolutionMiddleware` is configured correctly

---

## 📝 Test Results Template

```
Test Date: ___________
Tester: ___________

Test 1: Agent Tenant Creation API
  Status: [ ] Pass [ ] Fail
  Tenant ID: ___________
  Admin User ID: ___________
  Notes: ___________

Test 2: Get Tenant Details
  Status: [ ] Pass [ ] Fail
  Notes: ___________

Test 3: Tenants API with Password
  Status: [ ] Pass [ ] Fail
  Notes: ___________

Test 4: Validation Test
  Status: [ ] Pass [ ] Fail
  Notes: ___________

Database Verification:
  AbpTenants: [ ] Verified
  AbpUsers: [ ] Verified
  AbpUserRoles: [ ] Verified
  Tenant Context: [ ] Verified
```

---

**Last Updated:** 2026-01-19
