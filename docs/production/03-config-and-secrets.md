# Configuration & Secrets Inventory

**Status:** ✅ READY  
**Last Updated:** 2026-01-12  
**Purpose:** Definitive reference for all environment variables and configuration settings required for production deployment.

---

## Overview

This document catalogs all environment variables, connection strings, and configuration settings used across:
- **Backend (ASP.NET Core)**: GrcMvc application
- **Frontend (Next.js)**: shahin-ai-website
- **Database (PostgreSQL)**: Main database + Auth database
- **Infrastructure**: Docker Compose, Redis, external services

**Security Note:** This document lists **variable names only**. Actual secret values must be stored in `.env.production` (not committed to git) or a secrets management system.

---

## Configuration Table

| Variable Name | Required | Service | Purpose | Example Format | Where Used |
|--------------|----------|---------|---------|----------------|------------|
| **BACKEND - Core Application** |
| `ASPNETCORE_ENVIRONMENT` | ✅ Yes | api | Runtime environment | `Production` | `Program.cs`, `appsettings.Production.json` |
| `ASPNETCORE_URLS` | ✅ Yes | api | Kestrel listening URLs | `http://+:8080` | `docker-compose.production.yml`, `Program.cs`, `appsettings.Production.json` |
| `AllowedHosts` | ✅ Yes | api | Allowed host headers | `app.shahin-ai.com,portal.shahin-ai.com` | `appsettings.json`, `docker-compose.production.yml` |
| **BACKEND - Database Connections** |
| `DB_HOST` | ✅ Yes | api | PostgreSQL hostname | `db-prod` (Docker) or `localhost` (external) | `docker-compose.production.yml`, `Program.cs` |
| `DB_PORT` | ✅ Yes | api | PostgreSQL port | `5432` | `docker-compose.production.yml` |
| `DB_NAME` | ✅ Yes | api | Main database name | `GrcMvcDb` | `docker-compose.production.yml`, `scripts/init-db.sql` |
| `DB_USER` | ✅ Yes | api | PostgreSQL username | `grc_admin` | `docker-compose.production.yml`, `scripts/init-db.sql` |
| `DB_PASSWORD` | ✅ Yes | api | PostgreSQL password | `[SECRET]` | `docker-compose.production.yml`, `scripts/init-db.sql` |
| `ConnectionStrings__DefaultConnection` | ✅ Yes | api | EF Core main DB connection | `Host=db-prod;Database=GrcMvcDb;Username=grc_admin;Password=***;Port=5432` | `appsettings.json`, `Program.cs`, `GrcDbContext.cs` |
| `ConnectionStrings__GrcAuthDb` | ✅ Yes | api | EF Core auth DB connection | `Host=db-prod;Database=GrcMvcDb_auth;Username=grc_admin;Password=***;Port=5432` | `appsettings.json`, `Program.cs`, `GrcAuthDbContext.cs` |
| `ConnectionStrings__HangfireConnection` | ✅ Yes | api | Hangfire job storage | `Host=db-prod;Database=GrcMvcDb;Username=grc_admin;Password=***;Port=5432` | `appsettings.json`, `Program.cs` |
| **BACKEND - Authentication & JWT** |
| `JWT_SECRET` | ✅ Yes | api | JWT signing key (base64 or hex) | `[SECRET - min 32 bytes]` | `appsettings.json`, `Program.cs`, `JwtSettings` |
| `JwtSettings__Secret` | ✅ Yes | api | Alternative JWT secret key | `[SECRET]` | `appsettings.Production.json`, `docker-compose.production.yml` |
| `JWT_ISSUER` | ⚠️ Optional | api | JWT issuer claim | `shahin-grc` | `docker-compose.production.yml` (default: `shahin-grc`) |
| `JwtSettings__Issuer` | ⚠️ Optional | api | Alternative issuer setting | `ShahinAI` | `appsettings.Production.json` |
| `JWT_AUDIENCE` | ⚠️ Optional | api | JWT audience claim | `shahin-grc-api` | `docker-compose.production.yml` (default: `shahin-grc-api`) |
| `JwtSettings__Audience` | ⚠️ Optional | api | Alternative audience setting | `ShahinAIUsers` | `appsettings.Production.json` |
| **BACKEND - Redis Cache** |
| `REDIS_ENABLED` | ⚠️ Optional | api | Enable Redis caching | `true` | `docker-compose.production.yml`, `Program.cs` |
| `REDIS_CONNECTION_STRING` | ✅ Yes (if Redis enabled) | api | Redis connection string | `redis-prod:6379` | `docker-compose.production.yml`, `Program.cs` |
| `ConnectionStrings__Redis` | ✅ Yes (if Redis enabled) | api | Alternative Redis connection | `redis-prod:6379` | `appsettings.json`, `Program.cs` |
| **BACKEND - SMTP / Email** |
| `SMTP_HOST` | ⚠️ Optional | api | SMTP server hostname | `smtp.office365.com` | `Program.cs`, `appsettings.json` |
| `SMTP_PORT` | ⚠️ Optional | api | SMTP server port | `587` | `appsettings.json` |
| `SMTP_FROM_EMAIL` | ⚠️ Optional | api | Default sender email | `info@shahin-ai.com` | `Program.cs`, `appsettings.json` |
| `SMTP_USERNAME` | ⚠️ Optional | api | SMTP authentication username | `info@shahin-ai.com` | `Program.cs`, `appsettings.json` |
| `SMTP_PASSWORD` | ⚠️ Optional | api | SMTP authentication password | `[SECRET]` | `Program.cs`, `appsettings.json` |
| `SMTP_CLIENT_ID` | ⚠️ Optional | api | OAuth2 client ID (if UseOAuth2=true) | `[SECRET]` | `Program.cs`, `appsettings.Production.json` |
| `SMTP_CLIENT_SECRET` | ⚠️ Optional | api | OAuth2 client secret (if UseOAuth2=true) | `[SECRET]` | `Program.cs`, `appsettings.Production.json` |
| **BACKEND - Azure AD / Microsoft Graph** |
| `AZURE_TENANT_ID` | ⚠️ Optional | api | Azure AD tenant ID for SSO/Graph | `[UUID]` | `Program.cs`, `appsettings.Production.json`, `EmailOperations`, `CopilotAgent` |
| **BACKEND - Claude AI** |
| `CLAUDE_ENABLED` | ⚠️ Optional | api | Enable Claude AI agents | `false` | `docker-compose.production.yml`, `Program.cs` |
| `CLAUDE_API_KEY` | ✅ Yes (if Claude enabled) | api | Anthropic Claude API key | `[SECRET]` | `Program.cs`, `docker-compose.production.yml`, `ClaudeAgents` |
| `ClaudeAgents__ApiKey` | ✅ Yes (if Claude enabled) | api | Alternative Claude API key | `[SECRET]` | `appsettings.Production.json` |
| `ClaudeAgents__Enabled` | ⚠️ Optional | api | Alternative Claude enabled flag | `true` | `appsettings.Production.json` |
| `CLAUDE_MODEL` | ⚠️ Optional | api | Claude model identifier | `claude-sonnet-4-20250514` | `Program.cs` (default from `appsettings.json`) |
| `CLAUDE_MAX_TOKENS` | ⚠️ Optional | api | Max tokens per request | `4096` | `Program.cs` (default from `appsettings.json`) |
| **BACKEND - Kafka** |
| `KAFKA_BOOTSTRAP_SERVERS` | ⚠️ Optional | api | Kafka broker addresses | `localhost:9092` | `Program.cs`, `appsettings.json` |
| **BACKEND - Camunda BPM** |
| `CAMUNDA_BASE_URL` | ⚠️ Optional | api | Camunda REST API base URL | `http://localhost:8085/camunda` | `Program.cs`, `appsettings.json` |
| `CAMUNDA_USERNAME` | ⚠️ Optional | api | Camunda admin username | `admin` | `Program.cs`, `appsettings.json` |
| `CAMUNDA_PASSWORD` | ⚠️ Optional | api | Camunda admin password | `[SECRET]` | `Program.cs`, `appsettings.json` |
| `CAMUNDA_ENABLED` | ⚠️ Optional | api | Enable Camunda integration | `false` | `Program.cs`, `appsettings.json` |
| **BACKEND - Data Protection & Backups** |
| `DataProtection__KeysPath` | ⚠️ Optional | api | Path for data protection keys | `/app/keys` | `docker-compose.production.yml` |
| `Backup__Enabled` | ⚠️ Optional | api | Enable backup service | `true` | `docker-compose.production.yml` |
| `Backup__Directory` | ⚠️ Optional | api | Backup storage directory | `/app/backups` | `docker-compose.production.yml` |
| `Backup__RetentionDays` | ⚠️ Optional | api | Backup retention period | `30` | `docker-compose.production.yml` |
| **INFRA - Cloudflare Tunnel** |
| `CLOUDFLARE_TUNNEL_TOKEN` | ✅ Yes (if using Tunnel) | infra | Cloudflare Tunnel token | `[SECRET]` | `deploy-production.sh`, `docs/production/10-cloudflare-tunnel.md` |
| **FRONTEND - Next.js** |
| `NEXT_PUBLIC_API_URL` | ✅ Yes | web | Backend API base URL (public) | `https://app.shahin-ai.com` | `src/lib/api.ts`, `components/sections/OnboardingQuestionnaire.tsx` |
| `PORT` | ⚠️ Optional | web | Next.js server port | `3000` | `Dockerfile` (default: `3000`) |
| `NODE_ENV` | ✅ Yes | web | Node.js environment | `production` | `Dockerfile`, Next.js runtime |
| `HOSTNAME` | ⚠️ Optional | web | Next.js server hostname | `0.0.0.0` | `Dockerfile` |
| `NEXT_TELEMETRY_DISABLED` | ⚠️ Optional | web | Disable Next.js telemetry | `1` | `Dockerfile` |
| **DATABASE - PostgreSQL** |
| `POSTGRES_USER` | ✅ Yes | db | PostgreSQL superuser | `grc_admin` | `docker-compose.production.yml` |
| `POSTGRES_PASSWORD` | ✅ Yes | db | PostgreSQL superuser password | `[SECRET]` | `docker-compose.production.yml` |
| `POSTGRES_DB` | ✅ Yes | db | Initial database name | `GrcMvcDb` | `docker-compose.production.yml` |
| `POSTGRES_INITDB_ARGS` | ⚠️ Optional | db | PostgreSQL init arguments | `--encoding=UTF8` | `docker-compose.production.yml` |

---

## Environment Variable Priority

The application loads environment variables in the following order (highest priority first):

1. **System environment variables** (set in shell/systemd/Docker)
2. **`.env.production` file** (loaded by `Program.cs` for backend, Docker Compose for all services)
3. **`appsettings.Production.json`** (backend only, uses `${VAR_NAME}` substitution)

**Note:** `Program.cs` explicitly loads `.env.local` first (for local dev), then falls back to `.env` (for Docker/production). In production, only `.env.production` should exist.

---

## Connection String Formats

### PostgreSQL Connection Strings

**Main Database:**
```
Host={DB_HOST};Database={DB_NAME};Username={DB_USER};Password={DB_PASSWORD};Port={DB_PORT}
```

**Auth Database:**
```
Host={DB_HOST};Database={DB_NAME}_auth;Username={DB_USER};Password={DB_PASSWORD};Port={DB_PORT}
```

**Example (Docker Compose):**
```
Host=db-prod;Database=GrcMvcDb;Username=grc_admin;Password=***;Port=5432
```

### Redis Connection String

**Format:**
```
{HOST}:{PORT}
```

**Example (Docker Compose):**
```
redis-prod:6379
```

---

## Secrets Management

### Production Secrets (Never Commit)

The following variables **MUST** be kept secret and stored securely:

- `DB_PASSWORD` / `POSTGRES_PASSWORD`
- `JWT_SECRET` / `JwtSettings__Secret`
- `CLAUDE_API_KEY`
- `CLOUDFLARE_TUNNEL_TOKEN` (if using Cloudflare Tunnel)
- `SMTP_PASSWORD`
- `SMTP_CLIENT_SECRET`
- `AZURE_TENANT_ID` (if used)
- `CAMUNDA_PASSWORD` (if used)

### Recommended Storage Methods

1. **Docker Secrets** (Docker Swarm) or **Kubernetes Secrets**
2. **`.env.production` file** (on VPS, with `chmod 600` and excluded from git)
3. **HashiCorp Vault** or **Azure Key Vault** (for enterprise)
4. **Environment variables** set in systemd service files or Docker Compose (not in files)

---

## CORS & Allowed Origins

The backend expects the following origins (configured in `appsettings.json`):

- `https://portal.shahin-ai.com`
- `https://www.shahin-ai.com`
- `https://shahin-ai.com`
- `https://app.shahin-ai.com`
- `https://staging.shahin-ai.com`
- `https://dev.shahin-ai.com`
- `https://demo.shahin-ai.com`
- `https://test.shahin-ai.com`
- `http://localhost:3000` (development only)

**Production Note:** Update `AllowedOrigins` and `Cors:AllowedOrigins` in `appsettings.Production.json` to match your actual production domains.

---

## Missing Configuration Items

### ⚠️ MISSING: Application Insights Connection String

**Issue:** `appsettings.json` references `ApplicationInsights:ConnectionString` but no environment variable is defined.

**Proposed Fix:**
- Add `APPLICATIONINSIGHTS_CONNECTION_STRING` to `.env.production.example`
- Update `appsettings.Production.json` to use `${APPLICATIONINSIGHTS_CONNECTION_STRING}`

### ⚠️ MISSING: RabbitMQ Connection String (if MassTransit is used)

**Issue:** `Program.cs` references MassTransit/RabbitMQ but no connection string environment variable is found.

**Proposed Fix:**
- Add `RABBITMQ_CONNECTION_STRING` or `MassTransit__RabbitMQ__ConnectionString` to `.env.production.example`
- Document in this table if RabbitMQ is required for production

### ⚠️ MISSING: reCAPTCHA Keys (if enabled)

**Issue:** `appsettings.json` has `Security:Captcha` settings but no environment variables.

**Proposed Fix:**
- Add `RECAPTCHA_SITE_KEY` and `RECAPTCHA_SECRET_KEY` to `.env.production.example`
- Update `appsettings.Production.json` to use environment variables

---

## Next Steps

1. Create `.env.production.example` file (see below)
2. Update `appsettings.Production.json` to use environment variables for all secrets
3. Document RabbitMQ connection string if MassTransit is required
4. Add Application Insights connection string if monitoring is enabled

---

## Related Documents

- [Environment Specification](./02-environment-spec.md)
- [Deployment Guide](./05-deployment.md)
- [Security Baseline](./07-security-baseline.md)
