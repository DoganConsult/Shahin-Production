# Environment Files Scan Report

**Date:** 2026-01-20
**Scope:** All environment, configuration, and secret files

---

## CRITICAL: SECRETS EXPOSED IN 8 FILES!

### Summary of Exposed Credentials

| File | Credential Type | Value (Partial) |
|------|-----------------|-----------------|
| `railway-jwt-secret.txt` | JWT Secret | `etETf%Z9jqm-AiH...` |
| `railway-variables.json` | JWT Secret | Same as above |
| `RAILWAY_ENVIRONMENT_VARIABLES.txt` | JWT Secret | Same as above (3x) |
| `.claude/settings.local.json` | DB Password #1 | `QNcTvViWopMfCuns...` |
| `.claude/settings.local.json` | Cloudflare Token | `eyJhIjoiNjZiNTFh...` |
| `.claude/settings.local.json` | User Email | `elgazzar082@gmail.com` |
| `DatabaseExplorer.cs` | DB Password #2 | `VUykzDaybssURQkS...` |
| Multiple .ps1 files | DB Password #2 | Same as above |

---

## DETAILED FILE ANALYSIS

### 1. railway-jwt-secret.txt

**Location:** `/home/user/Shahin-Production/railway-jwt-secret.txt`
**Size:** 65 bytes
**Content:**
```
etETf%Z9jqm-AiH_YlIBoudRU^bv+rK?c4XGQs#nh5pOJ*1!y2PC7F.@W0&w$Lkx
```

**Risk:** CRITICAL - JWT signing key in plaintext file
**Action:** DELETE THIS FILE

---

### 2. railway-variables.json

**Location:** `/home/user/Shahin-Production/railway-variables.json`
**Lines:** 10

**Content:**
```json
{
  "DATABASE_URL": "${{Postgres.DATABASE_URL}}",
  "ASPNETCORE_ENVIRONMENT": "Production",
  "ASPNETCORE_URLS": "http://0.0.0.0:5000",
  "JWT_SECRET": "etETf%Z9jqm-AiH_YlIBoudRU^bv+rK?c4XGQs#nh5pOJ*1!y2PC7F.@W0&w$Lkx",
  "JwtSettings__Issuer": "https://portal.shahin-ai.com",
  "JwtSettings__Audience": "https://portal.shahin-ai.com",
  "Redis__ConnectionString": "${{Redis.REDIS_URL}}",
  "Redis__Enabled": "true"
}
```

**Issues Found:**
| Line | Issue |
|------|-------|
| 5 | JWT_SECRET in plaintext |

**Action:** Remove JWT_SECRET, use Railway secrets instead

---

### 3. RAILWAY_ENVIRONMENT_VARIABLES.txt

**Location:** `/home/user/Shahin-Production/RAILWAY_ENVIRONMENT_VARIABLES.txt`
**Lines:** 163

**Exposed Secrets:**
- Line 51: `JWT_SECRET=etETf%Z9jqm-AiH_YlIBoudRU^bv+rK?...`
- Line 112: Same JWT secret repeated
- Contains instructions mentioning `railway-jwt-secret.txt`

**Risk:** CRITICAL - Documentation file with production secrets
**Action:** DELETE or remove secrets

---

### 4. .claude/settings.local.json

**Location:** `/home/user/Shahin-Production/.claude/settings.local.json`
**Lines:** 120

**Exposed Secrets:**

#### 4.1 Cloudflare Tunnel Token (Line 15)
```
eyJhIjoiNjZiNTFhYzk2OTkxMWQ0MzY0ZjQ4M2Q4ODdhNjZjMGYiLCJ0IjoiZTliOTFhYWUtZDNkYS00MjMwLWE1YjctZTk3MDAzMjExNGFmIiwicyI6IlpqSmxNVEZtTWpRdE9XTmlaaTAwWkRnMExXSm1aVGN0TkdNM1pqQmxPVFJrWkRReiJ9
```

#### 4.2 Database Credentials (Line 113)
```python
DB_CONFIG = {
    'host': 'caboose.proxy.rlwy.net',
    'port': 11527,
    'database': 'GrcMvcDb',
    'user': 'postgres',
    'password': 'QNcTvViWopMfCunsyIkkXwuDpufzhkLs',
    'sslmode': 'require'
}
```

#### 4.3 User Email (Line 113)
```
elgazzar082@gmail.com
```

**Risk:** CRITICAL - Multiple secrets in Claude permissions file
**Action:** Remove all secrets from this file

---

## ALL DATABASES FOUND

| # | Host | Port | Database | Password |
|---|------|------|----------|----------|
| 1 | centerbeam.proxy.rlwy.net | 11539 | railway | `VUykzDaybssURQkS...` |
| 2 | caboose.proxy.rlwy.net | 11527 | GrcMvcDb | `QNcTvViWopMfCuns...` |

**Note:** Two different Railway databases with different credentials!

---

## ALL JWT SECRETS FOUND

| Location | Secret |
|----------|--------|
| railway-jwt-secret.txt | `etETf%Z9jqm-AiH_YlIBoudRU^bv+rK?c4XGQs#nh5pOJ*1!y2PC7F.@W0&w$Lkx` |
| railway-variables.json:5 | Same |
| RAILWAY_ENVIRONMENT_VARIABLES.txt:51 | Same |
| RAILWAY_ENVIRONMENT_VARIABLES.txt:112 | Same |

---

## ALL TOKENS FOUND

| Type | Location | Value (partial) |
|------|----------|-----------------|
| Cloudflare Tunnel | .claude/settings.local.json:15 | `eyJhIjoiNjZiNTFh...` |

---

## MISSING ENVIRONMENT FILES

The following files do NOT exist (which is good practice):
- `.env` - Not found
- `.env.local` - Not found
- `.env.production` - Not found
- `appsettings.json` - Not found (backend not in repo)
- `appsettings.Development.json` - Not found
- `appsettings.Production.json` - Not found

---

## ENVIRONMENT VARIABLES EXPECTED (from code)

### Frontend (landing-page)

| Variable | Default | Used In |
|----------|---------|---------|
| `NEXT_PUBLIC_API_URL` | `localhost:5137` or `localhost:5000` | api.ts, Navigation.tsx |

### Backend (from railway-variables.json)

| Variable | Status |
|----------|--------|
| `DATABASE_URL` | Template reference |
| `ASPNETCORE_ENVIRONMENT` | Production |
| `ASPNETCORE_URLS` | http://0.0.0.0:5000 |
| `JWT_SECRET` | EXPOSED! |
| `JwtSettings__Issuer` | https://portal.shahin-ai.com |
| `JwtSettings__Audience` | https://portal.shahin-ai.com |
| `Redis__ConnectionString` | Template reference |
| `Redis__Enabled` | true |

---

## SECURITY SUMMARY

### Files to DELETE Immediately
1. `railway-jwt-secret.txt` - Contains only JWT secret
2. `RAILWAY_ENVIRONMENT_VARIABLES.txt` - Contains JWT secret

### Files to CLEAN (remove secrets)
1. `railway-variables.json` - Remove JWT_SECRET line
2. `.claude/settings.local.json` - Remove DB credentials and tokens
3. `DatabaseExplorer.cs` - Remove hardcoded connection string
4. `explore-db-dotnet.ps1` - Remove hardcoded credentials
5. `explore-db.ps1` - Remove hardcoded credentials
6. `quick-db-explorer.ps1` - Remove hardcoded credentials

### Credentials to ROTATE
1. JWT Secret: `etETf%Z9jqm-AiH...`
2. DB Password 1: `VUykzDaybssURQkS...`
3. DB Password 2: `QNcTvViWopMfCuns...`
4. Cloudflare Token: `eyJhIjoiNjZiNTFh...`

---

## RECOMMENDED FIXES

### 1. Use Environment Variables
```bash
# Set in Railway Dashboard, NOT in code
JWT_SECRET=<new-rotated-secret>
DATABASE_URL=<from-railway>
```

### 2. Use .env.example (template only)
```bash
# .env.example - NO REAL VALUES
JWT_SECRET=your-jwt-secret-here
DATABASE_URL=postgresql://user:password@host:port/database
```

### 3. Add to .gitignore
```
.env
.env.local
.env.production
*.secret
*-secret.txt
```

---

## TOTAL EXPOSED CREDENTIALS

| Category | Count |
|----------|-------|
| JWT Secrets | 1 (in 4 files) |
| Database Passwords | 2 |
| Cloudflare Tokens | 1 |
| User Emails | 1 |
| **Total Unique Secrets** | **5** |
| **Total Files with Secrets** | **8** |

---

**Scan completed:** 2026-01-20
**Action Required:** IMMEDIATE credential rotation
