# 🚂 Railway SSH Connection Test

## Your Railway Service Details

- **Project ID:** `04c6aa68-c21b-4185-87a1-51142de7a839`
- **Environment ID:** `16a66d5c-eb10-4eca-8485-6dc941daed80`
- **Service ID:** `326f5e9d-19d0-4bdc-aeff-27cbf2545aa3`

---

## 🔧 Railway CLI Setup

### Install Railway CLI (if not installed)

**Windows (PowerShell):**
```powershell
# Using Scoop
scoop install railway

# Or using npm
npm i -g @railway/cli

# Or download from: https://github.com/railwayapp/cli/releases
```

**Verify Installation:**
```powershell
railway --version
```

**Login:**
```powershell
railway login
```

---

## 🧪 Test Database Connection via Railway SSH

### Method 1: Check Environment Variables

```bash
railway ssh --project=04c6aa68-c21b-4185-87a1-51142de7a839 --environment=16a66d5c-eb10-4eca-8485-6dc941daed80 --service=326f5e9d-19d0-4bdc-aeff-27cbf2545aa3

# Once connected, run:
echo "DATABASE_URL:"
printenv DATABASE_URL

echo ""
echo "All connection-related variables:"
printenv | grep -i connection
```

### Method 2: Test Database Connection Directly

```bash
railway ssh --project=04c6aa68-c21b-4185-87a1-51142de7a839 --environment=16a66d5c-eb10-4eca-8485-6dc941daed80 --service=326f5e9d-19d0-4bdc-aeff-27cbf2545aa3

# Once connected, test PostgreSQL connection:
psql $DATABASE_URL -c "SELECT version(), current_database(), current_user;"
```

### Method 3: Run Application Test Command

```bash
railway ssh --project=04c6aa68-c21b-4185-87a1-51142de7a839 --environment=16a66d5c-eb10-4eca-8485-6dc941daed80 --service=326f5e9d-19d0-4bdc-aeff-27cbf2545aa3

# Once connected:
cd /app  # or wherever your app is deployed
dotnet run -- TestDb
```

---

## 📊 What to Check in Railway

### 1. Environment Variables
- ✅ `DATABASE_URL` should be set automatically by Railway
- ✅ Check in Railway Dashboard → Your Service → Variables

### 2. Database Service
- ✅ PostgreSQL service should be linked to your application
- ✅ Check Railway Dashboard → Your Project → Services

### 3. Application Logs
- ✅ Check logs for connection string resolution:
  ```
  [CONFIG] ✅ Converted Railway DATABASE_URL to connection string
  [CONFIG] ✅ Connection string format validated
  ```

---

## 🔍 Quick Commands for Railway SSH

### Check DATABASE_URL:
```bash
railway ssh --project=04c6aa68-c21b-4185-87a1-51142de7a839 --environment=16a66d5c-eb10-4eca-8485-6dc941daed80 --service=326f5e9d-19d0-4bdc-aeff-27cbf2545aa3 "printenv DATABASE_URL"
```

### Test PostgreSQL Connection:
```bash
railway ssh --project=04c6aa68-c21b-4185-87a1-51142de7a839 --environment=16a66d5c-eb10-4eca-8485-6dc941daed80 --service=326f5e9d-19d0-4bdc-aeff-27cbf2545aa3 "psql \$DATABASE_URL -c 'SELECT version(), current_database(), current_user;'"
```

### Check Application Configuration:
```bash
railway ssh --project=04c6aa68-c21b-4185-87a1-51142de7a839 --environment=16a66d5c-eb10-4eca-8485-6dc941daed80 --service=326f5e9d-19d0-4bdc-aeff-27cbf2545aa3 "cd /app && dotnet run -- TestDb"
```

---

## 🎯 Expected Results

### ✅ If DATABASE_URL is Set:
```
DATABASE_URL=postgresql://postgres:password@host.railway.app:5432/railway
```

### ✅ If Connection Works:
```
PostgreSQL 15.x
Database: railway
User: postgres
```

### ✅ If Application Connects:
```
[CONFIG] ✅ Converted Railway DATABASE_URL to connection string
[CONFIG] ✅ Connection string format validated
✅ Connection Test Successful!
```

---

## 🚀 Alternative: Use Railway Dashboard

If Railway CLI is not available, you can:

1. **Check Environment Variables:**
   - Go to Railway Dashboard
   - Select your service
   - Go to Variables tab
   - Look for `DATABASE_URL`

2. **Check Logs:**
   - Go to Railway Dashboard
   - Select your service
   - Go to Deployments → Latest → Logs
   - Look for `[CONFIG]` messages

3. **Test Connection:**
   - Railway automatically sets `DATABASE_URL`
   - Your application auto-converts it
   - Check application logs for connection success

---

**Status:** Ready to test Railway connection via SSH! 🚂
