# 🚂 Railway Database Connection Setup

## ✅ Your Database Credentials

- **Username:** `postgres`
- **Password:** `VUykzDaybssURQkSAfxUYOBKBkDQSuVW`
- **Connection Method:** Railway Template Variable

---

## 🔧 Setup Steps

### Step 1: Add Variable to Your Service

1. Go to Railway Dashboard
2. Select your **application service** (not the database service)
3. Go to **Variables** tab
4. Click **+ New Variable**
5. Set:
   - **Variable Name:** `DATABASE_URL`
   - **Value:** `${{ Postgres.DATABASE_URL }}`
6. Click **Add**

**Important:** Use `${{ Postgres.DATABASE_URL }}` exactly as shown - Railway will auto-replace it with the actual connection string.

---

## ✅ What Happens Next

Once you set `DATABASE_URL = ${{ Postgres.DATABASE_URL }}`:

1. **Railway automatically replaces** `${{ Postgres.DATABASE_URL }}` with the real connection string
2. **Your application detects** `DATABASE_URL` environment variable
3. **Application auto-converts** Railway format to PostgreSQL connection string
4. **Database connects** automatically

**No code changes needed!** ✅ Your application already supports this.

---

## 🧪 Verify It Works

### Method 1: Check in Railway Dashboard

1. Go to your service → **Variables**
2. Verify `DATABASE_URL` is set
3. Check **Deployments** → Latest → **Logs**
4. Look for:
   ```
   [CONFIG] ✅ Converted Railway DATABASE_URL to connection string
   [CONFIG] ✅ Connection string format validated
   ```

### Method 2: Test via Railway SSH

```bash
railway ssh --project=04c6aa68-c21b-4185-87a1-51142de7a839 --environment=16a66d5c-eb10-4eca-8485-6dc941daed80 --service=326f5e9d-19d0-4bdc-aeff-27cbf2545aa3

# Once connected:
echo $DATABASE_URL
```

**Expected Output:**
```
postgresql://postgres:VUykzDaybssURQkSAfxUYOBKBkDQSuVW@host.railway.app:5432/railway
```

### Method 3: Test Database Connection

```bash
railway ssh --project=04c6aa68-c21b-4185-87a1-51142de7a839 --environment=16a66d5c-eb10-4eca-8485-6dc941daed80 --service=326f5e9d-19d0-4bdc-aeff-27cbf2545aa3 "psql \$DATABASE_URL -c 'SELECT version(), current_database(), current_user;'"
```

---

## 📋 Railway Template Variables

Railway provides these template variables for databases:

- `${{ Postgres.DATABASE_URL }}` - Full PostgreSQL connection URL
- `${{ Postgres.PGHOST }}` - Database host
- `${{ Postgres.PGPORT }}` - Database port
- `${{ Postgres.PGDATABASE }}` - Database name
- `${{ Postgres.PGUSER }}` - Database user
- `${{ Postgres.PGPASSWORD }}` - Database password

**For your application, use:** `${{ Postgres.DATABASE_URL }}`

---

## 🔍 How Your Application Handles This

Your application code (already implemented):

1. **Detects DATABASE_URL:**
   ```csharp
   var railwayUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
   ```

2. **Auto-converts format:**
   ```csharp
   // postgresql://user:pass@host:port/db
   // → Host=host;Database=db;Username=user;Password=pass;Port=port
   ```

3. **Validates and uses:**
   ```csharp
   // Validates format
   // Sets in IConfiguration
   // Connects to database
   ```

**Location:** `Shahin-Jan-2026/src/GrcMvc/Extensions/WebApplicationBuilderExtensions.cs` (lines 191-218)

---

## ✅ Setup Checklist

- [ ] Go to Railway Dashboard → Your Application Service
- [ ] Go to Variables tab
- [ ] Add new variable: `DATABASE_URL = ${{ Postgres.DATABASE_URL }}`
- [ ] Save variable
- [ ] Redeploy service (or Railway auto-redeploys)
- [ ] Check logs for connection success
- [ ] Verify application connects to database

---

## 🚀 After Setup

Once `DATABASE_URL` is set:

1. **Railway automatically:**
   - Replaces template variable with real connection string
   - Sets environment variable in your service
   - Makes it available to your application

2. **Your application automatically:**
   - Detects `DATABASE_URL`
   - Converts to PostgreSQL format
   - Connects to database
   - Logs success: `[CONFIG] ✅ Converted Railway DATABASE_URL`

3. **No manual configuration needed!** ✅

---

## 📊 Expected Logs

After deployment, you should see:

```
[CONFIG] ========================================
[CONFIG] Resolving Connection Strings
[CONFIG] ========================================
[CONFIG] 🔍 Checking environment variables:
[CONFIG]   ❌ ConnectionStrings__DefaultConnection = (not set)
[CONFIG]   ❌ CONNECTION_STRING = (not set)
[CONFIG] 🔍 Resolving connection string: DefaultConnection
[CONFIG] ✅ Converted Railway DATABASE_URL to connection string
[CONFIG] ✅ Connection string format validated
[CONFIG] ✅ Using database connection from: Environment Variable (Railway/Docker)
[CONFIG] 📊 Database: host.railway.app:5432 / postgres@railway
[CONFIG] ========================================
```

---

**Status:** ✅ **READY TO SETUP**

Just add `DATABASE_URL = ${{ Postgres.DATABASE_URL }}` to your service variables! 🚂
