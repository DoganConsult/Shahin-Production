# 🚂 Railway SSH - Quick Test Guide

## Your Railway Service

**SSH Command:**
```bash
railway ssh --project=04c6aa68-c21b-4185-87a1-51142de7a839 --environment=16a66d5c-eb10-4eca-8485-6dc941daed80 --service=326f5e9d-19d0-4bdc-aeff-27cbf2545aa3
```

---

## 🚀 Quick Test Steps

### Step 1: Connect to Railway
```bash
railway ssh --project=04c6aa68-c21b-4185-87a1-51142de7a839 --environment=16a66d5c-eb10-4eca-8485-6dc941daed80 --service=326f5e9d-19d0-4bdc-aeff-27cbf2545aa3
```

### Step 2: Check DATABASE_URL
```bash
echo $DATABASE_URL
```

**Expected:** Should show Railway PostgreSQL URL like:
```
postgresql://postgres:password@host.railway.app:5432/railway
```

### Step 3: Test Database Connection
```bash
# Option A: Using psql (if available)
psql $DATABASE_URL -c "SELECT version(), current_database(), current_user;"

# Option B: Using .NET application
cd /app
dotnet run -- TestDb
```

### Step 4: Run Full Test Script
```bash
# Copy and paste the test commands, or:
bash railway-test-commands.sh
```

---

## 📋 One-Line Test Commands

### Check DATABASE_URL:
```bash
railway ssh --project=04c6aa68-c21b-4185-87a1-51142de7a839 --environment=16a66d5c-eb10-4eca-8485-6dc941daed80 --service=326f5e9d-19d0-4bdc-aeff-27cbf2545aa3 "echo \$DATABASE_URL"
```

### Test PostgreSQL Connection:
```bash
railway ssh --project=04c6aa68-c21b-4185-87a1-51142de7a839 --environment=16a66d5c-eb10-4eca-8485-6dc941daed80 --service=326f5e9d-19d0-4bdc-aeff-27cbf2545aa3 "psql \$DATABASE_URL -c 'SELECT version(), current_database(), current_user;'"
```

### Run Application Test:
```bash
railway ssh --project=04c6aa68-c21b-4185-87a1-51142de7a839 --environment=16a66d5c-eb10-4eca-8485-6dc941daed80 --service=326f5e9d-19d0-4bdc-aeff-27cbf2545aa3 "cd /app && dotnet run -- TestDb"
```

---

## 🔍 What to Look For

### ✅ Success Indicators:
1. `DATABASE_URL` is set
2. Connection to PostgreSQL succeeds
3. Application logs show:
   ```
   [CONFIG] ✅ Converted Railway DATABASE_URL to connection string
   [CONFIG] ✅ Connection string format validated
   [CONFIG] ✅ Using database connection from: Environment Variable
   ```

### ❌ If DATABASE_URL is Missing:
1. Go to Railway Dashboard
2. Select your service
3. Go to Variables tab
4. Check if PostgreSQL service is linked
5. `DATABASE_URL` should be auto-set when PostgreSQL is linked

---

## 🛠️ Install Railway CLI (if needed)

**Windows:**
```powershell
# Using npm
npm i -g @railway/cli

# Or download from GitHub
# https://github.com/railwayapp/cli/releases
```

**Login:**
```bash
railway login
```

**Verify:**
```bash
railway whoami
```

---

## 📊 Expected Test Output

### ✅ DATABASE_URL Check:
```
postgresql://postgres:password@host.railway.app:5432/railway
```

### ✅ PostgreSQL Test:
```
PostgreSQL 15.x on x86_64-pc-linux-gnu
Database: railway
User: postgres
```

### ✅ Application Test:
```
[CONFIG] ✅ Converted Railway DATABASE_URL to connection string
[CONFIG] ✅ Connection string format validated
✅ Connection Test Successful!
   Database: railway
   User: postgres
```

---

**Ready to test!** Run the SSH command and test your Railway database connection! 🚂
