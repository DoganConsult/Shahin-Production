# 🚂 Railway Service Guide

## Service IDs in Your Project

### Services Found:

1. **Postgres Service**
   - Service ID: `326f5e9d-19d0-4bdc-aeff-27cbf2545aa3`
   - Type: Database (PostgreSQL)

2. **Redis Service**
   - Service ID: `1330d635-c5f8-4843-8be2-16b2a9655f87`
   - Type: Cache (Redis)

3. **Application Service** (Previously configured)
   - Service ID: `0cb7da15-a249-4cba-a197-677e800c306a`
   - Type: Application (ASP.NET Core)

---

## ✅ Variables Already Added

We've already added all required variables to the **Application Service**:
- Service ID: `0cb7da15-a249-4cba-a197-677e800c306a`

### Variables Configured:
- ✅ `DATABASE_URL` = `${{ Postgres.DATABASE_URL }}`
- ✅ `ASPNETCORE_ENVIRONMENT` = `Production`
- ✅ `ASPNETCORE_URLS` = `http://0.0.0.0:5000`
- ✅ `JWT_SECRET` = [Generated secret]
- ✅ `JwtSettings__Issuer` = `https://portal.shahin-ai.com`
- ✅ `JwtSettings__Audience` = `https://portal.shahin-ai.com`

---

## 🔍 How to Find Your Application Service

### Method 1: Railway Dashboard
1. Go to Railway Dashboard
2. Select your project: **Shahin-ai.com**
3. Look for services:
   - One should be your **application** (ASP.NET Core)
   - One is **Postgres** (database)
   - One is **Redis** (cache)

### Method 2: Check Service Names
- Application service might be named: "GrcMvc", "Shahin", "API", or similar
- Database service: "Postgres"
- Cache service: "Redis"

---

## 🧪 Test Application Service Connection

To SSH into your application service:

```bash
railway ssh --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a
```

Then check variables:
```bash
printenv | grep -E 'DATABASE_URL|JWT_SECRET|ASPNETCORE'
```

---

## 📋 Service Summary

| Service | ID | Purpose | Variables Added |
|---------|-----|---------|----------------|
| **Application** | `0cb7da15-a249-4cba-a197-677e800c306a` | ASP.NET Core App | ✅ Yes |
| **Postgres** | `326f5e9d-19d0-4bdc-aeff-27cbf2545aa3` | Database | N/A (auto-configured) |
| **Redis** | `1330d635-c5f8-4843-8be2-16b2a9655f87` | Cache | N/A (auto-configured) |

---

## ✅ Next Steps

1. **Verify Application Service Variables:**
   ```bash
   railway variable list -s 0cb7da15-a249-4cba-a197-677e800c306a -e 03604398-8431-4c35-8fce-e230c4c8d585
   ```

2. **SSH into Application Service:**
   ```bash
   railway ssh --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a
   ```

3. **Check Application Logs:**
   - Go to Railway Dashboard
   - Select your application service
   - Check Deployments → Latest → Logs
   - Look for: `[CONFIG] ✅ Converted Railway DATABASE_URL`

---

**All variables are configured on the application service!** ✅
