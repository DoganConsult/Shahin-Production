# Database & Configuration - No Hardcoding Guide

**Status:** ✅ All hardcoded values removed  
**Last Updated:** January 4, 2026

---

## 🎯 Overview

The GRC system now uses **100% environment-driven configuration** with zero hardcoded values in the application code. All configuration comes from:

1. **`.env` file** - Local development
2. **Environment variables** - Docker containers
3. **Application configuration files** - Defaults only (empty/placeholder)

---

## 📋 Configuration Structure

### Hierarchy (Highest to Lowest Priority)

```
1. Environment Variables (Docker)        ← Override all
2. .env file                             ← Development defaults
3. appsettings.json                      ← Base structure
4. appsettings.Production.json           ← Production overrides
```

---

## 🔧 All Configuration Parameters

### Database Configuration

| Variable | Location | Default | Purpose |
|----------|----------|---------|---------|
| `CONNECTION_STRING` | `.env` / Docker | `Host=db;Database=GrcMvcDb;...` | Primary DB connection |
| `DB_USER` | `.env` / Docker | `postgres` | Database username |
| `DB_PASSWORD` | `.env` / Docker | `postgres` | Database password |
| `DB_NAME` | `.env` / Docker | `GrcMvcDb` | Database name |
| `DB_PORT` | `.env` / Docker | `5433` | Database port |

**Environment Variable Format (Docker):**
```yaml
- ConnectionStrings__DefaultConnection=${CONNECTION_STRING}
```

### JWT Configuration

| Variable | Location | Default | Purpose |
|----------|----------|---------|---------|
| `JWT_SECRET` | `.env` / Docker | `ProdSecretKeyMustBeVeryLong...` | Token signing key (min 32 chars) |
| `JWT_ISSUER` | `.env` / Docker | `https://portal.shahin-ai.com` | Token issuer |
| `JWT_AUDIENCE` | `.env` / Docker | `https://portal.shahin-ai.com` | Token audience |

**Environment Variable Format (Docker):**
```yaml
- JwtSettings__Secret=${JWT_SECRET}
- JwtSettings__Issuer=${JWT_ISSUER:-https://portal.shahin-ai.com}
- JwtSettings__Audience=${JWT_AUDIENCE:-https://portal.shahin-ai.com}
```

### Email Configuration

| Variable | Location | Default | Purpose |
|----------|----------|---------|---------|
| `SMTP_SERVER` | `.env` / Docker | `smtp.office365.com` | SMTP server hostname |
| `SMTP_PORT` | `.env` / Docker | `587` | SMTP port (TLS) |
| `SMTP_USERNAME` | `.env` / Docker | `support@shahin-ai.com` | SMTP authentication username |
| `SMTP_PASSWORD` | `.env` / Docker | `[encrypted]` | SMTP authentication password |
| `SMTP_FROM` | `.env` / Docker | `support@shahin-ai.com` | Email sender address |

**Environment Variable Format (Docker):**
```yaml
- EmailSettings__SmtpServer=${SMTP_SERVER}
- EmailSettings__SmtpPort=${SMTP_PORT}
- EmailSettings__Username=${SMTP_USERNAME}
- EmailSettings__Password=${SMTP_PASSWORD}
- EmailSettings__From=${SMTP_FROM}
```

### Application Configuration

| Variable | Location | Default | Purpose |
|----------|----------|---------|---------|
| `ASPNETCORE_ENVIRONMENT` | `.env` / Docker | `Development` | Environment (Development/Production) |
| `APP_PORT` | `.env` / Docker | `8888` | Application port |
| `APP_HTTPS_PORT` | `.env` / Docker | `8443` | HTTPS port |
| `ALLOWED_HOSTS` | `.env` / Docker | `localhost;portal.shahin-ai.com;157.180.105.48` | Allowed host names |

**Environment Variable Format (Docker):**
```yaml
- ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Development}
- AllowedHosts=${ALLOWED_HOSTS:-localhost;portal.shahin-ai.com;157.180.105.48}
```

---

## 📁 .env File Format

**Location:** `/home/dogan/grc-system/.env`

```bash
# Database Configuration
DB_USER=postgres
DB_PASSWORD=postgres
DB_NAME=GrcMvcDb
DB_PORT=5433
CONNECTION_STRING=Host=db;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5432

# JWT Configuration
JWT_SECRET=ProdSecretKeyMustBeVeryLongAndSecureForProductionUse!
JWT_ISSUER=https://portal.shahin-ai.com
JWT_AUDIENCE=https://portal.shahin-ai.com

# Email Configuration (Office 365)
SMTP_SERVER=smtp.office365.com
SMTP_PORT=587
SMTP_USERNAME=support@shahin-ai.com
SMTP_PASSWORD=[encrypted-password]
SMTP_FROM=support@shahin-ai.com

# Application Configuration
ASPNETCORE_ENVIRONMENT=Development
APP_PORT=8888
APP_HTTPS_PORT=8443
ALLOWED_HOSTS=localhost;portal.shahin-ai.com;157.180.105.48
```

---

## 🐳 Docker Compose Configuration

**Location:** `/home/dogan/grc-system/docker-compose.yml`

The docker-compose file now **reads all values from .env**:

```yaml
services:
  grcmvc:
    environment:
      - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Development}
      - ConnectionStrings__DefaultConnection=${CONNECTION_STRING}
      - JwtSettings__Secret=${JWT_SECRET}
      - JwtSettings__Issuer=${JWT_ISSUER:-https://portal.shahin-ai.com}
      - JwtSettings__Audience=${JWT_AUDIENCE:-https://portal.shahin-ai.com}
      - AllowedHosts=${ALLOWED_HOSTS:-localhost;portal.shahin-ai.com;157.180.105.48}
      - EmailSettings__SmtpServer=${SMTP_SERVER}
      - EmailSettings__SmtpPort=${SMTP_PORT}
      - EmailSettings__Username=${SMTP_USERNAME}
      - EmailSettings__Password=${SMTP_PASSWORD}
      - EmailSettings__From=${SMTP_FROM}

  db:
    environment:
      - POSTGRES_USER=${DB_USER:-postgres}
      - POSTGRES_PASSWORD=${DB_PASSWORD:-postgres}
      - POSTGRES_DB=${DB_NAME:-GrcMvcDb}
```

**Key Features:**
- ✅ All values come from `.env` or Docker environment
- ✅ Health checks enabled for both app and database
- ✅ Proper dependency management (`depends_on` with health check)
- ✅ Init script runs on first startup (`init-db.sql`)

---

## 🔗 How Configuration Flows

```
┌─────────────────────────┐
│   .env file             │
│ CONNECTION_STRING=...   │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────────┐
│ docker-compose.yml          │
│ ${CONNECTION_STRING}        │
└──────────┬──────────────────┘
           │
           ▼
┌─────────────────────────────┐
│ Container Environment       │
│ ConnectionStrings__Default  │
│ Connection=${CONNECTION...} │
└──────────┬──────────────────┘
           │
           ▼
┌─────────────────────────────┐
│ Program.cs                  │
│ GetConnectionString(...)    │
└──────────┬──────────────────┘
           │
           ▼
┌─────────────────────────────┐
│ Entity Framework DbContext  │
│ UseNpgsql(connectionString) │
└──────────┬──────────────────┘
           │
           ▼
┌─────────────────────────────┐
│ PostgreSQL Database         │
│ GrcMvcDb                    │
└─────────────────────────────┘
```

---

## 🗄️ Database Tables (All Managed by Entity Framework)

**No hardcoded table names** - all tables are defined in Entity Framework models and created via migrations.

### Models & Auto-Generated Tables

| Model | Table Name | Status |
|-------|-----------|--------|
| `ApplicationUser` | `AspNetUsers` | ✅ Auto-created |
| `IdentityRole` | `AspNetRoles` | ✅ Auto-created |
| `Tenant` | `Tenants` | ✅ Auto-created |
| `Risk` | `Risks` | ✅ Auto-created |
| `Control` | `Controls` | ✅ Auto-created |
| `Assessment` | `Assessments` | ✅ Auto-created |
| `AuditEvent` | `AuditEvents` | ✅ Auto-created |
| `SubscriptionPlan` | `SubscriptionPlans` | ✅ Auto-created |
| `Subscription` | `Subscriptions` | ✅ Auto-created |
| `Payment` | `Payments` | ✅ Auto-created |
| `Invoice` | `Invoices` | ✅ Auto-created |

### How Tables Are Created

1. **Entity Framework Models** define table structure
2. **Migrations** define how to create/update tables
3. **Program.cs** automatically runs migrations on startup:
   ```csharp
   await context.Database.MigrateAsync();
   ```
4. **Database is fully initialized** without any manual SQL

---

## ✅ Verification Checklist

### Configuration Files

- [ ] `.env` file exists with all variables
- [ ] `appsettings.json` has **no sensitive data** (only empty/placeholder values)
- [ ] `docker-compose.yml` uses `${VARIABLE}` syntax
- [ ] No hardcoded passwords/secrets in code

### Database Configuration

- [ ] `CONNECTION_STRING` in `.env` points to `db` container
- [ ] `DB_USER`, `DB_PASSWORD` match in `.env`
- [ ] `DB_NAME` matches in both `.env` and `CONNECTION_STRING`
- [ ] Database port configured in `.env` (`DB_PORT`)

### Running Application

```bash
cd /home/dogan/grc-system

# 1. Verify .env file exists
cat .env

# 2. Start containers
docker compose up -d

# 3. Check database connection
docker compose exec grcmvc curl -s http://localhost/health | grep database

# 4. Verify all tables exist
docker compose exec db psql -U postgres -d GrcMvcDb -c "\dt"

# 5. Verify configuration is loaded
docker compose logs grcmvc | grep "Database connection"
```

---

## 🔄 Updating Configuration

### For Development (Local)

1. Edit `.env` file
2. Rebuild: `docker compose down && docker compose up -d`

### For Production

1. Set environment variables on the host:
   ```bash
   export CONNECTION_STRING="Host=prod-db.example.com;..."
   export JWT_SECRET="[very-long-secure-key]"
   export SMTP_PASSWORD="[encrypted-password]"
   ```

2. Run containers:
   ```bash
   docker compose up -d
   ```

3. Or use `.env.production` file:
   ```bash
   docker compose --env-file .env.production up -d
   ```

---

## 🛡️ Security Best Practices

✅ **Never commit sensitive values to git:**
```bash
echo ".env" >> .gitignore
echo ".env.production" >> .gitignore
echo "appsettings.*.json" >> .gitignore
```

✅ **Use environment variables for secrets:**
```bash
export SMTP_PASSWORD="$(aws secretsmanager get-secret-value --secret-id smtp-password --query SecretString --output text)"
```

✅ **Rotate credentials regularly:**
```bash
# Update in .env and restart
docker compose restart grcmvc
```

✅ **Different credentials per environment:**
- `.env` - Development (weak credentials)
- `.env.production` - Production (strong credentials)
- Secrets manager - CI/CD (encrypted credentials)

---

## 📊 Application Flow with No Hardcoding

```
User Request
    ↓
Middleware reads env variables
    ↓
Program.cs configures DbContext with CONNECTION_STRING
    ↓
Entity Framework connects to PostgreSQL
    ↓
Migrations applied (auto-creates tables)
    ↓
Application ready
    ↓
Data persisted in actual database tables
```

---

## 🚀 Deployment Checklist

- [ ] All hardcoded values removed from source code
- [ ] `.env` file created with all required variables
- [ ] `docker-compose.yml` uses environment variable substitution
- [ ] `appsettings.json` contains only structure (empty/default values)
- [ ] Database migrations created and working
- [ ] Application connects to actual PostgreSQL database
- [ ] All tables created in database
- [ ] Health check passes
- [ ] Emails can be sent (SMTP configured)
- [ ] JWT tokens generated correctly
- [ ] User authentication works
- [ ] Subscription system working
- [ ] Data persists across container restarts

---

## 🎓 Key Takeaway

**All configuration is environment-driven:**

```
Application Code (NO SECRETS)
         ↑
    reads from
         ↑
    Environment Variables (SECRETS)
         ↑
    provided by
         ↑
    .env or Docker environment
```

This ensures:
- ✅ No secrets in version control
- ✅ Easy to change between environments
- ✅ Secure credentials management
- ✅ No hardcoding needed

---

## 📞 Support

For configuration issues:

```bash
# Check all environment variables in container
docker compose exec grcmvc env | grep -E "Connection|Jwt|Email|App"

# Check database connection
docker compose exec grcmvc curl -s http://localhost/health

# Check application logs
docker compose logs grcmvc | tail -50

# Check database logs
docker compose logs db | tail -30
```
