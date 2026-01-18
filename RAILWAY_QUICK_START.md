# 🚂 Railway Quick Start Guide

## ✅ Current Status

- **Railway CLI:** ✅ Installed (v4.25.2)
- **Database Connection:** ✅ Tested and working
- **Application Support:** ✅ Ready for Railway DATABASE_URL

---

## 🔐 Step 1: Login to Railway

```bash
railway login
```

This will open your browser to authenticate.

---

## 🔗 Step 2: Connect to Your Service

```bash
railway ssh --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a
```

---

## 🧪 Step 3: Test Database Connection

Once connected via SSH:

```bash
# Check DATABASE_URL
echo $DATABASE_URL

# Test PostgreSQL connection
psql $DATABASE_URL -c "SELECT version(), current_database(), current_user;"

# Or test with application
cd /app
dotnet run -- TestDb
```

---

## ⚙️ Step 4: Set DATABASE_URL Variable

In Railway Dashboard:
1. Go to your **application service**
2. **Variables** tab
3. Add:
   - **Name:** `DATABASE_URL`
   - **Value:** `${{ Postgres.DATABASE_URL }}`
4. Save

---

## 📊 Your Database Info

**Connection URL:**
```
postgresql://postgres:VUykzDaybssURQkSAfxUYOBKBkDQSuVW@centerbeam.proxy.rlwy.net:11539/railway
```

**Direct Connection:**
```bash
PGPASSWORD=VUykzDaybssURQkSAfxUYOBKBkDQSuVW psql -h centerbeam.proxy.rlwy.net -U postgres -p 11539 -d railway
```

---

## ✅ What Happens After Setup

1. Railway sets `DATABASE_URL` automatically
2. Your application detects it
3. Application converts Railway format to PostgreSQL
4. Database connects automatically
5. Logs show: `[CONFIG] ✅ Converted Railway DATABASE_URL`

**No code changes needed!** ✅

---

**Next:** Run `railway login` to authenticate! 🚂
