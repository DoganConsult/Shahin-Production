# 🚀 Quick Commands to Add All Variables to Railway

## Step 1: Generate JWT Secret (if not done)

```powershell
# PowerShell
$jwtSecret = -join ((48..57) + (65..90) + (97..122) + (33,35,36,37,38,42,43,45,46,61,63,64,94,95) | Get-Random -Count 64 | ForEach-Object {[char]$_})
Write-Host $jwtSecret
```

Or use the generated secret from `railway-jwt-secret.txt`

---

## Step 2: Add All Required Variables

### Option A: Using Railway CLI (Recommended)

```bash
# 1. Database (already configured via template)
railway variables set DATABASE_URL="${{ Postgres.DATABASE_URL }}" --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a

# 2. Application Environment
railway variables set ASPNETCORE_ENVIRONMENT="Production" --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a

# 3. Application URLs
railway variables set ASPNETCORE_URLS="http://0.0.0.0:5000" --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a

# 4. JWT Secret (REPLACE WITH YOUR GENERATED SECRET)
railway variables set JWT_SECRET="YOUR_GENERATED_SECRET_HERE" --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a

# 5. JWT Settings (Recommended)
railway variables set JwtSettings__Issuer="https://portal.shahin-ai.com" --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a

railway variables set JwtSettings__Audience="https://portal.shahin-ai.com" --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a
```

### Option B: Using Railway Dashboard

1. Go to Railway Dashboard
2. Select your service
3. Go to **Variables** tab
4. Add each variable:

| Variable | Value |
|----------|-------|
| `DATABASE_URL` | `${{ Postgres.DATABASE_URL }}` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_URLS` | `http://0.0.0.0:5000` |
| `JWT_SECRET` | `[Your generated secret]` |
| `JwtSettings__Issuer` | `https://portal.shahin-ai.com` |
| `JwtSettings__Audience` | `https://portal.shahin-ai.com` |

---

## Step 3: Verify Variables

```bash
railway ssh --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a "printenv | grep -E 'DATABASE_URL|JWT_SECRET|ASPNETCORE'"
```

---

## ✅ Minimum Required Variables

For the application to start:

1. ✅ `DATABASE_URL` = `${{ Postgres.DATABASE_URL }}` (already set)
2. ✅ `JWT_SECRET` = [64+ character secret]
3. ✅ `ASPNETCORE_ENVIRONMENT` = `Production`
4. ✅ `ASPNETCORE_URLS` = `http://0.0.0.0:5000`

**That's it!** Everything else is optional.

---

## 📋 Complete Variable List

See `RAILWAY_ALL_VARIABLES_SETUP.md` for the complete list including optional variables.

---

**Ready to add!** Use the commands above or Railway Dashboard! 🚂
