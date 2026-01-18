pilit# 🔗 Railway Shared Variables Guide

## Understanding Railway's Shared Variables

Railway provides **shared variables** that automatically connect services within your project. These are special template variables that Railway replaces with actual connection strings.

---

## 📋 Postgres Shared Variables

### Available Postgres Variables

Railway provides these Postgres-related variables:

1. **`${{Postgres.DATABASE_URL}}`** ⭐ **USE THIS**
   - Full PostgreSQL connection string
   - Format: `postgresql://user:password@host:port/database`
   - **This is what you need for your application**

2. **`${{Postgres.PGHOST}}`**
   - Database host only
   - Example: `postgres.railway.internal`

3. **`${{Postgres.PGPORT}}`**
   - Database port only
   - Example: `5432`

4. **`${{Postgres.PGDATABASE}}`**
   - Database name only
   - Example: `railway`

5. **`${{Postgres.PGUSER}}`**
   - Database username only
   - Example: `postgres`

6. **`${{Postgres.PGPASSWORD}}`**
   - Database password only
   - Example: `VUykzDaybssURQkSAfxUYOBKBkDQSuVW`

### ✅ For Your Application: Use DATABASE_URL

```json
{
  "DATABASE_URL": "${{Postgres.DATABASE_URL}}"
}
```

**Why?**
- Your application expects a full connection string
- Railway automatically converts this to the correct format
- Your code already handles this in `WebApplicationBuilderExtensions.cs`

---

## 📋 Redis Shared Variables

### Available Redis Variables

Railway provides these Redis-related variables:

1. **`${{Redis.REDIS_URL}}`** ⭐ **USE THIS**
   - Full Redis connection string
   - Format: `redis://default:password@host:port`
   - **This is what you need for your application**

2. **`${{Redis.REDIS_HOST}}`**
   - Redis host only
   - Example: `redis.railway.internal`

3. **`${{Redis.REDIS_PORT}}`**
   - Redis port only
   - Example: `6379`

4. **`${{Redis.REDIS_PASSWORD}}`**
   - Redis password only

### ✅ For Your Application: Use REDIS_URL

```json
{
  "Redis__ConnectionString": "${{Redis.REDIS_URL}}"
}
```

**Why?**
- Your application expects a full Redis connection string
- Railway automatically provides the correct format
- Your code already handles this configuration

---

## 🎯 Complete Environment Variables (Correct Format)

### Use These Exact Values:

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

---

## ⚠️ Important Notes

### 1. Template Variables (DO NOT Change)

These are **template variables** - Railway replaces them automatically:

```
${{Postgres.DATABASE_URL}}  → Railway replaces with actual connection string
${{Redis.REDIS_URL}}        → Railway replaces with actual Redis URL
```

**DO NOT:**
- Replace with actual values
- Remove the `${{` or `}}`
- Change the service names (Postgres, Redis)

**Railway will automatically:**
- Detect these template variables
- Replace them with actual connection strings
- Update them if services change

### 2. How Railway Processes These

When you set:
```
DATABASE_URL = ${{Postgres.DATABASE_URL}}
```

Railway internally converts it to:
```
DATABASE_URL = postgresql://postgres:VUykzDaybssURQkSAfxUYOBKBkDQSuVW@postgres.railway.internal:5432/railway
```

Your application receives the actual connection string, not the template!

---

## 🔍 Verification

### Check Variables in Railway Dashboard

After adding variables, verify in Railway Dashboard:

1. Go to your service → Variables tab
2. You should see:
   ```
   DATABASE_URL = ${{Postgres.DATABASE_URL}}
   Redis__ConnectionString = ${{Redis.REDIS_URL}}
   ```

3. Railway shows the template, but injects the actual value at runtime

### Check at Runtime

Once deployed, you can verify the actual values:

```powershell
railway ssh

# Check DATABASE_URL (actual value)
echo $DATABASE_URL

# Check Redis connection (actual value)
echo $REDIS_URL
```

You'll see the actual connection strings, not the templates!

---

## 📊 Variable Mapping

### How Your Application Uses These

| Railway Variable | Your App Variable | Purpose |
|-----------------|-------------------|---------|
| `${{Postgres.DATABASE_URL}}` | `DATABASE_URL` | Main database connection |
| `${{Redis.REDIS_URL}}` | `Redis__ConnectionString` | Redis cache connection |
| N/A | `ASPNETCORE_ENVIRONMENT` | Environment setting |
| N/A | `ASPNETCORE_URLS` | Port binding |
| N/A | `JWT_SECRET` | Authentication secret |
| N/A | `JwtSettings__Issuer` | JWT issuer |
| N/A | `JwtSettings__Audience` | JWT audience |
| N/A | `Redis__Enabled` | Enable Redis |

---

## ✅ Final Configuration

### Copy This to Railway Dashboard:

**Variable 1:**
```
Name: DATABASE_URL
Value: ${{Postgres.DATABASE_URL}}
```

**Variable 2:**
```
Name: ASPNETCORE_ENVIRONMENT
Value: Production
```

**Variable 3:**
```
Name: ASPNETCORE_URLS
Value: http://0.0.0.0:5000
```

**Variable 4:**
```
Name: JWT_SECRET
Value: etETf%Z9jqm-AiH_YlIBoudRU^bv+rK?c4XGQs#nh5pOJ*1!y2PC7F.@W0&w$Lkx
```

**Variable 5:**
```
Name: JwtSettings__Issuer
Value: https://portal.shahin-ai.com
```

**Variable 6:**
```
Name: JwtSettings__Audience
Value: https://portal.shahin-ai.com
```

**Variable 7:**
```
Name: Redis__ConnectionString
Value: ${{Redis.REDIS_URL}}
```

**Variable 8:**
```
Name: Redis__Enabled
Value: true
```

---

## 🎯 Summary

**For Postgres:** Use `${{Postgres.DATABASE_URL}}`
- This is the shared variable that Railway provides
- It contains the full connection string
- Railway automatically replaces it with the actual value

**For Redis:** Use `${{Redis.REDIS_URL}}`
- This is the shared variable that Railway provides
- It contains the full Redis connection string
- Railway automatically replaces it with the actual value

**These are NOT placeholders you need to replace** - Railway does it automatically! ✅

---

## 🚀 Ready to Deploy

Once you add these 8 variables to Railway Dashboard:
1. Railway will inject the actual connection strings
2. Your application will connect to Postgres and Redis automatically
3. Migrations will run and create all 321 tables
4. Application will be live!

**No additional configuration needed!** 🎉
