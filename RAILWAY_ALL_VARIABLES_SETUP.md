# 🚂 Add All Configuration Variables to Railway

## 📋 Complete Variable List

### ✅ Already Configured
- **DATABASE_URL** = `${{ Postgres.DATABASE_URL }}` ✅ (Set via Railway template)

---

## 🔴 CRITICAL (Required for Application to Start)

### 1. JWT Secret
```bash
railway variables set JWT_SECRET="YOUR_SECRET_HERE" --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a
```
**Generate Secret:**
```bash
openssl rand -base64 64
```
**Or use PowerShell:**
```powershell
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object {[char]$_})
```

### 2. Application Environment
```bash
railway variables set ASPNETCORE_ENVIRONMENT="Production" --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a
```

### 3. Application URLs
```bash
railway variables set ASPNETCORE_URLS="http://0.0.0.0:5000" --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a
```

---

## 🟡 IMPORTANT (Recommended)

### 4. JWT Settings
```bash
railway variables set JwtSettings__Issuer="https://portal.shahin-ai.com" --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a

railway variables set JwtSettings__Audience="https://portal.shahin-ai.com" --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a
```

---

## 🟢 OPTIONAL (If Using Features)

### 5. Claude AI (if using AI features)
```bash
railway variables set CLAUDE_API_KEY="sk-ant-api03-YOUR_KEY" --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a

railway variables set CLAUDE_ENABLED="true" --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a

railway variables set CLAUDE_MODEL="claude-sonnet-4-20250514" --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a
```

### 6. SMTP Email (if using email)
```bash
railway variables set SMTP_FROM_EMAIL="info@shahin-ai.com" --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a

railway variables set SMTP_CLIENT_ID="YOUR_CLIENT_ID" --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a

railway variables set SMTP_CLIENT_SECRET="YOUR_CLIENT_SECRET" --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a
```

### 7. Azure/Microsoft Graph (if using SSO)
```bash
railway variables set AZURE_TENANT_ID="YOUR_TENANT_ID" --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a

railway variables set MSGRAPH_CLIENT_ID="YOUR_CLIENT_ID" --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a

railway variables set MSGRAPH_CLIENT_SECRET="YOUR_CLIENT_SECRET" --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a
```

---

## 🚀 Quick Setup Script

Run this PowerShell script to see all commands:
```powershell
.\add-railway-variables.ps1
```

---

## 📋 Setup via Railway Dashboard

### Step 1: Go to Railway Dashboard
1. Open Railway Dashboard
2. Select your project
3. Select your service: `0cb7da15-a249-4cba-a197-677e800c306a`
4. Go to **Variables** tab

### Step 2: Add Required Variables

| Variable Name | Value | Required |
|--------------|-------|----------|
| `DATABASE_URL` | `${{ Postgres.DATABASE_URL }}` | ✅ Yes (already set) |
| `JWT_SECRET` | `[Generate 64+ char secret]` | ✅ Yes |
| `ASPNETCORE_ENVIRONMENT` | `Production` | ✅ Yes |
| `ASPNETCORE_URLS` | `http://0.0.0.0:5000` | ✅ Yes |
| `JwtSettings__Issuer` | `https://portal.shahin-ai.com` | ⚠️ Recommended |
| `JwtSettings__Audience` | `https://portal.shahin-ai.com` | ⚠️ Recommended |

### Step 3: Add Optional Variables (if needed)

| Variable Name | Value | When Needed |
|--------------|-------|-------------|
| `CLAUDE_API_KEY` | `sk-ant-api03-...` | If using AI features |
| `CLAUDE_ENABLED` | `true` or `false` | If using AI features |
| `SMTP_FROM_EMAIL` | `info@shahin-ai.com` | If sending emails |
| `AZURE_TENANT_ID` | `...` | If using Azure SSO |

---

## ✅ Verification

After adding variables, verify:

```bash
railway ssh --project=402d90cb-9706-4b98-ae24-0f2e992c624c --environment=03604398-8431-4c35-8fce-e230c4c8d585 --service=0cb7da15-a249-4cba-a197-677e800c306a "printenv | grep -E 'DATABASE_URL|JWT_SECRET|ASPNETCORE'"
```

---

## 🎯 Minimum Required Variables

For the application to start, you need at minimum:

1. ✅ `DATABASE_URL` = `${{ Postgres.DATABASE_URL }}` (already set)
2. ✅ `JWT_SECRET` = [64+ character secret]
3. ✅ `ASPNETCORE_ENVIRONMENT` = `Production`
4. ✅ `ASPNETCORE_URLS` = `http://0.0.0.0:5000`

**Everything else is optional!**

---

**Ready to add variables!** Run the script or use Railway Dashboard! 🚂
